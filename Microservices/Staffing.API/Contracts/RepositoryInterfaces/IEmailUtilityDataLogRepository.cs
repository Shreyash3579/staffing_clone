using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IEmailUtilityDataLogRepository
    {
        Task<IEnumerable<EmailUtilityData>> UpsertEmailUtilityDataLog(DataTable emailUtilityData);
        Task<IEnumerable<EmailUtilityData>> GetEmailUtilityDataLogsByDateAndEmailType(DateTime dateOfEmail, string emailType);
    }
}
