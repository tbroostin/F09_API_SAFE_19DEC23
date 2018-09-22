// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class EmergencyContactTests
    {
        private string name;
        private string relationship;

        [TestInitialize] 
        public void Initialize()
        {
            name = "John Q. Public";
        }

        [TestMethod]
        public void EmergencyContact_Constructor()
        {
            var testContact = new EmergencyContact(name);
            Assert.AreEqual(name, testContact.Name);
            Assert.AreEqual(string.Empty, testContact.Relationship);
            Assert.AreEqual(string.Empty, testContact.DaytimePhone);
            Assert.AreEqual(string.Empty, testContact.EveningPhone);
            Assert.AreEqual(string.Empty, testContact.OtherPhone);
            Assert.AreEqual(null, testContact.EffectiveDate);
            Assert.AreEqual(true, testContact.IsEmergencyContact);
            Assert.AreEqual(false, testContact.IsMissingPersonContact);
            Assert.AreEqual(string.Empty, testContact.Address);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmergencyContact_Constructor_NullName()
        {
            var testContact = new EmergencyContact(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmergencyContact_NameTooLong()
        {
            // Contact name must not exceed 57 characters.
            name = "Abraham Barrymore Cedric David Ephraim Frederick Glisson-Hamrick";
            var testContact = new EmergencyContact(name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmergencyContact_AddressTooLong()
        {
            // Contact address must not exceed 75 characters.
            var testContact = new EmergencyContact(name)
            {
                DaytimePhone = "Day: 703-123-3456",
                EveningPhone = "Eve: 540-234-1234",
                OtherPhone = "Other: 703-098-6789",
                Relationship = "Relation is Papa",
                EffectiveDate = new DateTime(2014, 06, 27),
                IsEmergencyContact = true,
                IsMissingPersonContact = false,
                Address = "Papa's address is 13708 Longfellow Shore Boulevard, Apartment 314, Cityville, New York"
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmergencyContact_DaytimePhoneTooLong()
        {
            // Contact phone must not exceed 20 characters.
            var testContact = new EmergencyContact(name)
            {
                DaytimePhone = "Daytime phone: 703-123-3456",
                EveningPhone = "Eve: 540-234-1234",
                OtherPhone = "Other: 703-098-6789",
                Relationship = "Relation is Papa",
                EffectiveDate = new DateTime(2014, 06, 27),
                IsEmergencyContact = true,
                IsMissingPersonContact = false,
                Address = "Papa's address"
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmergencyContact_EveningPhoneTooLong()
        {
            // Contact phone must not exceed 20 characters.
            var testContact = new EmergencyContact(name)
            {
                DaytimePhone = "Day: 703-123-3456",
                EveningPhone = "Evening phone: 540-234-1234",
                OtherPhone = "Other: 703-098-6789",
                Relationship = "Relation is Papa",
                EffectiveDate = new DateTime(2014, 06, 27),
                IsEmergencyContact = true,
                IsMissingPersonContact = false,
                Address = "Papa's address"
            };
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmergencyContact_OtherPhoneTooLong()
        {
            // Contact phone must not exceed 20 characters.
            var testContact = new EmergencyContact(name)
            {
                DaytimePhone = "Day: 703-123-3456",
                EveningPhone = "Eve: 540-234-1234",
                OtherPhone = "Other phone: 703-098-6789",
                Relationship = "Relation is Papa",
                EffectiveDate = new DateTime(2014, 06, 27),
                IsEmergencyContact = true,
                IsMissingPersonContact = false,
                Address = "Papa's address"
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmergencyContact_RelationshipTooLong()
        {
            // Contact relationship must not exceed 25 characters.
            var testContact = new EmergencyContact(name)
            {
                DaytimePhone = "Day: 703-123-3456",
                EveningPhone = "Eve: 540-234-1234",
                OtherPhone = "Other: 703-098-6789",
                Relationship = "The relationship of this person to the student is father.",
                EffectiveDate = new DateTime(2014, 06, 27),
                IsEmergencyContact = true,
                IsMissingPersonContact = false,
                Address = "Papa's address"
            };
        }

    }
}
