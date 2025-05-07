using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface ISKUCaseTermsService
    {
        Task<IEnumerable<SKUTerm>> GetSKUTermList();
        Task<IEnumerable<SKUCaseTermsViewModel>> GetSKUTermsForOpportunity(Guid pipelineId);
        Task<IEnumerable<SKUCaseTermsViewModel>> GetSKUTermsForCase(string oldCaseCode);
        Task<SKUCaseTerms> InsertSKUCaseTerms(SKUCaseTerms skuCaseTerms);
        Task<SKUCaseTerms> UpdateSKUCaseTerms(SKUCaseTerms skuCaseTerms);
        Task DeleteSKUCaseTermsById(Guid Id);
        Task<IEnumerable<SKUCaseTermsViewModel>> GetSKUTermsForCaseOrOpportunityForDuration(string oldCaseCodes, string pipelineIds, DateTime startDate, DateTime endDate);

    }
}
