// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading
{
    /// <summary>
    /// Container for preliminary anonymous grading information for a course section
    /// </summary>
    [Serializable]
    public class SectionPreliminaryAnonymousGrading
    {
        /// <summary>
        /// Course section ID
        /// </summary>
        public string SectionId { get; private set; }

        private List<PreliminaryAnonymousGrade> primaryAnonymousGrades = new List<PreliminaryAnonymousGrade>();
        /// <summary>
        /// Collection of preliminary anonymous grades for the course section
        /// </summary>
        public ReadOnlyCollection<PreliminaryAnonymousGrade> AnonymousGradesForSection { get; private set; }

        private List<PreliminaryAnonymousGrade> crosslistedAnonymousGrades = new List<PreliminaryAnonymousGrade>();
        /// <summary>
        /// Collection of preliminary anonymous grades for any crosslisted course sections
        /// </summary>
        public ReadOnlyCollection<PreliminaryAnonymousGrade> AnonymousGradesForCrosslistedSections { get; private set; }

        private List<AnonymousGradeError> errors = new List<AnonymousGradeError>();
        /// <summary>
        /// Collection of preliminary anonymous grades for any crosslisted course sections
        /// </summary>
        public ReadOnlyCollection<AnonymousGradeError> Errors { get; private set; }

        /// <summary>
        /// Creates a <see cref="SectionPreliminaryAnonymousGrading"/> object
        /// </summary>
        /// <param name="courseSectionId">ID for the course section to which the preliminary anonymous grade information applies</param>
        /// <exception cref="ArgumentNullException">A course section ID is required when building course section preliminary anonymous grade information.</exception>
        public SectionPreliminaryAnonymousGrading(string courseSectionId)
        {
            if (string.IsNullOrEmpty(courseSectionId))
            {
                throw new ArgumentNullException("courseSectionId", "A course section ID is required when building course section preliminary anonymous grade information.");
            }

            SectionId = courseSectionId;
            AnonymousGradesForSection = primaryAnonymousGrades.AsReadOnly();
            AnonymousGradesForCrosslistedSections = crosslistedAnonymousGrades.AsReadOnly();
            Errors = errors.AsReadOnly();
        }

        /// <summary>
        /// Add a preliminary anonymous grade for the course section
        /// </summary>
        /// <param name="gradeToAdd">Preliminary anonymous grade to be added</param>
        /// <exception cref="ArgumentNullException">A preliminary anonymous grade for a course section cannot be null.</exception>
        /// <exception cref="ArgumentNullException">Cannot add section anonymous grade data when grade course section ID does not match course section ID.</exception>
        public void AddAnonymousGradeForSection(PreliminaryAnonymousGrade gradeToAdd)
        {
            if (gradeToAdd == null)
            {
                throw new ArgumentNullException("gradeToAdd", "A preliminary anonymous grade for a course section cannot be null.");
            }
            if (gradeToAdd.CourseSectionId != SectionId)
            {
                throw new ColleagueException(string.Format("Cannot add section anonymous grade data; grade course section ID {0} does not match course section ID {1}.",
                    gradeToAdd.CourseSectionId,
                    SectionId));
            }
            primaryAnonymousGrades.Add(gradeToAdd);
        }

        /// <summary>
        /// Add a preliminary anonymous grade for the course section
        /// </summary>
        /// <param name="gradeToAdd">Preliminary anonymous grade to be added</param>
        /// <exception cref="ArgumentNullException">A preliminary anonymous grade for a crosslisted course section cannot be null.</exception>
        /// <exception cref="ArgumentNullException">Cannot add crosslisted section anonymous grade data when grade course section ID matches course section ID.</exception>
        public void AddAnonymousGradeForCrosslistedSection(PreliminaryAnonymousGrade gradeToAdd)
        {
            if (gradeToAdd == null)
            {
                throw new ArgumentNullException("gradeToAdd", "A preliminary anonymous grade for a crosslisted course section cannot be null.");
            }
            if (gradeToAdd.CourseSectionId == SectionId)
            {
                throw new ColleagueException(string.Format("Cannot add crosslisted section anonymous grade data; grade course section ID {0} matches course section ID {1}.",
                    gradeToAdd.CourseSectionId,
                    SectionId));
            }
            crosslistedAnonymousGrades.Add(gradeToAdd);
        }

        /// <summary>
        /// Add a preliminary anonymous grade error for the course section
        /// </summary>
        /// <param name="errorToAdd">Preliminary anonymous grade error to be added</param>
        /// <exception cref="ArgumentNullException">A preliminary anonymous grade error cannot be null.</exception>
        public void AddError(AnonymousGradeError errorToAdd)
        {
            if (errorToAdd == null)
            {
                throw new ArgumentNullException("gradeToAdd", "A preliminary anonymous grade error cannot be null.");
            }
            errors.Add(errorToAdd);
        }
    }
}
