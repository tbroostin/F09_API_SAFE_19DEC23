using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class PlannedCreditTests
    {
        Course course;
        string sectionId;
        string termCode;
        decimal credits;
        PlannedCredit plannedCourse;

        [TestInitialize]
        public void Initialize()
        {
            course = new TestCourseRepository().Biol100;
            sectionId = "3";
            termCode = "2010/FA";
            credits = 3m;
        }

        [TestCleanup]
        public void Cleanup()
        {
            plannedCourse = null;
            course = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionIfCourseIsNull()
        {
            plannedCourse = new PlannedCredit(null, termCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionIfBothTermAndSectionNull()
        {
            plannedCourse = new PlannedCredit(course, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionIfBothTermAndSectionEmpty()
        {
            plannedCourse = new PlannedCredit(course, "");
        }

        [TestMethod]
        public void Course()
        {
            plannedCourse = new PlannedCredit(course, termCode);
            Assert.AreEqual(course, plannedCourse.Course);
        }

        [TestMethod]
        public void TermId()
        {
            plannedCourse = new PlannedCredit(course, termCode, sectionId);
            Assert.AreEqual(termCode, plannedCourse.TermCode);
        }

        [TestMethod]
        public void TermIdSetToEmptyIfArgumentNull()
        {
            plannedCourse = new PlannedCredit(course, null, sectionId);
            Assert.AreEqual(string.Empty, plannedCourse.TermCode);
        }

        [TestMethod]
        public void SectionId()
        {
            plannedCourse = new PlannedCredit(course, null, sectionId);
            Assert.AreEqual(sectionId, plannedCourse.SectionId);
        }

        [TestMethod]
        public void SectionIdSetToEmptyIfArgumentNull()
        {
            plannedCourse = new PlannedCredit(course, termCode, null);
            Assert.AreEqual(string.Empty, plannedCourse.SectionId);
        }

        [TestMethod]
        public void Credits()
        {
            plannedCourse = new PlannedCredit(course, termCode, null);
            plannedCourse.Credits = credits;
            Assert.AreEqual(credits, plannedCourse.Credits);
        }
    }
}
