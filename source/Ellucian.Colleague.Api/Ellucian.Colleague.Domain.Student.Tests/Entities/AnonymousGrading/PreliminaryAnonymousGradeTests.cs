// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.AnonymousGrading
{
    [TestClass]
    public class PreliminaryAnonymousGradeTests
    {
        string anonymousGradingId;
        string finalGradeId;
        string courseSectionId;
        string studentCourseSectionId;
        DateTime? finalGradeExpirationDate;

        [TestInitialize]
        public void PreliminaryAnonymousGradeTests_Initialize()
        {
            anonymousGradingId = "12345";
            finalGradeId = "1";
            courseSectionId = "123";
            studentCourseSectionId = "24680";
            finalGradeExpirationDate = DateTime.Today.AddDays(30);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PreliminaryAnonymousGrade_null_anonymousGradingId_throws_ArgumentNullException()
        {
            var entity = new PreliminaryAnonymousGrade(null, finalGradeId, courseSectionId, studentCourseSectionId, finalGradeExpirationDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PreliminaryAnonymousGrade_null_courseSectionId_throws_ArgumentNullException()
        {
            var entity = new PreliminaryAnonymousGrade(anonymousGradingId, finalGradeId, null, studentCourseSectionId, finalGradeExpirationDate);
        }

        [TestMethod]
        public void PreliminaryAnonymousGrade_valid()
        {
            var entity = new PreliminaryAnonymousGrade(anonymousGradingId, finalGradeId, courseSectionId, studentCourseSectionId, finalGradeExpirationDate);
            Assert.AreEqual(anonymousGradingId, entity.AnonymousGradingId);
            Assert.AreEqual(finalGradeId, entity.FinalGradeId);
            Assert.AreEqual(courseSectionId, entity.CourseSectionId);
            Assert.AreEqual(studentCourseSectionId, entity.StudentCourseSectionId);
            Assert.AreEqual(finalGradeExpirationDate, entity.FinalGradeExpirationDate);
        }
    }
}
