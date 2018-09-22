// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class InvoiceTypeTests
    {
        static string code;
        static string description;

        [TestInitialize]
        public void Initialize()
        {
            code = "BK";
            description = "Book Store";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoiceType_Constructor_NullCode()
        {
            var invoiceType = new InvoiceType(null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoiceType_Constructor_EmptyCode()
        {
            var invoiceType = new InvoiceType(string.Empty, description);
        }

        [TestMethod]
        public void InvoiceType_Constructor_ValidCode()
        {
            var invoiceType = new InvoiceType(code, description);
            Assert.AreEqual(code, invoiceType.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoiceType_Constructor_NullDescription()
        {
            var invoiceType = new InvoiceType(code, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoiceType_Constructor_EmptyDescription()
        {
            var invoiceType = new InvoiceType(code, string.Empty);
        }

        [TestMethod]
        public void InvoiceType_Constructor_ValidDescription()
        {
            var invoiceType = new InvoiceType(code, description);
            Assert.AreEqual(description, invoiceType.Description);

        }
    }
}
