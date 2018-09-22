// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Detailed information concerning financial aid applications. 
    /// </summary>
    [DataContract]
    public class FinancialAidApplication : BaseModel2
    {

        /// <summary>
        /// The person associated with the financial aid application.
        /// </summary>
        //[JsonProperty("applicant")]
        [DataMember(Name = "applicant", EmitDefaultValue = false)]
        public DtoProperties.FinancialAidApplicationApplicant Applicant { get; set; }

        /// <summary>
        /// The date the application was completed.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("applicationCompletedOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? ApplicationCompletedOn { get; set; }

        /// <summary>
        /// The financial aid year for which the applicant applied.
        /// </summary>
        [JsonProperty("aidYear", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 AidYear { get; set; }

        /// <summary>
        /// The methodology for which the application is applicable.
        /// </summary>

        [DataMember(Name = "methodology", EmitDefaultValue = false)]
        public FinancialAidApplicationsMethodology Methodology { get; set; }

        /// <summary>
        /// The source of the financial aid application.
        /// </summary>

        [DataMember(Name = "source", EmitDefaultValue = false)]
        public FinancialAidApplicationsSource Source { get; set; }

        /// <summary>
        /// The applicant's state of legal residence.
        /// </summary>
        [DataMember(Name = "stateOfLegalResidence", EmitDefaultValue = false)]
        public string StateOfLegalResidence { get; set; }

        /// <summary>
        /// The criteria for determining the applicant's dependency status.
        /// </summary>

        [DataMember(Name = "independenceCriteria", EmitDefaultValue = false)]
        //public FinancialAidApplicationsIndependenceCriteria IndependenceCriteria { get; set; }
        public IEnumerable<FinancialAidApplicationsIndependenceCriteria> IndependenceCriteria { get; set; }

        /// <summary>
        /// An indication of the applicant's interest in a work study program.
        /// </summary>
        [DataMember(Name = "workStudy", EmitDefaultValue = false)]
        public FinancialAidApplicationsInterest WorkStudy { get; set; }

        /// <summary>
        /// The applicant's housing preference.
        /// </summary>
        [DataMember(Name = "housingPreference", EmitDefaultValue = false)]
        public FinancialAidApplicationsHousingPreference HousingPreference { get; set; }

        /// <summary>
        /// The applicant's income information.
        /// </summary>
        [DataMember(Name = "applicantIncome", EmitDefaultValue = false)]
        public DtoProperties.FinancialAidApplicationApplicantIncomeDtoProperty ApplicantIncome { get; set; }

        /// <summary>
        /// The custodial parent(s) income information.
        /// </summary>
        [DataMember(Name = "custodialParentsIncome", EmitDefaultValue = false)]
        public DtoProperties.FinancialAidApplicationCustodialParentsIncomeDtoProperty CustodialParentsIncome { get; set; }

        /// <summary>
        /// The noncustodial parent(s) income information.
        /// </summary>
        [DataMember(Name = "noncustodialParentsIncome", EmitDefaultValue = false)]
        public DtoProperties.FinancialAidApplicationNoncustodialParentsIncomeDtoProperty NoncustodialParentsIncome { get; set; }
    }
}
