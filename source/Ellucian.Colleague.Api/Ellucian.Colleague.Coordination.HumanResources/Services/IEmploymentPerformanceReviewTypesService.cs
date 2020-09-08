//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for EmploymentPerformanceReviewTypes services
    /// </summary>
    public interface IEmploymentPerformanceReviewTypesService : IBaseService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes>> GetEmploymentPerformanceReviewTypesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes> GetEmploymentPerformanceReviewTypesByGuidAsync(string id);
    }
}
