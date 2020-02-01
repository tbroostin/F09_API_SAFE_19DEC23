// Copyright 2016 Ellucian Company L.P. an?d its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about student charges.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentCharge2 : BaseModel2
    {
        /// <summary>
        /// The person (student), who incurred the charge.  Specifically, this is the global identifier for the Student.
        /// </summary>
        [JsonProperty("student")]
        [FilterProperty("criteria")]
        public GuidObject2 Person { get; set; }

        /// <summary>
        /// The account receivable type the charge is attached to.  Specifically, the global identifier for the Account Receivable Type.
        /// </summary>
        [JsonProperty("fundingSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 FundingSource { get; set; }

        /// <summary>
        /// Accounting Code the charge is associated with.  Specifically, this is the global identifier for the Accounting Code.
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
        /// The date when the student becomes liable for the charge.
        /// </summary>
        [JsonProperty("chargeableOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? ChargeableOn { get; set; }

        /// <summary>
        /// Comments of the charges.
        /// </summary>
        [JsonProperty("comments", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Comments { get; set; }

        /// <summary>
        /// The student charge amount object.
        /// </summary>
        [JsonProperty("chargedAmount")]
        public Dtos.DtoProperties.ChargedAmountDtoProperty ChargedAmount { get; set; }

        /// <summary>
        /// The override description associated with the charge.
        /// </summary>
        [JsonProperty("reportingDetail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DtoProperties.StudentChargesReportingDtoProperty ReportingDetail { get; set; }

        /// <summary>
        /// The override description associated with the charge.
        /// </summary>
        [JsonProperty("overrideDescription", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string OverrideDescription { get; set; }
    }
}
