// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// This enumeration contains all of the status values that are available for vouchers.
    /// </summary>
    [Serializable]
    public enum RefundVoucherStatus
    {
        /// <summary>
        /// NotApproved status means that approvals are required for vouchers, and that the
        /// voucher does not have sufficient approval signatures to make it outstanding.
        /// </summary>
        NotApproved,

        /// <summary>
        /// Outstanding status means that the voucher amounts have been reflected in the
        /// general ledger.
        /// </summary>
        Outstanding,

        /// <summary>
        /// Paid status means that the voucher has been paid.
        /// </summary>
        Paid,

        /// <summary>
        /// Reconciled status means that the cash disbursement for the voucher has been
        /// reconciled with the bank.
        /// </summary>
        Reconciled,

        /// <summary>
        /// InProgress status means that the voucher is unfinished.
        /// </summary>
        InProgress,

        /// <summary>
        /// Voided status means that the voucher has been voided.
        /// </summary>
        Voided,

        /// <summary>
        /// Cancelled status means that the voucher was cancelled.
        /// </summary>
        Cancelled,

        /// <summary>
        /// Unknown status.
        /// </summary>
        Unknown
    }
}
