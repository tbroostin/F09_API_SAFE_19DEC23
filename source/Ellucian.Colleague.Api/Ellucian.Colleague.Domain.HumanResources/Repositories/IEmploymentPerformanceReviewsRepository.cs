//Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IEmploymentPerformanceReviewsRepository : IEthosExtended
    {
       

        /// <summary>
        /// Get a collection of EmploymentPerformanceReview
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of EmploymentPerformanceReview</returns>
        Task<Tuple<IEnumerable<EmploymentPerformanceReview>, int>> GetEmploymentPerformanceReviewsAsync(int offset, int limit, bool bypassCache = false);
        
        /// <summary>
        /// Returns a review for a specified Employment Performance Reviews key.
        /// </summary>
        /// <param name="ids">Key to Employment Performance Reviews to be returned</param>
        /// <returns>EmploymentPerformanceReview Objects</returns>
        Task<EmploymentPerformanceReview> GetEmploymentPerformanceReviewByIdAsync(string id);

        /// <summary>
        /// Create an EmploymentPerformanceReviews domain entity
        /// </summary>
        /// <param name="employmentPerformanceReviewsEntity"><see cref="EmploymentPerformanceReviews">The EmploymentPerformanceReviews domain entity to create</param>
        /// <returns><see cref="EmploymentPerformanceReviews">The created EmploymentPerformanceReviews domain entity</returns>
        Task<EmploymentPerformanceReview> CreateEmploymentPerformanceReviewsAsync(EmploymentPerformanceReview employmentPerformanceReviewsEntity);

        /// <summary>
        /// Update an EmploymentPerformanceReviews domain entity
        /// </summary>
        /// <param name="employmentPerformanceReviewsEntity"><see cref="EmploymentPerformanceReview">The EmploymentPerformanceReviews domain entity to update</param>
        /// <returns><see cref="EmploymentPerformanceReview">The updated EmploymentPerformanceReview domain entity</returns>
        Task<EmploymentPerformanceReview> UpdateEmploymentPerformanceReviewsAsync(EmploymentPerformanceReview employmentPerformanceReviewsEntity);
        
        /// <summary>
        /// Delete a employment performance review based on review id
        /// </summary>
        /// <param name="reviewsId"></param>
        /// <returns></returns>
        Task DeleteEmploymentPerformanceReviewsAsync(string employmentPerformanceReviewsId);

        /// <summary>
        /// Get a specific GUID from a Record Key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<string> GetGuidFromIdAsync(string key, string entity);

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string> GetIdFromGuidAsync(string id);

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<GuidLookupResult> GetInfoFromGuidAsync(string id);
    }
}
