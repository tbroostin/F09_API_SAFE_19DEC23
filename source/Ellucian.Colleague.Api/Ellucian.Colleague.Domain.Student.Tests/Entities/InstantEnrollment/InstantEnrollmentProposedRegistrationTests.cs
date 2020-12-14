// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentRegistrationProposedRegistrationTests
    {
        private string personId;
        private InstantEnrollmentPersonDemographic demo;
        private InstantEnrollmentPersonDemographic demoNullEmail;
        private InstantEnrollmentPersonDemographic demoEmptyEmail;
        private string acadProgram;
        private string catalog;
        private List<InstantEnrollmentRegistrationBaseSectionToRegister> sections;

        [TestInitialize]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Initialize()
        {
            personId = "0000001";
            demo = new InstantEnrollmentPersonDemographic("Joe", "TestClass") { EmailAddress = "joe@testclass.com" };
            demoNullEmail = new InstantEnrollmentPersonDemographic("Joe", "TestClass");
            demoEmptyEmail = new InstantEnrollmentPersonDemographic("Joe", "TestClass") { EmailAddress = String.Empty};
            acadProgram = "Program";
            catalog = "Catalog";
            sections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
            {
                new InstantEnrollmentRegistrationBaseSectionToRegister("SECT1", 300),
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_NullPersonNullDemo()
        {
            var reg = new InstantEnrollmentProposedRegistration(null, null, acadProgram, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_EmptyPersonNullDemo()
        {
            var reg = new InstantEnrollmentProposedRegistration(String.Empty, null, acadProgram, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_NullEmail()
        {
            var reg = new InstantEnrollmentProposedRegistration(null, demoNullEmail, acadProgram, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_EmptyEmail()
        {
            var reg = new InstantEnrollmentProposedRegistration(null, demoEmptyEmail, acadProgram, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_NullAcadProgram()
        {
            var reg = new InstantEnrollmentProposedRegistration(null, demo, null, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_EmptyAcadProgram()
        {
            var reg = new InstantEnrollmentProposedRegistration(null, demo, String.Empty, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_NullCatalog()
        {
            var reg = new InstantEnrollmentProposedRegistration(null, demo, acadProgram, null, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_EmptyCatalog()
        {
            var reg = new InstantEnrollmentProposedRegistration(null, demo, acadProgram, String.Empty, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_NullSections()
        {
            var reg = new InstantEnrollmentProposedRegistration(null, demo, acadProgram, catalog, null);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_ValidDemo()
        {
            var reg = new InstantEnrollmentProposedRegistration(null, demo, acadProgram, catalog, sections);
            Assert.AreEqual(String.Empty, reg.PersonId);
            Assert.AreSame(demo, reg.PersonDemographic);
            Assert.AreEqual(acadProgram, reg.AcademicProgram);
            Assert.AreEqual(catalog, reg.Catalog);
            Assert.AreEqual("SECT1", reg.ProposedSections[0].SectionId);
            Assert.AreEqual(300, reg.ProposedSections[0].AcademicCredits);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Constructor_ValidPersonId()
        {
            var reg = new InstantEnrollmentProposedRegistration(personId, null, acadProgram, catalog, sections);
            Assert.AreEqual(personId, reg.PersonId);
            Assert.AreEqual(acadProgram, reg.AcademicProgram);
            Assert.AreEqual(catalog, reg.Catalog);
            Assert.AreEqual("SECT1", reg.ProposedSections[0].SectionId);
            Assert.AreEqual(300, reg.ProposedSections[0].AcademicCredits);
        }

        [TestCleanup]
        public void InstantEnrollmentRegistrationProposedRegistrationTests_Cleanup()
        {
            personId = null;
            demo = null;
            demoNullEmail = null;
            demoEmptyEmail = null;
            acadProgram = null;
            catalog = null;
            sections = null;
        }
    }
}
