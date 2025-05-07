using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Staffing.AzureServiceBus.Contracts.Services;
using Staffing.AzureServiceBus.Models;
using static Staffing.AzureServiceBus.Core.Helpers.Constants;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Staffing.AzureServiceBus.Core.Helpers;
using System.Threading;
using System.Linq;

namespace Staffing.AzureServiceBus.Core.Services
{
    public class PricingConfiguratorQueueService : IPricingConfiguratorQueueService
    {
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IPipelineApiClient _pipelineApiClient;
        private static ServiceBusProcessor pcQueueReceiver = null;
        private static ServiceBusProcessor pcDLQReceiver = null;
        static string PCQueueName = ConfigurationUtility.GetValue("PricingConfiguratorServiceBus:PCQueueName");
        static string PCRecieverConnectionString = ConfigurationUtility.GetValue("PricingConfiguratorServiceBus:PCRecieverConnectionString");
        static int threads = Convert.ToInt32(ConfigurationUtility.GetValue("PricingConfiguratorServiceBus:MaxConcurrentThreads"));

        public PricingConfiguratorQueueService(IStaffingApiClient staffingApiClient, IPipelineApiClient pipelineApiClient)
        {
            _staffingApiClient = staffingApiClient;
            _pipelineApiClient = pipelineApiClient;
        }

        public async Task SubscribeToPricingConfiguratorQueue()
        {
            await RecieveMessageFromQueue(); //subscrie to normal Queue
            await RecieveMessageFromDeadLetterQueue(); //subscrie to DLQ
        }

        private async Task RecieveMessageFromQueue()
        {
            try
            {
                //Create Receiever Processsor on  Queue
                InitializeServiceBusProcessor(PCQueueName, SubQueue.None);

                //Assigns callbacks for Successs & error
                pcQueueReceiver.ProcessMessageAsync += ProcessMessageFromQueue;
                pcQueueReceiver.ProcessErrorAsync += ProcessErrorFromQueue;

                //Start Processing & receieing messages
                await pcQueueReceiver.StartProcessingAsync();

                //Stop Processing and Close Connection
                //await recieverQueue.StopProcessingAsync();
                //await recieverQueue.CloseAsync();
            }
            catch (Exception ex)
            {
                await pcQueueReceiver.StopProcessingAsync();
                await pcQueueReceiver.CloseAsync();
            }
        }

        private async Task RecieveMessageFromDeadLetterQueue()
        {
            try
            {
                //Create Receiever Processsor on  Queue
                InitializeServiceBusProcessor(PCQueueName, SubQueue.DeadLetter);


                //Assigns callbacks for Successs & error
                pcDLQReceiver.ProcessMessageAsync += ProcessMessageFromDeadLetterQueue;
                pcDLQReceiver.ProcessErrorAsync += ProcessErrorFromDeadLetterQueue;

                //Start Processing & receieing messages
                await pcDLQReceiver.StartProcessingAsync();

            }
            catch (Exception ex)
            {
                await pcDLQReceiver.StopProcessingAsync();
                await pcDLQReceiver.CloseAsync();
            }
        }

        private async Task ProcessMessageFromQueue(ProcessMessageEventArgs args)
        {
            await UpsertPricingSku(args);
        }

        private async Task UpsertPricingSku(ProcessMessageEventArgs args)
        {
            // To-do - logic to save pc sku data in staffing database
            IEnumerable<PricingSkuViewModel> pricingSkuData = null;
            PricingSku pricingSKU = new PricingSku();
            try
            {
                pricingSkuData = JsonConvert.DeserializeObject<PricingSkuMap>(args.Message.Body.ToString()).scenario;
                pricingSKU = await ConvertToPricingSKUModel(pricingSkuData);

                var isProcessSucceeded = false;
                var isDataLogged = false;
                foreach (var pricingSkus in pricingSkuData)
                {
                    pricingSkus.lastUpdatedBy = "Auto-PricingToBOSSSync";
                }
                isDataLogged = await UpsertPricingSkuDataLog(pricingSkuData);

                if(pricingSKU.PipelineId.Value != Guid.Empty)
                {
                    isProcessSucceeded = await CreatePricingSKU(pricingSKU);
                }

                if (isProcessSucceeded)
                {
                    //complete messgae once successfully completed
                    await args.CompleteMessageAsync(args.Message);
                }
                else
                {
                    //abandon message so that it can be retried
                    await args.AbandonMessageAsync(args.Message);
                }
            }
            catch (Exception e)
            {
                //Possible poison message. Mark them as complete to remove from Queue
                await args.CompleteMessageAsync(args.Message);
            }
        }

        public Task<PricingSku> ConvertToPricingSKUModel(IEnumerable<PricingSkuViewModel> pricingSkuData)
        {
            var pcSKU = pricingSkuData.GroupBy(x => x.sf_opportunity_id).Select(async item => new PricingSku()
            {
                PipelineId = (await _pipelineApiClient.GetOpportunityByCortexId(item.Key))?.PipelineId,
                CortexOpportunityId = item.Key,
                PricingTeamSize = string.Join("+", item.Select(x => x.abbreviation)),
                LastUpdatedBy = "Auto-PricingToBOSSSync"
            }).FirstOrDefault();

            return pcSKU;
        }

        public async Task<bool> CreatePricingSKU(PricingSku pricingSKU)
        {
            var insertedPricingSKU = await _staffingApiClient.UpsertPricingSKU(pricingSKU);
            var isPricingSKUSuccessfullyCreated = insertedPricingSKU.Id.HasValue && insertedPricingSKU.Id.Value != Guid.Empty;

            return isPricingSKUSuccessfullyCreated;
        }

        public async Task<bool> UpsertPricingSkuDataLog(IEnumerable<PricingSkuViewModel> pricingSkuData)
        {
            var upsertedPricingSkuDataLogs = await _staffingApiClient.UpsertPricingSkuDataLog(pricingSkuData);
            var isDataSuccessfullyLogged = upsertedPricingSkuDataLogs.Id.HasValue && upsertedPricingSkuDataLogs.Id.Value != Guid.Empty;
            return isDataSuccessfullyLogged;
        }

        private async Task ProcessMessageFromDeadLetterQueue(ProcessMessageEventArgs args)
        {
            await UpsertPricingSku(args);
        }

        private Task ProcessErrorFromQueue(ProcessErrorEventArgs args)
        {
            throw new ArgumentException("Message cannot be read from Queue: " + args.Exception.Message);
        }

        private Task ProcessErrorFromDeadLetterQueue(ProcessErrorEventArgs args)
        {
            throw new ArgumentException("Message cannot be read from Dead Letter Queue: " + args.Exception.Message);
        }

        private void InitializeServiceBusProcessor(string queueName, SubQueue queueType)
        {
            switch (queueType)
            {
                case SubQueue.None:
                    {
                        if (pcQueueReceiver == null)
                        {
                            //Create Receiever Client
                            var client = new ServiceBusClient(PCRecieverConnectionString);

                            //Create Receiever Processsor on  Queue
                            var options = new ServiceBusProcessorOptions()
                            {
                                AutoCompleteMessages = false,
                                MaxConcurrentCalls = threads,
                                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10),
                                SubQueue = SubQueue.None
                            };

                            pcQueueReceiver = client.CreateProcessor(queueName, options);
                        }
                        break;
                    }
                case SubQueue.DeadLetter:
                    {
                        if (pcDLQReceiver == null)
                        {
                            //Create Receiever Client
                            var client = new ServiceBusClient(PCRecieverConnectionString);

                            //Create Receiever Processsor on  Queue
                            var options = new ServiceBusProcessorOptions()
                            {
                                AutoCompleteMessages = false,
                                MaxConcurrentCalls = 5,
                                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10),
                                SubQueue = SubQueue.DeadLetter
                            };

                            pcDLQReceiver = client.CreateProcessor(queueName, options);
                        }
                        break;
                    }
            }
        }

        public async Task StopPricingConfiguratorQueue()
        {
            if (pcQueueReceiver != null)
            {
                await pcQueueReceiver.StopProcessingAsync();
                await pcQueueReceiver.CloseAsync();
            }

            if (pcDLQReceiver != null)
            {
                await pcDLQReceiver.StopProcessingAsync();
                await pcDLQReceiver.CloseAsync();
            }

        }
    }
}
