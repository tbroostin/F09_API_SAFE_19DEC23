// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class FacultyGradingConfigurationTests
    {
        [TestClass]
        public class FacultyGradingConfiguration_Constructor
        {           
            [TestMethod]
            public void FacultyGradingConfiguration_DefaultValues()
            {
                var config = new FacultyGradingConfiguration();
                Assert.IsFalse(config.IncludeCrosslistedStudents);
                Assert.IsFalse(config.IncludeDroppedWithdrawnStudents);
                Assert.AreEqual(0, config.AllowedGradingTerms.Count());
                Assert.AreEqual(0, config.NumberOfMidtermGrades);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void FacultyGradingConfiguration_NumberOfMidtermGrades_Negative()
            {
                var config = new FacultyGradingConfiguration();
                config.NumberOfMidtermGrades = -1;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void FacultyGradingConfiguration_NumberOfMidtermGrades_Over6()
            {
                var config = new FacultyGradingConfiguration();
                config.NumberOfMidtermGrades = 7;
            }
        }
        [TestClass]
        public class FacultyGradingConfiguration_AddGradingTerms
        {
            private FacultyGradingConfiguration configuration;

            [TestInitialize]
            public void Initialize()
            {
                configuration = new FacultyGradingConfiguration();
            }

            [TestCleanup]
            public void CleanUp()
            {
                configuration = null;
            }

            [TestMethod]
            public void FacultyGradingConfiguration_AddGradingTerm_No_Duplicates()
            {
                configuration.AddGradingTerm("2015/FA");
                configuration.AddGradingTerm("2015/FA");
                Assert.AreEqual(1, configuration.AllowedGradingTerms.Count());
                Assert.IsTrue(configuration.AllowedGradingTerms.Contains("2015/FA"));
            }

        }

    }
}
