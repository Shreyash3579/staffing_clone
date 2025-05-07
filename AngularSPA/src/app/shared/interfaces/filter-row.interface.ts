export interface FilterRow {
  andOr: string;
  filterField: string;
  filterOperator?: string;
  filterValue: string;
  parsedValue?: any; // This is used just to show data on UI. value should be used for all other purposes.
}
