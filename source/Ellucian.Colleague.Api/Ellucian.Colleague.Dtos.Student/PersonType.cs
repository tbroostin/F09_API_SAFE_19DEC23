// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Enumeration of possible values for person type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PersonType
    {
        /// <summary>
        /// Student
        /// </summary>
        Student,
        /// <summary>
        /// Advisor
        /// </summary>
        Advisor
    }
}
