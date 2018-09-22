/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using System.Threading;
using System;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PayStatementConfigurationServiceTests : CurrentUserSetup
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public ICurrentUserFactory currentUserFactory;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;
        public PayStatementConfigurationService payStatementConfigurationService;
        public Mock<IHumanResourcesReferenceDataRepository> humanResourcesReferenceDataRepositoryMock;
        public PayStatementConfiguration payStatementConfig;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            currentUserFactory = new EmployeeUserFactory();
            roleRepositoryMock = new Mock<IRoleRepository>();
            cacheProviderMock = new Mock<ICacheProvider>();
            dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            loggerMock = new Mock<ILogger>();
            humanResourcesReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            var adapter = new AutoMapperAdapter<PayStatementConfiguration, Dtos.HumanResources.PayStatementConfiguration>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<PayStatementConfiguration, Dtos.HumanResources.PayStatementConfiguration>()).Returns(adapter);
            payStatementConfig = new PayStatementConfiguration();
            payStatementConfigurationService = new PayStatementConfigurationService(
                humanResourcesReferenceDataRepositoryMock.Object,
                adapterRegistryMock.Object,
                currentUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            adapterRegistryMock = null;
            currentUserFactory = null;
            roleRepositoryMock = null;
            loggerMock = null;
            cacheProviderMock = null;
            dataAccessorMock = null;
            transFactoryMock = null;
            humanResourcesReferenceDataRepositoryMock = null;
            payStatementConfigurationService = null;
            payStatementConfig = null;
        }

        [TestMethod]
        public async Task ExpectedEqualsActual()
        {
            int? expectedDaysInt = 1;
            int? expectedYearsInt = 5;

            payStatementConfig.OffsetDaysCount = expectedDaysInt;
            payStatementConfig.PreviousYearsCount = expectedYearsInt;

            humanResourcesReferenceDataRepositoryMock.Setup(repo => repo.GetPayStatementConfigurationAsync())
                .Returns(Task.FromResult(payStatementConfig));

            var actuals = await payStatementConfigurationService.GetPayStatementConfigurationAsync();

            Assert.AreEqual(expectedDaysInt, actuals.OffsetDaysCount);
            Assert.AreEqual(expectedYearsInt, actuals.PreviousYearsCount);
        }
    }
}
