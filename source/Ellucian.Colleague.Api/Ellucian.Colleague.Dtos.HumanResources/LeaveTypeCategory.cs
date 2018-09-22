using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Categories that can be assigned to types of leave. Sick, Hourly and Sick, Salary may be separate leave types, but they
    /// both have the same category Sick
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LeaveTypeCategory
    {
        /// <summary>
        /// Default - this value must be the first one set here
        /// </summary>
        None,

        /// <summary>
        /// Compensatory
        /// </summary>
        Compensatory,

        /// <summary>
        /// Sick
        /// </summary>
        Sick,

        /// <summary>
        /// Vacation
        /// </summary>
        Vacation
    }
}
