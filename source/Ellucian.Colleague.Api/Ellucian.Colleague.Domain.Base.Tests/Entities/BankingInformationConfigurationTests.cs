using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class BankingInformationConfigurationTests
    {

        public BankingInformationConfiguration configuration;

        public void ConfigurationTestsInitialize()
        {
            configuration = new BankingInformationConfiguration();
        }

        [TestClass]
        public class ConstructorTests : BankingInformationConfigurationTests
        {
            [TestInitialize]
            public void Init()
            {
                base.ConfigurationTestsInitialize();
            }

            [TestMethod]
            public void ClassIsInstantiatedTest()
            {
                Assert.IsInstanceOfType(configuration, typeof(BankingInformationConfiguration));
            }

            [TestMethod]
            public void TACAreSetTest()
            {
                var tac = "terms and conditions text";
                configuration.AddEditAccountTermsAndConditions = tac;
                Assert.AreEqual(tac, configuration.AddEditAccountTermsAndConditions);
            }

            [TestMethod]
            public void PayrollMessageIsSetTest()
            {
                var tac = "blanket notify message";
                configuration.PayrollMessage = tac;
                Assert.AreEqual(tac, configuration.PayrollMessage);
            }

            [TestMethod]
            public void EffectiveDateMessageIsSetTest()
            {
                var tac = "effective date message";
                configuration.PayrollEffectiveDateMessage = tac;
                Assert.AreEqual(tac, configuration.PayrollEffectiveDateMessage);
            }

            [TestMethod]
            public void RemainderAccountRequiredIsSetTest()
            {
                configuration.IsRemainderAccountRequired = true;
                Assert.AreEqual(true, configuration.IsRemainderAccountRequired);
            }

            [TestMethod]
            public void UseFederalRoutingDirectoryIsSetTest()
            {
                configuration.UseFederalRoutingDirectory = true;
                Assert.AreEqual(true, configuration.UseFederalRoutingDirectory);
            }
        }
    }
}
