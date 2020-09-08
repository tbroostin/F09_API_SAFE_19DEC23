using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents a response entity of void purchase order
    /// </summary>
    [Serializable]
    public class PurchaseOrderVoidResponse
    {
        /// <summary>
        /// Purchase Order ID
        /// </summary>
        public string PurchaseOrderId { get; set; }

        /// <summary>
        /// The Purchase Order number
        /// </summary>
        public string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Flag to determine if error occured while voiding Purchase Order
        /// </summary>
        public bool ErrorOccured { get; set; }
        /// <summary>
        /// Errors
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Flag to determine if warning raised while voiding Purchase Order
        /// </summary>
        public bool WarningOccured { get; set; }

        /// <summary>
        /// Warnings
        /// </summary>
        public List<string> WarningMessages { get; set; }
    }
}
