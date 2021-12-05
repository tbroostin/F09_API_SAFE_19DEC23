// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.AnonymousGrading
{
    [TestClass]
    public class AnonymousGradeErrorTests
    {
        string scsId; 
        string stcId; 
        string psgwId; 
        string sttrId; 
        string message;

        [TestInitialize]
        public void AnonymousGradeErrorTests_Initialize()
        {
            scsId = "1";
            stcId = "2";
            psgwId = "1";
            sttrId = "0001234*2021/FA*UG";
            message = "No random ID for STUDENT.COURSE.SEC 1";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AnonymousGradeError_null_message_throws_ArgumentNullException()
        {
            var entity = new AnonymousGradeError(scsId, stcId, psgwId, sttrId, null);
        }

        [TestMethod]
        public void AnonymousGradeError_valid()
        {
            var entity = new AnonymousGradeError(scsId, stcId, psgwId, sttrId, message);
            Assert.AreEqual(scsId, entity.StudentCourseSecId);
            Assert.AreEqual(stcId, entity.StudentAcadCredId);
            Assert.AreEqual(psgwId, entity.PrelimStuGrdWorkId);
            Assert.AreEqual(sttrId, entity.StudentTermsId);
            Assert.AreEqual(message, entity.Message);
        }
    }
}
