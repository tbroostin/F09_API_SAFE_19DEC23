// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The possible values for the status of a course transfer.
    /// </summary>
    [Serializable]
    public enum CourseTransferStatusesCategory
    {
        /// <summary>
        /// The course transfer status is preliminary.
        /// </summary>
        Preliminary,
        /// <summary>
        /// The course transfer has been approved.
        /// </summary>
        Approved,
        /// <summary>
        /// The course transfer is in the process of being created and does not yet have a status.
        /// </summary>
        NotSet

    }
}
