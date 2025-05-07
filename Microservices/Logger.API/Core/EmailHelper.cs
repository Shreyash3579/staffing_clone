using Microservices.Common.Core.Helpers;
using Microservices.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace Logger.API.Core
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
            servicePrincipalId = Configuration["O365AppCredentials:ServicePrincipalId"];
            _graphClient = new GraphClientHelper().GraphServiceClient(tenantId, clientId, secretValue);
        }

        public static void SendMailToDeveloperHelpDesk(ErrorLogs errorLogs)
        {
            var exception = errorLogs.Error;
            var employeeCode = errorLogs.EmployeeCode;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var subjectLine = "Error In " + errorLogs.ApplicationName + " - " + environment;
            var emailRecipients = new Dictionary<string, string>();
            emailRecipients.Add("ToEmailAddress", Configuration["Email:ToEmailAddress"]);
            var body = GetMailBody(exception, employeeCode).ToString();
            SendEmailWithO365(emailRecipients, body, subjectLine);
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

            //var ccRecipients = emailRecipients["CcEmailAddress"].TrimEnd(';').Split(";");

            //if (ccRecipients.Any())
            //{
            //    ccRecipients.ToList().ForEach(cc =>
            //    {
            //        var recipient = new Recipient
            //        {
            //            EmailAddress = new EmailAddress
            //            {
            //                Address = cc
            //            }
            //        };

            //        message.CcRecipients.Add(recipient);
            //    });
            //}

            var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = false,
            };

            await _graphClient.Users[servicePrincipalId]
                  .SendMail
                  .PostAsync(requestBody);
        }

        private static StringBuilder GetMailBody(Exception ex, string employeeCode)
        {
            var errorMessage = new StringBuilder();
            errorMessage.Clear();
            errorMessage.Append(
                "<div style='font: bold 10pt Arial; margin-bottom: 20px; padding-bottom: 4px; border-bottom: 2px solid #FF6600'>");
            errorMessage.Append("<span style='color: #CC0000;'>&gt;&gt;&nbsp;</span>");
            errorMessage.Append("BOSS (Staffing System)");
            errorMessage.Append(
                " Error <div style='font: normal 10pt Arial; color: #505050; padding-top: 7px; padding-bottom: 1px'>");
            errorMessage.Append(
                "This is an automatically generated email created by the BOSS application upon experiencing an error. See below for details.");
            errorMessage.Append("</div></div>");

            errorMessage.Append("<table cellpadding='4' cellspacing='2' style='font: 10pt verdana'>");
            errorMessage.Append(GetErrorMessageTableRow("Exception",
                ex.ToString().Substring(0, ex.ToString().IndexOf(":", StringComparison.CurrentCultureIgnoreCase))));
            errorMessage.Append(GetErrorMessageTableRow("Error Message", ex.Message));
            errorMessage.Append(GetErrorMessageTableRow("Inner Exception",
                ex.InnerException?.Message ?? "<font color='gray'>[N/A]</font>"));
            errorMessage.Append(GetErrorMessageTableRow("Function",
                ex.TargetSite?.ToString() ?? "<font color='gray'>[N/A]</font>"));
            errorMessage.Append(GetErrorMessageTableRow("Stack Trace",
                ex.StackTrace?.Substring(3).Replace(" in ", "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;in ")
                    .Replace(" at ", "<br />at ").Trim() ?? "<font color='gray'>[N/A]</font>"));
            errorMessage.Append(GetErrorMessageTableRow("User ID", Environment.UserName));
            errorMessage.Append(GetErrorMessageTableRow("Employee Code", employeeCode));
            errorMessage.Append(GetErrorMessageTableRow("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")));
            errorMessage.Append(GetErrorMessageTableRow("Application Version",
                Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            errorMessage.Append(GetErrorMessageTableRow("Timestamp",
                DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.CurrentCulture)));
            errorMessage.Append(GetErrorMessageTableRow("Culture", CultureInfo.CurrentCulture.EnglishName));
            errorMessage.Append("</table>");

            return errorMessage;
        }

        private static string GetErrorMessageTableRow(string field, string data)
        {
            var sb = new StringBuilder();
            sb.Append(
                "<tr><td align='right' valign='top' style='background-color: #9DB3BD; color: #1A1D22; white-space: nowrap;'>");
            sb.Append("<strong>" + field + "</strong>:</td>");
            sb.Append("<td style='background-color: #DBE3E8; color: #272B33;'>");
            sb.Append(data);
            sb.Append("</td></tr>");
            return sb.ToString();
        }       
    }
}
