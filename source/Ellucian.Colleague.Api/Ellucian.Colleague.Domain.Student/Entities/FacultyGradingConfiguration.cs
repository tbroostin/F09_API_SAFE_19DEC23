// Copyright 2016 Ellucian Company L.P. and its affiliates.
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
        /// Constructor for GraduationConfiguration
        /// </summary>
        public FacultyGradingConfiguration()
        {
            IncludeCrosslistedStudents = false;
            IncludeDroppedWithdrawnStudents = false;
            AllowedGradingTerms = _allowedGradingTerms.AsReadOnly();
        }

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
