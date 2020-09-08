using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Procurement item information
    /// </summary>
    [Serializable]
    public class ProcurementItemInformation
    {
        private readonly string purchaseOrderId;
        /// <summary>
        /// Purchase Order Id
        /// </summary>
        public string PurchaseOrderId { get { return purchaseOrderId; } }

        /// <summary>
        /// Vendor Name
        /// </summary>
        public string Vendor { get; set; }

        private readonly string purchaseOrderNumber;
        /// <summary>
        /// Purchase Order Number
        /// </summary>
        public string PurchaseOrderNumber { get { return purchaseOrderNumber; } }

        private readonly string itemId;

        /// <summary>
        /// Item Id
        /// </summary>
        public string ItemId { get { return itemId; } }

        /// <summary>
        /// Item Description
        /// </summary>
        public string ItemDescription { get; set; }

        private readonly Decimal quantityOrdered;
        /// <summary>
        /// Quantity Ordered
        /// </summary>
        public Decimal QuantityOrdered { get { return quantityOrdered; } }

        /// <summary>
        /// Quantity Accepted
        /// </summary>
        public Decimal? QuantityAccepted { get; set; }

        /// <summary>
        /// Item MSDS flag
        /// </summary>
        public bool ItemMsdsFlag { get; set; }

        /// <summary>
        /// MSDS Received for item
        /// </summary>
        public bool ItemMsdsReceived { get; set; }

        /// <summary>
        /// Quantity Rejected
        /// </summary>
        public Decimal? QuantityRejected { get; set; }

        /// <summary>
        /// Return Date
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Return Via
        /// </summary>
        public string ReturnVia { get; set; }

        /// <summary>
        /// Return Authorization Number
        /// </summary>
        public string ReturnAuthorizationNumber { get; set; }

        /// <summary>
        /// Return Reason
        /// </summary>
        public string ReturnReason { get; set; }

        /// <summary>
        /// Return Comments
        /// </summary>
        public string ReturnComments { get; set; }

        /// <summary>
        /// Item Reordered
        /// </summary>
        public bool ReOrder { get; set; }

        /// <summary>
        /// Confirmation Email
        /// </summary>
        public string ConfirmationEmail { get; set; }

        public ProcurementItemInformation(string purchaseOrderId, string purchaseOrderNumber, string itemId, decimal quantityOrdered, decimal? quantityAccepted , decimal? quantityRejected, DateTime? returnDate,string returnReason) {

            if (string.IsNullOrEmpty(purchaseOrderId))
            {
                throw new ArgumentNullException("purchaseOrderId", "Purchase Order Id is a required field.");
            }

            if (string.IsNullOrEmpty(purchaseOrderNumber))
            {
                throw new ArgumentNullException("purchaseOrderNumber", "Purchase Order Number is a required field.");
            }

            if (string.IsNullOrEmpty(itemId))
            {
                throw new ArgumentNullException("itemId", "Item Id is a required field.");
            }

            if (quantityOrdered.Equals(0))
            {
                throw new ArgumentNullException("quantityOrdered", "Quantity Ordered  should have value.");
            }

            if(!(quantityAccepted.HasValue) && !(quantityRejected.HasValue))
            {
                throw new ArgumentNullException("quantityAccepted,quantityRejected", "Either Quantity Accepted or Quantity Rejected should have value.");
            }

            if (quantityAccepted.HasValue || quantityRejected.HasValue)
            {
                Decimal qtyAccepted = !string.IsNullOrEmpty(quantityAccepted.ToString()) ? quantityAccepted??0 : 0 ;
                Decimal qtyRejected = !string.IsNullOrEmpty(quantityRejected.ToString()) ? quantityRejected ?? 0 : 0;

                if(qtyAccepted+ qtyRejected <=0)

                throw new ArgumentNullException("quantityAccepted,quantityRejected", "Either Quantity Accepted or Quantity Rejected should have value.");
            }

            if (quantityRejected.HasValue)
            {
                if (!(returnDate.HasValue))
                {
                    throw new ArgumentNullException("returnDate", "Return Date is a required field.");
                }

                if (string.IsNullOrEmpty(returnReason))
                {
                    throw new ArgumentNullException("returnReason", "Return Reason is a required field.");
                }
            }

            this.itemId = itemId;
            this.purchaseOrderId = purchaseOrderId;
            this.purchaseOrderNumber = purchaseOrderNumber;
            this.quantityOrdered = quantityOrdered;
            this.ReturnReason = returnReason;
            this.ReturnDate = returnDate;
            this.QuantityAccepted = quantityAccepted;
            this.QuantityRejected = quantityRejected;
        }

    }
}
