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
    /// Different housing plans/codes
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationHousingPlanDto
    {
        /// <summary>
        /// On campus
        /// </summary>
        [EnumMember(Value = "OnCampus")]
        OnCampus = 1,

        /// <summary>
        /// With parent
        /// </summary>
        [EnumMember(Value = "WithParent")]
        WithParent = 2,

        /// <summary>
        /// Off Campus or Blank 
        /// </summary>
        [EnumMember(Value = "OffCampus")]
        OffCampus = 3
    }
}
