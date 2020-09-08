/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    public interface IAccountsPayableInvoicesRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<AccountsPayableInvoices>, int>> GetAccountsPayableInvoices2Async(int offset, int limit, string invoiceNumber);
        Task<AccountsPayableInvoices> GetAccountsPayableInvoicesByGuidAsync(string guid, bool allowVoid);
        Task<AccountsPayableInvoices> UpdateAccountsPayableInvoicesAsync(AccountsPayableInvoices accountsPayableInvoicesEntity);
        Task<AccountsPayableInvoices> CreateAccountsPayableInvoicesAsync(AccountsPayableInvoices accountsPayableInvoicesEntity);
        Task<string> GetAccountsPayableInvoicesIdFromGuidAsync(string guid);
        Task<string> GetAccountsPayableInvoicesGuidFromIdAsync(string id);
        Task<string> GetGuidFromID(string key, string entity);
        Task<GuidLookupResult> GetIdFromGuidAsync(string guid);
        Task<IDictionary<string, string>> GetProjectReferenceIds(string[] projectIds);
        Task<IDictionary<string, string>> GetProjectIdsFromReferenceNo(string[] projectRefNo);

    }
}