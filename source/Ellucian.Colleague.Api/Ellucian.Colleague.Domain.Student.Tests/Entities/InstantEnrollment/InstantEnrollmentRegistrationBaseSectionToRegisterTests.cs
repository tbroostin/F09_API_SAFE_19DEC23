// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentRegistrationBaseSectionToRegisterTests
    {
        private string section;
        private decimal? credits;

        [TestInitialize]
        public void InstantEnrollmentRegistrationBaseSectionToRegisterTests_Initialize()
        {
            section = "SECT1";
            credits = 300;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationBaseSectionToRegisterTests_Constructor_NullSection()
        {
            var sect = new InstantEnrollmentRegistrationBaseSectionToRegister(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationBaseSectionToRegisterTests_Constructor_EmptySection()
        {
            var sect = new InstantEnrollmentRegistrationBaseSectionToRegister(String.Empty, null);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationBaseSectionToRegisterTests_Constructor_NullCredits()
        {
            var sect = new InstantEnrollmentRegistrationBaseSectionToRegister(section, null);
            Assert.AreEqual(section, sect.SectionId);
            Assert.IsNull(sect.AcademicCredits);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationBaseSectionToRegisterTests_Constructor_NotNullCredits()
        {
            var sect = new InstantEnrollmentRegistrationBaseSectionToRegister(section, credits);
            Assert.AreEqual(section, sect.SectionId);
            Assert.AreEqual(credits, sect.AcademicCredits);
        }
    }
}
