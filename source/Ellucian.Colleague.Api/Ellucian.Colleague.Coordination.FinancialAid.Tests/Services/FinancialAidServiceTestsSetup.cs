using Ellucian.Colleague.Domain.Base.Repositories;
//Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    public class FinancialAidServiceTestsSetup : CurrentUserSetup
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory currentUserFactory;
        public IConfigurationRepository baseConfigurationRepository;
        public Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public void BaseInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            currentUserFactory = new CurrentUserSetup.StudentUserFactory();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
        }

        public void BaseCleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            roleRepositoryMock = null;
            currentUserFactory = null;
            baseConfigurationRepositoryMock = null;
            baseConfigurationRepository = null;
        }
    }
}
