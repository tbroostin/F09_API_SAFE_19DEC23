/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Pay statement information
    /// </summary>
    [DataContract]
    public class PayStatementInformation
    {
        /// <summary>
        /// The Gross Pay for the period
        /// </summary>
        [JsonProperty("gross")]
        [Metadata("WPA.GROSS.PAY", DataDescription = "The Gross Pay for the period")]
        public decimal GrossAmount { get; set; }

        /// <summary>
        /// The Net Pay for the period
        /// </summary>
        [JsonProperty("dednAmt")]
        [Metadata("WPA.TOTAL.BENDEDS", DataDescription = "")]
        public decimal DeductionAmount { get; set; }

        /// <summary>
        /// Total taxes for the period
        /// </summary>
        [JsonProperty("net")]
        [Metadata("WPA.NET.PAY", DataDescription = "Total taxes for the period")]
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Total benefits and deductions for the period
        /// </summary>
        [JsonProperty("taxAmt")]
        [Metadata("WPA.TOTAL.TAXES", DataDescription = "Total benefits and deductions for the period")]
        public decimal TaxAmount { get; set; }
    }
}
