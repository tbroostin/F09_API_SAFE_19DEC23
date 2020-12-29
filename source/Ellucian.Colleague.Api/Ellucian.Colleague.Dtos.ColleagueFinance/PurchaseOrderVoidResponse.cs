using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Represents a response of void purchase Order
    /// </summary>
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
        /// Flag to determine if error occured while voiding purchase Order
        /// </summary>
        public bool ErrorOccured { get; set; }
        /// <summary>
        /// Errors
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Flag to determine if warning raised while voiding purchase Order
        /// </summary>
        public bool WarningOccured { get; set; }

        /// <summary>
        /// Warnings
        /// </summary>
        public List<string> WarningMessages { get; set; }
    }
}
