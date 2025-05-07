using Staffing.TableauAPI.Entities;
using Staffing.TableauAPI.Repositories;
using System;
using System.Threading.Tasks;

namespace Staffing.TableauAPI.Services
{
    public class SeedDataService : ISeedDataService
    {
        public async Task Initialize(SiteDbContext context)
        {
            context.SiteItems.Add(new SiteEntity() { SiteType = 1000, Type = "Starter", Name = "Lasagne", Created = DateTime.Now });
            context.SiteItems.Add(new SiteEntity() { SiteType = 1100, Type = "Main", Name = "Hamburger", Created = DateTime.Now });
            context.SiteItems.Add(new SiteEntity() { SiteType = 1200, Type = "Dessert", Name = "Spaghetti", Created = DateTime.Now });
            context.SiteItems.Add(new SiteEntity() { SiteType = 1300, Type = "Starter", Name = "Pizza", Created = DateTime.Now });

            await context.SaveChangesAsync();
        }
    }
}
