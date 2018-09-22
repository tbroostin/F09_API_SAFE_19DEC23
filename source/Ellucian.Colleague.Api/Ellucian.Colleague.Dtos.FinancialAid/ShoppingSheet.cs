/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// ShoppingSheet DTO contains student and award year specific cost and award data to print on a financial aid shopping sheet
    /// </summary>
    public class ShoppingSheet
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
        public List<ShoppingSheetCostItem> Costs { get; set; }

        /// <summary>
        /// The student's total grants and scholarships awarded (free money) in the award year
        /// Will be null if there are no GrantsAndScholarships
        /// </summary>
        public int? TotalGrantsAndScholarships { get; set; }

        /// <summary>
        /// A list of the student's grants and scholarships in the award year
        /// </summary>
        public List<ShoppingSheetAwardItem> GrantsAndScholarships { get; set; }

        /// <summary>
        /// The Net Costs for the student in the award year (Costs - TotalGrantsAndScholarships)
        /// </summary>
        public int NetCosts { get; set; }

        /// <summary>
        /// A list of the student's work options in the award year
        /// </summary>
        public List<ShoppingSheetAwardItem> WorkOptions { get; set; }

        /// <summary>
        /// A list of the student's loan options in the award year
        /// </summary>
        public List<ShoppingSheetAwardItem> LoanOptions { get; set; }

        /// <summary>
        /// The Estimated Family Contribution as calculated by the Institution or Department of Education 
        /// </summary>
        public int? FamilyContribution { get; set; }

        /// <summary>
        /// A list of custom messages, specific to the student, to print on the shopping sheet.
        /// </summary>
        public List<string> CustomMessages { get; set; }
    }
}
