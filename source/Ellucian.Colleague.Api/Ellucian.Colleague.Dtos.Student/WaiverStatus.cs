// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The possible values for the status assigned to an individual waiver item.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WaiverStatus
    {
        /// <summary>
        /// No action has been taken on the item--it is neither waived nor denied
        /// </summary>
        NotSelected,
        /// <summary>
        /// Item has been waived
        /// </summary>
        Waived,
        /// <summary>
        /// Waiver has been denied for the item
        /// </summary>
        Denied
    }
}
