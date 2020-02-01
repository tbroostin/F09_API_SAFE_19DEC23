// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <remarks>
    /// All of the possible planned course warnings
    /// </remarks>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DegreePlanWarningType
    {
        /// <summary>
        /// Number of credits for a planned course is outside the range of credits for the course
        /// </summary>
        InvalidPlannedCredits,
        /// <summary>
        /// Number of credits for a planned course is negative
        /// </summary>
        NegativePlannedCredits,
        /// <summary>
        /// Missing a required course corequisite
        /// </summary>
        CorequisiteRequiredCourse,
        /// <summary>
        /// Missing an optional course corequisite
        /// </summary>
        CorequisiteOptionalCourse,
        /// <summary>
        /// Missing a required course section corequisite
        /// </summary>
        CorequisiteRequiredSection,
        /// <summary>
        /// Missing an option course section corequisite
        /// </summary>
        CorequisiteOptionalSection,
        /// <summary>
        /// Time conflict exists with another planned course
        /// </summary>
        TimeConflict,
        /// <summary>
        /// Course prerequisite is unsatisfied
        /// </summary>
        PrerequisiteUnsatisfied
    }
}
