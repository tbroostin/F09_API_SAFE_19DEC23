// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class CommodityCodeServiceTest
    {
        [TestClass]
        public class CommodityCodeService_Get: CurrentUserSetup
        {
            private IConfigurationRepository configurationRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IColleagueFinanceReferenceDataRepository> refRepoMock;
            private IColleagueFinanceReferenceDataRepository refRepo;
            public Mock<ILogger> loggerMock;
            private ILogger logger;
            private string commodityCodeGuid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
            private Domain.Entities.Permission permissionViewAnyPerson;

            private IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode> allCommodityCodes;
            private CommodityCodesService commodityCodeService;

            [TestInitialize]
            public void Initialize()
            {
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                refRepoMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                allCommodityCodes = new TestColleagueFinanceReferenceDataRepository().GetCommodityCodesAsync(false).Result;
                commodityCodeService = new CommodityCodesService(adapterRegistryMock.Object, configurationRepository, currentUserFactory, roleRepoMock.Object,
                    refRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                configurationRepository = null;
                refRepo = null;
                allCommodityCodes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                commodityCodeService = null;
            }

            [TestMethod]
            public async Task GetCommodityCodesService_CountCommodityCodesAsync()
            {
                refRepoMock.Setup(repo => repo.GetCommodityCodesAsync(false)).ReturnsAsync(allCommodityCodes);
                var commodityCodes = await commodityCodeService.GetCommodityCodesAsync(false);
                Assert.AreEqual(3, commodityCodes.Count());
            }

            [TestMethod]
            public async Task GetCommodityCodeServiceByGuid_CountCommodityCodesAsync()
            {
                Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityCode thisCC = allCommodityCodes.Where(i => i.Guid == commodityCodeGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetCommodityCodesAsync(true)).ReturnsAsync(allCommodityCodes.Where( i => i.Guid == commodityCodeGuid));

                var commodityCode = await commodityCodeService.GetCommodityCodeByIdAsync(commodityCodeGuid);
                Assert.AreEqual(commodityCode.Id, thisCC.Guid);
                Assert.AreEqual(commodityCode.Code, thisCC.Code);
                Assert.AreEqual(commodityCode.Description, string.Empty);
                Assert.AreEqual(commodityCode.Title, thisCC.Description);
            }
        }

        // sets up a current user
        public abstract class CurrentUserSetup
        {
            public Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory :ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }
    }
}
