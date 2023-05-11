// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is a Voucher Summary
    /// </summary>
    public class VoucherSummary
    {
        /// <summary>
        /// This is the voucher ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// This is the vendor ID.
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// This is the vendor name.
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// This is the voucher amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// This is the voucher date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// This is the voucher invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// This is the voucher invoice date.
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// This is the voucher requestor name
        /// </summary>
        public string RequestorName { get; set; }

        /// <summary>
        /// This is the voucher status.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public VoucherStatus Status { get; set; }

        /// <summary>
        /// The blanket purchase order Id associated with the voucher
        /// </summary>
        public string BlanketPurchaseOrderId { get; set; }

        /// <summary>
        /// The blanket purchase order number associated with the voucher
        /// </summary>
        public string BlanketPurchaseOrderNumber { get; set; }

        /// <summary>
        /// This is the recurring voucher ID associated with the voucher.
        /// </summary>
        public string RecurringVoucherId { get; set; }

        /// <summary>
        /// This is the recurring voucher number associated with the voucher.
        /// </summary>
        public string RecurringVoucherNumber { get; set; }

        /// <summary>
        /// Flag to indicate if document has attachment/s associated
        /// </summary>
        public bool AttachmentsIndicator { get; set; }

        /// <summary>
        /// List of purchase orders.
        /// </summary>
        public List<PurchaseOrderLinkSummary> PurchaseOrders { get; set; }

        /// <summary>
        /// List of Approvers associated to this voucher
        /// </summary>
        public List<Approver> Approvers { get; set; }

        /// <summary>
        /// Flag to indicate if document has returned from approval
        /// </summary>
        public bool ApprovalReturnedIndicator { get; set; }

    }
}
