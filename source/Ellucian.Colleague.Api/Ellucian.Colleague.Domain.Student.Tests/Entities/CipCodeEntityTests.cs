//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CipCodeTests
    {
        [TestClass]
        public class CipCodeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private int? year;
            private CipCode cipCodes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                year = 2020;
                cipCodes = new CipCode(guid, code, desc, year);
            }

            [TestMethod]
            public void CipCode_Code()
            {
                Assert.AreEqual(code, cipCodes.Code);
            }

            [TestMethod]
            public void CipCode_Description()
            {
                Assert.AreEqual(desc, cipCodes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CipCode_GuidNullException()
            {
                new CipCode(null, code, desc, year);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CipCode_CodeNullException()
            {
                new CipCode(guid, null, desc, year);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CipCode_DescNullException()
            {
                new CipCode(guid, code, null, year);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CipCodeGuidEmptyException()
            {
                new CipCode(string.Empty, code, desc, year);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CipCodeCodeEmptyException()
            {
                new CipCode(guid, string.Empty, desc, year);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CipCodeDescEmptyException()
            {
                new CipCode(guid, code, string.Empty, year);
            }

        }

        [TestClass]
        public class CipCode_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private int? year;
            private CipCode cipCodes1;
            private CipCode cipCodes2;
            private CipCode cipCodes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                year = 2020;
                cipCodes1 = new CipCode(guid, code, desc, year);
                cipCodes2 = new CipCode(guid, code, "Second Year", year);
                cipCodes3 = new CipCode(Guid.NewGuid().ToString(), "200", desc, year);
            }

            [TestMethod]
            public void CipCodeSameCodesEqual()
            {
                Assert.IsTrue(cipCodes1.Equals(cipCodes2));
            }

            [TestMethod]
            public void CipCodeDifferentCodeNotEqual()
            {
                Assert.IsFalse(cipCodes1.Equals(cipCodes3));
            }
        }

        [TestClass]
        public class CipCode_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private int? year;
            private CipCode cipCodes1;
            private CipCode cipCodes2;
            private CipCode cipCodes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                cipCodes1 = new CipCode(guid, code, desc, year);
                cipCodes2 = new CipCode(guid, code, "Second Year", year);
                cipCodes3 = new CipCode(Guid.NewGuid().ToString(), "200", desc, year);
            }

            [TestMethod]
            public void CipCodeSameCodeHashEqual()
            {
                Assert.AreEqual(cipCodes1.GetHashCode(), cipCodes2.GetHashCode());
            }

            [TestMethod]
            public void CipCodeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(cipCodes1.GetHashCode(), cipCodes3.GetHashCode());
            }
        }
    }
}
