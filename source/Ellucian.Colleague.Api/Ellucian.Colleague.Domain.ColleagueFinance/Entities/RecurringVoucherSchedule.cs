// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This the schedule for a recurring voucher
    /// </summary>
    [Serializable]
    public class RecurringVoucherSchedule
    {
        /// <summary>
        /// Private schedule date
        /// </summary>
        private readonly DateTime date;

        /// <summary>
        ///  Public getter for the private schedule date
        /// </summary>
        public DateTime Date { get { return date; } }

        /// <summary>
        /// Private schedule amount
        /// </summary>
        private decimal amount;

        /// <summary>
        /// Public getter for the private schedule amount
        /// </summary>
        public Decimal Amount { get { return amount; } }

        /// <summary>
        /// Schedule Tax Amount
        /// </summary>
        public Decimal TaxAmount { get; set; }

        private string voucherId { get; set; }
        /// <summary>
        /// Voucher Number associated with the schedule.
        /// Since VoucherId and PurgedVoucherId are mutually exclusive we set PurgedVoucherId
        /// to null when VoucherId is set.
        /// </summary>
        public string VoucherId
        {
            get { return this.voucherId; }
            set
            {
                this.voucherId = value;
                this.purgedVoucherId = null;
            }
        }

        private string purgedVoucherId { get; set; }
        /// <summary>
        /// Voucher ID that has been purged
        /// Since VoucherId and PurgedVoucherId are mutually exclusive we set VoucherId
        /// to null when PurgedVoucherId is set.
        /// </summary>
        public string PurgedVoucherId
        {
            get { return this.purgedVoucherId; }
            set
            {
                this.purgedVoucherId = value;
                this.voucherId = null;
            }
        }

        /// <summary>
        /// This constructor initializes a recurring voucher schedule domain entity
        /// </summary>
        /// <param name="date">The recurring voucher schedule date</param>
        /// <param name="amount">The recurring voucher schedule amount</param>
        public RecurringVoucherSchedule(DateTime date, decimal amount)
        {
            this.date = date;
            this.amount = amount;
        }
    }
}
