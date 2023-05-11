// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.AnonymousGrading
{
    [TestClass]
    public class SectionPreliminaryAnonymousGradingTests
    {
        string courseSectionId;
        AnonymousGradeError error;
        PreliminaryAnonymousGrade gradeForSection;
        PreliminaryAnonymousGrade gradeForCrosslist;

        [TestInitialize]
        public void SectionPreliminaryAnonymousGradingTests_Initialize()
        {
            courseSectionId = "12345";
            error = new AnonymousGradeError("12345", "23456", "12345", "0001234*2021/FA*UG", "Record locked!");
            gradeForSection = new PreliminaryAnonymousGrade("11223", "1", "12345", "12345", null);
            gradeForCrosslist = new PreliminaryAnonymousGrade("11224", "2", "12346", "12345", DateTime.Today.AddDays(30));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionPreliminaryAnonymousGrading_null_courseSectionId_throws_ArgumentNullException()
        {
            var entity = new SectionPreliminaryAnonymousGrading(null);
        }

        [TestMethod]
        public void SectionPreliminaryAnonymousGrading_valid()
        {
            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            Assert.AreEqual(courseSectionId, entity.SectionId);
            Assert.AreEqual(0, entity.AnonymousGradesForSection.Count);
            Assert.AreEqual(0, entity.AnonymousGradesForCrosslistedSections.Count);
            Assert.AreEqual(0, entity.Errors.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionPreliminaryAnonymousGrading_AddAnonymousGradeForSection_null_grade_throws_ArgumentNullException()
        {
            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            entity.AddAnonymousGradeForSection(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueException))]
        public void SectionPreliminaryAnonymousGrading_AddAnonymousGradeForSection_grade_wrong_SectionId_throws_ColleagueException()
        {
            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            entity.AddAnonymousGradeForSection(gradeForCrosslist);
        }

        [TestMethod]
        public void SectionPreliminaryAnonymousGrading_AddAnonymousGradeForSection_valid()
        {
            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            entity.AddAnonymousGradeForSection(gradeForSection);
            Assert.AreEqual(1, entity.AnonymousGradesForSection.Count);
            Assert.AreEqual(0, entity.AnonymousGradesForCrosslistedSections.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionPreliminaryAnonymousGrading_AddAnonymousGradeForCrosslistedSection_null_grade_throws_ArgumentNullException()
        {
            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            entity.AddAnonymousGradeForCrosslistedSection(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueException))]
        public void SectionPreliminaryAnonymousGrading_AddAnonymousGradeForCrosslistedSection_grade_wrong_SectionId_throws_ColleagueException()
        {
            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            entity.AddAnonymousGradeForCrosslistedSection(gradeForSection);
        }

        [TestMethod]
        public void SectionPreliminaryAnonymousGrading_AddAnonymousGradeForCrosslistedSection_valid()
        {
            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            entity.AddAnonymousGradeForCrosslistedSection(gradeForCrosslist);
            Assert.AreEqual(0, entity.AnonymousGradesForSection.Count);
            Assert.AreEqual(1, entity.AnonymousGradesForCrosslistedSections.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionPreliminaryAnonymousGrading_AddError_null_error_throws_ArgumentNullException()
        {
            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            entity.AddError(null);
        }

        [TestMethod]
        public void SectionPreliminaryAnonymousGrading_AddError_valid()
        {
            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            entity.AddError(error);
            Assert.AreEqual(1, entity.Errors.Count);
        }
        [TestMethod]
        public void SectionPreliminaryAnonymousGrading_AddAnonymousGradeForSectionWithMidtermGradeId_valid()
        {
            gradeForSection = new PreliminaryAnonymousGrade("11223","23456", "1", "12345", "12345", null);

            var entity = new SectionPreliminaryAnonymousGrading(courseSectionId);
            entity.AddAnonymousGradeForSection(gradeForSection);
            Assert.AreEqual(1, entity.AnonymousGradesForSection.Count);
            Assert.AreEqual("11223",entity.AnonymousGradesForSection[0].AnonymousGradingId);
            Assert.AreEqual("23456",entity.AnonymousGradesForSection[0].AnonymousMidTermGradingId);
        }
    }
}
