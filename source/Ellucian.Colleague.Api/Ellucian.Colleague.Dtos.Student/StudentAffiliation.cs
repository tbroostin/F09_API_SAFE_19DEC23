// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Affiliation information
    /// </summary>
    public class StudentAffiliation
    {
        /// <summary>
        /// Reference to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Term associated to this student data
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// Code of the organization or affiliation
        /// </summary>
        public string AffiliationCode { get; set; }
        /// <summary>
        /// Name of the organization or affiliation
        /// </summary>
        public string AffiliationName { get; set; }
        /// <summary>
        /// Role of this person within this organization
        /// </summary>
        public string RoleCode { get; set; }
        /// <summary>
        /// Name or Description of the Role
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// The Role for this person starts on this date
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// The Role for this person ends on this date
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// The Status of this member within this organization
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// The Status Name for the Status Code
        /// </summary>
        public string StatusName { get; set; }
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public StudentAffiliation()
        {

        }
    }
}
