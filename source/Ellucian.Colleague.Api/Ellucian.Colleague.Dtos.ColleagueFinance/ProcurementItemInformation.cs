using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Item information for receiving or returning
    /// </summary>
   public class ProcurementItemInformation
    {
        /// <summary>
        /// Purchase Order Id
        /// </summary>
        public string PurchaseOrderId { get; set; }

        /// <summary>
        /// Vendor Name
        /// </summary>
        public string Vendor { get; set; }

        /// <summary>
        /// Purchase Order Number
        /// </summary>
        public string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Item Id
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Item Description
        /// </summary>
        public string ItemDescription { get; set; }

        /// <summary>
        /// Quantity Ordered
        /// </summary>
        public Decimal QuantityOrdered { get; set; }

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

    }
}
