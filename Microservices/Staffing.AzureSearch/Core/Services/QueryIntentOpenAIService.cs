using Newtonsoft.Json;
using System.Net.Http.Headers;
using Staffing.AzureSearch.Models;
using Staffing.AzureSearch.Contracts.Services;
using Staffing.AzureSearch.Core.Helpers;
using System.Text.RegularExpressions;

namespace Staffing.AzureSearch.Core.Services
{
    public class QueryIntentOpenAIService : IQueryIntentOpenAIService
    {
        private readonly string azureOpenAIAPIKey;
        private readonly HttpClient _apiClient;
        private readonly string[] searchFields = new string[] { "employeeCode", "fullName", "positionNameWithAbbreviation", "levelGrade", "serviceLineName", "aggregatedRingfences", "officeDetail", "certificates", "aggregatedCaseExperiences","aggregatedPegCaseExperiences", "educationHistory", "employmentHistory", "prioritiesPreferences", "firstPriority", "secondPriority","thirdPriority", "travelInterest","travelRegions","travelDuration", "pdFocusAreas", "practiceAreasToAvoidStaffing", "excitedPracticeAreasForStaffing", "willingPracticeAreasForStaffing" };
        private readonly string[] filterFields = new string[] { "availabilityDates", "languages" };

        public QueryIntentOpenAIService(HttpClient httpClient)
        {
            azureOpenAIAPIKey = ConfigService.GetOpenAIApiKey();
            _apiClient = httpClient;
            ConfigureHttpClient(_apiClient, ConfigService.GetOpenAIBaseUrl());
        }

        public async Task<OpenAIGeneratedSearchQuery> GetAzureSearchQueryFromSearchText(string searchQuery)
        {
            var data = await GetQueryIntentFromOpenAI(searchQuery);
            return data;

        }

        private async Task<OpenAIGeneratedSearchQuery> GetQueryIntentFromOpenAI(string searchQuery)
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

            query = formatSearchQuery(query);
            return query;
        }

        #region Private Helpers

        private OpenAIGeneratedSearchQuery formatSearchQuery(OpenAIGeneratedSearchQuery searchQuery)
        {
            var fields = new string [] { "pdFocusAreas", "travelInterest", "firstPriority", "secondPriority", "thirdPriority" };

            // Regex pattern to match the specific field and its single-quoted value
            foreach(var fieldName in fields)
            {
                string pattern = $@"({fieldName}:\s*)'([^']*)'";

                // Replace the single quoted value with escaped double quotes
                searchQuery.search = Regex.Replace(searchQuery.search, pattern, m => $"{m.Groups[1].Value}\"{m.Groups[2].Value}\"");
            }

            if(searchQuery.search.Contains("aggregatedCaseExperiences:PEG"))
            {
                searchQuery.search = searchQuery.search.Replace("aggregatedCaseExperiences:PEG", "aggregatedRingfences:PEG"); //Sometimes AI could not understand that PEG is not a case experiece but rather a ringfence.
            }

            if (!string.IsNullOrEmpty(searchQuery.search))
            {
                searchQuery.search = searchQuery.search.Replace(" and", " AND").Replace(" or", " OR").Replace(" not", " NOT"); //NOte: use of space to avoid replace boolean opeartor in search query of fifeld name. In search we have to use CAPS boolean operators. Else we won't get results when using 'searchMode = all'
            }

            if (!string.IsNullOrEmpty(searchQuery.filter))
            {
                searchQuery.filter = searchQuery.filter.Replace(" AND", " and").Replace(" OR", " or"); ////NOte: use of space to avoid replace boolean opeartor in search query of fifeld name. In filter we have to use LOWERCASE boolean operators. Else we won't get results when using 'searchMode = all'
            }


            return searchQuery;
        }
        private async Task<OpenAIGeneratedSearchQuery> ValidateResponseFormatAndRetryIfInvalid(int retryCount, int maxRetry, OpenAIGeneratedSearchQuery query, List<PromptMessage> messages, string latestResponseFromAI)
        {
            if (retryCount > maxRetry)
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

        private OpenAIGeneratedSearchQuery ExtractAzureSearchQuery(string responseContent)
        {
            var includeTerminated = false;

            var generatedSearchQuery = new OpenAIGeneratedSearchQuery();
            try
            {
                generatedSearchQuery = JsonConvert.DeserializeObject<OpenAIGeneratedSearchQuery>(responseContent);

                // if includeTerminated: False, then add filter for activeStatus
                if (!includeTerminated)
                {
                    //if (!string.IsNullOrEmpty(generatedSearchQuery.search))
                    //{
                    //    generatedSearchQuery.search.Replace("and", "AND").Replace("or", "OR").Replace("not","NOT"); //In search we have to use CAPS boolean operators. Else we won't get results when using 'searchMode = all'
                    //}

                    if (string.IsNullOrEmpty(generatedSearchQuery.filter))
                    {
                        generatedSearchQuery.filter = "activeStatus ne 'Terminated'";
                    }
                    else
                    {
                        //generatedSearchQuery.filter.Replace("AND", "and").Replace("OR", "or"); //In filter we have to use LOWERCASE boolean operators. Else we won't get results when using 'searchMode = all'
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
            int daysUntilMonday = today.DayOfWeek != DayOfWeek.Monday ? ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7 : 7;
            var formattedMondayOfNextWeek = today.AddDays(daysUntilMonday).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var listFieldsithDesc = new IndexField[]
            {
                new IndexField( "employeeCode", "5 character alphanumeric string that stores employee code which is not a Noun.Regex pattern is ^[A-Za-z0-9]{5}$. Examples are 42aks, 42PKY,39209,37995" ),
                new IndexField( "fullName", "Case insensitive string data type that stores name of employee. Examples are john, bryne, Amy, brad wh, atul ni, nitin jain" ),
                new IndexField( "positionNameWithAbbreviation", "Case insensitive string data type that stores level name or position name of employee. Examples are, Manager, consultant, partner, design etc" ),
                new IndexField( "levelGrade", "Case insensitive alpha numeric employee designation. Examples are A1, c1, V1, M1, TT1, TG1 etc." ),
                new IndexField( "aggregatedRingfences", "The ringfence to which employee belongs. Examples are PEG,PEG-Surge etc." ),
                new IndexField( "serviceLineName", "The department to which employee belongs.Examples are General Consulting, AAG, Innovation Design, FRWD etc." ),
                new IndexField( "officeDetail", "Comma separated string of office/cluster/region name to which employee belongs. Examples are Boston, Delhi, London, Asia Pacific etc." ),
                new IndexField( "languages", "The language name and proficiency in that language" ),
                new IndexField( "certificates", "Certifications that employee has. Examples are: CFA, Facebook advertising etc." ),
                new IndexField( "availabilityDates", "The date on which employee is available.Use this field when user ask for available resources. All dates are relative to today's date." ),
                new IndexField( "aggregatedCaseExperiences", "Search key string data type that stores industry and capability the employee has past experience or affilications in." ),
                new IndexField( "aggregatedPegCaseExperiences", "Has experience data of PEG cases that employee has worked on.Example: use when query is like 'smaps with PEG experience in CP'" ),
                new IndexField( "employmentHistory", "Search key string data type that stores industry and capability the employee has past experience/expertise on before/prior to joining Bain " ),
                new IndexField( "educationHistory", "School which the employee went to and degree they achieved there and subject they majored in" )
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
               You MUSt NOT use * in any field. Exclude that field from search or filter if you are not sure about it.
               
                Task: ```
                
                Overview: Your task is to return Azure AI Search full lucene Search query in as JSON response for only 2 fields namely search and filter.The filter field is optional and search is mandatory.
                The format of output JSON looks like {searchQueryJsonFormat} in which ""search"" and ""filter"" keys should follow the following rules.
                Rule 1: 'search' parameter can only have fields from the following list {seachFieldsjson} . If you are unsure, DO NOT add anything in the 'search' parameter which is outside the specified list.
                Rule 2: 'filter' parameter can only have fields from the following list {filterFieldsSubFieldsJson}.  If you are unsure, DO NOT include any field in the 'filter' parameter outside the filter list.   
                
                Structure of Azure Search Index: I will be providing you the structure of Azure Search Index below (inside tripe hash ###). It contains list of fields with their description.
                The first key is the field name in the index. Second key is the description of what that field is supposed to do with some examples of what data it stores.
                You MUST understand this structure carefully before proceeding with the task. 

                ###
                    - `employeeCode`: 5 character alphanumeric string that stores employee code which is not a Noun. Regex pattern is ^[A-Za-z0-9]{5}$. Examples are 42aks, 42PKY, 39209, 37995
                    - `fullName`: Case insensitive string data type that stores name of employee. Examples are john, bryne, Amy, brad wh, atul ni, nitin jain
                    - `positionNameWithAbbreviation`: Case insensitive string data type that stores level name or position name of employee. Examples are, Manager, consultant, partner, design etc.
                    - `levelGrade`: Case insensitive alpha numeric employee designation. Examples are A1, c1, V1, M1, TT1, TG1 etc.
                    - `aggregatedRingfences`: The ringfence to which employee belongs. Examples are PEG, PEG-Surge etc.
                    - `serviceLineName`: The department to which employee belongs. Examples are General Consulting, AAG, Innovation Design, FRWD etc.
                    - `officeDetail`: Comma separated string of office/cluster/region name to which employee belongs. Examples are Boston, Delhi, London, Asia Pacific etc.
                    - `languages`: The language name and proficiency in that language
                    - `certificates`: Certifications that employee has. Examples are: CFA, Facebook advertising etc.
                    - `availabilityDates`: The date on which employee is available. Use this field when user ask for available resources. All dates are relative to today's date.
                    - `aggregatedCaseExperiences`: Search key string data type that stores industry and capability the employee has past experience or affiliations in.
                    - `aggregatedPegCaseExperiences`: Has experience data of PEG cases that employee has worked on. Example: use when query is like 'smaps with PEG experience in CP'
                    - `employmentHistory`: Search key string data type that stores industry and capability the employee has past experience/expertise on before/prior to joining Bain
                    - `educationHistory`: School which the employee went to and degree they achieved there and subject they majored in
                    - `prioritiesPreferences`: The first, second or third priority preferences of employee for project work. It contains data about case duration, phase of case, team structure etc.
                    - `travelPreferences`: The travel preferences of employee. It contains data about regions they can travel to, regions they cannot travel to, reason for not travelling etc.
                    - `pdFocusAreas`: The focus areas of employee for pd (professional development) purpose. It contains data about skills that would like to develop, skills they are good at etc. Eg, Modeling Analytics skills,Content Ownership Problem Cracking, Process Ownership like RDO / PMO / PMI ,Leadership  Supervising,Client Management  Communication, PEG  Due Diligence Work etc.
                    - 'excitedPracticeAreasForStaffing': The industry or capability practice areas that the employee is excited to work in. It contains data about practice areas they are excited to work in.
                    - 'willingPracticeAreasForStaffing': The industry or capability  practice areas that the employee is happy or willing to work in. It contains data about practice areas they are willing or are happy to work in.
                    - 'practiceAreasToAvoidStaffing': The industry or capability practice areas that the employee would like to avoid working in. It contains data about practice areas they would like to avoid working in, practice areas they are not willing to work in etc.

                ###

                Once the structure of AI search index and output JSON is defined, then perform the steps in sequence:
                1. Read the input given text.
                2. Create empty output JSON so it can be used in steps as output. 
                3. **proficiencyName Field:** 
                   - Include this field in output ONLY when user asks for language proficiency else exclude it.
                   - can have values only from the following list {languagesProficiencyEnumJson}.
                4. **availabilityDates Field:**
                   - this filter if present should be in the following format availabilityDates/any(a: a/date gt yyyy-MM-ddT00:00:00Z). 
                   - Today's date is {today}. Next Monday's date is {formattedMondayOfNextWeek}. When user asks for next week, then date should be from start of the week till end of next week. Similarly next month should be 1st day of month till last last day of month  
                6. **Combining Queries:**
                   - Use `AND` to combine multiple conditions, e.g., availabilityDates/any(a: a/date ge {formattedToday}) and languages/any(a: a/name eq 'English' and a/proficiencyName eq 'Fluent')
                   - Use `OR` to allow either condition, e.g., `fullName:(john) OR fullName:(amy)`.
                   - Use `NOT` to exclude conditions, e.g., `NOT aggregatedCaseExperiences:(retail)`.
                7. Use field aggregatedPegCaseExperiences ONLY when user specifically asks for PEG case experience. For everything else use aggregatedCaseExperiences field instead. For example use when, people search like PEG case experience in retail or retail PEG case experience or retail experienc
                8. When you find users searching for their work/case preference structure the query into a case priorities preference fields firstPriority, secondPriority, thirdPriority based on what user is asking for. 
                    These are exact match keyword fields. So escape and double quote this.The corresponding field should be based on the following information:
                    a. Case priorities (select short form from one of the following options):
                       - Capability of interest. Short form : 'Capability'
                       - Phase of case - join at start. Short form : 'join at start'
                       - Case duration - shorter case. Short form : 'shorter case'
                       - Case duration - longer case. Short form : 'longer case'
                       - Colocation with LT (e.g., OVP, SM/AP, Manager, local LT). Short form : 'colocation'
                       - Industry of interest. Short form : 'Industry'
                       - LT (existing relationship or reputation). Short form : 'LT relationship'
                       - A team structure with less complexity (e.g., M+4 with OVP, SVP and Expert Partner). Short form : 'team structure'
                9. When you find users searching for travel preference structure the query into a travel preference fields travelInterest, travelDuration, travelRegions. 
                    These are exact match keyword fields. So escape and double quote this. The corresponding field should be based on the following information:
                    a. for travelInterest field: Travel Willingness (select short form from one of the following options):
                       - Keen to travel and I would like to be prioritised for future travel cases. Short form : 'keen to travel'
                       - Happy to travel if required on the case I am staffed on. Short form : 'happy to travel'
                       - I would prefer not to travel but I am able and willing to if the case requires it. Short form : 'willing to'
                       - I am currently unable to travel due to one of the exemption reasons listed. Short form : 'unable to'

                    b. for travelDuration field: Travel Duration Preferences (select short form from one of the following options):
                       - Long haul (more than 4 hours). Short form : 'long haul'
                       - Short haul (less than 4 hours). Short form : 'short haul'
                       - Both long haul and short haul. Short form : 'both'

                    c. for travelRegions field: Travel Location Preferences (list the applicable locations into regions from the following):
                       - Africa
                       - APAC
                       - Americas
                       - Europe
                       - Middle East

                And finally once the structure is completed, consider the following requirements and constraints:
                   - In any step if you are not able to proceed stop generating response and do not generate non JSON response.
                   - If you are confused on which fields to use as key then search in fullName.
                ```
             Use the following examples to understand the structure of search and filter key in the output. You MUST follow the query structure exactly in the examples below. Do NOT create your own struture for fields. Answer in consistent style.
            """;

            return systemPrompt;
        }

        private string GetSystemPrompt2()
        {
            var today = DateTime.Now.Date;
            var formattedToday = today.ToString("yyyy-MM-ddTHH:mm:ssZ");
            int daysUntilMonday = today.DayOfWeek != DayOfWeek.Monday ? ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7 : 7;
            var formattedMondayOfNextWeek = today.AddDays(daysUntilMonday).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var listFieldsithDesc = new IndexField[]
            {
                new IndexField( "employeeCode", "5 character alphanumeric string that stores employee code which is not a Noun.Regex pattern is ^[A-Za-z0-9]{5}$. Examples are 42aks, 42PKY,39209,37995" ),
                new IndexField( "fullName", "Case insensitive string data type that stores name of employee. Examples are john, bryne, Amy, brad wh, atul ni, nitin jain" ),
                new IndexField( "positionNameWithAbbreviation", "Case insensitive string data type that stores level name or position name of employee. Examples are, Manager, consultant, partner, design etc" ),
                new IndexField( "levelGrade", "Case insensitive alpha numeric employee designation. Examples are A1, c1, V1, M1, TT1, TG1 etc." ),
                new IndexField( "aggregatedRingfences", "The ringfence to which employee belongs. Examples are PEG,PEG-Surge etc." ),
                new IndexField( "serviceLineName", "The department to which employee belongs.Examples are General Consulting, AAG, Innovation Design, FRWD etc." ),
                new IndexField( "officeDetail", "Comma separated string of office/cluster/region name to which employee belongs. Examples are Boston, Delhi, London, Asia Pacific etc." ),
                new IndexField( "languages", "The language name and proficiency in that language" ),
                new IndexField( "certificates", "Certifications that employee has. Examples are: CFA, Facebook advertising etc." ),
                new IndexField( "availabilityDates", "The date on which employee is available.Use this field when user ask for available resources. All dates are relative to today's date." ),
                new IndexField( "aggregatedCaseExperiences", "Search key string data type that stores industry and capability the employee has past experience or affilications in." ),
                new IndexField( "aggregatedPegCaseExperiences", "Has experience data of PEG cases that employee has worked on.Example: use when query is like 'smaps with PEG experience in CP'" ),
                new IndexField( "employmentHistory", "Search key string data type that stores industry and capability the employee has past experience/expertise on before/prior to joining Bain " ),
                new IndexField( "educationHistory", "School which the employee went to and degree they achieved there and subject they majored in" )
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
               You MUSt NOT use * in any field. Exclude that field from search or filter if you are not sure about it.
               
                Task: ```
                
                Overview: Your task is to return Azure AI Search full lucene Search query in as JSON response for only 2 fields namely search and filter.The filter field is optional and search is mandatory.
                The format of output JSON looks like {searchQueryJsonFormat} in which ""search"" and ""filter"" keys should follow the following rules.
                Rule 1: 'search' parameter can only have fields from the following list {seachFieldsjson} .Only include conditions for fields that are mentioned in the query. Omit the fields that are not in the list.
                Rule 2: 'filter' parameter can only have fields from the following list {filterFieldsSubFieldsJson}.Only include conditions for fields that are mentioned in the query. Omit the fields that are not in the list.
                Rule 3: Correct any obvious typos in location names if needed.

                Structure of Azure Search Index: I will be providing you the structure of Azure Search Index below (inside tripe hash ###). It contains list of fields with their description.
                The first key is the field name in the index. Second key is the description of what that field is supposed to do with some examples of what data it stores.
                You MUST understand this structure carefully before proceeding with the task. 

                ###
                Identify and extract the relevant fields from the query based on below fields schema: 
                    - `employeeCode`: 5 character alphanumeric string that stores employee code which is not a Noun. Regex pattern is ^[A-Za-z0-9]{5}$. Examples are 42aks, 42PKY, 39209, 37995
                    - `fullName`: Case insensitive string data type that stores name of employee. Examples are john, bryne, Amy, brad wh, atul ni, nitin jain
                    - `positionNameWithAbbreviation`: Case insensitive string data type that stores level name or position name of employee. Examples are, Manager, consultant, partner, design etc.
                    - `levelGrade`: Case insensitive alpha numeric employee designation. Examples are A1, c1, V1, M1, TT1, TG1 etc.
                    - `aggregatedRingfences`: The ringfence to which employee belongs. Examples are PEG, PEG-Surge etc.
                    - `serviceLineName`: The department to which employee belongs. Examples are General Consulting, AAG, Innovation Design, FRWD etc.
                    - `officeDetail`: Comma separated string of office/cluster/region name to which employee belongs. Examples are Boston, Delhi, London, Asia Pacific etc.
                    - `languages`: The language name and proficiency in that language
                    - `certificates`: Certifications that employee has. Examples are: CFA, Facebook advertising etc.
                    - `availabilityDates`: The date on which employee is available. Use this field when user ask for available resources. All dates are relative to today's date.
                    - `aggregatedCaseExperiences`: Search key string data type that stores industry and capability the employee has past experience or affiliations in.
                    - `aggregatedPegCaseExperiences`: Has experience data of PEG cases that employee has worked on. Example: use when query is like 'smaps with PEG experience in CP'
                    - `employmentHistory`: Search key string data type that stores industry and capability the employee has past experience/expertise on before/prior to joining Bain
                    - `educationHistory`: School which the employee went to and degree they achieved there and subject they majored in
                    - `prioritiesPreferences`: The first, second or third priority preferences of employee for project work. It contains data about case duration, phase of case, team structure etc.
                    - `travelPreferences`: The travel preferences of employee. Extract the willingness or ability to travel, including any preferred travel destinations and reason for travel.
                    - `pdFocusAreas`: The focus areas of employee for pd (professional development) purpose. It contains data about skills that would like to develop, skills they are good at etc.
                    - 'excitedPracticeAreasForStaffing': The industry or capability practice areas that the employee is excited to work in. It contains data about practice areas they are excited to work in.
                    - 'willingPracticeAreasForStaffing': The industry or capability  practice areas that the employee is happy or willing to work in. It contains data about practice areas they are willing or are happy to work in.
                    - 'practiceAreasToAvoidStaffing': The industry or capability practice areas that the employee would like to avoid working in. It contains data about practice areas they would like to avoid working in, practice areas they are not willing to work in etc.
                ###

                Once the structure of AI search index and output JSON is defined, then perform the steps in sequence:
                1. Read the input given text.
                2. Create empty output JSON so it can be used in steps as output. 
                3. **proficiencyName Field:** 
                   - Include this field in output ONLY when user asks for language proficiency else exclude it.
                   - can have values only from the following list {languagesProficiencyEnumJson}.
                4. **availabilityDates Field:**
                   - this filter if present should be in the following format availabilityDates/any(a: a/date gt yyyy-MM-ddT00:00:00Z). 
                   - Today's date is {today}. Next Monday's date is {formattedMondayOfNextWeek}. When user asks for next week, then date should be from start of the week till end of next week. Similarly next month should be 1st day of month till last last day of month  
                6. **Combining Queries:**
                   - Use `AND` to combine multiple conditions, e.g., availabilityDates/any(a: a/date ge {formattedToday}) and languages/any(a: a/name eq 'English' and a/proficiencyName eq 'Fluent')
                   - Use `OR` to allow either condition, e.g., `fullName:(john) OR fullName:(amy)`.
                   - Use `NOT` to exclude conditions, e.g., `NOT aggregatedCaseExperiences:(retail)`.
                ```

                Respond with just the Azure Cognitive Search query in JSON format. DO NOT explain the json. Just provide json object.
            """;

            return systemPrompt;
        }
        private Dictionary<string, (string, string)> GetTrainingData()
        {
            var today = DateTime.Now.Date;
            var formattedToday = today.ToString("yyyy-MM-ddTHH:mm:ssZ");

            int daysUntilMonday = today.DayOfWeek != DayOfWeek.Monday ? ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7 : 7;
            var formattedMondayOfNextWeek = today.AddDays(daysUntilMonday).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var formattedEndOfNextWeek = today.AddDays(daysUntilMonday + 6).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var trainInput1 = "Generate Azure Cognitive search query for the input text: available Managers in Boston or new york who know french and are bilingual having CFA certificate and have expertise in healthcare and startegy interlock";
            var trainOutput1 = JsonConvert.SerializeObject(new
            {
                search = "positionNameWithAbbreviation:Manager AND (officeDetail:Boston OR officeDetail:'new york')) AND certificates: CFA AND aggregatedCaseExperiences: 'healthcare x strategy'",
                filter = $@"languages/any(l: l/name eq 'French' and l/proficiencyName eq 'Native or Bilingual') and  availabilityDates/any(a: a/date eq {formattedToday})",
            });

            var trainInput2 = "Generate Azure Cognitive search query for the input text: Who has been to Boston ";
            var trainOutput2 = JsonConvert.SerializeObject(new
            {
                search = "",
                filter = "",
            });

            var trainInput3 = "Generate Azure Cognitive search query for the input text: Expert partners in chicago office and Innovation & Design service line who speaks french and have healthcare interlock strategy experience who has availability next week and have Scrum certificate ";
            var trainOutput3 = JsonConvert.SerializeObject(new
            {
                search = "positionNameWithAbbreviation:'Expert Partner' AND serviceLineName:'Innovation & Design' AND officeDetail:Chicago AND aggregatedCaseExperiences: 'healthcare x strategy' AND certificates: Scrum",
                filter = $@"availabilityDates/any(a: a/date ge {formattedMondayOfNextWeek} and a/date le  {formattedEndOfNextWeek}) AND languages/any(l: l/name eq 'French')"
            });

            var trainInput4 = "Generate Azure Cognitive search query for the input text: PEG ringfence  smaps in APAC";
            var trainOutput4 = JsonConvert.SerializeObject(new
            {
                search = "positionNameWithAbbreviation:SMAP AND aggregatedRingfences:'PEG' AND officeDetail:Asia Pacific",
                filter = ""
            });

            var trainInput5 = "Generate Azure Cognitive search query for the input text: copenhagen M1-M5 affiliated with AMS";
            var trainOutput5 = JsonConvert.SerializeObject(new
            {
                search = "(levelGrade:M1 OR levelGrade:M2 OR levelGrade:M3 OR levelGrade:M4 OR levelGrade:M5) AND officeDetail:Copenhagen AND aggregatedCaseExperiences:AMS",
                filter = ""
            });

            var trainInput6 = "Generate Azure Cognitive search query for the input text: EMEA SMAP with PEG case experience in retail";
            var trainOutput6 = JsonConvert.SerializeObject(new
            {
                search = "positionNameWithAbbreviation:SMAP AND aggregatedPegCaseExperiences:Retail AND officeDetail: EMEA",
                filter = ""
            });

            var trainInput7 = "Generate Azure Cognitive search query for the input text: Managers in PEG with retail";
            var trainOutput7 = JsonConvert.SerializeObject(new
            {
                search = "positionNameWithAbbreviation:Manager AND aggregatedCaseExperiences:Retail AND aggregatedRingfences: PEG",
                filter = ""
            });

            var trainInput8 = "Generate Azure Cognitive search query for the input text: ACs who are willing to work in retail and can travel for work to delhi and has preferences to work on short term cases";
            var trainOutput8 = JsonConvert.SerializeObject(new
            {
                search = "positionNameWithAbbreviation:AC AND (excitedPracticeAreasForStaffing:'retail' OR willingPracticeAreasForStaffing: 'retail') AND (firstPriority:\"shorter case\"~2 OR secondPriority:\"shorter case\"~2 OR thirdPriority:\"shorter case\"~2) AND (travelInterest:\"keen to travel\"^3 OR travelInterest:\"happy to travel\"^2 OR travelInterest:\"willing to\" ) AND travelRegions: 'Asia' ",
                filter = ""
            });

            var trainingData = new Dictionary<string, (string input, string output)>
            {
                { "trainInput1",(trainInput1, trainOutput1) },
                { "trainInput2",(trainInput2, trainOutput2) },
                { "trainInput3",(trainInput3, trainOutput3) },
                { "trainInput4",(trainInput4, trainOutput4) },
                { "trainInput5",(trainInput5, trainOutput5) },
                { "trainInput6",(trainInput6, trainOutput6) },
                { "trainInput7",(trainInput7, trainOutput7) },
                { "trainInput8",(trainInput8, trainOutput8) },
            };

            return trainingData;
        }
        
        #endregion

        #region POC Methods

        //        Example JSON Data:
        //                {{
        //                  ""preferences"": [
        //                    {{
        //                      ""id"": ""1"",
        //                      ""consolidatedPreferences"": ""First Priority is Case duration - shorter case.  Second Priority is Case duration - longer case.  Third Priority is Phase of case - join at start.My travel availability is I am currently unable to travel due to one of the exemption reasons listed - please specify.I can travel to following regions - APAC.I cannot travel due to NA.I'd prefer not to work in following industries : Advanced Manufacturing & Services (AMS), Energy & Natural Resources (ENR), Government/Public Sector (GOV), Social Impact (SI).  I'm happy to work in following industries : Communications & Media(CME), Consumer Products(CP), Financial Services(FS), Healthcare & Life Sciences(HC), Private Equity(Financial Investors) (PE), Retail(RET), Technology & Cloud Services(TCS).  I'd prefer not to work in following capabilities : Advanced Analytics (AA), Customer (CSM), Performance Improvement (PI).  I'm happy to work in following capabilities : Enterprise Technology(ET), Innovation & Design(I&D), M&A and Divestitures(M&A), Organization(ORG), Strategy(STRAT), Sustainability & Responsibility(SCR), Transformation & Change(TC).  "",
        //                    }
        //},
        //                    {
        //    {
        //        ""id"": ""2"",
        //                      ""consolidatedPreferences"": ""First Priority is Local LT (e.g., OVP, SM / AP, Manager).Second Priority is Team structure (e.g., M + 4 with OVP, SVP and Expert Partner).  Third Priority is Case duration - longer case.My travel availability is I would prefer not to travel but I am able and willing to if the case requires it.  I can travel to following regions -Americas.My flying time preference is that I can fly Short haul(less than 4 hours).  NA.I'm excited to work in following industries : Advanced Manufacturing & Services (AMS).  I'm happy to work in following industries : Communications & Media(CME), Energy & Natural Resources(ENR), Government / Public Sector(GOV), Private Equity(Financial Investors) (PE), Retail(RET), Social Impact(SI), Technology & Cloud Services(TCS).I'd prefer not to work in following industries : Consumer Products (CP), Financial Services (FS), Healthcare & Life Sciences (HC).  I'd prefer not to work in following capabilities : Advanced Analytics(AA), Enterprise Technology(ET), M & A and Divestitures(M&A), Sustainability & Responsibility(SCR).I'm happy to work in following capabilities : Customer (CSM), Innovation & Design (I&D), Organization (ORG), Performance Improvement (PI), Strategy (STRAT), Transformation & Change (TC).  "",
        //                    {
        //                        {
        //                            ""id"": ""3"",
        //                      ""consolidatedPreferences"": ""First Priority is Team structure (e.g., M + 4 with OVP, SVP and Expert Partner).  Second Priority is Local LT (e.g., OVP, SM / AP, Manager).Third Priority is Case duration - shorter case.My travel availability is I am currently unable to travel due to one of the exemption reasons listed -please specify.I cannot travel due to Medical commitments or situation that would make travelling for work challenging.  I'm excited to work in following industries : Advanced Manufacturing & Services (AMS), Communications & Media (CME), Consumer Products (CP), Private Equity (Financial Investors) (PE).  I'd prefer not to work in following industries : Energy & Natural Resources(ENR), Financial Services(FS), Government / Public Sector(GOV), Social Impact(SI).I'm happy to work in following industries : Healthcare & Life Sciences (HC), Retail (RET), Technology & Cloud Services (TCS).  I'd prefer not to work in following capabilities : Advanced Analytics(AA), Enterprise Technology(ET), Transformation & Change(TC).I'm happy to work in following capabilities : Customer (CSM), Innovation & Design (I&D), Strategy (STRAT).  I'm excited to work in following capabilities : M & A and Divestitures(M & A), Organization(ORG), Performance Improvement(PI), Sustainability & Responsibility(SCR).  "",
        //                    }
        //                        },
        //                  ]
        //                }
        //                }

        //            Example User Search Query:
        //                ""Find me people who prefer not to work in ENR""

        //                Example Output:
        //                [""1"", ""3""]

        //                Example User Search Query:
        //                ""Find me people who are happy to in AMS""

        //                Example Output:
        //                [""2"", ""3""]

        public async Task<string> GetCompletionsDataFromSearchResults(string searchQuery, string searchResults)
        {
            //var systemPrompt = $@"""
            //    Given a JSON with user preferences for project work, where each entry has an 'id' and 'consolidatedPreferences' data.
            //    Your task is to outputs only a list of 'id's whose 'consolidatedPreferences' satisfy the provided searchquery  , without any explanations.
            //    Please keep in mind the following things:
            //    1. Only provide what user is looking for. Example if user is looking for ids of people who prefer to work in a industry, 
            //        then DO NOT provide them ids of people who prefer not to work in that industry.
            //    2. If user is looking for people who prefer to work in industry, then add ids of people who are excited to work in that industry.
            //    3. YOu MUST go through each and every row in JSON and provide results after matching each column of row with user query.

            //    This is the json to extract the id from:
            //    {searchResults}

            //    Please provide the array of 'id's as the output without any explanations. If you don't find any 'id's that match then return empty string.
            //""";
            var systemPrompt = $@"""
                I have data on employee preferences with the following columns: id and consolidatedPreferences.
                The consolidatedPreferences column contains text information about the following
                1. Priority of projects the individual wants to work on,
                2. The industries they want to or are happy to work in. It also contains data of industries they do not want to work in.
                3. The capabilities they want to or are happy to work in. It also contains data of capabilities they do not want to work in.
                4. Their travel preferences i.e. if they can travel for project or not willing to travel for work. If willing to travel, then to which regions they can travel to. If not willing to travel, then reason for not travelling. 
                
                User will ask a question on preferences on following JSON data.
                    ``` 
                     {searchResults} 
                    ```
                Provide the array of 'id's as the output without any explanations. If you don't find any 'id's that match then return empty string.
            """;
            string searchdata = $@"""
{{
                  ""preferences"": [
                    {{
                      ""id"": ""39209"" ,
                      ""preferred_industries"": [""CME"", ""CP"",""ENR"", ""FS"",""GOV"", ""HC"",""PE"", ""RET"",""SI"", ""TCS""],
                      ""avoid_industries"": [""AMS""],
                      ""can_travel"": ""I am currently unable to travel due to one of the exemption reasons listed - please specify"",
                      ""reason_for_not_traveling"": ""test"",
                      ""preferred_travel_regions"": []
                    }},
                    {{
                      ""id"": ""04ASE"" ,
                      ""preferred_industries"": [""ENR"", ""FS"",""SI""],
                      ""avoid_industries"": [""CME"", ""CP"",""PE""],
                      ""can_travel"": ""Happy to travel if required on the case I am staffed on"",
                      ""reason_for_not_traveling"": """",
                      ""preferred_travel_regions"": [""Middle East""]
                    }},
{{
                      ""id"": ""22DAB"" ,
                      ""preferred_industries"": [""AMS"", ""FS"", ""HC"",""SI""],
                      ""avoid_industries"": [],
                      ""can_travel"": "",
                      ""reason_for_not_traveling"": """",
                      ""preferred_travel_regions"": []
                    }},
                    {{
                      ""id"": ""04DVA"" ,
                      ""preferred_industries"": [""RET""],
                      ""avoid_industries"": [""FS"", ""HC"",""GOV"", ""PE"",""SI"", ""TCS""],
                      ""can_travel"": ""Happy to travel if required on the case I am staffed on"",
                      ""reason_for_not_traveling"": """",
                      ""preferred_travel_regions"": [""regionA"", ""regionB""]
                    }},
{{
                      ""id"": ""38EWE"" ,
                      ""preferred_industries"": [""CME"",""ENR"",""GOV"",""RET"",""TCS""],
                      ""avoid_industries"": [""CP"", ""AMS"",""FS""],
                      ""can_travel"": ""I would prefer not to travel but I am able and willing to if the case requires it"",
                      ""reason_for_not_traveling"": """",
                      ""preferred_travel_regions"": [""regionA"", ""regionB""]
                    }},
                    {{
                      ""id"": ""47005"" ,
                      ""preferred_industries"": [""HC"", ""FS"",""PE"", ""RET""],
                      ""avoid_industries"": [""CP"", ""AMS"",""GOV"", ""CME""],
                      ""can_travel"": ""Keen to travel and I would like to be prioritised for future travel cases"",
                      ""reason_for_not_traveling"": """",
                      ""preferred_travel_regions"": [""regionA"", ""regionB""]
                    }},
{{
                      ""id"": ""04JMI"" ,
                      ""preferred_industries"": [""CME"", ""CP"", ""FS"", ""HC"",""PE"", ""RET"",""TCS""],
                      ""avoid_industries"": [""AMS"", ""FS"",""GOV"", ""SI""],
                      ""can_travel"": ""I am currently unable to travel due to one of the exemption reasons listed - please specify"",
                      ""reason_for_not_traveling"": ""NA"",
                      ""preferred_travel_regions"": []
                    }},
                    {{
                      ""id"": ""47005"" ,
                      ""preferred_industries"": [""HC"", ""RET"",""TCS""],
                      ""avoid_industries"": [""ENR"", ""FS"",""GOV"", ""SI""],
                      ""can_travel"": ""I am currently unable to travel due to one of the exemption reasons listed - please specify"",
                      ""reason_for_not_traveling"": ""Medical commitments or situation that would make travelling for work challenging"",
                      ""preferred_travel_regions"": [""regionA"", ""regionB""]
                    }},
{{
                      ""id"": ""67231"" ,
                      ""preferred_industries"": [""FS"",""PE"", ""SI""],
                      ""avoid_industries"": [""AMS"",""CME""],
                      ""can_travel"": ""I would prefer not to travel but I am able and willing to if the case requires it"",
                      ""reason_for_not_traveling"": """",
                      ""preferred_travel_regions"": [""regionA"", ""regionB""]
                    }},
                    {{
                      ""id"": ""37995"" ,
                      ""preferred_industries"": [""CME"", ""CP"", ""FS"",""GOV"",""PE"", ""RET"",""SI""],
                      ""avoid_industries"": [""ENR"",""HC""],
                      ""can_travel"": ""I am currently unable to travel due to one of the exemption reasons listed - please specify"",
                      ""reason_for_not_traveling"": ""test additional reason not to travel"",
                      ""preferred_travel_regions"": []
                    }},
{{
                      ""id"": ""01TDD"" ,
                      ""preferred_industries"": [""CME"", ""ENR"", ""FS"",""PE"", ""SI""],
                      ""avoid_industries"": [""HC"",""RET"", ""TCS""],
                      ""can_travel"": ""I am currently unable to travel due to one of the exemption reasons listed - please specify"",
                      ""reason_for_not_traveling"": """",
                      ""preferred_travel_regions"": []
                    }}
                    
                  ]
                }}
""";

            //Set System Context
            var messages = new List<PromptMessage>
            {
                new PromptMessage { role = "system", content = systemPrompt },
                //Add user search query
                //new PromptMessage { role = "user", content = "JSON data is inside triple brackets. <<< " + searchResults + " >>>/n" + searchQuery }
                new PromptMessage { role = "user", content =  searchQuery }
            };

            var responseContent = await GetQueryIntentFromCompletions(messages);
            return responseContent;
        }

        public async Task<string> GetCompletionsDataFromSearchResultsForNotes(string searchQuery, string searchResults)
        {
            var systemPrompt = $@"""
                You will be given a JSON that contains array of employee notes objects. The object would have 2 keys namely id and notes.
                The data contains information about employee specific notes that are used by staffing team to put them on cases.
                You would be given a search query that the user would input to filter relevant employees that matches the notes with search query.
                Your task is to output a filtered json that matches the provided searchquery  , without any explanations.
                Please keep in mind the following things:
                1. Only provide what user is looking for. Do not hallucinate and answer honestly.
                2. YOu MUST go through each and every row in JSON and provide results after matching each column of row with user query.

                This is the json to extract the data from:
                {searchResults}

                Provide the valid JSON output in the same format as input. Do not include ''',\n and any other verbose in the output. Just give the data. If you don't find any match then return empty string.
            """;
           
            //Set System Context
            var messages = new List<PromptMessage>
            {
                new PromptMessage { role = "system", content = systemPrompt },
                new PromptMessage { role = "user", content =  searchQuery }
            };

            var responseContent = await GetQueryIntentFromCompletions(messages);
            return responseContent;
        }

        #endregion

    }


}