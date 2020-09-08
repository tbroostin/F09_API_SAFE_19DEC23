//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for EmploymentPerformanceReviewRatings services
    /// </summary>
    public interface IEmploymentPerformanceReviewRatingsService : IBaseService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings>> GetEmploymentPerformanceReviewRatingsAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings> GetEmploymentPerformanceReviewRatingsByGuidAsync(string id);
    }
}
