using System;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Legal residence details
    /// </summary>
    public class LegalResidence
    {

        /// <summary>
        /// State of legal residence
        /// </summary>
        [JsonProperty("state", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata(DataDescription = "Student/Parent state of legal residence.", DataMaxLength = 2)]
        public string State { get; set; }

        /// <summary>
        /// Resident before a set date that increments every year
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("residentBefore", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata(DataDescription = "Student/Parent is a resident before a set date that increments every year.", DataMaxLength = 5)]
        public bool? ResidentBefore { get; set; }

        /// <summary>
        /// Date of legal residence
        /// </summary>
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata(DataDescription = "Student/Parent date of legal residence.", DataMaxLength = 10)]
        public string Date { get; set; }

    }
}