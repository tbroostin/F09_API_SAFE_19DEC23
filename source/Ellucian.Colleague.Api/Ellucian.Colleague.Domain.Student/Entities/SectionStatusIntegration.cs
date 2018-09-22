// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The possible values for the status of a section.
    /// </summary>
    [Serializable]
    public enum SectionStatusIntegration
    {
        /// <summary>
        /// The section is closed and cannot be registered for.
        /// </summary>
        Closed,
        /// <summary>
        /// The section is Open.
        /// </summary>
        Open,
        /// <summary>
        /// The section is pending.
        /// </summary>
        Pending,
        /// <summary>
        /// The section has been cancelled.
        /// </summary>
        Cancelled,
         /// <summary>
        /// The section status was not mapped on VTHM
        /// </summary>
        NotSet

    }
}
