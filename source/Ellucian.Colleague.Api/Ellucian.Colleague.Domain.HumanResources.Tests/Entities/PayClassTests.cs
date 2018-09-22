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
    public class PayClassTests
    {
        [TestClass]
        public class PayClassConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private PayClass payClasses;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                payClasses = new PayClass(guid, code, desc);
            }

            [TestMethod]
            public void PayClass_Code()
            {
                Assert.AreEqual(code, payClasses.Code);
            }

            [TestMethod]
            public void PayClass_Description()
            {
                Assert.AreEqual(desc, payClasses.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayClass_GuidNullException()
            {
                new PayClass(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayClass_CodeNullException()
            {
                new PayClass(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayClass_DescNullException()
            {
                new PayClass(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayClassGuidEmptyException()
            {
                new PayClass(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayClassCodeEmptyException()
            {
                new PayClass(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayClassDescEmptyException()
            {
                new PayClass(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class PayClass_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private PayClass payClasses1;
            private PayClass payClasses2;
            private PayClass payClasses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                payClasses1 = new PayClass(guid, code, desc);
                payClasses2 = new PayClass(guid, code, "Second Year");
                payClasses3 = new PayClass(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void PayClassSameCodesEqual()
            {
                Assert.IsTrue(payClasses1.Equals(payClasses2));
            }

            [TestMethod]
            public void PayClassDifferentCodeNotEqual()
            {
                Assert.IsFalse(payClasses1.Equals(payClasses3));
            }
        }

        [TestClass]
        public class PayClass_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private PayClass payClasses1;
            private PayClass payClasses2;
            private PayClass payClasses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                payClasses1 = new PayClass(guid, code, desc);
                payClasses2 = new PayClass(guid, code, "Second Year");
                payClasses3 = new PayClass(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void PayClassSameCodeHashEqual()
            {
                Assert.AreEqual(payClasses1.GetHashCode(), payClasses2.GetHashCode());
            }

            [TestMethod]
            public void PayClassDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(payClasses1.GetHashCode(), payClasses3.GetHashCode());
            }
        }
    }
}
