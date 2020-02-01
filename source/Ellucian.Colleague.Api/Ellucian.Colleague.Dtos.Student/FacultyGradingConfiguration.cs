﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains information that controls the display and processing of faculty grading.
    /// </summary>
    public class FacultyGradingConfiguration
    {
        /// <summary>
        /// Should dropped and withdrawn students be included in the list of students to grade
        /// </summary>
        public bool IncludeDroppedWithdrawnStudents { get; set; }

        /// <summary>
        /// Should students in crosslisted sections be included in the list of students to grade
        /// </summary>
        public bool IncludeCrosslistedStudents { get; set; }

        /// <summary>
        /// Should grades be immediately verified.
        /// </summary>
        public bool? VerifyGrades { get; set; }
        /// <summary>
        /// List of terms open to faculty grading
        /// </summary>
        public IEnumerable<string> AllowedGradingTerms { get; set; }

        /// <summary>
        /// Number of midterm grades being stored and managed for each class related to faculty grading (0 - 6).
        /// </summary>
        public int NumberOfMidtermGrades { get; set; }

        /// <summary>
        /// When true, limit midterm grading to the allowed grading terms. 
        /// </summary>
        public bool LimitMidtermGradingToAllowedTerms { get; set; }        

        /// <summary>
        /// When true, provide the faculty member with a way to indicate that midterm grades are complete for a section and midterm grade number (one through six.)
        /// </summary>
        public bool ProvideMidtermGradingCompleteFeature { get; set; }

        /// <summary>
        /// When true, do not allow midterm grading for a given sectino and midterm grade number (one through six) after the faculty member has indicated that midterm grading is complete.
        /// </summary>
        public bool LockMidtermGradingWhenComplete { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FacultyGradingConfiguration()
        {
            AllowedGradingTerms = new List<string>();
        }
    }
}
