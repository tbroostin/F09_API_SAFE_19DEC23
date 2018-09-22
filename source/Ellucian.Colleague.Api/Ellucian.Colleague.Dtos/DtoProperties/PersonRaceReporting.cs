// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// A person's race properties required for governmental or other reporting.
    /// </summary>
    [DataContract]
    public class PersonRaceReporting
    {
        /// <summary>
        /// The country with specific reporting requirements
        /// </summary>
        [JsonProperty("country")]
        public PersonRaceReportingCountry Country { get; set; }
    }

    /// <summary>
    /// A person's race reporting country and category.
    /// </summary>
    [DataContract]
    public class PersonRaceReportingCountry
    {
        /// <summary>
        /// Specific Country Code and category for reporting
        /// </summary>
        [JsonProperty("code")]
        public CountryCodeType? Code { get; set; }

        /// <summary>
        /// The category of race to which a person belongs.
        /// </summary>
        [JsonProperty("racialCategory", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonRaceCategory? RacialCategory { get; set; }
    }
}