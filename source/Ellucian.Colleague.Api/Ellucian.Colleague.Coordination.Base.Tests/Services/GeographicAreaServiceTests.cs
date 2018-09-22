// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class GeographicAreaServiceTests 
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

           public class PersonUserFactory : ICurrentUserFactory
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

        [TestClass]
        public class GeographicAreaService_Get : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IGeographicAreasRepository> geoRepoMock;
            private IGeographicAreasRepository geoRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.Chapter> allChapters;
            private IEnumerable<Domain.Base.Entities.County> allCounties;
            private IEnumerable<Domain.Base.Entities.ZipcodeXlat> allZipCodeXlats;
            private IEnumerable<Domain.Base.Entities.GeographicArea> allGeographicAreas;
            private IEnumerable<Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypes;
            private GeographicAreaService geographicAreaService;
            private string chapterGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";
            private string countyGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string zipCodeXlatGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize() {                
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                geoRepoMock = new Mock<IGeographicAreasRepository>();
                geoRepo = geoRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;

                allChapters = new TestGeographicAreaRepository().GetChapters();
                allCounties = new TestGeographicAreaRepository().GetCounties();
                allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();
                allGeographicAreas = new TestGeographicAreaRepository().GetGeographicAreas();

                allGeographicAreaTypes = new TestGeographicAreaRepository().Get();

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                geographicAreaService = new GeographicAreaService(adapterRegistry, refRepo, geoRepo, personRepo, currentUserFactory, roleRepo, logger, configRepo);
                refRepoMock.Setup(repo => repo.GetGeographicAreaTypesAsync(It.IsAny<bool>())).ReturnsAsync(allGeographicAreaTypes);
            }

            [TestCleanup]
            public void Cleanup() {
                refRepo = null;
                personRepo = null;
                allChapters = null;
                allCounties = null;
                allZipCodeXlats = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                geographicAreaService = null;
            }

            [TestMethod]
            public async Task GetGeographicAreaByGuid_ValidChapterGuid() {
                Ellucian.Colleague.Domain.Base.Entities.Chapter thisChapter = allChapters.Where(m => m.Guid == chapterGuid).FirstOrDefault();
                geoRepoMock.Setup(repo => repo.GetGeographicAreaByIdAsync(It.IsAny<string>())).ReturnsAsync(allGeographicAreas.FirstOrDefault(m => m.Guid == chapterGuid));
                Dtos.GeographicArea geographicArea = await geographicAreaService.GetGeographicAreaByGuidAsync(chapterGuid);
                Assert.AreEqual(thisChapter.Guid, geographicArea.Id);
                Assert.AreEqual(thisChapter.Code, geographicArea.Code);
                Assert.AreEqual(thisChapter.Description, geographicArea.Title);
                
            }

            
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetGeographicAreaByGuid_InvalidChapterGuid()
            {
                geoRepoMock.Setup(repo => repo.GetGeographicAreaByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
                await geographicAreaService.GetGeographicAreaByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetGeographicAreaByGuid_InvalidChapter()
            {
                geoRepoMock.Setup(repo => repo.GetGeographicAreaByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
                await geographicAreaService.GetGeographicAreaByGuidAsync(It.IsAny<string>());
            }


            [TestMethod]
            public async Task GetGeographicAreaByGuid_ValidCountyGuid()
            {
                County thisCounty = allCounties.Where(m => m.Guid == countyGuid).FirstOrDefault();
                geoRepoMock.Setup(repo => repo.GetGeographicAreaByIdAsync(It.IsAny<string>())).ReturnsAsync(allGeographicAreas.FirstOrDefault(m => m.Guid == countyGuid));
                Dtos.GeographicArea geographicArea = await geographicAreaService.GetGeographicAreaByGuidAsync(countyGuid);
                Assert.AreEqual(thisCounty.Guid, geographicArea.Id);
                Assert.AreEqual(thisCounty.Code, geographicArea.Code);
                Assert.AreEqual(thisCounty.Description, geographicArea.Title);
              
            }

            [TestMethod]
            public async Task GetGeographicAreaByGuid_ValidZipCodeXlatGuid()
            {
                ZipcodeXlat thisZipCodeXlat = allZipCodeXlats.Where(m => m.Guid == zipCodeXlatGuid).FirstOrDefault();
                geoRepoMock.Setup(repo => repo.GetGeographicAreaByIdAsync(It.IsAny<string>())).ReturnsAsync(allGeographicAreas.FirstOrDefault(m => m.Guid == zipCodeXlatGuid));
                Dtos.GeographicArea geographicArea = await geographicAreaService.GetGeographicAreaByGuidAsync(zipCodeXlatGuid);
                Assert.AreEqual(thisZipCodeXlat.Guid, geographicArea.Id);
                Assert.AreEqual(thisZipCodeXlat.Code, geographicArea.Code);
                Assert.AreEqual(thisZipCodeXlat.Description, geographicArea.Title);
               
            }

            [TestMethod]
            public async Task GetGeographicAreas_CountGeographicAreas()
            {
                geoRepoMock.Setup(repo => repo.GetGeographicAreasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.GeographicArea>, int>(allGeographicAreas, allGeographicAreas.Count()));
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>, int> geographicArea = await geographicAreaService.GetGeographicAreasAsync(0, 6);
                Assert.AreEqual(6, geographicArea.Item2);
            }

            [TestMethod]
            public async Task GetGeographicAreas_CompareGeographicAreas()
            {
                geoRepoMock.Setup(repo => repo.GetGeographicAreasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.GeographicArea>, int>(allGeographicAreas, allGeographicAreas.Count()));
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>, int> geographicArea = await geographicAreaService.GetGeographicAreasAsync(0, 6);
                Assert.AreEqual(allGeographicAreas.ElementAt(0).Guid, geographicArea.Item1.ElementAt(0).Id);
                Assert.AreEqual(allGeographicAreas.ElementAt(0).Code, geographicArea.Item1.ElementAt(0).Code);
                Assert.AreEqual(allGeographicAreas.ElementAt(0).Description, geographicArea.Item1.ElementAt(0).Title);

            }

        }
    }
}
