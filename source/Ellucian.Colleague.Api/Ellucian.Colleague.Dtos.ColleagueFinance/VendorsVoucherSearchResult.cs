// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Vendor's Information for voucher
    /// </summary>
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
        /// Vendor Country Code
        /// </summary>
        public string CountryCode { get; set; }

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

        /// <summary>
        /// Vendor default tax form.
        /// </summary>
        public string TaxForm { get; set; }

        /// <summary>
        /// Vendor default tax form code.
        /// </summary>
        public string TaxFormCode { get; set; }

        /// <summary>
        /// Vendor default tax form location.
        /// </summary>
        public string TaxFormLocation { get; set; }

        /// <summary>
        /// Withholding flag for 1099-NEC.
        /// </summary>
        public bool TaxForm1099NecWithholding { get; set; }

        /// <summary>
        /// Withholding flag for 1099-MISC.
        /// </summary>
        public bool TaxForm1099MiscWithholding { get; set; }

        /// <summary>
        /// Address type code
        /// </summary>
        public string AddressTypeCode { get; set; }

        /// <summary>
        /// Address type description
        /// </summary>
        public string AddressTypeDesc { get; set; }

        /// <summary>
        /// Vendor Ap Types
        /// </summary>
        public List<string> VendorApTypes { get; set; }

    }
}
