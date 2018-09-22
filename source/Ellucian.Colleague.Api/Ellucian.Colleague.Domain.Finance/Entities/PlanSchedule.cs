// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// The schedule for a single payment on a payment plan
    /// </summary>
    /// <remarks>Note that this is not the same as a ScheduledPayment.  However, it can be used on the front-end
    /// to define the pertinent information for the payment schedule, or it may be used on the back-end
    /// for reporting type purposes, such as for reproducing the terms acceptance.</remarks>
    [Serializable]
    public class PlanSchedule
    {
        private DateTime _dueDate;
        private decimal _amount;

        /// <summary>
        /// The date a scheduled payment is due on a payment plan
        /// </summary>
        public DateTime DueDate { get { return _dueDate; } }

        /// <summary>
        /// The amount that is due on the payment plan on the due date.
        /// </summary>
        public decimal Amount { get { return _amount; } }

        /// <summary>
        /// PlanSchedule constructor
        /// </summary>
        /// <param name="dueDate">Date payment is due</param>
        /// <param name="amount">Payment amount that is due</param>
        public PlanSchedule(DateTime dueDate, decimal amount)
        {
            _dueDate = dueDate;
            _amount = amount;
        }
    }
}
