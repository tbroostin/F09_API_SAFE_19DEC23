/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// ShoppingSheet DTO contains student and award year specific cost and award data to print on a financial aid shopping sheet
    /// </summary>
    public class ShoppingSheet3
    {
        /// <summary>
        /// The AwardYear this data applies to
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// The Colleague PERSON id of the student this data describes
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The student's total estimated costs in the award year
        /// Will be null if there are no Costs.
        /// </summary>
        public int? TotalEstimatedCost { get; set; }

        /// <summary>
        /// A list of the student's costs in the award year
        /// </summary>
        public List<ShoppingSheetCostItem2> Costs { get; set; }
    
        /// <summary>
        /// The student's total grants and scholarships awarded (free money) in the award year
        /// Will be null if there are no GrantsAndScholarships
        /// </summary>
        public int? TotalGrantsAndScholarships { get; set; }

        /// <summary>
        /// The student's total grants awarded. Only used for years 2020 onward
        /// </summary>
        public int? TotalGrants { get; set; }

        /// <summary>
        /// The student's total scholarships awarded. Only used for years 2020 onward
        /// </summary>
        public int? TotalScholarships { get; set; }

        /// <summary>
        /// A list of the student's grants and scholarships in the award year
        /// </summary>
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
        public int NetCosts { get; set; }

        /// <summary>
        /// The Net Costs for the student in the award year (Costs - TotalGrants - TotalScholarships)
        /// Only used for year 2020 and onward
        /// </summary>
        public int? CollegeFinancingPlanNetCosts { get; set; }

        /// <summary>
        /// A list of the student's work options in the award year
        /// </summary>
        public List<ShoppingSheetAwardItem2> WorkOptions { get; set; }

        /// <summary>
        /// A list of the student's loan options in the award year
        /// </summary>
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
        /// Total scholarships for grad CFP
        /// </summary>
        public int? CfpGradTotalScholarships { get; set; }

        /// <summary>
        /// Scholarships for Disadvantaged Students
        /// </summary>
        public int? DisadvantagedStudentGrant { get; set; }

        /// <summary>
        /// The student's total grants awarded. Only used for years 2020 onward
        /// </summary>
        public int? CfpGradTotalGrants { get; set; }

        /// <summary>
        /// Assistantships
        /// </summary>
        public int? Assistantships { get; set; }

        /// <summary>
        /// Total amount of the income share
        /// </summary>
        public int? IncomeShare { get; set; }

        /// <summary>
        /// Total amount for CFP pell grants
        /// </summary>
        public int? CfpPellGrants { get; set; }

        /// <summary>
        /// Net costs for UG CFP
        /// </summary>
        public int? CfpUndergradNetCosts { get; set; }

        /// <summary>
        /// Net costs for Grad CFP
        /// </summary>
        public int? CfpGradNetCosts { get; set; }

        /// <summary>
        /// Total amount for GPLUS loans
        /// </summary>
        public int? GraduatePlusLoans { get; set; }

        /// <summary>
        /// Total amount for HRSA Loans
        /// </summary>
        public int? HrsaLoans { get; set; }

        /// <summary>
        /// Total amount for Profile EFC
        /// </summary>
        public int? ProfileEfc { get; set; }

        /// <summary>
        /// Total amount for Fafsa EFC
        /// </summary>
        public int? FafsaEfc { get; set; }
    }
}
