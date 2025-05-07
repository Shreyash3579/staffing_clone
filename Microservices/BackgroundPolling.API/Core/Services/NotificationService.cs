using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IResourceApiClient _resourceApiClient;
        private readonly ICcmApiClient _ccmApiClient;
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(IResourceApiClient resourceApiClient, INotificationRepository notificationRepository, ICcmApiClient ccmApiClient)
        {
            _resourceApiClient = resourceApiClient;
            _ccmApiClient = ccmApiClient;
            _notificationRepository = notificationRepository;
        }
        public async Task<IEnumerable<Notification>> InsertCasesEndingNotification()
        {
            var caseEndsBeforeNumberOfDays = Convert.ToInt16(ConfigurationUtility.GetValue("Threshold:caseEndsBeforeNumberOfDays"));
            var casesEnding = await _ccmApiClient.GetCasesEndingBySpecificDate(caseEndsBeforeNumberOfDays);

            if (!casesEnding.Any())
                return Enumerable.Empty<Notification>();

            var notificationsForCaseEnding = casesEnding.Select(c => new Notification
            {
                NotificationTypeCode = Convert.ToInt16(NotificationType.CaseEnd),
                OfficeCode = c.ManagingOfficeCode ?? default(int), //TODO: Make office code in case model non nullable
                OldCaseCode = c.OldCaseCode,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                NotificationNotes = $"Case Ending - Case {c.CaseName} - {c.OldCaseCode} is ending",
            }).ToList();

            if (notificationsForCaseEnding.Count < 1)
                return Enumerable.Empty<Notification>();

            await _notificationRepository.InsertNotification(GetNotificationDTO(notificationsForCaseEnding));

            return notificationsForCaseEnding;

        }

        public async Task<IEnumerable<Notification>> InsertBackFillNotification()
        {
            var allocationEndingBeforeNumberOfDays = Convert.ToInt16(ConfigurationUtility.GetValue("Threshold:allocationEndingBeforeNumberOfDays"));
            var resourcesRequiresBackFill = await _notificationRepository.GetEmployeesRequiresBackFillBySpecificDate(allocationEndingBeforeNumberOfDays);

            if (!resourcesRequiresBackFill.Any())
                return Enumerable.Empty<Notification>();

            // Copy distinct items to get case details
            var resourceRequiresBackFillCopy = resourcesRequiresBackFill
                .GroupBy(rb => rb.OldCaseCode).Select(g => g.First()).ToList();

            var casesData = new List<CaseViewModel>();

            // Get cases in chunks as passing large number of oldCaseCodes might exceeds limit of query length allowed by IIS
            while (resourceRequiresBackFillCopy.Any())
            {
                var oldCaseCodes = string.Join(",", resourceRequiresBackFillCopy.ToList().Take(400).Select(un => un.OldCaseCode).Distinct());
                var casesPartialData = await _ccmApiClient.GetCaseDataByCaseCodes(oldCaseCodes);
                casesData.AddRange(casesPartialData);
                resourceRequiresBackFillCopy = resourceRequiresBackFillCopy.Skip(400).ToList();
            }
            var resources = await _resourceApiClient.GetEmployees();

            var notificationsForBackFill = (from backFill in resourcesRequiresBackFill
                                            let caseData = casesData.FirstOrDefault(cd => cd.OldCaseCode == backFill.OldCaseCode)
                                            where backFill.EndDate < caseData?.EndDate
                                            let resource = resources.FirstOrDefault(r => r.EmployeeCode == backFill.EmployeeCode)
                                            select new Notification
                                            {
                                                NotificationTypeCode = Convert.ToInt16(NotificationType.BackFill),
                                                EmployeeCode = backFill.EmployeeCode,
                                                OfficeCode = caseData.ManagingOfficeCode ?? default(int),
                                                OldCaseCode = backFill.OldCaseCode,
                                                StartDate = backFill.StartDate,
                                                EndDate = backFill.EndDate,
                                                NotificationNotes = $"Backfill - {resource?.FirstName} {resource?.LastName} might need a backfill " +
                                                                    $"on case {caseData?.CaseName} - {caseData?.OldCaseCode}",
                                            }).ToList();

            if (notificationsForBackFill.ToList().Count < 1)
                return Enumerable.Empty<Notification>();

            await _notificationRepository.InsertNotification(GetNotificationDTO(notificationsForBackFill));

            return notificationsForBackFill;
        }

        public static DataTable GetNotificationDTO(IEnumerable<Notification> notifications)
        {
            var notificationDataTable = new DataTable();
            notificationDataTable.Columns.Add("notificationTypeCode", typeof(Int16));
            notificationDataTable.Columns.Add("officeCode", typeof(Int16));
            notificationDataTable.Columns.Add("employeeCode", typeof(string));
            notificationDataTable.Columns.Add("oldCaseCode", typeof(string));
            notificationDataTable.Columns.Add("startDate", typeof(DateTime));
            notificationDataTable.Columns.Add("endDate", typeof(DateTime));
            notificationDataTable.Columns.Add("notificationNotes", typeof(string));

            foreach (var notification in notifications)
            {
                var row = notificationDataTable.NewRow();

                row["notificationTypeCode"] = notification.NotificationTypeCode;
                row["officeCode"] = notification.OfficeCode;
                row["employeeCode"] = (object)notification.EmployeeCode ?? DBNull.Value;
                row["oldCaseCode"] = (object)notification.OldCaseCode ?? DBNull.Value;
                row["startDate"] = (object)notification.StartDate ?? DBNull.Value;
                row["endDate"] = (object)notification.EndDate ?? DBNull.Value;
                row["notificationNotes"] = (object)notification.NotificationNotes ?? DBNull.Value;

                notificationDataTable.Rows.Add(row);

            }
            return notificationDataTable;
        }


    }
}
