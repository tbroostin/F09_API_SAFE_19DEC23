//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for AccountingStringSubcomponentValues services
    /// </summary>
    public interface IAccountingStringSubcomponentValuesService : IBaseService
    {
        /// <summary>
        /// Gets all accounting-string-subcomponent-values
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AccountingStringSubcomponentValues">accountingStringSubcomponentValues</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues>, int>> GetAccountingStringSubcomponentValuesAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a accountingStringSubcomponentValues by guid.
        /// </summary>
        /// <param name="guid">Guid of the accountingStringSubcomponentValues in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AccountingStringSubcomponentValues">accountingStringSubcomponentValues</see></returns>
        Task<Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues> GetAccountingStringSubcomponentValuesByGuidAsync(string guid, bool bypassCache = true);


    }
}
