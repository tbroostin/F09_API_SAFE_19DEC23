// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RetentionAlertSendEmailPreferenceTests
    {
        [TestMethod]
        public void RetentionAlertSendEmailPreferenceTests_ArgumentCheck_5()
        {
            RetentionAlertSendEmailPreference preference = new RetentionAlertSendEmailPreference(true, "message");

            Assert.AreEqual(true, preference.HasSendEmailFlag);
            Assert.AreEqual("message", preference.Message);
        }

    }
}
