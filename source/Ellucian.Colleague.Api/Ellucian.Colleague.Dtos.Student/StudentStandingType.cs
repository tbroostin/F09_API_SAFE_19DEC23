using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The different types of academic standings.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StudentStandingType
    {
        /// <summary>
        /// Academic Level
        /// </summary>
        AcademicLevel,
        /// <summary>
        /// Program
        /// </summary>
        Program,
        /// <summary>
        /// Term
        /// </summary>
        Term
    }
}
