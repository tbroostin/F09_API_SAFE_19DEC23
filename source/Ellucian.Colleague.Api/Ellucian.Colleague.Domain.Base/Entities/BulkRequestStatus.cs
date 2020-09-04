// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;


namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of a status types of the bulk request
    /// </summary>
    [Serializable]
    public enum BulkRequestStatus
    {
        /// <summary>
        /// inProgress
        /// </summary>
        InProgress,

        /// <summary>
        /// completed
        /// </summary>
        Completed,

        /// <summary>
        /// packagingCompleted
        /// </summary>
        PackagingCompleted,

        /// <summary>
        /// error
        /// </summary>
        Error
    }
}

