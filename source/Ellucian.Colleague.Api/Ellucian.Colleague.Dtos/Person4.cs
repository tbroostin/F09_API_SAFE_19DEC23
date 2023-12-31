﻿// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a person HEDM version 6 model
    /// </summary>
    [DataContract]
    public class Person4 : BaseModel2
    {
        /// <summary>
        /// The <see cref="PrivacyStatus">privacy status</see> of the person's information
        /// </summary>
        [JsonProperty("privacyStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonPrivacyDtoProperty PrivacyStatus { get; set; }

        /// <summary>
        /// <see cref="PersonNameDtoProperty">Names</see> of the person
        /// </summary>
        [JsonProperty("names")]
        public IEnumerable<PersonName2DtoProperty> PersonNames { get; set; }

        /// <summary>
        /// Date of birth
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("dateOfBirth", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Deceased date
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("dateDeceased", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? DeceasedDate { get; set; }

        /// <summary>
        /// The <see cref="GenderType2">gender type</see> of the person
        /// </summary>
        [JsonProperty("gender", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GenderType2? GenderType { get; set; }

        /// <summary>
        /// Globally unique identifier for religion
        /// </summary>
        [JsonProperty("religion", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Religion { get; set; }

        /// <summary>
        /// <see cref="PersonEthnicityDtoProperty">person ethnicity</see> object
        /// </summary>
        [JsonProperty("ethnicity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonEthnicityDtoProperty Ethnicity { get; set; }

        /// <summary>
        /// Collection of <see cref="PersonRaceDtoProperty">person races</see>
        /// </summary>
        [JsonProperty("races", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<PersonRaceDtoProperty> Races { get; set; }

        /// <summary>
        /// Collection of <see cref="PersonLanguageDtoProperty">person languages</see>
        /// </summary>
        [JsonProperty("languages", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<PersonLanguageDtoProperty> Languages { get; set; }

        /// <summary>
        /// <see cref="PersonMaritalStatusDtoProperty">person marital status</see> object
        /// </summary>
        [JsonProperty("maritalStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonMaritalStatusDtoProperty MaritalStatus { get; set; }

        /// <summary>
        /// <see cref="PersonVeteranStatusDtoProperty">person veteran status</see> object
        /// </summary>
        [JsonProperty("veteranStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonVeteranStatusDtoProperty VeteranStatus { get; set; }

        /// <summary>
        /// <see cref="PersonCitizenshipDtoProperty">person Citizenship Status</see> object
        /// </summary>
        [JsonProperty("citizenshipStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonCitizenshipDtoProperty CitizenshipStatus { get; set; }

        /// <summary>
        /// The ISO 3166-1 alpha-3 code of the country in which the person was born
        /// </summary>
        [JsonProperty("countryOfBirth", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CountryOfBirth { get; set; }

        /// <summary>
        /// The ISO 3166-1 alpha-3 code of the country in which the person is a citizen
        /// </summary>
        [JsonProperty("citizenshipCountry", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CitizenshipCountry { get; set; }

        /// <summary>
        /// <see cref="PersonRoleDtoProperty">Roles</see> of the person
        /// </summary>
        [JsonProperty("roles", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<PersonRoleDtoProperty> Roles { get; set; }

        /// <summary>
        /// Person's government issued <see cref="PersonIdentityDocument"> identity documents</see>
        /// </summary>
        [JsonProperty("identityDocuments", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<PersonIdentityDocument> IdentityDocuments { get; set; }

        /// <summary>
        /// <see cref="PersonCredential">Credentials</see> of the person
        /// </summary>
        [JsonProperty("credentials", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<Credential3DtoProperty> Credentials { get; set; }

        /// <summary>
        /// <see cref="Address">Addresses</see> of the person
        /// </summary>
        [JsonProperty("addresses", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<PersonAddressDtoProperty> Addresses { get; set; }

        /// <summary>
        /// <see cref="Phone">Phones</see> of the person
        /// </summary>
        [JsonProperty("phones", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<PersonPhoneDtoProperty> Phones { get; set; }

        /// <summary>
        /// <see cref="GuidObject">Email addresses</see> of the person
        /// </summary>
        [JsonProperty("emails", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<PersonEmailDtoProperty> EmailAddresses { get; set; }

        /// <summary>
        /// <see cref="GuidObject">Social Media information</see> for the person
        /// </summary>
        [JsonProperty("socialMedia", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<PersonSocialMediaDtoProperty> SocialMedia { get; set; }

        /// <summary>
        /// <see cref="GuidObject">Interests</see> for the person
        /// </summary>
        [JsonProperty("interests", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<GuidObject2> Interests { get; set; }

        /// <summary>
        /// Person constructor
        /// </summary>
        public Person4()
            : base()
        {
        }
    }
}