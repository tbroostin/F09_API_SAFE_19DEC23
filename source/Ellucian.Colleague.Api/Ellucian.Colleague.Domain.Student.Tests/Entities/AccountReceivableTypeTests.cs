using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AccountReceivableTypeTests
    {
        [TestClass]
        public class AccountReceivableTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private AccountReceivableType accountReceivableType;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Student Receivable";
                accountReceivableType = new AccountReceivableType(guid, code, desc);
            }

            [TestMethod]
            public void AccountReceivableType_Code()
            {
                Assert.AreEqual(code, accountReceivableType.Code);
            }

            [TestMethod]
            public void AccountReceivableType_Description()
            {
                Assert.AreEqual(desc, accountReceivableType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountReceivableType_GuidNullException()
            {
                new AccountReceivableType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountReceivableType_CodeNullException()
            {
                new AccountReceivableType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountReceivableType_DescNullException()
            {
                new AccountReceivableType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountReceivableTypeGuidEmptyException()
            {
                new AccountReceivableType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountReceivableTypeCodeEmptyException()
            {
                new AccountReceivableType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountReceivableTypeDescEmptyException()
            {
                new AccountReceivableType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class AccountReceivableType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private AccountReceivableType accountReceivableType1;
            private AccountReceivableType accountReceivableType2;
            private AccountReceivableType accountReceivableType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Student Receivable";
                accountReceivableType1 = new AccountReceivableType(guid, code, desc);
                accountReceivableType2 = new AccountReceivableType(guid, code, "Project Receivables");
                accountReceivableType3 = new AccountReceivableType(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void AccountReceivableTypeSameCodesEqual()
            {
                Assert.IsTrue(accountReceivableType1.Equals(accountReceivableType2));
            }

            [TestMethod]
            public void AccountReceivableTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(accountReceivableType1.Equals(accountReceivableType3));
            }
        }

        [TestClass]
        public class AccountReceivableType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private AccountReceivableType accountReceivableType1;
            private AccountReceivableType accountReceivableType2;
            private AccountReceivableType accountReceivableType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Student Receivable";
                accountReceivableType1 = new AccountReceivableType(guid, code, desc);
                accountReceivableType2 = new AccountReceivableType(guid, code, "Project Receivables");
                accountReceivableType3 = new AccountReceivableType(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void AccountReceivableTypeSameCodeHashEqual()
            {
                Assert.AreEqual(accountReceivableType1.GetHashCode(), accountReceivableType2.GetHashCode());
            }

            [TestMethod]
            public void AccountReceivableTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(accountReceivableType1.GetHashCode(), accountReceivableType3.GetHashCode());
            }
        }
    }
}
