// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The possible values for the status of a section.
    /// </summary>
    [Serializable]
    public enum SectionStatus
    {
        /// <summary>
        /// The section is active.
        /// </summary>
        Active,
        /// <summary>
        /// The section has been cancelled.
        /// </summary>
        Cancelled,
        /// <summary>
        /// The section is inactive and cannot be registered for.
        /// </summary>
        Inactive,
        /// <summary>
        /// The section is in the process of being created and does not yet have a status.
        /// </summary>
        NotSet

    }
}
