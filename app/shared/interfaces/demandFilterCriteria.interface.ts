import { SupplyFilterCriteria } from "./supplyFilterCriteria.interface";

export interface DemandFilterCriteria {
  startDate: string;                          // dates between
  endDate: string;                            // which cases/opportunities will be fetched
  officeCodes: string;                        // comma seperated list of office(s) codes
  clientCodes: string;                        // comma seperated list of client codes
  caseTypeCodes: string;                      // comma seperated list of case type(s) codes - billable or CD etc
  caseAttributeNames: string;                 // comma seperated list of case attribute(s) codes - PEG, ADAPT, FRWD etc
  opportunityStatusTypeCodes: string;         // comma seperated list of opportunity status type(s) codes - Path to Proposal, Outcome -Won etc
  minOpportunityProbability: number;          // property required for filtering the opportunities with probability percent >= the value of this property
  demandTypes: string;                        // comma seperated list of demand type(s) - Opportunity, New Demand etc
  caseExceptionShowList: string;              // list of additional case codes to be shown to user based on user's preferences
  caseExceptionHideList: string;              // list of case codes to be hidden from user based on user's preferences
  opportunityExceptionShowList: string;       // list of additional opportunities to be shown to user based on user's preferences
  opportunityExceptionHideList: string;       // list of opportunities to be hidden from user based on user's preferences
  projectStartIndex: number;                  // property required for pagination - start index to get next set of results
  pageSize: number;                           // property required for pagination - number of records to fetch
  caseAllocationsSortBy: string;              // property to sort allocations in case cards
  supplyFilterCriteria: SupplyFilterCriteria; // holds the supply filter criteria for fetching "cases staffed by supply" when corrsponding option in demand type filter is selected
  caseOppSortOrder: string;
  planningCardsSortOrder: string;
  industryPracticeAreaCodes: string           // comma seperated list of industry practice areas codes
  capabilityPracticeAreaCodes: string         // comma seperated list of capability practice areas codes
  isStaffedFromSupply: boolean;               // property required for filtering the projects with the "staff from my supply" bucket on case planning board
}
