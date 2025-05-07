using CCM.API.Contracts.Services;
using CCM.API.ViewModels;
using CCM.API.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCM.API.Core.Helpers;

namespace CCM.API.Core.Services
{
    public class ClientCaseAPIService: IClientCaseAPIService
    {
        private readonly IClientCaseAPIClient _clientCaseAPIClient;

        public ClientCaseAPIService(IClientCaseAPIClient clientCaseAPIClient)
        {
            _clientCaseAPIClient = clientCaseAPIClient;
        }

        public async Task<IEnumerable<CaseViewModelBasic>> GetModifiedCasesAfterLastPolledTime(DateTime? lastPolledTime)
        {
            if (lastPolledTime == null || lastPolledTime.Value == DateTime.MinValue)
            {
                lastPolledTime = Convert.ToDateTime(ConfigurationUtility.GetValue("SqlMinDate"));
            }

            var cases = await _clientCaseAPIClient.GetModifiedCasesAfterLastPolledTime(lastPolledTime);
            var modifiedCases = Enumerable.Empty<CaseViewModelBasic>();

            if (cases.Any())
            {
                modifiedCases = ConvertCaseDataFromClientCaseAPIToCaseViewModelBasic(cases);
            }

            return modifiedCases;
        }

        public async Task<IEnumerable<ClientViewModel>> GetClientsForTypeahead(string searchtext)
        {
            if (string.IsNullOrEmpty(searchtext))
                return Enumerable.Empty<ClientViewModel>();

            var clientsFromClientCaseAPI = await _clientCaseAPIClient.GetClientsForTypeahead(searchtext);

            var clients  = ConvertClientDataFromClientCaseAPIToClientViewModel(clientsFromClientCaseAPI);

            return clients;
        }

        #region Private Methods
        private static IEnumerable<CaseViewModelBasic> ConvertCaseDataFromClientCaseAPIToCaseViewModelBasic(IEnumerable<CaseDataFromClientCaseAPI> caseDataFromClientCaseAPIs)
        {
            var startDateForNewDemand = DateTime.Now.Date;

            var caseData = caseDataFromClientCaseAPIs.Select(r => new CaseViewModelBasic
            {
                CaseCode = r.CaseId.Value,
                CaseName = r.CaseName,
                CaseManagerCode = r.CaseManager ?? string.Empty,
                ClientCode = r.ClientId.Value,
                ClientName = r.ClientName,
                OldCaseCode = r.CaseCode,
                CaseType = r.CaseTypeName,
                CaseTypeCode = r.CaseType,
                ManagingOfficeCode = r.CaseOffice,
                BillingOfficeCode = r.BillingOffice,
                StartDate = r.StartDate.Value,
                EndDate = (r.EndDate == null || r.EndDate.Value.Equals(DateTime.MinValue)) ? r.ProjectedEndDate.Value : r.EndDate.Value,
                PrimaryIndustryTermCode = r.PrimaryIndustryTermCode,
                PrimaryIndustryTagId = r.PrimaryIndustryTagId,
                PrimaryCapabilityTermCode = r.PrimaryCapabilityTermCode,
                PrimaryCapabilityTagId = r.PrimaryCapabilityTagId,
                IsPrivateEquity = r.IsPegCase.ToUpper().Equals("YES"),
                CaseAttributes = r.CaseAttributeDetails,
                Type = r.StartDate >= startDateForNewDemand ? Constants.NewDemand : Constants.ActiveCase,
                LastUpdated = r.LastUpdated
            });

            return caseData;
        }

        private static IEnumerable<ClientViewModel> ConvertClientDataFromClientCaseAPIToClientViewModel(IEnumerable<ClientDataFromClientCaseAPI> clientDataFromClientCaseAPI)
        {
            var clientData = clientDataFromClientCaseAPI.Select(r => new ClientViewModel
            {
                ClientCode = r.ClientId,
                ClientName = r.ClientName
            });

            return clientData;
        }

        #endregion
    }
}
