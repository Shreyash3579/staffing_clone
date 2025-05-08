export interface SupplyGroupFilterCriteria {
    startDate: string;                          // dates between which active resources will be fetched
    endDate: string;                            
    employeeCodes: string;                      // eCodes of resources whose data will be fetched
    availabilityIncludes: string;               // comma seperated list of values that need to be included in availability calculation - CD cases, Opps etcs
    groupBy: string;                            // property to group resources on - level name, service line etc
    sortBy: string;                             // property to sort resources on - employee name, alocation percent etc
}
