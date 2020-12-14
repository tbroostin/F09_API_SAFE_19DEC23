// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Behavior for assigning academic program to students in instant enrollment workflows
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AddNewStudentProgramBehavior
    {
        /// <summary>
        /// Only new and inactive students are assigned an academic program
        /// </summary>
        New,
        /// <summary>
        /// Any student is assigned an academic program
        /// </summary>
        Any
    }
}
