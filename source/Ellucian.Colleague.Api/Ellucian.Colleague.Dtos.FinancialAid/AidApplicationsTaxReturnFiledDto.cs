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
    /// Tax return file completed?
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsTaxReturnFiledDto
    {
        /// <summary>
        /// Already Completed
        /// </summary>
        [EnumMember(Value = "AlreadyCompleted")]
        AlreadyCompleted = 1,

        /// <summary>
        /// Will File
        /// </summary>
        [EnumMember(Value = "WillFile")]
        WillFile = 2,

        /// <summary>
        /// Will not file
        /// </summary>
        [EnumMember(Value = "WillNotFile")] 
        WillNotFile = 3
    }
}
