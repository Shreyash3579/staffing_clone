using Staffing.Authentication.Contracts.Services;
using Staffing.Authentication.Core.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.Authentication.Core.Services
{
    public class ADService : IADservice
    {
        public async Task<bool> isGroupMember(string accountName, string employeeCode)
        {
            bool isMember = false;
            foreach(var account in accountName.Split(","))
            {
                var ADUsers = await ActiveDirectory.RetrieveGroupMembers(account);
                isMember = ADUsers.Any(x => x.AccountName.Equals(employeeCode, StringComparison.InvariantCultureIgnoreCase));

                if (isMember)
                    return isMember;
                else
                    continue;
            }
            return isMember;
        }
    }
}
