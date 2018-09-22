// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CurriculumConfiguration
    {
        /// <summary>
        /// Mapping for Course Subjects and Departments
        /// </summary>
        public ExternalMapping SubjectDepartmentMapping { get; set; }

        /// <summary>
        /// Status code assigned to active courses
        /// </summary>
        public string CourseActiveStatusCode { get; set; }

        /// <summary>
        /// Status code assigned to inactive courses
        /// </summary>
        public string CourseInactiveStatusCode { get; set; }

        /// <summary>
        /// Status code assigned to active courses
        /// </summary>
        public string SectionActiveStatusCode { get; set; }

        /// <summary>
        /// Status code assigned to inactive courses
        /// </summary>
        public string SectionInactiveStatusCode { get; set; }

        /// <summary>
        /// Default approving agency ID
        /// </summary>
        public string ApprovingAgencyId { get; set; }

        /// <summary>
        /// Default approver ID
        /// </summary>
        public string ApproverId { get; set; }

        /// <summary>
        /// Default credit type code
        /// </summary>
        public string DefaultCreditTypeCode { get; set; }

        /// <summary>
        /// Default instructional method code
        /// </summary>
        public string DefaultInstructionalMethodCode { get; set; }

        /// <summary>
        /// Default academic level code
        /// </summary>
        public string DefaultAcademicLevelCode { get; set; }

        /// <summary>
        /// Default course level code
        /// </summary>
        public string DefaultCourseLevelCode { get; set; }

        /// <summary>
        /// Default teaching arrangement
        /// </summary>
        public string DefaultTeachingArrangement { get; set; }

        /// <summary>
        /// Default Contract Type
        /// </summary>
        public string DefaultContractType { get; set; }

        /// <summary>
        /// Default contract position
        /// </summary>
        public string DefaultContractPosition { get; set; }

        /// <summary>
        /// Default Contract Load Period
        /// </summary>
        public string DefaultContractLoadPeriod { get; set; }

        /// <summary>
        /// Delimiter for course names
        /// </summary>
        public string CourseDelimiter { get; set; }

        /// <summary>
        /// Default waitlist rating
        /// </summary>
        public string WaitlistRatingCode { get; set; }

        /// <summary>
        /// Default for new courses: can be taken Pass/No Pass
        /// </summary>
        public bool AllowPassNoPass { get; set; }

        /// <summary>
        /// Default for new courses: can be audited
        /// </summary>
        public bool AllowAudit { get; set; }

        /// <summary>
        /// Default for new courses: can only be taken Pass/No Pass
        /// </summary>
        public bool OnlyPassNoPass { get; set; }

        /// <summary>
        /// Default for new courses: course allows waitlists
        /// </summary>
        public bool AllowWaitlist { get; set; }

        /// <summary>
        /// Default for new courses: student must get consent from the instructor
        /// </summary>
        public bool IsInstructorConsentRequired { get; set; }

        /// <summary>
        /// Indicates that requisites have been converted
        /// </summary>
        public bool AreRequisitesConverted { get; set; }
    }
}
