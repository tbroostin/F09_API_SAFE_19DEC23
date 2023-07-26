using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The field maps to Yes, No or DonotKnow enum values
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsYesOrNoDto
    {
        /// <summary>
        /// Yes
        /// </summary>
        [EnumMember(Value = "Yes")]
        Yes = 1,

        /// <summary>
        /// No
        /// </summary>
        [EnumMember(Value = "No")]
        No = 2,

        /// <summary>
        /// Don't Know
        /// </summary>
        [EnumMember(Value = "DoNotKnow")]
        DonotKnow = 3

    }
}
