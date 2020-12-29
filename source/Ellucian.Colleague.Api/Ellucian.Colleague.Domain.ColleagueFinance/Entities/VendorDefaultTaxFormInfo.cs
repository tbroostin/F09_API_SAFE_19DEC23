// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Vendor default tax info
    /// </summary>
    [Serializable]
    public class VendorDefaultTaxFormInfo
    {
        /// <summary>
        /// vendor ID.
        /// </summary>
        public string VendorId { get { return vendorId; } }

        /// <summary>
        /// Private variable for the vendor Id.
        /// </summary>
        private readonly string vendorId;

        /// <summary>
        /// Default Tax form.
        /// </summary>
        public string TaxForm { get; set; }

        /// <summary>
        /// Default Tax form box code.
        /// </summary>
        public string TaxFormBoxCode { get; set; }

        /// <summary>
        /// Default Tax form state / location.
        /// </summary>
        public string TaxFormState { get; set; }
        

        public VendorDefaultTaxFormInfo(string vendorId)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                throw new ArgumentNullException("vendorId", "VendorId is required field.");
            }
            this.vendorId = vendorId;
        }
    }
}
