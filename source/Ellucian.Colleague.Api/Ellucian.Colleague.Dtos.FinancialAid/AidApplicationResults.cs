/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Information about an aid application results
    /// </summary>
    [DataContract]
    public class AidApplicationResults
    {
        /// <summary>
        /// The derived identifier for the resource.
        /// </summary>        
        [JsonProperty("id")]
        [Metadata("FAAPP.CALC.RESULTS.ID", true, DataDescription = "The derived identifier for the resource.")]
        public string Id { get; set; }

        /// <summary>
        /// Contains the sequential key to the FAAPP.DEMO entity.
        /// </summary>        
        [JsonProperty("appDemoId")]
        [FilterProperty("criteria")]
        [Metadata("FAAPP.DEMO.ID", true, DataDescription = "Contains the sequential key to the FAAPP.DEMO entity.")]
        public string AppDemoId { get; set; }

        /// <summary>
        /// The Key to PERSON.
        /// </summary>        
        [JsonProperty("personId")]
        [FilterProperty("criteria")]
        [Metadata("FAPR.STUDENT.ID", false, DataDescription = "The Key to PERSON.")]
        public string PersonId { get; set; }

        /// <summary>
        /// The type of application record.
        /// </summary>        
        [JsonProperty("applicationType")]
        [FilterProperty("criteria")]
        [Metadata("FAPR.TYPE", false, DataDescription = "The type of application record.")]
        public string ApplicationType { get; set; }

        /// <summary>
        /// Stores the year associated to the application.
        /// </summary>        
        [JsonProperty("aidYear")]
        [FilterProperty("criteria")]
        [Metadata("FAPR.YEAR", false, DataDescription = "The year associated to the application.")]
        public string AidYear { get; set; }

        /// <summary>
        /// The student's assigned ID.
        /// </summary>        
        [JsonProperty("applicantAssignedId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        [Metadata("FAAD.ASSIGNED.ID", false, DataDescription = "The student's assigned ID.")]
        public string ApplicantAssignedId { get; set; }

        /// <summary>
        /// Transaction number for this application.
        /// </summary>        
        [JsonProperty("transactionNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        [Metadata("FAPR.TRANS.NBR", false, DataDescription = "Transaction number for this application.")]
        public int? TransactionNumber { get; set; }

        /// <summary>
        /// Indicates that a dependency override was requested on this transaction.
        /// </summary>        
        [JsonProperty("dependencyOverride", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.DEP.OVER", false, DataDescription = "Indicates that a dependency override was requested on this transaction.")]
        public string DependencyOverride { get; set; }

        /// <summary>
        /// The school code of the school who requested the override.
        /// </summary>        
        [JsonProperty("dependencyOverSchoolCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.DEP.OVER.SCHOOL.CODE", false, DataDescription = "The school code of the school who requested the override.")]
        public string DependencyOverSchoolCode { get; set; }

        /// <summary>
        /// Dependency Status based on dependency status data provided.
        /// </summary>        
        [JsonProperty("dependencyStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.DEP.STATUS", false, DataDescription = "Dependency status based on dependency status data provided.")]
        public string DependencyStatus { get; set; }

        /// <summary>
        /// Indicates the origin of the transaction.
        /// </summary>        
        [JsonProperty("transactionSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.TRANS.DATA.SOURCE.TYPE", false, DataDescription = "Indicates the origin of the transaction.")]
        public string TransactionSource { get; set; }

        /// <summary>
        /// The date the transaction was received.
        /// </summary>        
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("transactionReceiptDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.TRANS.RCPT.DATE", false, DataDescription = "The date the transaction was received.", DataMaxLength = 50)]
        public DateTime? TransactionReceiptDate { get; set; }

        /// <summary>
        /// Indicates the special circumstances under which the parental data was submitted. 
        /// </summary>        
        [JsonProperty("specialCircumstances", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.SPECIAL.CIRCUMSTANCES", false, DataDescription = "Indicates the special circumstances under which the parental data was submitted.")]
        public string SpecialCircumstances { get; set; }

        /// <summary>
        /// Parent asset threshold exceeded.
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("parentAssetExceeded", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.P.ASSET.THOLD.EXC", false, DataDescription = "Parent asset threshold exceeded.")]
        public bool? ParentAssetExceeded { get; set; }

        /// <summary>
        /// Student asset threshold exceeded.
        /// </summary>        
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("studentAssetExceeded", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.S.ASSET.THOLD.EXC", false, DataDescription = "Student asset threshold exceeded.")]
        public bool? StudentAssetExceeded { get; set; }

        /// <summary>
        /// Electronic transaction indicator destination number.
        /// </summary>        
        [JsonProperty("destinationNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ETI.DEST.NBR", false, DataDescription = "Electronic transaction indicator destination number. This number always need to begin with TG.")]
        public string DestinationNumber { get; set; }

        /// <summary>
        /// Student's current pseudo ID number.
        /// </summary>        
        [JsonProperty("studentCurrentPseudoId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.CURR.PSEUDO.ID", false, DataDescription = "Student's current pseudo ID number.")]
        public string StudentCurrentPseudoId { get; set; }

        /// <summary>
        /// Correction applied against transaction number.
        /// </summary>        
        [JsonProperty("correctionAppliedAgainst", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.CORR.APPLIED.AGAINST", false, DataDescription = "Correction applied against transaction number.")]
        public string CorrectionAppliedAgainst { get; set; }

        /// <summary>
        /// Professional Judgment Indicates whether a FAA requested an EFC adjustment an if it was processed.
        /// </summary>        
        [JsonProperty("profJudgementIndicator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PROF.JUDG.IND", false, DataDescription = "Professional judgment indicates whether a FAA requested an EFC adjustment an if it was processed.",DataMaxLength =30)]        
        public JudgementIndicator? ProfJudgementIndicator { get; set; }

        /// <summary>
        /// Indicates the origin of the initial application.
        /// </summary>        
        [JsonProperty("applicationDataSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.APPL.DATA.SOURCE.TYPE", false, DataDescription = "Indicates the origin of the initial application.")]
        public string ApplicationDataSource { get; set; }

        /// <summary>
        /// Date the application was received.
        /// </summary>       
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("applicationReceiptDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.APPL.RCPT.DATE", false, DataDescription = "Date the application was received.")]
        public DateTime? ApplicationReceiptDate { get; set; }

        /// <summary>
        /// This field is set if an address is the only change to this record.
        /// </summary>        
        [JsonProperty("addressOnlyChangeFlag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ADDR.ONLY.CHG.FLAG", false, DataDescription = "This field is set if an address is the only change to this record.")]
        public string AddressOnlyChangeFlag { get; set; }

        /// <summary>
        /// Indicates whether the application was pushed to school.
        /// </summary>        
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("pushedApplicationFlag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PUSHED.ISIR.FLAG", false, DataDescription = "Indicates whether the application was pushed to school.")]
        public bool? PushedApplicationFlag { get; set; }

        /// <summary>
        /// Indicates that the EFC changed on this transaction.
        /// </summary>        
        [JsonProperty("efcChangeFlag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.EFC.CHG.FLAG", false, DataDescription = "Indicates that the EFC changed on this transaction.", DataMaxLength = 15 )]       
        public EfcChangeFlag? EfcChangeFlag { get; set; }

        /// <summary>
        /// Indicates that the student's last name has changed on this transaction.
        /// </summary>        
        [JsonProperty("lastNameChange", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.S.LAST.NAME.CHG.FLAG", false, DataDescription = "Indicates that the student's last name has changed on this transaction.", DataMaxLength = 15)]
        public LastNameChange? LastNameChange { get; set; }

        /// <summary>
        /// This field is set if the reject status changes from the transaction being corrected.
        /// </summary>        
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("rejectStatusChange", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.REJ.CHG.FLAG", false, DataDescription = "This field is set if the reject status changes from the transaction being corrected.")]
        public bool? RejectStatusChange { get; set; }

        /// <summary>
        /// This field is set if the SARC flag has changed from the last transaction.
        /// </summary>        
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("sarcChange", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.SARC.CHG.FLAG", false, false, 1, DataDescription = "This field is set if the SARC flag has changed from the last transaction.")]
        public bool? SarcChange { get; set; }

        /// <summary>
        /// Compute number assigned to the application.
        /// </summary>        
        [JsonProperty("computeNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.COMPUTE.NBR", false, DataDescription ="Compute number assigned to the application.")]
        public string ComputeNumber { get; set; }

        /// <summary>
        /// Source of correction.
        /// </summary>        
        [JsonProperty("correctionSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.CORR.SOURCE", false, DataDescription = "Source of correction.")]
        public string CorrectionSource { get; set; }

        /// <summary>
        /// Duplicate pseudo ID indicator.
        /// </summary>        
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("duplicateIdIndicator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.DUP.PID", false, DataDescription = "Duplicate pseudo ID indicator.")]
        public bool? DuplicateIdIndicator { get; set; }

        /// <summary>
        /// Graduate flag.
        /// </summary>        
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("graduateFlag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.GRAD.FLAG", false, DataDescription = "Graduate flag.")]
        public bool? GraduateFlag { get; set; }

        /// <summary>
        /// Date that this transaction was processed.
        /// </summary>  
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("transactionProcessedDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.TRANS.PROC.DATE", false, DataDescription = "Date that this transaction was processed.")]
        public DateTime? TransactionProcessedDate { get; set; }

        /// <summary>
        /// Indicates if this is an initial or a correction transaction.
        /// </summary>        
        [JsonProperty("processedRecordType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PROC.RECORD.TYPE", false, DataDescription = "Indicates if this is an initial or a correction transaction.")]
        public string ProcessedRecordType { get; set; }

        /// <summary>
        /// Indicates whether this transaction has a reason to be rejected.
        /// </summary>        
        [JsonProperty("rejectReasonCodes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.REJECT.CODES", false, DataDescription = "Indicates whether this transaction has a reason to be rejected.")]
        public List<string> RejectReasonCodes { get; set; }

        /// <summary>
        /// Indicates whether simplified needs test met and taxable income is under the threshold.
        /// </summary>        
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("automaticZeroIndicator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.AZE.IND", false, DataDescription = "Indicates whether simplified needs test was met and taxable income is under the threshold.")]
        public bool? AutomaticZeroIndicator { get; set; }

        /// <summary>
        /// Student meets certain criteria for tax filing status and income level and is not required to provide asset information.
        /// </summary>        
        [JsonProperty("simplifiedNeedsTest", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.SNT.IND", false, DataDescription = "Student meets certain criteria for tax filing status and income level and is not required to provide asset information.", DataMaxLength = 10)]        
        public SimplifiedNeedsTest? SimplifiedNeedsTest { get; set; }

        /// <summary>
        /// Parents’ calculated tax status for the aid year.
        /// </summary>        
        [JsonProperty("parentCalculatedTaxStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.P.CALC.TAX.STATUS", false, DataDescription = "Parents’ calculated tax status for the aid year.")]
        public string ParentCalculatedTaxStatus { get; set; }

        /// <summary>
        /// Student's calculated tax status for the aid year.
        /// </summary>        
        [JsonProperty("studentCalculatedTaxStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.S.CALC.TAX.STATUS", false, DataDescription = "Student's calculated tax status for the aid year.")]
        public string StudentCalculatedTaxStatus { get; set; }

        /// <summary>
        /// Student's additional financial information total calculated amount.
        /// </summary>        
        [JsonProperty("studentAddlFinCalcTotal", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.S.ADDL.FIN.INFO.TOTAL", false, DataDescription = "Student's additional financial information total calculated amount.")]
        public int? StudentAddlFinCalcTotal { get; set; }

        /// <summary>
        /// Student's calculated untaxed income total.
        /// </summary>        
        [JsonProperty("studentOthUntaxIncomeCalcTotal", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.S.OTH.UNTX.INC", false, DataDescription = "Student's calculated untaxed income total.")]
        public int? studentOthUntaxIncomeCalcTotal { get; set; }

        /// <summary>
        /// Parent's additional financial information total calculated amount.
        /// </summary>        
        [JsonProperty("parentAddlFinCalcTotal", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.P.ADDL.FIN.INFO.TOTAL", false, DataDescription = "Parent's additional financial information total calculated amount.")]
        public int? ParentAddlFinCalcTotal { get; set; }

        /// <summary>
        /// Parent's calculated untaxed income total.
        /// </summary>        
        [JsonProperty("parentOtherUntaxIncomeCalcTotal", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.P.OTH.UNTX.INC", false, DataDescription = "Parent's calculated untaxed income total.")]
        public int? ParentOtherUntaxIncomeCalcTotal { get; set; }

        /// <summary>
        /// Indicates if the high school code submitted was invalid.
        /// </summary>        
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("invalidHighSchool", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.HS.INVALID.FLAG", false, DataDescription = "Indicates if the high school code submitted was invalid.")]
        public bool? InvalidHighSchool { get; set; }

        /// <summary>
        /// Assumed student details.
        /// </summary>        
        [JsonProperty("assumed", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata()]
        public AssumedStudentDetails Assumed { get; set; }

        /// <summary>
        /// The primary 9-month calculated EFC.
        /// </summary>        
        [JsonProperty("primaryEfc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.EFC", false, DataDescription = "The primary 9-month calculated EFC.")]
        public int? PrimaryEfc { get; set; }

        /// <summary>
        /// The secondary 9-month calculated EFC.
        /// </summary>        
        [JsonProperty("secondaryEfc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.SEC.EFC", false, DataDescription = "The secondary 9-month calculated EFC.")]
        public int? SecondaryEfc { get; set; }

        /// <summary>
        /// Signature reject EFC.
        /// </summary>        
        [JsonProperty("signatureRejectEfc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.SIGN.REJ.EFC", false, DataDescription = "Signature reject EFC.")]
        public int? SignatureRejectEfc { get; set; }

        /// <summary>
        /// Identifies the formula type used to calculate the primary EFC.
        /// </summary>        
        [JsonProperty("primaryEfcType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.EFC.TYPE", false, DataDescription = "Identifies the formula type used to calculate the primary EFC.")]
        public string PrimaryEfcType { get; set; }

        /// <summary>
        /// Alternate primary EFC.
        /// </summary>        
        [JsonProperty("alternatePrimaryEfc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.EFC.TYPE", false, DataDescription = "Alternate primary EFC.")]
        public AlternatePrimaryEfc AlternatePrimaryEfc { get; set; }

        /// <summary>
        /// Primary EFC total income.
        /// </summary>             
        [JsonProperty("totalIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.TI", false, DataDescription = "Primary EFC total income.")]
        public int? TotalIncome { get; set; }

        /// <summary>
        /// Primary EFC allowances against total income.
        /// </summary>             
        [JsonProperty("allowancesAgainstTotalIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.ATI", false, DataDescription = "Primary EFC allowances against total income.")]
        public int? AllowancesAgainstTotalIncome { get; set; }

        /// <summary>
        /// Primary EFC state and other taxes.
        /// </summary>             
        [JsonProperty("taxAllowance", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.STX", false, DataDescription = "Primary EFC state and other taxes.")]
        public int? TaxAllowance { get; set; }

        /// <summary>
        /// Primary employment allowance.
        /// </summary>             
        [JsonProperty("employmentAllowance", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.EA", false, DataDescription = "Primary employment allowance.")]
        public int? EmploymentAllowance { get; set; }

        /// <summary>
        /// Primary income protection allowance.
        /// </summary>             
        [JsonProperty("incomeProtectionAllowance", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.IPA", false, DataDescription = "Primary income protection allowance.")]
        public int? IncomeProtectionAllowance { get; set; }

        /// <summary>
        /// Primary available income.
        /// </summary>             
        [JsonProperty("availableIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.AI", false, DataDescription = "Primary available income.")]
        public int? AvailableIncome { get; set; }

        /// <summary>
        /// Primary contribution from available income.
        /// </summary>             
        [JsonProperty("availableIncomeContribution", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.CAI", false, DataDescription = "Primary contribution from available income.")]
        public int? AvailableIncomeContribution { get; set; }

        /// <summary>
        /// Primary discretionary net worth.
        /// </summary>             
        [JsonProperty("discretionaryNetWorth", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.DNW", false, DataDescription = "Primary discretionary net worth.")]
        public int? DiscretionaryNetWorth { get; set; }

        /// <summary>
        /// Primary net worth.
        /// </summary>             
        [JsonProperty("netWorth", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.NW", false, DataDescription = "Primary net worth.")]
        public int? NetWorth { get; set; }

        /// <summary>
        /// Primary asset protection allowance.
        /// </summary>             
        [JsonProperty("assetProtectionAllowance", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.APA", false, DataDescription = "Primary asset protection allowance.")]
        public int? AssetProtectionAllowance { get; set; }

        /// <summary>
        /// Primary contribution from assets.
        /// </summary>             
        [JsonProperty("parentContributionAssets", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.PCA", false, DataDescription = "Primary contribution from assets.")]
        public int? ParentContributionAssets { get; set; }

        /// <summary>
        /// Primary adjusted available income.
        /// </summary>             
        [JsonProperty("adjustedAvailableIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.AAI", false, DataDescription = "Primary adjusted available income.")]
        public int? AdjustedAvailableIncome { get; set; }

        /// <summary>
        /// Primary total student contribution.
        /// </summary>             
        [JsonProperty("totalPrimaryStudentContribution", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.TSC", false, DataDescription = "Primary total student contribution.")]
        public int? TotalPrimaryStudentContribution { get; set; }

        /// <summary>
        /// Primary total parent contribution.
        /// </summary>             
        [JsonProperty("totalPrimaryParentContribution", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.TPC", false, DataDescription = "Primary total parent contribution.")]
        public int? TotalPrimaryParentContribution { get; set; }

        /// <summary>
        /// Parents' contribution.
        /// </summary>             
        [JsonProperty("parentContribution", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.PC", false, DataDescription = "Parents' contribution.")]
        public int? ParentContribution { get; set; }

        /// <summary>
        /// Primary student total income.
        /// </summary>             
        [JsonProperty("studentTotalIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.STI", false, DataDescription = "Primary student total income.")]
        public int? StudentTotalIncome { get; set; }

        /// <summary>
        /// Primary student allowance against total income.
        /// </summary>             
        [JsonProperty("studentAllowanceAgainstIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.SATI", false, DataDescription = "Primary student allowance against total income.")]
        public int? StudentAllowanceAgainstIncome { get; set; }

        /// <summary>
        /// Primary student income contribution.
        /// </summary>             
        [JsonProperty("dependentStudentIncContrib", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.SIC", false, DataDescription = "Primary student income contribution.")]
        public int? DependentStudentIncContrib { get; set; }

        /// <summary>
        /// Primary student discretionary net worth.
        /// </summary>             
        [JsonProperty("studentDiscretionaryNetWorth", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.SDNW", false, DataDescription = "Primary student discretionary net worth.")]
        public int? StudentDiscretionaryNetWorth { get; set; }

        /// <summary>
        /// Primary student contribution from assets.
        /// </summary>             
        [JsonProperty("studentAssetContribution", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.SCA", false, DataDescription = "Primary student contribution from assets.")]
        public int? StudentAssetContribution { get; set; }

        /// <summary>
        /// Primary FISAP total income.
        /// </summary>             
        [JsonProperty("fisapTotalIncome", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.PRI.FTI", false, DataDescription = "Primary FISAP total income.")]
        public int? FisapTotalIncome { get; set; }

        /// <summary>
        /// A string made of a 0/1 to identify which application fields were corrected.
        /// </summary>             
        [JsonProperty("correctionFlags", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.CORRECTION.FLAGS", false, DataDescription = "A string made of a 0/1 to identify which application fields were corrected.")]
        public string CorrectionFlags { get; set; }

        /// <summary>
        /// A string of 0/1 that identifies which fields should be highlighted.
        /// </summary>             
        [JsonProperty("highlightFlags", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.HIGHLIGHT.FLAGS", false, DataDescription = "A string of 0/1 that identifies which fields should be highlighted.")]
        public string HighlightFlags { get; set; }

        /// <summary>
        /// Comments provided to communicate important results and processing information.
        /// </summary>             
        [JsonProperty("commentCodes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.COMMENT.CODES", false, DataDescription = "Comments provided to communicate important results and processing information.")]
        public List<string> CommentCodes { get; set; }

        /// <summary>
        /// Electronic Federal School Code Indicator.
        /// </summary>             
        [JsonProperty("electronicFedSchoolCodeInd", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ELEC.SCHOOL.IND", false, DataDescription = "Electronic Federal School Code Indicator.")]
        public string ElectronicFedSchoolCodeInd { get; set; }

        /// <summary>
        /// The ETI is set to indicate if the school receiving the aid application submitted input to generate the aid application transaction, or did not generate the transaction but was listed on the record. 
        /// </summary>             
        [JsonProperty("electronicTransactionIndicator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.ETI.FLAG", false, DataDescription = "The ETI is set to indicate if the school receiving the aid application submitted input to generate the aid application transaction, or did not generate the transaction but was listed on the record.")]
        public string ElectronicTransactionIndicator { get; set; }

        /// <summary>
        /// Student is selected for verification.
        /// </summary>             
        [JsonProperty("verificationSelected", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAPR.SELECTED.FOR.VERIF", false, DataDescription = "Student is selected for verification.")]
        public string VerificationSelected { get; set; }
    }    
}
