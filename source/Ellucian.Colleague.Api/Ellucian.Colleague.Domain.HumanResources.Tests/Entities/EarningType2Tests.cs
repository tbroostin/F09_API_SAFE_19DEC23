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
    public class EarningType2Tests
    {
        [TestClass]
        public class EarningType2Constructor
        {
            private string guid;
            private string code;
            private string desc;
            private EarningType2 earningTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                earningTypes = new EarningType2(guid, code, desc);
            }

            [TestMethod]
            public void EarningType2_Code()
            {
                Assert.AreEqual(code, earningTypes.Code);
            }

            [TestMethod]
            public void EarningType2_Description()
            {
                Assert.AreEqual(desc, earningTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EarningType2_GuidNullException()
            {
                new EarningType2(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EarningType2_CodeNullException()
            {
                new EarningType2(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EarningType2_DescNullException()
            {
                new EarningType2(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EarningType2GuidEmptyException()
            {
                new EarningType2(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EarningType2CodeEmptyException()
            {
                new EarningType2(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EarningType2DescEmptyException()
            {
                new EarningType2(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class EarningType2_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private EarningType2 earningTypes1;
            private EarningType2 earningTypes2;
            private EarningType2 earningTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                earningTypes1 = new EarningType2(guid, code, desc);
                earningTypes2 = new EarningType2(guid, code, "Second Year");
                earningTypes3 = new EarningType2(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void EarningType2SameCodesEqual()
            {
                Assert.IsTrue(earningTypes1.Equals(earningTypes2));
            }

            [TestMethod]
            public void EarningType2DifferentCodeNotEqual()
            {
                Assert.IsFalse(earningTypes1.Equals(earningTypes3));
            }
        }

        [TestClass]
        public class EarningType2_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private EarningType2 earningTypes1;
            private EarningType2 earningTypes2;
            private EarningType2 earningTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                earningTypes1 = new EarningType2(guid, code, desc);
                earningTypes2 = new EarningType2(guid, code, "Second Year");
                earningTypes3 = new EarningType2(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void EarningType2SameCodeHashEqual()
            {
                Assert.AreEqual(earningTypes1.GetHashCode(), earningTypes2.GetHashCode());
            }

            [TestMethod]
            public void EarningType2DifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(earningTypes1.GetHashCode(), earningTypes3.GetHashCode());
            }
        }
    }
}
