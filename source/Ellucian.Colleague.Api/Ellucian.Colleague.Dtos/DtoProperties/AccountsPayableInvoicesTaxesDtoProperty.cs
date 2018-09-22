// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The taxes that are applicable to the line item if different from the invoice.
    /// </summary>
    [DataContract]
    public class AccountsPayableInvoicesTaxesDtoProperty
    {
        /// <summary>
        /// A tax code that applies to the line item.
        /// </summary>
        [JsonProperty("taxCode")]
        public GuidObject2 TaxCode { get; set; }

        /// <summary>
        /// The amount of taxes for the line item based on the tax code as specified by the vendor.
        /// </summary>
        [JsonProperty("vendorAmount")]
        public Amount2DtoProperty VendorAmount{ get; set; }
    }
}