// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentZeroCostRegistrationResultTests
    {
        private List<InstantEnrollmentRegistrationBaseRegisteredSection> sections;
        private List<InstantEnrollmentRegistrationBaseMessage> messages;
        private string personId = "0000001";
        private string username = "aaubergine";

        [TestInitialize]
        public void InstantEnrollmentZeroCostRegistrationResult_Initialize()
        {
            sections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>()
            {
                new InstantEnrollmentRegistrationBaseRegisteredSection("POLI-100-01", 3),
                null,
                new InstantEnrollmentRegistrationBaseRegisteredSection("ART-100-01", 1),
                new InstantEnrollmentRegistrationBaseRegisteredSection("FREN-101-01", 0),
                new InstantEnrollmentRegistrationBaseRegisteredSection("ARTH-101-02", null),
            };
            messages = new List<InstantEnrollmentRegistrationBaseMessage>()
            {
                new InstantEnrollmentRegistrationBaseMessage("ART-100-01", "ART-100-01 Message"),
                null,
                new InstantEnrollmentRegistrationBaseMessage(null, "Message without section"),
                new InstantEnrollmentRegistrationBaseMessage("ARTH-101-02", "ARTH-101-02 Message"),
            };
        }

        [TestCleanup]
        public void InstantEnrollmentZeroCostRegistrationResult_Cleanup()
        {
            sections = null;
            messages = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentZeroCostRegistrationResult_Constructor_NullSections()
        {
            new InstantEnrollmentZeroCostRegistrationResult(false, null, messages, personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentZeroCostRegistrationResult_Constructor_NullMessages()
        {
            new InstantEnrollmentZeroCostRegistrationResult(false, sections, null, personId, null);
        }

        [TestMethod]
        public void InstantEnrollmentZeroCostRegistrationResult_Constructor_NullPersonId()
        {
            var result = new InstantEnrollmentZeroCostRegistrationResult(false, sections, messages, null, null);
            Assert.AreEqual(null, result.PersonId);
        }

        [TestMethod]
        public void InstantEnrollmentZeroCostRegistrationResult_Constructor_IsEmptyPersonId()
        {
            var result = new InstantEnrollmentZeroCostRegistrationResult(false, sections, messages, string.Empty, null);
            Assert.AreEqual(string.Empty, result.PersonId);
        }

        [TestMethod]
        public void InstantEnrollmentZeroCostRegistrationResult_Constructor_Valid()
        {
            var result = new InstantEnrollmentZeroCostRegistrationResult(true, sections, messages, personId, username);
            Assert.AreEqual(true, result.ErrorOccurred);
            Assert.AreEqual(personId, result.PersonId);
            Assert.AreEqual(sections.Count, result.RegisteredSections.Count);
            Assert.AreEqual(messages.Count, result.RegistrationMessages.Count);
            Assert.AreEqual(username, result.UserName);
        }
    }
}
