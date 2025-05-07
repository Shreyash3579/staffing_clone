using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models;

public class BossSearchResult
{
    public BossSearchQuery generatedLuceneSearchQuery { get; set; }
    public List<dynamic> searches { get; set; }
}

public class BossSearchQuery
{
    public string search { get; set; }
    public string filter { get; set; }
    public bool isErrorInGeneratingSearchQuery { get; set; } = false;
    public string errorResponse { get; set; }
}

public class BossSearchCriteria
{
    public string MustHavesSearchString { get; set; }
    public string NiceToHaveSearchString { get; set; }
    public string SearchTriggeredFrom { get; set; }
    public string LoggedInUser { get; set; }
    public string EmployeeCodesToSearchIn { get; set; }
}

