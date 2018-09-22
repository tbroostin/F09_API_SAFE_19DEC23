// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Is this requirement element (Program, Requirement, Subrequirement, or Group) complete?
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CompletionStatus
    {
        /// <summary>
        /// This item has had no coursework applied
        /// </summary>
        NotStarted, 
        /// <summary>
        /// Some coursework has been completed
        /// </summary>
        PartiallyCompleted, 
        /// <summary>
        /// All conditions have been met to complete this item
        /// </summary>
        Completed,
        /// <summary>
        /// This item has been waived; no coursework needs to be completed
        /// </summary>
        Waived
    }
}
