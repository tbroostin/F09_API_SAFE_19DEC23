// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationConfigurationTests
    {


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegistrationConfigurationConstructor_NegativeOffset()
        {
            var config = new RegistrationConfiguration(true, -1);

        }

        [TestMethod]
        public void RegistrationConfigurationConstructor_Success()
        {
            var config = new RegistrationConfiguration(true, 0);
            Assert.IsTrue(config.RequireFacultyAddAuthorization);
            Assert.AreEqual(0, config.AddAuthorizationStartOffsetDays);
            Assert.IsFalse(config.PromptForDropReason);
            Assert.IsFalse(config.RequireDropReason);

        }

        [TestMethod]
        public void RegistrationConfigurationConstructor_Success2()
        {
            var config = new RegistrationConfiguration(false, 9);
            config.PromptForDropReason = true;
            config.RequireDropReason = true;
            Assert.IsFalse (config.RequireFacultyAddAuthorization);
            Assert.AreEqual(9, config.AddAuthorizationStartOffsetDays);
            Assert.IsTrue(config.PromptForDropReason);
            Assert.IsTrue(config.RequireDropReason);

        }


    }
}

