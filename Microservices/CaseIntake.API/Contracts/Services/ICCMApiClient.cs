using CaseIntake.API.Models;
using System.Threading.Tasks;

namespace CaseIntake.API.Contracts.Services
{
    public interface ICCMApiClient
    {
        //Task<IList<CaseData>> GetCaseDataBasicByCaseCodes(string oldCaseCodeList);
        Task<CaseDetails> GetCaseDetailsByCaseCode(string oldCaseCode);

    }
}
