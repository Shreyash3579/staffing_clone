using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class EmployeeStaffingPreferenceService : IEmployeeStaffingPreferenceService
    {
        private readonly IIrisApiClient _irisApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        public EmployeeStaffingPreferenceService(IIrisApiClient irisApiClient,
            IStaffingApiClient staffingApiClient)
        {
            _irisApiClient = irisApiClient;
            _staffingApiClient = staffingApiClient;
        }
        public async Task<IEnumerable<EmployeeStaffingPreferencesViewModel>> GetEmployeeStaffingPreferences(string employeeCodes)
        {
            if (string.IsNullOrEmpty(employeeCodes))
            {
                return Enumerable.Empty<EmployeeStaffingPreferencesViewModel>();
            }
            var practiceAreas = await GetIndustryCapabilityPracticeAreas();
            var industryPracticeAreas = practiceAreas.Industry;
            var capabilityPracticeAreas = practiceAreas.Capability;

            var savedEmployeeStaffingPreferences = await _staffingApiClient.GetEmployeeStaffingPreferences(employeeCodes);

            if (savedEmployeeStaffingPreferences.Count() <= 0)
            {
                return GetEmployeeStaffingPreferencesForPracticeAreas(employeeCodes,
                    industryPracticeAreas, capabilityPracticeAreas); ;
            }

            var employeeSavedStaffingPreferences = GetSavedEmployeeStaffingPreferences(industryPracticeAreas, capabilityPracticeAreas, savedEmployeeStaffingPreferences);

            return employeeSavedStaffingPreferences;

        }

        public async Task<EmployeeStaffingPreferencesViewModel> UpsertEmployeeStaffingPreferences(EmployeeStaffingPreferencesViewModel employeeStaffingPreferences)
        {
            if (employeeStaffingPreferences.StaffingPreferences.Count == 0)
            {
                await _staffingApiClient.DeleteEmployeeStaffingPreferenceByType(employeeStaffingPreferences.EmployeeCode, employeeStaffingPreferences.PreferenceType);
                return new EmployeeStaffingPreferencesViewModel();
            }
            else
            {
                var employeeStaffingPreferencesToUpsert = ConvertEmployeeStaffingPreferenceViewModelToDBO(employeeStaffingPreferences);
                var upsertedEmployeeStaffingPreferences = await _staffingApiClient.UpsertEmployeeStaffingPreferences(employeeStaffingPreferencesToUpsert);
                var upsertedStaffingPreferences = ConvertEmployeeStaffingPreferenceToViewModel(upsertedEmployeeStaffingPreferences);
                return upsertedStaffingPreferences;
            }
            
        }

        #region Private Methods

        private List<EmployeeStaffingPreferencesViewModel> GetSavedEmployeeStaffingPreferences(IEnumerable<PracticeArea> industryPracticeAreas, IEnumerable<PracticeArea> capabilityPracticeAreas, IEnumerable<EmployeeStaffingPreferences> savedEmployeeStaffingPreferences)
        {
            var staffingPreferencesForIndustry = GetSavedEmployeeStaffingPreferencesForPracticeArea(savedEmployeeStaffingPreferences,
                            industryPracticeAreas, Constants.StaffingPreferenceType.Industry);
            var staffingPreferencesForCapability = GetSavedEmployeeStaffingPreferencesForPracticeArea(savedEmployeeStaffingPreferences,
                capabilityPracticeAreas, Constants.StaffingPreferenceType.Capability);

            var employeeSavedStaffingPreferences = new List<EmployeeStaffingPreferencesViewModel>();
            employeeSavedStaffingPreferences.AddRange(staffingPreferencesForIndustry);
            employeeSavedStaffingPreferences.AddRange(staffingPreferencesForCapability);
            return employeeSavedStaffingPreferences;
        }

        private List<EmployeeStaffingPreferencesViewModel> GetSavedEmployeeStaffingPreferencesForPracticeArea(
            IEnumerable<EmployeeStaffingPreferences> savedEmployeeStaffingPreferences, 
            IEnumerable<PracticeArea> practiceAreas, string preferenceType)
        {
            var employeePreferences = new List<EmployeeStaffingPreferencesViewModel>();

            foreach (var employeeGroup in savedEmployeeStaffingPreferences.GroupBy(x => x.EmployeeCode))
            {
                var staffingPreferences = new List<StaffingPreference>();

                foreach (var staffingPreference in employeeGroup.Where(x => x.PreferenceTypeCode == preferenceType))
                {
                    staffingPreferences.Add(new StaffingPreference
                    {
                        Code = staffingPreference.StaffingPreference,
                        Name = practiceAreas.FirstOrDefault(x => x.PracticeAreaCode == staffingPreference.StaffingPreference)?.PracticeAreaName,
                        Interest = staffingPreference.Interest,
                        NoInterest = staffingPreference.NoInterest
                    });
                }

                staffingPreferences = GetStaffingPreferencesUnionToDefaultPracticeAreas(practiceAreas, staffingPreferences);

                var staffingPreferenceForPracticeArea = new EmployeeStaffingPreferencesViewModel
                {
                    EmployeeCode = employeeGroup.ToList().FirstOrDefault().EmployeeCode,
                    PreferenceType = preferenceType,
                    StaffingPreferences = staffingPreferences
                };

                employeePreferences.Add(staffingPreferenceForPracticeArea);
            }

            return employeePreferences;
        }

        private static List<StaffingPreference> GetStaffingPreferencesUnionToDefaultPracticeAreas(IEnumerable<PracticeArea> practiceAreas, List<StaffingPreference> staffingPreferences)
        {
            var practiceAreasMapToStaffingPreferenceModel = practiceAreas.Select(item => new StaffingPreference
            {
                Code = item.PracticeAreaCode,
                Name = item.PracticeAreaName                
            }).ToList();

            staffingPreferences = staffingPreferences.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            var practiceAreasNotSavesAsEmployeeStaffingPreference = practiceAreasMapToStaffingPreferenceModel
                .Except(staffingPreferences, new StaffingPreferenceComparer());
            staffingPreferences.AddRange(practiceAreasNotSavesAsEmployeeStaffingPreference);
            return staffingPreferences;
        }

        private List<EmployeeStaffingPreferencesViewModel> GetEmployeeStaffingPreferencesForPracticeAreas(string employeeCodes, IEnumerable<PracticeArea> industryPracticeAreas, IEnumerable<PracticeArea> capabilityPracticeAreas)
        {
            string[] codesArray = employeeCodes.Split(',');

            List<EmployeeStaffingPreferencesViewModel> employeeStaffingPreferences = new List<EmployeeStaffingPreferencesViewModel>();

            foreach (string employeeCode in codesArray)
            {
                EmployeeStaffingPreferencesViewModel industryPreferences = new EmployeeStaffingPreferencesViewModel
                {
                    EmployeeCode = employeeCode.Trim(), 
                    PreferenceType = Constants.StaffingPreferenceType.Industry,
                    StaffingPreferences = industryPracticeAreas.Distinct().Select(item => new StaffingPreference
                    {
                        Code = item.PracticeAreaCode,
                        Name = item.PracticeAreaName
                    }).ToList()
                };
                employeeStaffingPreferences.Add(industryPreferences);

                EmployeeStaffingPreferencesViewModel capabilityPreferences = new EmployeeStaffingPreferencesViewModel
                {
                    EmployeeCode = employeeCode.Trim(), 
                    PreferenceType = Constants.StaffingPreferenceType.Capability,
                    StaffingPreferences = capabilityPracticeAreas.Distinct().Select(item => new StaffingPreference
                    {
                        Code = item.PracticeAreaCode,
                        Name = item.PracticeAreaName
                    }).ToList()
                };

                employeeStaffingPreferences.Add(capabilityPreferences);
            }

            return employeeStaffingPreferences;

         }

        private async Task<(IEnumerable<PracticeArea> Industry, IEnumerable<PracticeArea> Capability)> GetIndustryCapabilityPracticeAreas()
        {
            var industryPracticeAreasTask = _irisApiClient.GetAllIndustryPracticeArea();
            var capabilityPracticeAreasTask = _irisApiClient.GetAllCapabilityPracticeArea();

            await Task.WhenAll(industryPracticeAreasTask, capabilityPracticeAreasTask);

            var industryPracticeAreas = industryPracticeAreasTask.Result.GroupBy(x => x.PracticeAreaCode).Select(g => g.First());
            var capabilityPracticeAreas = capabilityPracticeAreasTask.Result.GroupBy(x => x.PracticeAreaCode).Select(g => g.First());

            return (industryPracticeAreas, capabilityPracticeAreas);
        }

        private IEnumerable<EmployeeStaffingPreferences> ConvertEmployeeStaffingPreferenceViewModelToDBO(EmployeeStaffingPreferencesViewModel employeeStaffingPreferences)
        {
            var staffingPreferences = new List<EmployeeStaffingPreferences>();

            foreach (var item in employeeStaffingPreferences.StaffingPreferences
                .Select((value, index) => (value, index)))
            {
                staffingPreferences.Add(new EmployeeStaffingPreferences
                {
                    EmployeeCode = employeeStaffingPreferences.EmployeeCode,
                    PreferenceTypeCode = employeeStaffingPreferences.PreferenceType,
                    StaffingPreference = item.value.Code,
                    Priority = item.index + 1,
                    LastUpdatedBy = employeeStaffingPreferences.LastUpdatedBy,
                    Interest = item.value.Interest,
                    NoInterest = item.value.NoInterest
                });
            }

            return staffingPreferences;
        }

        private EmployeeStaffingPreferencesViewModel ConvertEmployeeStaffingPreferenceToViewModel(IEnumerable<EmployeeStaffingPreferences> employeeStaffingPreferences)
        {
            if(employeeStaffingPreferences.Count() == 0)
            {
                return new EmployeeStaffingPreferencesViewModel();
            }
            var employeeStaffingPreferencesViewModel = new EmployeeStaffingPreferencesViewModel
            {
                EmployeeCode = employeeStaffingPreferences.FirstOrDefault().EmployeeCode,
                PreferenceType = employeeStaffingPreferences.FirstOrDefault().PreferenceTypeCode,
                StaffingPreferences = employeeStaffingPreferences.Select(item => new StaffingPreference
                {
                    Code = item.StaffingPreference
                }).ToList(),
                LastUpdatedBy = employeeStaffingPreferences.FirstOrDefault().LastUpdatedBy
            };

            return employeeStaffingPreferencesViewModel;
        }

        #endregion
    }
}
