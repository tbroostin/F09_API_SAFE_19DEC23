// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This enumeration contains the available status values for recurring vouchers
    /// </summary>
    [Serializable]
    public enum RecurringVoucherStatus
    {
        /// <summary>
        /// The recurring voucher has been cancelled
        /// </summary>
        Cancelled,

        /// <summary>
        /// The recurring voucher has been closed
        /// </summary>
        Closed,

        /// <summary>
        /// The recurring voucher is not approved which means that approvals are required
        /// for vouchers and it is waiting on sufficient approvals to become outstanding
        /// </summary>
        NotApproved,

        /// <summary>
        /// The recurring voucher is outstanding and the encumbrances
        /// are reflected in the general ledger
        /// </summary>
        Outstanding,

        /// <summary>
        /// The recurring voucher has been voided
        /// </summary>
        Voided
    }
}
