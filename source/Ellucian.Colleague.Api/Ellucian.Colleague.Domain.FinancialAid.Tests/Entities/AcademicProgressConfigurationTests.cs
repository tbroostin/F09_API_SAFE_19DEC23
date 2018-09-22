/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AcademicProgressConfigurationTests
    {
        public string officeId;
        public bool isAcademicProgressActive;
        public bool isAcademicProgressHistoryActive;
        public List<AcademicProgressPropertyConfiguration> propertyConfigurations;
        public List<string> academicProgressTypesToDisplay;

        public AcademicProgressConfiguration configuration;

        public void AcademicProgressConfigurationTestsInitialize()
        {
            officeId = "MAIN";
            isAcademicProgressActive = true;
            isAcademicProgressHistoryActive = true;
            propertyConfigurations = new List<AcademicProgressPropertyConfiguration>()
            {
                new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeAttemptedCredits),
                new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeOverallGpa)                                             
            };

            academicProgressTypesToDisplay = new List<string>();
            
            configuration = new AcademicProgressConfiguration(officeId);
        }

        [TestClass]
        public class AcademicProgressConfigurationConstructorTests : AcademicProgressConfigurationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressConfigurationTestsInitialize();
            }

            [TestMethod]
            public void OfficeIdTest()
            {
                Assert.AreEqual(officeId, configuration.OfficeId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OfficeIdRequiredTest()
            {
                new AcademicProgressConfiguration(null);
            }

            [TestMethod]
            public void PropertyConfigurationsInitializedTest()
            {
                Assert.IsNotNull(configuration.DetailPropertyConfigurations);
                Assert.IsFalse(configuration.DetailPropertyConfigurations.Any());
            }

            [TestMethod]
            public void IsAcademicProgressActiveInitializedToFalseTest()
            {
                Assert.IsFalse(configuration.IsSatisfactoryAcademicProgressActive);
            }

            [TestMethod]
            public void IsAcademicProgressActiveSetToTrueTest()
            {
                Assert.IsTrue(isAcademicProgressActive);
            }

            [TestMethod]
            public void IsAcademicProgressHistoryActiveInitializedToFalseTest()
            {
                Assert.IsFalse(configuration.IsSatisfactoryAcademicProgressHistoryActive);
            }

            [TestMethod]
            public void IsAcademicProgressHistoryActiveSetToTrueTest()
            {
                Assert.IsTrue(isAcademicProgressHistoryActive);
            }

            [TestMethod]
            public void AcademicProgressTypesToDisplay_GetSetTest()
            {
                academicProgressTypesToDisplay.Add("Federal");
                academicProgressTypesToDisplay.Add("FederalC");

                configuration.AcademicProgressTypesToDisplay = academicProgressTypesToDisplay;
                
                foreach (var sapType in academicProgressTypesToDisplay)
                {
                    Assert.IsTrue(configuration.AcademicProgressTypesToDisplay.Contains(sapType));
                }
            }

            [TestMethod]
            public void AcademicProgressTypesToDisplay_InitializedTest()
            {
                Assert.IsNotNull(configuration.AcademicProgressTypesToDisplay);
                Assert.AreEqual(academicProgressTypesToDisplay.Count(), configuration.AcademicProgressTypesToDisplay.Count());
            }

        }

        [TestClass]
        public class EqualsAndGetHashCodeTests : AcademicProgressConfigurationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressConfigurationTestsInitialize();
            }

            [TestMethod]
            public void Equal_OfficeIdEqualTest()
            {
                var test = new AcademicProgressConfiguration(officeId);
                Assert.AreEqual(test, configuration);
            }

            [TestMethod]
            public void EqualHashCode_OfficeIdEqualTest()
            {
                var test = new AcademicProgressConfiguration(officeId);
                Assert.AreEqual(test.GetHashCode(), configuration.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_OfficeIdNotEqualTest()
            {
                var test = new AcademicProgressConfiguration("foobar");
                Assert.AreNotEqual(test, configuration);
            }

            [TestMethod]
            public void NotEqualHashCode_OfficeIdNotEqualTest()
            {
                var test = new AcademicProgressConfiguration("foobar");
                Assert.AreNotEqual(test.GetHashCode(), configuration.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_NullTest()
            {
                Assert.IsFalse(configuration.Equals(null));
            }

            [TestMethod]
            public void NotEqual_DifferentObjectTypeTest()
            {
                Assert.IsFalse(configuration.Equals(AwardCategoryType.Grant));
            }
        }
    }
}
