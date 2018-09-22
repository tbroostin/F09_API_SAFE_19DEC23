//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for EmploymentPerformanceReviews services
    /// </summary>
    public interface IEmploymentPerformanceReviewsService : IBaseService
    {

        Task<Tuple<IEnumerable<Dtos.EmploymentPerformanceReviews>, int>> GetEmploymentPerformanceReviewsAsync(int offset, int limit, bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviews> GetEmploymentPerformanceReviewsByGuidAsync(string id);

        Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviews> PostEmploymentPerformanceReviewsAsync(Ellucian.Colleague.Dtos.EmploymentPerformanceReviews employmentPerformanceReviewsDto);

        Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviews> PutEmploymentPerformanceReviewsAsync(string guid, Dtos.EmploymentPerformanceReviews employmentPerformanceReviewsDto);
      
        Task DeleteEmploymentPerformanceReviewAsync(string employmentPerformanceReviewsId);
    }
}
