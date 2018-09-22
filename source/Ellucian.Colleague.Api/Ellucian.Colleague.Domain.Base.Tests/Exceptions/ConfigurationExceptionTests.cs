// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Exceptions
{
    [TestClass]
    public class ConfigurationExceptionTests
    {
        [TestMethod]
        public void ConfigurationException_Default()
        {
            var ce = new ConfigurationException();
            Assert.AreEqual("Configuration exception", ce.Message);
        }

        [TestMethod]
        public void ConfigurationException_Message()
        {
            var ce = new ConfigurationException("Configuration is not complete.");
            Assert.AreEqual("Configuration is not complete.", ce.Message);
        }

        [TestMethod]
        public void ConfigurationException_MessageInnerException()
        {
            var ex = new Exception("Required configuration field not specified.");
            var ce = new ConfigurationException("Configuration is not complete.", ex);
            Assert.AreEqual("Configuration is not complete.", ce.Message);
            Assert.AreEqual("Required configuration field not specified.", ce.InnerException.Message);
        }
    }
}
