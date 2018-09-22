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
    public class PersonPositionWageTests
    {
        public string id;
        public string personId;
        public string positionId;
        public string personPositionId;
        public string positionPayDefaultId;
        public string payClassId;
        public string payCycleId;
        public string regularWorkEarningsTypeId;
        public DateTime startDate;
        public DateTime? endDate;
        public bool isPaySuspended;
        public List<PositionFundingSource> fundingSources;
        public string earningsTypeGroupId;
        public List<EarningsTypeGroup> earnTypeGrouping;


        public PersonPositionWage personPositionWage;

        public void PersonPositionWageTestsInitialize()
        {
            id = "1234";
            personId = "0003914";
            positionId = "bar";
            personPositionId = "12345";
            positionPayDefaultId = "5";
            payClassId = "BM";
            payCycleId = "MC";
            regularWorkEarningsTypeId = "REG";
            startDate = new DateTime(2010, 1, 1);
            endDate = new DateTime(2050, 2, 1);
            isPaySuspended = false;
            fundingSources = new List<PositionFundingSource>() {
                new PositionFundingSource("f", 1) {ProjectId = "12345"}
            };
            earningsTypeGroupId = "ADMIN";
            //earnTypeGrouping = new List<EarningsTypeGroup>()
            //{
            //    new EarningsTypeGroup("ADMIN", "PER", "Personal Pay Leave")
            //};
        }

        [TestClass]
        public class PersonPositionWageConstructorTests : PersonPositionWageTests
        {
            public new PersonPositionWage personPositionWage
            {
                get
                {
                    return new PersonPositionWage(id, personId, positionId, personPositionId, positionPayDefaultId, payClassId, payCycleId, regularWorkEarningsTypeId, startDate, earningsTypeGroupId);
                }
            }
            [TestInitialize]
            public void Initialize()
            {
                PersonPositionWageTestsInitialize();
            }

            [TestMethod]
            public void IdTest()
            {
                Assert.AreEqual(id, personPositionWage.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = "";
                var error = personPositionWage;
            }

            [TestMethod]
            public void PersonIdTest()
            {
                Assert.AreEqual(personId, personPositionWage.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdRequiredTest()
            {
                personId = "";
                var error = personPositionWage;
            }

            [TestMethod]
            public void PositionIdTest()
            {
                Assert.AreEqual(positionId, personPositionWage.PositionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PositionIdRequiredTest()
            {
                positionId = "";
                var error = personPositionWage;
            }

            [TestMethod]
            public void PersonPositionIdTest()
            {
                Assert.AreEqual(personPositionId, personPositionWage.PersonPositionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonPositionIdRequiredTest()
            {
                personPositionId = "";
                var error = personPositionWage;
            }

            [TestMethod]
            public void PositionPayDefaultsIdTest()
            {
                Assert.AreEqual(positionPayDefaultId, personPositionWage.PositionPayDefaultId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PositionPayDefaultIdRequiredTest()
            {
                positionPayDefaultId = "";
                var error = personPositionWage;
            }

            [TestMethod]
            public void PayClassIdTest()
            {
                Assert.AreEqual(payClassId, personPositionWage.PayClassId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayClassIdRequiredTest()
            {
                payClassId = "";
                var error = personPositionWage;
            }

            [TestMethod]
            public void PayCycleIdTest()
            {
                Assert.AreEqual(payCycleId, personPositionWage.PayCycleId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayCycleIdRequiredTest()
            {
                payCycleId = "";
                var error = personPositionWage;
            }

            [TestMethod]
            public void RegularWorkEarningsTypeIdTest()
            {
                Assert.AreEqual(regularWorkEarningsTypeId, personPositionWage.RegularWorkEarningsTypeId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegularWorkEarningsTypeIdRequiredTest()
            {
                regularWorkEarningsTypeId = "";
                var error = personPositionWage;
            }

            [TestMethod]
            public void StartDateTest()
            {
                Assert.AreEqual(startDate, personPositionWage.StartDate);
            }

            [TestMethod]
            public void FundingSourcesIsInitializedTest()
            {
                Assert.IsNotNull(personPositionWage.FundingSources);
            }

            [TestMethod]
            public void EarningsTypeGroupIDTest()
            {
                Assert.AreEqual(earningsTypeGroupId, personPositionWage.EarningsTypeGroupId);
            }

            //[TestMethod]
            //public void EarnTypeGroupingTest()
            //{
            //    Assert.AreEqual(earnTypeGrouping, personPositionWage.EarningsTypeGroupEntries);
            //}
        }

        [TestClass]
        public class AttributesTests : PersonPositionWageTests
        {
            [TestInitialize]
            public void Initialize()
            {
                PersonPositionWageTestsInitialize();
                personPositionWage = new PersonPositionWage(id, personId, positionId, personPositionId, positionPayDefaultId, payClassId, payCycleId, regularWorkEarningsTypeId, startDate, earningsTypeGroupId);
            }

            [TestMethod]
            public void EndDateTest()
            {
                personPositionWage.EndDate = endDate;
                Assert.AreEqual(endDate, personPositionWage.EndDate.Value);
            }

            [TestMethod]
            public void EndDateSameAsStartDateTest()
            {
                personPositionWage.EndDate = personPositionWage.StartDate;
                Assert.AreEqual(personPositionWage.StartDate, personPositionWage.EndDate);
            }

            [TestMethod]
            public void EndDateCanBeSetToNullTest()
            {
                personPositionWage.EndDate = endDate;
                personPositionWage.EndDate = null;

                Assert.IsNull(personPositionWage.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EndDateMustBeAfterStartDateTest()
            {
                personPositionWage.EndDate = personPositionWage.StartDate.AddDays(-1);
            }

            [TestMethod]
            public void StartDateCanBeModifiedTest()
            {
                personPositionWage.EndDate = null;
                personPositionWage.StartDate = new DateTime(2050, 1, 1);
            }

            [TestMethod]
            public void StartDateCanBeSameAsEndDateTest()
            {
                personPositionWage.EndDate = endDate;
                personPositionWage.StartDate = personPositionWage.EndDate.Value;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void StartDateCannotBeAfterEndDateTest()
            {
                personPositionWage.EndDate = endDate;
                personPositionWage.StartDate = personPositionWage.EndDate.Value.AddDays(1);
            }
        }

        [TestClass]
        public class EqualsTests : PersonPositionWageTests
        {
            public PersonPositionWage buildPersonPositionWage()
            {
                return new PersonPositionWage(id, personId, positionId, personPositionId, positionPayDefaultId, payClassId, payCycleId, regularWorkEarningsTypeId, startDate, earningsTypeGroupId);
            }

            [TestInitialize]
            public void Initialize()
            {
                PersonPositionWageTestsInitialize();
            }

            [TestMethod]
            public void ObjectsEqualWhenIdsEqualTest()
            {
                var ppw1 = buildPersonPositionWage();
                var ppw2 = buildPersonPositionWage();
                Assert.IsTrue(ppw1.Equals(ppw2));
                Assert.IsTrue(ppw2.Equals(ppw1));
            }

            [TestMethod]
            public void HashCodesEqualWhenIdsEqualTest()
            {
                var ppw1 = buildPersonPositionWage();
                var ppw2 = buildPersonPositionWage();
                Assert.AreEqual(ppw1.GetHashCode(), ppw2.GetHashCode());
            }

            [TestMethod]
            public void ObjectsNotEqualWhenIdsNotEqualTest()
            {
                var ppw1 = buildPersonPositionWage();
                id = "foobar";
                var ppw2 = buildPersonPositionWage();
                Assert.IsFalse(ppw1.Equals(ppw2));
                Assert.IsFalse(ppw2.Equals(ppw1));
            }

            [TestMethod]
            public void HashCodesNotEqualWhenIdsNotEqualTest()
            {
                var ppw1 = buildPersonPositionWage();
                id = "foobar";
                var ppw2 = buildPersonPositionWage();
                Assert.AreNotEqual(ppw1.GetHashCode(), ppw2.GetHashCode());
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                Assert.IsFalse(buildPersonPositionWage().Equals(null));
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsDifferentTypeTest()
            {
                Assert.IsFalse(buildPersonPositionWage().Equals(new PositionFundingSource("foo", 1)));
            }
        }
    }
}
