// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class EmploymentPerformanceReviewResponse
    {
        public string WarningCode { get; set; }
        public string WarningMessage { get; set; }
        public string EmploymentPerformanceReviewId { get; set; }
        public string EmploymentPerformanceReviewGuid { get; set; }
    }
}
