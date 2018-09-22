// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class GraduationConfigurationTests
    {
        [TestClass]
        public class GraduationConfiguration_Constructor
        {           


            [TestMethod]
            public void GraduationConfigurationLists()
            {
                var config = new GraduationConfiguration();
                Assert.AreEqual(0, config.GraduationTerms.Count);
                Assert.AreEqual(0, config.ApplicationQuestions.Count);
                Assert.IsFalse(config.RequireImmediatePayment);
                Assert.IsFalse(config.OverrideCapAndGownDisplay);
            }
        }

        [TestClass]
        public class GraduationConfiguration_AddGraduationTerms
        {
            private GraduationConfiguration configuration;

            [TestInitialize]
            public void Initialize()
            {
                configuration = new GraduationConfiguration();
            }

            [TestCleanup]
            public void CleanUp()
            {
                configuration = null;
            }

            [TestMethod]
            public void GraduationConfiguration_AddGradTerm_No_Duplicates()
            {
                configuration.AddGraduationTerm("2015/FA");
                configuration.AddGraduationTerm("2015/FA");
                Assert.AreEqual(1, configuration.GraduationTerms.Count());
                Assert.IsTrue(configuration.GraduationTerms.Contains("2015/FA"));
            }
        }

        [TestClass]
        public class GraduationConfiguration_AddApplicationQuestion
        {
            private GraduationConfiguration configuration;

            [TestInitialize]
            public void Initialize()
            {
                configuration = new GraduationConfiguration();
            }

            [TestCleanup]
            public void CleanUp()
            {
                configuration = null;
            }

            [TestMethod]
            public void GraduationConfiguration_AddApplicationQuestion_No_Duplicates()
            {
                configuration.AddGraduationQuestion(GraduationQuestionType.AttendCommencement, true);
                configuration.AddGraduationQuestion(GraduationQuestionType.AttendCommencement, false);
                Assert.AreEqual(1, configuration.ApplicationQuestions.Count());
                Assert.IsTrue(configuration.ApplicationQuestions.ElementAt(0).IsRequired);
                Assert.AreEqual(GraduationQuestionType.AttendCommencement, configuration.ApplicationQuestions.ElementAt(0).Type);
            }
        }
    }
}
