using Staffing.TableauAPI.Repositories;
using System.Threading.Tasks;

namespace Staffing.TableauAPI.Services
{
    public interface ISeedDataService
    {
        Task Initialize(SiteDbContext context);
    }
}
