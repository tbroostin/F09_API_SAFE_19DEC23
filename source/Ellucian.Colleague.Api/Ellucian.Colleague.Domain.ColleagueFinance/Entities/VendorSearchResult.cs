// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Vendor search result
    /// </summary>
    [Serializable]
    public class VendorSearchResult
    {
        /// <summary>
        /// Document vendor ID.
        /// </summary>
        public string VendorId { get { return vendorId; } }

        /// <summary>
        /// Private variable for the vendor Id.
        /// </summary>
        private readonly string vendorId;

        /// <summary>
        /// Vendor Name.
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Vendor address.
        /// </summary>
        public string VendorAddress { get; set; }

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

        public VendorSearchResult(string vendorId)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                throw new ArgumentNullException("vendorId", "VendorId is required field.");
            }
            this.vendorId = vendorId;
        }
    }
}
