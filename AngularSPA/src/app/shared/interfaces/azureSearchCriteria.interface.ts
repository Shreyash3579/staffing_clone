export interface BossSearchResult {
  generatedLuceneSearchQuery : BossSearchQuery;
  searchResultsEcodes : string;
  searches: any
}

export interface BossSearchQuery {
  search: string;
  filter: string;
  isErrorInGeneratingSearchQuery?: boolean;
  errorResponse?: string;
}

export interface BossSearchCriteria {
  searchString: string;
  searchTriggeredFrom: string;
  employeeCodesToSearchIn: string;
  pageSize: number;
}