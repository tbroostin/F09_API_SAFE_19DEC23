// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System;

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
