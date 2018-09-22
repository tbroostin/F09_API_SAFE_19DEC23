/* Copyright 2014 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class EmergencyInformationTests
    {
        private string personID;

        [TestInitialize]
        public void Initialize()
        {
            personID = "0001234";
        }

        [TestMethod]
        public void EmergencyInformation_Constructor()
        {
            var testEmergencyInformation = new EmergencyInformation(personID);
            Assert.AreEqual(personID, testEmergencyInformation.PersonId);

            // There should be no entries in Emergency Contacts.
            Assert.AreEqual(0, testEmergencyInformation.EmergencyContacts.Count);
            Assert.AreEqual(string.Empty, testEmergencyInformation.AdditionalInformation);
            Assert.AreEqual(string.Empty, testEmergencyInformation.InsuranceInformation);
            Assert.AreEqual(string.Empty, testEmergencyInformation.HospitalPreference);
            Assert.AreEqual(0, testEmergencyInformation.HealthConditions.Count);
            Assert.AreEqual(null, testEmergencyInformation.ConfirmedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmergencyInformation_Constructor_NullPersonId()
        {
            var testEmergencyInformation = new EmergencyInformation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmergencyInformation_HospitalPreferenceTooLong()
        {
            var testEmergencyInformation = new EmergencyInformation(personID)
            {
                // Hospital preference must not exceed 50 characters.
                HospitalPreference = "Cliftondale-Hedgeson Memorial Hospital of North Carolina"
            };
        }

        [TestMethod]
        public void EmergencyInformation_AddEmergencyContact()
        {
            string name = "John Q. Public";
            var emergencyContact = new EmergencyContact(name);
            var testEmergencyInformation = new EmergencyInformation(personID);
            testEmergencyInformation.AddEmergencyContact(emergencyContact);
            Assert.AreEqual(1, testEmergencyInformation.EmergencyContacts.Count);
            Assert.IsTrue(testEmergencyInformation.EmergencyContacts.Contains(emergencyContact));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmergencyInformation_AddEmergencyContact_NullEmergencyContact()
        {
            var testEmergencyInformation = new EmergencyInformation(personID);
            testEmergencyInformation.AddEmergencyContact(null);
        }

        [TestMethod]
        public void EmergencyInformation_AddHealthCondition()
        {
            string healthCondition = "AL"; // Allergies would be an example.
            var testEmergencyInformation = new EmergencyInformation(personID);
            testEmergencyInformation.AddHealthCondition(healthCondition);
            Assert.AreEqual(1, testEmergencyInformation.HealthConditions.Count);
            Assert.IsTrue(testEmergencyInformation.HealthConditions.Contains(healthCondition));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmergencyInformation_AddHealthCondition_NullHealthCondition()
        {
            var testEmergencyInformation = new EmergencyInformation(personID);
            testEmergencyInformation.AddHealthCondition(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmergencyInformation_AddHealthCondition_EmptyHealthCondition()
        {
            var testEmergencyInformation = new EmergencyInformation(personID);
            testEmergencyInformation.AddHealthCondition("");
        }
    }
}
