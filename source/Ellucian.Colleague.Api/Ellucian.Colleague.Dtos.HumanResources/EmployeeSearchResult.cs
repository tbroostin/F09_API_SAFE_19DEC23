// Copyright 2023 Ellucian Company L.P. an?d its affiliates.
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Information about the searched employee.
    /// </summary>
    [DataContract]
    public class EmployeeSearchResult
    {
        /// <summary>
        ///  Unique system ID of this person
        /// </summary>
        [JsonProperty("id")]
        [Metadata("ID", DataDescription = "Unique system ID of this person")]
        public string Id { get; set; }

        /// <summary>
        /// Person's last name
        /// </summary>
        [JsonProperty("lastName")]
        [Metadata("LAST.NAME", DataDescription = "Person's last name")]
        public string LastName { get; set; }

        /// <summary>
        /// Person's first name
        /// </summary>
        [JsonProperty("firstName")]
        [Metadata("FIRST.NAME", DataDescription = "Person's first name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Person's middle name
        /// </summary>
        [JsonProperty("middleName")]
        [Metadata("MIDDLE.NAME", DataDescription = "Person's middle name")]
        public string MiddleName { get; set; }

        /// <summary>
        /// Person's birth/maiden last name
        /// </summary>
        [JsonProperty("birthNameLast")]
        [Metadata("BIRTH.NAME.LAST", DataDescription = "Person's birth/maiden last name")]
        public string BirthNameLast { get; set; }

        /// <summary>
        /// Person's birth/maiden first name
        /// </summary>
        [JsonProperty("birthNameFirst")]
        [Metadata("BIRTH.NAME.FIRST", DataDescription = "Person's birth/maiden first name")] 
        public string BirthNameFirst { get; set; }

        /// <summary>
        /// Person's birth/maiden middle name
        /// </summary>
        [JsonProperty("birthNameMiddle")]
        [Metadata("BIRTH.NAME.MIDDLE", DataDescription = "Person's birth/maiden middle name")]
        public string BirthNameMiddle { get; set; }

        /// <summary>
        /// Calculated preferred name, based on this person's stated preference
        /// </summary>
        [JsonProperty("preferredName")]
        [Metadata(DataDescription = "Calculated preferred name, based on this person's stated preference")]
        public string PreferredName { get; set; }

        /// <summary>
        /// Prefixes
        /// </summary>
        [JsonProperty("prefix")]
        [Metadata("PREFIX", DataDescription = "Prefixes")]
        public string Prefix { get; set; }

        /// <summary>
        /// Suffixes
        /// </summary>
        [JsonProperty("suffix")]
        [Metadata("SUFFIX", DataDescription = "Suffixes")]
        public string Suffix { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        [JsonProperty("gender")]
        [Metadata("GENDER", DataDescription = "Gender")]
        public string Gender { get; set; }

        /// <summary>
        /// Personal Pronoun Code indicating person's perferred manner of address
        /// </summary>
        [JsonProperty("personalPronounCode")]
        [Metadata("PERSONAL.PRONOUN", DataDescription = "Personal Pronoun Code indicating person's perferred manner of address")]
        public string PersonalPronounCode { get; set; }

        /// <summary>
        /// Name that should be used when displaying a person's name on reports and forms.
        /// This property is based on a Name Address Hierarcy and will be null if none is provided.
        /// </summary>
        [JsonProperty("personDisplayName")]
        [Metadata(DataDescription = "This property is based on a Name Address Hierarcy and will be null if none is provided")]
        public PersonHierarchyName PersonDisplayName { get; set; }
    }
}
