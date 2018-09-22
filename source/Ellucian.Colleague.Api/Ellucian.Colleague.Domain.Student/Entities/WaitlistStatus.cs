// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Status of a student on a waitlist
    /// </summary>
    [Serializable]
    public enum WaitlistStatus
    {
        /// <summary>
        /// Student is waiting to enroll in the section
        /// </summary>
        WaitingToEnroll = 1,
        /// <summary>
        /// Student has enrolled in the section
        /// </summary>
        Enrolled = 2,
        /// <summary>
        /// Student dropped from waitlist
        /// </summary>
        DroppedFromWaitlist = 3,
        /// <summary>
        /// Student offered enrollment in the section
        /// </summary>
        OfferedEnrollment = 4,
        /// <summary>
        /// Student allowed the enrollment offer to expire
        /// </summary>
        EnrollmentOfferExpired = 5,
        /// <summary>
        /// The section was cancelled
        /// </summary>
        SectionCancelled = 6,
        /// <summary>
        /// The waitlist for the section was closed
        /// </summary>
        WaitlistClosed = 7,
        /// <summary>
        /// Student enrolled in a different section of the course
        /// </summary>
        EnrolledInOtherSection = 8,
        /// <summary>
        /// Unknown waitlist status
        /// </summary>
        Unknown
    }
}
