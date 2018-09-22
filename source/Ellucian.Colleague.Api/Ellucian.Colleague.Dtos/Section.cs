// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// This is an instance of a course, offered once
    /// </summary>
    [DataContract]
    public class Section : BaseModel
    {
        /// <summary>
        /// Section number
        /// </summary>
        [DataMember(Name = "number")]
        public string Number { get; set; }

        /// <summary>
        /// Title of section
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Long description
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Section start date
        /// </summary>
        [DataMember(Name = "startDate")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Section end date
        /// </summary>
        [DataMember(Name = "endDate")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Course being offered
        /// </summary>
        [DataMember(Name = "course")]
        public GuidObject Course { get; set; }

        /// <summary>
        /// Credit information for the section
        /// </summary>
        [DataMember(Name = "credits")]
        public List<Credit> Credits { get; set; }

        /// <summary>
        /// The site where the section is held
        /// </summary>
        [DataMember(Name = "site")]
        public GuidObject Site { get; set; }

        /// <summary>
        /// A list of academic levels at which this section is offered
        /// </summary>
        [DataMember(Name = "academicLevels")]
        public List<GuidObject> AcademicLevels { get; set; }

        /// <summary>
        /// The grade scheme that applies to this section
        /// </summary>
        [DataMember(Name = "gradeSchemes")]
        public List<GuidObject> GradeSchemes { get; set; }

        /// <summary>
        /// A list of course levels associated with this section
        /// </summary>
        [DataMember(Name = "courseLevels")]
        public List<GuidObject> CourseLevels { get; set; }

        /// <summary>
        /// The current status of the section
        /// </summary>
        [DataMember(Name = "status")]
        public SectionStatus? Status { get; set; }

        /// <summary>
        /// The duration of the section
        /// </summary>
        [DataMember(Name = "duration")]
        public SectionDuration Duration { get; set; }

        /// <summary>
        /// The maximum enrollment for the section
        /// </summary>
        [DataMember(Name = "maxEnrollment")]
        public int? MaximumEnrollment { get; set; }

        /// <summary>
        /// A list of instructional events - time, date, room, etc.
        /// </summary>
        [DataMember(Name = "instructionalEvents")]
        public List<GuidObject> InstructionalEvents { get; set; }

        /// <summary>
        /// Information about the organizations who own this section
        /// </summary>
        [DataMember(Name = "owningOrganizations")]
        public List<OfferingOrganization> OwningOrganizations { get; set; }

        /// <summary>
        /// Section constructor
        /// </summary>
        public Section() : base()
        {
            Duration = new SectionDuration();
            Credits = new List<Credit>();
            AcademicLevels = new List<GuidObject>();
            GradeSchemes = new List<GuidObject>();
            CourseLevels = new List<GuidObject>();
            InstructionalEvents = new List<GuidObject>();
            OwningOrganizations = new List<OfferingOrganization>();
        }
    }
}
