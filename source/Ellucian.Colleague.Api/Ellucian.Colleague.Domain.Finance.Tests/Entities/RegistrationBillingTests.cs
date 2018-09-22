using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class RegistrationBillingTests
    {
        string id = "12345";
        string personId = "1234567";
        string accountTypeCode = "01";
        DateTime billingStart = DateTime.Parse("08/01/2013");
        DateTime billingEnd = DateTime.Parse("12/31/2013");
        string invoiceId = "123456789";
        List<RegistrationBillingItem> items = new List<RegistrationBillingItem>() { new RegistrationBillingItem("234567890", "1234567") };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBilling_Constructor_NullId()
        {
            var result = new RegistrationBilling(null, personId, accountTypeCode, billingStart, billingEnd, invoiceId, items);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBilling_Constructor_NullPersonId()
        {
            var result = new RegistrationBilling(id, null, accountTypeCode, billingStart, billingEnd, invoiceId, items);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBilling_Constructor_NullAccountTypeCode()
        {
            var result = new RegistrationBilling(id, personId, null, billingStart, billingEnd, invoiceId, items);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBilling_Constructor_NullBillingStartDate()
        {
            var result = new RegistrationBilling(id, personId, accountTypeCode, null, billingEnd, invoiceId, items);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBilling_Constructor_NullBillingEndDate()
        {
            var result = new RegistrationBilling(id, personId, accountTypeCode, billingStart, null, invoiceId, items);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RegistrationBilling_Constructor_BillingStartDateAfterEndDate()
        {
            var result = new RegistrationBilling(id, personId, accountTypeCode, billingEnd, billingStart, invoiceId, items);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBilling_Constructor_NullInvoiceId()
        {
            var result = new RegistrationBilling(id, personId, accountTypeCode, billingStart, billingEnd, null, items);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBilling_Constructor_NullItems()
        {
            var result = new RegistrationBilling(id, personId, accountTypeCode, billingStart, billingEnd, invoiceId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBilling_Constructor_NoItems()
        {
            var result = new RegistrationBilling(id, personId, accountTypeCode, billingStart, billingEnd, null, new List<RegistrationBillingItem>());
        }

        [TestMethod]
        public void RegistrationBilling_Constructor_Valid()
        {
            var result = new RegistrationBilling(id, personId, accountTypeCode, billingStart, billingEnd, invoiceId, items);

            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(personId, result.PersonId);
            Assert.AreEqual(accountTypeCode, result.AccountTypeCode);
            Assert.AreEqual(billingStart, result.BillingStart);
            Assert.AreEqual(billingEnd, result.BillingEnd);
            Assert.AreEqual(invoiceId, result.InvoiceId);
            CollectionAssert.AreEqual(items, result.Items.ToList());
        }
    }
}
