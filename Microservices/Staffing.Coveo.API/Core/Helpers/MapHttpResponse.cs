using Staffing.Coveo.API.Models;
using Staffing.Coveo.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Staffing.Coveo.API.Core.Helpers
{
    public static class MapHttpResponse
    {
        public static IEnumerable<Resource> MapResourceResponse(dynamic resourceData)
        {
            var resourceList = new List<Resource>();
            foreach (var item in resourceData.Results)
            {
                resourceList.Add(item.Raw);
            }
            return resourceList;
        }

        public static ResourcesViewModel MapCommonSourceResponse(ResourceResponse commonSourceData)
        {
            if (commonSourceData == null)
            {
                return new ResourcesViewModel();
            }

            var resourceList = new List<Resource>();
            var allocations = new List<ResourceAllocation>();
            var commitments = new List<Commitment>();
            var timeOffs = new List<TimeOff>();
            var trainings = new List<Training>();
            var terminations = new List<ResourceTermination>();
            var promotions = new List<ResourcePromotion>();
            var transfers = new List<ResourceTransfer>();
            var transitions = new List<ResourceTransition>();
            var loas = new List<ResourceLoA>();

            // Resources
            var resourcesList = commonSourceData.Results.Where(x => x.Raw.Source == ConfigurationUtility.AppSettings.Coveo.ResourceSource.Name).Select(x => x.Raw).ToList();
            foreach (var resource in resourcesList)
            {
                resourceList.Add(ConvertToResourceModel(resource, commonSourceData));
            }
            resourceList = resourceList.OrderBy(p => p.ActiveStatus).ThenBy(x => x.FullName).ThenByDescending(r => PadNumbers(r.LevelGrade)).Take(50).ToList();
            
            var distinctEmployeeCodes = resourceList.Select(x => x.EmployeeCode).Distinct();

            // Allocations
            var allocationsList = commonSourceData.Results.Where(x => x.Raw.Source == ConfigurationUtility.AppSettings.Coveo.AllocationSource.Name 
                                                                    && distinctEmployeeCodes.Contains(x.Raw.EmployeeCode)
                                                                    && isDateOnOrAfterToday(x.Raw.EndDate)).Select(y => y.Raw).ToList();
            foreach (var allocation in allocationsList)
            {
                allocations.Add(ConvertToAllocationModel(allocation));
            }

            // Commitments
            var commitmentsList = commonSourceData.Results.Where(x => x.Raw.Source == ConfigurationUtility.AppSettings.Coveo.CommitmentSource.Name
                                                                        && distinctEmployeeCodes.Contains(x.Raw.EmployeeCode)
                                                                        && isDateOnOrAfterToday(x.Raw.EndDate)).Select(y => y.Raw).ToList();
            foreach (var commitment in commitmentsList)
            {
                commitments.Add(ConvertToCommitmentModel(commitment));
            }

            // TimeOffs
            var timeOffsList = commonSourceData.Results.Where(x => x.Raw.Source == ConfigurationUtility.AppSettings.Coveo.TimeOffSource.Name
                                                                        && distinctEmployeeCodes.Contains(x.Raw.EmployeeCode)
                                                                        && isDateOnOrAfterToday(x.Raw.EndDate)).Select(y => y.Raw).ToList();
            foreach (var timeOff in timeOffsList)
            {
                timeOffs.Add(ConvertToTimeOffModel(timeOff));
            }

            // Trainings
            var trainingsList = commonSourceData.Results.Where(x => x.Raw.Source == ConfigurationUtility.AppSettings.Coveo.TrainingsSource.Name
                                                                        && distinctEmployeeCodes.Contains(x.Raw.EmployeeCode)
                                                                        && isDateOnOrAfterToday(x.Raw.EndDate)).Select(y => y.Raw).ToList();
            foreach (var training in trainingsList)
            {
                trainings.Add(ConvertToTrainingModel(training));
            }

            // Pending Transactions
            var transactionsList = commonSourceData.Results.Where(x => x.Raw.Source == ConfigurationUtility.AppSettings.Coveo.TransactionsSource.Name
                                                                        && distinctEmployeeCodes.Contains(x.Raw.EmployeeCode)
                                                                        && isDateOnOrAfterToday(x.Raw.EffectiveDate)).Select(y => y.Raw).ToList();
            
            // Terminations
            var terminationsList = transactionsList.Where(x => x.BusinessProcessType == "Termination"
                                                    && x.TransactionStatus == "Successfully Completed");
            foreach (var termination in terminationsList)
            {
                terminations.Add(ConvertToResourceTerminationModel(termination));
            }

            // Promotions
            var promotionsList = transactionsList.Where(x => x.BusinessProcessName.Trim() == "Change Job"
                                                    && x.TransactionStatus == "Successfully Completed"
                                                    && x.PDGradeCurrent != x.PDGradeProposed);
            foreach (var promotion in promotionsList)
            {
                promotions.Add(ConvertToResourcePromotionModel(promotion));
            }

            // Transfers
            var transfersList = transactionsList.Where(x => x.BusinessProcessName.Trim() == "Change Job"
                                                    && x.TransactionStatus == "Successfully Completed"
                                                    && x.OperatingOfficeCodeCurrent != null
                                                    && x.OperatingOfficeCodeProposed != null
                                                    && x.OperatingOfficeCodeCurrent != x.OperatingOfficeCodeProposed);
            transfersList = transfersList
                .OrderBy(o => o.EffectiveDate)
                .ThenBy(o => o.LastModifiedDate)
                .GroupBy(x => new { x.EmployeeCode, x.EffectiveDate })
                .Select(grp => grp.First()).ToList();
            foreach (var transfer in transfersList)
            {
                transfers.Add(ConvertToResourceTransferModel(transfer));
                var employee = resourceList.Find(x => x.EmployeeCode == transfer.EmployeeCode);
                resourceList.Add(CreateResourceModelForTransferEmployee(transfer, employee));
            }

            // Transitions
            var transitionsList = commonSourceData.Results.Where(x => x.Raw.Source == ConfigurationUtility.AppSettings.Coveo.TransactionsSource.Name
                                                                    && distinctEmployeeCodes.Contains(x.Raw.EmployeeCode))
                                                                .Select(y => y.Raw)
                                                                .Where(r => (r.BusinessProcessType == "Termination"
                                                                        || r.BusinessProcessType.StartsWith("Change Organization Assignments"))
                                                                    && r.TransactionStatus == "Successfully Completed"
                                                                    && r.TransitionStartDate != null
                                                                    && r.CostCentreIdProposed.StartsWith("0300_")
                                                                    && isDateOnOrAfterToday(r.TransitionEndDate));
            foreach (var transition in transitionsList)
            {
                transitions.Add(ConvertToResourceTransitionModel(transition));
            }

            
            transitions = transitions.GroupBy(x => x.EmployeeCode).Select(g => g.FirstOrDefault()).ToList();

            // LOAs
            var loasList = commonSourceData.Results.Where(x => x.Raw.Source == ConfigurationUtility.AppSettings.Coveo.WorkdayLOATransactionSource.Name
                                                                        && distinctEmployeeCodes.Contains(x.Raw.EmployeeCode)
                                                                        && isDateOnOrAfterToday(x.Raw.EndDate)).Select(y => y.Raw).ToList();
            foreach (var loa in loasList)
            {
                loas.Add(ConvertToLOAModel(loa));
            }

            var resourceViewModel = new ResourcesViewModel()
            {
                Resources = resourceList,
                Allocations = allocations,
                Commitments = commitments,
                TimeOffs = timeOffs,
                Trainings = trainings,
                Terminations = terminations,
                Promotions = promotions,
                Transfers = transfers,
                Transitions = transitions,
                LOAs = loas
            };


            return resourceViewModel;
        }

        public static IEnumerable<Case> MapCaseResponse(CaseResponse caseData)
        {
            var caseList = new List<Case>();
            foreach (var item in caseData.Results)
            {
                item.Raw.RequestDuration = caseData.Duration;
                item.Raw.SearchUid = caseData.SearchUid;

                caseList.Add(item.Raw);
            }
            return caseList;
        }

        public static IEnumerable<Opportunity> MapOpportunityResponse(OpportunityResponse opportunityData)
        {
            var opportunityList = new List<Opportunity>();
            foreach (var item in opportunityData.Results)
            {
                item.Raw.RequestDuration = opportunityData.Duration;
                item.Raw.SearchUid = opportunityData.SearchUid;

                opportunityList.Add(item.Raw);
            }
            return opportunityList;
        }

        #region private methods
        private static string PadNumbers(string input)
        {
            return string.IsNullOrEmpty(input) ? input : Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }

        private static DateTime RemoveTimeZoneOffset(this DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Unspecified);
        }

        private static bool isDateOnOrAfterToday(DateTime? date)
        {
            return date != null && DateTime.Compare(date.Value.Date, DateTime.Today) >= 0;
        }
        private static Resource ConvertToResourceModel(CommonSourceViewModel commonSourceViewModel, ResourceResponse resourceResponse)
        {
            var resource = new Resource()
            {
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                EmployeeType = commonSourceViewModel.EmployeeType,
                FirstName = commonSourceViewModel.FamiliarFirstName ?? commonSourceViewModel.FirstName,
                LastName = commonSourceViewModel.FamiliarLastName ?? commonSourceViewModel.LastName ?? "",
                FullName = $"{commonSourceViewModel.FamiliarLastName ?? commonSourceViewModel.LastName ?? ""}, {commonSourceViewModel.FamiliarFirstName ?? commonSourceViewModel.FirstName}",
                MentorEcode = commonSourceViewModel.MentorEcode,
                MentorName = commonSourceViewModel.MentorName,
                LevelGrade = commonSourceViewModel.LevelGrade,
                LevelName = commonSourceViewModel.LevelName,
                BillCode = commonSourceViewModel.BillCode,
                Position = new Position
                {
                    PositionGroupName = commonSourceViewModel.PositionGroupName,
                    PositionName = commonSourceViewModel.PositionName,
                    PositionCode = commonSourceViewModel.PositionCode
                },
                ProfileImageUrl = commonSourceViewModel.ProfileImageUrl,
                Office = new Office
                {
                    OfficeCode = Convert.ToInt32(commonSourceViewModel.HomeOfficeCode),
                    OfficeName = commonSourceViewModel.HomeOfficeName,
                    OfficeAbbreviation = commonSourceViewModel.HomeOfficeAbbreviation
                },
                SchedulingOffice = new Office
                {
                    OfficeCode = Convert.ToInt32(commonSourceViewModel.OperatingOfficeCode),
                    OfficeName = commonSourceViewModel.OperatingOfficeName,
                    OfficeAbbreviation = commonSourceViewModel.OperatingOfficeAbbreviation
                },
                Status = commonSourceViewModel.Status,
                ActiveStatus = commonSourceViewModel.ActiveStatus,
                StartDate = commonSourceViewModel.StartDate?.RemoveTimeZoneOffset(),
                TerminationDate = commonSourceViewModel.TerminationDate?.RemoveTimeZoneOffset(),
                IsTerminated = string.Equals(commonSourceViewModel.ActiveStatus, "terminated", StringComparison.OrdinalIgnoreCase),
                InternetAddress = commonSourceViewModel.InternetAddress,
                FTE = commonSourceViewModel.FTE,
                ServiceLine = new ServiceLine
                {
                    ServiceLineCode = commonSourceViewModel.ServiceLineCode,
                    ServiceLineName = commonSourceViewModel.ServiceLineName
                },
                SysCollection = commonSourceViewModel.SysCollection,
                Source = commonSourceViewModel.Source,
                Uri = commonSourceViewModel.Uri,
                UriHash = commonSourceViewModel.UriHash,
                RequestDuration = resourceResponse.Duration,
                SearchUid = resourceResponse.SearchUid,
                Title = commonSourceViewModel.Title
            };
            return resource;
        }

        private static ResourceAllocation ConvertToAllocationModel(CommonSourceViewModel commonSourceViewModel)
        {
            var allocation = new ResourceAllocation()
            {
                Id = commonSourceViewModel.Id,
                ClientCode = commonSourceViewModel.ClientCode,
                CaseCode = commonSourceViewModel.CaseCode,
                OldCaseCode = commonSourceViewModel.OldCaseCode,
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                ServiceLineCode = commonSourceViewModel.ServiceLineCode,
                ServiceLineName = commonSourceViewModel.ServiceLineName,
                OperatingOfficeCode = commonSourceViewModel.OperatingOfficeCode,
                CurrentLevelGrade = commonSourceViewModel.CurrentLevelGrade,
                Allocation = commonSourceViewModel.Allocation,
                StartDate = commonSourceViewModel.StartDate,
                EndDate = commonSourceViewModel.EndDate,
                PipelineId = commonSourceViewModel.PipelineId,
                InvestmentCode = commonSourceViewModel.InvestmentCode,
                InvestmentName = commonSourceViewModel.InvestmentName,
                CaseRoleCode = commonSourceViewModel.CaseRoleCode,
                CaseRoleName = commonSourceViewModel.CaseRoleName,
                Notes = commonSourceViewModel.Notes,
                LastUpdated = commonSourceViewModel.LastUpdated,
                LastUpdatedBy = commonSourceViewModel.LastUpdatedBy
            };
            return allocation;
        }

        private static Commitment ConvertToCommitmentModel(CommonSourceViewModel commonSourceViewModel)
        {
            var commitment = new Commitment()
            {
                Id = commonSourceViewModel.Id,
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                CommitmentTypeCode = commonSourceViewModel.CommitmentTypeCode,
                CommitmentTypeName = commonSourceViewModel.CommitmentTypeName,
                StartDate = commonSourceViewModel.StartDate,
                EndDate = commonSourceViewModel.EndDate,
                Notes = commonSourceViewModel.Notes,
                LastUpdated = commonSourceViewModel.LastUpdated,
                LastUpdatedBy = commonSourceViewModel.LastUpdatedBy
            };
            return commitment;
        }

        private static TimeOff ConvertToTimeOffModel(CommonSourceViewModel commonSourceViewModel)
        {
            var timeOff = new TimeOff()
            {
                Id = commonSourceViewModel.Id,
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                CommitmentTypeCode = commonSourceViewModel.CommitmentTypeCode,
                CommitmentTypeName = commonSourceViewModel.CommitmentTypeName,
                StartDate = commonSourceViewModel.StartDate,
                EndDate = commonSourceViewModel.EndDate,
                Notes = commonSourceViewModel.Notes
            };
            return timeOff;
        }

        private static Training ConvertToTrainingModel(CommonSourceViewModel commonSourceViewModel)
        {
            var training = new Training()
            {
                Id = commonSourceViewModel.Id,
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                StartDate = commonSourceViewModel.StartDate,
                EndDate = commonSourceViewModel.EndDate,
                Role = commonSourceViewModel.Role,
                TrainingName = commonSourceViewModel.TrainingName
            };
            return training;
        }

        private static Transaction ConvertToTransactionModel(CommonSourceViewModel commonSourceViewModel)
        {
            var transaction = new Transaction()
            {
                Id = commonSourceViewModel.Id,
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                EmployeeStatus = commonSourceViewModel.EmployeeStatus,
                BusinessProcessEvent = commonSourceViewModel.BusinessProcessEvent,
                BusinessProcessReason = commonSourceViewModel.BusinessProcessReason,
                BusinessProcessType = commonSourceViewModel.BusinessProcessType,
                BusinessProcessName = commonSourceViewModel.BusinessProcessName,
                CompletedDate = commonSourceViewModel.CompletedDate,
                EffectiveDate = commonSourceViewModel.EffectiveDate,
                LastModifiedDate = commonSourceViewModel.LastModifiedDate,
                MostRecentCorrectionDate = commonSourceViewModel.MostRecentCorrectionDate,
                TerminationEffectiveDate = commonSourceViewModel.TerminationEffectiveDate,
                TransactionStatus = commonSourceViewModel.TransactionStatus,
                BillCodeCurrent = commonSourceViewModel.BillCodeCurrent,
                BillCodeProposed = commonSourceViewModel.BillCodeProposed,
                FteCurrent = commonSourceViewModel.FteCurrent,
                FteProposed = commonSourceViewModel.FteProposed,
                HomeOfficeCodeCurrent = commonSourceViewModel.HomeOfficeCodeCurrent,
                HomeOfficeCodeProposed = commonSourceViewModel.HomeOfficeCodeProposed,
                OperatingOfficeCodeCurrent = commonSourceViewModel.OperatingOfficeCodeCurrent,
                OperatingOfficeCodeProposed = commonSourceViewModel.OperatingOfficeCodeProposed,
                PDGradeCurrent = commonSourceViewModel.PDGradeCurrent,
                PDGradeProposed = commonSourceViewModel.PDGradeProposed,
                PositionNameCurrent = commonSourceViewModel.PositionNameCurrent,
                PositionNameProposed = commonSourceViewModel.PositionNameProposed,
                PositionGroupNameCurrent = commonSourceViewModel.PositionGroupNameCurrent,
                PositionGroupNameProposed = commonSourceViewModel.PositionGroupNameProposed,
                ServiceLineCodeCurrent = commonSourceViewModel.ServiceLineCodeCurrent,
                ServiceLineCodeProposed = commonSourceViewModel.ServiceLineCodeProposed,
                ServiceLineNameCurrent = commonSourceViewModel.ServiceLineNameCurrent,
                ServiceLineNameProposed = commonSourceViewModel.ServiceLineNameProposed,
                TransitionStartDate = commonSourceViewModel.TransitionStartDate,
                TransitionEndDate = commonSourceViewModel.TransitionEndDate,
                CostCentreIdCurrent = commonSourceViewModel.CostCentreIdCurrent,
                CostCentreIdProposed = commonSourceViewModel.CostCentreIdProposed,
                TargetLastUpdated = commonSourceViewModel.TargetLastUpdated
            };
            return transaction;
        }

        private static ResourceLoA ConvertToLOAModel(CommonSourceViewModel commonSourceViewModel)
        {
            var loa = new ResourceLoA()
            {
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                EmployeeName = commonSourceViewModel.FullName,
                StartDate = commonSourceViewModel.StartDate,
                EndDate = commonSourceViewModel.EndDate,
                Description = commonSourceViewModel.Description
            };
            return loa;
        }

        private static ResourceTermination ConvertToResourceTerminationModel(CommonSourceViewModel commonSourceViewModel)
        {
            var termination = new ResourceTermination()
            {
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                BillCode = commonSourceViewModel.BillCodeCurrent,
                FTE = commonSourceViewModel.FteCurrent,
                LevelGrade = commonSourceViewModel.PDGradeCurrent,
                OperatingOffice = new Office
                {
                    OfficeCode = Convert.ToInt32(commonSourceViewModel.OperatingOfficeCodeCurrent)
                },
                Position = new Position
                {
                    PositionName = commonSourceViewModel.PositionNameCurrent,
                    PositionGroupName = commonSourceViewModel.PositionGroupNameCurrent
                },
                ServiceLine = new ServiceLine
                {
                    ServiceLineCode = commonSourceViewModel.ServiceLineCodeCurrent,
                    ServiceLineName = commonSourceViewModel.ServiceLineNameCurrent
                },
                EndDate = commonSourceViewModel.TerminationEffectiveDate.Value.Date.RemoveTimeZoneOffset(),
                LastUpdated = commonSourceViewModel.LastModifiedDate
            };
            return termination;
        }

        private static ResourceTransfer ConvertToResourceTransferModel(CommonSourceViewModel commonSourceViewModel)
        {
            var transfer = new ResourceTransfer()
            {
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                StartDate = commonSourceViewModel.EffectiveDate?.Date.RemoveTimeZoneOffset(),
                OperatingOffice = new Office
                {
                    OfficeCode = Convert.ToInt32(commonSourceViewModel.OperatingOfficeCodeProposed)
                },
                OperatingOfficeCurrent = new Office
                {
                    OfficeCode = Convert.ToInt32(commonSourceViewModel.OperatingOfficeCodeCurrent)
                },
                OperatingOfficeProposed = new Office
                {
                    OfficeCode = Convert.ToInt32(commonSourceViewModel.OperatingOfficeCodeProposed)
                },
                FTE = commonSourceViewModel.FteProposed,
                BillCode = commonSourceViewModel.BillCodeProposed,
                Position = new Position
                {
                    PositionName = commonSourceViewModel.PositionNameCurrent,
                    PositionGroupName = commonSourceViewModel.PositionGroupNameCurrent
                },
                ServiceLine = new ServiceLine
                {
                    ServiceLineCode = commonSourceViewModel.ServiceLineCodeCurrent,
                    ServiceLineName = commonSourceViewModel.ServiceLineNameCurrent
                },
                LevelGrade = commonSourceViewModel.PDGradeProposed,
                LastUpdated = commonSourceViewModel.LastModifiedDate
            };
            return transfer;
        }

        private static ResourcePromotion ConvertToResourcePromotionModel(CommonSourceViewModel commonSourceViewModel)
        {
            var promotion = new ResourcePromotion()
            {
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                StartDate = commonSourceViewModel.EffectiveDate?.RemoveTimeZoneOffset(),
                LevelGrade = commonSourceViewModel.PDGradeProposed,
                BillCode = commonSourceViewModel.BillCodeProposed,
                FTE = commonSourceViewModel.FteProposed,
                OperatingOffice = new Office
                {
                    OfficeCode = Convert.ToInt32(commonSourceViewModel.OperatingOfficeCodeProposed ?? commonSourceViewModel.OperatingOfficeCodeCurrent)
                },
                Position = new Position
                {
                    PositionName = commonSourceViewModel.PositionNameProposed,
                    PositionGroupName = commonSourceViewModel.PositionGroupNameProposed
                },
                ServiceLine = new ServiceLine
                {
                    ServiceLineCode = commonSourceViewModel.ServiceLineCodeProposed,
                    ServiceLineName = commonSourceViewModel.ServiceLineNameProposed
                },
                LastUpdated = commonSourceViewModel.LastModifiedDate
            };
            return promotion;
        }

        private static ResourceTransition ConvertToResourceTransitionModel(CommonSourceViewModel commonSourceViewModel)
        {
            var transition = new ResourceTransition()
            {
                EmployeeCode = commonSourceViewModel.EmployeeCode,
                StartDate = commonSourceViewModel.TransitionStartDate?.RemoveTimeZoneOffset(),
                EndDate = commonSourceViewModel.TransitionEndDate?.RemoveTimeZoneOffset(),
                OperatingOffice = new Office
                {
                    OfficeCode = Convert.ToInt32(commonSourceViewModel.OperatingOfficeCodeCurrent)
                },
                FTE = commonSourceViewModel.FteCurrent,
                BillCode = commonSourceViewModel.BillCodeCurrent,
                Position = new Position
                {
                    PositionName = commonSourceViewModel.PositionNameCurrent,
                    PositionGroupName = commonSourceViewModel.PositionGroupNameCurrent
                },
                ServiceLine = new ServiceLine
                {
                    ServiceLineCode = commonSourceViewModel.ServiceLineCodeCurrent,
                    ServiceLineName = commonSourceViewModel.ServiceLineNameCurrent
                },
                LevelGrade = commonSourceViewModel.PDGradeCurrent,
                LastUpdated = commonSourceViewModel.LastModifiedDate
            };
            return transition;
        }

        private static Resource CreateResourceModelForTransferEmployee(CommonSourceViewModel commonSourceViewModel, Resource transferredResource)
        {
            var resource = new Resource()
            {
                EmployeeCode = transferredResource.EmployeeCode,
                EmployeeType = transferredResource.EmployeeType,
                FirstName = transferredResource.FirstName,
                LastName = transferredResource.LastName,
                FullName = transferredResource.FullName,
                MentorEcode = transferredResource.MentorEcode,
                MentorName = transferredResource.MentorName,
                LevelGrade = transferredResource.LevelGrade,
                LevelName = transferredResource.LevelName,
                BillCode = transferredResource.BillCode,
                Position = transferredResource.Position,
                ProfileImageUrl = transferredResource.ProfileImageUrl,
                Office = transferredResource.Office,
                SchedulingOffice = new Office
                {
                    OfficeCode = Convert.ToInt32(commonSourceViewModel.OperatingOfficeCodeProposed)
                },
                Status = transferredResource.Status,
                ActiveStatus = transferredResource.ActiveStatus,
                StartDate = transferredResource.StartDate,
                TerminationDate = transferredResource.TerminationDate,
                IsTerminated = transferredResource.IsTerminated,
                InternetAddress = transferredResource.InternetAddress,
                FTE = transferredResource.FTE,
                ServiceLine = transferredResource.ServiceLine
            };
            return resource;
        }

        #endregion
    }
}
