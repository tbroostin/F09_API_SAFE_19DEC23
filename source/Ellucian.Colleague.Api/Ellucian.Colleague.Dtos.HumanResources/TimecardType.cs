/* Copyright 2016-2022 Ellucian Company L.P. and its affiliates. */
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Time Card Type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimecardType
    {
        /// <summary>
        /// No timecard type will be applied to the associated position
        /// </summary>
        None,
        /// <summary>
        /// Summary hours
        /// </summary>
        Summary,
        /// <summary>
        /// Detailed time
        /// </summary>
        Detailed,
        /// <summary>
        /// Clock In/Clock Out
        /// </summary>
        Clock
    }
}
