// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Finance;

namespace Ellucian.Colleague.Coordination.Finance
{
    public interface IReceiptService
    {
        /// <summary>
        /// Retrieve a cashier
        /// </summary>
        /// <param name="id">The ID of the cashier to retrieve</param>
        /// <returns>The indicated cashier</returns>
        Domain.Finance.Entities.Cashier GetCashier(string id);

        /// <summary>
        /// Create a receipt and associated financial items
        /// </summary>
        /// <param name="sourceReceipt">The information of the receipt of funds</param>
        /// <param name="sourcePayments">The information for any associated payments to create</param>
        /// <param name="sourceDeposits">The information for any associated deposits to create</param>
        /// <returns></returns>
        Receipt CreateReceipt(Receipt sourceReceipt, IEnumerable<ReceiptPayment> sourcePayments,
            IEnumerable<Deposit> sourceDeposits);
    }
}
