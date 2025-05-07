using Newtonsoft.Json;
using SharePointOnline.API.Models;

namespace SharePointOnline.API.Core.Helpers
{
    public static class Constants
    {
        public static class MissionStatus
        {
            public const string SignedOff = "Signed-off";
            public const string PendingOfficeSignOff = "Pending office sign-off";
        }
        //TODO: Remove this once we are able to connect to basis API client for getting this data
        private static string industryJsonData = @"[
        {
            ""practiceAreaCode"": ""3"",
            ""practiceAreaName"": ""Advanced Manufacturing & Services"",
            ""practiceAreaAbbreviation"": ""AMS""
        },
        {
            ""practiceAreaCode"": ""33"",
            ""practiceAreaName"": ""Communications & Media"",
            ""practiceAreaAbbreviation"": ""CME""
        },
        {
            ""practiceAreaCode"": ""40"",
            ""practiceAreaName"": ""Conglomerates"",
            ""practiceAreaAbbreviation"": ""CGL""
        },
        {
            ""practiceAreaCode"": ""2"",
            ""practiceAreaName"": ""Consumer Products"",
            ""practiceAreaAbbreviation"": ""CP""
        },
        {
            ""practiceAreaCode"": ""24"",
            ""practiceAreaName"": ""Energy & Natural Resources"",
            ""practiceAreaAbbreviation"": ""ENR""
        },
        {
            ""practiceAreaCode"": ""4"",
            ""practiceAreaName"": ""Financial Services"",
            ""practiceAreaAbbreviation"": ""FS""
        },
        {
            ""practiceAreaCode"": ""21"",
            ""practiceAreaName"": ""Government/Public Sector"",
            ""practiceAreaAbbreviation"": ""GOV""
        },
        {
            ""practiceAreaCode"": ""5"",
            ""practiceAreaName"": ""Healthcare & Life Sciences"",
            ""practiceAreaAbbreviation"": ""HC""
        },
        {
            ""practiceAreaCode"": ""25"",
            ""practiceAreaName"": ""Higher Education & Training"",
            ""practiceAreaAbbreviation"": ""EDUC""
        },
        {
            ""practiceAreaCode"": ""15"",
            ""practiceAreaName"": ""Private Equity (Financial Investors)"",
            ""practiceAreaAbbreviation"": ""PE""
        },
        {
            ""practiceAreaCode"": ""9"",
            ""practiceAreaName"": ""Retail"",
            ""practiceAreaAbbreviation"": ""RET""
        },
        {
            ""practiceAreaCode"": ""39"",
            ""practiceAreaName"": ""Services"",
            ""practiceAreaAbbreviation"": ""SVC""
        },
        {
            ""practiceAreaCode"": ""26"",
            ""practiceAreaName"": ""Social Impact"",
            ""practiceAreaAbbreviation"": ""SI""
        },
        {
            ""practiceAreaCode"": ""6"",
            ""practiceAreaName"": ""Technology & Cloud Services"",
            ""practiceAreaAbbreviation"": ""TCS""
        }]";

        private static string capabilityJsonData = @"[
        {
            ""practiceAreaCode"": ""36"",
            ""practiceAreaName"": ""Advanced Analytics"",
            ""practiceAreaAbbreviation"": ""AA""
        },
        {
            ""practiceAreaCode"": ""19"",
            ""practiceAreaName"": ""Customer"",
            ""practiceAreaAbbreviation"": ""CSM""
        },
        {
            ""practiceAreaCode"": ""34"",
            ""practiceAreaName"": ""Diversity, Equity, and Inclusion"",
            ""practiceAreaAbbreviation"": ""DEI""
        },
        {
            ""practiceAreaCode"": ""35"",
            ""practiceAreaName"": ""Enterprise Technology"",
            ""practiceAreaAbbreviation"": ""ET""
        },
        {
            ""practiceAreaCode"": ""41"",
            ""practiceAreaName"": ""Innovation & Design"",
            ""practiceAreaAbbreviation"": ""I&D""
        },
        {
            ""practiceAreaCode"": ""14"",
            ""practiceAreaName"": ""M&A and Divestitures"",
            ""practiceAreaAbbreviation"": ""M&A""
        },
        {
            ""practiceAreaCode"": ""12"",
            ""practiceAreaName"": ""Organization"",
            ""practiceAreaAbbreviation"": ""ORG""
        },
        {
            ""practiceAreaCode"": ""11"",
            ""practiceAreaName"": ""Performance Improvement"",
            ""practiceAreaAbbreviation"": ""PI""
        },
        {
            ""practiceAreaCode"": ""23"",
            ""practiceAreaName"": ""Private Equity (Financial Investor Transactions)"",
            ""practiceAreaAbbreviation"": null
        },
        {
            ""practiceAreaCode"": ""10"",
            ""practiceAreaName"": ""Strategy"",
            ""practiceAreaAbbreviation"": ""STRAT""
        },
        {
            ""practiceAreaCode"": ""37"",
            ""practiceAreaName"": ""Sustainability & Responsibility"",
            ""practiceAreaAbbreviation"": ""SCR""
        },
        {
            ""practiceAreaCode"": ""38"",
            ""practiceAreaName"": ""Transformation & Change"",
            ""practiceAreaAbbreviation"": ""TC""
        }
    ]";

        public static List<PracticeArea> INDUSTRY_PRACTICE_AREAS = JsonConvert.DeserializeObject<List<PracticeArea>>(industryJsonData);
        public static List<PracticeArea> CAPABILITY_PRACTICE_AREAS = JsonConvert.DeserializeObject<List<PracticeArea>>(capabilityJsonData);

    }
}
