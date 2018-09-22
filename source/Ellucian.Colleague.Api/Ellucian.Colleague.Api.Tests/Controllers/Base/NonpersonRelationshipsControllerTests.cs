//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class NonpersonRelationshipsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<INonPersonRelationshipsService> nonpersonRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        private NonPersonRelationshipsController nonpersonRelationshipsController;      
        //private IEnumerable<Domain.Base.Entities.Relationship> allRelationship;
        private List<Dtos.NonPersonRelationships> nonpersonRelationshipsCollection;
        private Tuple<IEnumerable<Dtos.NonPersonRelationships>, int> nonpersonRelationshipsDtoTuple;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Paging paging = new Paging(0, 2);

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            nonpersonRelationshipsServiceMock = new Mock<INonPersonRelationshipsService>();
            loggerMock = new Mock<ILogger>();

            BuildData();

            nonpersonRelationshipsController = new NonPersonRelationshipsController(nonpersonRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            nonpersonRelationshipsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            nonpersonRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
        }

        private void BuildData()
        {
            nonpersonRelationshipsCollection = new List<Dtos.NonPersonRelationships>()
            {
                new Dtos.NonPersonRelationships()
                {
                    Comment = "Comment 1",
                    DirectRelationshipType = new Dtos.GuidObject2("c63f011e-fdbb-4f61-b179-974386edb5e2"),
                    EndOn = DateTime.Today.AddDays(30),
                    Id = "78e913d6-e5a8-4286-818f-2c2554f4043b",
                    ReciprocalRelationshipType = new Dtos.GuidObject2("098851e0-8602-46fb-9870-9526a91280a1"),
                    Related = new Dtos.DtoProperties.NonpersonRelationshipsRelated() { institution = new Dtos.GuidObject2("93ea6a6a-b7b8-492e-a7b8-3455b66befdb") },
                    StartOn = DateTime.Today,
                    Status = new Dtos.GuidObject2("a12987e1-9241-40df-81f6-dbb3699f5711"),
                    Subject = new Dtos.DtoProperties.NonpersonRelationshipsSubject() { institution = new Dtos.GuidObject2("02f62048-5d3e-43c6-8de6-c20f9b187395") }
                },
                new Dtos.NonPersonRelationships()
                {
                    Comment = "Comment 2",
                    DirectRelationshipType = new Dtos.GuidObject2("d63f011e-fdbb-4f61-b179-974386edb5e2"),
                    EndOn = DateTime.Today.AddDays(30),
                    Id = "88e913d6-e5a8-4286-818f-2c2554f4043b",
                    ReciprocalRelationshipType = new Dtos.GuidObject2("198851e0-8602-46fb-9870-9526a91280a1"),
                    Related = new Dtos.DtoProperties.NonpersonRelationshipsRelated() { institution = new Dtos.GuidObject2("03ea6a6a-b7b8-492e-a7b8-3455b66befdb") },
                    StartOn = DateTime.Today,
                    Status = new Dtos.GuidObject2("b12987e1-9241-40df-81f6-dbb3699f5711"),
                    Subject = new Dtos.DtoProperties.NonpersonRelationshipsSubject() { institution = new Dtos.GuidObject2("12f62048-5d3e-43c6-8de6-c20f9b187395") }
                }
            };
            nonpersonRelationshipsDtoTuple = new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(nonpersonRelationshipsCollection, nonpersonRelationshipsCollection.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            nonpersonRelationshipsController = null;
            //allRelationship = null;
            nonpersonRelationshipsCollection = null;
            loggerMock = null;
            nonpersonRelationshipsServiceMock = null;
        }

        [TestMethod]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_ValidateFields_Organization_Nocache()
        {
            nonpersonRelationshipsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(nonpersonRelationshipsDtoTuple);
       
            var sourceContexts = await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "{\"organization\":{\"id\":\"e516db25-936a-486f-a574-152e9d4507fc\"}}", "", "", "");

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.NonPersonRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.NonPersonRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.NonPersonRelationships>;

            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_ValidateFields_Institution_Nocache()
        {
            nonpersonRelationshipsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(nonpersonRelationshipsDtoTuple);

            var sourceContexts = await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "", "{\"institution\":{\"id\":\"e516db25-936a-486f-a574-152e9d4507fc\"}}", "", "");

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.NonPersonRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.NonPersonRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.NonPersonRelationships>;

            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_ValidateFields_Person_Nocache()
        {
            nonpersonRelationshipsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(nonpersonRelationshipsDtoTuple);

            var sourceContexts = await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "", "", "{\"person\":{\"id\":\"e516db25-936a-486f-a574-152e9d4507fc\"}}", "");

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.NonPersonRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.NonPersonRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.NonPersonRelationships>;

            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_ValidateFields_RelationshipType_Nocache()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(nonpersonRelationshipsDtoTuple);

            var sourceContexts = await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "", "", "", "{\"relationshipType\":{\"id\":\"e516db25-936a-486f-a574-152e9d4507fc\"}}");

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.NonPersonRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.NonPersonRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.NonPersonRelationships>;

            Assert.IsNotNull(results);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_KeyNotFoundException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "", "", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_PermissionsException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "", "", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_ArgumentException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "", "", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_RepositoryException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "", "", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_IntegrationApiException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "", "", "", "");
        }

        //[TestMethod]
        //public async Task NonpersonRelationshipsController_GetNonpersonRelationshipsByGuidAsync_ValidateFields()
        //{
        //    var expected = nonpersonRelationshipsCollection.FirstOrDefault();
        //    nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

        //    var actual = await nonpersonRelationshipsController.GetNonPersonRelationshipsByGuidAsync(expected.Id);

        //    Assert.AreEqual(expected.Id, actual.Id, "Id");
        //    Assert.AreEqual(expected.Title, actual.Title, "Title");
        //    Assert.AreEqual(expected.Code, actual.Code, "Code");
        //}

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationships_Exception()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsAsync(null, "", "", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationshipsByGuidAsync_Exception()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationshipsByGuid_KeyNotFoundException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationshipsByGuid_PermissionsException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationshipsByGuid_ArgumentException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationshipsByGuid_RepositoryException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationshipsByGuid_IntegrationApiException()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_GetNonpersonRelationshipsByGuid_Exception()
        {
            nonpersonRelationshipsServiceMock.Setup(x => x.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await nonpersonRelationshipsController.GetNonPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_PostNonpersonRelationshipsAsync_Exception()
        {
            await nonpersonRelationshipsController.PostNonPersonRelationshipsAsync(nonpersonRelationshipsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_PutNonpersonRelationshipsAsync_Exception()
        {
            var sourceContext = nonpersonRelationshipsCollection.FirstOrDefault();
            await nonpersonRelationshipsController.PutNonPersonRelationshipsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonpersonRelationshipsController_DeleteNonpersonRelationshipsAsync_Exception()
        {
            await nonpersonRelationshipsController.DeleteNonPersonRelationshipsAsync(nonpersonRelationshipsCollection.FirstOrDefault().Id);
        }
    }
}