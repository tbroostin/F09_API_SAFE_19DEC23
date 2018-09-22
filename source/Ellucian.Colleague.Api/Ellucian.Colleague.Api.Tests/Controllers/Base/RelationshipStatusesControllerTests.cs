//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class RelationshipStatusesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IRelationshipStatusesService> relationshipStatusesServiceMock;
        private Mock<ILogger> loggerMock;
        private RelationshipStatusesController relationshipStatusesController;      
        private IEnumerable<Domain.Base.Entities.RelationshipStatus> allRelationStatuses;
        private List<Dtos.RelationshipStatuses> relationshipStatusesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            relationshipStatusesServiceMock = new Mock<IRelationshipStatusesService>();
            loggerMock = new Mock<ILogger>();
            relationshipStatusesCollection = new List<Dtos.RelationshipStatuses>();

            allRelationStatuses = new List<Domain.Base.Entities.RelationshipStatus>()
                {
                    new Domain.Base.Entities.RelationshipStatus("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.RelationshipStatus("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.RelationshipStatus("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allRelationStatuses)
            {
                var relationshipStatuses = new Ellucian.Colleague.Dtos.RelationshipStatuses
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                relationshipStatusesCollection.Add(relationshipStatuses);
            }

            relationshipStatusesController = new RelationshipStatusesController(relationshipStatusesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            relationshipStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            relationshipStatusesController = null;
            allRelationStatuses = null;
            relationshipStatusesCollection = null;
            loggerMock = null;
            relationshipStatusesServiceMock = null;
        }

        [TestMethod]
        public async Task RelationshipStatusesController_GetRelationshipStatuses_ValidateFields_Nocache()
        {
            relationshipStatusesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesAsync(false)).ReturnsAsync(relationshipStatusesCollection);
       
            var sourceContexts = (await relationshipStatusesController.GetRelationshipStatusesAsync()).ToList();
            Assert.AreEqual(relationshipStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = relationshipStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task RelationshipStatusesController_GetRelationshipStatuses_ValidateFields_Cache()
        {
            relationshipStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesAsync(true)).ReturnsAsync(relationshipStatusesCollection);

            var sourceContexts = (await relationshipStatusesController.GetRelationshipStatusesAsync()).ToList();
            Assert.AreEqual(relationshipStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = relationshipStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatuses_KeyNotFoundException()
        {
            //
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesAsync(false))
                .Throws<KeyNotFoundException>();
            await relationshipStatusesController.GetRelationshipStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatuses_PermissionsException()
        {
            
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesAsync(false))
                .Throws<PermissionsException>();
            await relationshipStatusesController.GetRelationshipStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatuses_ArgumentException()
        {
            
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesAsync(false))
                .Throws<ArgumentException>();
            await relationshipStatusesController.GetRelationshipStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatuses_RepositoryException()
        {
            
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesAsync(false))
                .Throws<RepositoryException>();
            await relationshipStatusesController.GetRelationshipStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatuses_IntegrationApiException()
        {
            
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesAsync(false))
                .Throws<IntegrationApiException>();
            await relationshipStatusesController.GetRelationshipStatusesAsync();
        }

        [TestMethod]
        public async Task RelationshipStatusesController_GetRelationshipStatusesByGuidAsync_ValidateFields()
        {
            var expected = relationshipStatusesCollection.FirstOrDefault();
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await relationshipStatusesController.GetRelationshipStatusesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatuses_Exception()
        {
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesAsync(false)).Throws<Exception>();
            await relationshipStatusesController.GetRelationshipStatusesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatusesByGuidAsync_Exception()
        {
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await relationshipStatusesController.GetRelationshipStatusesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatusesByGuid_KeyNotFoundException()
        {
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await relationshipStatusesController.GetRelationshipStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatusesByGuid_PermissionsException()
        {
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await relationshipStatusesController.GetRelationshipStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatusesByGuid_ArgumentException()
        {
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await relationshipStatusesController.GetRelationshipStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatusesByGuid_RepositoryException()
        {
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await relationshipStatusesController.GetRelationshipStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatusesByGuid_IntegrationApiException()
        {
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await relationshipStatusesController.GetRelationshipStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_GetRelationshipStatusesByGuid_Exception()
        {
            relationshipStatusesServiceMock.Setup(x => x.GetRelationshipStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await relationshipStatusesController.GetRelationshipStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_PostRelationshipStatusesAsync_Exception()
        {
            await relationshipStatusesController.PostRelationshipStatusesAsync(relationshipStatusesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_PutRelationshipStatusesAsync_Exception()
        {
            var sourceContext = relationshipStatusesCollection.FirstOrDefault();
            await relationshipStatusesController.PutRelationshipStatusesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipStatusesController_DeleteRelationshipStatusesAsync_Exception()
        {
            await relationshipStatusesController.DeleteRelationshipStatusesAsync(relationshipStatusesCollection.FirstOrDefault().Id);
        }
    }
}