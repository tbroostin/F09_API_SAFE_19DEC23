// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    [TestClass]
    public class FinancialAidModuleBootstapperTests
    {
        public Mock<IUnityContainer> unityContainerMock;

        public FinanceModuleBootstrapper bootstrapper;

        public RuleAdapterRegistry ruleAdapterRegistry;

        [TestInitialize]
        public void Initialize()
        {
            unityContainerMock = new Mock<IUnityContainer>();
            ruleAdapterRegistry = new RuleAdapterRegistry();
            unityContainerMock.Setup(u => u.Resolve(typeof(RuleAdapterRegistry), It.IsAny<string>(), It.IsAny<ResolverOverride[]>())).Returns(ruleAdapterRegistry);
            bootstrapper = new FinanceModuleBootstrapper();
        }

        [TestMethod]
        public void AccountHolderRuleAdapterTest()
        {
            bootstrapper.BootstrapModule(unityContainerMock.Object);

            var ruleAdapter = ruleAdapterRegistry.Get("PERSON.AR");

            Assert.IsInstanceOfType(ruleAdapter, typeof(AccountHolderRuleAdapter));
        }

        [TestMethod]
        public void InvoiceRuleAdapterTest()
        {
            bootstrapper.BootstrapModule(unityContainerMock.Object);

            var ruleAdapter = ruleAdapterRegistry.Get("AR.INVOICES");

            Assert.IsInstanceOfType(ruleAdapter, typeof(InvoiceRuleAdapter));
        }
    }

}
