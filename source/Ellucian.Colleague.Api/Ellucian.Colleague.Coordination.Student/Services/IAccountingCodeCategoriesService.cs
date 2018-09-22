//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AccountingCodeCategories services
    /// </summary>
    public interface IAccountingCodeCategoriesService : IBaseService
    {
          
        /// <summary>
        /// Gets all accounting-code-categories
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AccountingCodeCategories">accountingCodeCategories</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingCodeCategory>> GetAccountingCodeCategoriesAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a accountingCodeCategories by guid.
        /// </summary>
        /// <param name="guid">Guid of the accountingCodeCategories in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AccountingCodeCategories">accountingCodeCategories</see></returns>
        Task<Ellucian.Colleague.Dtos.AccountingCodeCategory> GetAccountingCodeCategoryByGuidAsync(string guid, bool bypassCache = false);

            
    }
}
