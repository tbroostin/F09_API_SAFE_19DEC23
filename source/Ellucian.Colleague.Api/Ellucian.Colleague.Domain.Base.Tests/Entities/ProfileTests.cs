// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ProfileTests
    {

        [TestMethod]
        public void Profile_ConfirmationDatePropertiesAreSet()
        {
            var personId = "0000007";
            var addressConfirmationDateTime = new DateTimeOffset(2001, 1, 2, 15, 16, 17, TimeSpan.FromHours(-3));
            var emailAddressConfirmationDateTime = new DateTimeOffset(2002, 3, 4, 18, 19, 20, TimeSpan.FromHours(-3));
            var phoneConfirmationDateTime = new DateTimeOffset(2003, 5, 6, 21, 22, 23, TimeSpan.FromHours(-3));

            var profile = new Profile(personId, "Brown");

            profile.AddressConfirmationDateTime = addressConfirmationDateTime;
            profile.EmailAddressConfirmationDateTime = emailAddressConfirmationDateTime;
            profile.PhoneConfirmationDateTime = phoneConfirmationDateTime;

            Assert.AreEqual(personId, profile.Id);
            Assert.AreEqual(addressConfirmationDateTime, profile.AddressConfirmationDateTime);
            Assert.AreEqual(emailAddressConfirmationDateTime, profile.EmailAddressConfirmationDateTime);
            Assert.AreEqual(phoneConfirmationDateTime, profile.PhoneConfirmationDateTime);
        }

        [TestMethod]
        public void Profile_AddEmailAddress_ShouldAddEmailAddresses()
        {
            var personId = "0000007";
            var profile = new Profile(personId, "Brown");

            var emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(new EmailAddress("rbrown@xmail.com", "COL") { IsPreferred = false });
            emailAddresses.Add(new EmailAddress("rlbrown@xellucian.com", "BUS") { IsPreferred = true });
            emailAddresses.Add(new EmailAddress("booboo@qmail.com", "WWW") { IsPreferred = false });

            foreach (var email in emailAddresses)
            {
                profile.AddEmailAddress(email);
            }

            Assert.AreEqual(emailAddresses.Count, profile.EmailAddresses.Count);
            Assert.AreEqual(emailAddresses.ElementAt(0), profile.EmailAddresses.ElementAt(0));
            Assert.AreEqual(emailAddresses.ElementAt(1), profile.EmailAddresses.ElementAt(1));
            Assert.AreEqual(emailAddresses.ElementAt(2), profile.EmailAddresses.ElementAt(2));
        }

        [TestMethod]
        public void Profile_AddAddress_ShouldAddAddresses()
        {
            var personId = "0000007";
            var profile = new Profile(personId, "Brown");

            var personAddresses = new List<Address>();
            personAddresses.Add(new Address("123", personId) { AddressLines = new List<string>() { "123 Main" }, City = "Anywhere", Type = "H" });
            personAddresses.Add(new Address("234", personId) { AddressLines = new List<string>() { "1 Oak Ave" }, State = "AL", Type = "B" });
            personAddresses.Add(new Address("567", personId) { AddressLines = new List<string>() { "1 New sT" }, State = "NY", Type = "Z" });

            foreach (var addr in personAddresses)
            {
                profile.AddAddress(addr);
            }

            Assert.AreEqual(personAddresses.Count, profile.Addresses.Count);
            Assert.AreEqual(personAddresses.ElementAt(0), profile.Addresses.ElementAt(0));
            Assert.AreEqual(personAddresses.ElementAt(1), profile.Addresses.ElementAt(1));
            Assert.AreEqual(personAddresses.ElementAt(2), profile.Addresses.ElementAt(2));

        }

        [TestMethod]
        public void Profile_AddPhone_ShouldAddPhones()
        {
            var personId = "0000007";
            var profile = new Profile(personId, "Brown");

            var personPhones = new PhoneNumber(personId);
            personPhones.AddPhone(new Phone("864-123-1234", "HO", "x123"));
            personPhones.AddPhone(new Phone("864-321-4321", "B", "x432"));
            personPhones.AddPhone(new Phone("555-800-1212", "X", "x432"));

            foreach (var phoneNumber in personPhones.PhoneNumbers)
            {
                profile.AddPhone(phoneNumber);
            }

            Assert.AreEqual(personPhones.PhoneNumbers.Count, profile.Phones.Count);
            Assert.AreEqual(personPhones.PhoneNumbers.ElementAt(0), profile.Phones.ElementAt(0));
            Assert.AreEqual(personPhones.PhoneNumbers.ElementAt(1), profile.Phones.ElementAt(1));
            Assert.AreEqual(personPhones.PhoneNumbers.ElementAt(2), profile.Phones.ElementAt(2));
        }
    }
}
