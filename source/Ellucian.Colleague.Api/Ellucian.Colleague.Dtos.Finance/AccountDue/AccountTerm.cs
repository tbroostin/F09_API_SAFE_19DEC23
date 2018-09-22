// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountDue
{
    /// <summary>
    /// Items due for a given term
    /// </summary>
    public class AccountTerm
    {
        /// <summary>
        /// AccountTerm constructor
        /// </summary>
        public AccountTerm()
        {
            GeneralItems = new List<AccountsReceivableDueItem>();
            InvoiceItems = new List<InvoiceDueItem>();
            PaymentPlanItems = new List<PaymentPlanDueItem>();
            DepositDueItems = new List<DepositDue>();
        }

        /// <summary>
        /// Total amount due for the term
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Term ID
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// Term Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// General AR account items due
        /// </summary>
        public List<AccountsReceivableDueItem> GeneralItems { get; set; }

        /// <summary>
        /// Invoice items due
        /// </summary>
        public List<InvoiceDueItem> InvoiceItems { get; set; }

        /// <summary>
        /// Plan payments due
        /// </summary>
        public List<PaymentPlanDueItem> PaymentPlanItems { get; set; }

        /// <summary>
        /// Deposits due
        /// </summary>
        public List<DepositDue> DepositDueItems { get; set; }
    }
}
