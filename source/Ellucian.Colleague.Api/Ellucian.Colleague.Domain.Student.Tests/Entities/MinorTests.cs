using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class MinorTests
    {
        [TestClass]
        public class MinorConstructor
        {
            private string code;
            private string desc;
            Minor Minor;

            [TestInitialize]
            public void Initialize()
            {
                code = "HIST";

                desc = "History Minor";

                Minor = new Minor(code, desc);
            }

            [TestMethod]
            public void MinorCode()
            {
                Assert.AreEqual(code, Minor.Code);
            }

            [TestMethod]
            public void MinorDescription()
            {
                Assert.AreEqual(desc, Minor.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MinorCodeNullException()
            {
                Minor = new Minor(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MinorDescriptionNullException()
            {
                Minor = new Minor(code, null);
            }
        }

        [TestClass]
        public class MinorGetHashCode
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Minor Minor1;
            Minor Minor2;
            Minor Minor3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "History Minor";
                Minor1 = new Minor(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Minor";
                Minor2 = new Minor(code2, desc2);

                Minor3 = new Minor(code1, desc1);
            }

            [TestMethod]
            public void MinorGetHashCodeTest()
            {
                // objects with equal ID have equal hash.
                Assert.AreEqual(Minor1.GetHashCode(), Minor3.GetHashCode());
                Assert.AreNotEqual(Minor1.GetHashCode(), Minor2.GetHashCode());
            }
        }

        [TestClass]
        public class CourseEquals
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Minor Minor1;
            Minor Minor2;
            Minor Minor3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "History Minor";
                Minor1 = new Minor(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Minor";
                Minor2 = new Minor(code2, desc2);

                Minor3 = new Minor(code1, desc1);
            }

            [TestMethod]
            public void MinorEqualsTest()
            {
                // ID determines equality
                Assert.IsTrue(Minor1.Equals(Minor3));
                Assert.IsFalse(Minor1.Equals(Minor2));
            }
        }
    }
}