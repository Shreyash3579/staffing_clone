using BackgroundPolling.API.Models;
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

namespace BackgroundPolling.API.Core.Helpers
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

        public static async void TestEmailWithO365(IDictionary<string, string> emailRecipients)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var subjectLine = environment + ": Testing O365 Email";

            var messageBody = new List<string>
            {
                $"Testing O365 Email"
            };
            var body = GetEmailBody("Hello", messageBody, "Anshul Agarwal").ToString();

            SendEmailWithO365(emailRecipients, body, subjectLine);
            
        }

        public static void SendAnalyticsIssueMailToDeveloperHelpDesk(IDictionary<string, string> emailRecipients,
            AnalyticsIncorrectInfo incorrectRows, string analyticsTable)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var subjectLine = environment + ": Data Issue Summary";

            var attachments = new List<Attachment>();

            if (incorrectRows.StaffingTransactionMismatch.Count > 0)
            {
                var sheetName = "WorkdayStaffingMismatch";
                var fileName = $"{sheetName}_{analyticsTable}";
                attachments.Add(GetExcelAttachmentForO365(incorrectRows.StaffingTransactionMismatch, fileName, sheetName));
            }
            if (incorrectRows.LoATransactionMismatch.Count > 0)
            {
                var sheetName = "WorkdayLoATransactionMismatch";
                var fileName = $"{sheetName}_{analyticsTable}";
                attachments.Add(GetExcelAttachmentForO365(incorrectRows.StaffingTransactionMismatch, fileName, sheetName));
            }
            if (incorrectRows.IncorrectAnlayticsData.Count > 0)
            {
                var sheetName = "IncorrectAnalyticsData";
                var fileName = $"{sheetName}_{analyticsTable}";
                attachments.Add(GetExcelAttachmentForO365(incorrectRows.StaffingTransactionMismatch, fileName, sheetName));
            }
            if (incorrectRows.BillRateMismatch.Count > 0)
            {
                var sheetName = "CcmBillRateMismatch";
                var fileName = $"{sheetName}_{analyticsTable}";
                attachments.Add(GetExcelAttachmentForO365(incorrectRows.StaffingTransactionMismatch, fileName, sheetName));
            }

            var messageBody = new List<string>
            {
                "This is an automatically generated email created by the BOSS application on finding data issues. See attached file for details.",
                "Please reach out to staffing development team for any questions/concerns."
            };
            var body = GetEmailBody("Hey", messageBody, "Staffing Team").ToString();
            SendEmailWithO365(emailRecipients, body, subjectLine, attachments);
        }

        public static void SendEmailToEmeaPegStaffingOfficer<T>(IDictionary<string, string> emailRecipients,
            IList<T> objectList) where T : class
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var subjectLine = environment + ": EMEA PEG Report by case";

            var today = DateTime.Now.Date.ToString("dd-MM-yyyy");
            var fileName = $"PEG RF Cases ({today})";

            var attachments = new List<Attachment>
            {
                GetExcelAttachmentForO365(objectList, fileName, "CasesServedByPegRF")
            };

            var messageBody = new List<string>
            {
                "Please find the attached sheet for the active cases served by ringfence.",
                "Let us know if you have any questions/concerns."
            };
            var body = GetEmailBody("Hello", messageBody, "Global Staffing Team").ToString();
            SendEmailWithO365(emailRecipients, body, subjectLine, attachments);
        }

        public static void SendEmailForAuditsOfAllocationByStaffingUser<T>(IDictionary<string, string> emailRecipients,
            IList<T> objectList, DateTime auditLogsEffectiveFromDate) where T : class
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var subjectLine = environment + ": Audit logs for BCN";

            var fileName = $"AuditLogs_BCN_{auditLogsEffectiveFromDate.ToString("dd-MM-yyyy")}";
            var attachment = GetExcelAttachmentForO365(objectList, fileName, "auditLogs");

            var attachments = new List<Attachment>
            {
                attachment
            };

            var messageBody = new List<string>
            {
                $"Please find the attached audit logs of the allocations effective from {auditLogsEffectiveFromDate.ToString("dd-MM-yyyy")}.",
                "Let us know if you have any questions/concerns."
            };
            var body = GetEmailBody("Hello", messageBody, "Nitin Jain").ToString();
            SendEmailWithO365(emailRecipients, body, subjectLine, attachments);

        }

        public static void SendEmailForCADMismatchAudit(IDictionary<string, string> emailRecipients,
            IList<CADMismatchLog> objectList, IList<CADMismatchLog> filterStaffingAndStaffingAnalyticsSyncData)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var todayDate = DateTime.Now.ToString("yyyy-MM-dd");

            var subjectLine = environment + ": Logs for Analytics Data Mismatch as of "+ todayDate;

            var smdRows = DataConversion.GetDataTableFromObjects(objectList.Where( x => x.SourceTable == "SMD").ToList());
            var raRows = DataConversion.GetDataTableFromObjects(objectList.Where(x => x.SourceTable == "RA").ToList());
            var filterStaffingAndStaffingAnalyticsSyncDataRows = DataConversion.GetDataTableFromObjects(filterStaffingAndStaffingAnalyticsSyncData);


            var smdBody = DataConversion.GetMailBodyFromDataTable(smdRows, "SMD Mismatch");
            var raBody = DataConversion.GetMailBodyFromDataTable(raRows, "RA Mismatch");
            var staffingBody = DataConversion.GetMailBodyFromDataTable(raRows, "CAD mismatch");
            var syncMismatchBody = DataConversion.GetMailBodyFromDataTable(filterStaffingAndStaffingAnalyticsSyncDataRows, "Staffing and Staffing Analytics Sync Mismatch");

            var mailBody = smdBody.AppendLine().Append(raBody).Append(syncMismatchBody);

            SendEmailWithO365(emailRecipients, mailBody.ToString(), subjectLine);

        }


        //private static System.Net.Mail.Attachment GetExcelAttachment<T>(IList<T> objectList,
        //    string fileName, string sheetName) where T : class
        //{
        //    var workBook = new ClosedXML.Excel.XLWorkbook();

        //    var dataTable = DataConversion.GetDataTableFromObjects(objectList);
        //    var filePath = DataConversion.ExportDataTableToExcel(dataTable, fileName, sheetName, workBook);

        //    return new System.Net.Mail.Attachment(filePath);
        //}

        private static FileAttachment GetExcelAttachmentForO365<T>(IList<T> objectList,
           string fileName, string sheetName) where T : class
        {
            var workBook = new ClosedXML.Excel.XLWorkbook();

            var dataTable = DataConversion.GetDataTableFromObjects(objectList);
            var filePath = DataConversion.ExportDataTableToExcel(dataTable, fileName, sheetName, workBook);

            var attachmentContentBytes = File.ReadAllBytes(filePath);

            var attachment = new FileAttachment
            {
                Name = fileName + ".xlsx",
                ContentBytes = attachmentContentBytes
            };

            return attachment;
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

        //private static void SendEmail(IDictionary<string, string> emailRecipients, string body,
        //    string subject, IList<System.Net.Mail.Attachment> attachemnts)
        //{
        //    var mailMessage = new MailMessage()
        //    {
        //        From = new MailAddress(emailRecipients["FromEmailAddress"],
        //        emailRecipients["FromEmailAddressDisplayName"]),
        //        Body = body,
        //        Subject = subject,
        //        IsBodyHtml = true,
        //    };

        //    var toRecipients = emailRecipients["ToEmailAddress"].TrimEnd(';').Split(";");
        //    var ccRecipients = emailRecipients.ContainsKey("CCEmailAddress")
        //         ? emailRecipients["CCEmailAddress"].TrimEnd(';').Split(";")
        //         : null;

        //    toRecipients.ToList().ForEach(x => mailMessage.To.Add(x));
        //    ccRecipients?.ToList()?.ForEach(x => mailMessage.To.Add(x));

        //    if (attachemnts.Count > 0)
        //    {
        //        attachemnts.ToList().ForEach(attachment => mailMessage.Attachments.Add(attachment));
        //    }

        //    var smtpClient = new SmtpClient
        //    {
        //        UseDefaultCredentials = true,
        //        Host = Configuration["Email:Host"],
        //        DeliveryMethod = SmtpDeliveryMethod.Network
        //    };
        //    smtpClient.Send(mailMessage);
        //}

        //private static void SendEmail(IDictionary<string, string> emailRecipients, string body,
        //    string subject)
        //{
        //    var mailMessage = new MailMessage()
        //    {
        //        From = new MailAddress(emailRecipients["FromEmailAddress"],
        //        emailRecipients["FromEmailAddressDisplayName"]),
        //        Body = body,
        //        Subject = subject,
        //        IsBodyHtml = true,
        //    };

        //    var toRecipients = emailRecipients["ToEmailAddress"].TrimEnd(';').Split(";");
        //    var ccRecipients = emailRecipients.ContainsKey("CCEmailAddress")
        //         ? emailRecipients["CCEmailAddress"].TrimEnd(';').Split(";")
        //         : null;

        //    toRecipients.ToList().ForEach(x => mailMessage.To.Add(x));
        //    ccRecipients?.ToList()?.ForEach(x => mailMessage.To.Add(x));

        //    var smtpClient = new SmtpClient
        //    {
        //        UseDefaultCredentials = true,
        //        Host = Configuration["Email:Host"],
        //        DeliveryMethod = SmtpDeliveryMethod.Network
        //    };
        //    smtpClient.Send(mailMessage);
        //}

        private static async void SendEmailWithO365(IDictionary<string, string> emailRecipients, string body,
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

            if (!string.IsNullOrEmpty(emailRecipients["CcEmailAddress"]))
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
                CcRecipients = new List<Recipient>(),
                BccRecipients = new List<Recipient>(),
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

            if (!string.IsNullOrEmpty(emailRecipients["CcEmailAddress"]))
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

            await _graphClient.Users[servicePrincipalId]
                  .SendMail
                  .PostAsync(requestBody);
        }
    }
}
