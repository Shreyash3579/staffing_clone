using System.Threading.Tasks;
using System;
using CaseIntake.API.Models;

namespace CaseIntake.API.Contracts.Services
{
    public interface IPipelineApiClient
    {
        Task<OpportunityDetails> GetOpportunityDetailsByPipelineId(Guid? pipelineId);

    }
}
