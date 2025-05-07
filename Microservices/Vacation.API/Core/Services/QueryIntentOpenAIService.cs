using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Vacation.API.Models;
using Vacation.API.Contracts.Services;
using Vacation.API.Core.Helpers;
using System.Linq;

namespace Vacation.API.Core.Services
{
    public class QueryIntentOpenAIService : IQueryIntentOpenAIService
    {
        private readonly string azureOpenAIAPIKey;
        private readonly HttpClient _apiClient;
        private readonly string[] searchFields = new string[] { "employeeCode", "fullName", "positionNameWithAbbreviation", "levelGrade", "serviceLineName", "aggregatedRingfences", "officeDetail", "certificates", "aggregatedCaseExperiences", "educationHistory", "employmentHistory" };
        private readonly string[] filterFields = new string[] { "availabilityDates", "languages" };

        public QueryIntentOpenAIService(HttpClient httpClient)
        {
            azureOpenAIAPIKey = ConfigService.GetOpenAIApiKey();
            _apiClient = httpClient;
            ConfigureHttpClient(_apiClient, ConfigService.GetOpenAIBaseUrl());
        }
        
        public async Task<BossSearchQuery> GetAzureSearchQueryFromSearchText(string searchQuery)
        {
            var data = await GetQueryIntentFromOpenAI(searchQuery);
            return data;

        }

        private async Task<BossSearchQuery> GetQueryIntentFromOpenAI(string searchQuery)
        {
            var systemPrompt = GetSystemPrompt();
            var trainingData = GetTrainingData();

            //Set System Context
            var messages = new List<PromptMessage>
            {
                new PromptMessage { role = "system", content = systemPrompt }
            };

            //Add few shots training to Prompt
            foreach (var trainItem in trainingData)
            {
                messages.Add(new PromptMessage { role = "user", content = trainItem.Value.Item1 });
                messages.Add(new PromptMessage { role = "assistant", content = trainItem.Value.Item2 });
            }

            //Add user search query
            messages.Add(new PromptMessage { role = "user", content = "Generate Azure Cognitive search query for the input text: " + searchQuery });

            var responseContent = await GetQueryIntentFromCompletions(messages);
            var query = ExtractAzureSearchQuery(responseContent);

            var retryCount = 0; 
            var maxRetry = 3;
            query = await ValidateResponseFormatAndRetryIfInvalid(retryCount, maxRetry, query, messages, responseContent);

            return query;
        }

        #region Private Helpers

        private async Task<BossSearchQuery> ValidateResponseFormatAndRetryIfInvalid(int retryCount, int maxRetry, BossSearchQuery query, List<PromptMessage> messages, string latestResponseFromAI)
        {
            if(retryCount > maxRetry)
            {
                return query;
            }

            var invalidSearchFields = filterFields.Where(x => query.search.Contains(x)).ToArray();
            var invalidFilterFields = searchFields.Where(x => query.filter.Contains(x)).ToArray();

            if (invalidSearchFields.Length > 0 || invalidFilterFields.Length > 0)
            {
                var errorFilterFields = string.Join(",", invalidFilterFields);
                var errorSearchFields = string.Join(",", invalidSearchFields);
                //var prompt = $@"""The last response is incorrect with {errorFilterFields} appearing in <<filter>>> key of output. They should have been in the <<<search>>> key of output  \n\n\n
                //<<<Search Fields: [{da}]>>>\r\n\r\n
                //<<<Filter Fields: [{da2}]>>>\r\n\r\n
                //Given the above list of Search Fileds and Filter Fields, Rearrange the output by strictly following the following 2 rules while generating the query
                //Rule 1: NO field from the above Filter Fields should be present in search key in output.\n
                //Rule 2: NO field from the above Search Fileds should be present in filter key in output """;
                var prompt = "";
                if (invalidSearchFields.Length > 0 && !invalidFilterFields.Any())
                {
                    prompt = $@"""The last response is incorrect with {errorSearchFields} appearing in <<<search>>> key of output. They should have been in the <<<filter>>> key of output. Be consistent with format  \n\n\n""";
                }
                else if (invalidFilterFields.Length > 0 && !invalidSearchFields.Any())
                {
                    prompt = $@"""The last response is incorrect with {errorFilterFields} appearing in <<<filter>>> key of output. They should have been in the <<<search>>> key of output. Be consistent with format  \n\n\n""";
                }
                else
                {
                    prompt = $@"""The last response is incorrect with {errorFilterFields} appearing in <<<filter>>> key of output and {errorSearchFields} appearing in <<search>>> key of output. 
                                {errorFilterFields} should have been in the <<<search>>> key of output and {errorSearchFields} should have been in the <<<filter>>> key of output \n\n\n""";
                }

                //check if query.search has fields not in searchFields or query.filter has fields not in filterFields


                messages.Add(new PromptMessage { role = "assistant", content = latestResponseFromAI });
                messages.Add(new PromptMessage { role = "user", content = prompt });

                latestResponseFromAI = await GetQueryIntentFromCompletions(messages);
                query = ExtractAzureSearchQuery(latestResponseFromAI);
                return await ValidateResponseFormatAndRetryIfInvalid(++retryCount, maxRetry, query, messages, latestResponseFromAI);

            }
            else
            {
                return query;
            }

        }

        private void ConfigureHttpClient(HttpClient client, string azureOpenAIBaseUrl)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("api-key", azureOpenAIAPIKey);
            client.BaseAddress = new Uri(azureOpenAIBaseUrl);
        }

        private async Task<string> GetQueryIntentFromCompletions(List<PromptMessage> messages)
        {
            var completionsModel = ConfigService.GetOpenAICompletionModel();
            var response = await _apiClient.PostAsJsonAsync($"openai/deployments/{completionsModel}/chat/completions?api-version=2023-05-15",
                new
                {

                    messages,
                    temperature = 0.1,
                    top_p = 0.1,
                    frequency_penalty = 0,
                    presence_penalty = 0
                }
            );

            response.EnsureSuccessStatusCode();

            var responseObject = response.Content.ReadAsStringAsync().Result;

            var data = JsonConvert.DeserializeObject<ChatCompletion>(responseObject);
            var choicesData = data.Choices?[0].Message.Content;
            return choicesData;
        }

        private BossSearchQuery ExtractAzureSearchQuery(string responseContent)
        {
            var includeTerminated = false;
            
            var generatedSearchQuery = new BossSearchQuery();
            try
            {
                generatedSearchQuery = JsonConvert.DeserializeObject<BossSearchQuery>(responseContent);

                // if includeTerminated: False, then add filter for activeStatus
                if (!includeTerminated)
                {
                    if (string.IsNullOrEmpty(generatedSearchQuery.filter))
                    {
                        generatedSearchQuery.filter = "activeStatus ne 'Terminated'";
                    }
                    else
                    {
                        generatedSearchQuery.filter.Replace("AND", "and").Replace("OR", "or");
                        generatedSearchQuery.filter = $"activeStatus ne 'Terminated' and {generatedSearchQuery.filter}";
                    }
                }
            }
            catch (Exception)
            {
                generatedSearchQuery.errorResponse = responseContent;
                generatedSearchQuery.isErrorInGeneratingSearchQuery = true;
            }
            return generatedSearchQuery;

        }

        private string GetSystemPrompt()
        {
            var today = DateTime.Now.Date;
            var formattedToday = today.ToString("yyyy-MM-ddTHH:mm:ssZ");
            
            var listFieldsithDesc = new IndexField[]
            {
                new IndexField( "employeeCode","string", "5 character alphanumeric string that stores employee code which is not a Noun.Regex pattern is ^[A-Za-z0-9]{5}$. Examples are 42aks, 42PKY,39209,37995" ),
                new IndexField( "fullName", "string","Case insensitive string data type that stores name of employee. Examples are john, bryne, Amy, brad wh, atul ni, nitin jain" ),
                new IndexField( "positionNameWithAbbreviation","string", "Case insensitive string data type that stores level or position of employee. Examples are, Manager, consultant, partner, design etc" ),
                new IndexField( "levelGrade","string", "Case insensitive string abbreviations of employee designation like A1, c1, V1 etc." ),
                new IndexField( "serviceLineName","string", "The department to which employee belongs. Examples are General Consulting, AAG, Innovation Design, FRWD etc." ),
                new IndexField( "aggregatedRingfences","string", "The cohort to which employee belongs. Examples are PEG,PEG-Surge etc." ),
                new IndexField( "officeDetail","string", "The office/cluster/region to which employee belongs. Examples are Boston, Delhi, London, Asia Pacific etc." ),
                new IndexField( "languages","array", "The language name and proficiency in that language" ),
                new IndexField( "certificates","string", "Certifications that employee has. Examples are: CFA, Facebook advertising etc." ),
                new IndexField( "availabilityDates","array", "The date on which employee is available. All dates are relative to today's date" ),
                new IndexField( "aggregatedCaseExperiences","string", "Search key string data type that stores industry and capability the employee has past experience/expertise on " ),
                new IndexField( "employmentHistory","string", "Search key string data type that stores industry and capability the employee has past experience/expertise on before/prior to joining Bain " ),
                new IndexField( "educationHistory","string", "School which the employee went to and degree they achieved there and subject they majored in" )

            };
            var listFieldsithDescJson = JsonConvert.SerializeObject(listFieldsithDesc);
            var seachFieldsjson = JsonConvert.SerializeObject(searchFields);
            //var filterFieldsJson = JsonConvert.SerializeObject(filterFields);
            var filterFieldsSubFields = new string[] { "availabilityDates/date", "languages/name", "languages/proficiencyName" };
            var filterFieldsSubFieldsJson = JsonConvert.SerializeObject(filterFieldsSubFields);
            var languagesProficiencyEnum = new string[] { "Native or Bilingual", "Full Professional Proficiency", "Limited Working Proficiency" };
            var languagesProficiencyEnumJson = JsonConvert.SerializeObject(languagesProficiencyEnum);
            var searchQueryJsonFormat = JsonConvert.SerializeObject(new
            {
                search = "<<searchText>>",
                filter = "<<filterText>>",
            });


            var systemPrompt = $@"""
               You are an expert Azure AI Search query generator. Use the step-by-step instructions (given in tripple backticks) to respond to user inputs.
               You return JSON response only for provided format and answer in a consistent style. 
               You answer truthfully and you do not answer anything else. If you are unsure of response you should return an empty JSON result without altering the structure of the JSON.
                
                Task: ```
                
                Overview: Your task is to return Azure AI Search full lucene Search query in as JSON response for only 2 fields. 
                The name of fields are search and filter.The filter field is optional and search is mandatory.
                The format of output JSON looks like {searchQueryJsonFormat} in which ""search"" and ""filter"" keys should follow the following rules.
                Rule 1: 'search' parameter can only have fields from the following list {seachFieldsjson} . If you are unsure, DO NOT add anything in the 'search' parameter which is outside the specified list.
                Rule 2: 'filter' parameter can only have fields from the following list {filterFieldsSubFieldsJson}.  If you are unsure, DO NOT include any field in the 'filter' parameter outside the filter list.   
                
                Once the structure of output JSON is defined, then perform the steps in sequence:
                1. Read the input given text.
                2. Create empty output JSON so it can be used in steps as output. 
                3. Use the {listFieldsithDescJson} to understand the structure of the azure AI search index. 
                4. Use the Description property from JSON in step 3 to get details about each FieldName. Use the examples in the description  to guide you with the query creation.
                5. proficiencyName can have values only from the following list {languagesProficiencyEnumJson}.
                6. availabilityDates filter if present should be in the following format availabilityDates/any(a: a/date gt yyyy-MM-ddT00:00:00Z). 
                7. Today's date is {today}.
                8. If multiple filters are present then syntax should be availabilityDates/any(a: a/date ge {formattedToday}) and languages/any(a: a/name eq 'English' and a/proficiencyName eq 'Fluent')
                Note: In any step if you are not able to proceed stop generating response and do not generate non JSON response.
    
                ```
             Use the following examples to understand the structure of search and filter key in the output. Answer in consistent style.
            """;

            return systemPrompt;
        }

        private Dictionary< string,(string, string)> GetTrainingData()
        {
            var today = DateTime.Now.Date;
            var formattedToday = today.ToString("yyyy-MM-ddTHH:mm:ssZ");

            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            var formattedMondayOfNextWeek = today.AddDays(daysUntilMonday).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var trainInput1 = "Generate Azure Cognitive search query for the input text: available Managers in Boston or new york who know french and are bilingual having CFA certificate and have expertise in healthcare and startegy interlock";
            var trainOutput1 = JsonConvert.SerializeObject(new
            {
                search = "(positionNameWithAbbreviation:Manager AND (officeDetail:Boston OR officeDetail:'new york')) AND certificates: CFA AND aggregatedCaseExperiences: 'healthcare x strategy')",
                filter = $@"languages/any(l: l/name eq 'French' and l/proficiencyName eq 'Native or Bilingual') and  availabilityDates/any(a: a/date eq {formattedToday})",
            });

            var trainInput2 = "Generate Azure Cognitive search query for the input text: Who has been to Boston ";
            var trainOutput2 = JsonConvert.SerializeObject(new
            {
                search = "",
                filter = "",
            });

            var trainInput3 = "Generate Azure Cognitive search query for the input text: Expert partners in chicago office and Innovation & Design service line who speaks french and have healthcare interlock strategy experience who are availble from Monday and have Scrum certificate ";
            var trainOutput3 = JsonConvert.SerializeObject(new
            {
                search = "(positionNameWithAbbreviation:'Expert Partner' AND serviceLineName:'Innovation & Design' AND officeDetail:Chicago AND aggregatedCaseExperiences: 'healthcare x strategy' AND certificates: Scrum)",
                filter = $@"availabilityDates/any(a: a/date eq {formattedMondayOfNextWeek}) AND languages/any(l: l/name eq 'French')"
            });

            var trainInput4 = "Generate Azure Cognitive search query for the input text: PEG ringfence  smaps in APAC";
            var trainOutput4 = JsonConvert.SerializeObject(new
            {
                search = "(positionNameWithAbbreviation:SMAP AND aggregatedRingfences:'PEG' AND officeDetail:Asia Pacific)",
                filter = ""
            });

            var trainingData = new Dictionary<string, (string input, string output)>
            {
                { "trainInput1",(trainInput1, trainOutput1) },
                { "trainInput2",(trainInput2, trainOutput2) },
                { "trainInput3",(trainInput3, trainOutput3) },
                { "trainInput4",(trainInput4, trainOutput4) },
            };

            return trainingData;
        }

        class IndexField
        {
            public string FieldName { get; set; }
            public string Description { get; set; }
            public string FieldType { get; set; }

            public IndexField(string fieldName, string description, string fieldType = null)
            {
                FieldName = fieldName;
                Description = description;
                FieldType = fieldType;
            }
        }
        #endregion

    }

}