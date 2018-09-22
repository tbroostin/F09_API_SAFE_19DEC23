using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class MajorTests
    {
        [TestClass]
        public class MajorConstructor
        {
            private string code;
            private string desc;
            Major major;

            [TestInitialize]
            public void Initialize()
            {
                code = "HIST";

                desc = "Histor Major";

                major = new Major(code, desc);
            }

            [TestMethod]
            public void MajorCode()
            {
                Assert.AreEqual(code, major.Code);
            }

            [TestMethod]
            public void MajorDescription()
            {
                Assert.AreEqual(desc, major.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MajorCodeNullException()
            {
                major = new Major(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MajorDescriptionNullException()
            {
                major = new Major(code, null);
            }
        }

        [TestClass]
        public class MajorGetHashCode
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Major major1;
            Major major2;
            Major major3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "Histor Major";
                major1 = new Major(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Major";
                major2 = new Major(code2, desc2);

                major3 = new Major(code1, desc1);
            }

            [TestMethod]
            public void MajorGetHashCodeTest()
            {
                // objects with equal ID have equal hash.
                Assert.AreEqual(major1.GetHashCode(), major3.GetHashCode());
                Assert.AreNotEqual(major1.GetHashCode(), major2.GetHashCode());
            }
        }

        [TestClass]
        public class CourseEquals
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Major major1;
            Major major2;
            Major major3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "Histor Major";
                major1 = new Major(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Major";
                major2 = new Major(code2, desc2);

                major3 = new Major(code1, desc1);
            }

            [TestMethod]
            public void MajorEqualsTest()
            {
                // ID determines equality
                Assert.IsTrue(major1.Equals(major3));
                Assert.IsFalse(major1.Equals(major2));
            }
        }
    }
}