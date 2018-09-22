/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PersonEmploymentStatusTests
    {
        string id;
        string personId;
        string primaryPositionId;
        string personPositionId;
        DateTime? startDate;
        DateTime? endDate;

        public PersonEmploymentStatus personEmploymentStatus;

        [TestClass]
        public class PersonEmploymentStatusTestsConstructorTests : PersonEmploymentStatusTests
        {

            [TestInitialize]
            public void Initialize()
            {
                id = "001";
                personId = "24601";
                primaryPositionId = "abc";
                personPositionId = "999";
                startDate = new DateTime();
                endDate = null;
            }

            [TestMethod]
            public void ConstructorSetsPropertiesTest()
            {
                personEmploymentStatus = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);
                Assert.AreEqual(id, personEmploymentStatus.Id);
                Assert.AreEqual(personId, personEmploymentStatus.PersonId);
                Assert.AreEqual(primaryPositionId, personEmploymentStatus.PrimaryPositionId);
                Assert.AreEqual(personPositionId, personEmploymentStatus.PersonPositionId);
                Assert.AreEqual(startDate, personEmploymentStatus.StartDate);
                Assert.AreEqual(endDate, personEmploymentStatus.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = "";
                personEmploymentStatus = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdRequiredTest()
            {
                personId = "";
                personEmploymentStatus = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrimaryPositionIdRequiredTest()
            {
                primaryPositionId = "";
                personEmploymentStatus = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonPositionIdRequiredTest()
            {
                personPositionId = "";
                personEmploymentStatus = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);
            }
            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void NullStartDatePassedInTest()
            {
                startDate = null;
                personEmploymentStatus = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);
            }

            [TestMethod]
            public void EqualsOverrideTests()
            {
                personEmploymentStatus = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);
                var personEmploymentStatus2 = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);

                Assert.IsTrue(personEmploymentStatus.Equals(personEmploymentStatus2));
                Assert.IsFalse(personEmploymentStatus.Equals(null));
                Assert.IsFalse(personEmploymentStatus.Equals(new Employee("123", "123")));
            }

            [TestMethod]
            public void GetHashCodeOverrideTest()
            {
                personEmploymentStatus = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);
                Assert.AreEqual(id.GetHashCode(), personEmploymentStatus.GetHashCode());
            }

            [TestMethod]
            public void ToStringOverrideTest()
            {
                personEmploymentStatus = new PersonEmploymentStatus(id, personId, primaryPositionId, personPositionId, startDate, endDate);
                Assert.AreEqual(id, personEmploymentStatus.ToString());
            }
        }
    }
}
