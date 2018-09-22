// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PrivacyConfigurationTests
    {
        private string recordDenialMessage;
        private PrivacyConfiguration configuration;

        [TestInitialize]
        public void Initialize()
        {
            recordDenialMessage = "Record not accesible due to a privacy request";
            configuration = new PrivacyConfiguration(recordDenialMessage);
        }

        [TestCleanup]
        public void Cleanup()
        {
            recordDenialMessage = null;
            configuration = null;
        }

        [TestMethod]
        public void PrivacyConfiguration_Constructor_RecordDenialMessage()
        {
            Assert.AreEqual(recordDenialMessage, configuration.RecordDenialMessage);
        }
    }
}
