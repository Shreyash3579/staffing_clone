using Staffing.Coveo.API.Models;

namespace Staffing.Coveo.API.Contracts.Services
{
    public interface ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public int NumberOfResults { get; set; }
        public string FirstResult { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }
}
