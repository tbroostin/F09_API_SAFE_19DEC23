//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Define the Student FAFSA Domain Entity
    /// </summary>
    [Serializable]
    public class Fafsa : FinancialAidApplication
    {
        /// <summary>
        /// True or False, is the student Pell Eligible
        /// </summary>
        public bool IsPellEligible { get; set; }
        /// <summary>
        /// Parents Adjusted Gross Income
        /// </summary>
        public int? ParentsAdjustedGrossIncome { get; set; }
        /// <summary>
        /// Students Adjusted Gross Income
        /// </summary>
        public int? StudentsAdjustedGrossIncome { get; set; }

        /// <summary>
        /// A dictionary where keys are TitleIV codes and values are
        /// associated housing codes
        /// </summary>
        public Dictionary<string, HousingCode?> HousingCodes { get; set; }

        /// <summary>
        /// Unique identifier (GUID) for Financial Aid Application 
        ///  
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// The date the application was completed
        /// </summary>
        public DateTime? ApplicationCompletedOn { get; set; }

        public string FafsaPrimaryId { get; set; }
        public string FafsaPrimaryIdCorrected { get; set; }
        public string FafsaPrimaryType { get; set; }
        
        /// <summary>
        /// The applicant's state of legal residence.
        /// </summary>
        public string StateOfLegalResidence { get; set; }

        /// <summary>
        /// The type of financial aid application (ISIR, CPSSG, CORR, PROF, IAPP, SUPP)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The Federal Isir Id on CS.ACYR of applicant
        /// </summary>
        public string CsFederalIsirId { get; set; }

        /// <summary>
        /// The Institutional Isir Id on CS.ACYR of applicant
        /// </summary>
        public string CsInstitutionalIsirId { get; set; }

        /// <summary>
        /// The housing code on CS.ACYR of applicant
        /// </summary>
        public string HousingCode { get; set; }

        /// <summary>
        /// The corrected from fafsa Id
        /// </summary>
        public string CorrectedFromId { get; set; }

        // ISIR.FAFA information for determining independence criteria
        public bool IsAtRiskHomeless { get; set; }
        public bool IsAdvancedDegreeStudent { get; set; }
        public bool HasDependentChildren { get; set; }
        public bool HasOtherDependents { get; set; }
        public bool IsOrphanOrWard { get; set; }
        public bool IsEmancipatedMinor { get; set; }
        public bool HasGuardian { get; set; }
        public bool IsHomelessBySchool { get; set; }
        public bool IsHomelessByHud { get; set; }
        public bool IsBornBeforeDate { get; set; }
        public bool IsMarried { get; set; }
        public bool IsVeteran { get; set; }
        public bool IsActiveDuty { get; set; }

        // ISIR.PROFILE information for determining independence criteria
        public bool HasDependentChildrenProfile { get; set; }
        public bool IsWardProfile { get; set; }
        public bool IsHomelessProfile { get; set; }
        public bool IsVeteranProfile { get; set; }
        public string MaritalStatusProfile { get; set; }

        public string StateOfLegalResidenceProfile { get; set; }
        public DateTime? ApplicationCompletedOnProfile { get; set; }

        public string WorkStudyInterest { get; set; }

        public string StudentTaxReturnStatus { get; set; }
        public string StudentTaxReturnStatusProfile { get; set; }
        public string ParentTaxReturnStatus { get; set; }
        public string ParentTaxReturnStatusProfile { get; set; }
        public string ParentTaxReturnStatusProfileNcp { get; set; }

        public int? StudentAdjustedGrossIncome { get; set; }
        public long? StudentAdjustedGrossIncomeProfile { get; set; }
        public int? ParentAdjustedGrossIncome { get; set; }
        public long? ParentAdjustedGrossIncomeProfile { get; set; }
        public long? ParentAdjustedGrossIncomeProfileNcp { get; set; }

        public long? StudentTotalIncomeProfileCorrected { get; set; }
        public long? StudentTotalIncomeProfileOrig { get; set; }
        public long? StudentTotalIncome { get; set; }
        public long? PrimaryTotalIncome { get; set; }
        public string StudentDependencyStatus { get; set; }
        public string StudentDepdendencyStatusInas { get; set; }

        public long? ParentTotalIncomeProfileCorrected { get; set; }
        public long? ParentTotalIncomeProfileOrig { get; set; }
        public long? ParentPrimaryTotalIncome { get; set; }

        public long? NoncustodialParentTotalIncomeProfile { get; set; }

        public int? StudentEarnedIncome { get; set; }
        public long? StudentEarnedIncomeProfile { get; set; }
        public int? SpouseEarnedIncome { get; set; }
        public long? SpouseEarnedIncomeProfile { get; set; }

        public int? Parent1EarnedIncome { get; set; }
        public long? Parent1EarnedIncomeProfile { get; set; }
        public int? Parent2EarnedIncome { get; set; }
        public long? Parent2EarnedIncomeProfile { get; set; }

        public long? NoncustodialParent1EarnedIncomeProfile { get; set; }
        public long? NoncustodialParent2EarnedIncomeProfile { get; set; }

        public string Parent1EducationLevel { get; set; }
        public string Parent1EducationLevelProfile { get; set; }

        public string Parent2EducationLevel { get; set; }
        public string Parent2EducationLevelProfile { get; set; }

        //
        // For financial aid application outcomes:
        //
        public string CalcResultsGuid { get; set; }
        public List<string> RejectionCodes { get; set; }
        public bool HasStudentAidReportC { get; set; }
        public string StudentInstitutionalNeedAnalysisSystemDependencyStatus { get; set; }
        public string StudentDependencyOverride { get; set; }
        public bool HasAutomaticZeroExpectedFamilyContribution { get; set; }
        public bool HasMetSimpleNeed { get; set; }
        public string FinancialAidAAministratorAdjustment { get; set; }
        public bool HasVerificationSelection { get; set; }
        public bool HasVerificationSelectionOriginal { get; set; }
        public string VerificationTracking { get; set; }
        public string VerificationTrackingOriginal { get; set; } 
        public long? InstitutionalNeedAnalysisParentsContribution { get; set; }
        public long? InstitutionalNeedAnalysisStudentContribution { get; set; }
        public long? InstitutionFamilyContributionOverrideAmount { get; set; }
        public long? NonCustodialParentCalculatedContributionNcp { get; set; }
        public long? NonCustodialParentOverrideAmountNcp { get; set; }
        public long? NonCustodialParentOverrideAmount { get; set; }
        public long? FisapTotalIncome { get; set; }
        public long? ParentContribution { get; set; }
        public long? StudentContribution { get; set; }
        public string NonCustodialParentContribution { get; set; }
        public long? ParentHomeDebt { get; set; }
        public long? ParentHomeValue { get; set; }
        public long? ParentHomeDebtNcp { get; set; }
        public long? ParentHomeValueNcp { get; set; }
        public long? StudentHomeDebt { get; set; }
        public long? StudentHomeValue { get; set; }
        public long? CfsParentOptionalImCalculation { get; set; }
        public long? CfsStudentOptionalImCalculation { get; set; }
        public bool HasNonCustodialParentProfile { get; set; }
        public bool HasIsirResults { get; set; }
        public bool HasIsirResultsOriginal { get; set; }
        public bool IsPellEligibleOriginal { get; set; }
        public string FafsaMaritalStatus { get; set; }
        public string ProfileMaritalStatus { get; set; }
        public int? ApplicantFamilySize { get; set; }
        public int? ParentFamilySize { get; set; }
        public int? ApplicantNumberInCollege { get; set; }
        public int? ParentNoInCollege { get; set; }
        public string FatherEducationLevel { get; set; }
        public string MotherEducationLevel { get; set; }

        /// <summary>
        /// Constructor for FinancialAidApplication object used for Ethos Data Model APIs.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <param name="awardYear"></param>
        /// <param name="guid"></param>
        public Fafsa(string id, string studentId, string awardYear, string guid)
            : base(id, awardYear, studentId)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", string.Format("Application GUID is empty or missing for ISIR.FAFSA {0}.", id));
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", string.Format("Student Id is empty or missing for ISIR.FAFSA {0}.", id));
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear", string.Format("Award Year is empty or missing for ISIR.FAFSA {0}.", id));
            }
            Guid = guid;
            IsPellEligible = false;
            HousingCodes = new Dictionary<string, HousingCode?>();
        }

        /// <summary>
        /// Create a new Fafsa object.
        /// </summary>
        /// <param name="id">Unique database record identifier</param>
        /// <param name="studentId">Student Id for Fafsa data</param>
        /// <param name="awardYear">Award Year to be use in Fafsa data</param>
        public Fafsa(string id, string awardYear, string studentId)
            : base(id, awardYear, studentId)
        {
            IsPellEligible = false;
            HousingCodes = new Dictionary<string, HousingCode?>();
        }

        /// <summary>
        /// Compares two FAFSA objects to determine their equality. This method
        /// calls the inherited FinancialAidApplication Equals method.
        /// </summary>
        /// <param name="obj">The Fafsa object to compare to this one</param>
        /// <returns>True if the Fafsa objects are equal. False, otherwise.</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Computes the HashCode of this Fafsa object. This method calls the inherited
        /// FinancialAidApplication GetHashCode method.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Computes the string representation of this Fafsa object. This method calls the inherited
        /// FinancialAidApplication ToString method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
