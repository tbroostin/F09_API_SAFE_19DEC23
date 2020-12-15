// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class PersonMatchCriteriaInstantEnrollmentTests
    {

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchCriteria_Constructor_NullFirstName()
        {
            var result = new PersonMatchCriteriaInstantEnrollment(null, "Last");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchCriteria_Constructor_NullLastName()
        {
            var result = new PersonMatchCriteriaInstantEnrollment("First", null);
        }

        [TestMethod]
        public void PersonMatchCriteria_Constructor_Valid()
        {
            var result = new PersonMatchCriteriaInstantEnrollment("First", "Last");
            Assert.AreEqual(result.FirstName, "First");
            Assert.AreEqual(result.LastName, "Last");
        }
    }
}
