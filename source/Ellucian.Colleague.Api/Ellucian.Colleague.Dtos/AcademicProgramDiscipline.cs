﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Academic Disciplines offered by an organization
    /// </summary>
    [DataContract]
    public class AcademicProgramDisciplines
    {
        /// <summary>
        /// An academic discipline associated with the academic program.
        /// </summary>
        [DataMember(Name = "discipline")]
        public GuidObject2 Discipline { get; set; }

        ///// <summary>
        ///// The institutional unit that administers the discipline (major, minor, concentration), typically a department within a school or college.
        ///// </summary>
        //[DataMember(Name = "administeringInstitutionUnits", EmitDefaultValue = false)]
        //public List<GuidObject2> AdministeringInstitutionUnit { get; set; }

        /// <summary>
        /// A list of academic disciplines that are subordinate to the discipline.
        /// </summary>
        [DataMember(Name = "subDisciplines", EmitDefaultValue = false)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
         public List<GuidObject2> SubDisciplines { get; set; }

        /// <summary>
        /// The academic disciplines offered as part of an academic program.
        /// </summary>
        public AcademicProgramDisciplines()
        {
            Discipline = new GuidObject2();
            //SubDisciplines = new List<GuidObject2>();
        }

    }

}