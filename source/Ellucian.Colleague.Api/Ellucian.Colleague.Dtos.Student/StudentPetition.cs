// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Dto for Student Petition and Faculty Consent
    /// </summary>
    public class StudentPetition
    {
        /// <summary>
        /// A unique identifier for the student petition. Null if a new petition or consent.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Student this petition or consent belongs to
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Course Id
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Required. The ID of the section for which petition or consent is created.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Required. Status Code for this petition or consent
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Reason Code for this petition -- required only if comment not provided
        /// </summary>
        public string ReasonCode { get; set; }

        /// <summary>
        /// Free-form comment for this petition -- required only if ReasonCode not provided
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The name of the person who last updated the petition or consent.Ignored if a new petition or consent.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// The Id of the person who set the petition or consent
        /// </summary>
        public string SetBy { get; set; }

        /// <summary>
        /// Date/time this petition or consent was last changed. Ignored if a new petition or consent.
        /// </summary>
        public DateTimeOffset DateTimeChanged { get; set; }

        /// <summary>
        /// Term code associated to the petition. Used if the petition is associated to a course and not a section. 
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Start Date associated to the petition. 
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date associated to the petition.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Type of Student Petition (faculty consent or regular petition)
        /// </summary>
        public StudentPetitionType Type { get; set; }
    }
}
