import { SkuTerm } from './skuTerm.interface';

export interface SKUCaseTerms {
    id?: string;
    skuTerms: SkuTerm[];
    oldCaseCode: string;
    pipelineId?: string;
    effectiveDate: string;
    lastUpdatedBy: string;
}
