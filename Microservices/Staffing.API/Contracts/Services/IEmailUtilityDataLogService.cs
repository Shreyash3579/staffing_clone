using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IEmailUtilityDataLogService
    {
        Task<IEnumerable<EmailUtilityData>> UpsertEmailUtilityDataLog(IEnumerable<EmailUtilityData> emailUtilityData);
        Task<IEnumerable<EmailUtilityData>> GetEmailUtilityDataLogsByDateAndEmailType(DateTime dateOfEmail, string emailType);
    }
}
