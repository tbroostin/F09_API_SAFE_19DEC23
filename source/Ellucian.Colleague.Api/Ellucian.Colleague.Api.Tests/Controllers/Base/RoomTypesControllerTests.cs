// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class RoomTypesControllerTests
    {
        [TestClass]
        public class RoomTypesControllerGet
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RoomTypesController roomTypesController;
            private Mock<IReferenceDataRepository> refRepositoryMock;
            private IReferenceDataRepository refRepository;

            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RoomTypes> allRoomTypes;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IRoomTypesService> roomTypesServiceMock;
            private IRoomTypesService roomTypesService;
            private string roomTypesGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

            List<Dtos.RoomTypes> allRoomTypesList;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                refRepositoryMock = new Mock<IReferenceDataRepository>();
                refRepository = refRepositoryMock.Object;

                roomTypesServiceMock = new Mock<IRoomTypesService>();
                roomTypesService = roomTypesServiceMock.Object;

                allRoomTypes = new TestRoomTypesRepository().Get();
                allRoomTypesList = new List<Dtos.RoomTypes>();

                roomTypesController = new RoomTypesController(roomTypesService, logger);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.RoomTypes, Dtos.RoomTypes>();

                roomTypesController.Request = new HttpRequestMessage();

                roomTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var roomType in allRoomTypes)
                {
                    Dtos.RoomTypes target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.RoomTypes, Dtos.RoomTypes>(roomType);
                    target.Id = roomType.Guid;

                    allRoomTypesList.Add(target);
                }

                refRepositoryMock.Setup(repo => repo.GetRoomTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRoomTypes);
                refRepositoryMock.Setup(repo => repo.RoomTypesAsync()).ReturnsAsync(allRoomTypes);
            }

            [TestCleanup]
            public void Cleanup()
            {
                roomTypesController = null;
                refRepository = null;
                roomTypesService = null;
                allRoomTypes = null;
                allRoomTypesList = null;
            }

            [TestMethod]
            public async Task GetRoomTypesByGuidAsync_Validate()
            {
                var thisRoomType = allRoomTypesList.Where(m => m.Id == roomTypesGuid).FirstOrDefault();

                roomTypesServiceMock.Setup(x => x.GetRoomTypesByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisRoomType);

                var roomType = await roomTypesController.GetRoomTypeByIdAsync(roomTypesGuid);
                Assert.AreEqual(thisRoomType.Id, roomType.Id);
                Assert.AreEqual(thisRoomType.Code, roomType.Code);
                Assert.AreEqual(thisRoomType.Description, roomType.Description);
                Assert.AreEqual(thisRoomType.Type, roomType.Type);
            }

            [TestMethod]
            public async Task RoomTypesController_GetHedmAsync()
            {
                roomTypesServiceMock.Setup(gc => gc.GetRoomTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRoomTypesList);

                var result = await roomTypesController.GetRoomTypesAsync();
                Assert.AreEqual(result.Count(), allRoomTypes.Count());

                int count = allRoomTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allRoomTypesList[i];
                    var actual = allRoomTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            [TestMethod]
            public async Task RoomTypesController_GetHedmAsync_CacheControlNotNull()
            {
                roomTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                roomTypesServiceMock.Setup(gc => gc.GetRoomTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRoomTypesList);

                var result = await roomTypesController.GetRoomTypesAsync();
                Assert.AreEqual(result.Count(), allRoomTypes.Count());

                int count = allRoomTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allRoomTypesList[i];
                    var actual = allRoomTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            [TestMethod]
            public async Task RoomTypesController_GetHedmAsync_NoCache()
            {
                roomTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                roomTypesController.Request.Headers.CacheControl.NoCache = true;

                roomTypesServiceMock.Setup(gc => gc.GetRoomTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRoomTypesList);

                var result = await roomTypesController.GetRoomTypesAsync();
                Assert.AreEqual(result.Count(), allRoomTypes.Count());

                int count = allRoomTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allRoomTypesList[i];
                    var actual = allRoomTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            [TestMethod]
            public async Task RoomTypesController_GetHedmAsync_Cache()
            {
                roomTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                roomTypesController.Request.Headers.CacheControl.NoCache = false;

                roomTypesServiceMock.Setup(gc => gc.GetRoomTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRoomTypesList);

                var result = await roomTypesController.GetRoomTypesAsync();
                Assert.AreEqual(result.Count(), allRoomTypes.Count());

                int count = allRoomTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allRoomTypesList[i];
                    var actual = allRoomTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            [TestMethod]
            public async Task RoomTypesController_GetByIdHedmAsync()
            {
                var thisRoomType = allRoomTypesList.Where(m => m.Id == "9ae3a175-1dfd-4937-b97b-3c9ad596e023").FirstOrDefault();

                roomTypesServiceMock.Setup(x => x.GetRoomTypesByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisRoomType);

                var roomType = await roomTypesController.GetRoomTypeByIdAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
                Assert.AreEqual(thisRoomType.Id, roomType.Id);
                Assert.AreEqual(thisRoomType.Code, roomType.Code);
                Assert.AreEqual(thisRoomType.Description, roomType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RoomTypeController_GetThrowsIntAppiExc()
            {
                roomTypesServiceMock.Setup(gc => gc.GetRoomTypesAsync(It.IsAny<bool>())).Throws<Exception>();

                await roomTypesController.GetRoomTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RoomTypeController_GetByIdThrowsIntAppiExc()
            {
                roomTypesServiceMock.Setup(gc => gc.GetRoomTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await roomTypesController.GetRoomTypeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void RoomTypeController_PostThrowsIntAppiExc()
            {
                roomTypesController.PostRoomType(allRoomTypesList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void RoomTypeController_PutThrowsIntAppiExc()
            {
                var result = roomTypesController.PutRoomType(allRoomTypesList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void RoomTypeController_DeleteThrowsIntAppiExc()
            {
                var result = roomTypesController.DeleteRoomType("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }
        }
    }
}