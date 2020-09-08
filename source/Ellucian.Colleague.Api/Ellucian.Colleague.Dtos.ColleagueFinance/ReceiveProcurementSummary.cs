using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is Receive Procurement Summary
    /// </summary>
    public class ReceiveProcurementSummary
    {
		/// <summary>
        /// purchase order ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The purchase order number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Puchase Order Line Item Information
        /// </summary>
        public List<LineItemSummary> LineItemInformation { get; set; }
        
        /// <summary>
        /// Vendor's detailed information
        /// </summary>
        public VendorInfo VendorInformation { get; set; }

        /// <summary>
        /// Reqiusition Summary
        /// </summary>
        public List<RequisitionLinkSummary> Requisitions {get; set;}

    }
    
}
