using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class DegreeTests
    {
        [TestClass]
        public class DegreeConstructor
        {
            private string code;
            private string desc;
            Degree degree;

            [TestInitialize]
            public void Initialize()
            {
                code = "AA";

                desc = "Associates of Art";

                degree = new Degree(code, desc);
            }

            [TestMethod]
            public void MajorCode()
            {
                Assert.AreEqual(code, degree.Code);
            }

            [TestMethod]
            public void MajorDescription()
            {
                Assert.AreEqual(desc, degree.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MajorCodeNullException()
            {
                degree = new Degree(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MajorDescriptionNullException()
            {
                degree = new Degree(code, null);
            }
        }

        [TestClass]
        public class MajorGetHashCode
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Degree degree1;
            Degree degree2;
            Degree degree3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "Histor Degree";
                degree1 = new Degree(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Degree";
                degree2 = new Degree(code2, desc2);

                degree3 = new Degree(code1, desc1);
            }

            [TestMethod]
            public void MajorGetHashCodeTest()
            {
                // objects with equal ID have equal hash.
                Assert.AreEqual(degree1.GetHashCode(), degree3.GetHashCode());
                Assert.AreNotEqual(degree1.GetHashCode(), degree2.GetHashCode());
            }
        }

        [TestClass]
        public class CourseEquals
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Degree degree1;
            Degree degree2;
            Degree degree3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "Histor Degree";
                degree1 = new Degree(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Degree";
                degree2 = new Degree(code2, desc2);

                degree3 = new Degree(code1, desc1);
            }

            [TestMethod]
            public void MajorEqualsTest()
            {
                // ID determines equality
                Assert.IsTrue(degree1.Equals(degree3));
                Assert.IsFalse(degree1.Equals(degree2));
            }
        }
    }
}