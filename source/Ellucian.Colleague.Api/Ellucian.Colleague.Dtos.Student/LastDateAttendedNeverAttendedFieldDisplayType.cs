// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Determins if and/or how the Last Date Attended / Never Attended field will be displayed
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LastDateAttendedNeverAttendedFieldDisplayType
    {
        /// <summary>
        /// LDA/NA field value may be modified
        /// </summary>
        Editable,
        /// <summary>
        /// LDA/NA field value is displayed but may not be modified
        /// </summary>
        ReadOnly,
        /// <summary>
        /// LDA/NA field value is not displayed
        /// </summary>
        Hidden,
    }
}
