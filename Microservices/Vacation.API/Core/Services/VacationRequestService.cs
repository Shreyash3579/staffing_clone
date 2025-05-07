using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vacation.API.Contracts.RepositoryInterfaces;
using Vacation.API.Contracts.Services;
using Vacation.API.ViewModels;

namespace Vacation.API.Core.Services
{
    public class VacationRequestService : IVacationRequestService
    {
        private readonly IVacationRequestRepository _vacationRequestRepository;

        public VacationRequestService(IVacationRequestRepository vacationRequestRepository)
        {
            _vacationRequestRepository = vacationRequestRepository;
        }

        public async Task<IEnumerable<VacationRequestViewModel>> GetVacationRequestsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
                throw new ArgumentException("Employee Code can not be null");
            if (effectiveFromDate != null && effectiveToDate != null && effectiveFromDate > effectiveToDate)
                throw new ArgumentException("EffectiveToDate should be greater than EffectiveFromDate");

            var vactionRequests = new List<VacationRequestViewModel>();

            var vactionRequestData = await
                _vacationRequestRepository.GetVacationRequestsByEmployee(employeeCode, effectiveFromDate, effectiveToDate);

            vactionRequests = vactionRequestData.Select(item => new VacationRequestViewModel
            {
                Id = item.Id,
                EmployeeCode = item.EmployeeCode,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Status = item.Status,
                Description = item.Description,
                Type = "Vacation"

            }).ToList();

            vactionRequests = MergeConsecutiveVacations(vactionRequests).ToList();

            return vactionRequests ?? Enumerable.Empty<VacationRequestViewModel>();
        }

        public async Task<IEnumerable<VacationRequestViewModel>> GetVacationRequests(DateTime? lastPolledDateTime)
        {
            var vactionRequests = new List<VacationRequestViewModel>();

            var vactionRequestData = await
                _vacationRequestRepository.GetVacationRequests(lastPolledDateTime);

            vactionRequests = vactionRequestData.Select(item => new VacationRequestViewModel
            {
                Id = item.Id,
                EmployeeCode = item.EmployeeCode,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Status = item.Status,
                Description = item.Description,
                Type = "Vacation",
                LastUpdated = item.LastUpdated,
                LastUpdatedBy = item.LastUpdatedBy,
                ReplicationServer = item.ReplicationServer

            }).ToList();

            return vactionRequests ?? Enumerable.Empty<VacationRequestViewModel>();
        }

        public async Task<IEnumerable<VacationRequestViewModel>> GetVacationsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<VacationRequestViewModel>();

            var vactionRequestData = await
                _vacationRequestRepository.GetVacationsWithinDateRangeByEmployeeCodes(employeeCodes, startDate, endDate);

            var vactionRequests = vactionRequestData.Select(item => new VacationRequestViewModel
            {
                Id = item.Id,
                EmployeeCode = item.EmployeeCode,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Status = item.Status,
                Description = item.Description,
                Type = "Vacation"

            }).OrderBy(r => r.StartDate).ToList();

            vactionRequests = MergeConsecutiveVacations(vactionRequests).ToList();

            return vactionRequests ?? Enumerable.Empty<VacationRequestViewModel>();
        }

        private IList<VacationRequestViewModel> MergeConsecutiveVacations(List<VacationRequestViewModel> employeesTimeOffsVm)
        {
            var resourcesTimeOffsMerged = new List<VacationRequestViewModel>();
            foreach (var group in employeesTimeOffsVm.GroupBy(g => new { g.EmployeeCode, g.Status }))
            {
                var resourceTimeOffs = group.ToList().OrderBy(o => o.StartDate).ToArray();
                var resourceTimeOff = resourceTimeOffs.FirstOrDefault();
                var endDate = resourceTimeOff.EndDate;
                for (var index = 1; index < resourceTimeOffs.Count(); index++)
                {
                    endDate = resourceTimeOffs[index - 1].EndDate;
                    if (IncludeWeekendsInTimeOffs(resourceTimeOffs, index))
                    {
                        continue;
                    }
                    resourceTimeOff.EndDate = endDate;
                    resourcesTimeOffsMerged.Add(resourceTimeOff.ShallowCopy());
                    resourceTimeOff.StartDate = resourceTimeOffs[index].StartDate;
                    resourceTimeOff.Description = resourceTimeOffs[index].Description;


                }
                resourceTimeOff.EndDate = resourceTimeOffs.LastOrDefault().EndDate;
                resourcesTimeOffsMerged.Add(resourceTimeOff.ShallowCopy());
            }

            return resourcesTimeOffsMerged;
        }

        private bool IncludeWeekendsInTimeOffs(VacationRequestViewModel[] resourceTimeOffs, int index)
        {
            if ((resourceTimeOffs[index].StartDate - resourceTimeOffs[index - 1].EndDate).TotalDays == 1)
                return true;

            if ((resourceTimeOffs[index].StartDate - resourceTimeOffs[index - 1].EndDate).TotalDays == 2)
            {
                return resourceTimeOffs[index - 1].EndDate.AddDays(1).DayOfWeek == DayOfWeek.Saturday
                    || resourceTimeOffs[index - 1].EndDate.AddDays(1).DayOfWeek == DayOfWeek.Sunday;
            }
            if ((resourceTimeOffs[index].StartDate - resourceTimeOffs[index - 1].EndDate).TotalDays == 3)
            {
                return resourceTimeOffs[index - 1].EndDate.AddDays(1).DayOfWeek == DayOfWeek.Saturday
                    || resourceTimeOffs[index - 1].EndDate.AddDays(2).DayOfWeek == DayOfWeek.Sunday;
            }
            return false;

        }
    }
}
