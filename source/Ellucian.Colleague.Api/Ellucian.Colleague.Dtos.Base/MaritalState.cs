using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Translate to Single, Married, Divorced, or Widowed
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MaritalState
    {
        /// <summary>
        /// Single
        /// </summary>
        Single,
        /// <summary>
        /// Married
        /// </summary>
        Married,
        /// <summary>
        /// Divorced
        /// </summary>
        Divorced,
        /// <summary>
        /// Widowed
        /// </summary>
        Widowed
    }
}
