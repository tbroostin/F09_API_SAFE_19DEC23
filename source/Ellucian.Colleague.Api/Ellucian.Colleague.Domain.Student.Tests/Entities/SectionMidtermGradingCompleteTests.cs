// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionMidtermGradingCompleteTests
    {
        public string bookId;
        public string requiredStatusCode;
        public string optionalStatusCode;

        [TestInitialize]
        public void Initialize()
        {
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void SectionMidtermGradingComplete_NullSectionId()
        {
            var entity = new SectionMidtermGradingComplete(null);
        }

        [TestMethod]
        public void SectionMidtermGradingComplete_Constructor()
        {
            // Only a section ID is needed to construct the entity.
            string sectionId = "123";
            var entity = new SectionMidtermGradingComplete(sectionId);
            Assert.AreEqual(entity.SectionId, sectionId);
            Assert.AreEqual(entity.MidtermGrading1Complete.Count, 0);
            Assert.AreEqual(entity.MidtermGrading2Complete.Count, 0);
            Assert.AreEqual(entity.MidtermGrading3Complete.Count, 0);
            Assert.AreEqual(entity.MidtermGrading4Complete.Count, 0);
            Assert.AreEqual(entity.MidtermGrading5Complete.Count, 0);
            Assert.AreEqual(entity.MidtermGrading6Complete.Count, 0);
        }

        [TestMethod]
        public void SectionMidtermGradingComplete_GetAndSetGradingCompleteLists()
        {
            var entity = new SectionMidtermGradingComplete("123");
            entity.AddMidtermGrading1Complete("Oper1", new DateTimeOffset(2010, 1, 1, 1, 1, 1, new TimeSpan()));
            entity.AddMidtermGrading1Complete("Oper2", new DateTimeOffset(2010, 1, 2, 1, 1, 2, new TimeSpan()));
            entity.AddMidtermGrading2Complete("Oper3", new DateTimeOffset(2010, 1, 3, 1, 1, 3, new TimeSpan()));
            entity.AddMidtermGrading2Complete("Oper4", new DateTimeOffset(2010, 1, 4, 1, 1, 4, new TimeSpan()));
            entity.AddMidtermGrading3Complete("Oper5", new DateTimeOffset(2010, 1, 5, 1, 1, 5, new TimeSpan()));
            entity.AddMidtermGrading3Complete("Oper6", new DateTimeOffset(2010, 1, 6, 1, 1, 6, new TimeSpan()));
            entity.AddMidtermGrading4Complete("Oper7", new DateTimeOffset(2010, 1, 7, 1, 1, 7, new TimeSpan()));
            entity.AddMidtermGrading4Complete("Oper8", new DateTimeOffset(2010, 1, 8, 1, 1, 8, new TimeSpan()));
            entity.AddMidtermGrading5Complete("Oper9", new DateTimeOffset(2010, 1, 9, 1, 1, 9, new TimeSpan()));
            entity.AddMidtermGrading5Complete("Oper10", new DateTimeOffset(2010, 1, 10, 1, 1, 10, new TimeSpan()));
            entity.AddMidtermGrading6Complete("Oper11", new DateTimeOffset(2010, 1, 11, 1, 1, 11, new TimeSpan()));
            entity.AddMidtermGrading6Complete("Oper12", new DateTimeOffset(2010, 1, 12, 1, 1, 12, new TimeSpan()));

            Assert.AreEqual(entity.MidtermGrading1Complete.Count, 2);
            Assert.AreEqual(entity.MidtermGrading2Complete.Count, 2);
            Assert.AreEqual(entity.MidtermGrading3Complete.Count, 2);
            Assert.AreEqual(entity.MidtermGrading4Complete.Count, 2);
            Assert.AreEqual(entity.MidtermGrading5Complete.Count, 2);
            Assert.AreEqual(entity.MidtermGrading6Complete.Count, 2);

            Assert.AreEqual(entity.MidtermGrading1Complete[0].CompleteOperator, "Oper1");
            Assert.AreEqual(entity.MidtermGrading1Complete[0].DateAndTime, new DateTimeOffset(2010, 1, 1, 1, 1, 1, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading1Complete[1].CompleteOperator, "Oper2");
            Assert.AreEqual(entity.MidtermGrading1Complete[1].DateAndTime, new DateTimeOffset(2010, 1, 2, 1, 1, 2, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading2Complete[0].CompleteOperator, "Oper3");
            Assert.AreEqual(entity.MidtermGrading2Complete[0].DateAndTime, new DateTimeOffset(2010, 1, 3, 1, 1, 3, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading2Complete[1].CompleteOperator, "Oper4");
            Assert.AreEqual(entity.MidtermGrading2Complete[1].DateAndTime, new DateTimeOffset(2010, 1, 4, 1, 1, 4, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading3Complete[0].CompleteOperator, "Oper5");
            Assert.AreEqual(entity.MidtermGrading3Complete[0].DateAndTime, new DateTimeOffset(2010, 1, 5, 1, 1, 5, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading3Complete[1].CompleteOperator, "Oper6");
            Assert.AreEqual(entity.MidtermGrading3Complete[1].DateAndTime, new DateTimeOffset(2010, 1, 6, 1, 1, 6, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading4Complete[0].CompleteOperator, "Oper7");
            Assert.AreEqual(entity.MidtermGrading4Complete[0].DateAndTime, new DateTimeOffset(2010, 1, 7, 1, 1, 7, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading4Complete[1].CompleteOperator, "Oper8");
            Assert.AreEqual(entity.MidtermGrading4Complete[1].DateAndTime, new DateTimeOffset(2010, 1, 8, 1, 1, 8, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading5Complete[0].CompleteOperator, "Oper9");
            Assert.AreEqual(entity.MidtermGrading5Complete[0].DateAndTime, new DateTimeOffset(2010, 1, 9, 1, 1, 9, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading5Complete[1].CompleteOperator, "Oper10");
            Assert.AreEqual(entity.MidtermGrading5Complete[1].DateAndTime, new DateTimeOffset(2010, 1, 10, 1, 1, 10, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading6Complete[0].CompleteOperator, "Oper11");
            Assert.AreEqual(entity.MidtermGrading6Complete[0].DateAndTime, new DateTimeOffset(2010, 1, 11, 1, 1, 11, new TimeSpan()));
            Assert.AreEqual(entity.MidtermGrading6Complete[1].CompleteOperator, "Oper12");
            Assert.AreEqual(entity.MidtermGrading6Complete[1].DateAndTime, new DateTimeOffset(2010, 1, 12, 1, 1, 12, new TimeSpan()));

        }


    }
}
