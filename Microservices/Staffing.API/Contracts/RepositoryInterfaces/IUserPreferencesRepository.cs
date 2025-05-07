using Staffing.API.Models;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IUserPreferencesRepository
    {
        Task<UserPreferences> GetUserPreferences(string employeeCode);
        Task<UserPreferences> InsertUserPreferences(UserPreferences userPreferences);
        Task<UserPreferences> UpdateUserPreferences(UserPreferences userPreferences);
        Task<UserPreferences> UpsertUserPreferences(UserPreferences userPreferences);
        Task<UserPreferences> DeleteUserPreferences(string employeeCode);
    }
}
