import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SortHelperService {

constructor() { }

public static sortByPipelineId(a, b) {
    const hasPipelineIdA = !!a.pipelineId;
    const hasPipelineIdB = !!b.pipelineId;
    if (hasPipelineIdA && !hasPipelineIdB) return -1;
    if (!hasPipelineIdA && hasPipelineIdB) return 1;
    return 0;
}

public static sortByCaseTypeCode(a, b, caseTypeOrder) {
    const hasOldCaseCodeA = !!a.oldCaseCode;
    const hasOldCaseCodeB = !!b.oldCaseCode;
    if (hasOldCaseCodeA && hasOldCaseCodeB) {
        const caseTypeA = caseTypeOrder.indexOf(a.caseTypeCode);
        const caseTypeB = caseTypeOrder.indexOf(b.caseTypeCode);
        if (caseTypeA !== caseTypeB) {
            return caseTypeA - caseTypeB;
        }
    }
    return 0;
}

public static sortByAlphaNumericandNulls(a, b) {
    if(a !== b) {
        if (!a || !b) {
        return !a ? 1 : -1; // Null values should come last
        }
        return a.localeCompare(b);
    }
    return 0;
}

public static sortNumeric(a, b, order) {
    if (a !== b) {
        if (order === 'desc')
            return (b || 0) - (a || 0);
        else
            return (a || 0) - (b || 0);
    }
}

public static sortDates(a, b, order) {
    const startDateA = new Date(a);
    const startDateB = new Date(b);

    if (startDateA.getTime() !== startDateB.getTime()) {
        if (order === 'desc')
            return <any>new Date(b) - <any>new Date(a);
        else
            return <any>new Date(a) - <any>new Date(b);
    }
    return 0;
}

}
