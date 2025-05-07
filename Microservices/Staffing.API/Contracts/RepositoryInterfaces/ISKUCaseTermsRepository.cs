using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ISKUCaseTermsRepository
    {
        Task<IEnumerable<SKUTerm>> GetSKUTermList();
        Task<IEnumerable<SKUCaseTerms>> GetSKUTermsForOpportunity(Guid pipelineId);
        Task<IEnumerable<SKUCaseTerms>> GetSKUTermsForCase(string oldCaseCode);
        Task<SKUCaseTerms> InsertSKUCaseTerms(SKUCaseTerms skuCaseTerms);
        Task<SKUCaseTerms> UpdateSKUCaseTerms(SKUCaseTerms skuCaseTerms);
        Task DeleteSKUCaseTermsById(Guid Id);
        Task<IEnumerable<SKUCaseTerms>> GetSKUTermsForCaseOrOpportunityForDuration(string oldCaseCodes, string pipelineIds, DateTime startDate, DateTime endDate);
    }
}
