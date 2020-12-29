using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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

            [TestInitialize]
            public void Initialize()
            {
                description = "Freshman Fall Block";
                courseIds = new List<string>() { "1", "2", "3" };
                block = new CourseBlocks(description, courseIds);
            }

            [TestMethod]
            public void Description()
            {
                Assert.AreEqual(description, block.Description);
            }

            [TestMethod]
            public void CourseIds()
            {
                Assert.AreEqual(courseIds.ElementAt(0), block.CourseIds.ElementAt(0));
                Assert.AreEqual(courseIds.ElementAt(1), block.CourseIds.ElementAt(1));
                Assert.AreEqual(courseIds.ElementAt(2), block.CourseIds.ElementAt(2));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfDescriptionIsNull()
            {
                var block1 = new CourseBlocks(null, courseIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfDescriptionIsBlank()
            {
                var block1 = new CourseBlocks("", courseIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfCourseIdsNull()
            {
                courseIds = null;
                var block1 = new CourseBlocks(description, courseIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfCourseIdsEmpty()
            {
                courseIds = new List<string>();
                var block1 = new CourseBlocks(description, courseIds);
            }
        }
    }
}
