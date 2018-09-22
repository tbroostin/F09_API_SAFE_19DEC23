/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Configuration options for creating financial aid shopping sheets
    /// </summary>
    public class ShoppingSheetConfiguration
    {

        /// <summary>
        /// This identifies the rule table that calculates custom messages to print on a shopping sheet for a given student
        /// </summary>
        public string CustomMessageRuleTableId { get; set; }

        /// <summary>
        /// This type is used by the FA Shopping Sheet to configure the scorecard graphics on the right side of the shopping sheet. 
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
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
        [JsonConverter(typeof(StringEnumConverter))]
        public ShoppingSheetEfcOption EfcOption { get; set; }

        /// <summary>
        /// This number is used with the Graduation Rate slider. It defines the boundary between low and medium.
        /// </summary>
        public decimal? LowToMediumBoundary { get; set; }

        /// <summary>
        /// This number is used with the Graduation Rate slider. It defines the boundary between medium and high.
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

    }
}
