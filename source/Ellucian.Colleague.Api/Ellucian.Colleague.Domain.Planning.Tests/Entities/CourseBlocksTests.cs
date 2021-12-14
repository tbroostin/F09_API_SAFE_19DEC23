using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Planning.Entities;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    public class CourseBlocksTests
    {
        [TestClass]
        public class CourseBlockConstructor
        {
            CourseBlocks block;
            string description;
            List<string> courseIds;
            List<string> coursePlaceholderIds;

            [TestInitialize]
            public void Initialize()
            {
                description = "Freshman Fall Block";
                courseIds = new List<string>() { "1", "2", "3" };
                coursePlaceholderIds = new List<string>() { "a", "b", "c" };
                block = new CourseBlocks(description: description, courseIds: courseIds, coursePlaceholderIds: coursePlaceholderIds);
            }

            [TestMethod]
            public void Description()
            {
                Assert.AreEqual(description, block.Description);
            }

            [TestMethod]
            public void CourseIds()
            {
                for (var i = 0; i < courseIds.Count; i++)
                {
                    Assert.AreEqual(courseIds[i], block.CourseIds[i]);
                }
            }

            [TestMethod]
            public void CoursePlaceholderIds()
            {
                for (var i = 0; i < coursePlaceholderIds.Count; i++)
                {
                    Assert.AreEqual(coursePlaceholderIds[i], block.CoursePlaceholderIds[i]);
                }
            }

            [TestMethod]
            public void NoExceptionWhenCourseIdsNullAndCoursePlaceholderIdsNotNull()
            {
                courseIds = null;
                var block1 = new CourseBlocks(description, courseIds, coursePlaceholderIds);
            }

            [TestMethod]
            public void NoExceptionWhenCourseIdsNotNullAndCoursePlaceholderIdsNull()
            {
                coursePlaceholderIds = null;
                var block1 = new CourseBlocks(description, courseIds, coursePlaceholderIds);
            }

            [TestMethod]
            public void NoExceptionWhenCourseIdsEmptyAndCoursePlaceholderIdsNotNull()
            {
                courseIds = new List<string>();
                var block1 = new CourseBlocks(description, courseIds, coursePlaceholderIds);
            }

            [TestMethod]
            public void NoExceptionWhenCourseIdsNotNullAndCoursePlaceholderIdsEmpty()
            {
                coursePlaceholderIds = new List<string>();
                var block1 = new CourseBlocks(description, courseIds, coursePlaceholderIds);
            }


            [TestMethod]
            public void NoExceptionWhenCourseIdsInvalidValuesAndCoursePlaceholderIdsValid()
            {
                courseIds = new List<string>() { "", null, "   " };
                var block1 = new CourseBlocks(description, courseIds, coursePlaceholderIds);
            }

            [TestMethod]
            public void NoExceptionWhenCourseIdsNotValidAndCoursePlaceholderIdsInvalidValues()
            {
                coursePlaceholderIds = new List<string>() { "", null, "   " };
                var block1 = new CourseBlocks(description, courseIds, coursePlaceholderIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfDescriptionIsNull()
            {
                var block1 = new CourseBlocks(null, courseIds, coursePlaceholderIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfDescriptionIsBlank()
            {
                var block1 = new CourseBlocks("", courseIds, coursePlaceholderIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfCourseIdsNullAndCoursePlaceholderIdsNull()
            {
                courseIds = null;
                coursePlaceholderIds = null;
                var block1 = new CourseBlocks(description, courseIds, coursePlaceholderIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfCourseIdsEmptyAndCoursePlaceholderIdsEmpty()
            {
                courseIds = new List<string>();
                coursePlaceholderIds = new List<string>();
                var block1 = new CourseBlocks(description, courseIds, coursePlaceholderIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfCourseIdsBlankValuesAndCoursePlaceholderIdsBlankValues()
            {
                courseIds = new List<string>() { "", null, "   " };
                coursePlaceholderIds = new List<string>() { "", null, "   " };
                var block1 = new CourseBlocks(description, courseIds, coursePlaceholderIds);
            }
        }
    }
}
