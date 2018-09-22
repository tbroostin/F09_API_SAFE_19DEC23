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
    public class DepositTypeTests
    {
        static string code = "MEALS";
        static string description = "Meal Plan Deposit";
        DepositType type = new DepositType(code, description);

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositType_Constructor_NullCode()
        {
            DepositType type = new DepositType(null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositType_Constructor_EmptyCode()
        {
            DepositType type = new DepositType(string.Empty, description);
        }

        [TestMethod]
        public void DepositType_Constructor_ValidCode()
        {
            Assert.AreEqual(code, type.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositType_Constructor_NullDescription()
        {
            DepositType type = new DepositType(code, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositType_Constructor_EmptyDescription()
        {
            DepositType type = new DepositType(code, string.Empty);
        }

        [TestMethod]
        public void DepositType_Constructor_ValidDescription()
        {
            Assert.AreEqual(description, type.Description);
        }
    }
}
