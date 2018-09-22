// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// The type of related vendor.
    /// </summary>
    [JsonConverter(typeof (StringEnumConverter))]
    public enum VendorType
    {
        /// <summary>
        /// Payment Vendor
        /// </summary>
        [EnumMember(Value = "paymentVendor")] 
        PaymentVendor,

        /// <summary>
        /// Parent Vendor
        /// </summary>
        [EnumMember(Value = "parentVendor")] 
        ParentVendor

    }
}