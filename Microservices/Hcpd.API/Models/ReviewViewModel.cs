using System;
using System.Collections.Generic;

namespace Hcpd.API.Models
{
    public class ReviewViewModel
    {
        public Guid ReviewId { get; set; }
        public IList<Rating> Ratings { get; set; }
        public string Document { get; set; }
        public string ReviewStatusCode { get; set; }
        public string ReviewStatus { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime ReviewStatusDate { get; set; }
    }

    public class Rating
    {
        public string RatingLabel { get; set; }
        public string RatingResult { get; set; }
    }
}