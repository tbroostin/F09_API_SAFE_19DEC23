// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Academic Standing
    /// </summary>
    public class StudentStanding
    {
        /// <summary>
        /// Unique student standing Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Student Id for Academic Standing
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Academic Standing for this student
        /// </summary>
        public string StandingCode { get; set; }
        /// <summary>
        /// Academic Standing assigned on this date
        /// </summary>
        public DateTime? StandingDate { get; set; }
        /// <summary>
        /// Academic standing may be associated to a program
        /// </summary>
        public string Program { get; set; }
        /// <summary>
        /// Academic standing may be associated to an academic level
        /// </summary>
        public string Level { get; set; }
        /// <summary>
        /// Academic standing may be associated to a specific term
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// Academic Standing Type (Level, Program, or Term)
        /// </summary>
        public StudentStandingType Type { get; set; }
        /// <summary>
        /// Calculated academic Standing for this student
        /// </summary>
        public string CalcStandingCode { get; set; }
        /// <summary>
        /// Override reason for this student's standing
        /// </summary>
        public string OverrideReason { get; set; }
        /// <summary>
        /// IsCurrent set to True for most current standing for any term up through an input current term.
        /// If no current term provided on input, this will not be set.
        /// </summary>
        public bool IsCurrent { get; set; }
    }
}
