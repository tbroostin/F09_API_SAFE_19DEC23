// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.AnonymousGrading
{
    [TestClass]
    public class PreliminaryAnonymousGradeUpdateResultTests
    {
        string studentCourseSectionId;
        PreliminaryAnonymousGradeUpdateStatus status;
        string message;

        [TestInitialize]
        public void PreliminaryAnonymousGradeUpdateResultTests_Initialize()
        {
            studentCourseSectionId = "12345";
            status = PreliminaryAnonymousGradeUpdateStatus.Success;
            message = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PreliminaryAnonymousGradeUpdateResult_null_studentCourseSectionId_throws_ArgumentNullException()
        {
            var entity = new PreliminaryAnonymousGradeUpdateResult(null, status, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueException))]
        public void PreliminaryAnonymousGradeUpdateResult_null_message_for_Failure_throws_ColleagueException()
        {
            var entity = new PreliminaryAnonymousGradeUpdateResult(studentCourseSectionId, PreliminaryAnonymousGradeUpdateStatus.Failure, message);
        }

        [TestMethod]
        public void PreliminaryAnonymousGradeUpdateResult_valid_Success()
        {
            var entity = new PreliminaryAnonymousGradeUpdateResult(studentCourseSectionId, status, message);
            Assert.AreEqual(studentCourseSectionId, entity.StudentCourseSectionId);
            Assert.AreEqual(status, entity.Status);
            Assert.AreEqual(message, entity.Message);
        }

        [TestMethod]
        public void PreliminaryAnonymousGradeUpdateResult_valid_Failure()
        {
            status = PreliminaryAnonymousGradeUpdateStatus.Failure;
            message = "Update could not be processed at this time.";
            var entity = new PreliminaryAnonymousGradeUpdateResult(studentCourseSectionId, status, message);
            Assert.AreEqual(studentCourseSectionId, entity.StudentCourseSectionId);
            Assert.AreEqual(status, entity.Status);
            Assert.AreEqual(message, entity.Message);
        }
    }
}
