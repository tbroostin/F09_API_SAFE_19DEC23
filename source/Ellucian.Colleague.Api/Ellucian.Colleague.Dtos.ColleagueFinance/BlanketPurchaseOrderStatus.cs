// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// These are the available statuses for a Blanket Purchase Order
    /// </summary>
    [Serializable]
    public enum BlanketPurchaseOrderStatus
    {
        /// <summary>
        /// The blanket purchase order has been closed 
        /// </summary>
        Closed,
        /// <summary>
        /// The blanket purchase order has not yet been completed and has not been posted
        /// </summary>
        InProgress,
        /// <summary>
        /// The blanket purchase order has been completed and it is awaiting
        /// approvals before it can be posted
        /// </summary>
        NotApproved,
        /// <summary>
        /// The blanket purchase order is outstanding; it has been posted
        /// </summary>
        Outstanding,
        /// <summary>
        /// The blanket purchase order has been voided
        /// </summary>
        Voided
    }
}
