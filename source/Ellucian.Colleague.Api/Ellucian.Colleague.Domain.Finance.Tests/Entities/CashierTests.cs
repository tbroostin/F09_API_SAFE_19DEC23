using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class CashierTests
    {
        private string id;
        private bool eCommEnabled;
        private string login;
        private Cashier result;
        private decimal ccLimit;
        private decimal ckLimit;

        [TestInitialize]
        public void Cashier_Initialize()
        {
            id = "0123456789";
            login = "abc";
            eCommEnabled = true;
            ccLimit = 1000m;
            ckLimit = 1500m;
            result = new Cashier(id, login, eCommEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cashier_Constructor_NullId()
        {
            result = new Cashier(null, login, eCommEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cashier_Constructor_EmptyId()
        {
            result = new Cashier(string.Empty, login, eCommEnabled);
        }

        [TestMethod]
        public void Cashier_Constructor_ValidId()
        {
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cashier_Constructor_NullLogin()
        {
            result = new Cashier(id, null, eCommEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cashier_Constructor_EmptyLogin()
        {
            result = new Cashier(string.Empty, login, eCommEnabled);
        }

        [TestMethod]
        public void Cashier_Constructor_ValidLogin()
        {
            Assert.AreEqual(login, result.Login);
        }

        [TestMethod]
        public void Cashier_Constructor_DefaultECommerceEnabled()
        {
            result = new Cashier(id, login);
            Assert.IsFalse(result.IsECommerceEnabled);
        }

        [TestMethod]
        public void Cashier_Constructor_ECommerceEnabledFalse()
        {
            result = new Cashier(id, login, false);
            Assert.IsFalse(result.IsECommerceEnabled);
        }

        [TestMethod]
        public void Cashier_Constructor_ECommerceEnabledTrue()
        {
            result = new Cashier(id, login, true);
            Assert.IsTrue(result.IsECommerceEnabled);
        }

        [TestMethod]
        public void Cashier_CheckLimitAmount_ValidUpdate()
        {
            result = new Cashier(id, login, eCommEnabled);
            result.CheckLimitAmount = ckLimit;
            Assert.AreEqual(ckLimit, result.CheckLimitAmount.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Cashier_CheckLimitAmount_InvalidUpdate()
        {
            result = new Cashier(id, login, eCommEnabled);
            result.CheckLimitAmount = ccLimit;
            result.CheckLimitAmount = ckLimit;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Cashier_CheckLimitAmount_InvalidAmount()
        {
            result = new Cashier(id, login, eCommEnabled);
            result.CheckLimitAmount = -ckLimit;
        }

        [TestMethod]
        public void Cashier_CreditCardLimitAmount_ValidUpdate()
        {
            result = new Cashier(id, login, eCommEnabled);
            result.CreditCardLimitAmount = ccLimit;
            Assert.AreEqual(ccLimit, result.CreditCardLimitAmount.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Cashier_CreditCardLimitAmount_InvalidUpdate()
        {
            result = new Cashier(id, login, eCommEnabled);
            result.CreditCardLimitAmount = ccLimit;
            result.CreditCardLimitAmount = ckLimit;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Cashier_CreditCardLimitAmount_InvalidAmount()
        {
            result = new Cashier(id, login, eCommEnabled);
            result.CreditCardLimitAmount = -ccLimit;
        }
    }
}
