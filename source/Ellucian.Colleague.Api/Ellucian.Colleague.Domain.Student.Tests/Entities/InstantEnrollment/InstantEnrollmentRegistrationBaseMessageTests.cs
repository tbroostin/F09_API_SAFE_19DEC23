// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentRegistrationBaseMessageTests
    {
        private string section;
        private string message;

        [TestInitialize]
        public void InstantEnrollmentRegistrationBaseMessageTests_Initialize()
        {
           message = "The message";
           section = "SECT1";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationBaseMessageTests_Constructor_NullCode()
        {
            var msg = new InstantEnrollmentRegistrationBaseMessage(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationBaseMessageTests_Constructor_EmptyCode()
        {
            var msg = new InstantEnrollmentRegistrationBaseMessage(String.Empty, null);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationBaseMessageTests_Constructor_ValidMessage()
        {
            var msg = new InstantEnrollmentRegistrationBaseMessage(null, message);
            Assert.AreEqual(message, msg.Message);
            Assert.IsNull(msg.MessageSection);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationBaseMessageTests_Constructor_ValidMessageSection()
        {
            var msg = new InstantEnrollmentRegistrationBaseMessage(section, message);
            Assert.AreEqual(message, msg.Message);
            Assert.AreEqual(section, msg.MessageSection);
        }

        [TestCleanup]
        public void InstantEnrollmentRegistrationBaseMessageTests_Cleanup()
        {
            message = null;
            section = null;
        }
    }
}
