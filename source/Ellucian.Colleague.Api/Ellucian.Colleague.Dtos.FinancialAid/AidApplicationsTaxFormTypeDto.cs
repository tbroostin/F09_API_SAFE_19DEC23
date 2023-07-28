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
    /// Tax form used
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsTaxFormTypeDto
    {
        /// <summary>
        /// IRS 1040
        /// </summary>
        [EnumMember(Value = "IRS1040")]
        IRS1040 = 1,

        /// <summary>
        /// Foreign tax return
        /// </summary>
        [EnumMember(Value = "ForeignTaxReturn")]
        ForeignTaxReturn = 3,

        /// <summary>
        /// A Tax return from Puerto Rico / U.S.A Territory / freely associated state
        /// </summary>
        [EnumMember(Value = "ATaxReturnFromPuertoRicoOrAUsaTerritoryOrFreelyAssociatedState")]
        ATaxReturnFromPuertoRicoOrAUsaTerritoryOrFreelyAssociatedState = 4
    }
}
