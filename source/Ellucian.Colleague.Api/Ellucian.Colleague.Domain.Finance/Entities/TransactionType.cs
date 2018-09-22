// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public enum TransactionType
    {
        Invoice, Receipt, FinancialAid, SponsorBilling, PayrollDeduction, Transfer, DepositAllocation, Refund
    }
}
