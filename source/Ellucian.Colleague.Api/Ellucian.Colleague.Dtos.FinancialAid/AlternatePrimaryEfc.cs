/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class AlternatePrimaryEfc
    {
        /// <summary>
        /// Primary alternate EFC for 1 month.
        /// </summary>             
        [JsonProperty("oneMonth", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.1MNTH.EFC", false, DataDescription = "Primary alternate EFC for 1 month.")]
        public int? OneMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 2 months.
        /// </summary>             
        [JsonProperty("twoMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.2MNTH.EFC", false, DataDescription = "Primary alternate EFC for 2 months.")]
        public int? TwoMonths { get; set; }

        /// <summary>
        /// Primary alternate EFC for 3 months.
        /// </summary>             
        [JsonProperty("threeMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.3MNTH.EFC", false, DataDescription = "Primary alternate EFC for 3 months.")]
        public int? ThreeMonths { get; set; }

        /// <summary>
        /// Primary alternate EFC for 4 months.
        /// </summary>             
        [JsonProperty("fourMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.4MNTH.EFC", false, DataDescription = "Primary alternate EFC for 4 months.")]
        public int? FourMonths { get; set; }

        /// <summary>
        /// Primary alternate EFC for 5 months.
        /// </summary>             
        [JsonProperty("fiveMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.5MNTH.EFC", false, DataDescription = "Primary alternate EFC for 5 months.")]
        public int? FiveMonths { get; set; }

        /// <summary>
        /// Primary alternate EFC for 6 months.
        /// </summary>             
        [JsonProperty("sixMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.6MNTH.EFC", false, DataDescription = "Primary alternate EFC for 6 months.")]
        public int? SixMonths { get; set; }

        /// <summary>
        /// Primary alternate EFC for 7 months.
        /// </summary>             
        [JsonProperty("sevenMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.7MNTH.EFC", false, DataDescription = "Primary alternate EFC for 7 months.")]
        public int? SevenMonths { get; set; }

        /// <summary>
        /// Primary alternate EFC for 8 months.
        /// </summary>             
        [JsonProperty("eightMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.8MNTH.EFC", false, DataDescription = "Primary alternate EFC for 8 months.")]
        public int? EightMonths { get; set; }

        /// <summary>
        /// Primary alternate EFC for 10 months.
        /// </summary>             
        [JsonProperty("tenMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.10MNTH.EFC", false, DataDescription = "Primary alternate EFC for 10 months.")]
        public int? TenMonths { get; set; }

        /// <summary>
        /// Primary alternate EFC for 11 months.
        /// </summary>             
        [JsonProperty("elevenMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.11MNTH.EFC", false, DataDescription = "Primary alternate EFC for 11 months.")]
        public int? ElevenMonths { get; set; }

        /// <summary>
        /// Primary alternate EFC for 12 months.
        /// </summary>             
        [JsonProperty("twelveMonths", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ALT.12MNTH.EFC", false, DataDescription = "Primary alternate EFC for 12 months.")]
        public int? TwelveMonths { get; set; }
    }
}
