// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Exceptions;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading
{
    [Serializable]
    /// <summary>
    /// Result of an attempted preliminary anonymous grade update
    /// </summary>
    public class PreliminaryAnonymousGradeUpdateResult
    {
        /// <summary>
        /// ID of the associated student course section data
        /// </summary>
        /// <remarks>This ID is synonymous with the record for preliminary student grade work data, as it is a shared ID.</remarks>
        public string StudentCourseSectionId { get; private set; }

        /// <summary>
        /// Status of the update
        /// </summary>
        public PreliminaryAnonymousGradeUpdateStatus Status { get; private set; }

        /// <summary>
        /// Informational message related to the status of the update
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PreliminaryAnonymousGradeUpdateResult"/> object
        /// </summary>
        /// <param name="studentCourseSectionId">ID for the course section to which the preliminary anonymous grade applies</param>
        /// <param name="status">Status of the update</param>
        /// <param name="message">Informational message related to the status of the update</param>
        /// <exception cref="ArgumentNullException">A student course section ID is required when building a preliminary anonymous grade update result.</exception>
        /// <exception cref="ColleagueException">An informational message is required when a preliminary anonymous grade update fails.</exception>
        public PreliminaryAnonymousGradeUpdateResult(string studentCourseSectionId, PreliminaryAnonymousGradeUpdateStatus status, string message)
        {
            if (string.IsNullOrEmpty(studentCourseSectionId))
            {
                throw new ArgumentNullException("studentCourseSectionId", "A student course section ID is required when building a preliminary anonymous grade update result.");
            }
            if (status == PreliminaryAnonymousGradeUpdateStatus.Failure && string.IsNullOrEmpty(message))
            {
                throw new ColleagueException("An informational message is required when a preliminary anonymous grade update fails.");
            }

            StudentCourseSectionId = studentCourseSectionId;
            Status = status;
            Message = message;
        }
    }
}
