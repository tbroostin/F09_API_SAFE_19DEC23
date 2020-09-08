// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RetentionAlertWorkCaseSetReminderTests
    {

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RetentionAlertWorkCaseSetReminderTests_ArgumentCheck_1()
        {
            RetentionAlertWorkCaseSetReminder reminder = new RetentionAlertWorkCaseSetReminder("1234567", null, null, null);
         }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RetentionAlertWorkCaseSetReminderTests_ArgumentCheck_2()
        {
            RetentionAlertWorkCaseSetReminder reminder = new RetentionAlertWorkCaseSetReminder("1234567", new DateTime(2019, 01, 01), null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RetentionAlertWorkCaseSetReminderTests_ArgumentCheck_3()
        {
            RetentionAlertWorkCaseSetReminder reminder = new RetentionAlertWorkCaseSetReminder("1234567", new DateTime(2019, 01, 01), "", null);
        }

        [TestMethod]
        public void RetentionAlertWorkCaseSetReminderTests_Success_1()
        {
            string updatedBy = "1234567";
            DateTime reminderDate = new DateTime(2019, 01, 01);
            string summary = "Test Summary";
            List<string> notes = null;
            RetentionAlertWorkCaseSetReminder reminder = new RetentionAlertWorkCaseSetReminder(updatedBy, reminderDate, summary, notes);

            Assert.AreEqual(updatedBy, reminder.UpdatedBy);
            Assert.AreEqual(reminderDate, reminder.ReminderDate);
            Assert.AreEqual(summary, reminder.Summary);
        }

        [TestMethod]
        public void RetentionAlertWorkCaseSetReminderTests_Success_2()
        {
            string updatedBy = "1234567";
            DateTime reminderDate = new DateTime(2019, 01, 01);
            string summary = "Test Summary";
            List<string> notes = new List<string>();
            RetentionAlertWorkCaseSetReminder reminder = new RetentionAlertWorkCaseSetReminder(updatedBy, reminderDate, summary, notes);

            Assert.AreEqual(updatedBy, reminder.UpdatedBy);
            Assert.AreEqual(reminderDate, reminder.ReminderDate);
            Assert.AreEqual(summary, reminder.Summary);
            Assert.AreEqual(notes, reminder.Notes);
        }

        [TestMethod]
        public void RetentionAlertWorkCaseSetReminderTests_Success_3()
        {
            string updatedBy = "1234567";
            DateTime reminderDate = new DateTime(2019, 01, 01);
            string summary = "Test Summary";
            List<string> notes = new List<string>() { "1" };
            RetentionAlertWorkCaseSetReminder reminder = new RetentionAlertWorkCaseSetReminder(updatedBy, reminderDate, summary, notes);

            Assert.AreEqual(updatedBy, reminder.UpdatedBy);
            Assert.AreEqual(reminderDate, reminder.ReminderDate);
            Assert.AreEqual(summary, reminder.Summary);
            Assert.AreEqual(notes, reminder.Notes);
        }

        [TestMethod]
        public void RetentionAlertWorkCaseSetReminderTests_Success_4()
        {
            string updatedBy = "1234567";
            DateTime reminderDate = new DateTime(2019, 01, 01);
            string summary = "Test Summary";
            List<string> notes = new List<string>() { "1", "2" };
            RetentionAlertWorkCaseSetReminder reminder = new RetentionAlertWorkCaseSetReminder(updatedBy, reminderDate, summary, notes);

            Assert.AreEqual(updatedBy, reminder.UpdatedBy);
            Assert.AreEqual(reminderDate, reminder.ReminderDate);
            Assert.AreEqual(summary, reminder.Summary);
            Assert.AreEqual(notes, reminder.Notes);
        }
    }
}
