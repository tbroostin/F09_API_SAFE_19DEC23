// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// The result of adding a course section to student's list of preferred course sections
    /// </summary>
    [Serializable]
    public class PortalStudentPreferredCourseSectionUpdateResult
    {
        /// <summary>
        /// ID of the course section being added to a student's list of preferred course sections
        /// </summary>
        public string CourseSectionId { get; private set; }

        /// <summary>
        /// Status from adding a course section to a student's list of preferred course sections
        /// </summary>
        public PortalStudentPreferredCourseSectionUpdateStatus Status { get; private set; }

        /// <summary>
        /// Message associated with adding a course section to a student's list of preferred course sections
        /// <remarks>Typically when a course section is added successfully there is no message; there should only be a message when the section could not be added to
        /// the student's list of preferred course sections, or when the course section has a required co-requisite section. In the latter scenario, the required
        /// co-requisite section would also be added to the student's list of preferred course sections and the message would indicate this.</remarks>
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PortalStudentPreferredCourseSectionUpdateResult"/>
        /// </summary>
        /// <param name="courseSectionId">ID of the course section being added to a student's list of preferred course sections</param>
        /// <param name="status">Status from adding a course section to a student's list of preferred course sections</param>
        /// <param name="message">Message associated with adding a course section to a student's list of preferred course sections</param>
        public PortalStudentPreferredCourseSectionUpdateResult(string courseSectionId, PortalStudentPreferredCourseSectionUpdateStatus status, string message = null)
        {
            if (string.IsNullOrWhiteSpace(courseSectionId))
            {
                throw new ArgumentNullException("courseSectionId", "A course section ID is required when creating the result of adding a course section to a student's list of preferred course sections.");
            }
            if (status == PortalStudentPreferredCourseSectionUpdateStatus.Error && string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("message", "A message/explanation is required when the result of adding a course section to a student's list of preferred course sections is an error.");
            }
            CourseSectionId = courseSectionId;
            Status = status;
            Message = message;
        }
    }
}
