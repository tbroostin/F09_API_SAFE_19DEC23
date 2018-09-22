//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class RoomRatesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IRoomRatesService> roomRatesServiceMock;
        private Mock<ILogger> loggerMock;
        private RoomRatesController roomRatesController;      
        private IEnumerable<RoomRate> allRoomRates;
        private List<Dtos.RoomRates> roomRatesCollection;

        [TestInitialize]
        public async void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            roomRatesServiceMock = new Mock<IRoomRatesService>();
            loggerMock = new Mock<ILogger>();
            roomRatesCollection = new List<Dtos.RoomRates>();

            allRoomRates  = new List<RoomRate>()
                {
                    new RoomRate("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new RoomRate("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new RoomRate("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allRoomRates)
            {
                var roomRates = new Ellucian.Colleague.Dtos.RoomRates
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                roomRatesCollection.Add(roomRates);
            }

            roomRatesController = new RoomRatesController(roomRatesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            roomRatesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            roomRatesController = null;
            allRoomRates = null;
            roomRatesCollection = null;
            loggerMock = null;
            roomRatesServiceMock = null;
            referenceDataRepositoryMock = null;
            adapterRegistryMock = null;
        }

        [TestMethod]
        public async Task RoomRatesController_GetRoomRates_ValidateFields_Nocache()
        {
            roomRatesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            roomRatesServiceMock.Setup(x => x.GetRoomRatesAsync(false)).ReturnsAsync(roomRatesCollection);
       
            var sourceContexts = (await roomRatesController.GetRoomRatesAsync()).ToList();
            Assert.AreEqual(roomRatesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = roomRatesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());

            }
        }

        [TestMethod]
        public async Task RoomRatesController_GetRoomRates_ValidateFields_Cache()
        {
            roomRatesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            roomRatesServiceMock.Setup(x => x.GetRoomRatesAsync(true)).ReturnsAsync(roomRatesCollection);

            var sourceContexts = (await roomRatesController.GetRoomRatesAsync()).ToList();
            Assert.AreEqual(roomRatesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = roomRatesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task RoomRatesController_GetRoomRatesByGuidAsync_ValidateFields()
        {
            var expected = roomRatesCollection.FirstOrDefault();
            roomRatesServiceMock.Setup(x => x.GetRoomRatesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await roomRatesController.GetRoomRatesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomRatesController_GetRoomRates_Exception()
        {
            roomRatesServiceMock.Setup(x => x.GetRoomRatesAsync(false)).Throws<Exception>();
            await roomRatesController.GetRoomRatesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomRatesController_GetRoomRatesByGuidAsync_Exception()
        {
            roomRatesServiceMock.Setup(x => x.GetRoomRatesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await roomRatesController.GetRoomRatesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomRatesController_PostRoomRatesAsync_Exception()
        {
            await roomRatesController.PostRoomRatesAsync(roomRatesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomRatesController_PutRoomRatesAsync_Exception()
        {
            var sourceContext = roomRatesCollection.FirstOrDefault();
            await roomRatesController.PutRoomRatesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomRatesController_DeleteRoomRatesAsync_Exception()
        {
            await roomRatesController.DeleteRoomRatesAsync(roomRatesCollection.FirstOrDefault().Id);
        }
    }
}