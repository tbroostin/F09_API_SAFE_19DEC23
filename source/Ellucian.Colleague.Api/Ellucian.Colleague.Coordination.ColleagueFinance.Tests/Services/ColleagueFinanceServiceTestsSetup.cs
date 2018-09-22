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
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    public abstract class ColleagueFinanceServiceTestsSetup : GeneralLedgerCurrentUser
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory GLCurrentUserFactory;

        public void MockInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            GLCurrentUserFactory = new GeneralLedgerCurrentUser.UserFactorySubset();

        }
    }
}
