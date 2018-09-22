//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class LeaveTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILeaveTypesService> leaveTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private LeaveTypesController leaveTypesController;      
        private IEnumerable<Domain.HumanResources.Entities.LeaveType> allLeaveTypes;
        private List<Dtos.LeaveTypes> leaveTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            leaveTypesServiceMock = new Mock<ILeaveTypesService>();
            loggerMock = new Mock<ILogger>();
            leaveTypesCollection = new List<Dtos.LeaveTypes>();

            allLeaveTypes = new List<Domain.HumanResources.Entities.LeaveType>()
                {
                    new Domain.HumanResources.Entities.LeaveType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.LeaveType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.LeaveType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allLeaveTypes)
            {
                var leaveTypes = new Ellucian.Colleague.Dtos.LeaveTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                leaveTypesCollection.Add(leaveTypes);
            }

            leaveTypesController = new LeaveTypesController(leaveTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            leaveTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            leaveTypesController = null;
            allLeaveTypes = null;
            leaveTypesCollection = null;
            loggerMock = null;
            leaveTypesServiceMock = null;
        }

        [TestMethod]
        public async Task LeaveTypesController_GetLeaveTypes_ValidateFields_Nocache()
        {
            leaveTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesAsync(false)).ReturnsAsync(leaveTypesCollection);
       
            var sourceContexts = (await leaveTypesController.GetLeaveTypesAsync()).ToList();
            Assert.AreEqual(leaveTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = leaveTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task LeaveTypesController_GetLeaveTypes_ValidateFields_Cache()
        {
            leaveTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesAsync(true)).ReturnsAsync(leaveTypesCollection);

            var sourceContexts = (await leaveTypesController.GetLeaveTypesAsync()).ToList();
            Assert.AreEqual(leaveTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = leaveTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypes_KeyNotFoundException()
        {
            //
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await leaveTypesController.GetLeaveTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypes_PermissionsException()
        {
            
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesAsync(false))
                .Throws<PermissionsException>();
            await leaveTypesController.GetLeaveTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypes_ArgumentException()
        {
            
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesAsync(false))
                .Throws<ArgumentException>();
            await leaveTypesController.GetLeaveTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypes_RepositoryException()
        {
            
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesAsync(false))
                .Throws<RepositoryException>();
            await leaveTypesController.GetLeaveTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypes_IntegrationApiException()
        {
            
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesAsync(false))
                .Throws<IntegrationApiException>();
            await leaveTypesController.GetLeaveTypesAsync();
        }

        [TestMethod]
        public async Task LeaveTypesController_GetLeaveTypesByGuidAsync_ValidateFields()
        {
            var expected = leaveTypesCollection.FirstOrDefault();
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await leaveTypesController.GetLeaveTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypes_Exception()
        {
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesAsync(false)).Throws<Exception>();
            await leaveTypesController.GetLeaveTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypesByGuidAsync_Exception()
        {
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await leaveTypesController.GetLeaveTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypesByGuid_KeyNotFoundException()
        {
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await leaveTypesController.GetLeaveTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypesByGuid_PermissionsException()
        {
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await leaveTypesController.GetLeaveTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypesByGuid_ArgumentException()
        {
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await leaveTypesController.GetLeaveTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypesByGuid_RepositoryException()
        {
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await leaveTypesController.GetLeaveTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypesByGuid_IntegrationApiException()
        {
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await leaveTypesController.GetLeaveTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_GetLeaveTypesByGuid_Exception()
        {
            leaveTypesServiceMock.Setup(x => x.GetLeaveTypesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await leaveTypesController.GetLeaveTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_PostLeaveTypesAsync_Exception()
        {
            await leaveTypesController.PostLeaveTypesAsync(leaveTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_PutLeaveTypesAsync_Exception()
        {
            var sourceContext = leaveTypesCollection.FirstOrDefault();
            await leaveTypesController.PutLeaveTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveTypesController_DeleteLeaveTypesAsync_Exception()
        {
            await leaveTypesController.DeleteLeaveTypesAsync(leaveTypesCollection.FirstOrDefault().Id);
        }
    }
}