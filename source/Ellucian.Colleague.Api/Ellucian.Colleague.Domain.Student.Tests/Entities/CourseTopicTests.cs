//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
     [TestClass]
    public class CourseTopicTests
    {
        [TestClass]
        public class CourseTopicConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private CourseTopic courseTopics;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                courseTopics = new CourseTopic(guid, code, desc);
            }

            [TestMethod]
            public void CourseTopic_Code()
            {
                Assert.AreEqual(code, courseTopics.Code);
            }

            [TestMethod]
            public void CourseTopic_Description()
            {
                Assert.AreEqual(desc, courseTopics.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTopic_GuidNullException()
            {
                new CourseTopic(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTopic_CodeNullException()
            {
                new CourseTopic(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTopic_DescNullException()
            {
                new CourseTopic(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTopicGuidEmptyException()
            {
                new CourseTopic(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTopicCodeEmptyException()
            {
                new CourseTopic(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTopicDescEmptyException()
            {
                new CourseTopic(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class CourseTopic_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private CourseTopic courseTopics1;
            private CourseTopic courseTopics2;
            private CourseTopic courseTopics3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                courseTopics1 = new CourseTopic(guid, code, desc);
                courseTopics2 = new CourseTopic(guid, code, "Second Year");
                courseTopics3 = new CourseTopic(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CourseTopicSameCodesEqual()
            {
                Assert.IsTrue(courseTopics1.Equals(courseTopics2));
            }

            [TestMethod]
            public void CourseTopicDifferentCodeNotEqual()
            {
                Assert.IsFalse(courseTopics1.Equals(courseTopics3));
            }
        }

        [TestClass]
        public class CourseTopic_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CourseTopic courseTopics1;
            private CourseTopic courseTopics2;
            private CourseTopic courseTopics3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                courseTopics1 = new CourseTopic(guid, code, desc);
                courseTopics2 = new CourseTopic(guid, code, "Second Year");
                courseTopics3 = new CourseTopic(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CourseTopicSameCodeHashEqual()
            {
                Assert.AreEqual(courseTopics1.GetHashCode(), courseTopics2.GetHashCode());
            }

            [TestMethod]
            public void CourseTopicDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(courseTopics1.GetHashCode(), courseTopics3.GetHashCode());
            }
        }
    }
}
