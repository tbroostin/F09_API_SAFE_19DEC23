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
    public class PositionTests
    {
        public string id;
        public string title;
        public string shortTitle;
        public string positionDept;
        public bool isExempt;
        public bool isSalary;
        public string supervisorPositionId;
        public string alternateSupervisorPositionId;
        public DateTime startDate;
        public DateTime? endDate;
        public List<string> PositionPayScheduleIds;

        public Position position;

        [TestClass]
        public class PositionConstructorTests : PositionTests
        {
            public Position createPosition()
            {
                return new Position(id, title, shortTitle, positionDept, startDate, isSalary);
            }

            [TestInitialize]
            public void Initialize()
            {
                id = "1234MUSADJ1234";
                title = "Music Faculty Professor, Tenure";
                shortTitle = "Music Professor";
                positionDept = "MUSC";
                startDate = new DateTime(2010, 1, 1);
                isSalary = false;                
            }

            [TestMethod]
            public void ConstructorSetsPropertiesTest()
            {
                position = createPosition();
                Assert.AreEqual(id, position.Id);
                Assert.AreEqual(title, position.Title);
                Assert.AreEqual(shortTitle, position.ShortTitle);
                Assert.AreEqual(positionDept, position.PositionDept);
                Assert.AreEqual(startDate, position.StartDate);
                Assert.AreEqual(isSalary, position.IsSalary);
            }

            [TestMethod]
            public void PositionPayScheduleIdsInitializedTest()
            {
                position = createPosition();
                Assert.IsNotNull(position.PositionPayScheduleIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TitleRequiredTest()
            {
                title = "";
                createPosition();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShortTitleRequiredTest()
            {
                shortTitle = "";
                createPosition();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PositionDeptRequiredTest()
            {
                positionDept = "";
                createPosition();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = "";
                position = createPosition();
                
            }
        }

        [TestClass]
        public class PositionAttributesTests : PositionTests
        {

            [TestInitialize]
            public void Initialize()
            {
                id = "1234MUSADJ1234";
                title = "Music Faculty Professor, Tenure";
                shortTitle = "Music Professor";
                positionDept = "MUSC";
                startDate = new DateTime(2010, 1, 1);
                isSalary = false;

                position = new Position(id, title, shortTitle, positionDept, startDate, isSalary); 
            }

            [TestMethod]
            public void TitleTest()
            {
                position.Title = "foobar";
                Assert.AreEqual("foobar", position.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TitleRequiredTest()
            {
                position.Title = " ";
            }

            [TestMethod]
            public void ShortTitleTest()
            {
                position.ShortTitle = "foobar";
                Assert.AreEqual("foobar", position.ShortTitle);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShortTitleRequiredTest()
            {
                position.ShortTitle = null;
            }

            [TestMethod]
            public void StartDateTest()
            {
                position.StartDate = DateTime.Today;
                Assert.AreEqual(DateTime.Today, position.StartDate);
            }

            [TestMethod]
            public void StartDateCanEqualEndDateTest()
            {
                position.StartDate = DateTime.Today;
                position.EndDate = DateTime.Today;
                Assert.AreEqual(position.StartDate, position.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void StartDateMustBeBeforeEndDateTest()
            {
                position.EndDate = DateTime.Today;
                position.StartDate = DateTime.Today.AddDays(1);
            }

            [TestMethod]
            public void EndDateTest()
            {
                position.EndDate = DateTime.Today;
                Assert.AreEqual(DateTime.Today, position.EndDate);
            }

            [TestMethod]
            public void EndDateCanEqualStartDateTest()
            {
                position.StartDate = DateTime.Today;
                position.EndDate = DateTime.Today;
                Assert.AreEqual(position.EndDate, position.StartDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EndDateMustBeAfterStartDateTest()
            {
                position.EndDate = position.StartDate.AddDays(-1);
            }

        }

        [TestClass]
        public class PositionEqualsTests : PositionTests
        {
            public Position createPosition()
            {
                return new Position(id, title, shortTitle, positionDept, startDate, isSalary);
            }

            [TestInitialize]
            public void Initialize()
            {
                id = "1234MUSADJ1234";
                title = "Music Faculty Professor, Tenure";
                shortTitle = "Music Professor";
                positionDept = "MUSC";
                startDate = new DateTime(2010, 1, 1);
                isSalary = false;
            }

            [TestMethod]
            public void ObjectsEqualWhenIdsAreEqualTest()
            {
                var pos1 = createPosition();
                var pos2 = createPosition();

                Assert.AreEqual(pos1, pos2);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenIdsAreNotEqualTest()
            {
                var pos1 = createPosition();
                id = "foobar";
                var pos2 = createPosition();

                Assert.AreNotEqual(pos1, pos2);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                var pos1 = createPosition();
                Assert.AreNotEqual(pos1, null);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsDifferentTypeTest()
            {
                var pos1 = createPosition();
                var notPos = new Base.Entities.Bank("011000015", "name", "011000015");
                Assert.AreNotEqual(pos1, notPos);
            }

            [TestMethod]
            public void HashCodeEqualWhenIdIsEqualTest()
            {
                var pos1 = createPosition();
                var pos2 = createPosition();
                Assert.AreEqual(pos1.GetHashCode(), pos2.GetHashCode());
            }

            [TestMethod]
            public void HashCodeNotEqualWhenIdsNotEqualTest()
            {
                var pos1 = createPosition();
                id = "foobar";
                var pos2 = createPosition();
                Assert.AreNotEqual(pos1.GetHashCode(), pos2.GetHashCode());
            }

            [TestMethod]
            public void ToStringTest()
            {
                var pos1 = createPosition();
                Assert.AreEqual(pos1.ShortTitle + "-" + pos1.Id, pos1.ToString());
            }
        }
    }
}
