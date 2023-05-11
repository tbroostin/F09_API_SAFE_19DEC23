// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class MyProgressConfigurationTests
    {
        [TestClass]
        public class MyProgressConfiguration_Constructor
        {
            [TestMethod]
            public void MyProgressParameterizedConfiguration()
            {
                var config = new MyProgressConfiguration(false, false, false, false, false);
                Assert.IsFalse(config.ShowAcademicLevelsStanding);
                Assert.IsFalse(config.HideProgressBarOverallProgress);
                Assert.IsFalse(config.HideProgressBarTotalCredits);
                Assert.IsFalse(config.HideProgressBarTotalInstitutionalCredits);
                Assert.IsFalse(config.ShowPseudoCoursesInRequirements);

            }
            [TestMethod]
            public void MyProgressParameterizedConfiguration_true()
            {
                var config = new MyProgressConfiguration(true, true, true, true, true);
                Assert.IsTrue(config.ShowAcademicLevelsStanding);
                Assert.IsTrue(config.HideProgressBarOverallProgress);
                Assert.IsTrue(config.HideProgressBarTotalCredits);
                Assert.IsTrue(config.HideProgressBarTotalInstitutionalCredits);
                Assert.IsTrue(config.ShowPseudoCoursesInRequirements);
            }
        }

    }
}
