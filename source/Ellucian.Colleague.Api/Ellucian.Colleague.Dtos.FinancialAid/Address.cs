/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Information about an aid application demographics
    /// </summary>
    [DataContract]
    public class Address
    {
        /// <summary>
        /// The student's permanent mailing address.
        /// </summary>        
        [JsonProperty("addressLine", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.ADDR", false, DataDescription = "The student's permanent mailing address.")]
        public string AddressLine { get; set; }

        /// <summary>
        /// The student's city.
        /// </summary>        
        [JsonProperty("city", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.CITY", false, DataDescription = "The student's city.")]
        public string City { get; set; }

        /// <summary>
        /// <see cref="RegionIsoCodes">The ISO code used to identify the region/state</see> of the student
        /// </summary>        
        [JsonProperty("state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.STATE", false, DataDescription = "The student's state of residence.")]
        public string State { get; set; }

        /// <summary>
        /// <see cref="CountryIsoCodes">The ISO code used to identify the country</see> of the student
        /// </summary>        
        [JsonProperty("country", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.COUNTRY", false, DataDescription = "The student's country for this aid application.")]
        public string Country { get; set; }

        /// <summary>
        /// The student's zip code.
        /// </summary>        
        [JsonProperty("zipCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.ZIP", false, DataDescription = "The student's zip code.")]
        public string ZipCode { get; set; }
    }
}
