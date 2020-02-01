// Copyright 2019 Ellucian Company L.P. and its affiliates.

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
