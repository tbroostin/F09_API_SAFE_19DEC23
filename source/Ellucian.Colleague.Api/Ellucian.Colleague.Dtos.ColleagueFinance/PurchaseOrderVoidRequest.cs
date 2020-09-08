using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Represents a request to void purchase Order
    /// </summary>
    public class PurchaseOrderVoidRequest
    {
        /// <summary>
        /// Person ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Purchase Order ID
        /// </summary>
        public string PurchaseOrderId { get; set; }

        /// <summary>
        ///  ConfirmationEmailAddresses
        /// </summary>
        public string ConfirmationEmailAddresses { get; set; }

        /// <summary>
        /// The purchase order internal comments
        /// </summary>
        public string InternalComments { get; set; }
    }
}
