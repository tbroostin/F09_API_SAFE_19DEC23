﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// An indication of wheather the vendor is a person or an organization
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ManualVendorType
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        //NotSet = 0,


        /// <summary>
        /// person
        /// </summary>
        [EnumMember(Value = "person")]
        Person,

        /// <summary>
        /// organization
        /// </summary>
        [EnumMember(Value = "organization")]
        Organization
    }
}


