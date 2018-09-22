// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A list of units of the educational institution (optionally, hierarchical) that own or are responsible for a course 
    /// </summary>
    [DataContract]
    public class OwningInstitutionUnit
    {
        /// <summary>
        /// A School, College, Division, Department, or any other organizational unit in the institution
        /// </summary>
        [DataMember(Name = "institutionUnit")]
        [FilterProperty("criteria")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 InstitutionUnit { get; set; }

        /// <summary>
        /// The portion of a course that is owned or allocated to a particular organization.
        /// </summary>
        [DataMember(Name = "ownershipPercentage", EmitDefaultValue = false)]
        public decimal OwnershipPercentage { get; set; }

        /// <summary>
        /// constructor for property initialization
        /// </summary>
        public OwningInstitutionUnit()
        {
            InstitutionUnit = new GuidObject2();
        }
    }
}