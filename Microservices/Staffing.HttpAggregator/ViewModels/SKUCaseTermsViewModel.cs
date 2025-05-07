using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class SKUCaseTermsViewModel
    {
        public Guid? Id { get; set; }
        public IEnumerable<SKUTerm> SKUTerms { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public string EffectiveDate { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
