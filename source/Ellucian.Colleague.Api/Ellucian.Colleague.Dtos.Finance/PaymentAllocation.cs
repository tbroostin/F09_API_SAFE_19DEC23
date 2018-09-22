// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A payment allocated to a payable item
    /// </summary>
    public class PaymentAllocation
    {
        /// <summary>
        /// ID of allocation
        /// </summary>
        public string Id { get; set;  }

        /// <summary>
        /// ID of payment for which allocation is made
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// Source of allocation
        /// </summary>
        public PaymentAllocationSource Source { get; set; }

        /// <summary>
        /// Allocation amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// ID of charge to which allocation is made
        /// </summary>
        public string ChargeId { get; set; }

        /// <summary>
        /// Indicator for whether allocation is invoice-specific
        /// </summary>
        public bool IsInvoiceAllocated { get; set; }

        /// <summary>
        /// ID of scheduled payment associated with allocation
        /// </summary>
        public string ScheduledPaymentId { get; set; }

        /// <summary>
        /// Amount that is unallocated
        /// </summary>
        public decimal UnallocatedAmount { get; set; }
    }
}
