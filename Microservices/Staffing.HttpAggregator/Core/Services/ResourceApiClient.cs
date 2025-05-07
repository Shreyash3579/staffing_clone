using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class ResourceApiClient : IResourceApiClient
    {
        private readonly HttpClient _apiClient;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResourceApiClient(HttpClient httpClient, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("ResourceApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(20);
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;

            if (endpointAddress.Contains("aip"))
            {
                SetHttpClientHeaders();
            }
        }

        private void SetHttpClientHeaders()
        {
            var authorizationApiKey1 = ConfigurationUtility.GetValue("Authorization:ApiKey1");
            var authorizationApiKeyValue1 = ConfigurationUtility.GetValue("Authorization:ApiKeyValue1");
            var authorizationApiKey2 = ConfigurationUtility.GetValue("Authorization:ApiKey2");
            var authorizationApiKeyValue2 = ConfigurationUtility.GetValue("Authorization:ApiKeyValue2");

            var authorizationHeader1 = _httpContextAccessor.HttpContext
                .Request.Headers[authorizationApiKey1];
            var authorizationHeader2 = _httpContextAccessor.HttpContext
                .Request.Headers[authorizationApiKey2];

            _apiClient.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:ResourcesApi"));

            if (string.IsNullOrEmpty(authorizationHeader1))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey1, authorizationApiKeyValue1);
            }
            if (string.IsNullOrEmpty(authorizationHeader2))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey2, authorizationApiKeyValue2);
            }
        }

        public async Task<List<Resource>> GetEmployees()
        {
            var resources = await _cache.GetOrCreateAsync<IList<Resource>>("ActiveEmployees", async cacheEntry =>
            {
                cacheEntry.AbsoluteExpiration = DateTime.Now.AddMinutes(30);
                cacheEntry.Priority = CacheItemPriority.Normal;
                var responseMessage = await _apiClient.GetAsync("api/Resources/employees");
                var activeEmployees = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result) ?? new List<Resource>();
                return activeEmployees.ToList();
            });

            return resources.ToList();
        }

        public async Task<List<Resource>> GetEmployeesIncludingTerminated()
        {
            var resources = await _cache.GetOrCreateAsync<IList<Resource>>("AllEmployees", async cacheEntry =>
           {
               cacheEntry.AbsoluteExpiration = DateTime.Now.AddMinutes(30);
               cacheEntry.Priority = CacheItemPriority.Normal;
               var responseMessage = await _apiClient.GetAsync("api/Resources/employeesIncludingTerminated");
               var allEmployees = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync()
                   .Result) ?? new List<Resource>();
               return allEmployees.ToList();
           });

            return resources.ToList();
        }

        public async Task<IEnumerable<Resource>> GetActiveEmployeesFilteredBySelectedValues(string officeCodes, DateTime startDate, DateTime endDate,
           string levelGrades, string positionCodes)
        {
            return await GetActiveEmployeesFilteredBySelectedValues(officeCodes, startDate, endDate, levelGrades, positionCodes, null);
        }

            public async Task<IEnumerable<Resource>> GetActiveEmployeesFilteredBySelectedValues(string officeCodes, DateTime startDate, DateTime endDate,
            string levelGrades, string positionCodes, string oDataQuery)
        {

            var payload = new { officeCodes, startDate, endDate, levelGrades, positionCodes };

            var url = oDataQuery != null ? $"api/Resources/employeesFilteredBySelectedValues?{oDataQuery}" : $"api/Resources/employeesFilteredBySelectedValues";

            var responseMessage = await _apiClient.PostAsJsonAsync(url, payload);
            var resources = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<Resource>();

            return resources;
        }

        public async Task<IEnumerable<Resource>> GetActiveEmployeesFilteredBySelectedGroupValues(string employeeCodes,
           DateTime startDate, DateTime endDate)
        {
            return await GetActiveEmployeesFilteredBySelectedGroupValues(employeeCodes, startDate, endDate, null); 
        }

            public async Task<IEnumerable<Resource>> GetActiveEmployeesFilteredBySelectedGroupValues(string employeeCodes, 
            DateTime startDate, DateTime endDate, string oDataQuery)
        {
            var payload = new { employeeCodes, startDate, endDate };

            var url = oDataQuery != null ? $"api/Resources/employeesFilteredBySelectedGroupValues?{oDataQuery}" : $"api/Resources/employeesFilteredBySelectedGroupValues";

            var responseMessage = await _apiClient.PostAsJsonAsync(url, payload);
            var resources = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<Resource>();

            return resources;
        }

        public async Task<IEnumerable<Resource>> GetEmployeesBySearchString(string searchString, bool? addTransfers = false)
        {
            var url = $"api/Resources/employeesBySearchString?searchString={searchString}&addTransfers={addTransfers}";

            var responseMessage = await _apiClient.GetAsync(url);
            var resources = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<Resource>();

            return resources;
        }

        public async Task<IEnumerable<Resource>> GetEmployeesIncludingTerminatedBySearchString(string searchString, bool? addTransfers = false)
        {
            IEnumerable<Resource> resources = Enumerable.Empty<Resource>();

            var url = $"api/Resources/employeesIncludingTerminatedBySearchString?searchString={searchString}&addTransfers={addTransfers}";
            
            if ((bool)!addTransfers)
            {
                resources = SearchEmployeeInMemoryCache(searchString);
            }
            if (!resources.Any())
            {
                var responseMessage = await _apiClient.GetAsync(url);
                resources = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<Resource>();
            }

            return resources;
        }

        public async Task<IEnumerable<ResourceLoA>> GetLOAsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<ResourceLoA>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/resourcesloa/loaByEmployees", payload);

            var resourcesLoAs =
                JsonConvert.DeserializeObject<IEnumerable<ResourceLoA>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceLoA>();

            return resourcesLoAs.ToList();
        }

        public async Task<IEnumerable<LOA>> GetLOAsByEmployeeCode(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Enumerable.Empty<LOA>();
            }

            var responseMessage =
               await _apiClient.GetAsync($"api/resourcesloa/LOAByEmployee?employeeCode={employeeCode}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");

            var resourcesLoAs =
                JsonConvert.DeserializeObject<IEnumerable<LOA>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<LOA>();

            return resourcesLoAs.ToList();
        }

        public async Task<Resource> GetEmployeeByEmployeeCode(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return new Resource();
            }

            var responseMessage =
               await _apiClient.GetAsync($"api/resources/employee?employeeCode={employeeCode}");

            var resource = JsonConvert.DeserializeObject<Resource>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new Resource();

            return resource;
        }

        public async Task<List<Resource>> GetEmployeesByEmployeeCodes(string employeeCodes)
        {
            if (string.IsNullOrEmpty(employeeCodes))
            {
                return new List<Resource>();
            }

            // Split and Format the employee codes in the OData syntax
            var codesArray = employeeCodes.Split(',').Select(code => code.Trim());
            var formattedCodes = string.Join("', '", codesArray);

            // Construct the OData query
            var odataQuery = $"api/resources/employees?$filter=employeecode in ('{formattedCodes}')";

            var responseMessage = await _apiClient.GetAsync(odataQuery);

            var resources = JsonConvert.DeserializeObject<List<Resource>>(await responseMessage.Content.ReadAsStringAsync()) ?? new List<Resource>();

            return resources;
        }


        public async Task<IEnumerable<ResourceTimeInLevel>> GetTimeInLevelByEmployeeCode(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Enumerable.Empty<ResourceTimeInLevel>();
            }

            var responseMessage =
               await _apiClient.GetAsync($"api/resourcesTimeInLevel/timeInLevelByEmployee?employeeCode={employeeCode}");

            var employeeTimeInLevel = JsonConvert.DeserializeObject<IEnumerable<ResourceTimeInLevel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceTimeInLevel>();

            return employeeTimeInLevel.ToList();
        }

        public async Task<IEnumerable<ResourceTransfer>> GetEmployeesPendingTransfersByEndDate(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<ResourceTransfer>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/resourcestransactions/transferByEmployees", payload);

            var resourcesTransfers =
                JsonConvert.DeserializeObject<IEnumerable<ResourceTransfer>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceTransfer>();

            return resourcesTransfers.ToList();
        }

        public async Task<IEnumerable<ResourceTransfer>> GetEmployeeTransfersWithinDateRange(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return Enumerable.Empty<ResourceTransfer>();

            var responseMessage =
               await _apiClient.GetAsync($"api/resourcestransactions/employeeTransfers?employeeCode={employeeCode}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");

            var resourcesTransfers =
                JsonConvert.DeserializeObject<IEnumerable<ResourceTransfer>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceTransfer>();

            return resourcesTransfers.ToList();
        }

        public async Task<IEnumerable<ResourceTransition>> GetTransitionsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<ResourceTransition>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/resourcestransactions/transitionByEmployees", payload);

            var resourcesTransitions =
                JsonConvert.DeserializeObject<IEnumerable<ResourceTransition>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceTransition>();

            return resourcesTransitions.ToList();
        }

        public async Task<ResourceTransition> GetTransitionByEmployeeCode(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return new ResourceTransition();

            var responseMessage =
               await _apiClient.GetAsync($"api/resourcestransactions/transitionByEmployee?employeeCode={employeeCode}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");

            var resourcesTransition =
                JsonConvert.DeserializeObject<ResourceTransition>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new ResourceTransition();

            return resourcesTransition;
        }

        public async Task<IEnumerable<ResourceTermination>> GetPendingTerminationsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<ResourceTermination>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/resourcesTransactions/pendingTerminationsWithinDateRangeByEmployees", payload);

            var resourcesTransitions =
                JsonConvert.DeserializeObject<IEnumerable<ResourceTermination>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceTermination>();

            return resourcesTransitions.ToList();
        }

        public async Task<ResourceTermination> GetTerminationByEmployeeCode(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return new ResourceTermination();

            var responseMessage =
               await _apiClient.GetAsync($"api/resourcesTransactions/terminationByEmployee?employeeCode={employeeCode}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");

            var resourcesTransitions =
                JsonConvert.DeserializeObject<ResourceTermination>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new ResourceTermination();

            return resourcesTransitions;
        }

        public async Task<IEnumerable<ResourceTimeOff>> GetEmployeesTimeoffs(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<ResourceTimeOff>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/resourcesTimeOff/timeOffsByEmployees", payload);

            var resourcesTimeOffs =
                JsonConvert.DeserializeObject<IEnumerable<ResourceTimeOff>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceTimeOff>();

            return resourcesTimeOffs.ToList();
        }

        public async Task<IEnumerable<ResourceTimeOff>> GetEmployeeTimeoffs(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return Enumerable.Empty<ResourceTimeOff>();

            var responseMessage =
               await _apiClient.GetAsync($"api/resourcesTimeOff/timeOffsByEmployee?employeeCode={employeeCode}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");

            var resourcesTimeOffs =
                JsonConvert.DeserializeObject<IEnumerable<ResourceTimeOff>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceTimeOff>();

            return resourcesTimeOffs.ToList();
        }

        public async Task<IEnumerable<ServiceLine>> GetServiceLines()
        {
            var responseMessage = await _apiClient.GetAsync($"api/Lookup/serviceLineList");
            var serviceLines = JsonConvert.DeserializeObject<IEnumerable<ServiceLine>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ServiceLine>();

            return serviceLines;
        }
        public async Task<IEnumerable<Office>> GetOffices()
        {
            var responseMessage = await _apiClient.GetAsync($"api/Lookup/officeList");
            var offices = JsonConvert.DeserializeObject<IEnumerable<Office>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<Office>();

            return offices;
        }

        public async Task<IEnumerable<EmployeeCertificates>> GetCertificatesByEmployeeCodes(string listEmployeeCodes)
        {
            if (string.IsNullOrEmpty(listEmployeeCodes))
                return Enumerable.Empty<EmployeeCertificates>();

            var payload = new { listEmployeeCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/resourcesCertification/certificationsByEmployees", payload);
            var employeesCertificates = JsonConvert.DeserializeObject<IEnumerable<EmployeeCertificates>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<EmployeeCertificates>();

            return employeesCertificates;
        }

        public async Task<IEnumerable<EmployeeLanguages>> GetLanguagesByEmployeeCodes(string listEmployeeCodes)
        {
            if (string.IsNullOrEmpty(listEmployeeCodes))
                return Enumerable.Empty<EmployeeLanguages>();

            var payload = new { listEmployeeCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/resourcesLanguage/employeeLanguagesByEmployeeCodes", payload);
            var employeesLanguages = JsonConvert.DeserializeObject<IEnumerable<EmployeeLanguages>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<EmployeeLanguages>();

            return employeesLanguages;
        }

        #region Private Methods
        private IEnumerable<Resource> SearchEmployeeInMemoryCache(string searchString)
        {
            IEnumerable<Resource> resources = Enumerable.Empty<Resource>();
            var cacheEntry = _cache.Get<IEnumerable<Resource>>("AllEmployees");
            if (cacheEntry != null && cacheEntry.Any())
            {
                var accessibleOffices = JWTHelper.GetAccessibleOffices(_httpContextAccessor.HttpContext);
                var officeCodes = accessibleOffices != null ? string.Join(",", accessibleOffices): null;

                resources = cacheEntry.Where(e =>
                        e.FirstName.RemoveDiacritics().StartsWith(searchString,
                            StringComparison.InvariantCultureIgnoreCase) ||
                        e.LastName.RemoveDiacritics().StartsWith(searchString,
                            StringComparison.InvariantCultureIgnoreCase) ||
                        $"{e.FirstName} {e.LastName}".RemoveDiacritics().StartsWith(searchString,
                            StringComparison.InvariantCultureIgnoreCase) ||
                        e.FullName.RemoveDiacritics().StartsWith(searchString,
                            StringComparison.InvariantCultureIgnoreCase) || e.EmployeeCode.ToLower() == searchString.ToLower())
                    .OrderBy(p => p.ActiveStatus).ThenBy(x => x.FullName).ThenByDescending(r => r.LevelGrade.PadNumbers()).ToList();

                if (!string.IsNullOrEmpty(officeCodes))
                {
                    resources = resources.Where(x => officeCodes.Contains(x.SchedulingOffice?.OfficeCode.ToString()));
                }

                resources = resources.Take(50);

            }
            return resources;
        }

        #endregion
    }
}
