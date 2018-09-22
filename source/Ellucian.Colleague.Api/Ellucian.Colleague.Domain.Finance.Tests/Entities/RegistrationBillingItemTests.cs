using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class RegistrationBillingItemTests
    {
        string id = "234567890";
        string acadCreditId = "12345";
   
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBillingItem_Constructor_NullId()
        {
            var result = new RegistrationBillingItem(null, acadCreditId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationBillingItem_Constructor_NullAcadCred()
        {
            var result = new RegistrationBillingItem(id, null);
        }

        [TestMethod]
        public void RegistrationBillingItem_Constructor_ValidId()
        {
            var result = new RegistrationBillingItem(id, acadCreditId);

            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void RegistrationBillingItem_Constructor_ValidAcademicCreditId()
        {
            var result = new RegistrationBillingItem(id, acadCreditId);

            Assert.AreEqual(acadCreditId, result.AcademicCreditId);
        }
    }
}
