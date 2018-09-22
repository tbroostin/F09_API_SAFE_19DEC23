using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Denotes whether a class is being taken for a grade, for pass/fail, or audit only.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GradingType
    {
        /// <summary>
        /// Class is being taken as graded.
        /// </summary>
        Graded,

        /// <summary>
        /// Class is being taken as pass or fail
        /// </summary>
        PassFail,

        /// <summary>
        /// Class is being audited
        /// </summary>
        Audit
    }

}
