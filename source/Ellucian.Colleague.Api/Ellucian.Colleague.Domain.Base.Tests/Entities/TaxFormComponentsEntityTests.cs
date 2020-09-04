//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
     [TestClass]
    public class BoxCodesTests
    {
        [TestClass]
        public class BoxCodesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private string taxCode;
            private BoxCodes taxFormComponents;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                taxFormComponents = new BoxCodes(guid, code, desc, taxCode);
            }

            [TestMethod]
            public void BoxCodes_Code()
            {
                Assert.AreEqual(code, taxFormComponents.Code);
            }

            [TestMethod]
            public void BoxCodes_Description()
            {
                Assert.AreEqual(desc, taxFormComponents.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BoxCodes_GuidNullException()
            {
                new BoxCodes(null, code, desc, taxCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BoxCodes_CodeNullException()
            {
                new BoxCodes(guid, null, desc, taxCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BoxCodes_DescNullException()
            {
                new BoxCodes(guid, code, null, taxCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BoxCodesGuidEmptyException()
            {
                new BoxCodes(string.Empty, code, desc, taxCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BoxCodesCodeEmptyException()
            {
                new BoxCodes(guid, string.Empty, desc, taxCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BoxCodesDescEmptyException()
            {
                new BoxCodes(guid, code, string.Empty, taxCode);
            }

        }

        [TestClass]
        public class BoxCodes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private string taxCode;
            private BoxCodes taxFormComponents1;
            private BoxCodes taxFormComponents2;
            private BoxCodes taxFormComponents3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                taxCode = "W2";
                taxFormComponents1 = new BoxCodes(guid, code, desc, taxCode);
                taxFormComponents2 = new BoxCodes(guid, code, "Second Year", taxCode);
                taxFormComponents3 = new BoxCodes(Guid.NewGuid().ToString(), "200", desc, taxCode);
            }

            [TestMethod]
            public void BoxCodesSameCodesEqual()
            {
                Assert.IsTrue(taxFormComponents1.Equals(taxFormComponents2));
            }

            [TestMethod]
            public void BoxCodesDifferentCodeNotEqual()
            {
                Assert.IsFalse(taxFormComponents1.Equals(taxFormComponents3));
            }
        }

        [TestClass]
        public class BoxCodes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private string taxCode;
            private BoxCodes taxFormComponents1;
            private BoxCodes taxFormComponents2;
            private BoxCodes taxFormComponents3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                taxCode = "W2";
                taxFormComponents1 = new BoxCodes(guid, code, desc, taxCode);
                taxFormComponents2 = new BoxCodes(guid, code, "Second Year", taxCode);
                taxFormComponents3 = new BoxCodes(Guid.NewGuid().ToString(), "200", desc, taxCode);
            }

            [TestMethod]
            public void BoxCodesSameCodeHashEqual()
            {
                Assert.AreEqual(taxFormComponents1.GetHashCode(), taxFormComponents2.GetHashCode());
            }

            [TestMethod]
            public void BoxCodesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(taxFormComponents1.GetHashCode(), taxFormComponents3.GetHashCode());
            }
        }
    }
}
