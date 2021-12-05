/*Copyright 2015-2020 Ellucian Company L.P. and its affiliates.*/
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
        public static async Task<ShoppingSheet2> BuildShoppingSheet2Async(
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
            var loanAmountRuleTableForYear = (ruleTables != null) ? ruleTables.FirstOrDefault(rt => rt.AwardYear == studentAwardYear.Code && rt.Code == studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.LoanAmountTextRuleId) : null;
            var edBenRuleTableForYear = (ruleTables != null) ? ruleTables.FirstOrDefault(rt => rt.AwardYear == studentAwardYear.Code && rt.Code == studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EducationBenTextRuleId) : null;
            var nextStepsRuleTableForYear = (ruleTables != null) ? ruleTables.FirstOrDefault(rt => rt.AwardYear == studentAwardYear.Code && rt.Code == studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.NextStepsRuleId) : null;




            if (budgetComponentsForYear.Count() == 0 && studentBudgetComponentsForYear.Count() > 0)
            {
                throw new ApplicationException(string.Format("Cannot create shopping sheet for award year {0} that has no defined budget components", studentAwardYear.Code));
            }
            if (studentBudgetComponentsForYear.Count() == 0 && studentAwardsForYear.Count() == 0)
            {
                throw new ApplicationException(string.Format("Cannot create shopping sheet for student {0}, awardYear {1} with no awards and no budgets", studentAwardYear.StudentId, studentAwardYear.Code));
            }

            var shoppingSheet = new ShoppingSheet2(studentAwardYear.Code, studentAwardYear.StudentId);


            var tuitionFeesBudgetComponentsForYear = new List<BudgetComponent>();
            var housingMealsOnBudgetComponentsForYear = new List<BudgetComponent>();
            var housingMealsOffBudgetComponentsForYear = new List<BudgetComponent>();
            var bookSuppliesBudgetComponentsForYear = new List<BudgetComponent>();
            var transportationBudgetComponentsForYear = new List<BudgetComponent>();
            var otherEducationBudgetComponentsForYear = new List<BudgetComponent>();

            var schoolScholarshipAwards = new List<StudentAward>();
            var stateScholarshipAwards = new List<StudentAward>();
            var otherScholarshipAwards = new List<StudentAward>();
            var pellGrantAwards = new List<StudentAward>();
            var schoolGrantAwards = new List<StudentAward>();
            var stateGrantAwards = new List<StudentAward>();
            var otherGrantAwards = new List<StudentAward>();
            var schoolLoanAwards = new List<StudentAward>();
            var parentPlusLoanAwards = new List<StudentAward>();
            var dlSubLoanAwards = new List<StudentAward>();
            var dlUnsubLoanAwards = new List<StudentAward>();
            var privateLoanAwards = new List<StudentAward>();
            var otherLoanAwards = new List<StudentAward>();
            var workStudyAwards = new List<StudentAward>();
            var otherJobAwards = new List<StudentAward>();

            var empPaidTuitionBenefits = new List<StudentAward>();
            var schoolPdTuitionBenefits = new List<StudentAward>();
            var tuitionRemWaivers = new List<StudentAward>();
            var disadvantagedStudentGrant = new List<StudentAward>();
            var assistantshipsList = new List<StudentAward>();
            var incomeShareList = new List<StudentAward>();
            var hrsaLoanList = new List<StudentAward>();
            var gplusLoanList = new List<StudentAward>();



            var tuitionFeesParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.TuitionAndFees;
            var housingMealsOnParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HousingAndMealsOn;
            var housingMealsOffParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HousingAndMealsOff;
            var booksSuppliesParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.BooksAndSupplies;
            var transportationParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.Transportation;
            var otherEducationParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherEducationCosts;

            var ruleTable1 = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration;

            var schoolScholarshipParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SchoolScholarships;
            var stateScholarshipParams = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.StateScholarships;
            var OtherScholarshipParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherScholarships;
            var federalPellParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PellGrants;
            var schoolGrantParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SchoolGrants;
            var stateGrantParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.StateGrants;
            var otherGrantParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherGrants;
            var schoolLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SchoolLoans;
            var parentPlusLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.ParentPlusLoans;
            var dlSubLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.DlSubLoans;
            var dlUnsubLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.DlUnsubLoans;
            var privateLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PrivateLoans;
            var otherLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherLoans;

            var workStudyParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.WorkStudy;
            var otherJobParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherJobs;

            var employeeTuitionBenefits = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EmployeeTuitionBenefits;
            var disadvantagedStudentScholarship = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.DisadvantagedStudentScholarship;
            var graduateUndergraduateRuleId = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.GraduateUndergraduateRuleId;
            var gradPlusLoans = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.GradPlusLoans;
            var hrsaLoans = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HrsaLoans;
            var schoolPaidTuitionBenefits = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SchoolPaidTuitionBenefits;
            var tuitionRemWaiver = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.TuitionRemWaiver;
            var assistantships = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.Assistantships;
            var incomeShare = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.Assistantships;


            var subInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SubInterestRate;
            var subOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SubOriginationFee;
            var unsubInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.UnsubInterestRate;
            var unsubOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.UnsubOriginationFee;
            var privateInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PrivateInterestRate;
            var privateOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PrivateOriginationFee;
            var institutionInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.InstitutionInterestRate;
            var institutionOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.InstitutionOriginationFee;
            var gradPlusInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.GradPlusInterestRate;
            var gradPlusOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.GradPlusOriginationFee;
            var hrsaInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HRSAInterestRate;
            var hrsaOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HRSAOriginationFee;
            var plusInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PlusInterestRate;
            var plusOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PlusOriginationFee;



            if (Convert.ToInt32(studentAwardYear.Code) >= 2020)
            {
                // 2020 cfp sorting calcs
                // Budget stuff
                foreach (var budgetComponent in budgetComponentsForYear)
                {
                    foreach (var tuitionParam in tuitionFeesParam)
                    {
                        if (budgetComponent.Code == tuitionParam)
                        {
                            tuitionFeesBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var housingOnParam in housingMealsOnParam)
                    {
                        if (budgetComponent.Code == housingOnParam)
                        {
                            housingMealsOnBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var housingOffParam in housingMealsOffParam)
                    {
                        if (budgetComponent.Code == housingOffParam)
                        {
                            housingMealsOffBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var bookParam in booksSuppliesParam)
                    {
                        if (budgetComponent.Code == bookParam)
                        {
                            bookSuppliesBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var transParam in transportationParam)
                    {
                        if (budgetComponent.Code == transParam)
                        {
                            transportationBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var otherEdParam in otherEducationParam)
                    {
                        if (budgetComponent.Code == otherEdParam)
                        {
                            otherEducationBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }
                }

                var cfpTuitionFees = tuitionFeesBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpHousingMealsOn = housingMealsOnBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpHousingMealsOff = housingMealsOffBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpBookSupplies = bookSuppliesBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpTransportation = transportationBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpOtherCosts = otherEducationBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));

                if (cfpTuitionFees.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.TuitionAndFees, cfpTuitionFees.Sum()));
                }
                if (cfpHousingMealsOn.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.HousingAndMealsOnCampus, cfpHousingMealsOn.Sum()));
                }
                if (cfpHousingMealsOff.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.HousingAndMealsOffCampus, cfpHousingMealsOff.Sum()));
                }
                if (cfpBookSupplies.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.BooksAndSupplies, cfpBookSupplies.Sum()));
                }
                if (cfpTransportation.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.Transportation, cfpTransportation.Sum()));
                }
                if (cfpOtherCosts.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.OtherCosts, cfpOtherCosts.Sum()));
                }

                // Awards stuff
                //Filter student awards by award status category
                studentAwardsForYear = ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(studentAwardsForYear).ToList();

                foreach (var award in studentAwardsForYear)
                {
                    foreach (var schoolScholarship in schoolScholarshipParam)
                    {
                        if (award.Award.Code == schoolScholarship)
                        {
                            schoolScholarshipAwards.Add(award);
                        }
                    }
                    foreach (var empPaid in employeeTuitionBenefits)
                    {
                        if (award.Award.Code == empPaid)
                        {
                            empPaidTuitionBenefits.Add(award);
                        }
                    }
                    foreach (var schoolPaid in schoolPaidTuitionBenefits)
                    {
                        if (award.Award.Code == schoolPaid)
                        {
                            schoolPdTuitionBenefits.Add(award);
                        }
                    }
                    foreach (var tuitionRem in tuitionRemWaiver)
                    {
                        if (award.Award.Code == tuitionRem)
                        {
                            tuitionRemWaivers.Add(award);
                        }
                    }
                    foreach (var stateScholarship in stateScholarshipParams)
                    {
                        if (award.Award.Code == stateScholarship)
                        {
                            stateScholarshipAwards.Add(award);
                        }
                    }
                    foreach (var otherScholarship in OtherScholarshipParam)
                    {
                        if (award.Award.Code == otherScholarship)
                        {
                            otherScholarshipAwards.Add(award);
                        }
                    }
                    foreach (var pellGrant in federalPellParam )
                    {
                        if (award.Award.Code == pellGrant)
                        {
                            pellGrantAwards.Add(award);
                        }
                    }
                    foreach (var schoolGrant in schoolGrantParam)
                    {
                        if (award.Award.Code == schoolGrant)
                        {
                            schoolGrantAwards.Add(award);
                        }
                    }
                    foreach (var disStudentGrant in disadvantagedStudentScholarship)
                    {
                        if (award.Award.Code == disStudentGrant)
                        {
                            disadvantagedStudentGrant.Add(award);
                        }
                    }
                    foreach (var hrsa in hrsaLoans)
                    {
                        if (award.Award.Code == hrsa)
                        {
                            hrsaLoanList.Add(award);
                        }
                    }
                    foreach (var gplus in gradPlusLoans)
                    {
                        if (award.Award.Code == gplus)
                        {
                            gplusLoanList.Add(award);
                        }
                    }
                    foreach (var assis in assistantships)
                    {
                        if (award.Award.Code == assis)
                        {
                            assistantshipsList.Add(award);
                        }
                    }
                    foreach (var income in incomeShare)
                    {
                        if (award.Award.Code == income)
                        {
                            incomeShareList.Add(award);
                        }
                    }
                    foreach (var stateGrant in stateGrantParam)
                    {
                        if (award.Award.Code == stateGrant)
                        {
                            stateGrantAwards.Add(award);
                        }
                    }
                    foreach (var otherGrant in otherGrantParam)
                    {
                        if (award.Award.Code == otherGrant)
                        {
                            otherGrantAwards.Add(award);
                        }
                    }
                    foreach (var schoolLoan in schoolLoanParam)
                    {
                        if (award.Award.Code == schoolLoan)
                        {
                            schoolLoanAwards.Add(award);
                        }
                    }
                    foreach (var parentPlusLoan in parentPlusLoanParam)
                    {
                        if (award.Award.Code == parentPlusLoan)
                        {
                            parentPlusLoanAwards.Add(award);
                        }
                    }
                    foreach (var dlSubLoan in dlSubLoanParam)
                    {
                        if (award.Award.Code == dlSubLoan)
                        {
                            dlSubLoanAwards.Add(award);
                        }
                    }
                    foreach (var dlUnsubLoan in dlUnsubLoanParam)
                    {
                        if (award.Award.Code == dlUnsubLoan)
                        {
                            dlUnsubLoanAwards.Add(award);
                        }
                    }
                    foreach (var privateLoan in privateLoanParam)
                    {
                        if (award.Award.Code == privateLoan)
                        {
                            privateLoanAwards.Add(award);
                        }
                    }
                    foreach (var otherLoan in otherLoanParam)
                    {
                        if (award.Award.Code == otherLoan)
                        {
                            otherLoanAwards.Add(award);
                        }
                    }
                    foreach (var workStudy in workStudyParam)
                    {
                        if (award.Award.Code == workStudy)
                        {
                            workStudyAwards.Add(award);
                        }
                    }
                    foreach (var otherJob in otherJobParam)
                    {
                        if (award.Award.Code == otherJob)
                        {
                            otherJobAwards.Add(award);
                        }
                    }
                }
                if (schoolScholarshipAwards.Count() > 0)
                {
                    shoppingSheet.Scholarships.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SchoolScholarships, schoolScholarshipAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (stateScholarshipAwards.Count() > 0)
                {
                    shoppingSheet.Scholarships.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.StateScholarships, stateScholarshipAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (otherScholarshipAwards.Count() > 0)
                {
                    shoppingSheet.Scholarships.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherScholarships, otherScholarshipAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (tuitionRemWaivers.Count() > 0)
                {
                    shoppingSheet.TuitionRemWaiver = tuitionRemWaivers.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (schoolPdTuitionBenefits.Count() > 0)
                {
                    shoppingSheet.SchoolPaidTuitionBenefits = schoolPdTuitionBenefits.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (empPaidTuitionBenefits.Count() > 0)
                {
                    shoppingSheet.EmployerPaidTuitionBenefits = schoolPdTuitionBenefits.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (disadvantagedStudentGrant.Count() > 0)
                {
                    shoppingSheet.DisadvantagedStudentGrant = disadvantagedStudentGrant.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (hrsaLoanList.Count() > 0)
                {
                    shoppingSheet.HrsaLoans = hrsaLoanList.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (gplusLoanList.Count() > 0)
                {
                    shoppingSheet.GraduatePlusLoans = gplusLoanList.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (assistantshipsList.Count() > 0)
                {
                    shoppingSheet.Assistantships = assistantshipsList.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (incomeShareList.Count() > 0)
                {
                    shoppingSheet.IncomeShare = incomeShareList.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (pellGrantAwards.Count() > 0)
                {
                    shoppingSheet.Grants.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.PellGrants, pellGrantAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (schoolGrantAwards.Count() > 0)
                {
                    shoppingSheet.Grants.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SchoolGrants, schoolGrantAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (stateGrantAwards.Count() > 0)
                {
                    shoppingSheet.Grants.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.StateGrants, stateGrantAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (otherGrantAwards.Count() > 0)
                {
                    shoppingSheet.Grants.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherGrants, otherGrantAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (schoolLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SchoolLoans, schoolLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (parentPlusLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.ParentPlusLoans, parentPlusLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (dlSubLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.DlSubLoan, dlSubLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (dlUnsubLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.DlUnsubLoan, dlUnsubLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (privateLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.PrivateLoans, privateLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (otherLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherLoans, otherLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (workStudyAwards.Count() > 0)
                {
                    shoppingSheet.WorkOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.WorkStudy, workStudyAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (otherJobAwards.Count() > 0)
                {
                    shoppingSheet.WorkOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherJobs, otherJobAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }



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
                if (loanAmountRuleTableForYear != null )
                {
                    shoppingSheet.LoanAmountMessages = (await loanAmountRuleTableForYear.GetRuleTableResultAsync(studentAwardYear)).ToList();
                }
                if (edBenRuleTableForYear != null)
                {
                    shoppingSheet.EducationBenefitsMessages = (await edBenRuleTableForYear.GetRuleTableResultAsync(studentAwardYear)).ToList();
                }
                if (nextStepsRuleTableForYear != null)
                {
                    shoppingSheet.NextStepsMessages = (await nextStepsRuleTableForYear.GetRuleTableResultAsync(studentAwardYear)).ToList();
                }

                shoppingSheet.SubInterestRate = subInterestRate;
                shoppingSheet.SubOriginationFee = subOriginationFee;
                shoppingSheet.UnsubInterestRate = unsubInterestRate;
                shoppingSheet.UnsubOriginationFee = unsubOriginationFee;
                shoppingSheet.PrivateInterestRate = privateInterestRate;
                shoppingSheet.PrivateOriginationFee = privateOriginationFee;
                shoppingSheet.InstitutionInterestRate = institutionInterestRate;
                shoppingSheet.InstitutionOriginationFee = institutionOriginationFee;
                shoppingSheet.GradPlusInterestRate = gradPlusInterestRate;
                shoppingSheet.GradPlusOriginationFee = gradPlusOriginationFee;
                shoppingSheet.HrsaInterestRate = hrsaInterestRate;
                shoppingSheet.HrsaOriginationFee = hrsaOriginationFee;
                shoppingSheet.PlusInterestRate = plusInterestRate;
                shoppingSheet.PlusOriginationFee = plusOriginationFee;


                // return shopping sheet
                return shoppingSheet;
                // end 2020 cfp calc
            }
            else
            {
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.TuitionAndFees, tuitionFees.Sum()));
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.HousingAndMeals, housingMeals.Sum()));
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.BooksAndSupplies, booksSupplies.Sum()));
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.Transportation, transportation.Sum()));
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.OtherCosts, otherCosts.Sum()));
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
                        new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SchoolGrants, schoolGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                var pellGrants = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.PellGrants && studentAward.AwardAmount.HasValue);
                if (pellGrants.Count() > 0)
                {
                    shoppingSheet.GrantsAndScholarships.Add(
                         new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.PellGrants, pellGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                var stateGrants = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.StateGrants && studentAward.AwardAmount.HasValue);
                if (stateGrants.Count() > 0)
                {
                    shoppingSheet.GrantsAndScholarships.Add(
                         new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.StateGrants, stateGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                var otherGrants = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.OtherGrants && studentAward.AwardAmount.HasValue);
                if (otherGrants.Count() > 0)
                {
                    shoppingSheet.GrantsAndScholarships.Add(
                         new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherGrants, otherGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                var workStudy = studentAwardsForYear
                     .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.WorkStudy && studentAward.AwardAmount.HasValue);
                if (workStudy.Count() > 0)
                {
                    shoppingSheet.WorkOptions.Add(
                         new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.WorkStudy, workStudy.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                //if there are no loans for a particular award group, add the group anyway and set the amount to zero.
                var perkinsLoans = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.PerkinsLoans && studentAward.AwardAmount.HasValue);
                shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.PerkinsLoans, perkinsLoans.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));

                var subsidizedLoans = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.SubsidizedLoans && studentAward.AwardAmount.HasValue);
                shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SubsidizedLoans, subsidizedLoans.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));

                var unsubsidizedLoans = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.UnsubsidizedLoans && studentAward.AwardAmount.HasValue);
                shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.UnsubsidizedLoans, unsubsidizedLoans.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));

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
        public static async Task<ShoppingSheet3> BuildShoppingSheet3Async(
            StudentAwardYear studentAwardYear,
            IEnumerable<StudentAward> studentAwards,
            IEnumerable<BudgetComponent> budgetComponents,
            IEnumerable<StudentBudgetComponent> studentBudgetComponents,
            IEnumerable<FinancialAidApplication2> applications,
            IEnumerable<ShoppingSheetRuleTable> ruleTables,
            IEnumerable<string> cfpVersionType, IEnumerable<ProfileEFC> profileEfc, IEnumerable<ProfileEFC> fafsaEfc)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            if (studentAwardYear.CurrentConfiguration == null || studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration == null)
            {
                throw new ApplicationException(string.Format("Shopping Sheet is not configured for student {0}, awardYear {1}, office {2} ", studentAwardYear.StudentId, studentAwardYear.Code, studentAwardYear.CurrentOffice.Id));
            }

            if (cfpVersionType == null)
            {
                throw new ArgumentNullException("cfpVersionType");
            }

            var budgetComponentsForYear = (budgetComponents != null) ? budgetComponents.Where(bc => bc.AwardYear == studentAwardYear.Code).ToList() : new List<BudgetComponent>();
            var studentBudgetComponentsForYear = (studentBudgetComponents != null) ? studentBudgetComponents.Where(c => c.StudentId == studentAwardYear.StudentId && c.AwardYear == studentAwardYear.Code).ToList() : new List<StudentBudgetComponent>();
            var studentAwardsForYear = (studentAwards != null) ? studentAwards.Where(a => a.StudentAwardYear.Equals(studentAwardYear) && a.Award != null) : new List<StudentAward>();
            var applicationsForYear = (applications != null) ? applications.Where(app => app.StudentId == studentAwardYear.StudentId && app.AwardYear == studentAwardYear.Code).ToList() : new List<FinancialAidApplication2>();
            var ruleTableForYear = (ruleTables != null) ? ruleTables.FirstOrDefault(rt => rt.AwardYear == studentAwardYear.Code && rt.Code == studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.CustomMessageRuleTableId) : null;
            var loanAmountRuleTableForYear = (ruleTables != null) ? ruleTables.FirstOrDefault(rt => rt.AwardYear == studentAwardYear.Code && rt.Code == studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.LoanAmountTextRuleId) : null;
            var edBenRuleTableForYear = (ruleTables != null) ? ruleTables.FirstOrDefault(rt => rt.AwardYear == studentAwardYear.Code && rt.Code == studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EducationBenTextRuleId) : null;
            var nextStepsRuleTableForYear = (ruleTables != null) ? ruleTables.FirstOrDefault(rt => rt.AwardYear == studentAwardYear.Code && rt.Code == studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.NextStepsRuleId) : null;




            if (budgetComponentsForYear.Count() == 0 && studentBudgetComponentsForYear.Count() > 0)
            {
                throw new ApplicationException(string.Format("Cannot create shopping sheet for award year {0} that has no defined budget components", studentAwardYear.Code));
            }
            if (studentBudgetComponentsForYear.Count() == 0 && studentAwardsForYear.Count() == 0)
            {
                throw new ApplicationException(string.Format("Cannot create shopping sheet for student {0}, awardYear {1} with no awards and no budgets", studentAwardYear.StudentId, studentAwardYear.Code));
            }

            var shoppingSheet = new ShoppingSheet3(studentAwardYear.Code, studentAwardYear.StudentId);


            var tuitionFeesBudgetComponentsForYear = new List<BudgetComponent>();
            var housingMealsOnBudgetComponentsForYear = new List<BudgetComponent>();
            var housingMealsOffBudgetComponentsForYear = new List<BudgetComponent>();
            var bookSuppliesBudgetComponentsForYear = new List<BudgetComponent>();
            var transportationBudgetComponentsForYear = new List<BudgetComponent>();
            var otherEducationBudgetComponentsForYear = new List<BudgetComponent>();

            var schoolScholarshipAwards = new List<StudentAward>();
            var stateScholarshipAwards = new List<StudentAward>();
            var otherScholarshipAwards = new List<StudentAward>();
            var pellGrantAwards = new List<StudentAward>();
            var schoolGrantAwards = new List<StudentAward>();
            var stateGrantAwards = new List<StudentAward>();
            var otherGrantAwards = new List<StudentAward>();
            var schoolLoanAwards = new List<StudentAward>();
            var parentPlusLoanAwards = new List<StudentAward>();
            var dlSubLoanAwards = new List<StudentAward>();
            var dlUnsubLoanAwards = new List<StudentAward>();
            var privateLoanAwards = new List<StudentAward>();
            var otherLoanAwards = new List<StudentAward>();
            var workStudyAwards = new List<StudentAward>();
            var otherJobAwards = new List<StudentAward>();

            var empPaidTuitionBenefits = new List<StudentAward>();
            var schoolPdTuitionBenefits = new List<StudentAward>();
            var tuitionRemWaivers = new List<StudentAward>();
            var disadvantagedStudentGrant = new List<StudentAward>();
            var assistantshipsList = new List<StudentAward>();
            var incomeShareList = new List<StudentAward>();
            var hrsaLoanList = new List<StudentAward>();
            var gplusLoanList = new List<StudentAward>();



            var tuitionFeesParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.TuitionAndFees;
            var housingMealsOnParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HousingAndMealsOn;
            var housingMealsOffParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HousingAndMealsOff;
            var booksSuppliesParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.BooksAndSupplies;
            var transportationParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.Transportation;
            var otherEducationParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherEducationCosts;

            var ruleTable1 = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration;

            var schoolScholarshipParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SchoolScholarships;
            var stateScholarshipParams = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.StateScholarships;
            var OtherScholarshipParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherScholarships;
            var federalPellParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PellGrants;
            var schoolGrantParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SchoolGrants;
            var stateGrantParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.StateGrants;
            var otherGrantParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherGrants;
            var schoolLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SchoolLoans;
            var parentPlusLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.ParentPlusLoans;
            var dlSubLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.DlSubLoans;
            var dlUnsubLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.DlUnsubLoans;
            var privateLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PrivateLoans;
            var otherLoanParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherLoans;

            var workStudyParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.WorkStudy;
            var otherJobParam = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.OtherJobs;

            var employeeTuitionBenefits = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EmployeeTuitionBenefits;
            var disadvantagedStudentScholarship = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.DisadvantagedStudentScholarship;
            var graduateUndergraduateRuleId = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.GraduateUndergraduateRuleId;
            var gradPlusLoans = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.GradPlusLoans;
            var hrsaLoans = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HrsaLoans;
            var schoolPaidTuitionBenefits = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SchoolPaidTuitionBenefits;
            var tuitionRemWaiver = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.TuitionRemWaiver;
            var assistantships = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.Assistantships;
            var incomeShare = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.IncomeShare;


            var subInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SubInterestRate;
            var subOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.SubOriginationFee;
            var unsubInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.UnsubInterestRate;
            var unsubOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.UnsubOriginationFee;
            var privateInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PrivateInterestRate;
            var privateOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PrivateOriginationFee;
            var institutionInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.InstitutionInterestRate;
            var institutionOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.InstitutionOriginationFee;
            var gradPlusInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.GradPlusInterestRate;
            var gradPlusOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.GradPlusOriginationFee;
            var hrsaInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HRSAInterestRate;
            var hrsaOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.HRSAOriginationFee;
            var plusInterestRate = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PlusInterestRate;
            var plusOriginationFee = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.PlusOriginationFee;

            if (profileEfc != null && profileEfc.Any())
            {
                var profileEfcForYear = profileEfc.Where(e => e.AwardYear == studentAwardYear.Code);
                if (profileEfcForYear != null && profileEfcForYear.Any())
                {
                    shoppingSheet.ProfileEfc = profileEfcForYear.FirstOrDefault().Amount;
                }
            }
            if (fafsaEfc != null && fafsaEfc.Any())
            {
                var fafsaEfcForYear = fafsaEfc.Where(e => e.AwardYear == studentAwardYear.Code);
                if (fafsaEfcForYear != null && fafsaEfcForYear.Any())
                {
                    shoppingSheet.FafsaEfc = fafsaEfcForYear.FirstOrDefault().Amount;
                }
            }



            if (Convert.ToInt32(studentAwardYear.Code) >= 2020)
            {
                // 2020 cfp sorting calcs
                // Budget stuff
                foreach (var budgetComponent in budgetComponentsForYear)
                {
                    foreach (var tuitionParam in tuitionFeesParam)
                    {
                        if (budgetComponent.Code == tuitionParam)
                        {
                            tuitionFeesBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var housingOnParam in housingMealsOnParam)
                    {
                        if (budgetComponent.Code == housingOnParam)
                        {
                            housingMealsOnBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var housingOffParam in housingMealsOffParam)
                    {
                        if (budgetComponent.Code == housingOffParam)
                        {
                            housingMealsOffBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var bookParam in booksSuppliesParam)
                    {
                        if (budgetComponent.Code == bookParam)
                        {
                            bookSuppliesBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var transParam in transportationParam)
                    {
                        if (budgetComponent.Code == transParam)
                        {
                            transportationBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }

                    foreach (var otherEdParam in otherEducationParam)
                    {
                        if (budgetComponent.Code == otherEdParam)
                        {
                            otherEducationBudgetComponentsForYear.Add(budgetComponent);
                        }
                    }
                }

                var cfpTuitionFees = tuitionFeesBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpHousingMealsOn = housingMealsOnBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpHousingMealsOff = housingMealsOffBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpBookSupplies = bookSuppliesBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpTransportation = transportationBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));
                var cfpOtherCosts = otherEducationBudgetComponentsForYear.Join(studentBudgetComponentsForYear, bc => bc.Code, sbc => sbc.BudgetComponentCode, (bc, sbc) =>
                (
                    sbc.CampusBasedOverrideAmount.HasValue ? sbc.CampusBasedOverrideAmount.Value : sbc.CampusBasedOriginalAmount
                ));

                if (cfpTuitionFees.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.TuitionAndFees, cfpTuitionFees.Sum()));
                }
                if (cfpHousingMealsOn.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.HousingAndMealsOnCampus, cfpHousingMealsOn.Sum()));
                }
                if (cfpHousingMealsOff.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.HousingAndMealsOffCampus, cfpHousingMealsOff.Sum()));
                }
                if (cfpBookSupplies.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.BooksAndSupplies, cfpBookSupplies.Sum()));
                }
                if (cfpTransportation.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.Transportation, cfpTransportation.Sum()));
                }
                if (cfpOtherCosts.Count() > 0)
                {
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.OtherCosts, cfpOtherCosts.Sum()));
                }

                // Awards stuff
                //Filter student awards by award status category
                studentAwardsForYear = ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(studentAwardsForYear).ToList();

                foreach (var award in studentAwardsForYear)
                {
                    foreach (var schoolScholarship in schoolScholarshipParam)
                    {
                        if (award.Award.Code == schoolScholarship)
                        {
                            schoolScholarshipAwards.Add(award);
                        }
                    }
                    foreach (var empPaid in employeeTuitionBenefits)
                    {
                        if (award.Award.Code == empPaid)
                        {
                            empPaidTuitionBenefits.Add(award);
                        }
                    }
                    foreach (var schoolPaid in schoolPaidTuitionBenefits)
                    {
                        if (award.Award.Code == schoolPaid)
                        {
                            schoolPdTuitionBenefits.Add(award);
                        }
                    }
                    foreach (var tuitionRem in tuitionRemWaiver)
                    {
                        if (award.Award.Code == tuitionRem)
                        {
                            tuitionRemWaivers.Add(award);
                        }
                    }
                    foreach (var stateScholarship in stateScholarshipParams)
                    {
                        if (award.Award.Code == stateScholarship)
                        {
                            stateScholarshipAwards.Add(award);
                        }
                    }
                    foreach (var otherScholarship in OtherScholarshipParam)
                    {
                        if (award.Award.Code == otherScholarship)
                        {
                            otherScholarshipAwards.Add(award);
                        }
                    }
                    foreach (var pellGrant in federalPellParam)
                    {
                        if (award.Award.Code == pellGrant)
                        {
                            pellGrantAwards.Add(award);
                        }
                    }
                    foreach (var schoolGrant in schoolGrantParam)
                    {
                        if (award.Award.Code == schoolGrant)
                        {
                            schoolGrantAwards.Add(award);
                        }
                    }
                    foreach (var disStudentGrant in disadvantagedStudentScholarship)
                    {
                        if (award.Award.Code == disStudentGrant)
                        {
                            disadvantagedStudentGrant.Add(award);
                        }
                    }
                    foreach (var hrsa in hrsaLoans)
                    {
                        if (award.Award.Code == hrsa)
                        {
                            hrsaLoanList.Add(award);
                        }
                    }
                    foreach (var gplus in gradPlusLoans)
                    {
                        if (award.Award.Code == gplus)
                        {
                            gplusLoanList.Add(award);
                        }
                    }
                    foreach (var assis in assistantships)
                    {
                        if (award.Award.Code == assis)
                        {
                            assistantshipsList.Add(award);
                        }
                    }
                    foreach (var income in incomeShare)
                    {
                        if (award.Award.Code == income)
                        {
                            incomeShareList.Add(award);
                        }
                    }
                    foreach (var stateGrant in stateGrantParam)
                    {
                        if (award.Award.Code == stateGrant)
                        {
                            stateGrantAwards.Add(award);
                        }
                    }
                    foreach (var otherGrant in otherGrantParam)
                    {
                        if (award.Award.Code == otherGrant)
                        {
                            otherGrantAwards.Add(award);
                        }
                    }
                    foreach (var schoolLoan in schoolLoanParam)
                    {
                        if (award.Award.Code == schoolLoan)
                        {
                            schoolLoanAwards.Add(award);
                        }
                    }
                    foreach (var parentPlusLoan in parentPlusLoanParam)
                    {
                        if (award.Award.Code == parentPlusLoan)
                        {
                            parentPlusLoanAwards.Add(award);
                        }
                    }
                    foreach (var dlSubLoan in dlSubLoanParam)
                    {
                        if (award.Award.Code == dlSubLoan)
                        {
                            dlSubLoanAwards.Add(award);
                        }
                    }
                    foreach (var dlUnsubLoan in dlUnsubLoanParam)
                    {
                        if (award.Award.Code == dlUnsubLoan)
                        {
                            dlUnsubLoanAwards.Add(award);
                        }
                    }
                    foreach (var privateLoan in privateLoanParam)
                    {
                        if (award.Award.Code == privateLoan)
                        {
                            privateLoanAwards.Add(award);
                        }
                    }
                    foreach (var otherLoan in otherLoanParam)
                    {
                        if (award.Award.Code == otherLoan)
                        {
                            otherLoanAwards.Add(award);
                        }
                    }
                    foreach (var workStudy in workStudyParam)
                    {
                        if (award.Award.Code == workStudy)
                        {
                            workStudyAwards.Add(award);
                        }
                    }
                    foreach (var otherJob in otherJobParam)
                    {
                        if (award.Award.Code == otherJob)
                        {
                            otherJobAwards.Add(award);
                        }
                    }
                }
                if (schoolScholarshipAwards.Count() > 0)
                {
                    shoppingSheet.Scholarships.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SchoolScholarships, schoolScholarshipAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (stateScholarshipAwards.Count() > 0)
                {
                    shoppingSheet.Scholarships.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.StateScholarships, stateScholarshipAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (otherScholarshipAwards.Count() > 0)
                {
                    shoppingSheet.Scholarships.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherScholarships, otherScholarshipAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (tuitionRemWaivers.Count() > 0)
                {
                    shoppingSheet.TuitionRemWaiver = tuitionRemWaivers.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (schoolPdTuitionBenefits.Count() > 0)
                {
                    shoppingSheet.SchoolPaidTuitionBenefits = schoolPdTuitionBenefits.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (empPaidTuitionBenefits.Count() > 0)
                {
                    shoppingSheet.EmployerPaidTuitionBenefits = empPaidTuitionBenefits.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (disadvantagedStudentGrant.Count() > 0)
                {
                    shoppingSheet.DisadvantagedStudentGrant = disadvantagedStudentGrant.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (hrsaLoanList.Count() > 0)
                {
                    shoppingSheet.HrsaLoans = hrsaLoanList.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (gplusLoanList.Count() > 0)
                {
                    shoppingSheet.GraduatePlusLoans = gplusLoanList.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (assistantshipsList.Count() > 0)
                {
                    shoppingSheet.Assistantships = assistantshipsList.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (incomeShareList.Count() > 0)
                {
                    shoppingSheet.IncomeShare = incomeShareList.Sum(s => decimal.ToInt32(s.AwardAmount.Value));
                }
                if (pellGrantAwards.Count() > 0)
                {
                    shoppingSheet.Grants.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.PellGrants, pellGrantAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (schoolGrantAwards.Count() > 0)
                {
                    shoppingSheet.Grants.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SchoolGrants, schoolGrantAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (stateGrantAwards.Count() > 0)
                {
                    shoppingSheet.Grants.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.StateGrants, stateGrantAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (otherGrantAwards.Count() > 0)
                {
                    shoppingSheet.Grants.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherGrants, otherGrantAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (schoolLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SchoolLoans, schoolLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (parentPlusLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.ParentPlusLoans, parentPlusLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (dlSubLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.DlSubLoan, dlSubLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (dlUnsubLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.DlUnsubLoan, dlUnsubLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (privateLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.PrivateLoans, privateLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (otherLoanAwards.Count() > 0)
                {
                    shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherLoans, otherLoanAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (workStudyAwards.Count() > 0)
                {
                    shoppingSheet.WorkOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.WorkStudy, workStudyAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }
                if (otherJobAwards.Count() > 0)
                {
                    shoppingSheet.WorkOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherJobs, otherJobAwards.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }



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
                if (loanAmountRuleTableForYear != null)
                {
                    shoppingSheet.LoanAmountMessages = (await loanAmountRuleTableForYear.GetRuleTableResultAsync(studentAwardYear)).ToList();
                }
                if (edBenRuleTableForYear != null)
                {
                    shoppingSheet.EducationBenefitsMessages = (await edBenRuleTableForYear.GetRuleTableResultAsync(studentAwardYear)).ToList();
                }
                if (nextStepsRuleTableForYear != null)
                {
                    shoppingSheet.NextStepsMessages = (await nextStepsRuleTableForYear.GetRuleTableResultAsync(studentAwardYear)).ToList();
                }

                shoppingSheet.CfpVersionType = cfpVersionType.FirstOrDefault();
                shoppingSheet.SubInterestRate = subInterestRate;
                shoppingSheet.SubOriginationFee = subOriginationFee;
                shoppingSheet.UnsubInterestRate = unsubInterestRate;
                shoppingSheet.UnsubOriginationFee = unsubOriginationFee;
                shoppingSheet.PrivateInterestRate = privateInterestRate;
                shoppingSheet.PrivateOriginationFee = privateOriginationFee;
                shoppingSheet.InstitutionInterestRate = institutionInterestRate;
                shoppingSheet.InstitutionOriginationFee = institutionOriginationFee;
                shoppingSheet.GradPlusInterestRate = gradPlusInterestRate;
                shoppingSheet.GradPlusOriginationFee = gradPlusOriginationFee;
                shoppingSheet.HrsaInterestRate = hrsaInterestRate;
                shoppingSheet.HrsaOriginationFee = hrsaOriginationFee;
                shoppingSheet.PlusInterestRate = plusInterestRate;
                shoppingSheet.PlusOriginationFee = plusOriginationFee;



                // return shopping sheet
                return shoppingSheet;
                // end 2020 cfp calc
            }
            else
            {
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.TuitionAndFees, tuitionFees.Sum()));
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.HousingAndMeals, housingMeals.Sum()));
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.BooksAndSupplies, booksSupplies.Sum()));
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.Transportation, transportation.Sum()));
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
                    shoppingSheet.Costs.Add(new ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2.OtherCosts, otherCosts.Sum()));
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
                        new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SchoolGrants, schoolGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                var pellGrants = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.PellGrants && studentAward.AwardAmount.HasValue);
                if (pellGrants.Count() > 0)
                {
                    shoppingSheet.GrantsAndScholarships.Add(
                         new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.PellGrants, pellGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                var stateGrants = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.StateGrants && studentAward.AwardAmount.HasValue);
                if (stateGrants.Count() > 0)
                {
                    shoppingSheet.GrantsAndScholarships.Add(
                         new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.StateGrants, stateGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                var otherGrants = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.OtherGrants && studentAward.AwardAmount.HasValue);
                if (otherGrants.Count() > 0)
                {
                    shoppingSheet.GrantsAndScholarships.Add(
                         new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.OtherGrants, otherGrants.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                var workStudy = studentAwardsForYear
                     .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.WorkStudy && studentAward.AwardAmount.HasValue);
                if (workStudy.Count() > 0)
                {
                    shoppingSheet.WorkOptions.Add(
                         new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.WorkStudy, workStudy.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));
                }

                //if there are no loans for a particular award group, add the group anyway and set the amount to zero.
                var perkinsLoans = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.PerkinsLoans && studentAward.AwardAmount.HasValue);
                shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.PerkinsLoans, perkinsLoans.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));

                var subsidizedLoans = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.SubsidizedLoans && studentAward.AwardAmount.HasValue);
                shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.SubsidizedLoans, subsidizedLoans.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));

                var unsubsidizedLoans = studentAwardsForYear
                    .Where(studentAward => studentAward.Award.ShoppingSheetGroup == ShoppingSheetAwardGroup.UnsubsidizedLoans && studentAward.AwardAmount.HasValue);
                shoppingSheet.LoanOptions.Add(new ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2.UnsubsidizedLoans, unsubsidizedLoans.Sum(s => decimal.ToInt32(s.AwardAmount.Value))));

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
}
