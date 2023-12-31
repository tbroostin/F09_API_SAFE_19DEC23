﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Organizations that are a place where people of different ages gain an education
    /// </summary>
    [DataContract]
    public class EducationalInstitution : BaseModel2
    {
        /// <summary>
        /// The human-readable name of a resource.
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// The human-readable description of a resource.
        /// </summary>
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// The type of the institution
        /// </summary>
        [DataMember(Name = "type")]
        [FilterProperty("criteria")]
        public Dtos.EnumProperties.EducationalInstitutionType Type { get; set; }

        /// <summary>
        /// Indicates if this is a 'home' or 'external' institution
        /// </summary>
        [DataMember(Name = "homeInstitution", EmitDefaultValue = false)]
        public Dtos.EnumProperties.HomeInstitutionType? HomeInstitution { get; set; }

        /// <summary>
        /// Standardized education specific codes/IDs that are assigned to the institution.
        /// </summary>
        [DataMember(Name = "standardizedCodes", EmitDefaultValue = false)]
        public IEnumerable<DtoProperties.StandardizedCodesDtoProperty> StandardizedCodes { get; set; }

        /// <summary>
        /// The physical locations where the institution resides or may be contacted by mail.
        /// </summary>
        [DataMember(Name = "addresses", EmitDefaultValue = false)]
        public IEnumerable<DtoProperties.PersonAddressDtoProperty> Addresses { get; set; }

        /// <summary>
        /// The phones and phone-connected devices at which the institution can be contacted.
        /// </summary>
        [DataMember(Name = "phones", EmitDefaultValue = false)]
        public IEnumerable<DtoProperties.PersonPhoneDtoProperty> Phones { get; set; }

        /// <summary>
        /// The email addresses at which the institution can be contacted.
        /// </summary>
        [DataMember(Name = "emails", EmitDefaultValue = false)]
        public IEnumerable<DtoProperties.PersonEmailDtoProperty> EmailAddresses { get; set; }

        /// <summary>
        /// The social media account at which the institution can be contacted.
        /// </summary>
        [DataMember(Name = "socialMedia", EmitDefaultValue = false)]
        public IEnumerable<DtoProperties.PersonSocialMediaDtoProperty> SocialMedia { get; set; }


        /// <summary>
        /// Credentials of the person
        /// </summary>
        [FilterProperty("criteria")]
        [JsonProperty("credentials", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<Credential3DtoProperty> Credentials { get; set; }


    }
}
