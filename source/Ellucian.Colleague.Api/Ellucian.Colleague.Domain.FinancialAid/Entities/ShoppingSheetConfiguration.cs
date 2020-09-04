/*Copyright 2015-2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Configuration options for creating financial aid shopping sheets
    /// </summary>
    [Serializable]
    public class ShoppingSheetConfiguration
    {
        /// <summary>
        /// This identifies the rule table that calculates custom messages to print on a shopping sheet for a given student
        /// </summary>
        public string CustomMessageRuleTableId { get; set; }

        /// <summary>
        /// This type is used by the FA Shopping Sheet to configure the scorecard graphics on the right side of the shopping sheet. 
        /// </summary>
        public ShoppingSheetOfficeType OfficeType { get; set; }

        /// <summary>
        /// This number is used on the shopping sheet to produce a slider bar to graphically show students and parents the school's graduation rate.
        /// This attribute could be null if the Department of Education does not provide the institution with a graduation rate.
        /// </summary>
        public decimal? GraduationRate { get; set; }

        /// <summary>
        /// This number is used on the shopping sheet to produce a bar graph that shows how the school's loan default rate 
        /// stacks up against the loan default rate of comparable institutions in the United States. This attribute could be null
        /// if the Department of Education does not provide the institution with a loan default rate.
        /// </summary>
        public decimal? LoanDefaultRate { get; set; }

        /// <summary>
        /// This number is used on the shopping sheet to produce a bar graph that shows how the school's loan default rate 
        /// stacks up against the loan default rate of comparable institutions in the United States. This attribute could be null
        /// if the Department of Education does not provide the institution with a national loan default rate.
        /// </summary>
        public decimal? NationalLoanDefaultRate { get; set; }

        /// <summary>
        /// This number is used on the shopping sheet to explain to students and parents the median borrowing amount of students at the school.
        /// This attribute could be null if the Department of Education does not provide the institution with a median borrowing amount.
        /// </summary>
        public int? MedianBorrowingAmount { get; set; }

        /// <summary>
        /// This number is used on the shopping sheet to explain the median amount that students typically pay each month to pay off their loans over ten years.
        /// This attribute could be null if the Department of Education does not provide the institution with a median monthly payment amount.
        /// </summary>
        public decimal? MedianMonthlyPaymentAmount { get; set; }

        /// <summary>
        /// The EfcOption allows schools who use the College Board PROFILE application to display the Estimated Family Contribution on the shopping sheet
        /// as calculated by the College Board instead of the Department of Education.
        /// </summary>
        public ShoppingSheetEfcOption EfcOption { get; set; }

        /// <summary>
        /// This number is used with the Graduation Rate slider. It defines the boundary between low and medium.
        /// </summary>
        public decimal? LowToMediumBoundary { get; set; }

        /// <summary>
        /// This number is used with the Graduation Rate slider. It defines the boundary between medium and high
        /// </summary>
        public decimal? MediumToHighBoundary { get; set; }

        /// <summary>
        /// This number is used on the shopping sheet to produce a bar graph that shows how the school's loan repayment rate 
        /// stacks up against the loan repayment rate of comparable institutions in the United States. This attribute could be null
        /// if the Department of Education does not provide the institution with an institutional loan repayment rate.
        /// </summary>
        public decimal? InstitutionRepaymentRate { get; set; }

        /// <summary>
        /// This number is used on the shopping sheet to produce a bar graph that shows how the school's loan repayment rate 
        /// stacks up against the loan repayment rate of comparable institutions in the United States. This attribute could be null
        /// if the Department of Education does not provide the institution with a national loan repayment rate.
        /// </summary>
        public decimal? NationalRepaymentRateAverage { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with tuition and fees
        /// </summary>
        public List<string> TuitionAndFees { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with books and supplies
        /// </summary>
        public List<string> BooksAndSupplies { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with transportation
        /// </summary>
        public List<string> Transportation { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with other education costs
        /// </summary>
        public List<string> OtherEducationCosts { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with school scholarships
        /// </summary>
        public List<string> SchoolScholarships { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with state scholarships
        /// </summary>
        public List<string> StateScholarships { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with other scholarships
        /// </summary>
        public List<string> OtherScholarships { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with pell grants
        /// </summary>
        public List<string> PellGrants { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with school grants
        /// </summary>
        public List<string> SchoolGrants { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with state grants
        /// </summary>
        public List<string> StateGrants { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with other grants
        /// </summary>
        public List<string> OtherGrants { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with direct subsidized loans
        /// </summary>
        public List<string> DlSubLoans { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with direct unsubsidized loans
        /// </summary>
        public List<string> DlUnsubLoans { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with private loans
        /// </summary>
        public List<string> PrivateLoans { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with school loans
        /// </summary>
        public List<string> SchoolLoans { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with other loans
        /// </summary>
        public List<string> OtherLoans { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with parent PLUS loans
        /// </summary>
        public List<string> ParentPlusLoans { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with work studies
        /// </summary>
        public List<string> WorkStudy { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with other jobs
        /// </summary>
        public List<string> OtherJobs { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with housing and meals for on campus
        /// </summary>
        public List<string> HousingAndMealsOn { get; set; }

        /// <summary>
        /// This is a list of strings that identify all codes associated with housing and meals for off campus
        /// </summary>
        public List<string> HousingAndMealsOff { get; set; }

        /// <summary>
        /// This is a string that contains the ID for the Loan Amount Text Rule
        /// </summary>
        public string LoanAmountTextRuleId { get; set; }

        /// <summary>
        /// This is a string that contains the ID for the Education Benefits Text Rule
        /// </summary>
        public string EducationBenTextRuleId { get; set; }

        /// <summary>
        /// This is a string that contains the ID for the Next Steps Text Rule
        /// </summary>
        public string NextStepsRuleId { get; set; }


        public ShoppingSheetConfiguration()
        {
            OfficeType = ShoppingSheetOfficeType.BachelorDegreeGranting;
            EfcOption = ShoppingSheetEfcOption.IsirEfc;
        }

    }
}
