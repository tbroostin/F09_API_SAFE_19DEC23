using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class ColleagueFinanceWebConfigurationsServiceTests
    {

        protected Mock<IColleagueFinanceWebConfigurationsRepository> colleagueFinanceWebConfigurationsRepoMock;
        protected Mock<IConfigurationRepository> configurationRepoMock;
        protected IColleagueFinanceWebConfigurationsRepository colleagueFinanceWebConfigurationsRepo;
        protected IConfigurationRepository configurationRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ColleagueFinanceWebConfiguration colleagueFinanceWebConfigurationsEntity;
        protected ColleagueFinanceWebConfigurationsService colleagueFinanceWebConfigurationsService;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;
        private ICurrentUserFactory currentUserFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            
            colleagueFinanceWebConfigurationsRepoMock = new Mock<IColleagueFinanceWebConfigurationsRepository>();
            colleagueFinanceWebConfigurationsRepo = colleagueFinanceWebConfigurationsRepoMock.Object;
            configurationRepoMock = new Mock<IConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            currentUserFactory = currentUserFactoryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            logger = new Mock<ILogger>().Object;
            colleagueFinanceWebConfigurationsEntity = new Domain.ColleagueFinance.Entities.ColleagueFinanceWebConfiguration() { DefaultEmailType = "PRI", PurchasingDefaults = new PurchasingDefaults { DefaultShipToCode = "MC" } };
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            colleagueFinanceWebConfigurationsService = new ColleagueFinanceWebConfigurationsService(colleagueFinanceWebConfigurationsRepo, configurationRepo, adapterRegistry, currentUserFactory, roleRepo, logger);

        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_ValidResult()
        {
            var result = await colleagueFinanceWebConfigurationsRepo.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.DefaultEmailType, result.DefaultEmailType);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.PurchasingDefaults.DefaultShipToCode, result.PurchasingDefaults.DefaultShipToCode);
        }

    }
}
