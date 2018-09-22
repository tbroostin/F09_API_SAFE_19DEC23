// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class SelfServiceConfigurationTests
    {
        [TestMethod]
        public void SelfServiceConfiguration_Constructor_True()
        {
            bool alwaysUseClipboardForBulkMailToLinks = true;
            var entity = new SelfServiceConfiguration(alwaysUseClipboardForBulkMailToLinks);
            Assert.IsTrue(entity.AlwaysUseClipboardForBulkMailToLinks);
        }

        [TestMethod]
        public void SelfServiceConfiguration_Constructor_False()
        {
            bool alwaysUseClipboardForBulkMailToLinks = false;
            var entity = new SelfServiceConfiguration(alwaysUseClipboardForBulkMailToLinks);
            Assert.IsFalse(entity.AlwaysUseClipboardForBulkMailToLinks);
        }
    }
}
