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
    public class AcademicProgressPropertyConfigurationTests
    {
        public AcademicProgressPropertyType type;
        public string label;
        public string description;
        public bool isHidden;

        public AcademicProgressPropertyConfiguration propertyConfiguration;

        public void AcademicProgressPropertyConfigurationTestsInitialize()
        {
            type = AcademicProgressPropertyType.CumulativeAttemptedCredits;
            label = "Cumulative Attempted Credits";
            description = "this is the description";
            isHidden = false;

            propertyConfiguration = new AcademicProgressPropertyConfiguration(type);
        }

        [TestClass]
        public class AcademicProgressPropertyConfigurationConstructorTests : AcademicProgressPropertyConfigurationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressPropertyConfigurationTestsInitialize();
            }

            [TestMethod]
            public void TypeTest()
            {
                Assert.AreEqual(type, propertyConfiguration.Type);
            }

            [TestMethod]
            public void IsHiddenDefaultsToTrueTest()
            {
                Assert.IsTrue(propertyConfiguration.IsHidden);
            }

            [TestMethod]
            public void LabelGetSetTest()
            {
                propertyConfiguration.Label = label;
                Assert.AreEqual(label, propertyConfiguration.Label);
            }

            [TestMethod]
            public void DescriptionGetSetTest()
            {
                propertyConfiguration.Description = description;
                Assert.AreEqual(description, propertyConfiguration.Description);
            }

            [TestMethod]
            public void IsHiddenGetSetTest()
            {
                propertyConfiguration.IsHidden = isHidden;
                Assert.AreEqual(isHidden, propertyConfiguration.IsHidden);
            }
        }

        [TestClass]
        public class EqualsAndHashCodeTests : AcademicProgressPropertyConfigurationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressPropertyConfigurationTestsInitialize();
            }

            [TestMethod]
            public void Equal_TypeEqualTest()
            {
                var test = new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeAttemptedCredits);
                Assert.AreEqual(test, propertyConfiguration);
            }

            [TestMethod]
            public void EqualHashCode_TypeEqualTest()
            {
                var test = new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeAttemptedCredits);
                Assert.AreEqual(test.GetHashCode(), propertyConfiguration.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_TypeNotEqualTest()
            {
                var test = new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeAttemptedCreditsExcludingRemedial);
                Assert.AreNotEqual(test, propertyConfiguration);
            }

            [TestMethod]
            public void NotEqualHashCode_TypeNotEqualTest()
            {
                var test = new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeAttemptedCreditsExcludingRemedial);
                Assert.AreNotEqual(test.GetHashCode(), propertyConfiguration.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_NullTest()
            {
                Assert.IsFalse(propertyConfiguration.Equals(null));
            }

            [TestMethod]
            public void NotEqual_DifferentObjectTypeTest()
            {
                Assert.IsFalse(propertyConfiguration.Equals(AcademicProgressPropertyType.CumulativeCompletedCredits));
            }
        }
    }
}
