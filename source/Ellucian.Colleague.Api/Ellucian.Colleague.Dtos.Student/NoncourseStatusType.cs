using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Indicates whether the noncourse item is officially accepted or notational or there is no type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NoncourseStatusType
    {
        /// <summary>
        /// No status type
        /// </summary>
        None,
        /// <summary>
        /// Status indicates the test "counts" and has been officially accepted.
        /// </summary>
        Accepted,
        /// <summary>
        /// Status indicates the test is just notational
        /// </summary>
        Notational,
        /// <summary>
        /// Status indicates the student has withdrawn and all non course work statuses were changed to withdrawn
        /// </summary>
        Withdrawn
    }
}
