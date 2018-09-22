using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CorequisiteTests
    {
        [TestClass]
        public class CorequisiteConstructor
        {
            private string id;
            private bool required;
            private Corequisite coreq;

            [TestInitialize]
            public void Initialize()
            {

            }

            [TestMethod]
            public void Id()
            {
                id = "12345";
                required = true;
                coreq = new Corequisite(id, required);
                Assert.AreEqual(id, coreq.Id);
            }

            [TestMethod]
            public void Required()
            {
                id = "12345";
                required = true;
                coreq = new Corequisite(id, required);
                Assert.AreEqual(required, coreq.Required);
            }

            [TestMethod]
            public void FalseInitializesRequiredToFalse()
            {
                id = "12345";
                required = false;
                coreq = new Corequisite(id, required);
                Assert.AreEqual(false, coreq.Required);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullIdThrowsException()
            {
                id = null;
                required = false;
                coreq = new Corequisite(id, required);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BlankIdThrowsException()
            {
                id = "";
                required = true;
                coreq = new Corequisite(id, required);
            }
        }

        [TestClass]
        public class CorequisiteEquals
        {
            private Corequisite coreq1;
            private Corequisite coreq2;
            private Corequisite coreq3;


            [TestInitialize]
            public void Initialize()
            {
                coreq1 = new Corequisite("123", false);
                coreq2 = new Corequisite("123", true);
                coreq3 = new Corequisite("456", false);
            }

            [TestMethod]
            public void RequisiteIdSameEqual()
            {
                Assert.IsTrue(coreq1.Equals(coreq2));
            }

            [TestMethod]
            public void RequisiteIdDifferentNotEqual()
            {
                Assert.IsFalse(coreq2.Equals(coreq3));
            }
        }

        [TestClass]
        public class CorequisiteGetHashCode
        {
            private Corequisite coreq1;
            private Corequisite coreq2;
            private Corequisite coreq3;


            [TestInitialize]
            public void Initialize()
            {
                coreq1 = new Corequisite("123", false);
                coreq2 = new Corequisite("123", true);
                coreq3 = new Corequisite("456", false);
            }

            [TestMethod]
            public void CoreqSameIdHashCodeEqual()
            {
                Assert.AreEqual(coreq1.GetHashCode(), coreq2.GetHashCode());
            }

            [TestMethod]
            public void CoreqDifferentIdHashCodeNotEqual()
            {
                Assert.AreNotEqual(coreq2.GetHashCode(), coreq3.GetHashCode());
            }
        }
    }
}
