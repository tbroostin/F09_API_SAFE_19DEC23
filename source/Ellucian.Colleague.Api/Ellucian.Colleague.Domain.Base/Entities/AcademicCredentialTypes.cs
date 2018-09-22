// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible Academic Credential Types
    /// </summary>
    [Serializable]
    public enum AcademicCredentialType
    {
        /// <summary>
        /// A degree awarded by an institution
        /// </summary>
        Degree,

        /// <summary>
        /// A diploma awarded by an institution
        /// </summary>
        Diploma,

        /// <summary>
        /// An honorary degree awarded by an institution
        /// </summary>
        Honorary,

        /// <summary>
        /// "A certificate awarded by an institution
        /// </summary>
        Certificate
    }
}