//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for PaymentTransactions services
    /// </summary>
    public interface IPaymentTransactionsService : IBaseService
    {
        /// <summary>
        /// Gets all payment-transactions
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="documentGuid">guid for accounts-payable-invoices</param>
        /// <param name="documentTypeValue">invoice or refund</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PaymentTransactions">paymentTransactions</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PaymentTransactions>, int>> GetPaymentTransactionsAsync(int offset, int limit, string documentId,
            InvoiceTypes documentTypeValue, bool bypassCache = false);

        /// <summary>
        /// Get a paymentTransactions by guid.
        /// </summary>
        /// <param name="guid">Guid of the paymentTransactions in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PaymentTransactions">paymentTransactions</see></returns>
        Task<Ellucian.Colleague.Dtos.PaymentTransactions> GetPaymentTransactionsByGuidAsync(string guid, bool bypassCache = false);

    }
}