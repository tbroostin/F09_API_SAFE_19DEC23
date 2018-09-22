using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PhoneNumberTests
    {
        private string personId;
        private PhoneNumber phoneNumber;

        [TestInitialize]
        public void Initialize()
        {
            personId = "0000304";
            phoneNumber = new PhoneNumber(personId);
            phoneNumber.AddPhone(new Phone("703-815-4221", "HO", null));
            phoneNumber.AddPhone(new Phone("304-577-9951", "BU", "123"));
        }

        [TestMethod]
        public void PhoneNumberPersonId()
        {
            Assert.AreEqual("0000304", phoneNumber.PersonId);
        }

        [TestMethod]
        public void PhoneNumberPhone()
        {
            Assert.AreEqual("703-815-4221", phoneNumber.PhoneNumbers.ElementAt(0).Number);
            Assert.AreEqual("304-577-9951", phoneNumber.PhoneNumbers.ElementAt(1).Number);
        }

        [TestMethod]
        public void PhoneNumberType()
        {
            Assert.AreEqual("HO", phoneNumber.PhoneNumbers.ElementAt(0).TypeCode);
            Assert.AreEqual("BU", phoneNumber.PhoneNumbers.ElementAt(1).TypeCode);
        }

        [TestMethod]
        public void PhoneNumberExtension()
        {
            Assert.AreEqual(null, phoneNumber.PhoneNumbers.ElementAt(0).Extension);
            Assert.AreEqual("123", phoneNumber.PhoneNumbers.ElementAt(1).Extension);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonIdNullException()
        {
            new PhoneNumber(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PhoneAddPhoneNullException()
        {
            string id = "0000304";
            var phoneNumber = new PhoneNumber(id);
            Phone phone = null;
            phoneNumber.AddPhone(phone);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PhoneAddPhoneException()
        {
            string id = "0000304";
            var phoneNumber = new PhoneNumber(id);
            Phone phone = new Phone("703-815-4212","H",string.Empty);
            phoneNumber.AddPhone(phone);
            phoneNumber.AddPhone(phone);
        }
    }
}