using Newtonsoft.Json;

namespace SharePointOnline.API.Models
{
    public class StaffingPreference
    {
        [JsonProperty("EmployeeCode")]
        public string EmployeeCode { get; set; }
        [JsonProperty("EmployeeEmail")]
        public string EmployeeEmail { get; set; }
        //-------------------Staffing Priorities---------------------
        [JsonProperty("First_x0020_Priority")]
        public string FirstPriority { get; set; }
        
        [JsonProperty("Second_x0020_Priority")]
        public string SecondPriority { get; set; }

        [JsonProperty("Third_x0020_Priority")]
        public string ThirdPriority { get; set; }

        //-------------------Industries---------------------
        //Industry and capability are underscored to save them as values for tokenization in index

        [JsonProperty("AdvancedManufacturingServices_x0")]
        public string AdvancedManufacturingServices_3 { get; set; }

        [JsonProperty("Communications_x002c__x0020_Medi")]
        public string CommunicationsMediaEntertainment_33 { get; set; }
        
        [JsonProperty("ConsumerProducts")]
        public string ConsumerProducts_2 { get; set; }

        [JsonProperty("Energy_x0020__x0026__x0020_Natur")]
        public string EnergyNaturalResources_24 { get; set; }

        [JsonProperty("Financial_x0020_Services_x0020__")]
        public string FinancialServices_4 { get; set; }

        [JsonProperty("Government_x0020__x0026__x0020_P")]
        public string GovernmentPublicSector_21 { get; set; }

        [JsonProperty("Healthcare_x0020__x0026__x0020_L")]
        public string HealthcareLifeSciences_5 { get; set; }

        [JsonProperty("Private_x0020_Equity_x0020__x002")]
        public string PrivateEquity_15 { get; set; }

        [JsonProperty("Retail")]
        public string Retail_9 { get; set; }

        [JsonProperty("Social_x0020_Impact_x0020__x0028")]
        public string SocialImpact_26 { get; set; }

        [JsonProperty("Technology_x0020__x0026__x0020_C")]
        public string TechnologyCloudServices_6 { get; set; }

        [JsonProperty("Industry_x0020_Experience")]
        public string PreBainIndustryExperience { get; set; }

        [JsonProperty("Conglomerates")]
        public string Conglomerates_40 { get; set; }

        [JsonProperty("HigherEducationTraining")]
        public string HigherEducationTraining_25 { get; set; }

        [JsonProperty("Services")]
        public string Services_39 { get; set; }

        [JsonIgnore]
        public string IndustryPreference1 { get; set; }
        [JsonIgnore]
        public string IndustryPreference2 { get; set; }
        [JsonIgnore]
        public string IndustryPreference3 { get; set; }

        //-------------------Capabilities---------------------
        [JsonProperty("Advanced_x0020_Analytics")]
        public string AdvancedAnalytics_36 { get; set; }

        [JsonProperty("Commercial_x0020_Excellence")]
        public string Commercial_Excellence { get; set; }//Check CE, DE&I

        [JsonProperty("Customer")]
        public string Customer_19 { get; set; }

        [JsonProperty("Enterprise_x0020_Technology_x002")]
        public string EnterpriseTechnology_35 { get; set; }

        [JsonProperty("Innovation_x0020__x0026__x0020_D")]
        public string InnovationDesign_41 { get; set; }

        [JsonProperty("M_x0026_A_x0020_and_x0020_Divest")]
        public string MADivestitures_14 { get; set; }

        [JsonProperty("Organisation")]
        public string Organisation_12 { get; set; }

        [JsonProperty("Performance_x0020_Improvement_x0")]
        public string PerformanceImprovement_11 { get; set; }

        [JsonProperty("Strategy")]
        public string Strategy_10 { get; set; }

        [JsonProperty("Sustainability_x0020__x0026__x00")]
        public string SustainabilityResponsibility_37 { get; set; }

        [JsonProperty("Transformation_x0020__x0026__x00")]
        public string TransformationChange_38 { get; set; }

        [JsonProperty("Sector_x0020_Experience")]
        public string PreBainCapabilityExperience { get; set; }

        [JsonProperty("Diversity_x002c_EquityandInclusi")]
        public string DiversityEquityandInclusion_34 { get; set; }

        [JsonIgnore]
        public string CapabilityPreference1 { get; set; }
        [JsonIgnore]
        public string CapabilityPreference2 { get; set; }
        [JsonIgnore]
        public string CapabilityPreference3 { get; set; }

        //-------------------Travel---------------------
        [JsonProperty("Travel")]
        public string TravelAvailability { get; set; }

        [JsonProperty("Short_x0020_haul_x0020__x002f__x")]
        public string FlyingPreference { get; set; }

        [JsonIgnore]
        public List<string> TravelRegions { get; set; }

        [JsonProperty("Reason_x0020_unable_x0020_to_x00")]
        public string ReasonUnableToTravel { get; set; }

        [JsonProperty("Any_x0020_additional_x0020_infor")]
        public string AdditionalTravelInfo { get; set; }

        [JsonProperty("Created")]
        public DateTime Created { get; set; }
        [JsonProperty("Modified")]
        public DateTime LastUpdated { get; set; }
        [JsonIgnore]
        public string CreatedBy { get; set; }
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }


        //---------------Combined Preference----------------
        [JsonIgnore]
        public string ConsolidatedPreferences { get; set; }

        [JsonIgnore]
        public string CasePriorities { get; set; }

        [JsonIgnore]
        public string PracticeAreasToAvoidStaffing { get; set; }
        [JsonIgnore]  
        public string PracticeAreasHappyStaffing { get; set; }
        [JsonIgnore]  
        public string PracticeAreasExcitedStaffing { get; set; }
        [JsonIgnore]
        public string InterestPracticeAreasForStaffing { get; set; }

        //----------------PD-------------------------------

        [JsonIgnore]
        public List<string> PdFocusAreas { get; set; }

        [JsonProperty("Pleasedescribe")]
        public string Pleasedescribe { get; set; }

    }
}
