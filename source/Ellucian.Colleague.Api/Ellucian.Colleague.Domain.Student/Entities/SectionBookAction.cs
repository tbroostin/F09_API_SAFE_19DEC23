// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Action to be taken for a section book assignment
    /// </summary>
    [Serializable]
    public enum SectionBookAction
    {
        /// <summary>
        /// Add a new book assignment for a section
        /// </summary>
        Add = 0,
        /// <summary>
        /// Update an existing book assignment for a section
        /// </summary>
        Update = 1,
        /// <summary>
        /// Remove a book assignment from a section
        /// </summary>
        Remove = 2
    }
}
