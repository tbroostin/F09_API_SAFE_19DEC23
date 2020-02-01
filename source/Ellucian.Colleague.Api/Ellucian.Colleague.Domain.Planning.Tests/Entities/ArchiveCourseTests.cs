using System;
using Ellucian.Colleague.Domain.Planning.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    public class ArchiveCourseTests
    {
        [TestMethod]
        public void ReturnsValidArchiveCourse()
        {
            ArchivedCourse ac = new ArchivedCourse("0001");
            Assert.AreEqual("0001", ac.CourseId);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchiveCourseThrowsWithNullCourseId()
        {
            ArchivedCourse ac = new ArchivedCourse(null);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchiveCourseThrowsWithEmptyCourseId()
        {
            ArchivedCourse ac = new ArchivedCourse("");
        }
    }
}
