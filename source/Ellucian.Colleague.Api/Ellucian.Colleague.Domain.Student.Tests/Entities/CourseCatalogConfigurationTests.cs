// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CourseCatalogConfigurationTests
    {
        [TestClass]
        public class CourseCatalogConfiguration_Constructor_Tests
        {
            private DateTime? startDate = DateTime.Now.AddDays(-10);
            private DateTime? endDate = DateTime.Now;

            [TestMethod]
            public void CourseCatalogConfigurationLists_NullDates()
            {
                var config = new CourseCatalogConfiguration(null, null);
                Assert.IsNotNull(config.CatalogFilterOptions);
                Assert.IsNull(config.EarliestSearchDate);
                Assert.IsNull(config.LatestSearchDate);
                Assert.AreEqual(0, config.CatalogFilterOptions.Count());
            }

            [TestMethod]
            public void CourseCatalogConfigurationLists_StartDateOnly()
            {
                var config = new CourseCatalogConfiguration(startDate, null);
                Assert.IsNotNull(config.CatalogFilterOptions);
                Assert.AreEqual(startDate, config.EarliestSearchDate);
                Assert.IsNull(config.LatestSearchDate);
                Assert.AreEqual(0, config.CatalogFilterOptions.Count());
            }

            [TestMethod]
            public void CourseCatalogConfigurationLists_EndDateOnly()
            {
                var config = new CourseCatalogConfiguration(null, endDate);
                Assert.IsNotNull(config.CatalogFilterOptions);
                Assert.AreEqual(endDate, config.LatestSearchDate);
                Assert.IsNull(config.EarliestSearchDate);
                Assert.AreEqual(0, config.CatalogFilterOptions.Count());
            }

            [TestMethod]
            public void CourseCatalogConfigurationLists_BothDates()
            {
                var config = new CourseCatalogConfiguration(startDate, endDate);
                Assert.IsNotNull(config.CatalogFilterOptions);
                Assert.AreEqual(startDate, config.EarliestSearchDate);
                Assert.AreEqual(endDate, config.LatestSearchDate);
                Assert.AreEqual(0, config.CatalogFilterOptions.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CourseCatalogConfigurationLists_LatestBeforeEarliestDate()
            {
                // Reverse the dates
                var config = new CourseCatalogConfiguration(endDate, startDate);
            }
        }

        [TestClass]
        public class CourseCatalogConfiguration_Properties_Tests
        {
            private DateTime? startDate = DateTime.Now.AddDays(-10);
            private DateTime? endDate = DateTime.Now;

            [TestMethod]
            public void CourseCatalogConfiguration_ShowCourseSectionFeeInformation_Get_Set()
            {
                var config = new CourseCatalogConfiguration(startDate, endDate);
                Assert.IsNotNull(config);
                Assert.IsFalse(config.ShowCourseSectionFeeInformation);
                config.ShowCourseSectionFeeInformation = true;
                Assert.IsTrue(config.ShowCourseSectionFeeInformation);
            }

            [TestMethod]
            public void CourseCatalogConfiguration_ShowCourseSectionBookInformation_Get_Set()
            {
                var config = new CourseCatalogConfiguration(startDate, endDate);
                Assert.IsNotNull(config);
                Assert.IsFalse(config.ShowCourseSectionBookInformation);
                config.ShowCourseSectionBookInformation = true;
                Assert.IsTrue(config.ShowCourseSectionBookInformation);
            }

            [TestMethod]
            public void CourseCatalogConfiguration_DefaultSelfServiceCourseCatalogSearchView_Get_Set()
            {
                var config = new CourseCatalogConfiguration(startDate, endDate);
                Assert.AreEqual(SelfServiceCourseCatalogSearchView.SubjectSearch, config.DefaultSelfServiceCourseCatalogSearchView);
                config.DefaultSelfServiceCourseCatalogSearchView = SelfServiceCourseCatalogSearchView.AdvancedSearch;
                Assert.AreEqual(SelfServiceCourseCatalogSearchView.AdvancedSearch, config.DefaultSelfServiceCourseCatalogSearchView);
            }

            [TestMethod]
            public void CourseCatalogConfiguration_DefaultSelfServiceCourseCatalogSearchResultView_Get_Set()
            {
                var config = new CourseCatalogConfiguration(startDate, endDate);
                Assert.AreEqual(SelfServiceCourseCatalogSearchResultView.CatalogListing, config.DefaultSelfServiceCourseCatalogSearchResultView);
                config.DefaultSelfServiceCourseCatalogSearchResultView = SelfServiceCourseCatalogSearchResultView.SectionListing;
                Assert.AreEqual(SelfServiceCourseCatalogSearchResultView.SectionListing, config.DefaultSelfServiceCourseCatalogSearchResultView);
            }
        }

        [TestClass]
        public class CourseCatalogConfiguration_AddCatalogFilterOption_Tests
        {
            private CourseCatalogConfiguration configuration;

            [TestInitialize]
            public void Initialize()
            {
                configuration = new CourseCatalogConfiguration(DateTime.Now.AddDays(-10), DateTime.Now);
            }

            [TestCleanup]
            public void CleanUp()
            {
                configuration = null;
            }

            [TestMethod]
            public void CourseCatalogConfiguration_AddCatalogFilterOption()
            {
                configuration.AddCatalogFilterOption(CatalogFilterType.Terms, true);
                Assert.AreEqual(1, configuration.CatalogFilterOptions.Count());
                Assert.IsTrue(configuration.CatalogFilterOptions[0].IsHidden);
                Assert.AreEqual(CatalogFilterType.Terms, configuration.CatalogFilterOptions[0].Type);
            }

            [TestMethod]
            public void CourseCatalogConfiguration_AddCatalogFilterOption_No_Duplicates()
            {
                configuration.AddCatalogFilterOption(CatalogFilterType.Terms, true);
                configuration.AddCatalogFilterOption(CatalogFilterType.Terms, true);
                Assert.AreEqual(1, configuration.CatalogFilterOptions.Count());
            }
        }

        
    }
}
