using System.ComponentModel;

namespace Staffing.AzureSearch.Core.Helpers
{
    public class Constants
    {
        public enum SearchSource
        {
            [Description("Search All from Home tab")]
            Home_Search_All,
            [Description("Search in Supply from Home tab")]
            Home_Search_Supply
        }
    }
}
