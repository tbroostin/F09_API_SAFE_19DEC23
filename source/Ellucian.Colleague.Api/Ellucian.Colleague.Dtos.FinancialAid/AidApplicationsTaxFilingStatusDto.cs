using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Tax return filing status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsTaxFilingStatusDto
    {
        /// <summary>
        /// Single
        /// </summary>
        [EnumMember(Value = "Single")]
        Single = 1,

        /// <summary>
        /// Married - filed joint return
        /// </summary>
        [EnumMember(Value = "MarriedFiledJointReturn")]
        MarriedFiledJointReturn = 2,

        /// <summary>
        /// Married - filed separate return
        /// </summary>
        [EnumMember(Value = "MarriedFiledSeparateReturn")]
        MarriedFiledSeparateReturn = 3,

        /// <summary>
        /// Head of household
        /// </summary>
        [EnumMember(Value = "HeadOfHousehold")]
        HeadOfHousehold = 4,

        /// <summary>
        /// Qualifying widow(er)
        /// </summary>
        [EnumMember(Value = "QualifyingWidowOrWidower")]
        QualifyingWidowOrWidower = 5,

        /// <summary>
        /// Don't know
        /// </summary>
        [EnumMember(Value = "DoNotKnow")]
        DoNotKnow = 6
    }
}
