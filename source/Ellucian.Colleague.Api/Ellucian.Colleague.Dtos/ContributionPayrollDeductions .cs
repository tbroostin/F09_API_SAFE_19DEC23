// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// DTO for EEDM model contribution-payroll-deductions
    /// </summary>
    [DataContract]
    public class ContributionPayrollDeductions : BaseModel2
    {
        /// <summary>
        /// The arrangement details related to the payroll deduction.
        /// </summary>
        [DataMember(Name = "arrangement")]
        [FilterProperty("criteria")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 Arrangement { get; set; }

        /// <summary>
        /// The date the payroll deduction was made.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [DataMember(Name = "deductedOn")]
        public DateTime DeductedOn { get; set; }

        /// <summary>
        /// The amount that was deducted from the payroll.
        /// </summary>
        [DataMember(Name = "amount")]
        public AmountDtoProperty Amount { get; set; }
    }
}
