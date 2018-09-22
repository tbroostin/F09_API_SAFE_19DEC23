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
    /// Indicates whether person is compensated by hour or by salary
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HourlySalaryIndicator
    {
        /// <summary>
        /// Hourly compensation
        /// </summary>
        Hourly,
        /// <summary>
        /// Salaried compensation
        /// </summary>
        Salary
    }
}
