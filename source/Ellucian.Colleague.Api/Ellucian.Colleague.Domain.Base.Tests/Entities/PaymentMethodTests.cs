// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PaymentMethodTests
    {
        private string code, description, office1, office2;
        private PaymentMethodCategory category;
        private bool webEnabled, eCommEnabled;

        [TestInitialize]
        public void Initialize()
        {
            code = "MC";
            description = "MasterCard";
            category = PaymentMethodCategory.CreditCard;
            webEnabled = true;
            eCommEnabled = true;
            office1 = "BUS";
            office2 = "CASH";
        }

        [TestMethod]
        public void PaymentMethod_Constructor_CodeValid()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            Assert.AreEqual(code, result.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentMethod_Constructor_CodeNull()
        {
            var result = new PaymentMethod(null, description, category, webEnabled, eCommEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentMethod_Constructor_CodeEmpty()
        {
            var result = new PaymentMethod(string.Empty, description, category, webEnabled, eCommEnabled);
        }

        [TestMethod]
        public void PaymentMethod_Constructor_DescriptionValid()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            Assert.AreEqual(description, result.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentMethod_Constructor_DescriptionNull()
        {
            var result = new PaymentMethod(code, null, category, webEnabled, eCommEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentMethod_Constructor_DescriptionEmpty()
        {
            var result = new PaymentMethod(code, string.Empty, category, webEnabled, eCommEnabled);
        }

        [TestMethod]
        public void PaymentMethod_Constructor_CategoryValid()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            Assert.AreEqual(category, result.Category);
        }

        [TestMethod]
        public void PaymentMethod_Constructor_IsWebEnabledValid()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            Assert.AreEqual(webEnabled, result.IsWebEnabled);
        }

        [TestMethod]
        public void PaymentMethod_Constructor_IsECommerceEnabledValid()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            Assert.AreEqual(eCommEnabled, result.IsECommerceEnabled);
        }

        [TestMethod]
        public void PaymentMethod_Constructor_ECommerceEnabledNotWebEnabled()
        {
            var result = new PaymentMethod(code, description, category, false, true);
            Assert.AreEqual(false, result.IsWebEnabled);
            Assert.AreEqual(true, result.IsECommerceEnabled);
        }

        [TestMethod]
        public void PaymentMethod_Constructor_NotWebEnabledOrECommerceEnabled()
        {
            var result = new PaymentMethod(code, description, category, false, false);
            Assert.AreEqual(false, result.IsWebEnabled);
            Assert.AreEqual(false, result.IsECommerceEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentMethod_Constructor_IsECommerceEnabledInvalid()
        {
            var result = new PaymentMethod(code, description, category, true, false);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void PaymentMethod_AddOfficeCode_NullOffice()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            result.AddOfficeCode(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentMethod_AddOfficeCode_EmptyOffice()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            result.AddOfficeCode(string.Empty);
        }

        [TestMethod]
        public void PaymentMethod_AddOfficeCode_OneOffice()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            result.AddOfficeCode(office1);
            Assert.AreEqual(1, result.OfficeCodes.Count);
            Assert.AreEqual(office1, result.OfficeCodes[0]);
        }

        [TestMethod]
        public void PaymentMethod_AddOfficeCode_DuplicateOffice()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            result.AddOfficeCode(office1);
            result.AddOfficeCode(office1);
            Assert.AreEqual(1, result.OfficeCodes.Count);
            Assert.AreEqual(office1, result.OfficeCodes[0]);
        }

        [TestMethod]
        public void PaymentMethod_AddOfficeCode_TwoOffices()
        {
            var result = new PaymentMethod(code, description, category, webEnabled, eCommEnabled);
            result.AddOfficeCode(office1);
            result.AddOfficeCode(office2);
            Assert.AreEqual(2, result.OfficeCodes.Count);
            Assert.AreEqual(office1, result.OfficeCodes[0]);
            Assert.AreEqual(office2, result.OfficeCodes[1]);
        }

        [TestMethod]
        public void paymentMethod_IsValidForStudentReceivable_Check()
        {
            var pmth = new PaymentMethod(code, description, PaymentMethodCategory.Check, webEnabled, eCommEnabled);
            Assert.IsTrue(pmth.IsValidForStudentReceivables);
        }

        [TestMethod]
        public void paymentMethod_IsValidForStudentReceivable_CreditCard()
        {
            var pmth = new PaymentMethod(code, description, PaymentMethodCategory.CreditCard, webEnabled, eCommEnabled);
            Assert.IsTrue(pmth.IsValidForStudentReceivables);
        }

        [TestMethod]
        public void paymentMethod_IsValidForStudentReceivable_ElectronicFundsTransfer()
        {
            var pmth = new PaymentMethod(code, description, PaymentMethodCategory.ElectronicFundsTransfer, webEnabled, eCommEnabled);
            Assert.IsTrue(pmth.IsValidForStudentReceivables);
        }

        [TestMethod]
        public void paymentMethod_IsValidForStudentReceivable_Other()
        {
            var pmth = new PaymentMethod(code, description, PaymentMethodCategory.Other, webEnabled, eCommEnabled);
            Assert.IsTrue(pmth.IsValidForStudentReceivables);
        }

        [TestMethod]
        public void paymentMethod_IsValidForStudentReceivable_Cash()
        {
            var pmth = new PaymentMethod(code, description, PaymentMethodCategory.Cash, webEnabled, eCommEnabled);
            Assert.IsFalse(pmth.IsValidForStudentReceivables);
        }
    }
}