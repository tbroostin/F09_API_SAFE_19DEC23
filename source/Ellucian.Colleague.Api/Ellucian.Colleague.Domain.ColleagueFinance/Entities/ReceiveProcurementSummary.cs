using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Receive Procurement Summary entity
    /// </summary>
    [Serializable]
    public class ReceiveProcurementSummary
    {
        /// <summary>
        /// The purchase order id
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Private id for public getter
        /// </summary>
        private readonly string id;

        /// <summary>
        /// Private number for public getter
        /// </summary>
        private readonly string number;

        /// <summary>
        /// The purchase order number
        /// </summary>
        public string Number { get { return number; } }

        /// <summary>
        /// List of Puchase Order Line Item Information
        /// </summary>
        public List<LineItemSummary> LineItemInformation { get; set; }

        /// <summary>
        /// Vendor's detailed information
        /// </summary>
        public VendorInfo VendorInformation { get; set; }


        public List<RequisitionLinkSummary> Requisitions { get; set; }
        
        /// <summary>
        /// Parameterized contructor to instantiate a ReceiveProcurementSummary object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="number"></param>
        /// <param name="lineItemInformation"></param>
        /// <param name="vendorInformation"></param>
        public ReceiveProcurementSummary(string id, string number, List<LineItemSummary> lineItemInformation, VendorInfo vendorInformation, List<RequisitionLinkSummary> requisitions)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number");
            }
            if (lineItemInformation == null)
            {
                throw new ArgumentNullException("lineItemInformation");
            }
            if (!lineItemInformation.Any())
            {
                throw new ArgumentException("At least one lineItemInformation record is required for a ReceiveProcurementSummary object.");
            }
            if (vendorInformation == null) {
                throw new ArgumentNullException("vendorInformation");
            }
            this.id = id;
            this.number = number;
            LineItemInformation = lineItemInformation;
            VendorInformation = vendorInformation;
            Requisitions = requisitions;
        }

    }
}
