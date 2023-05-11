/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// ShoppingSheet contains student and award year specific cost and award data to print on a financial aid shopping sheet
    /// </summary>
    [Serializable]
    public class ShoppingSheet3
    {
        /// <summary>
        /// The AwardYear this data applies to
        /// </summary>
        public string AwardYear { get { return _AwardYear; } }
        private readonly string _AwardYear;

        /// <summary>
        /// The Colleague PERSON id of the student this data describes
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        private readonly string _StudentId;

        /// <summary>
        /// The student's total estimated costs in the award year
        /// </summary>
        public int? TotalEstimatedCost
        {
            get
            {
                return (Costs.Count() > 0) ? new Nullable<int>(Costs.Sum(c => c.Cost)) : null;
            }
        }

        /// <summary>
        /// A list of the student's costs in the award year
        /// </summary>
        //public Dictionary<ShoppingSheetBudgetGroup, int> Costs { get; set; }
        public List<ShoppingSheetCostItem2> Costs { get; set; }

        /// <summary>
        /// The student's total grants and scholarships awarded (free money) in the award year
        /// </summary>
        public int? TotalGrantsAndScholarships
        {
            get
            {
                if (GrantsAndScholarships != null)
                {
                    return (GrantsAndScholarships.Count() > 0) ? new Nullable<int>(GrantsAndScholarships.Sum(gs => gs.Amount)) : null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The student's total scholarships awarded. Only used for years 2020 onward
        /// </summary>
        public int? TotalScholarships
        {
            get
            {
                if ((Scholarships != null) || (EmployerPaidTuitionBenefits != null))
                {
                    var total = (Scholarships.Count() > 0) ? new Nullable<int>(Scholarships.Sum(s => s.Amount)) : 0;
                    if (EmployerPaidTuitionBenefits != null)
                    {
                        total += EmployerPaidTuitionBenefits;
                    }

                    if (total == 0)
                    {
                        total = null;
                    }
                    return total;
                    //return (Scholarships.Count() > 0) ? new Nullable<int>(Scholarships.Sum(s => s.Amount)) : null;
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// The student's total grants awarded. Only used for years 2020 onward
        /// </summary>
        public int? TotalGrants
        {
            get
            {
                if (Grants != null)
                {
                    return (Grants.Count() > 0) ? new Nullable<int>(Grants.Sum(g => g.Amount)) : null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The student's total grants awarded. Only used for years 2020 onward
        /// </summary>
        public int? CfpGradTotalGrants
        {
            get
            {
                if ((Grants != null) || (DisadvantagedStudentGrant != null))
                {
                    var cfpGradGrants = Grants.Where(g => g.AwardGroup != ShoppingSheetAwardGroup2.PellGrants);
                    var total = (cfpGradGrants.Count() > 0) ? new Nullable<int>(cfpGradGrants.Sum(g => g.Amount)) : 0;
                    if (DisadvantagedStudentGrant != null)
                    {
                        total += DisadvantagedStudentGrant;
                    }
                    if (total == 0)
                    {
                        total = null;
                    }
                    return total;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Total of CFP Pell Grants
        /// </summary>
        public int? CfpPellGrants
        {
            get
            {
                if (Grants != null)
                {
                    var cfpPellGrants = Grants.Where(g => g.AwardGroup == ShoppingSheetAwardGroup2.PellGrants);
                    return (cfpPellGrants.Count() > 0) ? new Nullable<int>(cfpPellGrants.Sum(g => g.Amount)) : null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Total of scholarships for graduate CFP
        /// </summary>
        public int? CfpGradTotalScholarships
        {
            get
            {
                if ((Scholarships != null) || (EmployerPaidTuitionBenefits != null) || (SchoolPaidTuitionBenefits != null) || (TuitionRemWaiver != null))
                {
                    var total = (Scholarships.Count() > 0) ? new int?(Scholarships.Sum(s => s.Amount)) : 0;
                    if (EmployerPaidTuitionBenefits != null)
                    {
                        total += EmployerPaidTuitionBenefits;
                    }
                    if (SchoolPaidTuitionBenefits != null)
                    {
                        total += SchoolPaidTuitionBenefits;
                    }
                    if (TuitionRemWaiver != null)
                    {
                        total += TuitionRemWaiver;
                    }

                    if (total == 0)
                    {
                        total = null;
                    }

                    return total;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// A list of the student's grants and scholarships in the award year
        /// </summary>
        //public Dictionary<ShoppingSheetAwardGroup, int> GrantsAndScholarships { get; set; }
        public List<ShoppingSheetAwardItem2> GrantsAndScholarships { get; set; }

        /// <summary>
        /// A list of the student's scholarships in the award year
        /// </summary>
        public List<ShoppingSheetAwardItem2> Scholarships { get; set; }

        /// <summary>
        /// A list of the student's grants in the award year
        /// </summary>
        public List<ShoppingSheetAwardItem2> Grants { get; set; }

        /// <summary>
        /// The Net Costs for the student in the award year (Costs - TotalGrantsAndScholarships)
        /// </summary>
        public int NetCosts
        {
            get
            {
                var cost = (TotalEstimatedCost.HasValue) ? TotalEstimatedCost.Value : 0;
                var grants = (TotalGrantsAndScholarships.HasValue) ? TotalGrantsAndScholarships.Value : 0;
                return cost - grants;
            }
        }

        /// <summary>
        /// The Net Costs for the student in the award year (Costs - TotalGrants - TotalScholarships)
        /// Only used for year 2020 and onward
        /// </summary>
        public int? CollegeFinancingPlanNetCosts
        {
            get
            {
                var cost = (TotalEstimatedCost.HasValue) ? TotalEstimatedCost.Value : 0;
                var grants = (TotalGrants.HasValue) ? TotalGrants.Value : 0;
                var scholarships = (TotalScholarships.HasValue) ? TotalScholarships.Value : 0;
                return cost - grants - scholarships;
            }
        }

        /// <summary>
        /// Net cost for UG CFP
        /// </summary>
        public int? CfpUndergradNetCosts
        {
            get
            {
                var cost = (TotalEstimatedCost.HasValue) ? TotalEstimatedCost.Value : 0;
                var grants = (TotalGrants.HasValue) ? TotalGrants.Value : 0;
                var scholarships = (TotalScholarships.HasValue) ? TotalScholarships.Value : 0;
                //Per regulation clarification in 04/2022 Vets awards should reduce net costs
                var vets = VetBenAmt.HasValue ? VetBenAmt.Value : 0;
                //Removing as discussion with MXT indicated these should be added directly to TotalScholarships
                //scholarships += (EmployerPaidTuitionBenefits.HasValue) ? EmployerPaidTuitionBenefits.Value : 0;
                return cost - grants - scholarships - vets;
            }
        }

        /// <summary>
        /// Net costs for Grad CFP
        /// </summary>
        public int? CfpGradNetCosts
        {
            get
            {
                var cost = (TotalEstimatedCost.HasValue) ? TotalEstimatedCost.Value : 0;
                //Changed to CfpGradTotalGrants so as to exclude Pell Grant amounts in GR
                //Removed DisadvantagedStudentGrant as discussion with MXT was to include it in CfpGradTotalGrants
                var grants = (CfpGradTotalGrants.HasValue) ? CfpGradTotalGrants.Value : 0;
                //grants += (DisadvantagedStudentGrant.HasValue) ? DisadvantagedStudentGrant.Value : 0;
                //Changed to CfpGradTotalScholarships and removed the three lines beneath as they are included
                //within CfpGradTotalScholarships after speaking with MXT to confirm
                var scholarships = (CfpGradTotalScholarships.HasValue) ? CfpGradTotalScholarships.Value : 0;
                //scholarships += (EmployerPaidTuitionBenefits.HasValue) ? EmployerPaidTuitionBenefits.Value : 0;
                //scholarships += (SchoolPaidTuitionBenefits.HasValue) ? SchoolPaidTuitionBenefits.Value : 0;
                //scholarships += (TuitionRemWaiver.HasValue) ? TuitionRemWaiver.Value : 0;
                //Per regulation clarification in 04/2022 Vets awards should reduce net costs
                var vets = VetBenAmt.HasValue ? VetBenAmt.Value : 0;
                return cost - grants - scholarships - vets;
            }
        }

        /// <summary>
        /// A list of the student's work options in the award year
        /// </summary>
        //public Dictionary<ShoppingSheetAwardGroup, int> WorkOptions { get; set; }
        public List<ShoppingSheetAwardItem2> WorkOptions { get; set; }
        /// <summary>
        /// A list of the student's loan options in the award year
        /// </summary>
        //public Dictionary<ShoppingSheetAwardGroup, int> LoanOptions { get; set; }
        public List<ShoppingSheetAwardItem2> LoanOptions { get; set; }

        /// <summary>
        /// The Estimated Family Contribution as calculated by the Institution or Department of Education 
        /// </summary>
        public int? FamilyContribution { get; set; }

        /// <summary>
        /// A list of custom messages, specific to the student, to print on the shopping sheet.
        /// </summary>
        public List<string> CustomMessages { get; set; }

        /// <summary>
        /// A list of custom messages, specific to the student, to print on the shopping sheet.
        /// </summary>
        public List<string> LoanAmountMessages { get; set; }

        /// <summary>
        /// A list of custom messages, specific to the student, to print on the shopping sheet.
        /// </summary>
        public List<string> EducationBenefitsMessages { get; set; }

        /// <summary>
        /// A list of custom messages, specific to the student, to print on the shopping sheet.
        /// </summary>
        public List<string> NextStepsMessages { get; set; }

        /// <summary>
        /// The version type associated with a college financing plan (UG or GR)
        /// </summary>
        public string CfpVersionType { get; set; }

        /// <summary>
        /// Interest rate for a sub loan
        /// </summary>
        public Decimal? SubInterestRate { get; set; }

        /// <summary>
        /// Origination fee for a sub loan
        /// </summary>
        public Decimal? SubOriginationFee { get; set; }

        /// <summary>
        /// Interest rate for an unsub loan
        /// </summary>
        public Decimal? UnsubInterestRate { get; set; }

        /// <summary>
        /// Origination fee for an unsub loan
        /// </summary>
        public Decimal? UnsubOriginationFee { get; set; }

        /// <summary>
        /// Interest rate for a graduate unsub loan
        /// </summary>
        public Decimal? GradUnsubInterestRate { get; set; }

        /// <summary>
        /// Origination fee for a graduate unsub loan
        /// </summary>
        public Decimal? GradUnsubOriginationFee { get; set; }

        /// <summary>
        /// Interest rate associated with a private loan
        /// </summary>
        public Decimal? PrivateInterestRate { get; set; }

        /// <summary>
        /// Origination fee for a private loan
        /// </summary>
        public Decimal? PrivateOriginationFee { get; set; }

        /// <summary>
        /// Interest rate for an institutional loan
        /// </summary>
        public Decimal? InstitutionInterestRate { get; set; }

        /// <summary>
        /// Origination fee for an institutional loan
        /// </summary>
        public Decimal? InstitutionOriginationFee { get; set; }

        /// <summary>
        /// Interest rate for a grad plus loan
        /// </summary>
        public Decimal? GradPlusInterestRate { get; set; }

        /// <summary>
        /// Origination fee for a grad plus loan
        /// </summary>
        public Decimal? GradPlusOriginationFee { get; set; }

        /// <summary>
        /// Interest rate for HRSA loan
        /// </summary>
        public Decimal? HrsaInterestRate { get; set; }

        /// <summary>
        /// Origination fee for HRSA loan
        /// </summary>
        public Decimal? HrsaOriginationFee { get; set; }

        /// <summary>
        /// Interest rate for a plus loan
        /// </summary>
        public Decimal? PlusInterestRate { get; set; }

        /// <summary>
        /// Origination fee for a plus loan
        /// </summary>
        public Decimal? PlusOriginationFee { get; set; }

        /// <summary>
        /// Tuition benefits paid by an institution
        /// </summary>
        public int? SchoolPaidTuitionBenefits { get; set; }

        /// <summary>
        /// Tuition benefits paid by employer
        /// </summary>
        public int? EmployerPaidTuitionBenefits { get; set; }

        /// <summary>
        /// Tuition remission or waiver
        /// </summary>
        public int? TuitionRemWaiver { get; set; }

        /// <summary>
        /// Scholarships for Disadvantaged Students	
        /// </summary>
        public int? DisadvantagedStudentGrant { get; set; }

        /// <summary>
        /// Assistantships
        /// </summary>
        public int? Assistantships { get; set; }

        /// <summary>
        /// Total amount of the income share 
        /// </summary>
        public int? IncomeShare { get; set; }

        /// <summary>
        /// Total amount of the graduate plus loans
        /// </summary>
        public int? GraduatePlusLoans { get; set; }

        /// <summary>
        /// Total amount of HRSA Loans
        /// </summary>
        public int? HrsaLoans { get; set; }

        /// <summary>
        /// Total amount of Profile EFC (Based on Institutional Methodology)
        /// </summary>
        public int? ProfileEfc { get; set; }

        /// <summary>
        /// Total amount of Fafsa EFC
        /// </summary>
        public int? FafsaEfc { get; set; }

        ///<summary>
        ///Total Vets benefit amount
        /// </summary>
        public int? VetBenAmt { get; set; }




        /// <summary>
        /// Create a ShoppingSheet object
        /// </summary>
        /// <param name="awardYear">Required: Award Year this shopping sheet applies to</param>
        /// <param name="studentId">Required: Colleague PERSON id this shopping sheet applies to </param>
        public ShoppingSheet3(string awardYear, string studentId)
        {
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            _AwardYear = awardYear;
            _StudentId = studentId;
            if (Convert.ToInt32(awardYear) >= 2020) {
                // 6 costs
                Costs = new List<ShoppingSheetCostItem2>(6);
                // 4 grants
                Grants = new List<ShoppingSheetAwardItem2>(4);
                // 4 scholarships
                Scholarships = new List<ShoppingSheetAwardItem2>(4);
                // 2 work options
                WorkOptions = new List<ShoppingSheetAwardItem2>(2);
                // 6 loan options
                LoanOptions = new List<ShoppingSheetAwardItem2>(6);
                CustomMessages = new List<string>();
                LoanAmountMessages = new List<string>();
                EducationBenefitsMessages = new List<string>();
                NextStepsMessages = new List<string>();
            }
            else {
                Costs = new List<ShoppingSheetCostItem2>(5);
                GrantsAndScholarships = new List<ShoppingSheetAwardItem2>(4);
                WorkOptions = new List<ShoppingSheetAwardItem2>(1);
                LoanOptions = new List<ShoppingSheetAwardItem2>(3);
                CustomMessages = new List<string>();
            }
        }

        /// <summary>
        /// Two ShoppingSheets are equal when their award years and student ids are equal
        /// </summary>
        /// <param name="obj">The ShoppingSheet object to compare to this shopping sheet</param>
        /// <returns>True, if the shopping sheets are equal. False, otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var shoppingSheet = obj as ShoppingSheet;

            if (shoppingSheet.StudentId == this.StudentId &&
                shoppingSheet.AwardYear == this.AwardYear)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the HashCode of this ShoppingSheet based on the Award Year and studentId
        /// </summary>
        /// <returns>The HashCode of this object </returns>
        public override int GetHashCode()
        {
            return AwardYear.GetHashCode() ^ StudentId.GetHashCode();
        }

        /// <summary>
        /// Gets the ShoppingSheet string represented by the AwardYear and StudentId
        /// </summary>
        /// <returns>The string representation of this object</returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}", AwardYear, StudentId);
        }
    }
}
