using Azure.Search.Documents.Models;

namespace Staffing.AzureSearch.Models
{
    public class BossSearchResult
    {
        public OpenAIGeneratedSearchQuery GeneratedLuceneSearchQuery { get; set; }
        public OpenAIGeneratedSearchQuery? GeneratedLuceneSearchQueryForNiceToHave { get; set; }
        public List<SearchResult<Resource>> Searches { get; set; }
    }

    public class OpenAIGeneratedSearchQuery
    {
        // The text to search for.
        public string search { get; set; }

        // The list of results.
        public string filter { get; set; }

        public bool isErrorInGeneratingSearchQuery { get; set; } = false;
        public string errorResponse { get; set; }
    }

    public class Resource
    {
        public string id { get; set; }
        public string employeeCode { get; set; }
        public string fullName { get; set; }
        public string levelGrade { get; set; }
        public string levelName { get; set; }
        public string fte { get; set; }
        public string serviceLineCode { get; set; }
        public string serviceLineName { get; set; }
        public string aggregatedRingfences { get; set; }
        public string staffingTag { get; set; }
        public string positionName { get; set; }
        public DateTime hireDate { get; set; }
        public DateTime startDate { get; set; }
        public DateTime? terminationDate { get; set; }
        public string departmentName { get; set; }
        public string homeOfficeName { get; set; }
        public string operatingOfficeCode { get; set; }
        public string operatingOfficeName { get; set; }
        public string operatingOfficeAbbreviation { get; set; }
        public string certificates { get; set; }
        public string clientsWorkedWith { get; set; }
        public string aggregatedCaseExperiences { get; set; }
        public IEnumerable<Language> languages { get; set; }
        public IEnumerable<Availability> availabilityDates { get; set; }
        public IEnumerable<PracticeAffiliation> practiceAffiliations { get; set; }
        //public string staffingPreferences { get; set; }
        public string notes { get; set; }
        public string positionNameWithAbbreviation { get; set; }
        public string officeDetail { get; set; }
    }

    public class ResourcePartial
    {
        public string id { get; set; }
        public string notes { get; set; }
        public List<float> notesVector { get; set; }
    }

    public class ResourceNotesPartial
    {
        public string id { get; set; }
        public string notes { get; set; }
    }

    public class Language
    {
        public string name { get; set; }
        public string proficiencyName { get; set; }
    }

    public class Availability
    {
        public DateTime date { get; set; }
        public short availabilityPercent { get; set; }
    }

    public class PracticeAffiliation
    {
        public string term { get; set; }
        public string role { get; set; }
    }

    public class BossSearchCriteria
    {
        public required string MustHavesSearchString { get; set; }
        public string? NiceToHaveSearchString { get; set; }
        public required string SearchTriggeredFrom { get; set; }
        public required string LoggedInUser { get; set; }
        public string? EmployeeCodesToSearchIn { get; set; }
    }
}
