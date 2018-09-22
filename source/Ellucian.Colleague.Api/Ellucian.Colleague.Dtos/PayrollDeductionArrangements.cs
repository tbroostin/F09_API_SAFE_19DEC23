// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a payroll deduction arrangements HEDM version 6 model
    /// </summary>
    [DataContract]
    public class PayrollDeductionArrangements : BaseModel2
    {
        /// <summary>
        /// The employee for whom the payroll deduction is requested.
        /// </summary>
        [DataMember(Name = "person")]
        [FilterProperty("criteria")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 Person { get; set; }

        /// <summary>
        /// The target towards which payroll deductions are requested.
        /// </summary>
        [JsonProperty("paymentTarget", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PaymentTargetDtoProperty PaymentTarget { get; set; }

        /// <summary>
        /// The status of a payroll deduction request.
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public EnumProperties.PayrollDeductionArrangementStatuses? Status { get; set; }

        /// <summary>
        /// The amount to be deducted per instance.
        /// </summary>
        [JsonProperty("amountPerPayment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AmountDtoProperty amountPerPayment { get; set; }

        /// <summary>
        /// The total amount to be deducted.
        /// </summary>
        [JsonProperty("totalAmount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AmountDtoProperty TotalAmount { get; set; }

        /// <summary>
        /// The date when the payroll deductions should begin.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("startOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The date when the payroll deductions should end.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("endOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? EndDate { get; set; }
 
        /// <summary>
        /// The pay periods for which the payroll deductions are applicable.
        /// </summary>
        [JsonProperty("payPeriodOccurence", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PayPeriodOccurance PayPeriodOccurence { get; set; }

        /// <summary>
        /// The reason why a property was changed (example: status change).
        /// </summary>
        [JsonProperty("changeReason", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 ChangeReason { get; set; }

        /// <summary>
        /// Person constructor
        /// </summary>
        public PayrollDeductionArrangements()
            : base()
        {
        }
    }
}