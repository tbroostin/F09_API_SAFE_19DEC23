/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    public interface IPaymentTransactionsRepository
    {
        Task<Tuple<IEnumerable<PaymentTransaction>, int>> GetPaymentTransactionsAsync(int offset, int limit, string documentId, InvoiceOrRefund invoiceOrRefund);
        Task<PaymentTransaction> GetPaymentTransactionsByGuidAsync(string guid);
        Task<string> GetPaymentTransactionsIdFromGuidAsync(string guid);
        Task<string> GetGuidFromIdAsync(string entity, string id, string secondaryField = "", string secondaryKey = "");


    }
}