// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The curriculum objective associated with the student's academic program.
    /// </summary>
    public enum CurriculumObjectiveCategory
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// matriculated
        /// </summary>
        Matriculated,

        /// <summary>
        /// outcome
        Outcome,

        /// <summary>
        /// recruited
        /// </summary>
        Recruited,

        /// <summary>
        /// applied
        /// </summary>
        Applied,
    }
}