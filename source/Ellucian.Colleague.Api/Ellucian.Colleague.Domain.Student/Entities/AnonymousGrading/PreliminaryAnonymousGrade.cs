// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading
{
    /// <summary>
    /// Preliminary anonymous grade
    /// </summary>
    [Serializable]
    public class PreliminaryAnonymousGrade
    {
        /// <summary>
        /// Anonymous grading ID for the preliminary anonymous grade
        /// </summary>
        /// <remarks>
        /// Depending on the institution's setting for how to generate random IDs, this could be either:
        /// - (S)ection: a random grading ID assigned for a student on a specific course section
        /// - (T)erm: a random grading ID assigned for a student for a specific academic term and academic level
        /// </remarks>
        public string AnonymousGradingId { get; private set; }
        /// <summary>
        /// Anonymous grading ID for MidTerm grades
        /// </summary>
        public string AnonymousMidTermGradingId { get; private set; }

        /// <summary>
        /// ID for the final grade for the preliminary anonymous grade
        /// </summary>
        /// <remarks>
        /// Preliminary anonymous grades are stored in an interim table and only become "true" final grades once they are posted.
        /// If a student has a posted final grade (i.e. a final grade recorded on the academic credit for the student/course section) then the preliminary anonymous grade is no longer relevant.
        /// </remarks>
        public string FinalGradeId { get; private set; }

        /// <summary>
        /// ID for the course section to which the preliminary anonymous grade applies
        /// </summary>
        public string CourseSectionId { get; private set; }

        /// <summary>
        /// ID of the associated student course section data
        /// </summary>
        /// <remarks>This ID is synonymous with the record for preliminary student grade work data, as it is a shared ID.</remarks>
        public string StudentCourseSectionId { get; private set; }

        /// <summary>
        /// Optional date on which the associated final grade expires
        /// </summary>
        public DateTime? FinalGradeExpirationDate { get; private set; }

        /// <summary>
        /// Creates a <see cref="PreliminaryAnonymousGrade"/> object
        /// </summary>
        /// <param name="anonymousGradingId">Anonymous grading ID for the preliminary anonymous grade</param>
        /// <param name="finalGradeId">ID for the final grade for the preliminary anonymous grade</param>
        /// <param name="courseSectionId">ID for the course section to which the preliminary anonymous grade applies</param>
        /// <param name="studentCourseSectionId">ID of the associated student course section data</param>
        /// <param name="finalGradeExpirationDate">Optional date on which the associated final grade expires</param>
        /// <exception cref="ArgumentNullException">An anonymous grading ID is required when building preliminary anonymous grade information.</exception>
        /// <exception cref="ArgumentNullException">A course section ID is required when building preliminary anonymous grade information.</exception>
        public PreliminaryAnonymousGrade(string anonymousGradingId, string finalGradeId, string courseSectionId, string studentCourseSectionId, DateTime? finalGradeExpirationDate)
        {
            if (string.IsNullOrEmpty(anonymousGradingId))
            {
                throw new ArgumentNullException("anonymousGradingId", "An anonymous grading ID is required when building preliminary anonymous grade information.");
            }
            if (string.IsNullOrEmpty(courseSectionId))
            {
                throw new ArgumentNullException("courseSectionId", "A course section ID is required when building preliminary anonymous grade information.");
            }

            AnonymousGradingId = anonymousGradingId;
            FinalGradeId = finalGradeId;
            CourseSectionId = courseSectionId;
            StudentCourseSectionId = studentCourseSectionId;
            FinalGradeExpirationDate = finalGradeExpirationDate;
        }
        //with midterm anonymous grading id
        public PreliminaryAnonymousGrade(string anonymousGradingId, string anonymousMidTermGradingId,  string finalGradeId, string courseSectionId, string studentCourseSectionId, DateTime? finalGradeExpirationDate):this(anonymousGradingId, finalGradeId, courseSectionId,  studentCourseSectionId,  finalGradeExpirationDate)
        {
            this.AnonymousMidTermGradingId = anonymousMidTermGradingId;
        }
    }
}
