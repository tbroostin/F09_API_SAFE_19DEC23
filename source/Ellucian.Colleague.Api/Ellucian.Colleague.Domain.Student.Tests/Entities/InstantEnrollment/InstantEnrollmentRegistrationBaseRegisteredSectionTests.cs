// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentRegistrationBaseRegisteredSectionTests
    {
        private string section;
        private decimal? cost;

        [TestInitialize]
        public void InstantEnrollmentRegistrationBaseRegisteredSectionTests_Initialize()
        {
            section = "SECT1";
            cost = 100;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationBaseRegisteredSectionTests_Constructor_NullSection()
        {
            var sect = new InstantEnrollmentRegistrationBaseRegisteredSection(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationBaseRegisteredSectionTests_Constructor_EmptySection()
        {
            var sect = new InstantEnrollmentRegistrationBaseRegisteredSection(String.Empty, null);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationBaseRegisteredSectionTests_Constructor_NullCost()
        {
            var sect = new InstantEnrollmentRegistrationBaseRegisteredSection(section, null);
            Assert.AreEqual(section, sect.SectionId);
            Assert.AreEqual(0, sect.SectionCost);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationBaseRegisteredSectionTests_Constructor_NonNullCost()
        {
            var sect = new InstantEnrollmentRegistrationBaseRegisteredSection(section, cost);
            Assert.AreEqual(section, sect.SectionId);
            Assert.AreEqual(cost, sect.SectionCost);
        }
    }
}
