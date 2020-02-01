// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonAgreementTests
    {
        private string id;
        private string personId;
        private string agreementCode;
        private string agreementPeriodCode;
        private bool personCanDeclinePersonAgreement;
        private string title;
        private DateTime dueDate;
        private List<string> text;
        private PersonAgreementStatus? status;
        private DateTimeOffset? actionTimestamp;
        private PersonAgreement personAgreement;

        [TestInitialize]
        public void Initialize()
        {
            id = "1";
            personId = "0001234";
            agreementCode = "AGR1";
            agreementPeriodCode = "2019FA";
            personCanDeclinePersonAgreement = true;
            title = "Financial Consent";
            dueDate = DateTime.Today.AddDays(30);
            text = new List<string> { "This is the text of the PersonAgreement." } ;
            status = PersonAgreementStatus.Accepted;
            actionTimestamp = DateTimeOffset.Now;
            personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
        }

        [TestClass]
        public class PersonAgreementConstructor_ : PersonAgreementTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_NullId()
            {
                personAgreement = new PersonAgreement(null, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_EmptyId()
            {
                personAgreement = new PersonAgreement(string.Empty, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            public void PersonAgreementConstructor_ValidId()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                Assert.AreEqual(id, personAgreement.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_NullPersonId()
            {
                personAgreement = new PersonAgreement(id, null, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_EmptyPersonId()
            {
                personAgreement = new PersonAgreement(id, string.Empty, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            public void PersonAgreementConstructor_PersonId()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                Assert.AreEqual(personId, personAgreement.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_NullAgreementCode()
            {
                personAgreement = new PersonAgreement(id, personId, null, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_EmptyAgreementCode()
            {
                personAgreement = new PersonAgreement(id, personId, string.Empty, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            public void PersonAgreementConstructor_AgreementCode()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                Assert.AreEqual(agreementCode, personAgreement.AgreementCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_NullAgreementPeriodCode()
            {
                personAgreement = new PersonAgreement(id, personId, null, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_EmptyAgreementPeriodCode()
            {
                personAgreement = new PersonAgreement(id, personId, string.Empty, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            public void PersonAgreementConstructor_AgreementPeriodCode()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                Assert.AreEqual(agreementPeriodCode, personAgreement.AgreementPeriodCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void PersonAgreementConstructor_PersonCanDeclinePersonAgreement_Invalid()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, false, title, dueDate, text, PersonAgreementStatus.Declined, actionTimestamp);
            }

            [TestMethod]
            public void PersonAgreementConstructor_PersonCanDeclinePersonAgreement()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                Assert.AreEqual(personCanDeclinePersonAgreement, personAgreement.PersonCanDeclineAgreement);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_NullTitle()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, null, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonAgreementConstructor_EmptyTitle()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, string.Empty, dueDate, text, status, actionTimestamp);
            }

            [TestMethod]
            public void PersonAgreementConstructor_Title()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                Assert.AreEqual(title, personAgreement.Title);
            }

            [TestMethod]
            public void PersonAgreementConstructor_DueDate()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                Assert.AreEqual(dueDate, personAgreement.DueDate);
            }

            [TestMethod]
            public void PersonAgreementConstructor_Text()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                CollectionAssert.AreEqual(text, personAgreement.Text.ToList());
            }

            [TestMethod]
            public void PersonAgreementConstructor_Text_null()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, null, status, actionTimestamp);
                CollectionAssert.AreEqual(new List<string>(), personAgreement.Text.ToList());
            }

            [TestMethod]
            public void PersonAgreementConstructor_Status()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                Assert.AreEqual(status, personAgreement.Status);
            }

            [TestMethod]
            public void PersonAgreementConstructor_ActionTimestamp()
            {
                personAgreement = new PersonAgreement(id, personId, agreementCode, agreementPeriodCode, personCanDeclinePersonAgreement, title, dueDate, text, status, actionTimestamp);
                Assert.AreEqual(actionTimestamp, personAgreement.ActionTimestamp);
            }
        }
    }
}
