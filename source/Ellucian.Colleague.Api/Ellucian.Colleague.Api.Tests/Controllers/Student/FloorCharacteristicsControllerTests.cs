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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class FloorCharacteristicsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFloorCharacteristicsService> floorCharacteristicsServiceMock;
        private Mock<ILogger> loggerMock;
        private FloorCharacteristicsController floorCharacteristicsController;      
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics> allFloorCharacteristics;
        private List<Dtos.FloorCharacteristics> floorCharacteristicsCollection;

        [TestInitialize]
        public async void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            floorCharacteristicsServiceMock = new Mock<IFloorCharacteristicsService>();
            loggerMock = new Mock<ILogger>();
            floorCharacteristicsCollection = new List<Dtos.FloorCharacteristics>();

            allFloorCharacteristics  = new List<Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allFloorCharacteristics)
            {
                var floorCharacteristics = new Ellucian.Colleague.Dtos.FloorCharacteristics
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                floorCharacteristicsCollection.Add(floorCharacteristics);
            }

            floorCharacteristicsController = new FloorCharacteristicsController(floorCharacteristicsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            floorCharacteristicsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            floorCharacteristicsController = null;
            allFloorCharacteristics = null;
            floorCharacteristicsCollection = null;
            loggerMock = null;
            floorCharacteristicsServiceMock = null;
        }

        [TestMethod]
        public async Task FloorCharacteristicsController_GetFloorCharacteristics_ValidateFields_Nocache()
        {
            floorCharacteristicsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            floorCharacteristicsServiceMock.Setup(x => x.GetFloorCharacteristicsAsync(false)).ReturnsAsync(floorCharacteristicsCollection);
       
            var sourceContexts = (await floorCharacteristicsController.GetFloorCharacteristicsAsync()).ToList();
            Assert.AreEqual(floorCharacteristicsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = floorCharacteristicsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FloorCharacteristicsController_GetFloorCharacteristics_ValidateFields_Cache()
        {
            floorCharacteristicsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            floorCharacteristicsServiceMock.Setup(x => x.GetFloorCharacteristicsAsync(true)).ReturnsAsync(floorCharacteristicsCollection);

            var sourceContexts = (await floorCharacteristicsController.GetFloorCharacteristicsAsync()).ToList();
            Assert.AreEqual(floorCharacteristicsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = floorCharacteristicsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FloorCharacteristicsController_GetFloorCharacteristicsByGuidAsync_ValidateFields()
        {
            var expected = floorCharacteristicsCollection.FirstOrDefault();
            floorCharacteristicsServiceMock.Setup(x => x.GetFloorCharacteristicsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await floorCharacteristicsController.GetFloorCharacteristicsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FloorCharacteristicsController_GetFloorCharacteristics_Exception()
        {
            floorCharacteristicsServiceMock.Setup(x => x.GetFloorCharacteristicsAsync(false)).Throws<Exception>();
            await floorCharacteristicsController.GetFloorCharacteristicsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FloorCharacteristicsController_GetFloorCharacteristicsByGuidAsync_Exception()
        {
            floorCharacteristicsServiceMock.Setup(x => x.GetFloorCharacteristicsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await floorCharacteristicsController.GetFloorCharacteristicsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FloorCharacteristicsController_PostFloorCharacteristicsAsync_Exception()
        {
            await floorCharacteristicsController.PostFloorCharacteristicsAsync(floorCharacteristicsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FloorCharacteristicsController_PutFloorCharacteristicsAsync_Exception()
        {
            var sourceContext = floorCharacteristicsCollection.FirstOrDefault();
            await floorCharacteristicsController.PutFloorCharacteristicsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FloorCharacteristicsController_DeleteFloorCharacteristicsAsync_Exception()
        {
            await floorCharacteristicsController.DeleteFloorCharacteristicsAsync(floorCharacteristicsCollection.FirstOrDefault().Id);
        }
    }
}