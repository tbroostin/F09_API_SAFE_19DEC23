﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Smallest unit of instruction that can lead to granting credits
    /// </summary>
    [DataContract]
    public class Course2 :BaseModel2
    {
        /// <summary>
        /// Human-readable name
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Human-readable description
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Globally unique identifier for course subject
        /// </summary>
        [DataMember(Name = "subject")]
        public GuidObject2 Subject { get; set; }

        /// <summary>
        /// Collection of globally unique identifiers for course levels
        /// </summary>
        [DataMember(Name = "courseLevels")]
        public List<GuidObject2> CourseLevels { get; set; }

        /// <summary>
        /// Collection of globally unique identifiers for course instructional methods
        /// </summary>
        [DataMember(Name = "instructionalMethods")]
        public List<GuidObject2> InstructionMethods { get; set; }

        /// <summary>
        /// Collection of organizations that own, or are responsible for, a course, and their percentages of ownership
        /// </summary>
        [DataMember(Name = "owningOrganizations")]
        public List<OfferingOrganization2> OwningOrganizations { get; set; }

        /// <summary>
        /// The starting date at which a Course may have Sections created and scheduled to be taken
        /// </summary>
        //[JsonConverter(typeof(DateOnlyConverter))]
        [DataMember(Name = "schedulingStartOn")]
        public DateTime EffectiveStartDate { get; set; }

        /// <summary>
        /// The ending date at which a Course may no longer have Sections created nor scheduled to be taken
        /// </summary>
        //[JsonConverter(typeof(DateOnlyConverter))]
        [DataMember(Name = "schedulingEndOn", EmitDefaultValue=false)]
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>
        /// A numbering scheme to distinguish different Courses
        /// </summary>
        [DataMember(Name = "number")]
        public string Number { get; set; }

        /// <summary>
        /// Collection of globally unique identifiers for course academic levels
        /// </summary>
        [DataMember(Name = "academicLevels")]
        public List<GuidObject2> AcademicLevels { get; set; }

        /// <summary>
        /// Collection of globally unique identifiers for course grade schemes
        /// </summary>
        [DataMember(Name = "gradeSchemes")]
        public List<GuidObject2> GradeSchemes { get; set; }

        /// <summary>
        /// Credit information for the course
        /// </summary>
        [DataMember(Name = "credits")]
        public List<Credit2> Credits { get; set; }

        /// <summary>
        /// Constructor for Course
        /// </summary>
        public Course2()
            : base()
        {
            Subject = new GuidObject2(null);
            CourseLevels = new List<GuidObject2>();
            InstructionMethods = new List<GuidObject2>();
            OwningOrganizations = new List<OfferingOrganization2>();
            AcademicLevels = new List<GuidObject2>();
            GradeSchemes = new List<GuidObject2>();
            Credits = new List<Credit2>();
        }
    }
}