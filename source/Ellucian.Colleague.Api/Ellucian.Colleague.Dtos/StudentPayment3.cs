﻿// Copyright 2017 Ellucian Company L.P. an?d its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information student payments.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentPayment3 : BaseModel2
    {
        /// <summary>
        /// The person (student), who incurred the charge.  Specifically, this is the global identifier for the Student.
        /// </summary>
        [JsonProperty("student")]
        [FilterProperty("criteria")]
        public GuidObject2 Person { get; set; }

        /// <summary>
        /// The distribution method of the funding source associated with the student payment.  
        /// Specifically, the global identifier for the Account Receivable Type.
        /// </summary>
        [JsonProperty("fundingSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 FundingSource { get; set; }

        /// <summary>
        /// The accounting code of the funding destination associated with the student payment.  
        /// Specifically, this is the global identifier for the Accounting Code.
        /// </summary>
        [JsonProperty("fundingDestination", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 FundingDestination { get; set; }

        /// <summary>
        /// The term the charges are incurred in.  Specifically, this is the global identifier for the Academic Period.
        /// </summary>
        [JsonProperty("academicPeriod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 AcademicPeriod { get; set; }

        /// <summary>
        /// Type of the charge the student incurred. 
        /// </summary>
        [JsonProperty("paymentType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public EnumProperties.StudentPaymentTypes2 PaymentType { get; set; }        

        /// <summary>
        /// The date when the student becomes liable for the charge.
        /// </summary>
        [JsonProperty("paidOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? PaidOn { get; set; }

        /// <summary>
        /// Comments of the charges.
        /// </summary>
        [JsonProperty("comments", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Comments { get; set; }

        /// <summary>
        /// The monetary value for the specified currency.
        /// </summary>
        [JsonProperty("amount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DtoProperties.AmountDtoProperty Amount { get; set; }

        /// <summary>
        /// The override description associated with the charge.
        /// </summary>
        [JsonProperty("reportingDetail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DtoProperties.StudentPaymentsReportingDtoProperty ReportingDetail { get; set; }

        /// <summary>
        /// The override description associated with the charge.
        /// </summary>
        [JsonProperty("overrideDescription", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string OverrideDescription { get; set; }
    }
}
