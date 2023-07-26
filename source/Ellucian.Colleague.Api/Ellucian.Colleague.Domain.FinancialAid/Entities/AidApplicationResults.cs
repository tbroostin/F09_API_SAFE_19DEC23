// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// aid application results entity
    /// </summary>
    [Serializable]
    public class AidApplicationResults
    {
        /// <summary>
        /// The derived identifier for the resource.
        /// </summary>
        public string Id { get { return _id; } }
        private readonly string _id;
        /// <summary>
        /// Contains the sequential key to the FAAPP.DEMO entity.
        /// </summary>        
        public string AppDemoId { get { return _appDemoId; } }
        private readonly string _appDemoId;

        /// <summary>
        /// The Key to PERSON.
        /// </summary>        
        public string PersonId { get; set; }

        /// <summary>
        /// The type of application record.
        /// </summary>        
        public string AidApplicationType { get; set; }

        /// <summary>
        /// Stores the year associated to the application.
        /// </summary>        
        public string AidYear { get; set; }

        /// <summary>
        /// The student's assigned ID.
        /// </summary>        
        public string ApplicantAssignedId { get; set; }

        /// <summary>
        /// Transaction number for this application.
        /// </summary>        
        public int? TransactionNumber { get; set; }

        /// <summary>
        /// Indicates that a dependency override was requested on this transaction.
        /// </summary>        
        public string DependencyOverride { get; set; }

        /// <summary>
        /// The school code of the school who requested the override.
        /// </summary>        
        public string DependencyOverSchoolCode { get; set; }

        /// <summary>
        /// Dependency Status based on dependency status data provided.
        /// </summary>        
        public string DependencyStatus { get; set; }

        /// <summary>
        /// Indicates the origin of the transaction.
        /// </summary>        
        public string TransactionSource { get; set; }

        /// <summary>
        /// The date the transaction was received.
        /// </summary>        
        public DateTime? TransactionReceiptDate { get; set; }

        /// <summary>
        /// Indicates the special circumstances under which the parental data was submitted. 
        /// </summary>        
        public string SpecialCircumstances { get; set; }

        /// <summary>
        /// Parent asset threshold exceeded.
        /// </summary>        
        public bool? ParentAssetExceeded { get; set; }

        /// <summary>
        /// Student asset threshold exceeded.
        /// </summary>        
        public bool? StudentAssetExceeded { get; set; }

        /// <summary>
        /// Electronic transaction indicator destination number.
        /// </summary>        
        public string DestinationNumber { get; set; }

        /// <summary>
        /// Student's current pseudo ID number.
        /// </summary>        
        public string StudentCurrentPseudoId { get; set; }

        /// <summary>
        /// Correction applied against transaction number.
        /// </summary>        
        public string CorrectionAppliedAgainst { get; set; }

        /// <summary>
        /// Professional Judgment Indicates whether a FAA requested an EFC adjustment an if it was processed.
        /// </summary>    
        public string ProfJudgementIndicator { get; set; }

        /// <summary>
        /// Indicates the origin of the initial application.
        /// </summary>        
        public string ApplicationDataSource { get; set; }

        /// <summary>
        /// Date the application was received.
        /// </summary>        
        public DateTime? ApplicationReceiptDate { get; set; }

        /// <summary>
        /// This field is set if an address is the only change to this record.
        /// </summary>        
        public string AddressOnlyChangeFlag { get; set; }

        /// <summary>
        /// Indicates whether the application was pushed to school.
        /// </summary>        
        public bool? PushedApplicationFlag { get; set; }

        /// <summary>
        /// Indicates that the EFC changed on this transaction.
        /// </summary>        
        public string EfcChangeFlag { get; set; }

        /// <summary>
        /// Indicates that the student's last name has changed on this transaction.
        /// </summary>        
        public string LastNameChange { get; set; }

        /// <summary>
        /// This field is set if the reject status changes from the transaction being corrected.
        /// </summary>        
        public bool? RejectStatusChange { get; set; }

        /// <summary>
        /// This field is set if the SARC flag has changed from the last transaction.
        /// </summary>        
        public bool? SarcChange { get; set; }

        /// <summary>
        /// Compute number assigned to the application.
        /// </summary>        
        public string ComputeNumber { get; set; }

        /// <summary>
        /// Source of correction.
        /// </summary>        
        public string CorrectionSource { get; set; }

        /// <summary>
        /// Duplicate pseudo ID indicator.
        /// </summary>        
        public bool? DuplicateIdIndicator { get; set; }

        /// <summary>
        /// Graduate flag.
        /// </summary>        
        public bool? GraduateFlag { get; set; }

        /// <summary>
        /// Date that this transaction was processed.
        /// </summary>        
        public DateTime? TransactionProcessedDate { get; set; }

        /// <summary>
        /// Indicates if this is an initial or a correction transaction.
        /// </summary>        
        public string ProcessedRecordType { get; set; }

        /// <summary>
        /// Indicates whether this transaction has a reason to be rejected.
        /// </summary>        
        public List<string> RejectReasonCodes { get; set; }

        /// <summary>
        /// Indicates whether simplified needs test met and taxable income is under the threshold.
        /// </summary>        
        public bool? AutomaticZeroIndicator { get; set; }

        /// <summary>
        /// Student meets certain criteria for tax filing status and income level and is not required to provide asset information.
        /// </summary>               
        public string SimplifiedNeedsTest { get; set; }

        /// <summary>
        /// Parents’ calculated tax status for the aid year.
        /// </summary>        
        public string ParentCalculatedTaxStatus { get; set; }

        /// <summary>
        /// Student's calculated tax status for the aid year.
        /// </summary>        
        public string StudentCalculatedTaxStatus { get; set; }

        /// <summary>
        /// Student's additional financial information total calculated amount.
        /// </summary>        
        public int? StudentAddlFinCalcTotal { get; set; }

        /// <summary>
        /// Student's calculated untaxed income total.
        /// </summary>        
        public int? studentOthUntaxIncomeCalcTotal { get; set; }

        /// <summary>
        /// Parent's additional financial information total calculated amount.
        /// </summary>        
        public int? ParentAddlFinCalcTotal { get; set; }

        /// <summary>
        /// Parent's calculated untaxed income total.
        /// </summary>        
        public int? ParentOtherUntaxIncomeCalcTotal { get; set; }

        /// <summary>
        /// Indicates if the high school code submitted was invalid.
        /// </summary>        
        public bool? InvalidHighSchool { get; set; }

        /// <summary>
        /// Assumed citizenship.
        /// </summary>        
        public string AssumCitizenship { get; set; }

        //ENUM
        /// <summary>
        /// Assumed student marital status.
        /// </summary>        
        public string AssumSMarStat { get; set; }

        /// <summary>
        /// Assumed student AGI.
        /// </summary>        
        public int? AssumSAgi { get; set; }

        /// <summary>
        /// Assumed student taxes paid.
        /// </summary>        
        public int? AssumSTaxPd { get; set; }

        /// <summary>
        /// Assumed student income from work.
        /// </summary>        
        public int? AssumSIncWork { get; set; }

        /// <summary>
        /// Assumed spouse income from work.
        /// </summary>        
        public int? AssumSpIncWork { get; set; }

        /// <summary>
        /// Assumed student additional financial info amount.
        /// </summary>        
        public int? AssumSAddlFinAmt { get; set; }

        /// <summary>
        /// Assumed birth date prior.
        /// </summary>         
        public string AssumBirthDatePrior { get; set; }

        /// <summary>
        /// AAssume the student is married/remarried.
        /// </summary>     
        public string AssumSMarried { get; set; }

        /// <summary>
        /// Assumed to have children that the student supports.
        /// </summary>     
        public string AssumChildren { get; set; }

        /// <summary>
        /// Assumed to have legal dependents other than children or spouse.
        /// </summary>             
        public string AssumLegalDep { get; set; }

        /// <summary>
        /// Assumed student's number in family.
        /// </summary>             
        public int? AssumSNbrFamily { get; set; }

        /// <summary>
        /// Assumed student's number in college.
        /// </summary>             
        public int? AssumSNbrCollege { get; set; }

        /// <summary>
        /// Assumed student's asset threshold exceeded.
        /// </summary>             
        public bool? AssumSAssetTholdExc { get; set; }

        /// <summary>
        /// Assumed parent's marital status.
        /// </summary>             
        public string AssumPMarStat { get; set; }

        /// <summary>
        /// Assumed parent 1 SSN.
        /// </summary>             
        public bool? AssumPar1Ssn { get; set; }

        /// <summary>
        /// Assumed parent 2 SSN.
        /// </summary>             
        public bool? AssumPar2Ssn { get; set; }

        /// <summary>
        /// Assumed parents' number in family.
        /// </summary>             
        public int? AssumPNbrFamily { get; set; }

        /// <summary>
        /// Assumed parents' number in college.
        /// </summary>             
        public int? AssumPNbrCollege { get; set; }

        /// <summary>
        /// Assumed parents' adjusted gross income.
        /// </summary>             
        public int? AssumPAgi { get; set; }

        /// <summary>
        /// Assumed parents' U.S. taxes paid.
        /// </summary>             
        public int? AssumPTaxPd { get; set; }

        /// <summary>
        /// Assumed parent 1 income from work.
        /// </summary>             
        public int? AssumPar1Income { get; set; }

        /// <summary>
        /// Assumed parent 2 income from work.
        /// </summary>             
        public int? AssumPar2Income { get; set; }

        /// <summary>
        /// Assumed parents' additional financial info amount.
        /// </summary>             
        public int? AssumPAddlFinAmt { get; set; }

        /// <summary>
        /// Assumed parents' asset threshold exceeded.
        /// </summary>             
        public bool? AssumPAssetTholdExc { get; set; }

        /// <summary>
        /// The primary 9-month calculated EFC.
        /// </summary>        
        public int? PrimaryEfc { get; set; }

        /// <summary>
        /// The secondary 9-month calculated EFC.
        /// </summary>        
        public int? SecondaryEfc { get; set; }

        /// <summary>
        /// Signature reject EFC.
        /// </summary>        
        public int? SignatureRejectEfc { get; set; }

        /// <summary>
        /// Identifies the formula type used to calculate the primary EFC.
        /// </summary>        
        public string PrimaryEfcType { get; set; }

        /// <summary>
        /// Primary alternate EFC for 1 month.
        /// </summary>             
        public int? PriAlt1mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 2 months.
        /// </summary>             
        public int? PriAlt2mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 3 months.
        /// </summary>             
        public int? PriAlt3mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 4 months.
        /// </summary>             
        public int? PriAlt4mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 5 months.
        /// </summary>             
        public int? PriAlt5mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 6 months.
        /// </summary>             
        public int? PriAlt6mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 7 months.
        /// </summary>             
        public int? PriAlt7mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 8 months.
        /// </summary>             
        public int? PriAlt8mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 10 months.
        /// </summary>             
        public int? PriAlt10mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 11 months.
        /// </summary>             
        public int? PriAlt11mnthEfc { get; set; }

        /// <summary>
        /// Primary alternate EFC for 12 months.
        /// </summary>             
        public int? PriAlt12mnthEfc { get; set; }

        /// <summary>
        /// Primary EFC total income.
        /// </summary>             
        public int? TotalIncome { get; set; }

        /// <summary>
        /// Primary EFC allowances against total income.
        /// </summary>             
        public int? AllowancesAgainstTotalIncome { get; set; }

        /// <summary>
        /// Primary EFC state and other taxes.
        /// </summary>             
        public int? TaxAllowance { get; set; }

        /// <summary>
        /// PPrimary employment allowance.
        /// </summary>             
        public int? EmploymentAllowance { get; set; }

        /// <summary>
        /// Primary income protection allowance.
        /// </summary>             
        public int? IncomeProtectionAllowance { get; set; }

        /// <summary>
        /// Primary available income.
        /// </summary>             
        public int? AvailableIncome { get; set; }

        /// <summary>
        /// Primary contribution from available income.
        /// </summary>             
        public int? AvailableIncomeContribution { get; set; }

        /// <summary>
        /// Primary discretionary net worth.
        /// </summary>             
        public int? DiscretionaryNetWorth { get; set; }

        /// <summary>
        /// Primary net worth.
        /// </summary>             
        public int? NetWorth { get; set; }

        /// <summary>
        /// Primary asset protection allowance.
        /// </summary>             
        public int? AssetProtectionAllowance { get; set; }

        /// <summary>
        /// Primary contribution from assets.
        /// </summary>             
        public int? ParentContributionAssets { get; set; }

        /// <summary>
        /// Primary adjusted available income.
        /// </summary>             
        public int? AdjustedAvailableIncome { get; set; }

        /// <summary>
        /// Primary total student contribution.
        /// </summary>             
        public int? TotalPrimaryStudentContribution { get; set; }

        /// <summary>
        /// Primary total parent contribution.
        /// </summary>             
        public int? TotalPrimaryParentContribution { get; set; }

        /// <summary>
        /// Parents' contribution.
        /// </summary>             
        public int? ParentContribution { get; set; }

        /// <summary>
        /// Primary student total income.
        /// </summary>             
        public int? StudentTotalIncome { get; set; }

        /// <summary>
        /// Primary student allowance against total income.
        /// </summary>             
        public int? StudentAllowanceAgainstIncome { get; set; }

        /// <summary>
        /// Primary student income contribution.
        /// </summary>             
        public int? DependentStudentIncContrib { get; set; }

        /// <summary>
        /// Primary student discretionary net worth.
        /// </summary>             
        public int? StudentDiscretionaryNetWorth { get; set; }

        /// <summary>
        /// Primary student contribution from assets.
        /// </summary>             
        public int? StudentAssetContribution { get; set; }

        /// <summary>
        /// Primary FISAP total income.
        /// </summary>             
        public int? FisapTotalIncome { get; set; }

        /// <summary>
        /// A string made of a 0/1 to identify which application fields were corrected.
        /// </summary>             
        public string CorrectionFlags { get; set; }

        /// <summary>
        /// A string of 0/1 that identifies which fields should be highlighted.
        /// </summary>             
        public string HighlightFlags { get; set; }

        /// <summary>
        /// Comments provided to communicate important results and processing information.
        /// </summary>             
        public List<string> CommentCodes { get; set; }

        /// <summary>
        /// Electronic Federal School Code Indicator.
        /// </summary>             
        public string ElectronicFedSchoolCodeInd { get; set; }

        /// <summary>
        /// The ETI is set to indicate if the school receiving the aid application submitted input to generate the aid application transaction, or did not generate the transaction but was listed on the record. 
        /// </summary>             
        public string ElectronicTransactionIndicator { get; set; }

        /// <summary>
        /// Student is selected for verification.
        /// </summary>             
        public string VerificationSelected { get; set; }

        /// <summary>
        /// constructor to initialize properties
        /// </summary>
        /// <param name="id">Id of the record</param>
        /// <param name="appDemoId">AppDemo Id</param>
        public AidApplicationResults(string id, string appDemoId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (appDemoId is null)
            {
                throw new ArgumentNullException("appDemoId");
            }

            _id = id;
            _appDemoId = appDemoId;
        }
    }
}
