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
    public class NonCourseGradeUsesTests
    {
        [TestClass]
        public class NonCourseGradeUsesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private NonCourseGradeUses nonCourseGradeUses;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                nonCourseGradeUses = new NonCourseGradeUses(guid, code, desc);
            }

            [TestMethod]
            public void NonCourseGradeUses_Code()
            {
                Assert.AreEqual(code, nonCourseGradeUses.Code);
            }

            [TestMethod]
            public void NonCourseGradeUses_Description()
            {
                Assert.AreEqual(desc, nonCourseGradeUses.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonCourseGradeUses_GuidNullException()
            {
                new NonCourseGradeUses(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonCourseGradeUses_CodeNullException()
            {
                new NonCourseGradeUses(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonCourseGradeUses_DescNullException()
            {
                new NonCourseGradeUses(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonCourseGradeUsesGuidEmptyException()
            {
                new NonCourseGradeUses(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonCourseGradeUsesCodeEmptyException()
            {
                new NonCourseGradeUses(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonCourseGradeUsesDescEmptyException()
            {
                new NonCourseGradeUses(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class NonCourseGradeUses_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private NonCourseGradeUses nonCourseGradeUses1;
            private NonCourseGradeUses nonCourseGradeUses2;
            private NonCourseGradeUses nonCourseGradeUses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                nonCourseGradeUses1 = new NonCourseGradeUses(guid, code, desc);
                nonCourseGradeUses2 = new NonCourseGradeUses(guid, code, "Second Year");
                nonCourseGradeUses3 = new NonCourseGradeUses(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void NonCourseGradeUsesSameCodesEqual()
            {
                Assert.IsTrue(nonCourseGradeUses1.Equals(nonCourseGradeUses2));
            }

            [TestMethod]
            public void NonCourseGradeUsesDifferentCodeNotEqual()
            {
                Assert.IsFalse(nonCourseGradeUses1.Equals(nonCourseGradeUses3));
            }
        }

        [TestClass]
        public class NonCourseGradeUses_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private NonCourseGradeUses nonCourseGradeUses1;
            private NonCourseGradeUses nonCourseGradeUses2;
            private NonCourseGradeUses nonCourseGradeUses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                nonCourseGradeUses1 = new NonCourseGradeUses(guid, code, desc);
                nonCourseGradeUses2 = new NonCourseGradeUses(guid, code, "Second Year");
                nonCourseGradeUses3 = new NonCourseGradeUses(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void NonCourseGradeUsesSameCodeHashEqual()
            {
                Assert.AreEqual(nonCourseGradeUses1.GetHashCode(), nonCourseGradeUses2.GetHashCode());
            }

            [TestMethod]
            public void NonCourseGradeUsesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(nonCourseGradeUses1.GetHashCode(), nonCourseGradeUses3.GetHashCode());
            }
        }
    }
}
