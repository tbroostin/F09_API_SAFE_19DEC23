// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// The valid transaction types for an accounts receivable transaction
    /// </summary>
    [Serializable]
    public enum ReceivableTransactionType
    {
        Invoice,
        ReceiptPayment,
        FinancialAid,
        SponsoredBilling,
        PayrollDeduction,
        Transfer,
        DepositAllocation,
        Refund,
        Unknown
    }
}
