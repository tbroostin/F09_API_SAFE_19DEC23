//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
     [TestClass]
    public class PayCycle2Tests
    {
        [TestClass]
        public class PayCycle2Constructor
        {
            private string guid;
            private string code;
            private string desc;
            private PayCycle2 payCycles;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                payCycles = new PayCycle2(guid, code, desc);
            }

            [TestMethod]
            public void PayCycle2_Code()
            {
                Assert.AreEqual(code, payCycles.Code);
            }

            [TestMethod]
            public void PayCycle2_Description()
            {
                Assert.AreEqual(desc, payCycles.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayCycle2_GuidNullException()
            {
                new PayCycle2(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayCycle2_CodeNullException()
            {
                new PayCycle2(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayCycle2_DescNullException()
            {
                new PayCycle2(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayCycle2GuidEmptyException()
            {
                new PayCycle2(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayCycle2CodeEmptyException()
            {
                new PayCycle2(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayCycle2DescEmptyException()
            {
                new PayCycle2(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class PayCycle2_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private PayCycle2 payCycles1;
            private PayCycle2 payCycles2;
            private PayCycle2 payCycles3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                payCycles1 = new PayCycle2(guid, code, desc);
                payCycles2 = new PayCycle2(guid, code, "Second Year");
                payCycles3 = new PayCycle2(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void PayCycle2SameCodesEqual()
            {
                Assert.IsTrue(payCycles1.Equals(payCycles2));
            }

            [TestMethod]
            public void PayCycle2DifferentCodeNotEqual()
            {
                Assert.IsFalse(payCycles1.Equals(payCycles3));
            }
        }

        [TestClass]
        public class PayCycle2_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private PayCycle2 payCycles1;
            private PayCycle2 payCycles2;
            private PayCycle2 payCycles3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                payCycles1 = new PayCycle2(guid, code, desc);
                payCycles2 = new PayCycle2(guid, code, "Second Year");
                payCycles3 = new PayCycle2(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void PayCycle2SameCodeHashEqual()
            {
                Assert.AreEqual(payCycles1.GetHashCode(), payCycles2.GetHashCode());
            }

            [TestMethod]
            public void PaycycleDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(payCycles1.GetHashCode(), payCycles3.GetHashCode());
            }
        }
    }
}
