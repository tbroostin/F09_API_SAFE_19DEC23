// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class OfficeCodeTests
    {
        private string code;
        private string description;
        private OfficeCode officeCode;

        [TestInitialize]
        public void Initialize()
        {
            code = "HIS";
            description = "Hispanic/Latino";
            officeCode = new OfficeCode(code, description);
        }

        [TestClass]
        public class OfficeCodeConstructor : OfficeCodeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OfficeCodeConstructorNullCode()
            {
                officeCode = new OfficeCode(null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OfficeCodeConstructorEmptyCode()
            {
                officeCode = new OfficeCode(string.Empty, description);
            }

            [TestMethod]
            public void OfficeCodeConstructorValidCode()
            {
                officeCode = new OfficeCode(code, description);
                Assert.AreEqual(code, officeCode.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OfficeCodeConstructorNullDescription()
            {
                officeCode = new OfficeCode(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OfficeCodeConstructorEmptyDescription()
            {
                officeCode = new OfficeCode(code, string.Empty);
            }

            [TestMethod]
            public void OfficeCodeConstructorValidDescription()
            {
                officeCode = new OfficeCode(code, description);
                Assert.AreEqual(description, officeCode.Description);
            }
        }

        [TestMethod]
        public void OfficeCode_ToString()
        {
            officeCode = new OfficeCode(code, description);
            Assert.AreEqual(code, officeCode.ToString());
        }
    }
}
