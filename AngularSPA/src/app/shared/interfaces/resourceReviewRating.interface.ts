export interface ResourceReviewRating {
  reviewId: string;
  ratings: Rating;
  document: string;
  lastUpdated: Date;
  reviewStatusDate: Date;
}

interface Rating {
  ratingLabel: string;
  ratingResult: string;
}
