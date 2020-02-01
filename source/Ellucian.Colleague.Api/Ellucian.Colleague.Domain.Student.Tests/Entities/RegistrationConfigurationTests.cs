// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationConfigurationTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegistrationConfigurationConstructor_NegativeOffset()
        {
            var config = new RegistrationConfiguration(true, -1);

        }

        [TestMethod]
        public void RegistrationConfigurationConstructor_Success()
        {
            var config = new RegistrationConfiguration(true, 0);
            Assert.IsTrue(config.RequireFacultyAddAuthorization);
            Assert.AreEqual(0, config.AddAuthorizationStartOffsetDays);
            Assert.IsFalse(config.PromptForDropReason);
            Assert.IsFalse(config.RequireDropReason);
            Assert.IsFalse(config.ShowBooksOnPrintedSchedules);
            Assert.IsFalse(config.ShowCommentsOnPrintedSchedules);
            Assert.IsTrue(config.AddDefaultTermsToDegreePlan);
            Assert.IsFalse(config.QuickRegistrationIsEnabled);
            Assert.AreEqual(0, config.QuickRegistrationTermCodes.Count); 
        }

        [TestMethod]
        public void RegistrationConfigurationConstructor_Success2()
        {
            var config = new RegistrationConfiguration(false, 9, true);
            config.PromptForDropReason = true;
            config.RequireDropReason = true;
            config.ShowBooksOnPrintedSchedules = true;
            config.ShowCommentsOnPrintedSchedules = true;
            config.AddDefaultTermsToDegreePlan = false;
            config.AddQuickRegistrationTerm("2019/FA");
            Assert.IsFalse (config.RequireFacultyAddAuthorization);
            Assert.AreEqual(9, config.AddAuthorizationStartOffsetDays);
            Assert.IsTrue(config.PromptForDropReason);
            Assert.IsTrue(config.RequireDropReason);
            Assert.IsTrue(config.ShowBooksOnPrintedSchedules);
            Assert.IsTrue(config.ShowCommentsOnPrintedSchedules);
            Assert.IsFalse(config.AddDefaultTermsToDegreePlan);
            Assert.IsTrue(config.QuickRegistrationIsEnabled);
            Assert.AreEqual(1, config.QuickRegistrationTermCodes.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationConfiguration_AddQuickRegistrationTerm_null_throws_exception()
        {
            var config = new RegistrationConfiguration(false, 9, true);
            Assert.AreEqual(0, config.QuickRegistrationTermCodes.Count);
            config.AddQuickRegistrationTerm(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationConfiguration_AddQuickRegistrationTerm_empty_string_throws_exception()
        {
            var config = new RegistrationConfiguration(false, 9, true);
            Assert.AreEqual(0, config.QuickRegistrationTermCodes.Count);
            config.AddQuickRegistrationTerm(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void RegistrationConfiguration_AddQuickRegistrationTerm_throws_exception_when_QuickRegistrationIsEnabled_false()
        {
            var config = new RegistrationConfiguration(false, 9, false);
            Assert.IsFalse(config.QuickRegistrationIsEnabled);
            config.AddQuickRegistrationTerm("2019/FA");
        }
    }
}

