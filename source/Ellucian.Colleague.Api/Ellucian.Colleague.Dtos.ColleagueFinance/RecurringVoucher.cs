// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is the recurring voucher DTO
    /// </summary>
    public class RecurringVoucher
    {
        /// <summary>
        /// The recurring voucher ID
        /// </summary>
        public string RecurringVoucherId { get; set; }

        /// <summary>
        /// The recurring voucher vendor ID
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// The recurring voucher vendor name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// The recurring voucher amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The recurring voucher date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The recurring voucher maintenance date
        /// </summary>
        public DateTime? MaintenanceDate { get; set; }

        /// <summary>
        /// The recurring voucher invoice number
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// The recurring voucher invoice date
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// The recurring voucher current status
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RecurringVoucherStatus Status { get; set; }

        /// <summary>
        /// The recurring voucher current status date
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// The recurring voucher AP type
        /// </summary>
        public string ApType { get; set; }

        /// <summary>
        /// The recurring voucher comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Currency code indicating a foreign currency
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The total schedule amount in local currency
        /// </summary>
        public decimal? TotalScheduleAmountInLocalCurrency { get; set; }

        /// <summary>
        /// The total schedule tax amount in local currency
        /// </summary>
        public decimal? TotalScheduleTaxAmountInLocalCurrency { get; set; }

        /// <summary>
        /// The recurring voucher list of approval information
        /// </summary>
        public List<Approver> Approvers { get; set; }

        /// <summary>
        /// List of the recurring voucher schedules
        /// </summary>
        public List<RecurringVoucherSchedule> Schedules { get; set; }
    }
}
