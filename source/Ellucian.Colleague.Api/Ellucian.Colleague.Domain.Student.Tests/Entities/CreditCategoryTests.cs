// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CreditCategoryTests
    {
        [TestClass]
        public class CreditCategory_Constructor
        {
            private string guid;
            private string code;
            private string desc;
            private CreditType type;
            private CreditCategory cc;

            [TestInitialize]
            public void Initialize()
            {
                guid = GetGuid();
                code = "CE";
                desc = "Continuing Ed";
                type = CreditType.ContinuingEducation;
                cc = new CreditCategory(guid, code, desc, type);
            }

            [TestMethod]
            public void CreditCategory_Guid()
            {
                Assert.AreEqual(guid, cc.Guid);
            }

            [TestMethod]
            public void CreditCategory_Code()
            {
                Assert.AreEqual(code, cc.Code);
            }

            [TestMethod]
            public void CreditCategory_Description()
            {
                Assert.AreEqual(desc, cc.Description);
            }

            [TestMethod]
            public void CreditCategory_CreditType()
            {
                Assert.AreEqual(type, cc.CreditType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CreditCategory_GuidNullException()
            {
                new CreditCategory(null, code, desc, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CreditCategory_GuidEmptyException()
            {
                new CreditCategory(string.Empty, code, desc, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CreditCategory_CodeNullException()
            {
                new CreditCategory(guid, null, desc, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CreditCategoryCodeEmptyException()
            {
                new CreditCategory(guid, string.Empty, desc, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CreditCategoryDescEmptyException()
            {
                new CreditCategory(guid, code, string.Empty, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CreditCategory_DescNullException()
            {
                new CreditCategory(guid, code, null, type);
            }

        }

        private static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}