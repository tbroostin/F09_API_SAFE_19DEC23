// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// The name components of a person
    /// </summary>
    [DataContract]
    public class PersonHierarchyName
    {
        /// <summary>
        /// Code of the hierarchy that was used to calculate this name object
        /// </summary>
        [JsonProperty("hierarchyCode")]
        [Metadata(DataDescription = "Code of the hierarchy that was used to calculate this name object")]
        public string HierarchyCode { get; set; }

        /// <summary>
        /// Full name calculated for this person based on the associated hierarchy code
        /// </summary>
        [JsonProperty("fullName")]
        [Metadata(DataDescription = "Full name calculated for this person based on the associated hierarchy code")]
        public string FullName { get; set; }

        /// <summary>
        /// The person's first name associated most closely with the hierarchy name.  
        /// </summary>
        [JsonProperty("firstName")]
        [Metadata(DataDescription = "The person's first name associated most closely with the hierarchy name")]
        public string FirstName { get; set; }

        /// <summary>
        /// The person's middle name associated most closely with the hierarchy name.
        /// </summary>
        [JsonProperty("middleName")]
        [Metadata(DataDescription = "The person's middle name associated most closely with the hierarchy name")]
        public string MiddleName { get; set; }

        /// <summary>
        /// The person's last name associated most closely with the hierarchy name. 
        /// </summary>
        [JsonProperty("lastName")]
        [Metadata(DataDescription = "The person's last name associated most closely with the hierarchy name")]
        public string LastName { get; set; }
    }
}
