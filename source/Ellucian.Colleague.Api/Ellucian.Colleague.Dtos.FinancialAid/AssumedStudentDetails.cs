/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class AssumedStudentDetails
    {
        /// <summary>
        /// Assumed citizenship.
        /// </summary>        
        [JsonProperty("citizenship", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.CITIZENSHIP", false, DataDescription = "Assumed citizenship.", DataMaxLength = 25)]
        public AssumedCitizenshipStatus? Citizenship { get; set; }

        //ENUM
        /// <summary>
        /// Assumed student marital status.
        /// </summary>        
        [JsonProperty("studentMaritalStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.S.MAR.STAT", false, DataDescription = "Assumed student marital status.", DataMaxLength = 25)]
        public AssumedStudentMaritalStatus? StudentMaritalStatus { get; set; }

        /// <summary>
        /// Assumed student AGI.
        /// </summary>        
        [JsonProperty("studentAgi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.S.AGI", false, DataDescription = "Assumed student AGI.")]
        public int? StudentAgi { get; set; }

        /// <summary>
        /// Assumed student taxes paid.
        /// </summary>        
        [JsonProperty("studentTaxPaid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.S.TAX.PD", false, DataDescription = "Assumed student taxes paid.")]
        public int? StudentTaxPaid { get; set; }

        /// <summary>
        /// Assumed student income from work.
        /// </summary>        
        [JsonProperty("studentWorkIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.S.INC.WORK", false, DataDescription = "Assumed student income from work.")]
        public int? StudentWorkIncome { get; set; }

        /// <summary>
        /// Assumed spouse income from work.
        /// </summary>        
        [JsonProperty("spouseWorkIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.SP.INC.WORK", false, DataDescription = "Assumed spouse income from work.")]
        public int? SpouseWorkIncome { get; set; }

        /// <summary>
        /// Assumed student additional financial info amount.
        /// </summary>        
        [JsonProperty("studentAddlFinInfoTotal", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.S.ADDL.FIN.AMT", false, DataDescription = "Assumed student additional financial info amount.")]
        public int? StudentAddlFinInfoTotal { get; set; }

        /// <summary>
        /// Assumed birth date prior.
        /// </summary>         
        [JsonProperty("birthDatePrior", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.BIRTH.DATE.PRIOR", false, DataDescription = "Assumed date of birth prior.", DataMaxLength = 10)]
        public AssumedYesNo? BirthDatePrior { get; set; }

        /// <summary>
        /// Assume the student is married/remarried.
        /// </summary>     
        [JsonProperty("studentMarried", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.S.MARRIED", false, DataDescription = "Assumed the student is married/remarried.", DataMaxLength = 10)]
        public AssumedYesNo? StudentMarried { get; set; }

        /// <summary>
        /// Assumed to have children that the student supports.
        /// </summary>     
        [JsonProperty("dependentChildren", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.CHILDREN", false, DataDescription = "Assumed to have children that the student supports.", DataMaxLength = 10)]
        public AssumedYesNo? DependentChildren { get; set; }

        /// <summary>
        /// Assumed to have legal dependents other than children or spouse.
        /// </summary>             
        [JsonProperty("otherDependents", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.LEGAL.DEP", false, DataDescription = "Assumed to have legal dependents other than children or spouse.")]
        public string OtherDependents { get; set; }

        /// <summary>
        /// Assumed student's number in family.
        /// </summary>             
        [JsonProperty("studentFamilySize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.S.NBR.FAMILY", false, DataDescription = "Assumed student's number in family.")]
        public int? studentFamilySize { get; set; }

        /// <summary>
        /// Assumed student's number in college.
        /// </summary>             
        [JsonProperty("studentNumberInCollege", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.S.NBR.COLLEGE", false, DataDescription = "Assumed student's number in college.")]
        public int? StudentNumberInCollege { get; set; }

        /// <summary>
        /// Assumed student's asset threshold exceeded.
        /// </summary>             
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("studentAssetThreshold", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.S.ASSET.THOLD.EXC", false, DataDescription = "Assumed student's asset threshold exceeded.")]
        public bool? StudentAssetThreshold { get; set; }

        /// <summary>
        /// Assumed parent's marital status.
        /// </summary>             
        [JsonProperty("parentMaritalStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.P.MAR.STAT", false, DataDescription = "Assumed parent's marital status.", DataMaxLength =25)]
        public AssumedParentMaritalStatus? ParentMaritalStatus { get; set; }

        /// <summary>
        /// Assumed parent 1 SSN.
        /// </summary>             
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("firstParentSsn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.PAR1.SSN", false, DataDescription = "Assumed parent 1 SSN.")]
        public bool? FirstParentSsn { get; set; }

        /// <summary>
        /// Assumed parent 2 SSN.
        /// </summary>             
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("secondParentSsn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.PAR2.SSN", false, DataDescription = "Assumed parent 2 SSN.")]
        public bool? SecondParentSsn { get; set; }

        /// <summary>
        /// Assumed parents' number in family.
        /// </summary>             
        [JsonProperty("parentFamilySize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.P.NBR.FAMILY", false, DataDescription = "Assumed parents' number in family.")]
        public int? ParentFamilySize { get; set; }

        /// <summary>
        /// Assumed parents' number in college.
        /// </summary>             
        [JsonProperty("parentNumCollege", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.P.NBR.COLLEGE", false, DataDescription = "Assumed parents' number in college.")]
        public int? ParentNumCollege { get; set; }

        /// <summary>
        /// Assumed parents' adjusted gross income.
        /// </summary>             
        [JsonProperty("parentAgi", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.P.AGI", false, DataDescription = "Assumed parents' adjusted gross income.")]
        public int? ParentAgi { get; set; }

        /// <summary>
        /// Assumed parents' U.S. taxes paid.
        /// </summary>             
        [JsonProperty("parentTaxPaid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.P.TAX.PD", false, DataDescription = "Assumed parents' U.S. taxes paid.")]
        public int? ParentTaxPaid { get; set; }

        /// <summary>
        /// Assumed parent 1 income from work.
        /// </summary>             
        [JsonProperty("firstParentWorkIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.PAR1.INCOME", false, DataDescription = "Assumed parent 1 income from work.")]
        public int? FirstParentWorkIncome { get; set; }

        /// <summary>
        /// Assumed parent 2 income from work.
        /// </summary>             
        [JsonProperty("secondParentWorkIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.PAR2.INCOME", false, DataDescription = "Assumed parent 2 income from work.")]
        public int? SecondParentWorkIncome { get; set; }

        /// <summary>
        /// Assumed parents' additional financial info amount.
        /// </summary>             
        [JsonProperty("parentAddlFinancial", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.P.ADDL.FIN.AMT", false, DataDescription = "Assumed parents' additional financial information amount.")]
        public int? ParentAddlFinancial { get; set; }

        /// <summary>
        /// Assumed parents' asset threshold exceeded.
        /// </summary>             
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("parentAssetThreshold", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ASSUM.P.ASSET.THOLD.EXC", false, DataDescription = "Assumed parents' asset threshold exceeded.")]
        public bool? ParentAssetThreshold { get; set; }

    }
}
