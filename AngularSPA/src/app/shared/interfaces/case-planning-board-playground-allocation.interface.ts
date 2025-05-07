export interface CasePlanningBoardPlaygroundAllocation {
    employeeCode: string;
    newStartDate?: Date;
    newEndDate?: Date;
    newAllocation?: Number;
    newInvestmentCode?: Number;
    previousStartDate?: Date;
    previousEndDate?: Date;
    previousAllocation?: Number;
    previousInvestmentCode?: Number;
    isOpportunity: boolean;
    caseEndDate?: Date;
  }
