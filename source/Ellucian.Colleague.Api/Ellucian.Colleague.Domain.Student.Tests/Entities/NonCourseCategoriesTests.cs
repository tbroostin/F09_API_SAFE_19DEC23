//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class NonCourseCategoriesTests
    {
        [TestClass]
        public class NonCourseCategoriesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private NonCourseCategories nonCourseCategories;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                nonCourseCategories = new NonCourseCategories(guid, code, desc);
            }

            [TestMethod]
            public void NonCourseCategories_Code()
            {
                Assert.AreEqual(code, nonCourseCategories.Code);
            }

            [TestMethod]
            public void NonCourseCategories_Description()
            {
                Assert.AreEqual(desc, nonCourseCategories.Description);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void NonCourseCategories_GuidNullException()
            {
                new NonCourseCategories(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void NonCourseCategories_CodeNullException()
            {
                new NonCourseCategories(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void NonCourseCategories_DescNullException()
            {
                new NonCourseCategories(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void NonCourseCategoriesGuidEmptyException()
            {
                new NonCourseCategories(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void NonCourseCategoriesCodeEmptyException()
            {
                new NonCourseCategories(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void NonCourseCategoriesDescEmptyException()
            {
                new NonCourseCategories(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class NonCourseCategories_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private NonCourseCategories nonCourseCategories1;
            private NonCourseCategories nonCourseCategories2;
            private NonCourseCategories nonCourseCategories3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                nonCourseCategories1 = new NonCourseCategories(guid, code, desc);
                nonCourseCategories2 = new NonCourseCategories(guid, code, "Second Year");
                nonCourseCategories3 = new NonCourseCategories(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void NonCourseCategoriesSameCodesEqual()
            {
                Assert.IsTrue(nonCourseCategories1.Equals(nonCourseCategories2));
            }

            [TestMethod]
            public void NonCourseCategoriesDifferentCodeNotEqual()
            {
                Assert.IsFalse(nonCourseCategories1.Equals(nonCourseCategories3));
            }
        }
    }
}
