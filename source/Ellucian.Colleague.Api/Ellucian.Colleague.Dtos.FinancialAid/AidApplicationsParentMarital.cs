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
    /// Parent's marital statuses 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsParentMarital
    {
        /// <summary>
        /// Married/Remarried
        /// </summary>
        [EnumMember(Value = "MarriedOrRemarried")]
        MarriedOrRemarried = 1,

        /// <summary>
        /// Never Married
        /// </summary>
        [EnumMember(Value = "NeverMarried")]
        NeverMarried = 2,

        /// <summary>
        /// Divorced/ Separated
        /// </summary>
        [EnumMember(Value = "DivorcedOrSeparated")]
        DivorcedOrSeparated = 3,

        /// <summary>
        /// Widowed
        /// </summary>
        [EnumMember(Value = "Widowed")] 
        Widowed = 4,

        /// <summary>
        /// Unmarried and Both parents living together
        /// </summary>
        [EnumMember(Value = "UnmarriedAndBothParentsLivingTogether")] 
        UnmarriedAndBothParentsLivingTogether = 5
    }
}
