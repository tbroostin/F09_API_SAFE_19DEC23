// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A payment to be paid on a certain date as part of a payment plan's schedule of payments
    /// </summary>
    public class ScheduledPayment
    {
        /// <summary>
        /// ID of the scheduled payment
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the payment plan to which the scheduled payment belongs
        /// </summary>
        public string PlanId { get; set; }
        
        /// <summary>
        /// Amount to be paid
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Date on which the scheduled payment is due to be paid
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Determines whether the scheduled payment is overdue
        /// </summary>
        public bool IsPastDue { get; set; }

        /// <summary>
        /// Amount paid against the scheduled payment
        /// </summary>
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Date on which last payment was made against scheduled payment
        /// </summary>
        public DateTime? LastPaidDate { get; set; }
    }
}
