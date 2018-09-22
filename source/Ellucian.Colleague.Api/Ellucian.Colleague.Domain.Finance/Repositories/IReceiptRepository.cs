// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    /// <summary>
    /// The interface to a Receipt Repository
    /// </summary>
    public interface IReceiptRepository
    {
        /// <summary>
        /// Get a receipt by ID
        /// </summary>
        /// <param name="id">Receipt ID</param>
        /// <returns>The requested receipt</returns>
        Receipt GetReceipt(string id);

        ///// <summary>
        ///// Get a receipt by receipt number
        ///// </summary>
        ///// <param name="receiptNumber">Receipt number</param>
        ///// <returns>The requested receipt</returns>
        //Receipt GetReceiptByReceiptNumber(string receiptNumber);

        ///// <summary>
        ///// Get a receipt by deposit ID
        ///// </summary>
        ///// <param name="depositId">Deposit ID</param>
        ///// <returns>The requested receipt</returns>
        //Receipt GetReceiptByDepositId(string depositId);

        /// <summary>
        /// Get a cashier by ID
        /// </summary>
        /// <param name="id">Cashier ID</param>
        /// <returns>Cashier entity</returns>
        Cashier GetCashier(string id);

        /// <summary>
        /// Create a receipt
        /// </summary>
        /// <param name="receipt">The receipt to create</param>
        /// <param name="payments">The set of payments to create</param>
        /// <param name="deposits">The set of deposits to create</param>
        /// <returns>The newly created receipt</returns>
        /// <remarks>Either payments, or deposits, or both, must be specified</remarks>
        Receipt CreateReceipt(Receipt receipt, IEnumerable<ReceiptPayment> payments, IEnumerable<Deposit> deposits);

        ///// <summary>
        ///// Retrieve a cashier's session by id
        ///// </summary>
        ///// <param name="id">The id of the session to retrieve</param>
        ///// <returns>The retrieved session</returns>
        //ReceiptSession GetReceiptSession(string id);
    }
}
