// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is the voucher DTO.
    /// </summary>
    public class Voucher2
    {
        /// <summary>
        /// This is the voucher ID.
        /// </summary>
        public string VoucherId { get; set; }

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
        /// This is the voucher due date.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// This is the voucher maintenance date.
        /// </summary>
        public DateTime? MaintenanceDate { get; set; }

        /// <summary>
        /// This is the voucher invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// This is the voucher invoice date.
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// This is the voucher check number.
        /// </summary>
        public string CheckNumber { get; set; }

        /// <summary>
        /// This is the voucher check date.
        /// </summary>
        public DateTime? CheckDate { get; set; }

        /// <summary>
        /// This is the voucher comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// This is the purchase order ID associated with the voucher.
        /// </summary>
        public string PurchaseOrderId { get; set; }

        /// <summary>
        /// This is the blanket purchase order ID associated with the voucher.
        /// </summary>
        public string BlanketPurchaseOrderId { get; set; }

        /// <summary>
        /// This is the recurring voucher ID associated with the voucher.
        /// </summary>
        public string RecurringVoucherId { get; set; }

        /// <summary>
        /// This is the voucher currency code.
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// This is the voucher status.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public VoucherStatus Status { get; set; }

        /// <summary>
        /// This is the voucher status date
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// This is the voucher AP type.
        /// </summary>
        public string ApType { get; set; }

        /// <summary>
        /// This is the list of voucher line items.
        /// </summary>
        public List<LineItem> LineItems { get; set; }

        /// <summary>
        /// This is the list of approval information on the voucher.
        /// </summary>
        public List<Approver> Approvers { get; set; }

        /// <summary>
        /// This is the vendor address.        
        /// </summary>
        public List<string> VendorAddressLines { get; set; }

        /// <summary>
        /// This is the vendor city.
        /// </summary>
        public string VendorCity { get; set; }

        /// <summary>
        /// This is the vendor state.
        /// </summary>
        public string VendorState { get; set; }

        /// <summary>
        /// This is the vendor postal code.
        /// </summary>
        public string VendorZip { get; set; }

        /// <summary>
        /// This is the vendor country.
        /// </summary>
        public string VendorCountry { get; set; }


        /// <summary>
        /// List of email addresses - confirmation email notifications would be sent to these email addresses on create / update .
        /// </summary>
        public List<string> ConfirmationEmailAddresses { get; set; }

        /// <summary>
        /// Address type code
        /// </summary>
        public string VendorAddressTypeCode { get; set; }

        /// <summary>
        /// Address type description
        /// </summary>
        public string VendorAddressTypeDesc { get; set; }
    }
}
