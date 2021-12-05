// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
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

            [TestMethod]
            public void FacultyGradingConfiguration_FinalGradesLastDateAttendedNeverAttendedDisplayBehavior_Get_Set()
            {
                var config = new FacultyGradingConfiguration();
                Assert.AreEqual(LastDateAttendedNeverAttendedFieldDisplayType.Editable, config.FinalGradesLastDateAttendedNeverAttendedDisplayBehavior);
                config.FinalGradesLastDateAttendedNeverAttendedDisplayBehavior = LastDateAttendedNeverAttendedFieldDisplayType.Hidden;
                Assert.AreEqual(LastDateAttendedNeverAttendedFieldDisplayType.Hidden, config.FinalGradesLastDateAttendedNeverAttendedDisplayBehavior);
                config.FinalGradesLastDateAttendedNeverAttendedDisplayBehavior = LastDateAttendedNeverAttendedFieldDisplayType.ReadOnly;
                Assert.AreEqual(LastDateAttendedNeverAttendedFieldDisplayType.ReadOnly, config.FinalGradesLastDateAttendedNeverAttendedDisplayBehavior);
            }

            [TestMethod]
            public void FacultyGradingConfiguration_MidtermGradesLastDateAttendedNeverAttendedDisplayBehavior_Get_Set()
            {
                var config = new FacultyGradingConfiguration();
                Assert.AreEqual(LastDateAttendedNeverAttendedFieldDisplayType.Editable, config.MidtermGradesLastDateAttendedNeverAttendedDisplayBehavior);
                config.MidtermGradesLastDateAttendedNeverAttendedDisplayBehavior = LastDateAttendedNeverAttendedFieldDisplayType.Hidden;
                Assert.AreEqual(LastDateAttendedNeverAttendedFieldDisplayType.Hidden, config.MidtermGradesLastDateAttendedNeverAttendedDisplayBehavior);
                config.MidtermGradesLastDateAttendedNeverAttendedDisplayBehavior = LastDateAttendedNeverAttendedFieldDisplayType.ReadOnly;
                Assert.AreEqual(LastDateAttendedNeverAttendedFieldDisplayType.ReadOnly, config.MidtermGradesLastDateAttendedNeverAttendedDisplayBehavior);
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
