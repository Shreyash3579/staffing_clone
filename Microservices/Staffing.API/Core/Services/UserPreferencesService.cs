using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class UserPreferencesService : IUserPreferencesService
    {
        private readonly IUserPreferencesRepository _userPreferencesRepository;

        public UserPreferencesService(IUserPreferencesRepository userPreferencesRepository)
        {
            _userPreferencesRepository = userPreferencesRepository;
        }
        public async Task<UserPreferences> GetUserPreferences(string employeeCode)
        {
            return await _userPreferencesRepository.GetUserPreferences(employeeCode);
        }

        public async Task<UserPreferences> InsertUserPreferences(UserPreferences userPreferences)
        {
            return await _userPreferencesRepository.InsertUserPreferences(userPreferences);
        }

        public async Task<UserPreferences> UpdateUserPreferences(UserPreferences userPreferences)
        {
            return await _userPreferencesRepository.UpdateUserPreferences(userPreferences);
        }

        public async Task<UserPreferences> UpsertUserPreferences(UserPreferences userPreferences)
        {
            return await _userPreferencesRepository.UpsertUserPreferences(userPreferences);
        }

        public async Task<UserPreferences> DeleteUserPreferences(string employeeCode)
        {
            return await _userPreferencesRepository.DeleteUserPreferences(employeeCode);
        }
    }
}
