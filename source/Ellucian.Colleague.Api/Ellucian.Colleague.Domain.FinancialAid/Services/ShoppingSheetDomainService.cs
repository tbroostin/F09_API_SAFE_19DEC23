/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Services
{
    /// <summary>
    /// The Financial Aid ShoppingSheet Domain Service provides methods to build shopping sheet objects 
    /// using various other financial aid objects
    /// </summary>
    public static class ShoppingSheetDomainService
    {
        /// <summary>
        /// Build a shopping sheet for the given StudentAwardYear.
        /// Ensure that the lists of studentAwards, FAFSAs, profile applications and studentBudgets contain
        /// data for the student in StudentAwardYear.
        /// </summary>
        /// <param name="studentAwardYear">The StudentAwardYear for which to build a shopping sheet</param>
        /// <param name="studentAwards">A list of StudentAwards. </param>
        /// <param name="awards">A list of Awards</param>
        /// <param name="budgetComponents">A list of budgetComponents</param>
        /// <param name="studentBudgetComponents">A list of StudentBudgetComponents</param>
        /// <param name="applications">A list of FinancialAidApplications</param>
        /// <returns>A ShoppingSheet for the given StudentAwardYear</returns>
        /// <exception cref="ApplicationException">Thrown if there's no shopping sheet configuration for the studentAwardYear</exception>
        /// <exception cref="ApplicationException">Thrown if the student has assigned budgets but there are no budgetComponents for the award year</exception>
        /// <exception cref="ApplicationException">Thrown if the student has neither budgets nor awards for the year</exception>
        public static async Task<ShoppingSheet> BuildShoppingSheetAsync(
            StudentAwardYear studentAwardYear,
            IEnumerable<StudentAward> studentAwards,
            IEnumerable<BudgetComponent> budgetComponents,
            IEnumerable<StudentBudgetComponent> studentBudgetComponents,
            IEnumerable<FinancialAidApplication2> applications,
            IEnumerable<ShoppingSheetRuleTable> ruleTables)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            if (studentAwardYear.CurrentConfiguration == null || studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration == null)
            {
                throw new ApplicationException(string.Format("Shopping Sheet is not configured for student {0}, awardYear {1}, office {2} ", studentAwardYear.StudentId, studentAwardYear.Code, studentAwardYear.CurrentOffice.Id));
            }

            var budgetComponentsForYear = (budgetComponents != null) ? budgetComponents.Where(bc => bc.AwardYear == studentAwardYear.Code).ToList() : new List<BudgetComponent>();
            var studentBudgetComponentsForYear = (studentBudgetComponents != null) ? studentBudgetComponents.Where(c => c.StudentId == studentAwardYear.StudentId && c.AwardYear == studentAwardYear.Code).ToList() : new List<StudentBudgetComponent>();
            var studentAwardsForYear = (studentAwards != null) ? studentAwards.Where(a => a.StudentAwardYear.Equals(studentAwardYear) && a.Award != null) : new List<StudentAward>();
            var applicationsForYear = (applications != null) ? applications.Where(app => app.StudentId == studentAwardYear.StudentId && app.AwardYear == studentAwardYear.Code).ToList() : new List<FinancialAidApplication2>();
            var ruleTableForYear = (ruleTables != null) ? ruleTables.FirstOrDefault(rt => rt.AwardYear == studentAwardYear.Code && rt.Code == studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.CustomMessageRuleTableId) : null;

            if (budgetComponentsForYear.Count() == 0 && studentBudgetComponentsForYear.Count() > 0)
            {
                throw new ApplicationException(string.Format("Cannot create shopping sheet for award year {0} that has no defined budget components", studentAwardYear.Code));
            }
            if (studentBudgetComponentsForYear.Count() == 0 && studentAwardsForYear.Count() == 0)
            {
                throw new ApplicationException(string.Format("Cannot create shopping sheet for student {0}, awardYear {1} with no awards and no budgets", studentAwardYear.StudentId, studentAwardYear.Code));
            }

            var shoppingSheet = new ShoppingSheet(studentAwardYear.Code, studentAwardYear.StudentId);

            //set budgets
            //create list of cost amounts from student budget components that have a budget shopping sheet group of tuition and fees          
            //if the number of items in that list > 0, then the student has tuition and fees budgets. otherwise, the student has no tuition and fees budget components
            var tuitionFees = budgetComponentsForYear
                .Where(budget => budget.ShoppingSheetGroup == ShoppingSheetBudgetGroup.TuitionAndFees)
                .Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    //if an override exists, use it. otherwise, use the original amount
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
            if (tuitionFees.Count() > 0)
            {
                shoppingSheet.Costs.Add(new ShoppingSheetCostItem(ShoppingSheetBudgetGroup.TuitionAndFees, tuitionFees.Sum()));
            }

            //see comment from tuition and fees
            var housingMeals = budgetComponentsForYear
                .Where(budget => budget.ShoppingSheetGroup == ShoppingSheetBudgetGroup.HousingAndMeals)
                .Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
            if (housingMeals.Count() > 0)
            {
                shoppingSheet.Costs.Add(new ShoppingSheetCostItem(ShoppingSheetBudgetGroup.HousingAndMeals, housingMeals.Sum()));
            }

            //see comment from tuition and fees
            var booksSupplies = budgetComponentsForYear
                .Where(budget => budget.ShoppingSheetGroup == ShoppingSheetBudgetGroup.BooksAndSupplies)
                .Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
            if (booksSupplies.Count() > 0)
            {
                shoppingSheet.Costs.Add(new ShoppingSheetCostItem(ShoppingSheetBudgetGroup.BooksAndSupplies, booksSupplies.Sum()));
            }

            //see comment from tuition and fees
            var transportation = budgetComponentsForYear
                .Where(budget => budget.ShoppingSheetGroup == ShoppingSheetBudgetGroup.Transportation)
                .Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
            if (transportation.Count() > 0)
            {
                shoppingSheet.Costs.Add(new ShoppingSheetCostItem(ShoppingSheetBudgetGroup.Transportation, transportation.Sum()));
            }

            //see comment from tuition and fees
            var otherCosts = budgetComponentsForYear
                .Where(budget => budget.ShoppingSheetGroup == ShoppingSheetBudgetGroup.OtherCosts)
                .Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
            if (otherCosts.Count() > 0)
            {
                shoppingSheet.Costs.Add(new ShoppingSheetCostItem(ShoppingSheetBudgetGroup.OtherCosts, otherCosts.Sum()));
            }

            //Filter student awards by award status category
            studentAwardsForYear = ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(studentAwardsForYear).ToList();

            //if there are no grants/scholarships/workstudy for a particular award group, don't add that group
            //to the shopping sheet.
            var schoolGrants = studentAwardsForYear
                .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.SchoolGrants && studentAward.AwardAmount.HasValue);
            if (schoolGrants.Count() > 0)
            {
                shoppingSheet.GrantsAndScholarships.Add(
                    new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.SchoolGrants, schoolGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
            }

            var pellGrants = studentAwardsForYear
                .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.PellGrants && studentAward.AwardAmount.HasValue);
            if (pellGrants.Count() > 0)
            {
                shoppingSheet.GrantsAndScholarships.Add(
                     new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.PellGrants, pellGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
            }

            var stateGrants = studentAwardsForYear
                .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.StateGrants && studentAward.AwardAmount.HasValue);
            if (stateGrants.Count() > 0)
            {
                shoppingSheet.GrantsAndScholarships.Add(
                     new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.StateGrants, stateGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
            }

            var otherGrants = studentAwardsForYear
                .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.OtherGrants && studentAward.AwardAmount.HasValue);
            if (otherGrants.Count() > 0)
            {
                shoppingSheet.GrantsAndScholarships.Add(
                     new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.OtherGrants, otherGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
            }

            var workStudy = studentAwardsForYear
                 .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.WorkStudy && studentAward.AwardAmount.HasValue);
            if (workStudy.Count() > 0)
            {
                shoppingSheet.WorkOptions.Add(
                     new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.WorkStudy, workStudy.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
            }

            //if there are no loans for a particular award group, add the group anyway and set the amount to zero.
            var perkinsLoans = studentAwardsForYear
                .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.PerkinsLoans && studentAward.AwardAmount.HasValue);
            shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.PerkinsLoans, perkinsLoans.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));

            var subsidizedLoans = studentAwardsForYear
                .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.SubsidizedLoans && studentAward.AwardAmount.HasValue);
            shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.SubsidizedLoans, subsidizedLoans.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));

            var unsubsidizedLoans = studentAwardsForYear
                .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.UnsubsidizedLoans && studentAward.AwardAmount.HasValue);
            shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.UnsubsidizedLoans, unsubsidizedLoans.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));

            //set Efc based on shoppingsheet configuration option
            switch (studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption)
            {
                case ShoppingSheetEfcOption.IsirEfc:
                    var federalApplication = applicationsForYear.FirstOrDefault(app => app.IsFederallyFlagged);
                    shoppingSheet.FamilyContribution = (federalApplication != null) ? federalApplication.FamilyContribution : null;
                    break;

                case ShoppingSheetEfcOption.ProfileEfc:
                    var institutionalProfileApplication = applicationsForYear.OfType<ProfileApplication>().FirstOrDefault(app => app.IsInstitutionallyFlagged);
                    shoppingSheet.FamilyContribution = (institutionalProfileApplication != null) ? institutionalProfileApplication.InstitutionalFamilyContribution : null;
                    break;

                case ShoppingSheetEfcOption.ProfileEfcUntilIsirExists:
                    var federalFafsa = applicationsForYear.OfType<Fafsa>().FirstOrDefault(app => app.IsFederallyFlagged);
                    if (federalFafsa != null)
                    {
                        shoppingSheet.FamilyContribution = federalFafsa.FamilyContribution;
                    }
                    else
                    {
                        var institutionalProfile = applicationsForYear.OfType<ProfileApplication>().FirstOrDefault(app => app.IsInstitutionallyFlagged);
                        shoppingSheet.FamilyContribution = (institutionalProfile != null) ? institutionalProfile.InstitutionalFamilyContribution : null;
                    }
                    break;
            }

            //set custom messages
            if (ruleTableForYear != null)
            {
                shoppingSheet.CustomMessages = (await ruleTableForYear.GetRuleTableResultAsync(studentAwardYear)).ToList();
            }

            return shoppingSheet;
        }
    }
}
