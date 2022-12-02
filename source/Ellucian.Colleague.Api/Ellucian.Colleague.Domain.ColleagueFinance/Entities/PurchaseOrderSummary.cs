// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Purchase Order Summary entity
    /// </summary>
    [Serializable]
    public class PurchaseOrderSummary : AccountsPayablePurchasingDocument
    {
        /// <summary>
        /// Private number for public getter
        /// </summary>
        private readonly string number;

        /// <summary>
        /// The purchase order number
        /// </summary>
        public string Number { get { return number; } }
        
        /// <summary>
        /// The purchase order submitted by operator for funds availability checking.
        /// </summary>
        public string SubmittedBy { get; set; }
        

        /// <summary>
        /// The purchase order current status
        /// </summary>
        public PurchaseOrderStatus? Status { get; set; }
        
        /// <summary>
        /// The purchase order initiator name
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// The purchase order requestor name
        /// </summary>
        public string RequestorName { get; set; }        

        /// <summary>
        /// List of requisitions.
        /// </summary>
        public ReadOnlyCollection<RequisitionSummary> Requisitions { get; private set; }
        private readonly List<RequisitionSummary> requisitions = new List<RequisitionSummary>();

        /// <summary>
        /// List of VoucherIds
        /// </summary>
        public ReadOnlyCollection<string> VoucherIds { get; private set; }
        private readonly List<string> voucherIds = new List<string>();

        /// <summary>
        /// This constructor initializes the purchase order summary domain entity
        /// </summary>
        /// <param name="id">Purchase order ID</param>
        /// <param name="number">Purchase order number</param>
        /// <param name="vendor name">Purchase order vendor name</param>        
        /// <param name="date">Purchase order date</param>
        /// <exception cref="ArgumentNullException">Thrown if any applicable parameters are null</exception>
        public PurchaseOrderSummary(string id, string number, string vendorName, DateTime date)
            : base(id, vendorName, date)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "Number is a required field.");
            }

            this.number = number;
            Requisitions = this.requisitions.AsReadOnly();
            VoucherIds = this.voucherIds.AsReadOnly();
        }

        /// <summary>
        /// Add an associated requisition to the purchase order.
        /// </summary>
        /// <param name="requisition">The associated requisition</param>
        public void AddRequisition(RequisitionSummary requisition)
        {
            if(requisition == null)
                throw new ArgumentNullException("requistion", "requistion must have a value.");

            if(!requisitions.Any(x=> x.Id == requisition.Id))
            {
                this.requisitions.Add(requisition);
            }
        }
        /// <summary>
        /// Add an associated voucherId to the purchase order.
        /// </summary>
        /// <param name="voucherId">The associated voucher id</param>
        public void AddVoucherId(string voucherId)
        {
            if (string.IsNullOrEmpty(voucherId))
                throw new ArgumentNullException("voucherId", "voucherId must have a value.");

            if (!voucherIds.Any(x => x == voucherId))
            {
                this.voucherIds.Add(voucherId);
            }
        }

    }
}
