using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// the different high school Diploma or Equivalent
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsHSGradtype
    {
        /// <summary>
        /// 1. High School Diploma
        /// </summary>
        [EnumMember(Value = "HighSchoolDiploma")]
        HighSchoolDiploma = 1,

        /// <summary>
        /// 2. GED/ State Equivalent Test
        /// </summary>
        [EnumMember(Value = "GEDOrStateEquivalentTest")]
        GEDOrStateEquivalentTest = 2,

        /// <summary>
        /// 3. Home Schooled
        /// </summary>
        [EnumMember(Value = "HomeSchooled")]
        HomeSchooled = 3,

        /// <summary>
        /// 4. None of the above/Others 
        /// </summary>
        [EnumMember(Value = "Others")]
        Others = 4
    }
}
