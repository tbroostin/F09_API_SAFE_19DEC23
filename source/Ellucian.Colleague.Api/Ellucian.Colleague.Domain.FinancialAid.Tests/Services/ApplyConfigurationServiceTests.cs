//Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Services
{
    /// <summary>
    /// Test class for ApplyConfigurationService
    /// </summary>
    [TestClass]
    public class ApplyConfigurationServiceTests
    {
        protected TestStudentAwardYearRepository studentAwardYearRepository;
        protected List<StudentAwardYear> inputStudentAwardYears;
        protected IEnumerable<FinancialAidOffice> inputOffices;
        protected TestFinancialAidOfficeRepository officeRepository;
        protected CurrentOfficeService currentOfficeService;

        public string studentId;

        public void ApplyConfigurationServiceTestsInitialize()
        {
            studentId = "0003914";
            officeRepository = new TestFinancialAidOfficeRepository();
            inputOffices = officeRepository.GetFinancialAidOfficesAsync().Result;
            currentOfficeService = new CurrentOfficeService(inputOffices);
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            inputStudentAwardYears = studentAwardYearRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService).Result.ToList();
        }

        public void ApplyConfigurationServiceTestsCleanup()
        {
            studentId = null;
            officeRepository = null;
            inputOffices = null;
            currentOfficeService = null;
            studentAwardYearRepository = null;
            inputStudentAwardYears = null;
        }

        /// <summary>
        /// Test class for FilterStudentAwardYears method
        /// </summary>
        [TestClass]
        public class FilterStudentAwardYearsTests : ApplyConfigurationServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                ApplyConfigurationServiceTestsInitialize();
            }

            [TestCleanup]
            public void Cleanup()
            {
                ApplyConfigurationServiceTestsCleanup();
            }

            /// <summary>
            /// Tests if FilterStudentAwardYears method returns no studentAwardYears currentConfiguration 
            /// IsSelfService flag of which is set to false
            /// </summary>
            [TestMethod]
            public void StudentAwardYearsFiltered_NotIsSelfServiceActiveTest()
            {
                inputStudentAwardYears.ForEach(say => say.CurrentConfiguration.IsSelfServiceActive = false);
                Assert.IsTrue(ApplyConfigurationService.FilterStudentAwardYears(inputStudentAwardYears).Count() == 0);
            }

            /// <summary>
            /// Tests if FilterStudentAwardYears method returns all studentAwardYears currentConfiguration 
            /// IsSelfService flag of which is set to true
            /// </summary>
            [TestMethod]
            public void StudentAwardYearsNotFiltered_IsSelfServiceActiveTest()
            {
                inputStudentAwardYears.ForEach(say => say.CurrentConfiguration.IsSelfServiceActive = true);
                Assert.AreEqual(inputStudentAwardYears.Count, ApplyConfigurationService.FilterStudentAwardYears(inputStudentAwardYears).Count());
            }
        }

        /// <summary>
        /// Test class for FilterAwardLetters method
        /// </summary>
        [TestClass]
        public class FilterStudentAwardLettersTests : ApplyConfigurationServiceTests
        {
            private TestAwardLetterRepository awardLetterRepository;
            private IEnumerable<AwardLetter> inputAwardLetters;
            private TestFafsaRepository fafsaRepository;
            private IEnumerable<Fafsa> inputFafsaRecords;

            [TestInitialize]
            public void Initialize()
            {
                ApplyConfigurationServiceTestsInitialize();
                awardLetterRepository = new TestAwardLetterRepository();
                fafsaRepository = new TestFafsaRepository();

                inputFafsaRecords = fafsaRepository.GetFafsasAsync(new List<string>() { studentId }, inputStudentAwardYears.Select(say => say.Code)).Result;
                inputAwardLetters = awardLetterRepository.GetAwardLetters(studentId, inputStudentAwardYears, inputFafsaRecords);
            }

            [TestCleanup]
            public void Cleanup()
            {
                ApplyConfigurationServiceTestsCleanup();
                awardLetterRepository = null;
                inputAwardLetters = null;
                fafsaRepository = null;
                inputFafsaRecords = null;
            }

            /// <summary>
            /// Tests if no award letters are returned if the configuration for corresponding years
            /// has IsAwardLetterActive flag off
            /// </summary>
            [TestMethod]
            public void FilterAwardLetters_NotIsAwardLetterActiveTest()
            {
                inputStudentAwardYears.ForEach(say => say.CurrentConfiguration.IsAwardLetterActive = false);
                inputAwardLetters = awardLetterRepository.GetAwardLetters(studentId, inputStudentAwardYears, inputFafsaRecords);
                var filteredAwardLetterCount = ApplyConfigurationService.FilterAwardLetters(inputAwardLetters).Count();
                Assert.AreNotEqual(inputAwardLetters.Count(), filteredAwardLetterCount);
                Assert.IsTrue(filteredAwardLetterCount == 0);
            }

            /// <summary>
            /// Tests if all award letters are returned from FilterAwardLetters method if configuration of the corresponding
            /// award year IsAwardLetterActive flag is on
            /// </summary>
            [TestMethod]
            public void FilterAwardLetters_IsAwardLetterActiveTest()
            {
                inputStudentAwardYears.ForEach(say => say.CurrentConfiguration.IsAwardLetterActive = true);
                inputAwardLetters = awardLetterRepository.GetAwardLetters(studentId, inputStudentAwardYears, inputFafsaRecords);

                Assert.AreEqual(inputAwardLetters.Count(), ApplyConfigurationService.FilterAwardLetters(inputAwardLetters).Count());
            }

            /// <summary>
            /// Tests if no award letter is returned after runing FilterAwardLetters on a single award letter
            /// that is associated with a year configured with IsAwardLetterActive set to false
            /// </summary>
            [TestMethod]
            public void FilterAwardLetters_SingleAwardLetterNotReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                inputStudentAwardYears.First().CurrentConfiguration.IsAwardLetterActive = false;
                var inputAwardLetter = awardLetterRepository.GetAwardLetters(studentId, inputStudentAwardYears, inputFafsaRecords).First(al => al.AwardYear.Code == awardYear);
                
                Assert.IsNull(ApplyConfigurationService.FilterAwardLetters(inputAwardLetter));
            }

            /// <summary>
            /// Tests if the same award letter is returned after runing FilterAwardLetters on a single award letter
            /// that is associated with a year configured with IsAwardLetterActive set to true
            /// </summary>
            [TestMethod]
            public void FilterAwardLetters_SingleAwardLetterReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                inputStudentAwardYears.First().CurrentConfiguration.IsAwardLetterActive = true;
                var inputAwardLetter = awardLetterRepository.GetAwardLetters(studentId, inputStudentAwardYears, inputFafsaRecords).First(al => al.AwardYear.Code == awardYear);
                var filteredAwardLetter = ApplyConfigurationService.FilterAwardLetters(inputAwardLetter);
                Assert.IsNotNull(filteredAwardLetter);
                Assert.IsTrue(inputAwardLetter.Equals(filteredAwardLetter));
            }
        }

        /// <summary>
        /// Test class for FilterStudentAwards as well as FilterAwardsByAwardStatusCategory methods
        /// </summary>
        [TestClass]
        public class FilterStudentAwardsTests : ApplyConfigurationServiceTests
        {
            private TestStudentAwardRepository studentAwardRepository;
            private List<StudentAward> inputStudentAwards;
            private TestFinancialAidReferenceDataRepository faReferenceDataRepository;
            private IEnumerable<Award> allAwards;
            private IEnumerable<AwardStatus> allAwardStatuses;

            [TestInitialize]
            public void Initialize()
            {
                ApplyConfigurationServiceTestsInitialize();
                studentAwardRepository = new TestStudentAwardRepository();
                faReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
                allAwards = faReferenceDataRepository.Awards;
                allAwardStatuses = faReferenceDataRepository.AwardStatuses;
                inputStudentAwards = studentAwardRepository.GetAllStudentAwardsAsync(studentId, inputStudentAwardYears, allAwards, allAwardStatuses).Result.ToList();
            }

            [TestCleanup]
            public void Cleanup()
            {
                ApplyConfigurationServiceTestsCleanup();
                studentAwardRepository = null;
                inputStudentAwards = null;
                faReferenceDataRepository = null;
                allAwards = null;
                allAwardStatuses = null;
            }

            /// <summary>
            /// Tests if all the awards get returned by FilterStudentAwards method if all of them have IsViewable set to true
            /// </summary>
            [TestMethod]
            public void FilterStudentAwards_AllAreViewableTest()
            {
                //Makes sure all awards will have IsViewable property set to true
                inputStudentAwardYears.ForEach(say => {
                    say.CurrentConfiguration.IsAwardingActive = true;
                    say.CurrentConfiguration.ExcludeAwardCategoriesView = null;
                    say.CurrentConfiguration.ExcludeAwardsView = null;
                    say.CurrentConfiguration.ExcludeAwardStatusCategoriesView = null;
                    say.CurrentConfiguration.ExcludeAwardPeriods = null;
                });         
                
                inputStudentAwards = studentAwardRepository.GetAllStudentAwardsAsync(studentId, inputStudentAwardYears, allAwards, allAwardStatuses).Result.ToList();
                var filteredStudentAwardsCount = ApplyConfigurationService.FilterStudentAwards(inputStudentAwards).Count();
                
                Assert.IsTrue(filteredStudentAwardsCount > 0);
                Assert.AreEqual(inputStudentAwards.Count(), filteredStudentAwardsCount);
            }

            /// <summary>
            /// Tests if no student awards are returned from FilterStudentAwards method if
            /// the corresponding configuration for the year has IsAwardingActive flag set to false
            /// </summary>
            [TestMethod]
            public void FilterStudentAwards_AllAreNotViewableTest()
            {
                inputStudentAwardYears.ForEach(say => 
                    say.CurrentConfiguration.IsAwardingActive = false);

                inputStudentAwards = studentAwardRepository.GetAllStudentAwardsAsync(studentId, inputStudentAwardYears, allAwards, allAwardStatuses).Result.ToList();
                var filteredStudentAwardsCount = ApplyConfigurationService.FilterStudentAwards(inputStudentAwards).Count();

                Assert.IsTrue(filteredStudentAwardsCount == 0);
            }

            /// <summary>
            /// Tests if no award is returned when calling FilterStudentAwards on a single award that is associated with a year
            /// configured with IsAwardingActive set to false
            /// </summary>
            [TestMethod]
            public void FilterStudentAward_NoAwardIsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                inputStudentAwardYears.First().CurrentConfiguration.IsAwardingActive = false;
                var inputStudentAward = studentAwardRepository.GetAllStudentAwardsAsync(studentId, inputStudentAwardYears, allAwards, allAwardStatuses).Result.First(sa => sa.StudentAwardYear.Code == awardYear);
                Assert.IsNull(ApplyConfigurationService.FilterStudentAwards(inputStudentAward));
            }

            /// <summary>
            /// Tests if the award is returned and it equals the input one when calling 
            /// FilterStudentAwards on a single award that IsViewable
            /// </summary>
            [TestMethod]
            public void FilterStudentAward_SameAwardIsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                currentConfiguration.IsAwardingActive = true;
                currentConfiguration.ExcludeAwardCategoriesView = null;
                currentConfiguration.ExcludeAwardStatusCategoriesView = null;
                currentConfiguration.ExcludeAwardsView = null;
                currentConfiguration.ExcludeAwardPeriods = null;

                var inputStudentAward = studentAwardRepository.GetAllStudentAwardsAsync(studentId, inputStudentAwardYears, allAwards, allAwardStatuses).Result.First(sa => sa.StudentAwardYear.Code == awardYear);
                var filteredAward = ApplyConfigurationService.FilterStudentAwards(inputStudentAward);
                Assert.IsNotNull(filteredAward);
                Assert.IsTrue(inputStudentAward.Equals(filteredAward));
            }

            /// <summary>
            /// Tests if an award that !IsViewable but its code is included in the overrides list and
            /// at least one of its award periods is also overriden is returned
            /// by the FilterStudentAward method
            /// </summary>
            [TestMethod]
            public void FilterStudentAward_NotIsViewableReturnedAwardTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var inputAward = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear).First();
                var awardPeriodToOverride = inputAward.StudentAwardPeriods.First();
                
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;                
                //include award's code in to be excluded from viewing list
                currentConfiguration.ExcludeAwardsView = new List<string>() { inputAward.Award.Code };
                //include award's code in awards to be overriden from viewing
                var awardIdOverrides = new List<string>() { inputAward.Award.Code };
                var awardPeriodIdsToOverride = new List<string> { awardPeriodToOverride.AwardPeriodId };
                var filteredAward = ApplyConfigurationService.FilterStudentAwards(new List<StudentAward>(){inputAward}, awardIdOverrides, awardPeriodIdsToOverride).FirstOrDefault();
                Assert.IsNotNull(filteredAward);
                Assert.IsTrue(filteredAward.StudentAwardPeriods.Count == 1);
            }

            /// <summary>
            /// Tests if no awards are returned back if all of the awards have a status that is set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet = new List<AwardStatusCategory>() { AwardStatusCategory.Rejected };
                foreach (var award in awardsForYear)
                {
                    award.StudentAwardPeriods.ForEach(sap => sap.AwardStatus = new AwardStatus("R", "Rejected", AwardStatusCategory.Rejected));
                }

                Assert.IsTrue(ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear).Count() == 0);
            }

            /// <summary>
            /// Tests if all awards are returned back if all awards have the status that is not set to be 
            /// excluded in the current year configuration
            /// </summary>
            [TestMethod]
            public void FilterAwardsForAwardLetterAndShoppingSheetDisplay_AllAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet = new List<AwardStatusCategory>() { AwardStatusCategory.Rejected };
                foreach (var award in awardsForYear)
                {
                    award.StudentAwardPeriods.ForEach(sap => sap.AwardStatus = new AwardStatus("A", "Accepted", AwardStatusCategory.Accepted));
                }

                var filteredAwardsCount = ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear).Count();
                Assert.AreEqual(awardsForYear.Count(), filteredAwardsCount);
            }

            /// <summary>
            /// Tests if the award with multiple student award periods gets filtered off the award periods
            /// that have award statuses included in the list of statuses to be excluded from the award letter
            /// and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void FilterAwardsForAwardLetterAndShoppingSheetDisplay_StudentAwardPeriodFilteredTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var award = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear).First(sa => sa.StudentAwardPeriods.Count > 1);                
                var initialAwardPeriodCount = award.StudentAwardPeriods.Count;
                award.StudentAwardPeriods.First().AwardStatus = new AwardStatus("R", "R", AwardStatusCategory.Rejected);

                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;                
                currentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet = new List<AwardStatusCategory>() { AwardStatusCategory.Rejected };                
                
                var filteredAward = ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(new List<StudentAward>() {award}).FirstOrDefault();

                Assert.IsNotNull(filteredAward);
                Assert.AreNotEqual(initialAwardPeriodCount, filteredAward.StudentAwardPeriods.Count);

            }

            /// <summary>
            /// Tests if no awards are returned back if all of the awards have codes that are set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void AllAwardsExcluded_FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet = awardsForYear.Select(a => a.Award.Code).Distinct().ToList();
                

                Assert.IsFalse(ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear).Any());
            }

            /// <summary>
            /// Tests if some awards are returned back if only one of the awards has a code that is set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void NotAllAwardsExcluded_FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet = new List<string>() { awardsForYear.First().Award.Code };


                Assert.IsTrue(ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear).Any());
            }

            /// <summary>
            /// Tests if all awards are returned back if no award codes are set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void NoAwardsExcluded_FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet = new List<string>();

                var actualAwards = ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear);
                Assert.IsTrue(actualAwards.Any());
                Assert.AreEqual(awardsForYear.Count(), actualAwards.Count());
            }

            /// <summary>
            /// Tests if no awards are returned back if all of the award categories are set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void AllAwardCategoriesExcluded_FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = awardsForYear.Select(a => a.Award.AwardCategory.Code).Distinct().ToList();


                Assert.IsFalse(ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear).Any());
            }

            /// <summary>
            /// Tests if some awards are returned back if only one of the award categories has a code that is set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void NotAllAwardCategoriesExcluded_FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = new List<string>() { awardsForYear.First().Award.AwardCategory.Code };


                Assert.IsTrue(ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear).Any());
            }

            /// <summary>
            /// Tests if all awards are returned back if no award categories are set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void NoAwardCategoriesExcluded_FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = new List<string>();

                var actualAwards = ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear);
                Assert.IsTrue(actualAwards.Any());
                Assert.AreEqual(awardsForYear.Count(), actualAwards.Count());
            }

            /// <summary>
            /// Tests if no awards are returned back if all of the award periods have codes that are set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void AllAwardPeriodsExcluded_FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = awardsForYear.SelectMany(a => a.StudentAwardPeriods).Select(ap => ap.AwardPeriodId).Distinct().ToList();


                Assert.IsFalse(ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear).Any());
            }

            /// <summary>
            /// Tests if some awards are returned back if only one of the award periods is set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void NotAllAwardPeriodsExcluded_FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = new List<string>() { awardsForYear.First().StudentAwardPeriods.First().AwardPeriodId };


                Assert.IsTrue(ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear).Any());
            }

            /// <summary>
            /// Tests if all awards are returned back if no award periods are set to be excluded
            /// from award letter and shopping sheet in the current year configuration
            /// </summary>
            [TestMethod]
            public void NoAwardPeriodsExcluded_FilterAwardsForAwardLetterAndShoppingSheetDisplay_NoAwardsReturnedTest()
            {
                var awardYear = inputStudentAwardYears.First().Code;
                var currentConfiguration = inputStudentAwardYears.First().CurrentConfiguration;
                var awardsForYear = inputStudentAwards.Where(sa => sa.StudentAwardYear.Code == awardYear);
                currentConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = new List<string>();

                var actualAwards = ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(awardsForYear);
                Assert.IsTrue(actualAwards.Any());
                Assert.AreEqual(awardsForYear.Count(), actualAwards.Count());
            }
        }
    }
}
