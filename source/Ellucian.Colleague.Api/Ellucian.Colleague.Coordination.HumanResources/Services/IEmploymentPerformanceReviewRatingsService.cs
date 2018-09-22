//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for EmploymentPerformanceReviewRatings services
    /// </summary>
    public interface IEmploymentPerformanceReviewRatingsService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings>> GetEmploymentPerformanceReviewRatingsAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings> GetEmploymentPerformanceReviewRatingsByGuidAsync(string id);
    }
}
