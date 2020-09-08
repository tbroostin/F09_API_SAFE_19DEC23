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
    public class RetentionAlertWorkCaseManageRemindersTests
    {

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RetentionAlertWorkCaseManageReminders_ArgumentCheck_1()
        {
            RetentionAlertWorkCaseManageReminders reminders = new RetentionAlertWorkCaseManageReminders(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RetentionAlertWorkCaseManageReminders_ArgumentCheck_2()
        {
            RetentionAlertWorkCaseManageReminders reminders = new RetentionAlertWorkCaseManageReminders("", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RetentionAlertWorkCaseManageReminders_ArgumentCheck_3()
        {
            RetentionAlertWorkCaseManageReminders reminders = new RetentionAlertWorkCaseManageReminders("1", null);
        }

        [TestMethod]
        public void RetentionAlertWorkCaseManageReminders_ArgumentCheck_4()
        {
            RetentionAlertWorkCaseManageReminders reminders = new RetentionAlertWorkCaseManageReminders("1", 
                new List<RetentionAlertWorkCaseManageReminder>() { 
                    new RetentionAlertWorkCaseManageReminder()
                    {
                        CaseItemsId = "1",
                        ClearReminderDateFlag = "Y"
                    }
                });

            Assert.AreEqual("1", reminders.UpdatedBy);
            Assert.AreEqual("1", reminders.Reminders.ToList()[0].CaseItemsId);
            Assert.AreEqual("Y", reminders.Reminders.ToList()[0].ClearReminderDateFlag);
        }

    }
}
