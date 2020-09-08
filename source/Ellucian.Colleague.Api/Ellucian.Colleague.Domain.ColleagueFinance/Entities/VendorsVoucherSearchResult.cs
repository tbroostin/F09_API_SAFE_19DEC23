using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Vendor's Information for Voucher
    /// </summary>
    [Serializable]
    public class VendorsVoucherSearchResult
    {
        /// <summary>
        /// The vendor id for voucher
        /// </summary>        
        public string VendorId { get; set; }

        /// <summary>
        /// The voucher vendor name in multiple lines
        /// </summary>
        public List<string> VendorNameLines { get; set; }

        /// <summary>
        /// The voucher vendor miscellaneous name
        /// </summary>
        public string VendorMiscName { get; set; }

        /// <summary>
        /// Vendor Address Lines
        /// </summary>
        public List<string> AddressLines { get; set; }

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
        /// Vendor Country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Formatted Address
        /// </summary>
        public string FormattedAddress { get; set; }

        /// <summary>
        /// Address Id
        /// </summary>
        public string AddressId { get; set; }

        /// <summary>
        /// Identifies whether the vendor information is for miscellaneous vendor or not
        /// </summary>
        public bool MiscVendor { get; set; }

        /// <summary>
        /// Identifies whether the vendor information is reimbursed for himself or not
        /// </summary>
        public bool ReImburseMyself { get; set; }

        /// <summary>
        /// Flag to differentiate us/canada and non-us/canada address
        /// </summary>
        public bool IsInternationalAddress { get; set; }

    }
}
