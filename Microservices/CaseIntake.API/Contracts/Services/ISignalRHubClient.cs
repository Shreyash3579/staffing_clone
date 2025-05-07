using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CaseIntake.API.Contracts.Services
{
    public interface ISignalRHubClient
    {
        Task<string> GetUpdateOnCaseIntakeChanges(string sharedEmployeeCodes);
    }
}