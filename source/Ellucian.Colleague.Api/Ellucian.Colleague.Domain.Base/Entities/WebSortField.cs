// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible sort fields
    /// </summary>
    [Serializable]
    public enum WebSortField
    {
        /// <summary>
        /// Description sort field
        /// </summary>
        Description,

        /// <summary>
        /// Status sort field
        /// </summary>
        Status,

        /// <summary>
        /// Status Date sort field
        /// </summary>
        StatusDate,

        /// <summary>
        /// Due Date sort field
        /// </summary>
        DueDate,

        /// <summary>
        /// Office Description sort field
        /// </summary>
        OfficeDescription
    }
}