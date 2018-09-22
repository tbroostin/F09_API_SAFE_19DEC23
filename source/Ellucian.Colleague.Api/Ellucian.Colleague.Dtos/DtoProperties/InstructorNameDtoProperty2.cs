// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Instructor Name DTO Property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class InstructorNameDtoProperty2
    {
        /// <summary>
        /// The <see cref="PersonNameType">type</see> of the person's name
        /// </summary>
        [JsonProperty("type")]
        public InstructorNameTypeDtoProperty NameType { get; set; }

        /// <summary>
        /// Person's title/prefix
        /// </summary>
        [JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        /// Person's first name
        /// </summary>
        [JsonProperty("firstName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FirstName { get; set; }

        /// <summary>
        /// Person's middle name
        /// </summary>
        [JsonProperty("middleName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string MiddleName { get; set; }

        /// <summary>
        /// Person's last name prefix
        /// </summary>
        [JsonProperty("lastNamePrefix", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LastNamePrefix { get; set; }

        /// <summary>
        /// Person's last name
        /// </summary>
        [JsonProperty("lastName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LastName { get; set; }

        /// <summary>
        /// Person's last name
        /// </summary>
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        /// <summary>
        /// Person's pedigree/suffix
        /// </summary>
        [JsonProperty("pedigree", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Pedigree { get; set; }

        /// <summary>
        /// Person's preferred, short-hand name/nickname
        /// </summary>
        [JsonProperty("preferredName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PreferredName { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        [JsonConstructor]
        public InstructorNameDtoProperty2()
        {

        }
    }
}
