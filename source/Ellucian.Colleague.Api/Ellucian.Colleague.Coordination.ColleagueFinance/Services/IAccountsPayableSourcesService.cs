//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for AccountsPayableSources services
    /// </summary>
    public interface IAccountsPayableSourcesService : IBaseService
    {
        Task<IEnumerable<AccountsPayableSources>> GetAccountsPayableSourcesAsync(bool bypassCache = false);
        Task<AccountsPayableSources> GetAccountsPayableSourcesByGuidAsync(string id);
    }
}
