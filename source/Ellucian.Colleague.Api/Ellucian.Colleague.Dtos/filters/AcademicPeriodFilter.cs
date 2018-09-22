﻿// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// academicCatalog named query
    /// </summary>
    [DataContract]
    public class AcademicPeriodFilter
    {

        /// <summary>
        /// Registration Status
        /// </summary>
        [DataMember(Name = "registration", EmitDefaultValue = false)]
        [FilterProperty("criteria")]
        public TermRegistrationStatus? RegistrationStatus { get; set; }
        /// <summary>
        /// Term Code
        /// </summary>
        [DataMember(Name = "code", EmitDefaultValue = false)]
        [FilterProperty("criteria")]
        public string Code { get; set; }

        /// <summary>
        /// The <see cref="AcademicPeriodCategory">Academic Period category</see>
        /// </summary>
        [DataMember(Name = "category", EmitDefaultValue = false)]
        [FilterProperty("criteria")]
        public AcademicPeriodCategory2Filter Category { get; set; }
    }

    /// <summary>
    /// Information about an Academic Period Category
    /// </summary>
    [DataContract]
    public class AcademicPeriodCategory2Filter
    {
        /// <summary>
        /// The <see cref="AcademicTimePeriod">Academic Time Period type</see>
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        [FilterProperty("criteria")]
        public AcademicTimePeriod2? Type { get; set; }
    }

}
