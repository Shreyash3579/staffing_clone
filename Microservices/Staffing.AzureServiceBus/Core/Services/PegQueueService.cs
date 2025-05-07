using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Staffing.AzureServiceBus.Contracts.Services;
using Staffing.AzureServiceBus.Core.Helpers;
using Staffing.AzureServiceBus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Staffing.AzureServiceBus.Core.Helpers.Constants;

namespace Staffing.AzureServiceBus.Core.Services
{
    public class PegQueueService : IPegQueueService
    {
        private static ServiceBusProcessor pegQueueReceiver = null;
        private static ServiceBusProcessor pegDLQReceiver = null;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly ISignalRHubClient _signalRHubClient;
        static string PegRecieverConnectionString = ConfigurationUtility.GetValue("PegServiceBus:PegRecieverConnectionString");
        static string PegQueueName = ConfigurationUtility.GetValue("PegServiceBus:PegQueueName");
        static string StaffingSenderConnectionString = ConfigurationUtility.GetValue("PegServiceBus:StaffingSenderConnectionString");
        static string StaffingQueueName = ConfigurationUtility.GetValue("PegServiceBus:StaffingQueueName");
        static int threads = Convert.ToInt32(ConfigurationUtility.GetValue("PegServiceBus:MaxConcurrentThreads"));


        public PegQueueService(IStaffingApiClient staffingApiClient, ISignalRHubClient signalRHubClient)
        {
            _staffingApiClient = staffingApiClient;
            _signalRHubClient = signalRHubClient;
        }

        public async Task SubscribeToPegQueue()
        {
            await RecieveMessageFromQueue(); //subscrie to normal Queue
            await RecieveMessageFromDeadLetterQueue(); //subscrie to DLQ

        }

        public async Task<bool> SendToPegQueue(IEnumerable<PegOpportunityMap> convertedPlanningCards)
        {
            var serviceBusClient = new ServiceBusClient(StaffingSenderConnectionString);
            var sender = serviceBusClient.CreateSender(StaffingQueueName);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            bool messageStatus;
            try
            {

                foreach (var convertedCard in convertedPlanningCards)
                {
                    var data = JsonConvert.SerializeObject(convertedCard);
                    var message = new ServiceBusMessage(data);
                    message.MessageId = Guid.NewGuid().ToString() + "-" + convertedCard.OpportunityId;

                    if (!messageBatch.TryAddMessage(message))
                    {
                        messageStatus = false;
                        return messageStatus;
                    }
                }
                await sender.SendMessagesAsync(messageBatch);
                messageStatus = true;
            }
            catch (Exception e)
            {
                messageStatus = false;
            }
            await sender.CloseAsync();

            return messageStatus;
        }

        public async Task StopPegQueue()
        {
            if (pegQueueReceiver != null)
            {
                await pegQueueReceiver.StopProcessingAsync();
                await pegQueueReceiver.CloseAsync();
            }

            if (pegDLQReceiver != null)
            {
                await pegDLQReceiver.StopProcessingAsync();
                await pegDLQReceiver.CloseAsync();
            }

        }

        private void InitializeServiceBusProcessor(string queueName, SubQueue queueType)
        {
            switch (queueType)
            {
                case SubQueue.None:
                    {
                        if (pegQueueReceiver == null)
                        {
                            //Create Receiever Client
                            var client = new ServiceBusClient(PegRecieverConnectionString);

                            //Create Receiever Processsor on  Queue
                            var options = new ServiceBusProcessorOptions()
                            {
                                AutoCompleteMessages = false,
                                MaxConcurrentCalls = threads,
                                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10),
                                SubQueue = SubQueue.None
                            };

                            pegQueueReceiver = client.CreateProcessor(queueName, options);
                        }
                        break;
                    }
                case SubQueue.DeadLetter:
                    {
                        if (pegDLQReceiver == null)
                        {
                            //Create Receiever Client
                            var client = new ServiceBusClient(PegRecieverConnectionString);

                            //Create Receiever Processsor on  Queue
                            var options = new ServiceBusProcessorOptions()
                            {
                                AutoCompleteMessages = false,
                                MaxConcurrentCalls = 5,
                                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10),
                                SubQueue = SubQueue.DeadLetter
                            };

                            pegDLQReceiver = client.CreateProcessor(queueName, options);
                        }
                        break;
                    }
            }
        }

        private async Task RecieveMessageFromQueue()
        {

            try
            {

                //Create Receiever Processsor on  Queue

                InitializeServiceBusProcessor(PegQueueName, SubQueue.None);

                //Assigns callbacks for Successs & error

                pegQueueReceiver.ProcessMessageAsync += ProcessMessageFromQueue;
                pegQueueReceiver.ProcessErrorAsync += ProcessErrorFromQueue;

                //Start Processing & receieing messages
                await pegQueueReceiver.StartProcessingAsync();

                //Stop Processing and Close Connection
                //await recieverQueue.StopProcessingAsync();
                //await recieverQueue.CloseAsync();
            }
            catch (Exception ex)
            {
                await pegQueueReceiver.StopProcessingAsync();
                await pegQueueReceiver.CloseAsync();
            }
        }
        private async Task RecieveMessageFromDeadLetterQueue()
        {

            try
            {

                //Create Receiever Processsor on  Queue

                InitializeServiceBusProcessor(PegQueueName, SubQueue.DeadLetter);


                //Assigns callbacks for Successs & error

                pegDLQReceiver.ProcessMessageAsync += ProcessMessageFromDeadLetterQueue;
                pegDLQReceiver.ProcessErrorAsync += ProcessErrorFromDeadLetterQueue;

                //Start Processing & receieing messages
                await pegDLQReceiver.StartProcessingAsync();

            }
            catch (Exception ex)
            {
                await pegDLQReceiver.StopProcessingAsync();
                await pegDLQReceiver.CloseAsync();
            }
        }

        private async Task ProcessMessageFromQueue(ProcessMessageEventArgs args)
        {
            await UpsertPlanningCardWithPEGOpportunity(args);
        }

        private async Task UpsertPlanningCardWithPEGOpportunity(ProcessMessageEventArgs args)
        {
            PegOpportunityViewModel pegOpportunityData = null;
            PegOpportunity pegLead = new PegOpportunity();
            try
            {
                pegOpportunityData = JsonConvert.DeserializeObject<PegOpportunityViewModel>(args.Message.Body.ToString());
                pegLead = pegOpportunityData.data;

                var oppsThatExistsInBOSS = await GetOppsThatExistsInBOSS(pegLead);
                var isOppExistsInBOSS = oppsThatExistsInBOSS != null && oppsThatExistsInBOSS.Any();

                var isProcessSucceeded = false;

                if (pegOpportunityData.Action == PegOpportunityDataActions.CREATE)
                {
                    isProcessSucceeded = await CreatePlanningCard(pegLead, isOppExistsInBOSS);
                }
                else if (pegOpportunityData.Action == PegOpportunityDataActions.UPDATE)
                {
                    var pegOpp = oppsThatExistsInBOSS.FirstOrDefault(x => x.PegOpportunityId == pegLead.OpportunityId);
                    if (pegOpp != null)
                    {
                        pegOpp.Name = pegLead.Name;
                        pegOpp.StartDate = pegLead.StartDate;
                        pegOpp.EndDate = pegLead.EndDate;
                        pegOpp.SharedOfficeCodes = pegLead.OfficeCodes;
                        pegOpp.ProbabilityPercent = pegLead.ProbabilityPercent;
                        pegOpp.Source = "Auto-PegToBOSSSync";
                        pegOpp.LastUpdatedBy = pegLead.LastUpdatedBy;
                        pegOpp.SourceLastUpdated = pegLead.LastUpdatedDate;
                        isProcessSucceeded = await UpdatePlanningCard(pegOpp);
                        //send the data to the signalR controller
                    }
                    else
                    {
                        await args.CompleteMessageAsync(args.Message);
                    }

                }
                else if (pegOpportunityData.Action == PegOpportunityDataActions.DELETE)
                {
                    isProcessSucceeded = await DeletePlanningCard(oppsThatExistsInBOSS.FirstOrDefault(), pegLead, isOppExistsInBOSS);
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

        private async Task<bool> UpdatePlanningCard(PlanningCard pegOpp)
        {
            var updatedPlanningCard = await _staffingApiClient.UpsertPlanningCard(pegOpp);

            var pegOppForSignalR = new PegOpportunity
            {
                PlanningCardId = pegOpp.Id,
                ProbabilityPercent = pegOpp.ProbabilityPercent,
                StartDate = pegOpp.StartDate,
                EndDate = pegOpp.EndDate,
                OfficeCodes = pegOpp.SharedOfficeCodes,
                LastUpdatedBy = pegOpp?.LastUpdatedBy,
                OpportunityId = pegOpp.PegOpportunityId,
                Name = pegOpp?.Name
            };

            var isNotificationSentToSignalR = await UpdateSignalRHub(pegOppForSignalR);

            var isPlanningCardSuccessfullyUpdated = (updatedPlanningCard.Id.HasValue && updatedPlanningCard.Id.Value != Guid.Empty);

            return isPlanningCardSuccessfullyUpdated;
        }

        private async Task<bool> UpdateSignalRHub(PegOpportunity pegOpp)
        {
            var updatedNotification = await _signalRHubClient.UpdateSignalRHub(pegOpp);

            // Convert the updatedNotification string to a boolean value
            bool isUpdatedBySignalR;
            if (!string.IsNullOrEmpty(updatedNotification) && bool.TryParse(updatedNotification, out isUpdatedBySignalR))
            {
                // Conversion successful, return the boolean value
                return isUpdatedBySignalR;
            }
            else
            {
                // Handle conversion failure (e.g., return default value or throw an exception)
                return false; 
            }
        }

        private async Task<IList<PlanningCard>> GetOppsThatExistsInBOSS(PegOpportunity pegLead)
        {
            return pegLead != null && !string.IsNullOrEmpty(pegLead.OpportunityId)
                    ? await _staffingApiClient.GetPlanningCardByPegOpportunityIds(pegLead.OpportunityId)
                    : null;
        }

        private async Task<bool> CreatePlanningCard(PegOpportunity pegLead, bool isOppExistsInBOSS)
        {
            var isOpportunityConverted = await ConvertPegOppToPlanningCard(pegLead, isOppExistsInBOSS);
            return isOpportunityConverted;
        }

        private async Task<bool> DeletePlanningCard(PlanningCard planningCard, PegOpportunity pegLead, bool isOppExistsInBOSS)
        {
            var deletedPlanningCard = false;

            if (isOppExistsInBOSS)
            {
                await _staffingApiClient.DeletePlanningCard(planningCard.Id, pegLead.LastUpdatedBy);
                deletedPlanningCard = true;
            }
            else
            {
                deletedPlanningCard = true;
            }
            return deletedPlanningCard;
        }

        private async Task ProcessMessageFromDeadLetterQueue(ProcessMessageEventArgs args)
        {
            await UpsertPlanningCardWithPEGOpportunity(args);
        }

        private async Task<bool> ConvertPegOppToPlanningCard(PegOpportunity pegOpportunity, bool isOppExistsInBOSS)
        {
            if (!isOppExistsInBOSS)
            {
                var convertedPlanningCard = ConvertToPlanningCardModel(pegOpportunity);
                var insertedPlanningCard = await _staffingApiClient.UpsertPlanningCard(convertedPlanningCard);

                var isOppSuccessfullyConverted = insertedPlanningCard.Id.HasValue && insertedPlanningCard.Id.Value != Guid.Empty;

                return isOppSuccessfullyConverted;
            }
            else
            {
                //mark duplicate message as read and processed
                return true;
            }
        }

        private async Task ProcessErrorFromQueue(ProcessErrorEventArgs args)
        {
            throw new ArgumentException("Message cannot be read from Queue: " + args.Exception.Message);
        }
        private async Task ProcessErrorFromDeadLetterQueue(ProcessErrorEventArgs args)
        {
            throw new ArgumentException("Message cannot be read from Dead Letter Queue: " + args.Exception.Message);
        }


        private PlanningCard ConvertToPlanningCardModel(PegOpportunity pegOpportunity)
        {
            var planningCard = new PlanningCard
            {
                PegOpportunityId = pegOpportunity.OpportunityId,
                Name = pegOpportunity.Name,
                StartDate = pegOpportunity.StartDate,
                EndDate = pegOpportunity.EndDate,
                IsShared = true,
                SharedOfficeCodes = pegOpportunity.OfficeCodes,
                ProbabilityPercent = pegOpportunity.ProbabilityPercent,
                IncludeInCapacityReporting = true, //default to true 
                SharedStaffingTags = "P", //shared with PEG by default
                CreatedBy = "7Auto",// "PegToBOSSSync",
                Source = "Auto-PegToBOSSSync",
                LastUpdatedBy = "7Auto", // We will update these values once we receive them from PEG
                SourceLastUpdated = (DateTime?)null  // We will update these values once we receive them from PEG
            };

            return planningCard;
        }
    }
}