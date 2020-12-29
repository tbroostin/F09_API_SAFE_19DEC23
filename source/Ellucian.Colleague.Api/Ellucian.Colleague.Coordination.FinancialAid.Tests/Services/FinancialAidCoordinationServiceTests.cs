//Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    /// <summary>
    /// Tests class for FinancialAidCoordinationService
    /// </summary>
    [TestClass]
    public class FinancialAidCoordinationServiceTests : FinancialAidServiceTestsSetup
    {
        /// <summary>
        /// FinancialAidCoordinationServiceUnderTest utilizes methods of the abstract service class
        /// </summary>
        public class FinancialAidCoordinationServiceUnderTest : FinancialAidCoordinationService
        {
            public FinancialAidCoordinationServiceUnderTest(IConfigurationRepository configurationRepository,
                IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
                : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
            {
            }

            public bool IsSelf(string studentId) { return base.UserIsSelf(studentId); }

            public bool HasAccessPermission(string studentId, params Domain.Base.Entities.ProxyWorkflowConstants[] proxyPermissions)
            {
                return base.UserHasAccessPermission(studentId, proxyPermissions);
            }
        }

        /// <summary>
        /// Mini test role repository
        /// </summary>
        public class TestRoleRepository : IRoleRepository
        {
            public IEnumerable<Ellucian.Colleague.Domain.Entities.Role> roles = new List<Ellucian.Colleague.Domain.Entities.Role>()
            {
                new Ellucian.Colleague.Domain.Entities.Role(1, "FINANCIAL AID COUNSELOR"),                
                new Ellucian.Colleague.Domain.Entities.Role(2, "STUDENT")
            };

            public IEnumerable<Ellucian.Colleague.Domain.Entities.Role> Roles { get { return roles; } }
            public async Task<IEnumerable<Ellucian.Colleague.Domain.Entities.Role>> GetRolesAsync() { return (await Task.FromResult(new List<Ellucian.Colleague.Domain.Entities.Role>()));}
        }

        private static FinancialAidCoordinationServiceUnderTest financialAidCoordinationService;
        private TestRoleRepository testRoleRepository;

        [TestInitialize]
        public void Initialize()
        {
            BaseInitialize();
            testRoleRepository = new TestRoleRepository();
            roleRepositoryMock.Setup(r => r.Roles).Returns(testRoleRepository.roles);

                        financialAidCoordinationService = new FinancialAidCoordinationServiceUnderTest(baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object); 
           
        }

        [TestMethod]
        public void UserIsSelfReturnsTrueTest()
        {
            Assert.IsTrue(financialAidCoordinationService.IsSelf(currentUserFactory.CurrentUser.PersonId));
        }

        [TestMethod]
        public void UserIsSelfReturnsFalseTest()
        {
            Assert.IsFalse(financialAidCoordinationService.IsSelf("foo"));
        }

        [TestMethod]
        public void UserHasAccessPermission_UserIsSelfReturnsTrueTest()
        {
            Assert.IsTrue(financialAidCoordinationService.HasAccessPermission(currentUserFactory.CurrentUser.PersonId));
        }

        [TestMethod]
        public void UserHasAccessPermission_HasPermissionReturnsTrueTest()
        {
            testRoleRepository.roles.First().AddPermission(new Permission("VIEW.FINANCIAL.AID.INFORMATION"));
            Assert.IsTrue(financialAidCoordinationService.HasAccessPermission("foo"));
        }

        [TestMethod]
        public void UserHasAccessPermission_HasProxyAccessReturnsTrueTest()
        {
            currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
            financialAidCoordinationService = new FinancialAidCoordinationServiceUnderTest(baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object); 
            Assert.IsTrue(financialAidCoordinationService.HasAccessPermission("0003914"));
        }

        [TestMethod]
        public void UserHasAccessPermission_PermissionCodeMatch_HasProxyAccessReturnsTrueTest()
        {
            currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
            financialAidCoordinationService = new FinancialAidCoordinationServiceUnderTest(baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            Assert.IsTrue(financialAidCoordinationService.HasAccessPermission("0003914", new Domain.Base.Entities.ProxyWorkflowConstants[] { Domain.Base.Entities.ProxyWorkflowConstants.FinancialAidAwardLetter }));
        }

        [TestMethod]
        public void UserHasAccessPermission_PermissionCodeNoMatch_HasProxyAccessReturnsTrueTest()
        {
            currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
            financialAidCoordinationService = new FinancialAidCoordinationServiceUnderTest(baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            Assert.IsFalse(financialAidCoordinationService.HasAccessPermission("0003914", new Domain.Base.Entities.ProxyWorkflowConstants[] { Domain.Base.Entities.ProxyWorkflowConstants.FinancialAidCorrespondenceOption }));
        }

        [TestMethod]
        public void UserHasAccessPermission_ReturnsFalseTest()
        {
            Assert.IsFalse(financialAidCoordinationService.HasAccessPermission("foo"));
        }
    }
}
