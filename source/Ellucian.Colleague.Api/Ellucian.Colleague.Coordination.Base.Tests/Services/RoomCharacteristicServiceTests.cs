// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class RoomCharacteristicServiceTests
    {
        [TestClass]
        public class RoomCharacteristicServiceTests_Get : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            public Mock<ILogger> loggerMock;
            private IConfigurationRepository _configurationRepository;
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private IEnumerable<Domain.Base.Entities.RoomCharacteristic> roomCharacteristicEntities;
            private IEnumerable<Dtos.RoomCharacteristic> roomCharacteristicDtos;

            private RoomCharacteristicService roomCharacteristicService;
            private string roomTypesGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepoMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                _configurationRepository = _configurationRepositoryMock.Object;

                BuildData();

                adapterRegistryMock = new Mock<IAdapterRegistry>();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                roomCharacteristicService = new RoomCharacteristicService( adapterRegistryMock.Object, referenceDataRepositoryMock.Object, currentUserFactory, _configurationRepository, roleRepoMock.Object, loggerMock.Object);
            }

            private void BuildData()
            {
                roomCharacteristicEntities = new List<Domain.Base.Entities.RoomCharacteristic>() 
                {
                    new Domain.Base.Entities.RoomCharacteristic("84e13c85-3faf-42fe-8dd9-ee87621c53fd", "LT", "Natural Lighting"),
                    new Domain.Base.Entities.RoomCharacteristic("e7bccff3-1487-4aa5-b1a8-ac0d900951a0", "SM", "Smoking"),
                    new Domain.Base.Entities.RoomCharacteristic("623603f8-b7ef-40e5-9cd0-34b2a0de7605", "MA", "Male Room"),
                    new Domain.Base.Entities.RoomCharacteristic("ee4cb17c-c625-49ff-bdcc-9a2bda200e5c", "FE", "Female Room")
                };
                roomCharacteristicDtos = new List<Dtos.RoomCharacteristic>() 
                {
                    new Dtos.RoomCharacteristic(){ Id = "84e13c85-3faf-42fe-8dd9-ee87621c53fd", Code = "LT", Title = "Natural Lighting"},
                    new Dtos.RoomCharacteristic(){ Id = "e7bccff3-1487-4aa5-b1a8-ac0d900951a0", Code = "SM", Title = "Smoking"},
                    new Dtos.RoomCharacteristic(){ Id = "623603f8-b7ef-40e5-9cd0-34b2a0de7605", Code = "MA", Title = "Male Room"},
                    new Dtos.RoomCharacteristic(){ Id = "ee4cb17c-c625-49ff-bdcc-9a2bda200e5c", Code = "FE", Title = "Female Room"}
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                referenceDataRepositoryMock = null;
                roomCharacteristicEntities = null;
                adapterRegistryMock = null;
                roleRepoMock = null;
                loggerMock = null;
                roomCharacteristicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task RoomCharacteristicService_GetRoomCharacteristicsAsync()
            {
                referenceDataRepositoryMock.Setup(i => i.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roomCharacteristicEntities);
                var results = await roomCharacteristicService.GetRoomCharacteristicsAsync(It.IsAny<bool>());
                Assert.IsNotNull(results);
                Assert.AreEqual(4, results.Count());

                foreach (var actual in results)
                {
                    var expected = roomCharacteristicEntities.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Title);
                }
            }

            [TestMethod]
            public async Task RoomCharacteristicService_GetRoomCharacteristicsAsync_Null()
            {
                referenceDataRepositoryMock.Setup(i => i.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(new List<Domain.Base.Entities.RoomCharacteristic>());
                var results = await roomCharacteristicService.GetRoomCharacteristicsAsync(It.IsAny<bool>());
                Assert.IsNull(results);
            }

            [TestMethod]
            public async Task RoomCharacteristicService_GetRoomCharacteristicByGuidAsync()
            {
                string id = "ee4cb17c-c625-49ff-bdcc-9a2bda200e5c";
                referenceDataRepositoryMock.Setup(i => i.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roomCharacteristicEntities);
                var expected = roomCharacteristicEntities.FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));

                var actual = await roomCharacteristicService.GetRoomCharacteristicByGuidAsync(id);

                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RoomCharacteristicService_GetRoomCharacteristicByGuidAsync_ArgumentNullException()
            {
                var actual = await roomCharacteristicService.GetRoomCharacteristicByGuidAsync("");  
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RoomCharacteristicService_GetRoomCharacteristicByGuidAsync_KeyNotFoundException()
            {
                var actual = await roomCharacteristicService.GetRoomCharacteristicByGuidAsync("1234");
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