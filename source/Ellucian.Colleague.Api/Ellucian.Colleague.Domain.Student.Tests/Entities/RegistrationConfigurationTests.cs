// Copyright 2018-2022 Ellucian Company L.P. and its affiliates.
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
            new RegistrationConfiguration(true, false, false, -1);
        }

        [TestMethod]
        public void RegistrationConfigurationConstructor_Success()
        {
            var config = new RegistrationConfiguration(true, false, false, 0);
            Assert.IsTrue(config.RequireFacultyAddAuthorization);
            Assert.AreEqual(0, config.AddAuthorizationStartOffsetDays);
            Assert.IsFalse(config.PromptForDropReason);
            Assert.IsFalse(config.RequireDropReason);
            Assert.IsFalse(config.ShowBooksOnPrintedSchedules);
            Assert.IsFalse(config.ShowCommentsOnPrintedSchedules);
            Assert.IsTrue(config.AddDefaultTermsToDegreePlan);
            Assert.IsFalse(config.QuickRegistrationIsEnabled);
            Assert.AreEqual(0, config.QuickRegistrationTermCodes.Count);
            Assert.IsNull(config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsFalse(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);
            Assert.IsFalse(config.SeatServiceIsEnabled);
        }

        [TestMethod]
        public void RegistrationConfigurationConstructor_Success2()
        {
            var config = new RegistrationConfiguration(false, false, false, 9, true);
            config.PromptForDropReason = true;
            config.RequireDropReason = true;
            config.ShowBooksOnPrintedSchedules = true;
            config.ShowCommentsOnPrintedSchedules = true;
            config.AddDefaultTermsToDegreePlan = false;
            config.AddQuickRegistrationTerm("2019/FA");
            Assert.IsFalse(config.RequireFacultyAddAuthorization);
            Assert.AreEqual(9, config.AddAuthorizationStartOffsetDays);
            Assert.IsTrue(config.PromptForDropReason);
            Assert.IsTrue(config.RequireDropReason);
            Assert.IsTrue(config.ShowBooksOnPrintedSchedules);
            Assert.IsTrue(config.ShowCommentsOnPrintedSchedules);
            Assert.IsFalse(config.AddDefaultTermsToDegreePlan);
            Assert.IsTrue(config.QuickRegistrationIsEnabled);
            Assert.AreEqual(1, config.QuickRegistrationTermCodes.Count);
            Assert.IsNull(config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsFalse(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);
            Assert.IsFalse(config.SeatServiceIsEnabled);
        }

        [TestMethod]
        public void RegistrationConfigurationConstructor_Success3()
        {
            var config = new RegistrationConfiguration(false, false, false, 9, false, true);
            config.PromptForDropReason = true;
            config.RequireDropReason = true;
            config.ShowBooksOnPrintedSchedules = true;
            config.ShowCommentsOnPrintedSchedules = true;
            config.AddDefaultTermsToDegreePlan = false;
            Assert.IsFalse(config.RequireFacultyAddAuthorization);
            Assert.AreEqual(9, config.AddAuthorizationStartOffsetDays);
            Assert.IsTrue(config.PromptForDropReason);
            Assert.IsTrue(config.RequireDropReason);
            Assert.IsTrue(config.ShowBooksOnPrintedSchedules);
            Assert.IsTrue(config.ShowCommentsOnPrintedSchedules);
            Assert.IsFalse(config.AddDefaultTermsToDegreePlan);
            Assert.IsFalse(config.QuickRegistrationIsEnabled);
            Assert.AreEqual(0, config.QuickRegistrationTermCodes.Count);
            Assert.IsNull(config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsFalse(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);
            Assert.IsTrue(config.SeatServiceIsEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationConfiguration_AddQuickRegistrationTerm_null_throws_exception()
        {
            var config = new RegistrationConfiguration(false, false, false, 9, true);
            Assert.AreEqual(0, config.QuickRegistrationTermCodes.Count);
            config.AddQuickRegistrationTerm(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationConfiguration_AddQuickRegistrationTerm_empty_string_throws_exception()
        {
            var config = new RegistrationConfiguration(false, false, false, 9, true);
            Assert.AreEqual(0, config.QuickRegistrationTermCodes.Count);
            config.AddQuickRegistrationTerm(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void RegistrationConfiguration_AddQuickRegistrationTerm_throws_exception_when_QuickRegistrationIsEnabled_false()
        {
            var config = new RegistrationConfiguration(false, false, false, 9, false);
            Assert.IsFalse(config.QuickRegistrationIsEnabled);
            config.AddQuickRegistrationTerm("2019/FA");
        }

        [TestMethod]
        public void RegistrationConfiguration_AlwaysPromptUsersForIntentToWithdrawWhenDropping_true()
        {
            var config = new RegistrationConfiguration(true, false, false, 0);
            Assert.IsNull(config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsFalse(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);

            config.AlwaysPromptUsersForIntentToWithdrawWhenDropping = true;
            Assert.IsNull(config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsTrue(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);
        }

        [TestMethod]
        public void RegistrationConfiguration_CensusDateNumberForPromptingIntentToWithdraw_valid()
        {
            var config = new RegistrationConfiguration(true, false, false, 0);
            Assert.IsNull(config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsFalse(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);

            config.CensusDateNumberForPromptingIntentToWithdraw = 1;
            Assert.AreEqual(1, config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsFalse(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RegistrationConfiguration_CensusDateNumberForPromptingIntentToWithdraw_invalid()
        {
            var config = new RegistrationConfiguration(true, false, false, 0);
            Assert.IsNull(config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsFalse(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);

            config.CensusDateNumberForPromptingIntentToWithdraw = -1;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegistrationConfiguration_CensusDateNumberForPromptingIntentToWithdraw_valid_with_AlwaysPromptUsersForIntentToWithdrawWhenDropping_true()
        {
            var config = new RegistrationConfiguration(true, false, false, 0);
            Assert.IsNull(config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsFalse(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);

            config.CensusDateNumberForPromptingIntentToWithdraw = 1;
            config.AlwaysPromptUsersForIntentToWithdrawWhenDropping = true;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegistrationConfiguration_AlwaysPromptUsersForIntentToWithdrawWhenDropping_true_with_CensusDateNumberForPromptingIntentToWithdraw_valid()
        {
            var config = new RegistrationConfiguration(true, false, false, 0);
            Assert.IsNull(config.CensusDateNumberForPromptingIntentToWithdraw);
            Assert.IsFalse(config.AlwaysPromptUsersForIntentToWithdrawWhenDropping);

            config.AlwaysPromptUsersForIntentToWithdrawWhenDropping = true;
            config.CensusDateNumberForPromptingIntentToWithdraw = 1;
        }
    }
}

