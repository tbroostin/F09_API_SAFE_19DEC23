using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class EmergencyInformationConfigurationTests
    {
        [TestMethod]
        public void EmergencyInformationConfiguration_CanSetValuesToFalse()
        {
            var emerConfig = new EmergencyInformationConfiguration(false, false, false, false);
            Assert.IsFalse(emerConfig.AllowOptOut);
            Assert.IsFalse(emerConfig.HideHealthConditions);
            Assert.IsFalse(emerConfig.HideOtherInformation);
            Assert.IsFalse(emerConfig.RequireContact);
        }

        [TestMethod]
        public void EmergencyInformationConfiguration_CanChangeValuesToTrue()
        {
            var emerConfig = new EmergencyInformationConfiguration(true, true, true, true);
            Assert.IsTrue(emerConfig.AllowOptOut);
            Assert.IsTrue(emerConfig.HideHealthConditions);
            Assert.IsTrue(emerConfig.HideOtherInformation);
            Assert.IsTrue(emerConfig.RequireContact);
        }
    }
}
