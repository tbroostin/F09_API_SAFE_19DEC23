// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class CommodityUnitTypeServiceTest
    {
        [TestClass]
        public class CommodityUnitTypeService_Get: CurrentUserSetup
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
            private string commodityUnitYTypeGuid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
            private Domain.Entities.Permission permissionViewAnyPerson;

            private IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType> allCommodityUnitTypes;
            private CommodityUnitTypesService commodityUnitTypeService;

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

                allCommodityUnitTypes = new TestColleagueFinanceReferenceDataRepository().GetCommodityUnitTypesAsync(false).Result;
                commodityUnitTypeService = new CommodityUnitTypesService(adapterRegistry, currentUserFactory, roleRepo, configurationRepository, refRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                configurationRepository = null;
                refRepo = null;
                allCommodityUnitTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                commodityUnitTypeService = null;
            }

            [TestMethod]
            public async Task GetCommodityUnitTypesService_CountCommodityUnitTypesAsync()
            {
                refRepoMock.Setup(repo => repo.GetCommodityUnitTypesAsync(false)).ReturnsAsync(allCommodityUnitTypes);
                var commodityUnitTypes = await commodityUnitTypeService.GetCommodityUnitTypesAsync(false);
                Assert.AreEqual(3, commodityUnitTypes.Count());
            }

            [TestMethod]
            public async Task GetCommodityUnitTypeServiceByGuid_CountCommodityUnitTypesAsync()
            {
                Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityUnitType thisCUT = allCommodityUnitTypes.Where(i => i.Guid == commodityUnitYTypeGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetCommodityUnitTypesAsync(true)).ReturnsAsync(allCommodityUnitTypes.Where( i => i.Guid == commodityUnitYTypeGuid));

                var commodityUnitType = await commodityUnitTypeService.GetCommodityUnitTypeByIdAsync(commodityUnitYTypeGuid);
                Assert.AreEqual(commodityUnitType.Id, thisCUT.Guid);
                Assert.AreEqual(commodityUnitType.Code, thisCUT.Code);
                Assert.AreEqual(commodityUnitType.Description, string.Empty);
                Assert.AreEqual(commodityUnitType.Title, thisCUT.Description);
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
