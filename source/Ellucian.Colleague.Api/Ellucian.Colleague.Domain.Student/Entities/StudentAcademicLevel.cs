// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Academic Level information for a student.
    /// </summary>
    [Serializable]
    public class StudentAcademicLevel
    {
        /// <summary>
        /// Academic Level code
        /// </summary>
        public String AcademicLevel { get; set; }
        /// <summary>
        /// Admit Status
        /// </summary>
        public String AdmitStatus { get; set; }
        /// <summary>
        /// Class level code
        /// </summary>
        public String ClassLevel { get; set; }
        /// <summary>
        /// Starting term
        /// </summary>
        public String StartTerm { get; set; }
        /// <summary>
        /// List of academic credits
        /// </summary>
        public List<String> AcademicCredits { get; set; }
        /// <summary>
        /// IsActive boolean, true if currentTerm take on input and student acad level
        /// dates intersect with that term.  Set to false if no currentTerm on input
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// List of StudentAcademicLevelCohort
        /// </summary>
        public List<StudentAcademicLevelCohort> StudentAcademicLevelCohorts { get; set; }

        /// <summary>
        /// Student's academic level start date
        /// </summary>
        public DateTime? StudentAcademicLevelStartDate { get; private set; }
        /// <summary>
        /// Student's academic level end date
        /// </summary>
        public DateTime? StudentAcademicLevelEndDate { get; private set; }


        public StudentAcademicLevel(string academicLevel, string admitStatus, string classLevel, string startTerm, IEnumerable<string> academicCredits, bool isActive)
        {
            if (string.IsNullOrEmpty(academicLevel))
            {
                throw new ArgumentNullException("academicLevel", "A Student Academic Level must have an academic level.");
            }
            this.AcademicLevel = academicLevel;
            this.AdmitStatus = !String.IsNullOrWhiteSpace(admitStatus) ? admitStatus : null;
            this.ClassLevel = !String.IsNullOrWhiteSpace(classLevel) ? classLevel : null;
            this.StartTerm = !String.IsNullOrWhiteSpace(startTerm) ? startTerm : null;
            this.AcademicCredits = academicCredits != null ? academicCredits.ToList() : new List<String>();
            this.IsActive = isActive;
        }

        public StudentAcademicLevel(string academicLevel, string admitStatus, string classLevel, string startTerm, IEnumerable<string> academicCredits, bool isActive,
            DateTime? studentAcadLevelStartDate, DateTime? studentAcadLevelEndDate):this(academicLevel, admitStatus,  classLevel,  startTerm,  academicCredits,  isActive)
        {
            if (string.IsNullOrEmpty(academicLevel))
            {
                throw new ArgumentNullException("academicLevel", "A Student Academic Level must have an academic level.");
            }
             this.StudentAcademicLevelStartDate = studentAcadLevelStartDate;
            this.StudentAcademicLevelEndDate = studentAcadLevelEndDate;
        }
    }
}