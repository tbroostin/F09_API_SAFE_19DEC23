using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SpecializationTests
    {
        [TestClass]
        public class SpecializationConstructor
        {
            private string code;
            private string desc;
            Specialization Specialization;

            [TestInitialize]
            public void Initialize()
            {
                code = "HIST";

                desc = "Histor Specialization";

                Specialization = new Specialization(code, desc);
            }

            [TestMethod]
            public void SpecializationCode()
            {
                Assert.AreEqual(code, Specialization.Code);
            }

            [TestMethod]
            public void SpecializationDescription()
            {
                Assert.AreEqual(desc, Specialization.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SpecializationCodeNullException()
            {
                Specialization = new Specialization(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SpecializationDescriptionNullException()
            {
                Specialization = new Specialization(code, null);
            }
        }

        [TestClass]
        public class SpecializationGetHashCode
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Specialization Specialization1;
            Specialization Specialization2;
            Specialization Specialization3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "Histor Specialization";
                Specialization1 = new Specialization(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Specialization";
                Specialization2 = new Specialization(code2, desc2);

                Specialization3 = new Specialization(code1, desc1);
            }

            [TestMethod]
            public void SpecializationGetHashCodeTest()
            {
                // objects with equal ID have equal hash.
                Assert.AreEqual(Specialization1.GetHashCode(), Specialization3.GetHashCode());
                Assert.AreNotEqual(Specialization1.GetHashCode(), Specialization2.GetHashCode());
            }
        }

        [TestClass]
        public class CourseEquals
        {
            private string code1;
            private string desc1;
            private string code2;
            private string desc2;

            Specialization Specialization1;
            Specialization Specialization2;
            Specialization Specialization3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "HIST";
                desc1 = "Histor Specialization";
                Specialization1 = new Specialization(code1, desc1);

                code2 = "MATH";
                desc2 = "Mathematics Specialization";
                Specialization2 = new Specialization(code2, desc2);

                Specialization3 = new Specialization(code1, desc1);
            }

            [TestMethod]
            public void SpecializationEqualsTest()
            {
                // ID determines equality
                Assert.IsTrue(Specialization1.Equals(Specialization3));
                Assert.IsFalse(Specialization1.Equals(Specialization2));
            }
        }
    }
}