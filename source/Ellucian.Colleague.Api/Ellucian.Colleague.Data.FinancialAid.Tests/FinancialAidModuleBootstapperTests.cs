using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base;
using Ellucian.Web.Http.Bootstrapping;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.FinancialAid.Tests
{
    [TestClass]
    public class FinancialAidModuleBootstapperTests
    {
        public Mock<IUnityContainer> unityContainerMock;

        public FinancialAidModuleBootstrapper bootstrapper;

        public RuleAdapterRegistry ruleAdapterRegistry;

        [TestInitialize]
        public void Initialize()
        {
            unityContainerMock = new Mock<IUnityContainer>();
            ruleAdapterRegistry = new RuleAdapterRegistry();


            unityContainerMock.Setup(u => u.Resolve(typeof(RuleAdapterRegistry), It.IsAny<string>(), It.IsAny<ResolverOverride[]>())).Returns(ruleAdapterRegistry);

            bootstrapper = new FinancialAidModuleBootstrapper();
        }

        [TestMethod]
        public void StudentAwardYearNeedsAnalysisRuleAdapterTest()
        {
            bootstrapper.BootstrapModule(unityContainerMock.Object);

            var ruleAdapter = ruleAdapterRegistry.Get("CS.ACYR");

            Assert.IsInstanceOfType(ruleAdapter, typeof(StudentAwardYearNeedsAnalysisRuleAdapter));
        }
    }

}
