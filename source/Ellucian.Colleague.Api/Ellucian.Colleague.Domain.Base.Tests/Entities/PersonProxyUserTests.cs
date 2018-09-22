// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonProxyUserTests
    {
        protected List<EmailAddress> emails = new List<EmailAddress>()
        {
            new EmailAddress("mail1@mail.com", "BUS"),
            new EmailAddress("mail2@mail.com", "OTH"),

        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonProxyUser_NullGivenName()
        {
            var user = new PersonProxyUser(null, null, "Last", emails, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonProxyUser_EmptyGivenName()
        {
            var user = new PersonProxyUser(null, string.Empty, "Last", emails, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonProxyUser_NullEmail()
        {
            var user = new PersonProxyUser(null, "first", "Last", null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PersonProxyUser_EmptyEmail()
        {
            var user = new PersonProxyUser(null, "first", "Last", new List<EmailAddress>(), null, null);
        }

        [TestMethod]
        public void PersonProxyUser_NullPhone()
        {
            var user = new PersonProxyUser(null, "first", "Last", emails, null, null);
            Assert.IsNotNull(user.Phones);
            Assert.AreEqual(0, user.Phones.Count);
        }

        [TestMethod]
        public void PersonProxyUser_NullNames()
        {
            var user = new PersonProxyUser(null, "first", "Last", emails, null, null);
            Assert.IsNotNull(user.FormerNames);
            Assert.AreEqual(0, user.FormerNames.Count);
        }

        [TestMethod]
        public void PersonProxyUser_NonNullPhones()
        {
            var phones = new List<Phone>(){new Phone("number1", "HO", "Ext1"),
                                           new Phone("number2", "HO", "Ext2")};
            var user = new PersonProxyUser(null, "first", "Last", emails, phones, null);
            Assert.IsNotNull(user.Phones);
            CollectionAssert.AreEqual(phones, user.Phones);
        }

        [TestMethod]
        public void PersonProxyUser_NonNullNames()
        {
            var names = new List<PersonName>(){new PersonName("Given1", "Middle1", "Last1"),
                                               new PersonName("Given2", "Middle2", "Last2")};
            var user = new PersonProxyUser(null, "first", "Last", emails, null, names);
            Assert.IsNotNull(user.FormerNames);
            CollectionAssert.AreEqual(names, user.FormerNames);
        }
    }
}
