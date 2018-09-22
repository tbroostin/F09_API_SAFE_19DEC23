// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for the AccountingString service.
    /// </summary>
    public interface IAccountingStringService : IBaseService
    {
        /// <summary>
        /// Returns the AccountingString if found and valid
        /// </summary>
        /// <param name="accountingStringValue">accountingString to check</param>
        /// <param name="validOn">date to check</param>
        ///  <returns>Returns the AccountingString if found and valid.</returns>
        Task<AccountingString> GetAccoutingStringByFilterCriteriaAsync(string accountingStringValue, DateTime? validOn = null);

        Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponent>> GetAccountingStringComponentsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AccountingStringComponent> GetAccountingStringComponentsByGuidAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringFormats>> GetAccountingStringFormatsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AccountingStringFormats> GetAccountingStringFormatsByGuidAsync(string id);

        /// <summary>
        /// Get the all values for accounting String Component values
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponentValues>,int>> GetAccountingStringComponentValuesAsync(int Offset, int Limit, string component, string transactionStatus, string typeAccount, string typeFund, bool bypassCache = false);
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponentValues2>, int>> GetAccountingStringComponentValues2Async(int Offset, int Limit, string component, string transactionStatus, string typeAccount, string typeFund, bool bypassCache = false);

        /// <summary>
        /// Get a specific value for accounting string component values
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.AccountingStringComponentValues> GetAccountingStringComponentValuesByGuidAsync(string id);
        Task<Ellucian.Colleague.Dtos.AccountingStringComponentValues2> GetAccountingStringComponentValues2ByGuidAsync(string id, bool bypassCache);

        /// <summary>
        /// Get the all values for accounting String Component values.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="component"></param>
        /// <param name="transactionStatus"></param>
        /// <param name="typeAccount"></param>
        /// <param name="typeFund"></param>
        /// <param name="effectiveOn"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.AccountingStringComponentValues3>, int>> GetAccountingStringComponentValues3Async(int offset, int limit, Dtos.AccountingStringComponentValues3 criteria, DateTime? effectiveOn, bool bypassCache = false);

        /// <summary>
        /// Get the all values for accounting String Component value by guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Dtos.AccountingStringComponentValues3> GetAccountingStringComponentValues3ByGuidAsync(string guid, bool bypassCache);
    }
}
