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
    public class RoomTypesServiceTests
    {
        [TestClass]
        public class RoomTypesService_Get : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            public Mock<ILogger> loggerMock;
            private ILogger logger;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private IEnumerable<Domain.Base.Entities.RoomTypes> allRoomTypes;
            private IEnumerable<Domain.Base.Entities.RoomTypes> allRoomTypes2;
            private RoomTypesService roomTypesService;
            private string roomTypesGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {

                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                allRoomTypes = new TestRoomTypesRepository().Get();
                allRoomTypes2 = new TestRoomTypesRepository().GetRoomTypes();

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                roomTypesService = new RoomTypesService(adapterRegistry, refRepo, baseConfigurationRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                allRoomTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                roomTypesService = null;
            }

            [TestMethod]
            public async Task GetRoomTypesByGuidAsync_CompareRoomTypesAsync()
            {
                Ellucian.Colleague.Domain.Base.Entities.RoomTypes thisRoomType = allRoomTypes.Where(m => m.Guid == roomTypesGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes.Where(m => m.Guid == roomTypesGuid));
                var roomType = await roomTypesService.GetRoomTypesByGuidAsync(roomTypesGuid);
                Assert.AreEqual(thisRoomType.Guid, roomType.Id);
                Assert.AreEqual(thisRoomType.Code, roomType.Code);
                Assert.AreEqual(thisRoomType.Description, roomType.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Classroom, roomType.Type);
            }


            [TestMethod]
            public async Task GetRoomTypesAsync_CountRoomTypesAsync()
            {
                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(false)).ReturnsAsync(allRoomTypes);
                var roomTypes = await roomTypesService.GetRoomTypesAsync();
                Assert.AreEqual(2, roomTypes.Count());
            }

            [TestMethod]
            public async Task GetRoomTypesAsync_CompareRoomTypesAsync_Cache()
            {
                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(false)).ReturnsAsync(allRoomTypes);
                var roomTypes = await roomTypesService.GetRoomTypesAsync();
                Assert.AreEqual(allRoomTypes.ElementAt(0).Guid, roomTypes.ElementAt(0).Id);
                Assert.AreEqual(allRoomTypes.ElementAt(0).Code, roomTypes.ElementAt(0).Code);
                Assert.AreEqual(allRoomTypes.ElementAt(0).Description, roomTypes.ElementAt(0).Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Classroom, roomTypes.ElementAt(0).Type);
            }

            [TestMethod]
            public async Task GetRoomTypesAsync_CompareRoomTypesAsync_NoCache()
            {
                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes);
                var roomTypes = await roomTypesService.GetRoomTypesAsync(true);
                Assert.AreEqual(allRoomTypes.ElementAt(0).Guid, roomTypes.ElementAt(0).Id);
                Assert.AreEqual(allRoomTypes.ElementAt(0).Code, roomTypes.ElementAt(0).Code);
                Assert.AreEqual(allRoomTypes.ElementAt(0).Description, roomTypes.ElementAt(0).Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Classroom, roomTypes.ElementAt(0).Type);
            }

            [ExpectedException(typeof(InvalidOperationException))]
            [TestMethod]
            public async Task GetRoomTypesByGuidAsync_EmptyAsync()
            {
                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(false)).ReturnsAsync(allRoomTypes);
                await roomTypesService.GetRoomTypesByGuidAsync(null);
            }

            [ExpectedException(typeof(InvalidOperationException))]
            [TestMethod]
            public async Task GetRoomTypesByGuidAsync_InvalidAsync()
            {
                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes);
                await roomTypesService.GetRoomTypesByGuidAsync("INVALID");
            }
            
            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public async Task GetRoomTypesByGuidAsync_KeyNotFound()
            {
                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).Throws<KeyNotFoundException>();
                await roomTypesService.GetRoomTypesByGuidAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            [ExpectedException(typeof(Exception))]
            [TestMethod]
            public async Task GetRoomTypesByGuidAsync_ThrowsExc()
            {
                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).Throws<Exception>();
                await roomTypesService.GetRoomTypesByGuidAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Amphitheater()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Amphitheater;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Amphitheater, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_AnimalQuarters()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Animalquarters;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Animalquarters, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Apartment()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Apartment;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Apartment, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Artstudio()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Artstudio;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Artstudio, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Atrium()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Atrium;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Atrium, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Audiovisuallab()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Audiovisuallab;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Audiovisuallab, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Auditorium()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Auditorium;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Auditorium, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Ballroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Ballroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Ballroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Booth()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Booth;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Booth, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Classroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Classroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Classroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Clinic()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Clinic;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Clinic, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Computerlaboratory()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Computerlaboratory;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Computerlaboratory, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Conferenceroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Conferenceroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Conferenceroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Daycare()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Daycare;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Daycare, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Foodfacility()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Foodfacility;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Foodfacility, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Generalusefacility()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Generalusefacility;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Generalusefacility, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Greenhouse()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Greenhouse;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Greenhouse, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Healthcarefacility()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Healthcarefacility;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Healthcarefacility, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_House()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.House;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.House, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Lecturehall()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Lecturehall;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Lecturehall, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Lounge()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Lounge;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Lounge, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Mechanicslab()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Mechanicslab;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Mechanicslab, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Merchandisingroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Merchandisingroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Merchandisingroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Musicroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Musicroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Musicroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Office()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Office;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Office, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Other()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Other;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Other, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Performingartstudio()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Performingartsstudio;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Performingartsstudio, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Residencehallroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Residencehallroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Residencehallroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Residencedoubleroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Residentialdoubleroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Residentialdoubleroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Residentialsingleroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Residentialsingleroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Residentialsingleroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Residentialsuiteroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Residentialsuiteroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Residentialsuiteroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Residentialtripleroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Residentialtripleroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Residentialtripleroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Sciencelaboratory()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Sciencelaboratory;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Sciencelaboratory, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Seminarroom()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Seminarroom;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Seminarroom, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Specialusefacility()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Specialusefacility;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Specialusefacility, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Studyfacility()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Studyfacility;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Studyfacility, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Supportfacility()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Supportfacility;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Supportfacility, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Null()
            {
                var roomTypes = new Ellucian.Colleague.Domain.Base.Entities.RoomTypes("9ae3a175-1dfd-4937-b97b-3c9ad596e023", "110", "Lecture Hall", null);

                var roomType = roomTypes.Type;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Other, roomTypeItem.Type);
            }

            [TestMethod]
            public async Task GetRoomTypesItemByGuid_RoomTypeType_Theater()
            {
                var roomType = Ellucian.Colleague.Domain.Base.Entities.RoomType.Theater;
                var thisroomType = allRoomTypes2.Where(x => x.Type == roomType).FirstOrDefault();

                refRepoMock.Setup(repo => repo.GetRoomTypesAsync(true)).ReturnsAsync(allRoomTypes2.Where(x => x.Type == roomType));
                var roomTypeItem = (await roomTypesService.GetRoomTypesAsync(true)).FirstOrDefault();

                Assert.AreEqual(thisroomType.Guid, roomTypeItem.Id);
                Assert.AreEqual(thisroomType.Code, roomTypeItem.Code);
                Assert.AreEqual(thisroomType.Description, roomTypeItem.Title);
                Assert.AreEqual(Dtos.RoomTypeTypes.Other, roomTypeItem.Type);
            }
        }

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
    }
}