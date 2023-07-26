// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Aid Applications entity
    /// </summary>
    [Serializable]
    public class AidApplications
    {
        // Required fields

        /// <summary>
        /// Unique ID of the entity
        /// </summary>
        public string Id { get { return _id; } }
        private readonly string _id;

        /// <summary>
        /// Contains the sequential key to the FAAPP.DEMO entity
        /// </summary>        
        public string AppDemoId { get { return _appDemoId; } }
        private readonly string _appDemoId;

        /// <summary>
        /// Person related to this applicantion
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Type associated to the application.
        /// </summary>
        public string AidApplicationType { get; set; }

        /// <summary>
        /// Financial aid year associated to the application
        /// </summary>
        public string AidYear { get; set; }

        // Non-required fields

        /// <summary>
        /// Assigned ID of student
        /// </summary>
        public string AssignedID { get; set; }

        /// <summary>
        /// First Bachelors Degree
        /// </summary>
        public bool? DegreeBy { get; set; }

        /// <summary>
        /// Grade level in college
        /// </summary>
        public string GradeLevelInCollege { get; set; }

        /// <summary>
        /// Degree or Certificate
        /// </summary>
        public string DegreeOrCertificate { get; set; }

        /// <summary>
        /// Born Before 01-01-2000
        /// </summary>
        public bool? BornBefore { get; set; }

        /// <summary>
        /// Is Student Married?
        /// </summary>
        public bool? Married { get; set; }

        /// <summary>
        /// Working on a Master's or Doctorate Program?
        /// </summary>
        public bool? GradOrProfProgram { get; set; }

        /// <summary>
        /// Are you on active duty in U.S. Armed Forces?
        /// </summary>
        public bool? ActiveDuty { get; set; }

        /// <summary>
        /// Veteran of U.S. Armed Forces?
        /// </summary>
        public bool? UsVeteran { get; set; }

        /// <summary>
        /// Have Children You Support?
        /// </summary>
        public bool? DependentChildren { get; set; }

        /// <summary>
        /// Have Legal Dependents Other than Children or Spouse?
        /// </summary>
        public bool? OtherDependents { get; set; }

        /// <summary>
        /// Orphan, Ward of Court, or Foster Care?
        /// </summary>
        public bool? OrphanWardFoster { get; set; }

        /// <summary>
        ///  Student is an emancipated minor?
        /// </summary>
        public bool? EmancipatedMinor { get; set; }

        /// <summary>
        /// Student is in legal guardianship?
        /// </summary>
        public bool? LegalGuardianship { get; set; }

        /// <summary>
        /// Unaccompanied youth determined by school district liason?
        /// </summary>
        public bool? HomelessBySchool { get; set; }

        /// <summary>
        /// Unaccompanied youth determined by HUD?
        /// </summary>
        public bool? HomelessByHud { get; set; }

        /// <summary>
        /// At risk of homelessness?
        /// </summary>
        public bool? HomelessAtRisk { get; set; }

        /// <summary>
		/// Student Tax Return filed 
		/// </summary>
        public string StudentTaxReturnFiled { get; set; }

        /// <summary>
		/// Student Tax Return filed 
		/// </summary>
        public string StudentTaxFormType { get; set; }

        /// <summary>
		/// Student Tax filing status 
		/// </summary>
        public string StudentTaxFilingStatus { get; set; }

        /// <summary>
		/// Student Schedule1 
		/// </summary>
        public string StudentSched1 { get; set; }

        /// <summary>
		/// Student AGI 
		/// </summary>
        public int? StudentAgi { get; set; }

        /// <summary>
		/// Student US Tax paid 
		/// </summary>
        public int? StudentUsTaxPd { get; set; }

        /// <summary>
		/// Student INC
		/// </summary>
        public int? SStudentInc { get; set; }

        /// <summary>
        /// Student Spouse Inc
        /// </summary>
        public int? SpouseInc { get; set; }

        /// <summary>
        /// Student Cash
        /// </summary>
        public int? StudentCash { get; set; }

        /// <summary>
        /// Student invested net worth
        /// </summary>
        public int? StudentInvNetWorth { get; set; }

        /// <summary>
        /// Student business net worth
        /// </summary>
        public int? StudentBusNetWorth { get; set; }

        /// <summary>
		/// Student Educational Credit
		/// </summary>
        public int? StudentEduCredit { get; set; }

        /// <summary>
        /// Student child support paid
        /// </summary>
        public int? StudentChildSupPaid { get; set; }

        /// <summary>
        /// Student Need Based Emp
        /// </summary>
        public int? StudentNeedBasedEmp { get; set; }

        /// <summary>
        /// Student Grant School Aid
        /// </summary>
        public int? StudentGrantScholAid { get; set; }

        /// <summary>
        /// Student Combat pay
        /// </summary>
        public int? StudentCombatPay { get; set; }

        /// <summary>
		/// Student Co-op Earnings
		/// </summary>
        public int? StudentCoOpEarnings { get; set; }

        /// <summary>
        /// Student Pension Payments
        /// </summary>
        public int? StudentPensionPayments { get; set; }

        /// <summary>
        /// Student IRA payments
        /// </summary>
        public int? StudentIraPayments { get; set; }

        /// <summary>
        /// Student Child Support Received 
        /// </summary>
        public int? StudentChildSupRecv { get; set; }

        /// <summary>
        /// Student Interest Income
        /// </summary>
        public int? StudentInterestIncome { get; set; }

        /// <summary>
        /// Student Unpaid tax IRA pen
        /// </summary>
        public int? StudentUntxIraPen { get; set; }

        /// <summary>
		/// Student Vet Non-Ed Benefit
		/// </summary>
        public int? StudentVetNonEdBen { get; set; }

        /// <summary>
        /// Student Other Untaxed Income
        /// </summary>
        public int? StudentOtherUntaxedInc { get; set; }

        /// <summary>
        /// Student Other Non-Rep Money
        /// </summary>
        public int? StudentOtherNonRepMoney { get; set; }

        /// <summary>
        /// Student Military and Clergy Allowances 
        /// </summary>
        public int? StudentMilitaryClergyAllow { get; set; }

        /// <summary>
		/// Student Legal residency state
		/// </summary>
        public string StudentLegalResSt { get; set; }

        /// <summary>
        /// Student Legal resident before date
        /// </summary>
        public bool? StudentLegalResB4 { get; set; }

        /// <summary>
        /// Student legal residency date
        /// </summary>
        public string StudentLegalResDate { get; set; }

        /// <summary>
		/// Parent 1 Grade level
		/// </summary>
        public string P1GradeLvl { get; set; }

        /// <summary>
        /// Parent 2 Grade levl
        /// </summary>
        public string P2GradeLvl { get; set; }

        /// <summary>
        ///  HighSchool Grad Type
        /// </summary>
        public string HsGradType { get; set; }

        /// <summary>
		/// High School Name
		/// </summary>
        public string HsName { get; set; }

        /// <summary>
        /// High School City
        /// </summary>
        public string HsCity { get; set; }

        /// <summary>
        /// High School State
        /// </summary>
        public string HsState { get; set; }

        /// <summary>
        /// High School Code
        /// </summary>
        public string HsCode { get; set; }

        /// <summary>
        /// Student's Number of Family Members
        /// </summary>
        public int? StudentNumberInFamily { get; set; }

        /// <summary>
        /// Student's Number in College
        /// </summary>
        public int? StudentNumberInCollege { get; set; }

        /// <summary>
		/// Student SSI Benefit
		/// </summary>
        public bool? SSsiBen { get; set; }

        /// <summary>
        /// Student food stamps
        /// </summary>
        public bool? SFoodStamps { get; set; }

        /// <summary>
		/// Students Lunch benefit
		/// </summary>
        public bool? SLunchBen { get; set; }

        /// <summary>
        /// Student Tranf benefit
        /// </summary>
        public bool? STanf { get; set; }

        /// <summary>
        /// Students WIC
        /// </summary>
        public bool? SWic { get; set; }

        /// <summary>
        /// Students Dislocate worker
        /// </summary>
        public string SDislWorker { get; set; }

        /// <summary>
        /// Federal School Code #1
        /// </summary>
        public string SchoolCode1 { get; set; }

        /// <summary>
        /// Federal School Code #1 Housing Plans
        /// </summary>
        public string HousingPlan1 { get; set; }

        /// <summary>
        /// Federal School Code #2
        /// </summary>
        public string SchoolCode2 { get; set; }

        /// <summary>
        /// Federal School Code #3 Housing Plans
        /// </summary>
        public string HousingPlan2 { get; set; }

        /// <summary>
        /// Federal School Code #3
        /// </summary>
        public string SchoolCode3 { get; set; }

        /// <summary>
        ///  Federal School Code #3 Housing Plans
        /// </summary>
        public string HousingPlan3 { get; set; }

        /// <summary>
        /// Federal School Code #4
        /// </summary>
        public string SchoolCode4 { get; set; }

        /// <summary>
        ///  Federal School Code #4 Housing Plans
        /// </summary>
        public string HousingPlan4 { get; set; }

        /// <summary>
        /// Federal School Code #5
        /// </summary>
        public string SchoolCode5 { get; set; }

        /// <summary>
        ///  Federal School Code #5 Housing Plans
        /// </summary>
        public string HousingPlan5 { get; set; }

        /// <summary>
        /// Federal School Code #6
        /// </summary>
        public string SchoolCode6 { get; set; }

        /// <summary>
        ///  Federal School Code #6 Housing Plans
        /// </summary>
        public string HousingPlan6 { get; set; }

        /// <summary>
        /// Federal School Code #7
        /// </summary>
        public string SchoolCode7 { get; set; }

        /// <summary>
        /// Federal School Code #7 Housing Plans
        /// </summary>
        public string HousingPlan7 { get; set; }

        /// <summary>
        /// Federal School Code #8
        /// </summary>
        public string SchoolCode8 { get; set; }

        /// <summary>
        /// Federal School Code #8 Housing Plans
        /// </summary>
        public string HousingPlan8 { get; set; }

        /// <summary>
        /// Federal School Code #9
        /// </summary>
        public string SchoolCode9 { get; set; }

        /// <summary>
        /// Federal School Code #9 Housing Plans
        /// </summary>
        public string HousingPlan9 { get; set; }

        /// <summary>
        /// Federal School Code #10
        /// </summary>
        public string SchoolCode10 { get; set; }

        /// <summary>
        /// Federal School Code #10 Housing Plans
        /// </summary>
        public string HousingPlan10 { get; set; }

        /// <summary>
        /// Student Marital Status
        /// </summary>
        public string StudentMaritalStatus { get; set; }

        /// <summary>
        /// Student Marital Date
        /// </summary>
        public string StudentMaritalDate { get; set; }

        /// <summary>
        /// Date the application was completed.
        /// </summary>
        public DateTime? ApplicationCompleteDate { get; set; }

        /// <summary>
        /// Signed By Indicates if only the applicant, or only the parent, or both applicant and parent signed the transaction.
        /// </summary>
        public string SignedFlag { get; set; }

        /// <summary>
        /// Preparers Social Security Number Indicates that the Preparers SSN is provided on the transaction.
        /// </summary>
        public int? PreparerSsn { get; set; }

        /// <summary>
        /// Preparers Employer Identification Number (EIN)
        /// </summary>
        public int? PreparerEin { get; set; }

        /// <summary>
        /// Preparers Signature Indicates that a preparer signed the transaction.
        /// </summary>
        public string PreparerSigned { get; set; }

        /// <summary>
        /// Parent Marital Status
        /// </summary>
        public string PMaritalStatus { get; set; }

        /// <summary>
        /// Parent Marital Date
        /// </summary>
        public string PMaritalDate { get; set; }

        /// <summary>
        /// First Parent SSN
        /// </summary>
        public int? P1Ssn { get; set; }

        /// <summary>
        /// First Parent Last Name 
        /// </summary>
        public string P1LastName { get; set; }

        /// <summary>
        /// First Parent first initial 
        /// </summary>
        public string P1FirstInit { get; set; }

        /// <summary>
        /// First Parent Date of Birth
        /// </summary>
        public DateTime? P1Dob { get; set; }

        /// <summary>
        /// Second Parent SSN
        /// </summary>
        public int? P2Ssn { get; set; }

        /// <summary>
        /// Second Parent Last Name 
        /// </summary>
        public string P2LastName { get; set; }

        /// <summary>
        /// Second Parent first initial 
        /// </summary>
        public string P2FirstInit { get; set; }

        /// <summary>
        /// Second Parent Date of Birth
        /// </summary>
        public DateTime? P2Dob { get; set; }

        /// <summary>
        /// Parent's email address
        /// </summary>
        public string ParentEmail { get; set; }

        /// <summary>
        /// Parent's legal residency status 
        /// </summary>
        public string PLegalResSt { get; set; }

        /// <summary>
        /// Parent's legal residency before date 
        /// </summary>
        public bool? PLegalResB4 { get; set; }

        /// <summary>
        /// Parent's Legal residency date
        /// </summary>
        public string PLegalResDate { get; set; }

        /// <summary>
        /// Parent's number in family
        /// </summary>
        public int? PNbrFamily { get; set; }

        /// <summary>
        /// Parent's number in college
        /// </summary>
        public int? PNbrCollege { get; set; }

        /// <summary>
		/// Parent's income : SSI benefit
		/// </summary>
        public bool? PSsiBen { get; set; }

        /// <summary>
        /// Parent's Food stamps
        /// </summary>
        public bool? PFoodStamps { get; set; }

        /// <summary>
        /// Parent's lunch benefit
        /// </summary>
        public bool? PLunchBen { get; set; }

        /// <summary>
        /// Parent's tanf benefit
        /// </summary>
        public bool? PTanf { get; set; }

        /// <summary>
        /// Parent's WIC Benefit
        /// </summary>
        public bool? PWic { get; set; }

        /// <summary>
        /// Parent's tax return filed
        /// </summary>
        public string PTaxReturnFiled { get; set; }

        /// <summary>
        /// Parent's tax form type 
        /// </summary>
        public string PTaxFormType { get; set; }

        /// <summary>
        /// Parent's tax filing status 
        /// </summary>
        public string PTaxFilingStatus { get; set; }

        /// <summary>
        /// Parents schedule 1 filed
        /// </summary>
        public string PSched1 { get; set; }

        /// <summary>
        /// Parent dislocated worker
        /// </summary>
        public string PDisWorker { get; set; }

        /// <summary>
        /// Parent Adjust gross income
        /// </summary>
        public int? PAgi { get; set; }

        /// <summary>
        /// Parent US Tax paid
        /// </summary>
        public int? PUsTaxPaid { get; set; }

        /// <summary>
        /// Parent1 Income
        /// </summary>
        public int? P1Income { get; set; }

        /// <summary>
        /// Parent 2 Income
        /// </summary>
        public int? P2Income { get; set; }

        /// <summary>
        /// Parent's cash checking savings
        /// </summary>
        public int? PCash { get; set; }

        /// <summary>
        /// Parent's investment net worth
        /// </summary>
        public int? PInvNetWorth { get; set; }

        /// <summary>
        /// Parent's Business or farm net worth 
        /// </summary>
        public int? PBusNetWorth { get; set; }

        /// <summary>
        /// Parent's Educational credits
        /// </summary> 
        public int? PEduCredit { get; set; }

        /// <summary>
        /// Parent child support paid
        /// </summary>
        public int? PChildSupportPd { get; set; }

        /// <summary>
        /// Parent need based employement
        /// </summary>
        public int? PNeedBasedEmp { get; set; }

        /// <summary>
        /// Parent Grant or Scholarship Aid
        /// </summary>
        public int? PGrantScholAid { get; set; }

        /// <summary>
        /// Parent combat pay
        /// </summary>
        public int? PCombatPay { get; set; }

        /// <summary>
        /// Parent co-op earnings
        /// </summary>
        public int? PCoOpEarnings { get; set; }

        /// <summary>
        /// Parent pension payments
        /// </summary>
        public int? PPensionPymts { get; set; }

        /// <summary>
        /// Parent IRA payments
        /// </summary>
        public int? PIraPymts { get; set; }

        /// <summary>
        /// Parent Child Support received
        /// </summary>
        public int? PChildSupRcvd { get; set; }

        /// <summary>
        /// Parent Tax exempt interest income
        /// </summary>
        public int? PUntxIntInc { get; set; }

        /// <summary>
        /// Parent untaxed and IRA pensions
        /// </summary>
        public int? PUntxIraPen { get; set; }

        /// <summary>
        /// Parent Military or Clergy allowances
        /// </summary>
        public int? PMilClerAllow { get; set; }

        /// <summary>
        /// Parent veternary non ed benefit
        /// </summary>
        public int? PVetNonEdBen { get; set; }

        /// <summary>
        /// Parents other untaxed income
        /// </summary>
        public int? POtherUntxInc { get; set; }


        /// <summary>
        /// constructor to initialize properties
        /// </summary>
        /// <param name="id">Id of the record</param>
        /// <param name="studentId">Student Id</param>
        /// <param name="aidYear">Fin aid year</param>
        /// <param name="applicationType">application type</param>

        public AidApplications(string id, string appdemoID)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if(string.IsNullOrEmpty(appdemoID))
            {
                throw new ArgumentNullException("appdemoID");
            }

            /*if(id != null && !id.Equals(appdemoID))
            {
                throw new Exception("Value of id is not same as appdemoID");
            }*/

            /*if (string.IsNullOrEmpty(aidYear))
            {
                throw new ArgumentNullException("aidYear");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(applicationType))
            {
                throw new ArgumentNullException("applicationType");
            }*/

            _id = id;
            /*_aidYear = aidYear;
            _personId = studentId;
            _aidApplicantType = applicationType;*/
            _appDemoId = appdemoID;
        }
    }
}
