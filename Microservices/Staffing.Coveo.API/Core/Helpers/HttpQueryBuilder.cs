using Staffing.Coveo.API.Models;
using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Staffing.Coveo.API.Core.Helpers
{
    /// <summary>
    /// This will generate the queries based on multiple scenarios such as appending q(query) or aq(advanced query) in the coveo search url
    /// </summary>
    public class HttpQueryBuilder
    {
        private readonly string _organizationId;
        private readonly string _coveoSearchURL;
        private readonly string _coveoAnalyticsSearchURL;
        private readonly string _coveoAnalyticsClickURL;
        private readonly string _queryKeyword;
        private readonly string _advancedQueryKeyword;

        private readonly UriBuilder _uriBuilder;
        private readonly UriBuilder _uriBuilderForAnalyticsSearch;
        private readonly UriBuilder _uriBuilderForAnalyticsClick;
        private NameValueCollection _query;
        private NameValueCollection _analyticsQueryForSearch;
        private NameValueCollection _analyticsQueryForClick;

        public HttpQueryBuilder()
        {
            // Setting up default configuration
            _organizationId = ConfigurationUtility.AppSettings.Coveo.OrganizationId;
            _coveoSearchURL = ConfigurationUtility.AppSettings.Coveo.SearchURL;
            _coveoAnalyticsSearchURL = ConfigurationUtility.AppSettings.Coveo.Analytics.Search.Url;
            _coveoAnalyticsClickURL = ConfigurationUtility.AppSettings.Coveo.Analytics.Click.Url;
            _queryKeyword = ConfigurationUtility.AppSettings.Coveo.Query;
            _advancedQueryKeyword = ConfigurationUtility.AppSettings.Coveo.AdvancedQuery;

            _uriBuilder = new UriBuilder(_coveoSearchURL);
            _uriBuilderForAnalyticsSearch = new UriBuilder(_coveoAnalyticsSearchURL);
            _uriBuilderForAnalyticsClick = new UriBuilder(_coveoAnalyticsClickURL);
            _query = HttpUtility.ParseQueryString(_uriBuilder.Query);
            _analyticsQueryForSearch = HttpUtility.ParseQueryString(_uriBuilderForAnalyticsSearch.Query);
            _analyticsQueryForClick = HttpUtility.ParseQueryString(_uriBuilderForAnalyticsClick.Query);
            _query["organizationId"] = _organizationId;
            _analyticsQueryForSearch["org"] = _organizationId;
            _analyticsQueryForClick["org"] = _organizationId;

            // This will filter the data based on the pipeline name provided
            // More discussion will be required on this
            // Ex -> pipeline - wildcards
            // This pipeline has three query parameters - 
            // wildcards - To enable the wildcards in query e.g. if we search for john* it will search for all the names starts with john
            // partialMatch - To enable the partial match on the query
            // partialMatchKeywords - right now the value is one which means if the keyword is john doe
            //                        it will search for john and doe separately on all the fields

            _query["pipeline"] = ConfigurationUtility.AppSettings.Coveo.Pipeline;
            _query["searchHub"] = ConfigurationUtility.AppSettings.Coveo.SearchHub;
        }

        public UriBuilder GetUrlWithQueryAndAdvancedQuery(string searchTerm, dynamic source, bool? test = false)
        {
            // This is applicable to a variety of fields, such as first name, last name employee code or searchable name.
            if (test.Value == true)
            {
                _query[_queryKeyword] = GenerateQueryBasedOnFieldTest(source, searchTerm);
            }
            else
                _query[_queryKeyword] = GenerateQueryBasedOnField(source, searchTerm);
            _query[_advancedQueryKeyword] = GenerateAdvancedQueryBasedOnField(source);
            UpdateQueryParameters(ref _query, source);
            _uriBuilder.Query = _query.ToString();
            return _uriBuilder;
        }

        public UriBuilder GetUrlForAnalytics(string type = "search")
        {
            switch (type.ToLower())
            {
                case "click":
                    _uriBuilderForAnalyticsClick.Query = _analyticsQueryForClick.ToString();
                    return _uriBuilderForAnalyticsClick;
                default:
                    _uriBuilderForAnalyticsSearch.Query = _analyticsQueryForSearch.ToString();
                    return _uriBuilderForAnalyticsSearch;
            }
        }

        public static string GenerateQueryBasedOnField(dynamic source, string searchTerm)
        {
            if (source == null) return string.Empty;

            var query = new StringBuilder();
            var splittedFields = source.Field.Split(",");
            var generatedQuery = new StringBuilder();
            for (int fieldCount = 0; fieldCount < splittedFields.Length; fieldCount++)
            {
                generatedQuery.Append($"@{splittedFields[fieldCount]}=\"{searchTerm}*\"");
                if (fieldCount != splittedFields.Length - 1) generatedQuery.Append(" OR ");
            }

            var inExpression = generatedQuery.ToString();

            if (source.NestedQuery != null && source.NestedQuery.IsRequired)
            {
                var inField = source.NestedQuery.InField;
                var outField = source.NestedQuery.OutField;

                var nestedQuery = $"{outField}=[[{inField}] {inExpression}]";
                query.Append(nestedQuery);
            }
            else
            {
                query.Append(inExpression);
            }

            return query.ToString();
        }

        public static string GenerateQueryBasedOnFieldTest(dynamic source, string searchTerm)
        {
            if (source == null) return string.Empty;

            var query = new StringBuilder();
            var splittedFields = source.Field.Split(",");
            var generatedQuery = new StringBuilder();
            for (int fieldCount = 0; fieldCount < splittedFields.Length; fieldCount++)
            {
                generatedQuery.Append($"@{splittedFields[fieldCount]}=\"{searchTerm}\"");
                if (fieldCount != splittedFields.Length - 1) generatedQuery.Append(" OR ");
            }
            generatedQuery.Append($" AND @source=\"{source.Name}\"");

            var inExpression = generatedQuery.ToString();

            if (source.NestedQuery != null && source.NestedQuery.IsRequired)
            {
                var inField = source.NestedQuery.InField;
                var outField = source.NestedQuery.OutField;

                var nestedQuery = $"{outField}=[[{inField}] {inExpression}]";
                query.Append(nestedQuery);
            }
            else
            {
                query.Append(inExpression);
            }

            return query.ToString();
        }

        public static string GenerateAdvancedQueryBasedOnField(dynamic source)
        {
            var splittedSources = source.Name.Split(",");

            if (source.NestedQuery != null && source.NestedQuery.IsRequired)
            {
                splittedSources = source.NestedQuery.Sources.Split(",");
            }

            var generatedQuery = new StringBuilder();

            for (int sourceCount = 0; sourceCount < splittedSources.Length; sourceCount++)
            {
                generatedQuery.Append($"@source=={splittedSources[sourceCount]}");
                if (sourceCount != splittedSources.Length - 1) generatedQuery.Append(" OR ");
            }
            return generatedQuery.ToString();
        }

        public static NameValueCollection UpdateQueryParameters(ref NameValueCollection query, dynamic source)
        {
            var defaultQueryParameters = ConfigurationUtility.AppSettings.Coveo.DefaultQueryPatameters.Split(",");
            foreach (var queyParam in defaultQueryParameters)
            {
                var sourceParam = Char.ToUpper(queyParam[0]) + queyParam.Substring(1);
                var sourceValue = source.GetType().GetProperty(sourceParam).GetValue(source);
                query[queyParam] = sourceValue.ToString();
            }
            return query;
        }
    }
}
