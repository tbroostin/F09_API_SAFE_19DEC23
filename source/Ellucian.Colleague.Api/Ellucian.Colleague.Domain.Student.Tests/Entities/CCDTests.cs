using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CcdTests
    {
        [TestClass]
        public class CcdConstructor
        {
            private string code;
            private string desc;
            Ccd Ccd;

            [TestInitialize]
            public void Initialize()
            {
                code = "HIST";

                desc = "History Ccd";

                Ccd = new Ccd(code, desc);
            }

            [TestMethod]
            public void CcdCode()
            {
                Assert.AreEqual(code, Ccd.Code);
            }

            [TestMethod]
            public void CcdDescription()
            {
                Assert.AreEqual(desc, Ccd.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CcdCodeNullException()
            {
                Ccd = new Ccd(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CcdDescriptionNullException()
            {
                Ccd = new Ccd(code, null);
            }
        }

        [TestClass]
        public class CcdGetHashCode
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Ccd Ccd1;
            Ccd Ccd2;
            Ccd Ccd3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "History Ccd";
                Ccd1 = new Ccd(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Ccd";
                Ccd2 = new Ccd(code2, desc2);

                Ccd3 = new Ccd(code1, desc1);
            }

            [TestMethod]
            public void CcdGetHashCodeTest()
            {
                // objects with equal ID have equal hash.
                Assert.AreEqual(Ccd1.GetHashCode(), Ccd3.GetHashCode());
                Assert.AreNotEqual(Ccd1.GetHashCode(), Ccd2.GetHashCode());
            }
        }

        [TestClass]
        public class CourseEquals
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Ccd Ccd1;
            Ccd Ccd2;
            Ccd Ccd3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "History Ccd";
                Ccd1 = new Ccd(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Ccd";
                Ccd2 = new Ccd(code2, desc2);

                Ccd3 = new Ccd(code1, desc1);
            }

            [TestMethod]
            public void CcdEqualsTest()
            {
                // ID determines equality
                Assert.IsTrue(Ccd1.Equals(Ccd3));
                Assert.IsFalse(Ccd1.Equals(Ccd2));
            }
        }
    }
}