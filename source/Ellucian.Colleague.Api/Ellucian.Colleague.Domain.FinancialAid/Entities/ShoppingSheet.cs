/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
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
    public class ShoppingSheet
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
        public List<ShoppingSheetCostItem> Costs { get; set; }

        /// <summary>
        /// The student's total grants and scholarships awarded (free money) in the award year
        /// </summary>
        public int? TotalGrantsAndScholarships
        {
            get
            {
                return (GrantsAndScholarships.Count() > 0) ? new Nullable<int>(GrantsAndScholarships.Sum(gs => gs.Amount)) : null;
            }
        }

        /// <summary>
        /// A list of the student's grants and scholarships in the award year
        /// </summary>
        //public Dictionary<ShoppingSheetAwardGroup, int> GrantsAndScholarships { get; set; }
        public List<ShoppingSheetAwardItem> GrantsAndScholarships { get; set; }

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
        /// A list of the student's work options in the award year
        /// </summary>
        //public Dictionary<ShoppingSheetAwardGroup, int> WorkOptions { get; set; }
        public List<ShoppingSheetAwardItem> WorkOptions { get; set; }
        /// <summary>
        /// A list of the student's loan options in the award year
        /// </summary>
        //public Dictionary<ShoppingSheetAwardGroup, int> LoanOptions { get; set; }
        public List<ShoppingSheetAwardItem> LoanOptions { get; set; }

        /// <summary>
        /// The Estimated Family Contribution as calculated by the Institution or Department of Education 
        /// </summary>
        public int? FamilyContribution { get; set; }

        /// <summary>
        /// A list of custom messages, specific to the student, to print on the shopping sheet.
        /// </summary>
        public List<string> CustomMessages { get; set; }


        /// <summary>
        /// Create a ShoppingSheet object
        /// </summary>
        /// <param name="awardYear">Required: Award Year this shopping sheet applies to</param>
        /// <param name="studentId">Required: Colleague PERSON id this shopping sheet applies to </param>
        public ShoppingSheet(string awardYear, string studentId)
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

            Costs = new List<ShoppingSheetCostItem>(5);
            GrantsAndScholarships = new List<ShoppingSheetAwardItem>(4);
            WorkOptions = new List<ShoppingSheetAwardItem>(1);
            LoanOptions = new List<ShoppingSheetAwardItem>(3);
            CustomMessages = new List<string>();
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
