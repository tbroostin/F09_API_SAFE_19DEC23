// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Domain entity class for PurchaseOrderReceipt
    /// </summary>
    [Serializable]
    public class PurchaseOrderReceipt
    {
       private string guid;

        public string Guid
        {
            get { return guid; }
            set
            {
                if (string.IsNullOrEmpty(guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

        public string Recordkey { get; set; }

        /// <summary>
        /// Key to the PURCHASE.ORDERS file for the purchase order whose line items are being received. 
        /// (Note: it's the sequential key, not the human-readable PO.NO.)
        /// </summary>
        public string PoId { get; set; }

        /// <summary>
        /// Free-form text containing the packing slip number.
        /// </summary>
        public string PackingSlip { get; set; }

        /// <summary>
        /// Date the goods were received.
        /// </summary>
        public DateTime? ReceivedDate { get; set; }

        /// <summary>
        /// PERSON ID of the person who received the goods
        /// </summary>
        public string ReceivedBy { get; set; }

        /// <summary>
        /// Key to the SHIP.VIAS file representing how the goods were shipped
        /// </summary>
        public string ArrivedVia { get; set; }

        /// <summary>
        /// Receipt-level (as opposed to item-level) comments
        /// </summary>
        public string ReceivingComments { get; set; }

        //List of PO.RECEIPT.ITEM.INTG for all the line items on this receipt transaction.
        public List<PurchaseOrderReceiptItem> ReceiptItems { get; set; }

        /// <summary>
        /// This constructor initializes the domain entity.
        /// </summary>
        /// <param name="guid"></param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public PurchaseOrderReceipt(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Guid is a required field when creating a PurchaseOrderReceipt");
            }

            this.guid = guid;
        }
    } 
}