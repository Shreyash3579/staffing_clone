using Staffing.HttpAggregator.Contracts.Services;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Text;
using Staffing.HttpAggregator.Core.Helpers;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using Staffing.HttpAggregator.Models;
using System.Collections.Generic;
using Staffing.HttpAggregator.ViewModels;
using Staffing.HttpAggregator.API.Core.Helpers;
using System.Text.RegularExpressions;
using static Staffing.HttpAggregator.Core.Helpers.Constants;

namespace Staffing.HttpAggregator.Core.Services
{
    public class ExpertEmailUtilityService : IExpertEmailUtilityService
    {
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IResourceAllocationService _resourceAllocationService;
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private static short dayOfExpertEmailDelivery = Convert.ToInt16(ConfigurationUtility.GetValue("ExpertEmailUtility:DayOfExpertEmailDelivery"));
        private static short dayOfApacInnovationAndDesignEmailDelivery = Convert.ToInt16(ConfigurationUtility.GetValue("InnovationAndDesignEmailUtility:DayOfEmailDelivery"));
        private static DateTime today = DateTime.Today;
        private DateTime dateOfExpertEmailDelivery = (new DateTime(today.Year, today.Month, dayOfExpertEmailDelivery)).Date;
        private DateTime dateOfInnovationAndDesignEmailDelivery = (new DateTime(today.Year, today.Month, dayOfApacInnovationAndDesignEmailDelivery)).Date;

        public ExpertEmailUtilityService(IResourceApiClient resourceApiClient, IResourceAllocationService resourceAllocationService, ICCMApiClient ccmApiClient,
            IStaffingApiClient staffingApiClient)
        {
            _resourceApiClient = resourceApiClient;
            _resourceAllocationService = resourceAllocationService;
            _staffingApiClient = staffingApiClient;
            _ccmApiClient = ccmApiClient;
        }

        public async Task<string> SendMonthlyStaffingAllocationsEmailToExperts(string employeeCodes)
        {
            IEnumerable<Resource> expertEmployeesForEmails;

            var activeEmployees = await _resourceApiClient.GetEmployees();
            var today = DateTime.Today;
            activeEmployees = activeEmployees.Where(x => x.StartDate.Value.Date < today).ToList(); //remove Not Yet Started Employees

            //if Employee codes, then send Emails to those employees only.. else send to all
            //else send to all experts
            if (!string.IsNullOrEmpty(employeeCodes))
            {
                activeEmployees = activeEmployees.Where(x => employeeCodes.Contains(x.EmployeeCode)).ToList();
                expertEmployeesForEmails = FilterExpertEmployeesForEmails(activeEmployees);
            }
            else
            {
                var dataLogs = await _staffingApiClient.GetEmailUtilityDataLogsByDateAndEmailType(dateOfExpertEmailDelivery, "1");

                expertEmployeesForEmails = FilterExpertEmployeesForEmails(activeEmployees);

                var listRetryEmployees = dataLogs.Where(x => x.status != EmailStatus.SUCCESS);

                //If no data logs, that means job is running for 1st time.
                //Else If data logs are there and there are failures, that means job failed in between and retry for those failed employees
                //Else Process completed successfully and we need not do anything else and return.
                if (!dataLogs.Any())
                {
                    List<EmailUtilityData> emailUtilityDataLogs = new List<EmailUtilityData>();

                    foreach (var employee in expertEmployeesForEmails)
                    {
                        emailUtilityDataLogs.Add(new EmailUtilityData
                        {
                            employeeCode = employee.EmployeeCode,
                            employeeName = employee.FullName,
                            currentLevelGrade = employee.LevelGrade,
                            positionGroupName = employee.Position.PositionGroupName,
                            serviceLineName = employee.ServiceLine.ServiceLineName,
                            officeName = employee.Office.OfficeName,
                            status = EmailStatus.INPROGRESS,
                            date = dateOfExpertEmailDelivery,
                            EmailType = EmailType.EXPERTS,
                            lastUpdatedBy = "Auto-Email Utility"
                        });
                        await Task.Delay(10);
                    }

                    await _staffingApiClient.UpsertEmailUtilityDataLog(emailUtilityDataLogs);
                }
                else if(dataLogs.Any() && listRetryEmployees.Where(x => x.status == EmailStatus.FAILED || x.status == EmailStatus.INPROGRESS).Any())
                {
                    var listRetryEmployeeCodes = string.Join(",", listRetryEmployees.Select(x => x.employeeCode));
                    expertEmployeesForEmails = expertEmployeesForEmails.Where(x => listRetryEmployeeCodes.Contains(x.EmployeeCode)).ToList();
                    
                }
                else
                {
                    return "No Employees found for email retry";
                }
            }

            var processStatus = await SendStaffingAllocationsMailToExperts(expertEmployeesForEmails.OrderBy(x => x.FullName));

            return processStatus;
        }

        public async Task<string> SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(string employeeCodes)
        {
            IEnumerable<Resource> employeesForEmails;

            var activeEmployees = await _resourceApiClient.GetEmployees();
            var today = DateTime.Today;
            activeEmployees = activeEmployees.Where(x => x.StartDate.Value.Date < today).ToList(); //remove Not Yet Started Employees

            //if Employee codes, then send Emails to those employees only.. else send to all
            //else send to all
            if (!string.IsNullOrEmpty(employeeCodes))
            {
                activeEmployees = activeEmployees.Where(x => employeeCodes.Contains(x.EmployeeCode)).ToList();
                employeesForEmails = await FilterApacInnovationAndDesignEmployeesForEmails(activeEmployees);
            }
            else
            {
                var employeesForEmailsTask = FilterApacInnovationAndDesignEmployeesForEmails(activeEmployees);
                var dataLogTask = _staffingApiClient.GetEmailUtilityDataLogsByDateAndEmailType(dateOfInnovationAndDesignEmailDelivery, "2");

                await Task.WhenAll(employeesForEmailsTask, dataLogTask);

                employeesForEmails = employeesForEmailsTask.Result;
                var dataLogs = dataLogTask.Result;

                var listRetryEmployees = dataLogs.Where(x => x.status != EmailStatus.SUCCESS);

                //If no data logs, that means job is running for 1st time.
                //Else If data logs are there and there are failures, that means job failed in between and retry for those failed employees
                //Else Process completed successfully and we need not do anything else and return.
                if (!dataLogs.Any())
                {
                    List<EmailUtilityData> emailUtilityDataLogs = new List<EmailUtilityData>();

                    foreach (var employee in employeesForEmails)
                    {
                        emailUtilityDataLogs.Add(new EmailUtilityData
                        {
                            employeeCode = employee.EmployeeCode,
                            employeeName = employee.FullName,
                            currentLevelGrade = employee.LevelGrade,
                            positionGroupName = employee.Position.PositionGroupName,
                            serviceLineName = employee.ServiceLine.ServiceLineName,
                            officeName = employee.Office.OfficeName,
                            status = EmailStatus.INPROGRESS,
                            date = dateOfInnovationAndDesignEmailDelivery,
                            EmailType = EmailType.INNOVATIONANDDESIGN,
                            lastUpdatedBy = "Auto-Email Utility"
                        });
                    }

                    await _staffingApiClient.UpsertEmailUtilityDataLog(emailUtilityDataLogs);
                }
                else if (dataLogs.Any() && listRetryEmployees.Where(x => x.status == EmailStatus.FAILED || x.status == EmailStatus.INPROGRESS).Any())
                {
                    var listRetryEmployeeCodes = string.Join(",", listRetryEmployees.Select(x => x.employeeCode));
                    employeesForEmails = employeesForEmails.Where(x => listRetryEmployeeCodes.Contains(x.EmployeeCode)).ToList();

                }
                else
                {
                    return "No Employees found for email retry";
                }
            }

            var processStatus = await SendStaffingAllocationsMailToApacInnovationAndDesign(employeesForEmails.OrderBy(x => x.FullName));

            return processStatus;
        }

        private IEnumerable<Resource> FilterExpertEmployeesForEmails(IEnumerable<Resource> employeesData)
        {
            var expertsServiceLineCode = ServiceLineCodes.EXPERTS;
            var expertPositionGroups = ConfigurationUtility.GetValue("ExpertEmailUtility:PositionGroupNames"); //position gorup = Expert Senior Manager, Expert Associate Partner, Expert Partner, Expert Senior Associate Consultant, Expert Consultant, Expert Manager 

            var expertEmployeesForEmails = employeesData.Where(x => x.ServiceLine.ServiceLineCode == expertsServiceLineCode
                && expertPositionGroups.Split(",").Contains(x.Position.PositionGroupName));

            return expertEmployeesForEmails;
        }

        private async Task<IEnumerable<Resource>> FilterApacInnovationAndDesignEmployeesForEmails(IEnumerable<Resource> employeesData)
        {
            var innovationServiceLineCode = ServiceLineCodes.InnovationAndDesign;
            var apacRegionCode = 15;

            var officesForApacRegion = await _ccmApiClient.GetOfficesFlatListByRegionOrCluster(apacRegionCode);
            var officeCodesForApacRegion = string.Join(",", officesForApacRegion.Select(x => x.OfficeCode).Distinct());

            string pattern = @"^TT(1[0-9])";
            Regex regex = new Regex(pattern);

            var expertEmployeesForEmails = employeesData.Where(x => x.ServiceLine.ServiceLineCode == innovationServiceLineCode
                && regex.IsMatch(x.LevelGrade)
                && officeCodesForApacRegion.Contains(x.Office.OfficeCode.ToString()));

            return expertEmployeesForEmails;
        }


        private async Task<string> SendStaffingAllocationsMailToExperts(IEnumerable<Resource> expertEmployeesForEmails)
        {
            List<EmailUtilityData> emailUtilityDataLogs = new List<EmailUtilityData>();

            var startDate = (new DateTime(dateOfExpertEmailDelivery.Year, dateOfExpertEmailDelivery.Month, 1)).AddMonths(-1);
            var listEmployeeCodesForEmail = string.Join(",", expertEmployeesForEmails.Select(x => x.EmployeeCode));

            var resourceAllocations = await _resourceAllocationService.GetResourceAllocationsByEmployeeCodes(listEmployeeCodesForEmail, startDate, null);

            resourceAllocations = resourceAllocations.Where(x => !string.IsNullOrEmpty(x.OldCaseCode));

            var maxParallelThreads = new ParallelOptions { MaxDegreeOfParallelism = 5 };

            foreach (var employee in expertEmployeesForEmails)
            {
                var dataLog = new EmailUtilityData
                {
                    employeeCode = employee.EmployeeCode,
                    employeeName = employee.FullName,
                    currentLevelGrade = employee.LevelGrade,
                    positionGroupName = employee.Position.PositionGroupName,
                    serviceLineName = employee.ServiceLine.ServiceLineName,
                    officeName = employee.Office.OfficeName,
                    date = dateOfExpertEmailDelivery,
                    EmailType = EmailType.EXPERTS,
                    lastUpdatedBy = "Auto-Email Utility"
                };

                try
                {
                    var resourceAllocationsOfEmployee = resourceAllocations.Where(x => x.EmployeeCode == employee.EmployeeCode)
                        .OrderBy(x => x.StartDate).ThenBy(y => y.EndDate);

                    SendEmailToExperts(resourceAllocationsOfEmployee, employee);

                    dataLog.status = EmailStatus.SUCCESS;
                }
                catch (Exception ex)
                {
                    dataLog.status = EmailStatus.FAILED;
                    dataLog.exception = ex.InnerException?.Message ?? ex.Message;
                }
                finally{
                    lock (emailUtilityDataLogs)
                    {
                        emailUtilityDataLogs.Add(dataLog);
                    }
                }
                await Task.Delay(1000);
            };

            await _staffingApiClient.UpsertEmailUtilityDataLog(emailUtilityDataLogs);

            SendEmailSummaryToSupportMailbox(emailUtilityDataLogs, EmailType.EXPERTS);
            return "Process Completed Successfully";
        }

        private async Task<string> SendStaffingAllocationsMailToApacInnovationAndDesign(IEnumerable<Resource> employeesForEmails)
        {
            List<EmailUtilityData> emailUtilityDataLogs = new List<EmailUtilityData>();

            var startDate = (new DateTime(dateOfInnovationAndDesignEmailDelivery.Year, dateOfInnovationAndDesignEmailDelivery.Month, 1)).AddMonths(-1);
            var listEmployeeCodesForEmail = string.Join(",", employeesForEmails.Select(x => x.EmployeeCode));

            var resourceAllocations = await _resourceAllocationService.GetResourceAllocationsByEmployeeCodes(listEmployeeCodesForEmail, startDate, null);

            resourceAllocations = resourceAllocations.Where(x => !string.IsNullOrEmpty(x.OldCaseCode));

            var maxParallelThreads = new ParallelOptions { MaxDegreeOfParallelism = 8 };

            Parallel.ForEach(employeesForEmails, maxParallelThreads, employee =>
            {
                var dataLog = new EmailUtilityData
                {
                    employeeCode = employee.EmployeeCode,
                    employeeName = employee.FullName,
                    currentLevelGrade = employee.LevelGrade,
                    positionGroupName = employee.Position.PositionGroupName,
                    serviceLineName = employee.ServiceLine.ServiceLineName,
                    officeName = employee.Office.OfficeName,
                    date = dateOfExpertEmailDelivery,
                    EmailType = EmailType.INNOVATIONANDDESIGN,
                    lastUpdatedBy = "Auto-Email Utility"
                };

                try
                {
                    var resourceAllocationsOfEmployee = resourceAllocations.Where(x => x.EmployeeCode == employee.EmployeeCode)
                        .OrderBy(x => x.StartDate).ThenBy(y => y.EndDate);

                    SendEmailToApacInnovationAndDesign(resourceAllocationsOfEmployee, employee);

                    dataLog.status = EmailStatus.SUCCESS;
                }
                catch (Exception ex)
                {
                    dataLog.status = EmailStatus.FAILED;
                    dataLog.exception = ex.InnerException?.Message ?? ex.Message;
                }
                finally
                {
                    lock (emailUtilityDataLogs)
                    {
                        emailUtilityDataLogs.Add(dataLog);
                    }
                }
            });

            await _staffingApiClient.UpsertEmailUtilityDataLog(emailUtilityDataLogs);

            SendEmailSummaryToSupportMailbox(emailUtilityDataLogs, EmailType.INNOVATIONANDDESIGN);
            return "Process Completed Successfully";
        }
        //private void SendEmail(IEnumerable<ResourceAssignmentViewModel> resourceAllocationsOfEmployee, Resource employee)
        //{
        //    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        //    string subjectLine;
        //    string body;

        //    var ccReceipients = ConfigurationUtility.GetValue("ExpertEmailUtility:CCEmailAddress").TrimEnd(';').Split(";");

        //    using (var smtpClient = new SmtpClient(ConfigurationUtility.GetValue("Email:Host")))
        //    {
        //        smtpClient.UseDefaultCredentials = true;
        //        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

        //        MailMessage mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(ConfigurationUtility.GetValue("ExpertEmailUtility:FromEmailAddress"), ConfigurationUtility.GetValue("ExpertEmailUtility:FromEmailAddressDisplayName")),
        //            DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
        //        };

        //        if (environment != Environments.Production)
        //        {
        //            subjectLine = "Action required: Please Review Staffing Allocations in BOSS - " + environment;
        //            var toReceipents = ConfigurationUtility.GetValue("ExpertEmailUtility:DevTeamEmailAddress").TrimEnd(';').Split(";");

        //            foreach (string receipent in toReceipents)
        //            {
        //                mailMessage.To.Add(receipent);
        //            }
        //        }
        //        else
        //        {
        //            //If production send to actual users and from Expert mailbox
        //            subjectLine = "Action required: Please Review Staffing Allocations in BOSS";
        //            mailMessage.To.Add(employee.InternetAddress);

        //        }

        //        body = GetMailBodyTemplate().ToString();
        //        var previousMonthName = DateTime.Now.AddMonths(-1).ToString("MMMM", CultureInfo.InvariantCulture);
        //        body = body.Replace("EMPLOYEE_NAME", employee.FirstName)
        //            .Replace("ALLOC_START_DATE", previousMonthName)
        //            .Replace("ALLOCATIONS_TABLE", GetAllocationsTable(resourceAllocationsOfEmployee, employee.FullName));

        //        foreach (string receipent in ccReceipients)
        //        {
        //            if (!string.IsNullOrEmpty(receipent))
        //                mailMessage.CC.Add(receipent);
        //        }

        //        mailMessage.IsBodyHtml = true;
        //        mailMessage.Body = body;
        //        mailMessage.Subject = subjectLine;
        //        smtpClient.Send(mailMessage);
        //    }
        //}

        private void SendEmailToExperts(IEnumerable<ResourceAssignmentViewModel> resourceAllocationsOfEmployee, Resource employee)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string subjectLine, body;
            bool isSendToSupportMailbox = false;

            body = GetMailBodyTemplate().ToString();
            var previousMonthName = DateTime.Now.AddMonths(-1).ToString("MMMM", CultureInfo.InvariantCulture);
            body = body.Replace("EMPLOYEE_NAME", employee.FirstName)
                .Replace("ALLOC_START_DATE", previousMonthName)
                .Replace("ALLOCATIONS_TABLE", GetAllocationsTable(resourceAllocationsOfEmployee, employee.FullName))
                .Replace("SIGNATURE", "The Expert Program team");

            var emailRecipients = GetEmailRecipientsByEmailType(EmailType.EXPERTS, isSendToSupportMailbox);

            if (environment == Environments.Production)
            {
                if (emailRecipients.ContainsKey("ToEmailAddress"))
                {
                    emailRecipients["ToEmailAddress"] = employee.InternetAddress;
                }
                else
                {
                    emailRecipients.Add("ToEmailAddress", employee.InternetAddress);
                }

            }

            subjectLine = "Action required: Please Review Staffing Allocations in BOSS" + (environment != Environments.Production ? " - " + environment : "");

            EmailHelper.SendEmailWithO365(emailRecipients, body, subjectLine);

            //print here
            //using (StreamWriter writer = new StreamWriter(filePath, true))
            //{
            //    writer.WriteLine("Email received : " + employee.FirstName);
            //}
        }

        private void SendEmailToApacInnovationAndDesign(IEnumerable<ResourceAssignmentViewModel> resourceAllocationsOfEmployee, Resource employee)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string subjectLine, body;
            bool isSendToSupportMailbox = false;
            
            body = GetMailBodyTemplate().ToString();
           
            var previousMonthName = DateTime.Now.AddMonths(-1).ToString("MMMM", CultureInfo.InvariantCulture);
            body = body.Replace("EMPLOYEE_NAME", employee.FirstName)
                .Replace("ALLOC_START_DATE", previousMonthName)
                .Replace("ALLOCATIONS_TABLE", GetAllocationsTable(resourceAllocationsOfEmployee, employee.FullName))
                .Replace("SIGNATURE","The Apac I&D Program team");

            var emailRecipients = GetEmailRecipientsByEmailType(EmailType.INNOVATIONANDDESIGN, isSendToSupportMailbox);

            if (environment == Environments.Production)
            {
                if (emailRecipients.ContainsKey("ToEmailAddress"))
                {
                    emailRecipients["ToEmailAddress"] = employee.InternetAddress;
                }
                else
                {
                    emailRecipients.Add("ToEmailAddress", employee.InternetAddress);
                }
            }

            subjectLine = "Action required: Please Review Staffing Allocations in BOSS" + (environment != Environments.Production ? " - " + environment : "");

            EmailHelper.SendEmailWithO365(emailRecipients, body, subjectLine);
        }

        private void SendEmailSummaryToSupportMailbox(IList<EmailUtilityData> emailUtilityDataLogs, string emailType)
        {
            bool isSendToSupportMailbox = true;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string subjectLine = string.Empty, body;

            body = GetSupportGroupMailBody().ToString();
            
            if(emailType == EmailType.EXPERTS)
            {
                body = body.Replace("FAILED_EMPLOYEES_TABLE", GetEmployeesTable(emailUtilityDataLogs?.Where(x => x.status == Constants.EmailStatus.FAILED)))
                .Replace("SUCCESS_EMPLOYEES_TABLE", GetEmployeesTable(emailUtilityDataLogs?.Where(x => x.status == Constants.EmailStatus.SUCCESS)))
                .Replace("SIGNATURE", "The Expert Program team");

                subjectLine = "Please review the email summary to expert group" + (environment != Environments.Production ? " - " + environment : "");

            }
            else if(emailType == EmailType.INNOVATIONANDDESIGN)
            {
                body = body.Replace("FAILED_EMPLOYEES_TABLE", GetEmployeesTable(emailUtilityDataLogs?.Where(x => x.status == Constants.EmailStatus.FAILED)))
                .Replace("SUCCESS_EMPLOYEES_TABLE", GetEmployeesTable(emailUtilityDataLogs?.Where(x => x.status == Constants.EmailStatus.SUCCESS)))
                .Replace("SIGNATURE", "The Apac I&D Program team");

                subjectLine = "Please review the email summary to Apac I&D  group" + (environment != Environments.Production ? " - " + environment : "");

            }

            var emailRecipients = GetEmailRecipientsByEmailType(emailType, isSendToSupportMailbox);
            EmailHelper.SendEmailWithO365(emailRecipients, body, subjectLine);
        }


        private Dictionary<string, string> GetEmailRecipientsByEmailType(string emailType, bool isSendToSupportMailbox = true)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var emailRecipients = new Dictionary<string, string>();

            if (environment != Environments.Production)
            {
                emailRecipients.Add("FromEmailAddress", ConfigurationUtility.GetValue("DebugEmail:FromEmailAddress"));
                emailRecipients.Add("ToEmailAddress", ConfigurationUtility.GetValue("DebugEmail:ToEmailAddress"));
                emailRecipients.Add("CcEmailAddress", ConfigurationUtility.GetValue("DebugEmail:CcEmailAddress"));
                emailRecipients.Add("BccEmailAddress", ConfigurationUtility.GetValue("DebugEmail:BCCEmailAddress"));
            }
            else
            {
                if(emailType == EmailType.EXPERTS)
                {
                    //If production send to actual users and from Expert mailbox
                    emailRecipients.Add("FromEmailAddress", ConfigurationUtility.GetValue("ExpertEmailUtility:FromEmailAddress"));

                    if(isSendToSupportMailbox)
                    {
                        emailRecipients.Add("ToEmailAddress", ConfigurationUtility.GetValue("ExpertEmailUtility:staffingSupportGroupEmailAddress"));
                        emailRecipients.Add("CcEmailAddress", ConfigurationUtility.GetValue("ExpertEmailUtility:developerGroupEmailAddress"));
                    }
                    else
                    {
                        emailRecipients.Add("CcEmailAddress", ConfigurationUtility.GetValue("ExpertEmailUtility:CCEmailAddress"));
                        emailRecipients.Add("BccEmailAddress", ConfigurationUtility.GetValue("ExpertEmailUtility:BCCEmailAddress"));
                    }
                    
                }
                else if(emailType == EmailType.INNOVATIONANDDESIGN)
                {
                    //If production send to actual users and from Expert mailbox
                    emailRecipients.Add("FromEmailAddress", ConfigurationUtility.GetValue("InnovationAndDesignEmailUtility:FromEmailAddress"));

                    if (isSendToSupportMailbox)
                    {
                        emailRecipients.Add("ToEmailAddress", ConfigurationUtility.GetValue("InnovationAndDesignEmailUtility:staffingSupportGroupEmailAddress"));
                        emailRecipients.Add("CcEmailAddress", ConfigurationUtility.GetValue("InnovationAndDesignEmailUtility:developerGroupEmailAddress"));
                    }
                    else
                    {
                        emailRecipients.Add("CcEmailAddress", ConfigurationUtility.GetValue("InnovationAndDesignEmailUtility:CCEmailAddress"));
                        emailRecipients.Add("BccEmailAddress", ConfigurationUtility.GetValue("InnovationAndDesignEmailUtility:BCCEmailAddress"));
                    }
                }
                
            }

            return emailRecipients;
        }

        private StringBuilder GetSupportGroupMailBody()
        {
            var emailMessage = new StringBuilder();
            emailMessage.Clear();
            emailMessage.Append(
                "<div style='font: 10pt Arial; margin-bottom: 20px; padding-bottom: 4px;'>");
            emailMessage.Append(
                "Hello,");

            emailMessage.Append(
                "<p>Hope you are doing well!</p>");

            emailMessage.Append(
                "<p>You are receiving this email as we would like you to <strong> review which employees successfully/failed to receive the email.</strong> The below is a record of <strong>employees who successfully received the email and who failed to receive the email regarding their allocations</strong>.</p>");

            emailMessage.Append("<p>");
            emailMessage.Append(
                "Please take manual steps for employees who failed to receive the email. ");
            emailMessage.Append("<ol>");

            emailMessage.Append(
                "<li>Employees who <strong> failed to receive </strong>the email:</li>");

            emailMessage.Append("<div>");
            emailMessage.Append("FAILED_EMPLOYEES_TABLE");
            emailMessage.Append("</div>");

            emailMessage.Append(
                "<li>Employees who <strong> successfully received </strong> the email:</li>");

            emailMessage.Append("<div>");
            emailMessage.Append("SUCCESS_EMPLOYEES_TABLE");
            emailMessage.Append("</div>");

            emailMessage.Append("</ol>");
            emailMessage.Append("</p>");

            emailMessage.Append("</div>");

            emailMessage.Append(
                "Thank you,");
            emailMessage.Append("<br>");
            emailMessage.Append(
                "SIGNATURE");
            emailMessage.Append("</div>");

            return emailMessage;
        }

        private string GetEmployeesTable(IEnumerable<EmailUtilityData> employeeList)
        {
            var table = new StringBuilder();
            int count = 0;

            //Allocations Table
            table.Append("<table width='100%' border='1' style='border-collapse: collapse;font: 10pt Arial'>");
            table.Append(GetTableHeader(true));
            if (employeeList.Count() == 0)
            {
                table.Append("<tr>");
                table.Append("<td colspan='7' style='text-align:center;'>No Employees</td>");
                table.Append("</tr>");
            }
            foreach (var employee in employeeList)
            {
                count = count + 1;
                table.Append("<tr>");
                table.Append(GetTableCell(count.ToString()));
                table.Append(GetTableCell(employee.employeeName));
                table.Append(GetTableCell(employee.employeeCode));
                table.Append(GetTableCell(employee.status));
                table.Append(GetTableCell(employee.positionGroupName));
                table.Append(GetTableCell(employee.serviceLineName));
                table.Append(GetTableCell(employee.officeName));
                table.Append("</tr>");
            }
            table.Append("</table>");

            return table.ToString();
        }

        private StringBuilder GetMailBodyTemplate()
        {
            var emailMessage = new StringBuilder();
            emailMessage.Clear();
            emailMessage.Append(
                "<div style='font: 10pt Arial; margin-bottom: 20px; padding-bottom: 4px;'>");
            emailMessage.Append(
                "Hello EMPLOYEE_NAME,");

            emailMessage.Append(
                "<p>Hope you are doing well!</p>");

            emailMessage.Append(
                "<p>You are receiving this email as we would like you to <strong> review your staffing allocations.</strong> The below is a record of your <strong>active staffing allocations covering the period from <font color='red'>ALLOC_START_DATE 1<sup>st</sup></font> - onwards, </strong>pulled directly from BOSS (Bain's Staffing System).</p>");

            emailMessage.Append("<p>");
            emailMessage.Append(
                "Please set aside <strong>~10 minutes to review the information below</strong> and reply to this email with either: ");
            emailMessage.Append("<ol>");
            emailMessage.Append(
                "<li><strong> Yes,</strong> I confirm that my current staffing record is correct and up-to-date </li>");
            emailMessage.Append(
                "<li><strong>No,</strong> my current staffing record is incorrect and/or missing information</li>");
            emailMessage.Append("</ol>");
            emailMessage.Append("</p>");

            emailMessage.Append("<p>");
            emailMessage.Append(
                "<strong>If any staffing allocations are missing or the information below needs updating, please reply back to this email indicating the updates and our team will update in BOSS.</strong>");
            emailMessage.Append("</p>");
            emailMessage.Append("</div>");

            emailMessage.Append("<div>");
            emailMessage.Append("ALLOCATIONS_TABLE");
            emailMessage.Append("</div>");

            emailMessage.Append(
                "<div style='font: 10pt Arial;'>");
            emailMessage.Append("<p>");
            emailMessage.Append(
                "Do note that per Global FP&A guidance, we are not able to approve retroactive change requests after financial close, therefore <strong>we will not be allowed to change staffing data after the 25<sup>th</sup> of the following month.</strong>");
            emailMessage.Append("</p>");

            emailMessage.Append("<p>");
            emailMessage.Append(
                "<strong>As a best practice, do let us know as soon as you start on a new assignment, to help us keep the staffing system up to date.</strong> Other benefits of this process include correct information on your Iris profile, more accurate capture of price realization on cases, and correct trigger of HR processes (upward feedback, LTSU, CTSU, Case Reviews,etc.,).");
            emailMessage.Append("</p>");

            emailMessage.Append("<p>");
            emailMessage.Append(
                "<em>*Note: If you have any feedback on how we share this information each month, please let us know.</em>");
            emailMessage.Append("</p>");

            emailMessage.Append(
                "Thank you,");
            emailMessage.Append("<br>");
            emailMessage.Append(
                "SIGNATURE");
            emailMessage.Append("</div>");



            return emailMessage;
        }

        private string GetAllocationsTable(IEnumerable<ResourceAssignmentViewModel> resourceAllocationsOfEmployee, string employeeFullName)
        {
            var table = new StringBuilder();

            //Allocations Table
            table.Append("<table width='100%' border='1' style='border-collapse: collapse;font: 10pt Arial'>");
            table.Append(GetTableHeader());
            if(resourceAllocationsOfEmployee.Count() == 0)
            {
                table.Append("<tr>");

                table.Append("<td colspan='1'>" + employeeFullName + "</td>");
                table.Append("<td colspan='8' style='text-align:center;'>No Staffing Data</td>");
                table.Append("</tr>");
            }
            foreach (var resourceAllocation in resourceAllocationsOfEmployee)
            {
                table.Append("<tr>");
                table.Append(GetTableCell(resourceAllocation.EmployeeName));
                table.Append(GetTableCell(resourceAllocation.ClientName));
                table.Append(GetTableCell(resourceAllocation.CaseName));
                table.Append(GetTableCell(resourceAllocation.OldCaseCode));
                table.Append(GetTableCell(resourceAllocation.Allocation.ToString()));
                table.Append(GetTableCell(resourceAllocation.CaseTypeCode.ToString()));
                table.Append(GetTableCell(resourceAllocation.StartDate.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)));
                table.Append(GetTableCell(resourceAllocation.EndDate.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)));
                table.Append(GetTableCell(resourceAllocation.Notes));
                table.Append("</tr>");
            }
            table.Append("</table>");

            return table.ToString();
        }

        private string GetTableHeader(bool sendToSupportGroup = false)
        {
            var columnsNames = sendToSupportGroup ?
                new string[] { "S.No.", "Name", "Employee Code", "Status", "Position Group", "Service Line", "Office"} :
                new string[] { "Name", "Client", "Case Name" , "Case Code", "Allocation", "Case Type", "Start Date", "End Date", "Notes" };

            var sb = new StringBuilder();
            sb.Append("<tr>");

            foreach (var columnHeader in columnsNames)
            {
                sb.Append("<th style='min-width:70px' align ='left'>" + columnHeader + "</th>");
            }

            sb.Append("</th></tr>");
            return sb.ToString();
        }

        private string GetTableCell(string cellData)
        {
            return ("<td style='min-width:70px' align ='left'>" + cellData + "</td>");
        }
    }
}
