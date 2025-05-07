using System.Collections.Generic;
using System.Linq;
using Staffing.TableauAPI.Entities;
using Staffing.TableauAPI.Models;

namespace Staffing.TableauAPI.Repositories
{
    public interface ISiteRepository
    {
        SiteEntity GetSingle(int id);
        void Add(SiteEntity item);
        void Delete(int id);
        SiteEntity Update(int id, SiteEntity item);
        IQueryable<SiteEntity> GetAll(QueryParameters queryParameters);

        ICollection<SiteEntity> GetRandomSite();
        int Count();

        bool Save();
    }
}
