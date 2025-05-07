using Staffing.TableauAPI.Entities;
using Staffing.TableauAPI.Helpers;
using Staffing.TableauAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Staffing.TableauAPI.Repositories
{
    public class SiteSqlRepository : ISiteRepository
    {
        private readonly SiteDbContext _SiteDbContext;

        public SiteSqlRepository(SiteDbContext SiteDbContext)
        {
            _SiteDbContext = SiteDbContext;
        }

        public SiteEntity GetSingle(int id)
        {
            return _SiteDbContext.SiteItems.FirstOrDefault(x => x.Id == id);
        }

        public void Add(SiteEntity item)
        {
            _SiteDbContext.SiteItems.Add(item);
        }

        public void Delete(int id)
        {
            SiteEntity SiteItem = GetSingle(id);
            _SiteDbContext.SiteItems.Remove(SiteItem);
        }

        public SiteEntity Update(int id, SiteEntity item)
        {
            _SiteDbContext.SiteItems.Update(item);
            return item;
        }

        public IQueryable<SiteEntity> GetAll(QueryParameters queryParameters)
        {
            IQueryable<SiteEntity> _allItems = _SiteDbContext.SiteItems.OrderBy(queryParameters.OrderBy,
              queryParameters.IsDescending());

            if (queryParameters.HasQuery())
            {
                _allItems = _allItems
                    .Where(x => x.SiteType.ToString().Contains(queryParameters.Query.ToLowerInvariant())
                    || x.Name.ToLowerInvariant().Contains(queryParameters.Query.ToLowerInvariant()));
            }

            return _allItems
                .Skip(queryParameters.PageCount * (queryParameters.Page - 1))
                .Take(queryParameters.PageCount);
        }

        public int Count()
        {
            return _SiteDbContext.SiteItems.Count();
        }

        public bool Save()
        {
            return (_SiteDbContext.SaveChanges() >= 0);
        }

        public ICollection<SiteEntity> GetRandomSite()
        {
            List<SiteEntity> toReturn = new List<SiteEntity>();

            toReturn.Add(GetRandomItem("Starter"));
            toReturn.Add(GetRandomItem("Main"));
            toReturn.Add(GetRandomItem("Dessert"));

            return toReturn;
        }

        private SiteEntity GetRandomItem(string type)
        {
            return _SiteDbContext.SiteItems
                .Where(x => x.Type == type)
                .OrderBy(o => Guid.NewGuid())
                .FirstOrDefault();
        }
    }
}
