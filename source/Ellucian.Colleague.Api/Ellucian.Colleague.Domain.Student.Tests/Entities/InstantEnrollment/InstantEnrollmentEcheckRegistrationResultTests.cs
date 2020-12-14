// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentEcheckRegistrationResultTests
    {
        private List<InstantEnrollmentRegistrationBaseRegisteredSection> sections;
        private List<InstantEnrollmentRegistrationBaseMessage> messages;
        private string studentId;
        private string receipt;
        private string userName;

        [TestInitialize]
        public void InstantEnrollmentEcheckRegistrationResultTests_Initialize()
        {
            sections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>()
            {
                new InstantEnrollmentRegistrationBaseRegisteredSection("SECT1", 10000),
            };
            messages = new List<InstantEnrollmentRegistrationBaseMessage>()
            {
                new InstantEnrollmentRegistrationBaseMessage("SECT1", "SECT1 Message"),
            };
            studentId = "0000001";
            receipt = "RECEIPT";
            userName = "aaubergine";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentEcheckRegistrationResult_Constructor_NullSections()
        {
            var result = new InstantEnrollmentEcheckRegistrationResult(false, null, null, studentId, receipt, userName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentEcheckRegistrationResult_Constructor_NullMessages()
        {
            var result = new InstantEnrollmentEcheckRegistrationResult(false, sections, null, studentId, receipt, userName);
        }

        [TestMethod]
        public void InstantEnrollmentEcheckRegistrationResult_Constructor_Valid()
        {
            var result = new InstantEnrollmentEcheckRegistrationResult(true, sections, messages, studentId, receipt, userName);
            Assert.IsTrue(result.ErrorOccurred);
            Assert.AreEqual("SECT1", result.RegisteredSections[0].SectionId);
            Assert.AreEqual(10000, result.RegisteredSections[0].SectionCost);
            Assert.AreEqual("SECT1", result.RegistrationMessages[0].MessageSection);
            Assert.AreEqual("SECT1 Message", result.RegistrationMessages[0].Message);
            Assert.AreEqual(studentId, result.PersonId);
            Assert.AreEqual(receipt, result.CashReceipt);
            Assert.AreEqual(userName, result.UserName);
        }

        [TestCleanup]
        public void InstantEnrollmentEcheckRegistrationResultTests_Initialize_Cleanup()
        {
            sections = null;
            messages = null;
            receipt = null;
        }
    }
}
