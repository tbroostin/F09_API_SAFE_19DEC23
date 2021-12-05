// Copyright 2020 Ellucian Company L.P. and its affiliates
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// This indicates the availability status on a section to indicate the section is Open for registration, or is applicable for waitlist
    /// or is closed for registration since all the seats are already taken.
    /// </summary>
    [Serializable]
    public enum SectionAvailabilityStatusType
    {
        /// <summary>
        /// This indicates if the section is open. The section is open when the seats are available for registration.
        /// </summary>
        Open,

        /// <summary>
        /// This indicates if the section is available for being waitlisted.
        /// </summary>
        Waitlisted,

        /// <summary>
        /// This indicates when the section does not have any available seats or when all the seats are registered for the section.
        /// </summary>
        Closed
    }
}

