// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ReceivableTypeTests
    {
        static string code = "01";
        static string description = "Student Receivables";
        ReceivableType type = new ReceivableType(code, description);

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableType_Constructor_NullCode()
        {
            ReceivableType type = new ReceivableType(null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableType_Constructor_EmptyCode()
        {
            ReceivableType type = new ReceivableType(string.Empty, description);
        }

        [TestMethod]
        public void ReceivableType_Constructor_ValidCode()
        {
            Assert.AreEqual(code, type.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableType_Constructor_NullDescription()
        {
            ReceivableType type = new ReceivableType(code, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableType_Constructor_EmptyDescription()
        {
            ReceivableType type = new ReceivableType(code, string.Empty);
        }

        [TestMethod]
        public void ReceivableType_Constructor_ValidDescription()
        {
            Assert.AreEqual(description, type.Description);
        }
    }
}
