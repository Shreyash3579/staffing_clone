using Staffing.Authentication.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Staffing.Authentication.Core.Helpers
{
    public static class ActiveDirectory
    {
        public static async Task<List<ADUser>> RetrieveGroupMembers(string accountName)
        {
            IList<ADUser> activeDirectoryUsers = new List<ADUser>();
            var name = ConfigurationUtility.GetValue("LDAP:name").ToString();
            var container = ConfigurationUtility.GetValue("LDAP:container").ToString();
            var ADUsers = await Task.Run(() =>
            {
                using (var context = new PrincipalContext(ContextType.Domain, name, container))
                {
                    var group = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, accountName);

                    if (group != null)
                        try
                        {
                            var users = group.GetMembers(true).Where(r => r.StructuralObjectClass == "user");
                            foreach (var p in users)
                                activeDirectoryUsers.Add(new ADUser
                                {
                                    AccountName = p.SamAccountName,
                                    ObjectClass = p.StructuralObjectClass,
                                    DisplayName = p.DisplayName,
                                    Id = string.Format("{0}_{1}", p.SamAccountName, p.StructuralObjectClass),
                                    Email = ((UserPrincipal)p).EmailAddress,
                                    FirstName = ((UserPrincipal)p).GivenName,
                                    LastName = ((UserPrincipal)p).Surname,
                                    EmployeeCode = p.SamAccountName
                                });
                        }
                        catch (COMException ex)
                        {
                            return null;
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                        finally
                        {
                            group.Dispose();
                        }
                }

                return activeDirectoryUsers.ToList();
            });

            return ADUsers;
        }
    }
}
