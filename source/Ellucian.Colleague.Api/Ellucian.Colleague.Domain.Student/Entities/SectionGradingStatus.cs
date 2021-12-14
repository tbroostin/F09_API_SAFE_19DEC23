// Copyright 2021 Ellucian Company L.P. and its affiliates
using Ellucian.Colleague.Domain.Base.Exceptions;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Course section grading status
    /// </summary>
    /// <remarks>When all of the final grades for a course section have been posted, Colleague records the date and time that the last final grade was posted along with the person ID of the user who posted it.</remarks>
    [Serializable]
    public class SectionGradingStatus
    {
        /// <summary>
        /// The date on which the last final grade was posted for the course section
        /// </summary>
        public DateTime? FinalGradesPostedDate { get; private set; }

        /// <summary>
        /// The time of day at which the last final grade was posted for the course section
        /// </summary>
        public DateTimeOffset? FinalGradesPostedTime { get; private set; }

        /// <summary>
        /// Unique identifier for the person who posted the last final grade for the course section
        /// </summary>
        public string GradesPostedByPersonId { get; private set; }

        /// <summary>
        /// Unique identifier for the course section
        /// </summary>
        public string CourseSectionId { get; private set; }

        /// <summary>
        /// Creates a new <see cref="SectionGradingStatus"/>
        /// </summary>
        /// <param name="courseSectionId">Unique identifier for the course section</param>
        /// <param name="gradesPostedByPersonId">Unique identifier for the person who posted the last final grade for the course section</param>
        /// <param name="finalGradesPostedDate">The date on which the last final grade was posted for the course section</param>
        /// <param name="finalGradesPostedTime">The time of day at which the last final grade was posted for the course section</param>
        /// <remarks>
        /// Course section grading status does not NEED a date, time, and person ID; if a course section has not had all of its final grades posted then this data would not exist;
        /// however, if one of these pieces of data is provided, then all three must be provided together. And none of these pieces of data can be provided without the other two.</remarks>
        /// <exception cref="ArgumentNullException">A course section ID is required when building course section grading status.</exception>
        /// <exception cref="ColleagueException">All three of date, time, and person ID - or none of them - must be provided when building course section grading status.</exception>
        public SectionGradingStatus(string courseSectionId, string gradesPostedByPersonId, DateTime? finalGradesPostedDate, DateTimeOffset? finalGradesPostedTime)
        {
            if (string.IsNullOrEmpty(courseSectionId))
            {
                throw new ArgumentNullException("courseSectionId", "A course section ID is required when building course section grading status.");
            }
            if (finalGradesPostedDate.HasValue)
            {
                if (!finalGradesPostedTime.HasValue)
                {
                    throw new ColleagueException("If a date is provided, then a time of day is also required when building course section grading status.");
                }
                if (string.IsNullOrWhiteSpace(gradesPostedByPersonId))
                {
                    throw new ColleagueException("If a date and time are provided, then the ID of the person who posted the last final grade for the course section is required when building course section grading status.");
                }
            } 
            else
            {
                if (finalGradesPostedTime.HasValue)
                {
                    throw new ColleagueException("A time of day cannot be provided without an associated date when building course section grading status.");
                }
                if (!string.IsNullOrWhiteSpace(gradesPostedByPersonId))
                {
                    throw new ColleagueException("The ID of the person who posted the last final grade for the course section cannot be provided without an associated date and time when building course section grading status.");
                }
            }

            CourseSectionId = courseSectionId;
            FinalGradesPostedDate = finalGradesPostedDate;
            FinalGradesPostedTime = finalGradesPostedTime;
            GradesPostedByPersonId = gradesPostedByPersonId;
        }
    }
}
