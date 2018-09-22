using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Category of the test - admissions, placement, other
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TestType
    {
        /// <summary>
        /// Admissions type of test
        /// </summary>
        Admissions, 
        /// <summary>
        /// Placement type of test
        /// </summary>
        Placement,
        /// <summary>
        /// Other type of test
        /// </summary>
        Other
    }
}
