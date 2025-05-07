using Microservices.Common.Core.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Net.Mail;
using System.Text;

namespace Staffing.HttpAggregator.API.Core.Helpers
{
    public static class EmailHelper
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        private static readonly GraphServiceClient _graphClient;
        private static readonly string servicePrincipalId;


        static EmailHelper()
        {
            var clientId = Configuration["O365AppCredentials:ClientId"];
            var tenantId = Configuration["O365AppCredentials:TenantId"];
            var secretValue = Configuration["O365AppCredentials:ClientSecret"];
            servicePrincipalId  = Configuration["O365AppCredentials:ServicePrincipalId"];
            _graphClient = new GraphClientHelper().GraphServiceClient(tenantId, clientId, secretValue);
        }

        private static StringBuilder GetEmailBody(string saluation, IList<string> bodyMessages,
            string signature)
        {
            var emailMessage = new StringBuilder();
            emailMessage.Clear();

            emailMessage.Append(
                "<div style='margin-bottom: 20px; padding-bottom: 4px;'>");
            emailMessage.Append($"<span>{saluation},<br/></span>");
            emailMessage.Append("<br/>");
            bodyMessages.ToList().ForEach(message => emailMessage.Append($"<div>{message}</div><br/>"));
            emailMessage.Append("Regards,<br/>");
            emailMessage.Append(signature);

            return emailMessage;
        }

        public static async void SendEmailWithO365(IDictionary<string, string> emailRecipients, string body,
            string subject)
        {
            Message message = new()
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = body
                },
                ToRecipients = new List<Recipient>(),
                CcRecipients = new List<Recipient>(),
                BccRecipients = new List<Recipient>(),
            };

            var toRecipients = emailRecipients["ToEmailAddress"].TrimEnd(';').Split(";");
            toRecipients.ToList().ForEach(to =>
            {
                var recipient = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = to
                    }
                };

                message.ToRecipients.Add(recipient);
            });

            if(!string.IsNullOrEmpty(emailRecipients["CcEmailAddress"]))
            {
                var ccRecipients = emailRecipients["CcEmailAddress"].TrimEnd(';').Split(";");
                ccRecipients.ToList().ForEach(cc =>
                {
                    var recipient = new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = cc
                        }
                    };

                    message.CcRecipients.Add(recipient);
                });
            }

            if (emailRecipients.Keys.Contains("BccEmailAddress") && !string.IsNullOrEmpty(emailRecipients["BccEmailAddress"]))
            {
                var bccRecipients = emailRecipients["BccEmailAddress"].TrimEnd(';').Split(";");
                bccRecipients.ToList().ForEach(bcc =>
                {
                    var recipient = new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = bcc
                        }
                    };

                    message.BccRecipients.Add(recipient);
                });
            }

            var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = false,
            };

            if (!string.IsNullOrEmpty(emailRecipients["FromEmailAddress"]))
            {
                var fromEmail = emailRecipients["FromEmailAddress"].ToString();
                await _graphClient.Users[fromEmail]
                  .SendMail
                  .PostAsync(requestBody);
            }
            else
            {
                await _graphClient.Users[servicePrincipalId]
                  .SendMail
                  .PostAsync(requestBody);
            }

        }

        private static async void SendEmailWithO365(IDictionary<string, string> emailRecipients, string body,
            string subject, List<Attachment> attachments)
        {
            Message message = new()
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = body
                },
                ToRecipients = new List<Recipient>(),
                Attachments = attachments

            };

            var toRecipients = emailRecipients["ToEmailAddress"].TrimEnd(';').Split(";");
            toRecipients.ToList().ForEach(to =>
            {
                var recipient = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = to
                    }
                };

                message.ToRecipients.Add(recipient);
            });

            var ccRecipients = emailRecipients["CcEmailAddress"].TrimEnd(';').Split(";");

            if (ccRecipients.Any())
            {
                ccRecipients.ToList().ForEach(cc =>
                {
                    var recipient = new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = cc
                        }
                    };

                    message.CcRecipients.Add(recipient);
                });
            }

            var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = false,
            };

            await _graphClient.Users[servicePrincipalId]
                  .SendMail
                  .PostAsync(requestBody);
        }
    }
}
