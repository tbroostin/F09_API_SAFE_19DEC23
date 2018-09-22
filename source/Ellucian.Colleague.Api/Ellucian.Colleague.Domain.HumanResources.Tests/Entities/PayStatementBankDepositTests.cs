/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementBankDepositTests
    {
        public string expectedBankName = "FOO";
        public BankAccountType expectedBankAccountType = BankAccountType.Savings;
        public string expectedAccountLastFour = "1234";
        public decimal? expectedDepositAmount = 500;

        [TestMethod]
        public void PayStatementBankDepositConstructorPropertiesAreSetTest()
        {
            var actual = new PayStatementBankDeposit(expectedBankName, expectedBankAccountType, expectedAccountLastFour, expectedDepositAmount);
            Assert.AreEqual(expectedBankName, actual.BankName);
            Assert.AreEqual(expectedBankAccountType, actual.BankAccountType);
            Assert.AreEqual(expectedAccountLastFour, actual.AccountIdLastFour);
            Assert.AreEqual(expectedDepositAmount, actual.DepositAmount);
        }

        [TestMethod]
        public void NullBankNameThrowsException()
        {
            var actual = new PayStatementBankDeposit(null, expectedBankAccountType, expectedAccountLastFour, expectedDepositAmount);
            Assert.AreEqual(string.Empty, actual.BankName);
        }
    }
}
