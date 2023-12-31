﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// A person's ethnicity properties required for governmental or other reporting.
    /// </summary>
    [DataContract]
    public class PersonEthnicityReporting
    {
        /// <summary>
        /// The country with specific reporting requirements
        /// </summary>
        [DataMember(Name="country")]
        public PersonEthnicityReportingCountry Country { get; set; }
    }

    /// <summary>
    /// A person's ethnicity reporting country and category.
    /// </summary>
    [DataContract]
    public class PersonEthnicityReportingCountry
    {
        /// <summary>
        /// Specific Country Code and category for reporting
        /// </summary>
        [JsonProperty("code")]
        public CountryCodeType Code { get; set; }

        /// <summary>
        /// The category of ethnic origin to which a person belongs.
        /// </summary>
        [JsonProperty("ethnicCategory", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonEthnicityCategory? EthnicCategory { get; set; }
    }
}