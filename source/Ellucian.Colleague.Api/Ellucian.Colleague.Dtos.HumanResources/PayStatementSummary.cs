/*Copyright 2017-2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Pay statement summary construct for use in data transfer
    /// </summary>
    [DataContract]
    public class PayStatementSummary
    {
        /// <summary>
        /// Pay Statement Id
        /// </summary>
        [JsonProperty("id")]
        [Metadata("WEB.PAY.ADVICES", DataDescription = "Pay Statement Id.")]
        public string Id { get; set; }

        /// <summary>
        /// The Date the employee was paid
        /// </summary>
        [JsonProperty("payDate")]
        [Metadata("WPA.CHECK.DATE", DataDescription = "The Date the employee was paid.")]
        public DateTime PayDate { get; set; }
    }
}
