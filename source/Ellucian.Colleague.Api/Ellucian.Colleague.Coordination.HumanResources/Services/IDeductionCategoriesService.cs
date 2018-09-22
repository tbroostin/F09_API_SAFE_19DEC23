//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for DeductionCategories services
    /// </summary>
    public interface IDeductionCategoriesService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.DeductionCategories>> GetDeductionCategoriesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.DeductionCategories> GetDeductionCategoriesByGuidAsync(string id);
    }
}
