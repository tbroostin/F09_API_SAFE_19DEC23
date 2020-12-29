/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PersonPositionTests
    {
        public string id;
        public string personId;
        public string positionId;
        public string supervisorId;
        public string alternateSupervisorId;
        public DateTime startDate;
        public DateTime? endDate;
        public Decimal? fullTimeEquivalent;

        public PersonPosition personPosition;

        [TestClass]
        public class PersonPositionConstructorTests : PersonPositionTests
        {
            public PersonPosition createPersonPosition()
            {
                return new PersonPosition(id, personId, positionId, startDate, fullTimeEquivalent);
            }

            [TestInitialize]
            public void Initialize()
            {
                id = "123";
                personId = "0003914";
                positionId = "1234MUSADJ1234";
                startDate = new DateTime(2016, 3, 17);
                fullTimeEquivalent = 0.5m;
            }

            [TestMethod]
            public void ConstructorSetsPropertiesTest()
            {
                personPosition = createPersonPosition();
                Assert.AreEqual(id, personPosition.Id);
                Assert.AreEqual(personId, personPosition.PersonId);
                Assert.AreEqual(positionId, personPosition.PositionId);
                Assert.AreEqual(startDate, personPosition.StartDate);
                Assert.AreEqual(fullTimeEquivalent, personPosition.FullTimeEquivalent);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = "";
                createPersonPosition();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdRequiredTest()
            {
                personId = "";
                createPersonPosition();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PositionIdRequiredTest()
            {
                positionId = "";
                createPersonPosition();
            }

        }

        [TestClass]
        public class PersonPositionAttributesTests : PersonPositionTests
        {
            [TestInitialize]
            public void Initialize()
            {
                id = "123";
                personId = "0003914";
                positionId = "1234MUSADJ1234";
                startDate = new DateTime(2016, 3, 17);
                endDate = null;
                supervisorId = "0003915";
                alternateSupervisorId = "0003916";
                fullTimeEquivalent = 0.5m;

                personPosition = new PersonPosition(id, personId, positionId, startDate, fullTimeEquivalent);
            }

            [TestMethod]
            public void SupervisorIdTest()
            {
                personPosition.SupervisorId = supervisorId;
                Assert.AreEqual(supervisorId, personPosition.SupervisorId);
            }

            [TestMethod]
            public void AlternateSupervisorIdTest()
            {
                personPosition.AlternateSupervisorId = alternateSupervisorId;
                Assert.AreEqual(alternateSupervisorId, personPosition.AlternateSupervisorId);
            }

            [TestMethod]
            public void StartDateTest()
            {
                personPosition.StartDate = DateTime.Today;
                Assert.AreEqual(DateTime.Today, personPosition.StartDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void StartDateCannotBeAfterEndDateTest()
            {
                personPosition.EndDate = new DateTime(2016, 4, 17);
                personPosition.StartDate = new DateTime(2016, 5, 17);
            }

            [TestMethod]
            public void StartDateCanEqualEndDateTest()
            {
                personPosition.EndDate = new DateTime(2016, 4, 17);
                personPosition.StartDate = personPosition.EndDate.Value;

                Assert.AreEqual(personPosition.StartDate, personPosition.EndDate.Value);
            }

            [TestMethod]
            public void EndDateTest()
            {
                personPosition.EndDate = new DateTime(2016, 4, 17);
                Assert.AreEqual(new DateTime(2016, 4, 17), personPosition.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EndDateCannotBeBeforeStartDateTest()
            {
                personPosition.EndDate = personPosition.StartDate.AddDays(-1);
            }

            [TestMethod]
            public void EndDateCanEqualStartDateTest()
            {
                personPosition.EndDate = personPosition.StartDate;

                Assert.AreEqual(personPosition.EndDate, personPosition.StartDate);
            }

            [TestMethod]
            public void FTETest()
            {
                personPosition.FullTimeEquivalent = 0.5m;

                Assert.AreEqual(0.5m, personPosition.FullTimeEquivalent);
            }
        }

        [TestClass]
        public class EqualsTests : PersonPositionTests
        {
            public PersonPosition createPersonPosition()
            {
                return new PersonPosition(id, personId, positionId, startDate, fullTimeEquivalent);
            }

            [TestInitialize]
            public void Initialize()
            {
                id = "123";
                personId = "0003914";
                positionId = "1234MUSADJ1234";
                startDate = new DateTime(2016, 3, 17);
                fullTimeEquivalent = 0.5m;
            }

            [TestMethod]
            public void ObjectsEqualWhenIdsAreEqualTest()
            {
                var pp1 = createPersonPosition();
                var pp2 = createPersonPosition();

                Assert.IsTrue(pp1.Equals(pp2));
                Assert.IsTrue(pp2.Equals(pp1));
            }

            [TestMethod]
            public void ObjectsNotEqualWhenIdsNotEqualTest()
            {
                var pp1 = createPersonPosition();
                id = "451";
                var pp2 = createPersonPosition();

                Assert.IsFalse(pp1.Equals(pp2));
                Assert.IsFalse(pp2.Equals(pp1));
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                var pp1 = createPersonPosition();

                Assert.IsFalse(pp1.Equals(null));
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsDifferentTypeTest()
            {
                var pp1 = createPersonPosition();
                var position = new Position("foo", "foobar", "bar", "foodept", DateTime.Today, false);

                Assert.IsFalse(pp1.Equals(position));
            }

            [TestMethod]
            public void HashCodeEqualWhenIdIsEqualTest()
            {
                var pp1 = createPersonPosition();
                var pp2 = createPersonPosition();

                Assert.AreEqual(pp1.GetHashCode(), pp2.GetHashCode());
            }

            [TestMethod]
            public void HashCodeNotEqualWhenPositionIdIsNotEqualTest()
            {
                var pp1 = createPersonPosition();
                positionId = "ZAGME202500AD";
                var pp2 = createPersonPosition();
                Assert.AreNotEqual(pp1.GetHashCode(), pp2.GetHashCode());
            }

            [TestMethod]
            public void HashCodeNotEqualWhenPersonIdIsNotEqualTest()
            {
                var pp1 = createPersonPosition();
                personId = "0012882";
                var pp2 = createPersonPosition();
                Assert.AreNotEqual(pp1.GetHashCode(), pp2.GetHashCode());
            }

            [TestMethod]
            public void ToStringTest()
            {
                var pos1 = createPersonPosition();
                Assert.AreEqual(id, pos1.ToString());
            }
        }
    }
}
