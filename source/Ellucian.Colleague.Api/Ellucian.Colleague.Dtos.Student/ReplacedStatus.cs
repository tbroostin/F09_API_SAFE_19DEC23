using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Statuses that indicate if an academic credit has been or may be replaced by another credit in a program evaluation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReplacedStatus
    {
        /// <summary>
        /// This academic credit has not been replaced by another academic credit
        /// </summary>
        NotReplaced,
        /// <summary>
        /// This academic credit has been replaced by another academic credit
        /// </summary>
        Replaced,
        /// <summary>
        /// This academic credit may be replaced by another academic credit that is still in progress
        /// </summary>
        ReplaceInProgress,

    }
}
