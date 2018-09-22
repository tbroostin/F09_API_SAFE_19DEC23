//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// FinancialAidExplanation DTO - holds information about 
    /// financial aid related explanations of different types
    /// </summary>
    public class FinancialAidExplanation
    {
        /// <summary>
        /// Explanation type: e.g. PellLEU
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FinancialAidExplanationType ExplanationType { get; set; }

        /// <summary>
        /// Explanation text
        /// </summary>
        public string ExplanationText { get; set; }
    }
}
