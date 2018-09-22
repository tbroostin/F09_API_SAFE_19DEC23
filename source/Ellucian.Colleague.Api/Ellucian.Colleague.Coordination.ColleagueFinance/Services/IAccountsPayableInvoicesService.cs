//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for AccountsPayableInvoices services
    /// </summary>
    public interface IAccountsPayableInvoicesService : IBaseService
    {
        Task<Tuple<IEnumerable<Dtos.AccountsPayableInvoices2>, int>> GetAccountsPayableInvoices2Async(int offset, int limit, bool bypassCache = false);
        Task<Dtos.AccountsPayableInvoices2> GetAccountsPayableInvoices2ByGuidAsync(string guid);
        Task<Dtos.AccountsPayableInvoices2> PutAccountsPayableInvoices2Async(string guid, Dtos.AccountsPayableInvoices2 accountsPayableInvoices);
        Task<Dtos.AccountsPayableInvoices2> PostAccountsPayableInvoices2Async(Dtos.AccountsPayableInvoices2 accountsPayableInvoices);
        
    }
}
