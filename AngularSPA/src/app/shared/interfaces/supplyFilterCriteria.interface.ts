export interface SupplyFilterCriteria {
    startDate: string;                          // dates between
    endDate: string;                            // which active resources will be fetched
    officeCodes: string;                        // comma seperated list of office(s) codes
    levelGrades: string;                        // comma seperated list of level grade(s) codes - AC, Manager etc
    staffingTags: string;                       // comma seperated list of service line codes and Commitments saved in Staffing DB - Consulting, ADAPT, FRWD, PEG, etc
    availabilityIncludes: string;               // comma seperated list of values that need to be included in availability calculation - CD cases, Opps etcs
    groupBy: string;                            // property to group resources on - level name, service line etc
    sortBy: string;                             // property to sort resources on - employee name, alocation percent etc
    sortDirection: string;
    affiliationRoleCodes: string;
    practiceAreaCodes: string;                      // comma separate list of practice area codes
    employeeStatuses: string;                   // comma separate list of employee status like 'LOA','Active','Not yet Started' etc
    positionCodes: string;                      // comma separate list of position Codes
    staffableAsTypeCodes: string;                   // comma separated list of staffable as roles codes
}
