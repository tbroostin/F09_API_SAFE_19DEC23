using System;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Colleague.Dtos;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Student's marital status and marital status date
    /// </summary>
    public class StudentMaritalInfo
    {
        /// <summary>
        ///<see cref="AidApplicationsStudentMarital"/> Student marital status
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.S.MARITAL.STATUS", false, DataDescription = "Student's marital status.", DataMaxLength = 20)]
        public AidApplicationsStudentMarital? Status { get; set; }

        /// <summary>
        /// Student's marital status date
        /// </summary>
        [JsonProperty("maritalDate", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.S.MARITAL.DATE", false, DataDescription = "Student's marital status date.")]
        public string Date { get; set; } 
    }
}
