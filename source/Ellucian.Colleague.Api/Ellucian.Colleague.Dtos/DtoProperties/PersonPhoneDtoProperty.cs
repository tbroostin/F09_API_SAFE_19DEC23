﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Information about a phone number.
    /// </summary>
    [DataContract]
    public class PersonPhoneDtoProperty
    {
        /// <summary>
        /// The <see cref="PhoneType">type</see> of phone number
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonPhoneTypeDtoProperty Type { get; set; }

        /// <summary>
        /// Specifies if the phone is preferred over others of the same type.
        /// </summary>
        [JsonProperty("preference", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonPreference? Preference { get; set; }

        /// <summary>
        /// The country calling code of telephone and/or mobile device when dialing internationally.
        /// </summary>
        [JsonProperty("countryCallingCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CountryCallingCode { get; set; }

        /// <summary>
        /// The phone number
        /// </summary>
        [JsonProperty("number", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Number { get; set; }

        /// <summary>
        /// The phone number extension
        /// </summary>
        [JsonProperty("extension", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Extension { get; set; }
    }
}