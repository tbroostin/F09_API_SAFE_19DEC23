/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    public interface IPaymentTransactionsRepository
    {
        Task<Tuple<IEnumerable<PaymentTransaction>, int>> GetPaymentTransactionsAsync(int offset, int limit, string documentId, InvoiceOrRefund invoiceOrRefund, string docNumber, List<string> refPoDoc, List<string> refBpoDoc, List<string> refRecDoc);
        Task<PaymentTransaction> GetPaymentTransactionsByGuidAsync(string guid);
        Task<string> GetPaymentTransactionsIdFromGuidAsync(string guid);
        Task<Dictionary<string, string>> GetPaymentTransactionsGuidsCollectionAsync(IEnumerable<string> ids, string filename);

        Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename);

        Task<string> GetIdFromGuidAsync(string guid, string entity);
    }
}