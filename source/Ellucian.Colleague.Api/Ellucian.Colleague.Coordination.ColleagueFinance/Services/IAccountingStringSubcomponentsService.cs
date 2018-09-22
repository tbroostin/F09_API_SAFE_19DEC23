//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for AccountingStringSubcomponents services
    /// </summary>
    public interface IAccountingStringSubcomponentsService : IBaseService
    {

        /// <summary>
        /// Gets all accounting-string-subcomponents
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AccountingStringSubcomponents">accountingStringSubcomponents</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringSubcomponents>> GetAccountingStringSubcomponentsAsync(bool bypassCache = false);

        /// <summary>
        /// Get a accountingStringSubcomponents by guid.
        /// </summary>
        /// <param name="guid">Guid of the accountingStringSubcomponents in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AccountingStringSubcomponents">accountingStringSubcomponents</see></returns>
        Task<Ellucian.Colleague.Dtos.AccountingStringSubcomponents> GetAccountingStringSubcomponentsByGuidAsync(string guid, bool bypassCache = true);


    }
}
