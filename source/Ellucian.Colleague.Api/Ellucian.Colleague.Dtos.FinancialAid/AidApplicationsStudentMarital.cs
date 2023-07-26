using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Student's marital statuses
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsStudentMarital
    {
        /// <summary>
        /// Single
        /// </summary>
        [EnumMember(Value = "Single")]
        Single = 1,

        /// <summary>
        /// Married/remarried
        /// </summary>
        [EnumMember(Value = "MarriedOrRemarried")]
        MarriedOrRemarried = 2,

        /// <summary>
        /// Separated
        /// </summary>
        [EnumMember(Value = "Separated")]
        Separated = 3,

        /// <summary>
        /// Divorced/Widowed
        /// </summary>
        [EnumMember(Value = "DivorcedOrWidowed")]
        DivorcedOrWidowed = 4,
    }
}
