// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is the recurring voucher schedule DTO
    /// </summary>
    public class RecurringVoucherSchedule
    {
        /// <summary>
        /// Schedule Date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Schedule Amount
        /// </summary>
        public Decimal Amount { get; set; }

        /// <summary>
        /// Schedule Tax Amount
        /// </summary>
        public Decimal TaxAmount { get; set; }

        /// <summary>
        /// Voucher number associated with the schedule
        /// </summary>
        public string VoucherId { get; set; }

        /// <summary>
        /// Voucher ID that has been purged
        /// </summary>
        public string PurgedVoucherId { get; set; }
    }
}
