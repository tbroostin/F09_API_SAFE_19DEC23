//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class WithdrawReasonTests
    {
        [TestClass]
        public class WithdrawReasonConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private WithdrawReason withdrawReasons;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AC";
                desc = "Academic Reasons";
                withdrawReasons = new WithdrawReason(guid, code, desc);
            }

            [TestMethod]
            public void WithdrawReason_Code()
            {
                Assert.AreEqual(code, withdrawReasons.Code);
            }

            [TestMethod]
            public void WithdrawReason_Description()
            {
                Assert.AreEqual(desc, withdrawReasons.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void WithdrawReason_GuidNullException()
            {
                new WithdrawReason(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void WithdrawReason_CodeNullException()
            {
                new WithdrawReason(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void WithdrawReason_DescNullException()
            {
                new WithdrawReason(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void WithdrawReasonGuidEmptyException()
            {
                new WithdrawReason(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void WithdrawReasonCodeEmptyException()
            {
                new WithdrawReason(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void WithdrawReasonDescEmptyException()
            {
                new WithdrawReason(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class WithdrawReason_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private WithdrawReason withdrawReasons1;
            private WithdrawReason withdrawReasons2;
            private WithdrawReason withdrawReasons3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AC";
                desc = "Academic Reasons";
                withdrawReasons1 = new WithdrawReason(guid, code, desc);
                withdrawReasons2 = new WithdrawReason(guid, code, "Second Year");
                withdrawReasons3 = new WithdrawReason(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void WithdrawReasonSameCodesEqual()
            {
                Assert.IsTrue(withdrawReasons1.Equals(withdrawReasons2));
            }

            [TestMethod]
            public void WithdrawReasonDifferentCodeNotEqual()
            {
                Assert.IsFalse(withdrawReasons1.Equals(withdrawReasons3));
            }
        }

        [TestClass]
        public class WithdrawReason_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private WithdrawReason withdrawReasons1;
            private WithdrawReason withdrawReasons2;
            private WithdrawReason withdrawReasons3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AC";
                desc = "Academic Reasons";
                withdrawReasons1 = new WithdrawReason(guid, code, desc);
                withdrawReasons2 = new WithdrawReason(guid, code, "Second Year");
                withdrawReasons3 = new WithdrawReason(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void WithdrawReasonSameCodeHashEqual()
            {
                Assert.AreEqual(withdrawReasons1.GetHashCode(), withdrawReasons2.GetHashCode());
            }

            [TestMethod]
            public void WithdrawReasonDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(withdrawReasons1.GetHashCode(), withdrawReasons3.GetHashCode());
            }
        }
    }
}
