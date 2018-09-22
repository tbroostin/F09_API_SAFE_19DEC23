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
    public class BillingOverrideReasonsTests
    {
        [TestClass]
        public class BillingOverrideReasonsConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private BillingOverrideReasons billingOverrideReasons;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                billingOverrideReasons = new BillingOverrideReasons(guid, code, desc);
            }

            [TestMethod]
            public void BillingOverrideReasons_Code()
            {
                Assert.AreEqual(code, billingOverrideReasons.Code);
            }

            [TestMethod]
            public void BillingOverrideReasons_Description()
            {
                Assert.AreEqual(desc, billingOverrideReasons.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BillingOverrideReasons_GuidNullException()
            {
                new BillingOverrideReasons(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BillingOverrideReasons_CodeNullException()
            {
                new BillingOverrideReasons(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BillingOverrideReasons_DescNullException()
            {
                new BillingOverrideReasons(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BillingOverrideReasonsGuidEmptyException()
            {
                new BillingOverrideReasons(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BillingOverrideReasonsCodeEmptyException()
            {
                new BillingOverrideReasons(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BillingOverrideReasonsDescEmptyException()
            {
                new BillingOverrideReasons(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class BillingOverrideReasons_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private BillingOverrideReasons billingOverrideReasons1;
            private BillingOverrideReasons billingOverrideReasons2;
            private BillingOverrideReasons billingOverrideReasons3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                billingOverrideReasons1 = new BillingOverrideReasons(guid, code, desc);
                billingOverrideReasons2 = new BillingOverrideReasons(guid, code, "Second Year");
                billingOverrideReasons3 = new BillingOverrideReasons(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void BillingOverrideReasonsSameCodesEqual()
            {
                Assert.IsTrue(billingOverrideReasons1.Equals(billingOverrideReasons2));
            }

            [TestMethod]
            public void BillingOverrideReasonsDifferentCodeNotEqual()
            {
                Assert.IsFalse(billingOverrideReasons1.Equals(billingOverrideReasons3));
            }
        }

        [TestClass]
        public class BillingOverrideReasons_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private BillingOverrideReasons billingOverrideReasons1;
            private BillingOverrideReasons billingOverrideReasons2;
            private BillingOverrideReasons billingOverrideReasons3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                billingOverrideReasons1 = new BillingOverrideReasons(guid, code, desc);
                billingOverrideReasons2 = new BillingOverrideReasons(guid, code, "Second Year");
                billingOverrideReasons3 = new BillingOverrideReasons(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void BillingOverrideReasonsSameCodeHashEqual()
            {
                Assert.AreEqual(billingOverrideReasons1.GetHashCode(), billingOverrideReasons2.GetHashCode());
            }

            [TestMethod]
            public void BillingOverrideReasonsDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(billingOverrideReasons1.GetHashCode(), billingOverrideReasons3.GetHashCode());
            }
        }
    }
}