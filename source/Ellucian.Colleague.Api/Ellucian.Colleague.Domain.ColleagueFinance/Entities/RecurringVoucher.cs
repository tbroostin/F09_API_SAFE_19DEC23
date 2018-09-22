// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a recurring voucher
    /// </summary>
    [Serializable]
    public class RecurringVoucher : AccountsPayablePurchasingDocument
    {
        /// <summary>
        /// Private variable for the recurring voucher current status
        /// </summary>
        private readonly RecurringVoucherStatus status;

        /// <summary>
        /// The public getter for the private recurring voucher current status
        /// </summary>
        public RecurringVoucherStatus Status { get { return status; } }

        /// <summary>
        /// Private current status date for public getter
        /// </summary>
        private readonly DateTime statusDate;

        /// <summary>
        /// The recurring voucher current status date
        /// </summary>
        public DateTime StatusDate { get { return statusDate; } }

        /// <summary>
        /// Private variable for the recurring voucher invoice number
        /// </summary>
        private readonly string invoiceNumber;

        /// <summary>
        /// Public getter for the private recurring voucher invoice number
        /// </summary>
        public string InvoiceNumber { get { return invoiceNumber; } }

        /// <summary>
        /// Private variable for the recurring voucher invoice date
        /// </summary>
        private readonly DateTime invoiceDate;

        /// <summary>
        /// Public getter for the private recurring voucher invoice date
        /// </summary>
        public DateTime InvoiceDate { get { return invoiceDate; } }

        /// <summary>
        /// Exchange rate used to calculate the local amounts.
        /// </summary>
        public decimal? ExchangeRate { get; set; }

        /// <summary>
        /// Returns the sum of the schedule amounts after being converted to local currency 
        /// </summary>
        public decimal? TotalScheduleAmountInLocalCurrency
        {
            get
            {
                if (this.IsLocalCurrencySetupInvalid())
                    return null;

                return Math.Round(this.schedules.Sum(x => x.Amount) / this.ExchangeRate.Value, 2);
            }
        }

        /// <summary>
        /// Returns the sum of the schedule tax amounts after being converted to local currency.
        /// </summary>
        public decimal? TotalScheduleTaxAmountInLocalCurrency
        {
            get
            {
                if (this.IsLocalCurrencySetupInvalid())
                    return null;

                return Math.Round(this.schedules.Sum(x => x.TaxAmount) / this.ExchangeRate.Value, 2);
            }
        }

        /// <summary>
        /// Determines if a local currency amount can be calculated.
        /// </summary>
        private bool IsLocalCurrencySetupInvalid()
        {
            return string.IsNullOrEmpty(this.CurrencyCode) || !this.ExchangeRate.HasValue || this.ExchangeRate <= 0;
        }

        /// <summary>
        /// The private list of schedules for the recurring voucher
        /// </summary>
        private readonly List<RecurringVoucherSchedule> schedules = new List<RecurringVoucherSchedule>();

        /// <summary>
        /// List of the recurring voucher schedules
        /// </summary>
        public ReadOnlyCollection<RecurringVoucherSchedule> Schedules { get; private set; }

        /// <summary>
        /// This constructor initializes the recurring voucher domain entity
        /// </summary>
        /// <param name="recurringVoucherId">The recurring voucher ID</param>
        /// <param name="date">The recurring voucher date</param>
        /// <param name="status">The recurring voucher current status</param>
        /// <param name="vendorName">The recurring voucher vendor name</param>
        /// <param name="invoiceNumber">The recurring voucher invoice number</param>
        /// <param name="invoiceDate">The recurring voucher invoice date</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public RecurringVoucher(string recurringVoucherId, DateTime date, RecurringVoucherStatus status, DateTime statusDate, string vendorName, string invoiceNumber, DateTime invoiceDate)
            : base(recurringVoucherId, vendorName, date)
        {
            if (string.IsNullOrEmpty(invoiceNumber))
            {
                throw new ArgumentNullException("invoiceNumber", "Invoice number is a required field.");
            }

            this.status = status;
            this.statusDate = statusDate;
            this.invoiceNumber = invoiceNumber;
            this.invoiceDate = invoiceDate;
            Schedules = this.schedules.AsReadOnly();
        }

        /// <summary>
        /// This method adds a schedule to the list of schedules for the recurring voucher
        /// </summary>
        /// <param name="schedule">The recurring voucher schedule</param>
        public void AddSchedule(RecurringVoucherSchedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException("schedule", "The schedule cannot be null.");
            }

            bool isInList = false;
            if (schedules != null)
            {
                foreach (var sch in schedules)
                {
                    if ((sch.Date == schedule.Date))
                    {
                        isInList = true;
                    }
                }
            }
            if (!isInList)
            {
                schedules.Add(schedule);
            }
        }
    }
}