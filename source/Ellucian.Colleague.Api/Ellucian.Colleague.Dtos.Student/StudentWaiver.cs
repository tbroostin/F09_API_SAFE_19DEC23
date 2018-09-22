// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A waiver for a student in a given course or section.
    /// </summary>
    public class StudentWaiver
    {
        /// <summary>
        /// A unique identifier for the waiver. Null if a new waiver.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Student this waiver belongs to
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The Id of the course for which waiver is created -- required if section Id not provided.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// The effective term of this waiver. 
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// The effective start date of this waiver. Used to apply the waiver to all sections over
        /// a given date range. Required only if term not identified.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The effective end date of this waiver. May be null to indicate waiver is open-ended.
        /// </summary>
        public DateTime? EndDate { get; set; }
      
        /// <summary>
        /// The ID of the section for which waiver is created -- provided if course not present
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Reason Code for this waiver -- required only if comment not provided
        /// </summary>
        public string ReasonCode { get; set; }

        /// <summary>
        /// Free-form comment for this waiver -- required only if ReasonCode not provided
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The Id of the person who authorized the waiver
        /// </summary>
        public string AuthorizedBy { get; set; }

        /// <summary>
        /// Date/time this waiver was last changed
        /// </summary>
        public DateTimeOffset DateTimeChanged { get; set; }

        /// <summary>
        /// Person who last changed this waiver
        /// </summary>
        public string ChangedBy { get; set; }

        /// <summary>
        /// Indicates whether the waiver has been revoked by the registrar and is no longer in effect
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// List of requisite waivers. Includes only requirement-based requisites.
        /// </summary>
        public List<RequisiteWaiver> RequisiteWaivers { get; set; }
    }
}
