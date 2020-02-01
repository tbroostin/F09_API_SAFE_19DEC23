// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Requisition Summary
    /// </summary>
    [Serializable]
    public class RequisitionSummary : AccountsPayablePurchasingDocument
    {
        /// <summary>
        /// Private number for public getter
        /// </summary>
        private readonly string number;

        /// <summary>
        /// The requisition number
        /// </summary>
        public string Number { get { return number; } }

        /// <summary>
        /// The requisition current status
        /// </summary>
        public RequisitionStatus Status { get; set; }
        
        /// <summary>
        /// The requisition current status date
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// The requisition initiator name
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// The requisition requestor name
        /// </summary>
        public string RequestorName { get; set; }

        /// <summary>
        /// The blanket purchase order Id associated with the requisition
        /// </summary>
        public string BlanketPurchaseOrderId { get; set; }

        /// <summary>
        /// The blanket purchase order number associated with the requisition
        /// </summary>
        public string BlanketPurchaseOrderNumber { get; set; }

        /// <summary>
        /// List of cost center GL components.
        /// </summary>
        public ReadOnlyCollection<PurchaseOrderSummary> PurchaseOrders { get; private set; }
        private readonly List<PurchaseOrderSummary> purchaseOrders = new List<PurchaseOrderSummary>();



        /// <summary>
        /// This constructor initializes the requisition domain entity
        /// </summary>
        /// <param name="id">Requisition ID</param>
        /// <param name="number">Requisition number</param>
        /// <param name="vendorName">Requisition vendor name</param>
        /// <param name="statusDate">Requisition status date</param>
        /// <param name="date">Requisition date</param>
        /// <exception cref="ArgumentNullException">Thrown if any applicable parameters are null</exception>
        public RequisitionSummary(string id, string number, string vendorName, DateTime date)
            : base(id, vendorName, date)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "Number is a required field.");
            }

            this.number = number;            
            PurchaseOrders = this.purchaseOrders.AsReadOnly();
        }


        /// <summary>
        /// This method adds a purchase order to the list of purchase orders for the requisition
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
