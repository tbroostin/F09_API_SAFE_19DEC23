using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Registration Action Types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RegistrationAction
    {
        /// <summary>
        /// Add (Register for) a course section
        /// </summary>
        Add,
        /// <summary>
        /// Take a course section pass or fail
        /// </summary>
        PassFail,
        /// <summary>
        /// Audit a course section
        /// </summary>
        Audit,
        /// <summary>
        /// Drop a course section
        /// </summary>
        Drop,
        /// <summary>
        /// Add student to the course section waitlist
        /// </summary>
        Waitlist,
        /// <summary>
        /// Remove student from the course section waitlist
        /// </summary>
        RemoveFromWaitlist
    }
}
