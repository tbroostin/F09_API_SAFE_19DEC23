using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ChargeCodeTests
    {
        string code = "MATFE";
        string desc = "Materials Fee";
        int priority = 50;

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ChargeCode_Constructor_PriorityLessThanZero()
        {
            priority = -1;
            var result = new ChargeCode(code, desc, priority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ChargeCode_Constructor_PriorityGreaterThan999()
        {
            priority = 1000;
            var result = new ChargeCode(code, desc, priority);
        }

        [TestMethod]
        public void ChargeCode_Constructor_NullPriority()
        {
            var result = new ChargeCode(code, desc, null);

            Assert.AreEqual(999, result.Priority);
        }

        [TestMethod]
        public void ChargeCode_Constructor_ZeroPriority()
        {
            var result = new ChargeCode(code, desc, 0);

            Assert.AreEqual(999, result.Priority);
        }

        [TestMethod]
        public void ChargeCode_Constructor_ValidPriority()
        {
            var result = new ChargeCode(code, desc, 50);

            Assert.AreEqual(50, result.Priority);
        }

    }
}
