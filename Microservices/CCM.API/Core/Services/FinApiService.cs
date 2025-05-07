using CCM.API.Contracts.Services;
using CCM.API.Models;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCM.API.Core.Helpers;

namespace CCM.API.Core.Services
{
    public class FinApiService : IFinApiService
    {
        private readonly IFinApiClient _finApiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FinApiService(IFinApiClient finApiClient, IHttpContextAccessor httpContextAccessor)
        {
            _finApiClient = finApiClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Office>> GetOfficesFlatListByRegionOrCluster(int regionOrClusterCode)
        {
            var officeHierarchy = await _finApiClient.GetOffices("REV", "A");

            var accessibleOfficesData = GetAccessibleOfficesAndHierarchyTopDown(officeHierarchy, regionOrClusterCode, new List<int>()).ToList();

            var result = officeHierarchy.Where(x => accessibleOfficesData.Contains(x.OfficeCode) && x.EntityTypeCode == "O")
                .Select(item => new Office
                {
                    OfficeCode = item.OfficeCode,
                    OfficeAbbreviation = item.OfficeAbbreviation,
                    OfficeName = item.OfficeName
                }).ToList();

            return result ?? Enumerable.Empty<Office>();
        }

        public async Task<IEnumerable<Office>> GetOfficeList(IList<int> accessibleOffices)
        {
            var revOffices = await _finApiClient.GetOffices("REV", "A");

            var offices = revOffices.Where(x => x.EntityTypeCode == "O").Select(item =>
            {

                RevOffice officeCluster = getOfficeClusterRegion(item.OfficeCode, Constants.OfficeClusterEntityTypeCode, revOffices);
                RevOffice officeRegion = getOfficeClusterRegion(item.OfficeCode, Constants.OfficeRegionEntityTypeCode, revOffices);
                RevOffice officeSubRegion = getOfficeClusterRegion(item.OfficeCode, Constants.OfficeSubRegionEntityTypeCode, revOffices);


                return new Office
                {
                    OfficeCode = item.OfficeCode,
                    OfficeName = item.OfficeName,
                    OfficeAbbreviation = item.OfficeAbbreviation,
                    OfficeCluster = officeCluster != null && !string.IsNullOrEmpty(officeCluster.OfficeName) ? officeCluster.OfficeName : item.OfficeName,
                    OfficeRegion = officeRegion.OfficeName,
                    OfficeRegionCode = officeRegion.OfficeCode,
                    OfficeSubRegion = officeSubRegion?.OfficeName

                };
            }).ToList();

            if (accessibleOffices != null && accessibleOffices.Contains(0))
            {
                return Enumerable.Empty<Office>();
            }

            return accessibleOffices != null && accessibleOffices.Any()
                ? offices.Where(o => accessibleOffices.Contains(o.OfficeCode))
                : offices;
        }
  

        public async Task<OfficeHierarchy> GetOfficeHierarchyByOffices(string officeCodes)
        {
            if (string.IsNullOrEmpty(officeCodes))
                throw new ArgumentException("Office Code(s) can not be null");

            var officeList = officeCodes.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            return await GetOfficeHierarchy(officeList);
        }

        public async Task<OfficeHierarchy> GetOfficeHierarchy(IList<int> accessibleOffices)
        {
            //0 means user doesn't have access to any office data
            if (accessibleOffices != null && accessibleOffices.Contains(0))
                return new OfficeHierarchy() { Text = string.Empty, Value = string.Empty, Children = Enumerable.Empty<OfficeHierarchy>().ToList() };

            var revOffices = await _finApiClient.GetOffices("REV", "A");

            if (accessibleOffices != null && accessibleOffices.Any())
            {
                accessibleOffices = GetAccessibleOfficesAndHierarchy(revOffices, accessibleOffices.ToList(), new List<int>()).ToList();
                revOffices = revOffices.Where(x => accessibleOffices.Contains(x.OfficeCode));
            }
            var officeHierarchy = ConvertFlatLocationsToOfficeHierarchy(revOffices.ToList());
            return officeHierarchy;

        }

        public async Task<IEnumerable<RevOffice>> GetOfficeListFromFinance()
        {
            var revOffices = await _finApiClient.GetOffices(null, null);
            return revOffices.ToList();
        }

        public async Task<IEnumerable<BillRate>> GetBillRateByOffices(string officeCodes)
        {
            if (string.IsNullOrEmpty(officeCodes))
                throw new ArgumentException("Office Code(s) can not be null");

            var billRates =  await 
                _finApiClient.GetBillRateByOffices(officeCodes);
            return billRates ?? Enumerable.Empty<BillRate>();
        }
        public async Task<IEnumerable<BillRate>> GetBillRates()
        {
            var billRates = await
                _finApiClient.GetBillRates();
            return billRates ?? Enumerable.Empty<BillRate>();
        }


        #region Helper Methods
        private IList<int> GetAccessibleOfficesAndHierarchyTopDown(IEnumerable<RevOffice> offices, int regionOrClusterCode, List<int> accessibleOfficesAndHierarchy)
        {
            var accessibleOffices = new List<int>();
            if (accessibleOffices.Count() == 0)
                accessibleOffices.Add(regionOrClusterCode); //Cluster or Region code

            if (!accessibleOfficesAndHierarchy.Any())
            {
                accessibleOfficesAndHierarchy = accessibleOffices;
                var childOffices = offices
                    .Where(x => accessibleOffices.Contains(x.ParentOfficeCode))
                    .Select(x => x.OfficeCode).ToList().Distinct();
                accessibleOfficesAndHierarchy = accessibleOfficesAndHierarchy.Concat(childOffices).ToList();
                return GetAccessibleOfficesAndHierarchyTopDown(offices, regionOrClusterCode, accessibleOfficesAndHierarchy);
            }
            var accessibleChildOffices = offices
                .Where(x => accessibleOfficesAndHierarchy.Contains(x.ParentOfficeCode))
                .Select(x => x.OfficeCode).ToList().Distinct();
            var newAccessibleChildOffices = accessibleChildOffices.Except(accessibleOfficesAndHierarchy).ToList();
            if (newAccessibleChildOffices.Any())
            {
                accessibleOfficesAndHierarchy = accessibleOfficesAndHierarchy.Concat(newAccessibleChildOffices).ToList();
                return GetAccessibleOfficesAndHierarchyTopDown(offices, regionOrClusterCode, accessibleOfficesAndHierarchy);

            }

            return accessibleOfficesAndHierarchy.Distinct().ToList();
        }

        private RevOffice getOfficeClusterRegion(int officeCode, string entityTypeCode, IEnumerable<RevOffice> revOffices)
        {
            var office = revOffices.FirstOrDefault(o => o.OfficeCode == officeCode);
            if (office == null)
            {
                return null;
            }

            if (office.EntityTypeCode == entityTypeCode)
            {
                return office;
            }

            return getOfficeClusterRegion(office.ParentOfficeCode, entityTypeCode, revOffices);
        }

        private IList<int> GetAccessibleOfficesAndHierarchy(IEnumerable<RevOffice> offices, List<int> accessibleOffices,  List<int> accessibleOfficesAndHierarchy)
        {
            //var accessibleOffices = JWTHelper.GetAccessibleOffices(_httpContextAccessor.HttpContext)?.ToList();
            if (!accessibleOfficesAndHierarchy.Any())
            {
                accessibleOfficesAndHierarchy = accessibleOffices;
                var parentOffices = offices
                    .Where(x => accessibleOffices.Contains(x.OfficeCode))
                    .Select(x => x.ParentOfficeCode).ToList().Distinct();
                accessibleOfficesAndHierarchy = accessibleOfficesAndHierarchy.Concat(parentOffices).ToList();
                return GetAccessibleOfficesAndHierarchy(offices, accessibleOffices, accessibleOfficesAndHierarchy);
            }
            var accessibleParentOffices = offices
                .Where(x => accessibleOfficesAndHierarchy.Contains(x.OfficeCode))
                .Select(x => x.ParentOfficeCode).ToList().Distinct();
            var newAccessibleParentOffices = accessibleParentOffices.Except(accessibleOfficesAndHierarchy).ToList();
            if (newAccessibleParentOffices.Any())
            {
                accessibleOfficesAndHierarchy = accessibleOfficesAndHierarchy.Concat(newAccessibleParentOffices).ToList();
                return GetAccessibleOfficesAndHierarchy(offices, accessibleOffices, accessibleOfficesAndHierarchy);

            }
            return accessibleOfficesAndHierarchy.Distinct().ToList();
        }

        private OfficeHierarchy ConvertFlatLocationsToOfficeHierarchy(IList<RevOffice> flatLocations)
        {
            var lookup = flatLocations
                .Select(fl => new LocationHierarchy
                {
                    OfficeCode = fl.OfficeCode,
                    Office = new Office
                    {
                        OfficeCode = fl.OfficeCode,
                        OfficeName = fl.OfficeName,
                        OfficeAbbreviation = fl.OfficeAbbreviation
                    },
                    ParentOfficeCode = fl.ParentOfficeCode,
                    EntityTypeCode = fl.EntityTypeCode
                }).ToLookup(l => l.ParentOfficeCode);

            foreach (var location in lookup.SelectMany(x => x))
            {
                location.ChildLocations = lookup[location.OfficeCode].ToList();
            }

            var locationHierarchy = lookup[9992].ToList()[0];
            return GetOfficeHierarchy(locationHierarchy);
        }
        private OfficeHierarchy GetOfficeHierarchy(LocationHierarchy location)
        {
            return new OfficeHierarchy
            {
                Children = location.ChildLocations.Select(GetOfficeHierarchy).ToList(),
                Text = location.Office.OfficeName,
                Value = location.Office.OfficeCode.ToString()
            };
        }
        
        #endregion


    }
}