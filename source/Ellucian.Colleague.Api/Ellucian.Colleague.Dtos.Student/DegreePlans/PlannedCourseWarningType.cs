// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <remarks>
    /// All of the possible planned course warnings
    /// </remarks>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlannedCourseWarningType
    {
        /// <summary>
        /// Number of planned credits is invalid because it falls outside the planned credit range for the course
        /// </summary>
        InvalidPlannedCredits,
        /// <summary>
        /// Planned credits is negative
        /// </summary>
        NegativePlannedCredits,
        /// <summary>
        /// Planned course has a time conflict with one or more other planned courses.
        /// </summary>
        TimeConflict,
        /// <summary>
        /// Planned course has an unmet requisite
        /// </summary>
        UnmetRequisite,
        /// <summary>
        /// Planned course has been planned in a term in which it may not be offered
        /// </summary>
        CourseOfferingConflict
    }
}
