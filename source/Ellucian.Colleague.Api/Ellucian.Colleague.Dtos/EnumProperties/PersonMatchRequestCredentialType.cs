﻿// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// Enumeration of possible instructor name types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PersonMatchRequestCredentialType
    {
        /// <summary>
        /// Default for when the enumeration is not set
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// SSN
        /// </summary>
        [EnumMember(Value = "ssn")]
        Ssn,

        /// <summary>
        /// SSN
        /// </summary>
        [EnumMember(Value = "sin")]
        Sin,

        /// <summary>
        /// Tax Identification Number
        /// </summary>
        [EnumMember(Value = "taxIdentificationNumber")]
        TaxIdentificationNumber
    }
}