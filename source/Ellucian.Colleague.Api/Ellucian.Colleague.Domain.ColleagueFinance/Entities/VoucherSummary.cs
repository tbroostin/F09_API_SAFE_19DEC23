// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Voucher Summary entity
    /// </summary>
    [Serializable]
    public class VoucherSummary : AccountsPayablePurchasingDocument
    {
        /// <summary>
        /// Private invoice number for public getter
        /// </summary>
        private readonly string invoiceNumber;

        /// <summary>
        /// This is the voucher invoice number.
        /// </summary>
        public string InvoiceNumber { get { return invoiceNumber; }}

        /// <summary>
        /// This is the voucher invoice date.
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// The voucher current status
        /// </summary>
        public VoucherStatus Status { get; set; }

        /// <summary>
        /// The voucher requestor name
        /// </summary>
        public string RequestorName { get; set; }

        /// <summary>
        /// List of purchase orders.
        /// </summary>
        public ReadOnlyCollection<PurchaseOrderSummary> PurchaseOrders { get; private set; }
        private readonly List<PurchaseOrderSummary> purchaseOrders = new List<PurchaseOrderSummary>();

        /// <summary>
        /// The blanket purchase order Id associated with the voucher
        /// </summary>
        public string BlanketPurchaseOrderId { get; set; }

        /// <summary>
        /// The blanket purchase order number associated with the voucher
        /// </summary>
        public string BlanketPurchaseOrderNumber { get; set; }

        /// <summary>
        /// This constructor initializes the purchase order summary domain entity
        /// </summary>
        /// <param name="id">Purchase order ID</param>
        /// <param name="number">Purchase order number</param>
        /// <param name="vendor name">Purchase order vendor name</param>        
        /// <param name="date">Purchase order date</param>
        /// <exception cref="ArgumentNullException">Thrown if any applicable parameters are null</exception>
        public VoucherSummary(string id, string number, string vendorName, DateTime date)
            : base(id, vendorName, date)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "Number is a required field.");
            }
            this.invoiceNumber = number;
            PurchaseOrders = this.purchaseOrders.AsReadOnly();
        }

        /// <summary>
        /// Add an associated purchase order to a voucher.
        /// </summary>
        /// <param name="purchaseOrder">The associated purchase order</param>
        public void AddPurchaseOrder(PurchaseOrderSummary purchaseOrder)
        {
            if (purchaseOrder == null)
                throw new ArgumentNullException("purchaseOrder", "purchaseOrder must have a value.");

            if (!purchaseOrders.Any(x => x.Id == purchaseOrder.Id))
            {
                this.purchaseOrders.Add(purchaseOrder);
            }
        }

    }
}
