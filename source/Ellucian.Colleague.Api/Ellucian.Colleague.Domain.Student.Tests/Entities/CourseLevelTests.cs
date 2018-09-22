using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CourseLevelTests
    {
        [TestClass]
        public class CourseLevelConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private CourseLevel courseLevel;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "100";
                desc = "First Year";
                courseLevel = new CourseLevel(guid, code, desc);
            }

            [TestMethod]
            public void CourseLevel_Code()
            {
                Assert.AreEqual(code, courseLevel.Code);
            }

            [TestMethod]
            public void CourseLevel_Description()
            {
                Assert.AreEqual(desc, courseLevel.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseLevel_GuidNullException()
            {
                new CourseLevel(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseLevel_CodeNullException()
            {
                new CourseLevel(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseLevel_DescNullException()
            {
                new CourseLevel(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseLevelGuidEmptyException()
            {
                new CourseLevel(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseLevelCodeEmptyException()
            {
                new CourseLevel(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseLevelDescEmptyException()
            {
                new CourseLevel(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class CourseLevel_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private CourseLevel courseLevel1;
            private CourseLevel courseLevel2;
            private CourseLevel courseLevel3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "100";
                desc = "First Year";
                courseLevel1 = new CourseLevel(guid, code, desc);
                courseLevel2 = new CourseLevel(guid, code, "Second Year");
                courseLevel3 = new CourseLevel(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CourseLevelSameCodesEqual()
            {
                Assert.IsTrue(courseLevel1.Equals(courseLevel2));
            }

            [TestMethod]
            public void CourseLevelDifferentCodeNotEqual()
            {
                Assert.IsFalse(courseLevel1.Equals(courseLevel3));
            }
        }

        [TestClass]
        public class CourseLevel_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CourseLevel courseLevel1;
            private CourseLevel courseLevel2;
            private CourseLevel courseLevel3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "100";
                desc = "First Year";
                courseLevel1 = new CourseLevel(guid, code, desc);
                courseLevel2 = new CourseLevel(guid, code, "Second Year");
                courseLevel3 = new CourseLevel(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CourseLevelSameCodeHashEqual()
            {
                Assert.AreEqual(courseLevel1.GetHashCode(), courseLevel2.GetHashCode());
            }

            [TestMethod]
            public void CourseLevelDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(courseLevel1.GetHashCode(), courseLevel3.GetHashCode());
            }
        }
    }
}
