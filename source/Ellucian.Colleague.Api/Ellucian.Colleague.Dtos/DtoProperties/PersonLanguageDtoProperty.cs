// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The languagues the person has certain degree of proficiency in.
    /// </summary>
    [DataContract]
    public class PersonLanguageDtoProperty
    {
        /// <summary>
        /// The ISO 639-3 alpha-3 languague code
        /// </summary>
        [JsonProperty("code", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonLanguageCode? Code { get; set; }

        /// <summary>
        /// Language preference indicator.  Only one language should be set to primary for a person.
        /// </summary>
        [JsonProperty("preference", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonLanguagePreference? Preference { get; set; }
    }
}