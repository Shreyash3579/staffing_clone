using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackgroundPolling.API.Models;
using System.Linq;

namespace BackgroundPolling.API.Core.Services
{
    public class StaffingPollingService : IStaffingPollingService
    {
        private readonly IStaffingPollingRepository _staffingPollingRepository;
        private readonly ICcmApiClient _ccmApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IResourceApiClient _resourceApiClient;

        public StaffingPollingService(IStaffingPollingRepository staffingPollingRepository,
            ICcmApiClient ccmApiClient, IStaffingApiClient staffingApiClient, IResourceApiClient resourceApiClient)
        {
            _staffingPollingRepository = staffingPollingRepository;
            _ccmApiClient = ccmApiClient;
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
        }
        
        public async Task<string> DeleteSecurityUsersWithExpiredEndDate()
        {
            return await _staffingPollingRepository.DeleteSecurityUsersWithExpiredEndDate();
        }

        public async Task DeleteAnalyticsLog()
        {
            await _staffingPollingRepository.DeleteAnalyticsLog();
        }

        public async Task UpdateSecurityUserForWFPRole()
        {
            List<OfficeHierarchyDetails> newAddedOfficeHierarchyList = await getNewlyAddedOffices();

            List<ServiceLine> newlyAddedServiceLineList = await getNewlyAddedServiceLines();


            if (!newAddedOfficeHierarchyList.Any() && !newlyAddedServiceLineList.Any())
            {
                return;
            }

            await _staffingApiClient.UpdateSecurityUserForWFPRole(newAddedOfficeHierarchyList, newlyAddedServiceLineList);
            
        }

        private async Task<List<OfficeHierarchyDetails>> getNewlyAddedOffices()
        {
            var officeList = await _ccmApiClient.GetRevOfficeList();
            var previousOfficeList = await _staffingApiClient.GetRevOfficeList();

            await _staffingApiClient.SaveRevOfficeList(officeList);

            var newlyAddedOffices = officeList
                .Where(o => !previousOfficeList.Any(p => p.OfficeCode == o.OfficeCode))
                .ToList();


            if (newlyAddedOffices.Count == 0 || previousOfficeList == null || !previousOfficeList.Any())
            {
                return new List<OfficeHierarchyDetails>();
            }

            // List to store office hierarchy details
            List <OfficeHierarchyDetails> officeHierarchyList = new List<OfficeHierarchyDetails>();

            foreach (var newOffice in newlyAddedOffices)
            {
                var parentOfficeCodes = FindParentOffices(newOffice.OfficeCode, officeList);

                officeHierarchyList.Add(new OfficeHierarchyDetails
                {
                    OfficeCode = newOffice.OfficeCode.ToString(),
                    ParentOfficeCodes = string.Join(",", parentOfficeCodes)
                });
            }

            return officeHierarchyList;
        }

        private async Task<List<ServiceLine>> getNewlyAddedServiceLines()
        {
            var serviceLineList = await _resourceApiClient.GetServiceLines();
            var previousServiceLineList = await _staffingApiClient.GetServiceLineList();

            await _staffingApiClient.SaveServiceLineList(serviceLineList);

            var newlyAddedServiceLines = serviceLineList
                .Where(o => !previousServiceLineList.Any(p => p.ServiceLineCode == o.ServiceLineCode))
                .ToList();

            if (newlyAddedServiceLines.Count == 0 || previousServiceLineList == null || !previousServiceLineList.Any())
            {
                return new List<ServiceLine>();
            }

            return newlyAddedServiceLines;

        }

        private IEnumerable<string> FindParentOffices(int officeCode, IEnumerable<RevOffice> officeList)
        {
            var parentOffices = new List<string>();

            // Make sure the office exists in the office list
            var office = officeList.FirstOrDefault(o => o.OfficeCode == officeCode);

            if (office != null)
            {
                parentOffices.Add(office.ParentOfficeCode.ToString());

                var parentOfficesRecursive = FindParentOffices(office.ParentOfficeCode, officeList);
                parentOffices.AddRange(parentOfficesRecursive);
            }

            return parentOffices.Distinct().ToList();
        }


    }
}
