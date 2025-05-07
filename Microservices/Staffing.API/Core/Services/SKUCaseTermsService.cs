using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class SKUCaseTermsService : ISKUCaseTermsService
    {
        private readonly ISKUCaseTermsRepository _skuTermRepository;

        public SKUCaseTermsService(ISKUCaseTermsRepository skuTermRepository)
        {
            _skuTermRepository = skuTermRepository;
        }

        public async Task<IEnumerable<SKUTerm>> GetSKUTermList()
        {
            var skuTermList = await _skuTermRepository.GetSKUTermList();
            return skuTermList;
        }

        public async Task<IEnumerable<SKUCaseTermsViewModel>> GetSKUTermsForOpportunity(Guid pipelineId)
        {
            if (pipelineId == null || pipelineId == Guid.Empty)
            {
                throw new ArgumentException("pipelineId cannot be null or empty");
            }
            var skuCaseTermsData = await _skuTermRepository.GetSKUTermsForOpportunity(pipelineId);

            var skuCaseTerms = skuCaseTermsData
                .OrderBy(r => r.EffectiveDate)
                .Select(item => new SKUCaseTermsViewModel
                {
                    Id = item.Id,
                    OldCaseCode = item.OldCaseCode,
                    PipelineId = item.PipelineId,
                    SKUTermsCodes = item.SKUTermsCodes,
                    EffectiveDate = item.EffectiveDate.ToString("dd-MMM-yyyy"),
                    LastUpdatedBy = item.LastUpdatedBy
                });
            return skuCaseTerms;
        }

        public async Task<IEnumerable<SKUCaseTermsViewModel>> GetSKUTermsForCase(string oldCaseCode)
        {
            if (string.IsNullOrEmpty(oldCaseCode))
            {
                throw new ArgumentException("oldCaseCode cannot be null or empty");
            }
            var skuCaseTermsData = await _skuTermRepository.GetSKUTermsForCase(oldCaseCode);

            var skuCaseTerms = skuCaseTermsData
                .OrderBy(r => r.EffectiveDate)
                .Select(item => new SKUCaseTermsViewModel
                {
                    Id = item.Id,
                    OldCaseCode = item.OldCaseCode,
                    PipelineId = item.PipelineId,
                    SKUTermsCodes = item.SKUTermsCodes,
                    EffectiveDate = item.EffectiveDate.ToString("dd-MMM-yyyy"),
                    LastUpdatedBy = item.LastUpdatedBy
                });

            return skuCaseTerms;
        }

        public async Task<SKUCaseTerms> InsertSKUCaseTerms(SKUCaseTerms skuCaseTerms)
        {
            if (skuCaseTerms == null)
            {
                throw new ArgumentException("skuCaseTerms cannot be null");
            }
            if (string.IsNullOrEmpty(skuCaseTerms.OldCaseCode) &&
                (skuCaseTerms.PipelineId == null || skuCaseTerms.PipelineId == Guid.Empty))
            {
                throw new ArgumentException("OldCaseCode and PipelineId both cannot be null or empty");
            }
            if (skuCaseTerms.EffectiveDate == null || skuCaseTerms.EffectiveDate == DateTime.MinValue)
            {
                throw new ArgumentException("EffectiveDate cannot be null");
            }
            if (string.IsNullOrEmpty(skuCaseTerms.LastUpdatedBy))
            {
                throw new ArgumentException("LastUpdatedBy cannot be null or empty");
            }
            return await _skuTermRepository.InsertSKUCaseTerms(skuCaseTerms);
        }

        public async Task<SKUCaseTerms> UpdateSKUCaseTerms(SKUCaseTerms skuCaseTerms)
        {
            if (skuCaseTerms == null)
            {
                throw new ArgumentException("skuCaseTerms cannot be null");
            }
            if (skuCaseTerms.Id == null || skuCaseTerms.Id == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be null or empty");
            }
            if (string.IsNullOrEmpty(skuCaseTerms.OldCaseCode) &&
                (skuCaseTerms.PipelineId == null || skuCaseTerms.PipelineId == Guid.Empty))
            {
                throw new ArgumentException("OldCaseCode and PipelineId both cannot be null or empty");
            }
            if (skuCaseTerms.EffectiveDate == null || skuCaseTerms.EffectiveDate == DateTime.MinValue)
            {
                throw new ArgumentException("EffectiveDate cannot be null");
            }
            if (string.IsNullOrEmpty(skuCaseTerms.LastUpdatedBy))
            {
                throw new ArgumentException("LastUpdatedBy cannot be null or empty");
            }
            return await _skuTermRepository.UpdateSKUCaseTerms(skuCaseTerms);
        }

        public async Task DeleteSKUCaseTermsById(Guid Id)
        {
            if (Id == null || Id == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be null or empty");
            }
            await _skuTermRepository.DeleteSKUCaseTermsById(Id);
        }

        public async Task<IEnumerable<SKUCaseTermsViewModel>> GetSKUTermsForCaseOrOpportunityForDuration(string oldCaseCodes, string pipelineIds, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds))
            {
                throw new ArgumentException("Both oldCaseCodes and pipelineIds cannot be null or empty");
            }
            if (startDate == DateTime.MinValue || startDate == null)
            {
                throw new ArgumentException("startDate cannot be null");
            }
            if (endDate == DateTime.MinValue || endDate == null)
            {
                throw new ArgumentException("endDate cannot be null");
            }
            var skuTermsData = await _skuTermRepository.GetSKUTermsForCaseOrOpportunityForDuration(oldCaseCodes, pipelineIds, startDate, endDate);
            var skuCaseTerms = skuTermsData.Select(item => new SKUCaseTermsViewModel
            {
                Id = item.Id,
                OldCaseCode = item.OldCaseCode,
                PipelineId = item.PipelineId,
                SKUTermsCodes = item.SKUTermsCodes,
                EffectiveDate = item.EffectiveDate.ToString("dd-MMM-yyyy"),
                LastUpdatedBy = item.LastUpdatedBy
            });
            return skuCaseTerms;
        }
    }
}
