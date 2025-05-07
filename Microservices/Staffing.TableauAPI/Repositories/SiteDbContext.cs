using Microsoft.EntityFrameworkCore;
using Staffing.TableauAPI.Entities;

namespace Staffing.TableauAPI.Repositories
{
    public class SiteDbContext : DbContext
    {
        public SiteDbContext(DbContextOptions<SiteDbContext> options)
           : base(options)
        {

        }

        public DbSet<SiteEntity> SiteItems { get; set; }

    }
}
