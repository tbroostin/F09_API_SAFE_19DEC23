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
    public class RoommateCharacteristicsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IRoommateCharacteristicsService> roommateCharacteristicsServiceMock;
        private Mock<ILogger> loggerMock;
        private RoommateCharacteristicsController roommateCharacteristicsController;      
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.RoommateCharacteristics> allRoommateCharacteristics;
        private List<Dtos.RoommateCharacteristics> roommateCharacteristicsCollection;

        [TestInitialize]
        public async void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            roommateCharacteristicsServiceMock = new Mock<IRoommateCharacteristicsService>();
            loggerMock = new Mock<ILogger>();
            roommateCharacteristicsCollection = new List<Dtos.RoommateCharacteristics>();

            allRoommateCharacteristics  = new List<Ellucian.Colleague.Domain.Student.Entities.RoommateCharacteristics>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.RoommateCharacteristics("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.RoommateCharacteristics("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.RoommateCharacteristics("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allRoommateCharacteristics)
            {
                var roommateCharacteristics = new Ellucian.Colleague.Dtos.RoommateCharacteristics
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                roommateCharacteristicsCollection.Add(roommateCharacteristics);
            }

            roommateCharacteristicsController = new RoommateCharacteristicsController(roommateCharacteristicsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            roommateCharacteristicsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            roommateCharacteristicsController = null;
            allRoommateCharacteristics = null;
            roommateCharacteristicsCollection = null;
            loggerMock = null;
            roommateCharacteristicsServiceMock = null;
        }

        [TestMethod]
        public async Task RoommateCharacteristicsController_GetRoommateCharacteristics_ValidateFields_Nocache()
        {
            roommateCharacteristicsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            roommateCharacteristicsServiceMock.Setup(x => x.GetRoommateCharacteristicsAsync(false)).ReturnsAsync(roommateCharacteristicsCollection);
       
            var sourceContexts = (await roommateCharacteristicsController.GetRoommateCharacteristicsAsync()).ToList();
            Assert.AreEqual(roommateCharacteristicsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = roommateCharacteristicsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task RoommateCharacteristicsController_GetRoommateCharacteristics_ValidateFields_Cache()
        {
            roommateCharacteristicsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            roommateCharacteristicsServiceMock.Setup(x => x.GetRoommateCharacteristicsAsync(true)).ReturnsAsync(roommateCharacteristicsCollection);

            var sourceContexts = (await roommateCharacteristicsController.GetRoommateCharacteristicsAsync()).ToList();
            Assert.AreEqual(roommateCharacteristicsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = roommateCharacteristicsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task RoommateCharacteristicsController_GetRoommateCharacteristicsByGuidAsync_ValidateFields()
        {
            var expected = roommateCharacteristicsCollection.FirstOrDefault();
            roommateCharacteristicsServiceMock.Setup(x => x.GetRoommateCharacteristicsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await roommateCharacteristicsController.GetRoommateCharacteristicsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoommateCharacteristicsController_GetRoommateCharacteristics_Exception()
        {
            roommateCharacteristicsServiceMock.Setup(x => x.GetRoommateCharacteristicsAsync(false)).Throws<Exception>();
            await roommateCharacteristicsController.GetRoommateCharacteristicsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoommateCharacteristicsController_GetRoommateCharacteristicsByGuidAsync_Exception()
        {
            roommateCharacteristicsServiceMock.Setup(x => x.GetRoommateCharacteristicsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await roommateCharacteristicsController.GetRoommateCharacteristicsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoommateCharacteristicsController_PostRoommateCharacteristicsAsync_Exception()
        {
            await roommateCharacteristicsController.PostRoommateCharacteristicsAsync(roommateCharacteristicsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoommateCharacteristicsController_PutRoommateCharacteristicsAsync_Exception()
        {
            var sourceContext = roommateCharacteristicsCollection.FirstOrDefault();
            await roommateCharacteristicsController.PutRoommateCharacteristicsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoommateCharacteristicsController_DeleteRoommateCharacteristicsAsync_Exception()
        {
            await roommateCharacteristicsController.DeleteRoommateCharacteristicsAsync(roommateCharacteristicsCollection.FirstOrDefault().Id);
        }
    }
}