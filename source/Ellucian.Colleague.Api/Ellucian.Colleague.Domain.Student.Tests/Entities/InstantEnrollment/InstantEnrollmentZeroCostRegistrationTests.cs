// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentZeroCostRegistrationTests
    {
        private string personId;
        private InstantEnrollmentPersonDemographic personDemographic;
        private InstantEnrollmentPersonDemographic personDemographicNullEmail;
        private InstantEnrollmentPersonDemographic personDemographicEmptyEmail;

        private string acadProgram;
        private string catalog;
        private List<InstantEnrollmentRegistrationBaseSectionToRegister> sections;

        [TestInitialize]
        public void InstantEnrollmentZeroCostRegistration_Initialize()
        {
            personId = "0000001";

            personDemographic = new InstantEnrollmentPersonDemographic("Joe", "TestClass") { EmailAddress = "joe@testclass.com" };
            personDemographicNullEmail = new InstantEnrollmentPersonDemographic("Joe", "TestClass") { EmailAddress = null };
            personDemographicEmptyEmail = new InstantEnrollmentPersonDemographic("Joe", "TestClass") { EmailAddress = String.Empty };

            acadProgram = "POLI.BS";
            catalog = "2018";
            sections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
            {
                new InstantEnrollmentRegistrationBaseSectionToRegister("POLI-100-01", 3){ MarketingSource = "Radio", RegistrationReason = "POLI-100-01 Registration Reason"},
                new InstantEnrollmentRegistrationBaseSectionToRegister("ART-100-01", 1),
                new InstantEnrollmentRegistrationBaseSectionToRegister("ARTH-101-02", null),
            };
        }

        [TestCleanup]
        public void InstantEnrollmentZeroCostRegistration_Cleanup()
        {
            personId = null;
            personDemographic = null;
            personDemographicNullEmail = null;
            personDemographicEmptyEmail = null;
            acadProgram = null;
            catalog = null;
            sections = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentZeroCostRegistration_Constructor_NullPersonIdNullPersonDemographic()
        {
            new InstantEnrollmentZeroCostRegistration(null, null, acadProgram, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentZeroCostRegistration_Constructor_EmptyPersonIdNullPersonDemographic()
        {
            new InstantEnrollmentZeroCostRegistration(string.Empty, null, acadProgram, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentZeroCostRegistration_Constructor_NullPersonIdNullAcadProgram()
        {
            new InstantEnrollmentZeroCostRegistration(null, personDemographic, null, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentZeroCostRegistration_Constructor_EmptyPersonIdEmptyAcadProgram()
        {
            new InstantEnrollmentZeroCostRegistration(string.Empty, personDemographic, string.Empty, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentZeroCostRegistration_Constructor_NullPersonIdNullCatalog()
        {
            new InstantEnrollmentZeroCostRegistration(null, personDemographic, acadProgram, null, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentZeroCostRegistration_Constructor_EmptyPersonIdEmptyCatalog()
        {
            new InstantEnrollmentZeroCostRegistration(string.Empty, personDemographic, acadProgram, string.Empty, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentZeroCostRegistration_Constructor_NullSections()
        {
            new InstantEnrollmentZeroCostRegistration(string.Empty, personDemographic, acadProgram, catalog, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentZeroCostRegistration_Constructor_NullEmail()
        {
            new InstantEnrollmentProposedRegistration(null, personDemographicNullEmail, acadProgram, catalog, sections);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentZeroCostRegistration_Constructor_EmptyEmail()
        {
            new InstantEnrollmentProposedRegistration(null, personDemographicEmptyEmail, acadProgram, catalog, sections);
        }

        [TestMethod]
        public void InstantEnrollmentZeroCostRegistration_Constructor_ValidPersonIdNullDemographicsAcadProgramCatalog()
        {
            var reg = new InstantEnrollmentZeroCostRegistration(personId, null, null, null, sections);
            Assert.AreEqual(personId, reg.PersonId);
            for (var i = 0; i < sections.Count; i++)
            {
                Assert.AreEqual(sections[i].SectionId, reg.ProposedSections[i].SectionId);
                Assert.AreEqual(sections[i].MarketingSource, reg.ProposedSections[i].MarketingSource);
                Assert.AreEqual(sections[i].RegistrationReason, reg.ProposedSections[i].RegistrationReason);
                Assert.AreEqual(sections[i].AcademicCredits, reg.ProposedSections[i].AcademicCredits);
            }
        }

        [TestMethod]
        public void InstantEnrollmentZeroCostRegistration_Constructor_ValidPersonIdNullDemographicsEmptyAcadProgramCatalog()
        {
            var reg = new InstantEnrollmentZeroCostRegistration(personId, null, string.Empty, string.Empty, sections);
            Assert.AreEqual(personId, reg.PersonId);
            for (var i = 0; i < sections.Count; i++)
            {
                Assert.AreEqual(sections[i].SectionId, reg.ProposedSections[i].SectionId);
                Assert.AreEqual(sections[i].MarketingSource, reg.ProposedSections[i].MarketingSource);
                Assert.AreEqual(sections[i].RegistrationReason, reg.ProposedSections[i].RegistrationReason);
                Assert.AreEqual(sections[i].AcademicCredits, reg.ProposedSections[i].AcademicCredits);
            }
        }

        [TestMethod]
        public void InstantEnrollmentZeroCostRegistration_Constructor_ValidPersonId()
        {
            var reg = new InstantEnrollmentZeroCostRegistration(personId, null, acadProgram, catalog, sections);
            Assert.AreEqual(personId, reg.PersonId);
            Assert.AreEqual(acadProgram, reg.AcademicProgram);
            Assert.AreEqual(catalog, reg.Catalog);
            for (var i = 0; i < sections.Count; i++)
            {
                Assert.AreEqual(sections[i].SectionId, reg.ProposedSections[i].SectionId);
                Assert.AreEqual(sections[i].MarketingSource, reg.ProposedSections[i].MarketingSource);
                Assert.AreEqual(sections[i].RegistrationReason, reg.ProposedSections[i].RegistrationReason);
                Assert.AreEqual(sections[i].AcademicCredits, reg.ProposedSections[i].AcademicCredits);
            }
        }


    }
}
