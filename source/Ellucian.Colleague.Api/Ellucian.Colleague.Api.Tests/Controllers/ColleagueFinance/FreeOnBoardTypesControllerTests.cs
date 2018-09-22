//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class FreeOnBoardTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFreeOnBoardTypesService> freeOnBoardTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private FreeOnBoardTypesController freeOnBoardTypesController;      
        private IEnumerable<Domain.ColleagueFinance.Entities.FreeOnBoardType> allFobs;
        private List<Dtos.FreeOnBoardTypes> freeOnBoardTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            freeOnBoardTypesServiceMock = new Mock<IFreeOnBoardTypesService>();
            loggerMock = new Mock<ILogger>();
            freeOnBoardTypesCollection = new List<Dtos.FreeOnBoardTypes>();

            allFobs = new List<Domain.ColleagueFinance.Entities.FreeOnBoardType>()
                {
                    new Domain.ColleagueFinance.Entities.FreeOnBoardType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.ColleagueFinance.Entities.FreeOnBoardType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.ColleagueFinance.Entities.FreeOnBoardType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allFobs)
            {
                var freeOnBoardTypes = new Ellucian.Colleague.Dtos.FreeOnBoardTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                freeOnBoardTypesCollection.Add(freeOnBoardTypes);
            }

            freeOnBoardTypesController = new FreeOnBoardTypesController(freeOnBoardTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            freeOnBoardTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            freeOnBoardTypesController = null;
            allFobs = null;
            freeOnBoardTypesCollection = null;
            loggerMock = null;
            freeOnBoardTypesServiceMock = null;
        }

        [TestMethod]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypes_ValidateFields_Nocache()
        {
            freeOnBoardTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesAsync(false)).ReturnsAsync(freeOnBoardTypesCollection);
       
            var sourceContexts = (await freeOnBoardTypesController.GetFreeOnBoardTypesAsync()).ToList();
            Assert.AreEqual(freeOnBoardTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = freeOnBoardTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypes_ValidateFields_Cache()
        {
            freeOnBoardTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesAsync(true)).ReturnsAsync(freeOnBoardTypesCollection);

            var sourceContexts = (await freeOnBoardTypesController.GetFreeOnBoardTypesAsync()).ToList();
            Assert.AreEqual(freeOnBoardTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = freeOnBoardTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypes_KeyNotFoundException()
        {
            //
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypes_PermissionsException()
        {
            
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesAsync(false))
                .Throws<PermissionsException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypes_ArgumentException()
        {
            
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesAsync(false))
                .Throws<ArgumentException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypes_RepositoryException()
        {
            
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesAsync(false))
                .Throws<RepositoryException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypes_IntegrationApiException()
        {
            
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesAsync(false))
                .Throws<IntegrationApiException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesAsync();
        }

        [TestMethod]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypesByGuidAsync_ValidateFields()
        {
            freeOnBoardTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = freeOnBoardTypesCollection.FirstOrDefault();
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await freeOnBoardTypesController.GetFreeOnBoardTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypes_Exception()
        {
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesAsync(false)).Throws<Exception>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypesByGuidAsync_Exception()
        {
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypesByGuid_KeyNotFoundException()
        {
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypesByGuid_PermissionsException()
        {
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypesByGuid_ArgumentException()
        {
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypesByGuid_RepositoryException()
        {
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypesByGuid_IntegrationApiException()
        {
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_GetFreeOnBoardTypesByGuid_Exception()
        {
            freeOnBoardTypesServiceMock.Setup(x => x.GetFreeOnBoardTypesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await freeOnBoardTypesController.GetFreeOnBoardTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_PostFreeOnBoardTypesAsync_Exception()
        {
            await freeOnBoardTypesController.PostFreeOnBoardTypesAsync(freeOnBoardTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_PutFreeOnBoardTypesAsync_Exception()
        {
            var sourceContext = freeOnBoardTypesCollection.FirstOrDefault();
            await freeOnBoardTypesController.PutFreeOnBoardTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FreeOnBoardTypesController_DeleteFreeOnBoardTypesAsync_Exception()
        {
            await freeOnBoardTypesController.DeleteFreeOnBoardTypesAsync(freeOnBoardTypesCollection.FirstOrDefault().Id);
        }
    }
}