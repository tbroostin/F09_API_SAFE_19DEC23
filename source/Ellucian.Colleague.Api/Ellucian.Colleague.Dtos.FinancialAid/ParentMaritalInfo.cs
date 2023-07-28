using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Parent's marital status and marital status date
    /// </summary>
    public class ParentMaritalInfo
    {
        /// <summary>
        ///<see cref="AidApplicationsParentMarital"/> Parent's marital status
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.P.MARITAL.STATUS", false, DataDescription = "Parents' marital status.", DataMaxLength = 40)]
        public AidApplicationsParentMarital? Status { get; set; }

        /// <summary>
        /// Parent's date of marriage
        /// </summary>
        [JsonProperty("maritalDate", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.P.MARITAL.DATE", false, DataDescription = "Parents' date of marriage.")]
        public string Date { get; set; }
    }
}
