// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that controls how faculty grading is handled.
    /// </summary>
    [Serializable]
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
        /// List of terms open for faculty grading
        /// </summary>
        public ReadOnlyCollection<string> AllowedGradingTerms { get; private set; }
        private readonly List<string> _allowedGradingTerms = new List<string>();
       
        private int _NumberOfMidtermGrades;
        /// <summary>
        /// Number of midterm grades being stored and managed for each class (0 - 6)
        /// </summary>
        public int NumberOfMidtermGrades 
        {
            get { return _NumberOfMidtermGrades; }
            set
            {
                if (value != null)
                {
                    if (value < 0 || value > 6)
                    {
                        throw new ArgumentOutOfRangeException("value", "Number Of Midter Grades must be a number between 0 and 6.");
                    }
                    _NumberOfMidtermGrades = value;
                }
            }
        }

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
        /// When true, do not allow faculty users to drop students who do not have a Last Date Attended and have not been flagged as never attending a course section.
        /// </summary>
        public bool RequireLastDateAttendedOrNeverAttendedFlagBeforeFacultyDrop { get; set; }

        /// <summary>
        /// Determines if and/or how the Last Date Attended / Never Attended field will be displayed for Final Grading in Colleague Self-Service
        /// </summary>
        public LastDateAttendedNeverAttendedFieldDisplayType FinalGradesLastDateAttendedNeverAttendedDisplayBehavior { get; set; }

        /// <summary>
        /// Determines if and/or how the Last Date Attended / Never Attended field will be displayed for Midterm Grading in Colleague Self-Service
        /// </summary>
        public LastDateAttendedNeverAttendedFieldDisplayType MidtermGradesLastDateAttendedNeverAttendedDisplayBehavior { get; set; }

        /// <summary>
        /// When true, display Pass/Audit column in Roster tab of Faculty
        /// </summary>
        public bool ShowPassAudit { get; set; }

        /// <summary>
        /// When true, display Repeated column in Roster tab of Faculty
        /// </summary>
        public bool ShowRepeated { get; set; }
        /// <summary>
        /// Deteermines if faculty is allowed to provide midterm or final grades to dropped or withdrawn students.
        /// </summary>
        public bool IsGradingAllowedForDroppedWithdrawnStudents { get; set; }
        /// <summary>
        /// Determines if faculty is allowed to provide midterm or final grades to students who never attended the class.
        /// </summary>
        public bool IsGradingAllowedForNeverAttendedStudents { get; set; }

        /// <summary>
        /// Constructor for GraduationConfiguration
        /// </summary>
        public FacultyGradingConfiguration()
        {
            IncludeCrosslistedStudents = false;
            IncludeDroppedWithdrawnStudents = false;
            AllowedGradingTerms = _allowedGradingTerms.AsReadOnly();
            LimitMidtermGradingToAllowedTerms = false;
            ProvideMidtermGradingCompleteFeature = false;
            LockMidtermGradingWhenComplete = false;
            FinalGradesLastDateAttendedNeverAttendedDisplayBehavior = LastDateAttendedNeverAttendedFieldDisplayType.Editable;
            MidtermGradesLastDateAttendedNeverAttendedDisplayBehavior = LastDateAttendedNeverAttendedFieldDisplayType.Editable;
            ShowPassAudit = false;
            ShowRepeated = false;
            IsGradingAllowedForDroppedWithdrawnStudents = true; //default is allowed
            IsGradingAllowedForNeverAttendedStudents = true; //default is allowed
        }

        /// <summary>
        /// Adds a grading term
        /// </summary>
        /// <param name="gradingTermCode">Term code for the grading term to add</param>
        public void AddGradingTerm(string gradingTermCode)
        {
            if (string.IsNullOrEmpty(gradingTermCode))
            {
                throw new ArgumentNullException("gradingTermCode", "Grading Term Code must be specified");
            }
            if (!AllowedGradingTerms.Any(r => r.Equals(gradingTermCode)))
            {
                _allowedGradingTerms.Add(gradingTermCode);
            }
        }
    }
}
