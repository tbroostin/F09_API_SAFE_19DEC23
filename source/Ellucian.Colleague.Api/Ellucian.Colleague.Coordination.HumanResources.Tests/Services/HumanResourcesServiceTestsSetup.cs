/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    public abstract class HumanResourcesServiceTestsSetup : CurrentUserSetup
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory employeeCurrentUserFactory;
        public Mock<ICurrentUserFactory> employeeCurrentUserFactoryMock;

        public Mock<IAdapterRegistry> proxyAdapterRegistryMock;
        public Mock<IRoleRepository> proxyRoleRepositoryMock;
        public Mock<ILogger> proxyLoggerMock;
        public ICurrentUserFactory proxyEmployeeCurrentUserFactory;

        public void MockInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            employeeCurrentUserFactory = new CurrentUserSetup.EmployeeUserFactory();
            employeeCurrentUserFactoryMock = new Mock<ICurrentUserFactory>();

            proxyAdapterRegistryMock = new Mock<IAdapterRegistry>();
            proxyRoleRepositoryMock = new Mock<IRoleRepository>();
            proxyLoggerMock = new Mock<ILogger>();
            proxyEmployeeCurrentUserFactory = new CurrentUserSetup.EmployeeUserFactory();
        }
    }
}
