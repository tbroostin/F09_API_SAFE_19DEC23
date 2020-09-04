using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Vendor's Information for purchase order
    /// </summary>
    public class VendorInfo
    {
        /// <summary>
        /// The purchase order vendor id
        /// </summary>        
        public string VendorId { get; set; }

        /// <summary>
        /// The purchase order vendor name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// The purchase order vendor miscellaneous name
        /// </summary>
        public string VendorMiscName { get; set; }

        /// <summary>
        /// Vendor Address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Vendor City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Vendor State
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Vendor Zip
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// Vendort Country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// List of Vendor contact associated to the Vendor
        /// </summary>
        public List<VendorContactSummary> VendorContacts { get; set; }

    }
}
