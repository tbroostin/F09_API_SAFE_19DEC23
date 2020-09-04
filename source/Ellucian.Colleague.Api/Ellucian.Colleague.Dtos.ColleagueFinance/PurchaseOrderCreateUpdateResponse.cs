using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Represents a response of create / update new purchase Order
    /// </summary>
    public class PurchaseOrderCreateUpdateResponse
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
        /// The Purchase Order date
        /// </summary>
        public DateTime PurchaseOrderDate { get; set; }

        /// <summary>
        /// Flag to determine if error occured while creating purchase Order
        /// </summary>
        public bool ErrorOccured { get; set; }
        /// <summary>
        /// Errors
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Flag to determine if warning raised while creating purchase Order
        /// </summary>
        public bool WarningOccured { get; set; }

        /// <summary>
        /// Warnings
        /// </summary>
        public List<string> WarningMessages { get; set; }
    }
}
