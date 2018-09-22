// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Is this requirement element planned out?  
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlanningStatus
    {
        /// <summary>
        /// No coursework has been planned toward this item
        /// </summary>
        NotPlanned, 
        /// <summary>
        /// Some coursework has been planned
        /// </summary>
        PartiallyPlanned,
        /// <summary>
        /// All required courses have been planned
        /// </summary>
        CompletelyPlanned
    }
}
