// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class RoomCharacteristicsControllerTests
    {
        [TestClass]
        public class RoomCharacteristicsControllerGet
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

            private RoomCharacteristicsController roomCharacteristicsController;
            private Mock<IRoomCharacteristicService> roomCharacteristicService;
            ILogger logger = new Mock<ILogger>().Object;

            List<Dtos.RoomCharacteristic> roomCharacteristicsDtos = new List<RoomCharacteristic>();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                roomCharacteristicsDtos = BuildData();

                roomCharacteristicService = new Mock<IRoomCharacteristicService>();
                roomCharacteristicsController = new RoomCharacteristicsController(roomCharacteristicService.Object, logger);
                roomCharacteristicsController.Request = new HttpRequestMessage();
                roomCharacteristicsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            }

            private List<RoomCharacteristic> BuildData()
            {
                List<RoomCharacteristic> roomCharacteristicsList = new List<RoomCharacteristic>() 
                {
                    new RoomCharacteristic(){ Id = "84e13c85-3faf-42fe-8dd9-ee87621c53fd", Code = "LT", Title = "Natural Lighting"},
                    new RoomCharacteristic(){ Id = "e7bccff3-1487-4aa5-b1a8-ac0d900951a0", Code = "SM", Title = "Smoking"},
                    new RoomCharacteristic(){ Id = "623603f8-b7ef-40e5-9cd0-34b2a0de7605", Code = "MA", Title = "Male Room"},
                    new RoomCharacteristic(){ Id = "ee4cb17c-c625-49ff-bdcc-9a2bda200e5c", Code = "FE", Title = "Female Room"}
                };

                return roomCharacteristicsList;
            }

            [TestCleanup]
            public void Cleanup()
            {
                roomCharacteristicsController = null;
                roomCharacteristicService = null;
                logger = null;
            }

            [TestMethod]
            public async Task RoomCharacteristicsController_GetRoomCharacteristicsAsync()
            {
                roomCharacteristicService.Setup(gc => gc.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roomCharacteristicsDtos);

                var result = await roomCharacteristicsController.GetRoomCharacteristicsAsync();
                Assert.AreEqual(4, roomCharacteristicsDtos.Count());                
            }

            [TestMethod]
            public async Task RoomCharacteristicsController_GetRoomCharacteristicsAsync_True()
            {
                roomCharacteristicService.Setup(gc => gc.GetRoomCharacteristicsAsync(true)).ReturnsAsync(roomCharacteristicsDtos);
                roomCharacteristicsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                var result = await roomCharacteristicsController.GetRoomCharacteristicsAsync();
                Assert.AreEqual(4, roomCharacteristicsDtos.Count());
            }

            [TestMethod]
            public async Task RoomCharacteristicsController_GetRoomCharacteristicsAsync_False()
            {
                roomCharacteristicService.Setup(gc => gc.GetRoomCharacteristicsAsync(false)).ReturnsAsync(roomCharacteristicsDtos);
                roomCharacteristicsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                var result = await roomCharacteristicsController.GetRoomCharacteristicsAsync();
                Assert.AreEqual(4, roomCharacteristicsDtos.Count());
            }

            [TestMethod]
            public async Task RoomCharacteristicsController_GetRoomCharacteristicByIdAsync()
            {
                string id = "ee4cb17c-c625-49ff-bdcc-9a2bda200e5c";
                var roomCharacteristic = roomCharacteristicsDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

                roomCharacteristicService.Setup(gc => gc.GetRoomCharacteristicByGuidAsync(id)).ReturnsAsync(roomCharacteristic);

                var result = await roomCharacteristicsController.GetRoomCharacteristicByIdAsync(id);

                Assert.AreEqual(roomCharacteristic.Code, result.Code);
                Assert.AreEqual(roomCharacteristic.Id, result.Id);
                Assert.AreEqual(roomCharacteristic.Title, result.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RoomCharacteristicsController_GetRoomCharacteristicsAsync_Exception()
            {
                roomCharacteristicService.Setup(gc => gc.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var result = await roomCharacteristicsController.GetRoomCharacteristicsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RoomCharacteristicsController_GetRoomCharacteristicByIdAsync_ArgumentNullException()
            {
                var result = await roomCharacteristicsController.GetRoomCharacteristicByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RoomCharacteristicsController_GetRoomCharacteristicByIdAsync_KeyNotFoundException()
            {
                roomCharacteristicService.Setup(gc => gc.GetRoomCharacteristicByGuidAsync("1234")).ThrowsAsync(new KeyNotFoundException());
                var result = await roomCharacteristicsController.GetRoomCharacteristicByIdAsync("1234");
            }
            
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RoomCharacteristicsController_GetRoomCharacteristicByIdAsync_Exception()
            {
                roomCharacteristicService.Setup(gc => gc.GetRoomCharacteristicByGuidAsync("123")).ThrowsAsync(new Exception());
                var result = await roomCharacteristicsController.GetRoomCharacteristicByIdAsync("123");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RoomCharacteristicsController_PUT_Exception()
            {
                var result = await roomCharacteristicsController.PutRoomCharacteristicAsync(It.IsAny<RoomCharacteristic>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RoomCharacteristicsController_POST_Exception()
            {
                var result = await roomCharacteristicsController.PostRoomCharacteristicAsync(It.IsAny<RoomCharacteristic>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RoomCharacteristicsController_DELETE_Exception()
            {
                await roomCharacteristicsController.DeleteRoomCharacteristicAsync(It.IsAny<string>());
            }
        }
    }
}