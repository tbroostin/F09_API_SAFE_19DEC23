﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Academic Disciplines offered by an organization
    /// </summary>
    [DataContract]
    public class AcademicDiscipline2 : AbbreviationItem
    {
        /// <summary>
        /// The <see cref="AcademicDisciplineType">Academic Discipline type</see>
        /// </summary>
        [DataMember(Name = "type")]
        public AcademicDisciplineType Type { get; set; }
        /// <summary>
        /// Holds the reporting country (USA) and the CIP code of the major or minor, if available
        /// </summary>
        [DataMember(Name = "reporting", EmitDefaultValue = false)]
        public List<ReportingDtoProperty> Reporting { get; set; }
    }
}