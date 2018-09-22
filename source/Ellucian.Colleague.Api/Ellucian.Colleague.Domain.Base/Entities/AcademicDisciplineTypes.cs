// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible Academic Discipline Types
    /// </summary>
    [Serializable]
    public enum AcademicDisciplineType
    {
        /// <summary>
        /// A Minor.
        /// </summary>
        Minor,

        /// <summary>
        /// A Major.
        /// </summary>
        Major,

        /// <summary>
        /// A Concentration.
        /// </summary>
        Concentration
    }
}