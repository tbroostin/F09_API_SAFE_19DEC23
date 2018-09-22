// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This enumeration contains the available statuses for a Purchase Order
    /// </summary>
    [Serializable]
    public enum PurchaseOrderStatus
    {
        /// <summary>
        /// The purchase order has at least one item with an accepted status
        /// </summary>
        Accepted,
        /// <summary>
        /// The purchase order has at least one item with a backordered status
        /// </summary>
        Backordered,
        /// <summary>
        /// The purchase order has at least one item with a closed 
        /// and from now on it can only be purged
        /// </summary>
        Closed,
        /// <summary>
        /// The purchase order has not yet been completed and has not been posted
        /// </summary>
        InProgress,
        /// <summary>
        /// The purchase order has at least one item with an invoiced status
        /// </summary>
        Invoiced,
        /// <summary>
        /// The purchase order has been completed but it is awaiting
        /// approvals before it can be posted
        /// </summary>
        NotApproved,
        /// <summary>
        /// The purchase order has at least one item with an accepted status
        /// </summary>
        Outstanding,
        /// <summary>
        /// The purchase order has at least one item with a paid status
        /// </summary>
        Paid,
        /// <summary>
        /// The purchase order has at least one item with a reconciled 
        /// status and from now on it can only be purged
        /// </summary>
        Reconciled,
        /// <summary>
        /// The purchase order has at least one item with a voided
        /// status and from now on it can only be purged
        /// </summary>
        Voided
    }
}
