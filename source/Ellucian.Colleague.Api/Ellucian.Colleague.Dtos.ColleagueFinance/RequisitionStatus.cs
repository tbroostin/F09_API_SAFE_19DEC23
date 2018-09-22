// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This enumeration contains the available statuses for a Requisition
    /// </summary>
    [Serializable]
    public enum RequisitionStatus
    {
        /// <summary>
        /// The requisition has not yet been completed and has not been posted
        /// </summary>
        InProgress,
        /// <summary>
        /// The requisition has at least one item in a purchase order
        /// </summary>
        PoCreated,
        /// <summary>
        /// The requisition has been completed and it is awaiting
        /// approvals but it has been posted
        /// </summary>
        NotApproved,
        /// <summary>
        /// The requisition is outstanding
        /// </summary>
        Outstanding,
    }
}
