// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CourseTypeTests
    {
        [TestClass]
        public class CourseTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private bool showInCourseSearch;
            private CourseType courseType;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "STND";
                desc = "Standard";
                showInCourseSearch = true;
                courseType = new CourseType(guid, code, desc, showInCourseSearch);
            }

            [TestMethod]
            public void CourseTypeGuid()
            {
                Assert.AreEqual(guid, courseType.Guid);
            }

            [TestMethod]
            public void CourseTypeShowInCourseSearch()
            {
                Assert.AreEqual(showInCourseSearch, courseType.ShowInCourseSearch);
            }

            [TestMethod]
            public void CourseTypeCode()
            {
                Assert.AreEqual(code, courseType.Code);
            }

            [TestMethod]
            public void CourseTypeDescription()
            {
                Assert.AreEqual(desc, courseType.Description);
            }

            [TestMethod]
            public void CourseTypeShowInCatalog_TrueByDefault()
            {
                Assert.IsTrue(courseType.ShowInCourseSearch);
            }

            [TestMethod]
            public void CourseTypeShowInCatalog_SetToFalse()
            {
                var testCourseType = new CourseType(guid, code, desc, false);
                Assert.IsFalse(testCourseType.ShowInCourseSearch);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTypeCodeNullException()
            {
                new CourseType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTypeDescNullException()
            {
                new CourseType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTypes_GuidNullException()
            {
                new CourseType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTypesGuidEmptyException()
            {
                new CourseType(string.Empty, code, desc);
            }
        }

        [TestClass]
        public class CourseTypeEquals
        {
            private string guid;
            private string code;
            private string desc;
            private CourseType courseType1;
            private CourseType courseType2;
            private CourseType courseType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "STND";
                desc = "Standard";
                courseType1 = new CourseType(guid, code, desc);
                courseType2 = new CourseType(guid, code, "Coop Work Experience");
                courseType3 = new CourseType(guid, "COOP", desc);
            }

            [TestMethod]
            public void CourseTypeSameCodesEqual()
            {
                Assert.IsTrue(courseType1.Equals(courseType2));
            }

            [TestMethod]
            public void CourseTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(courseType1.Equals(courseType3));
            }
        }

        [TestClass]
        public class CourseTypeGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CourseType courseType1;
            private CourseType courseType2;
            private CourseType courseType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "STND";
                desc = "Standard";
                courseType1 = new CourseType(guid, code, desc);
                courseType2 = new CourseType(guid, code, "Coop Work Experience");
                courseType3 = new CourseType(guid, "COOP", desc);
            }

            [TestMethod]
            public void CourseTypeSameCodeHashEqual()
            {
                Assert.AreEqual(courseType1.GetHashCode(), courseType2.GetHashCode());
            }

            [TestMethod]
            public void CourseTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(courseType1.GetHashCode(), courseType3.GetHashCode());
            }
        }
    }
}
