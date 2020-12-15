// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentProposedRegistrationResultTests
    {
        private List<InstantEnrollmentRegistrationBaseRegisteredSection> sections;
        private List<InstantEnrollmentRegistrationBaseMessage> messages;

        [TestInitialize]
        public void InstantEnrollmentProposedRegistrationResultTests_Initialize()
        {
            sections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>()
            {
                new InstantEnrollmentRegistrationBaseRegisteredSection("SECT1", 10000),
            };
            messages = new List<InstantEnrollmentRegistrationBaseMessage>()
            {
                new InstantEnrollmentRegistrationBaseMessage("SECT1", "SECT1 Message"),
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentProposedRegistrationResult_Constructor_NullSections()
        {
            var result = new InstantEnrollmentProposedRegistrationResult(false, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentProposedRegistrationResult_Constructor_NullMessages()
        {
            var result = new InstantEnrollmentProposedRegistrationResult(false, sections, null);
        }

        [TestMethod]
        public void InstantEnrollmentProposedRegistrationResult_Constructor_Valid()
        {
            var result = new InstantEnrollmentProposedRegistrationResult(true, sections, messages);
            Assert.IsTrue(result.ErrorOccurred);
            Assert.AreEqual("SECT1", result.RegisteredSections[0].SectionId);
            Assert.AreEqual(10000, result.RegisteredSections[0].SectionCost);
            Assert.AreEqual("SECT1", result.RegistrationMessages[0].MessageSection);
            Assert.AreEqual("SECT1 Message", result.RegistrationMessages[0].Message);
        }

        [TestCleanup]
        public void InstantEnrollmentProposedRegistrationResultTests_Initialize_Cleanup()
        {
            sections = null;
            messages = null;
        }
    }
}
