//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonRelationshipsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPersonRelationshipsService> personRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        private PersonRelationshipsController personRelationshipsController;
        private IEnumerable<Domain.Base.Entities.Relationship> allRelationship;
        private List<Dtos.PersonRelationships> personRelationshipsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            personRelationshipsServiceMock = new Mock<IPersonRelationshipsService>();
            loggerMock = new Mock<ILogger>();
            personRelationshipsCollection = new List<Dtos.PersonRelationships>();

            allRelationship = new List<Domain.Base.Entities.Relationship>()
                {
                    new Domain.Base.Entities.Relationship("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "d2253ac7-9931-4560-b42f-1fccd43c952e", "Athletic", false, DateTime.Now, DateTime.Now) { Guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", SubjectPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e", RelationPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e"},
                    new Domain.Base.Entities.Relationship("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "Academic", true, DateTime.Now, DateTime.Now) { Guid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", SubjectPersonGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", RelationPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e"},
                    new Domain.Base.Entities.Relationship("d2253ac7-9931-4560-b42f-1fccd43c952e", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "Cultural", true, DateTime.Now, DateTime.Now) { Guid = "d2253ac7-9931-4560-b42f-1fccd43c952e", SubjectPersonGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", RelationPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e"}
                };

            foreach (var source in allRelationship)
            {
                var personRelationships = new Ellucian.Colleague.Dtos.PersonRelationships
                {
                    Id = source.Guid,
                    SubjectPerson = new GuidObject2(source.SubjectPersonGuid),
                    Related = new PersonRelationshipsRelatedPerson() { person = new GuidObject2(source.RelationPersonGuid) },
                    StartOn = DateTime.Now,
                    EndOn = DateTime.Now
                };
                personRelationshipsCollection.Add(personRelationships);
            }

            personRelationshipsController = new PersonRelationshipsController(personRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            personRelationshipsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            personRelationshipsController = null;
            allRelationship = null;
            personRelationshipsCollection = null;
            loggerMock = null;
            personRelationshipsServiceMock = null;
        }

        [TestMethod]
        public async Task PersonRelationshipsController_GetPersonRelationships_ValidateFields_Nocache()
        {
            personRelationshipsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false,
                Public = true
            };

            int Offset = 0;
            int Limit = 4;
            var PersonRelationshipsTuple =
                new Tuple<IEnumerable<Dtos.PersonRelationships>, int>(personRelationshipsCollection.Take(4), personRelationshipsCollection.Count());

            personRelationshipsServiceMock.Setup(i => i.GetPersonRelationshipsAsync(Offset, Limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(PersonRelationshipsTuple);

            Paging paging = new Paging(Limit, Offset);
            var actuals = await personRelationshipsController.GetPersonRelationshipsAsync(paging);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonRelationships>;

            Assert.IsNotNull(results);
            Assert.AreEqual(3, results.Count());

            foreach (var actual in results)
            {
                var expected = personRelationshipsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.SubjectPerson, actual.SubjectPerson);
                Assert.AreEqual(expected.Related.person.Id, actual.Related.person.Id);

                if (expected.StartOn.Value != null)
                {
                    Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
                }
                if (expected.EndOn.Value != null)
                {
                    Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                }
            }
        }

        [TestMethod]
        public async Task PersonRelationshipsController_GetPersonRelationships_ValidateFields_Cache()
        {
            personRelationshipsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            int Offset = 0;
            int Limit = 4;
            var PersonRelationshipsTuple =
                new Tuple<IEnumerable<Dtos.PersonRelationships>, int>(personRelationshipsCollection.Take(4), personRelationshipsCollection.Count());

            personRelationshipsServiceMock.Setup(i => i.GetPersonRelationshipsAsync(Offset, Limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(PersonRelationshipsTuple);

            Paging paging = new Paging(Limit, Offset);
            var actuals = await personRelationshipsController.GetPersonRelationshipsAsync(paging);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonRelationships>;

            Assert.IsNotNull(results);
            Assert.AreEqual(3, results.Count());

            foreach (var actual in results)
            {
                var expected = personRelationshipsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.SubjectPerson, actual.SubjectPerson);
                Assert.AreEqual(expected.Related.person.Id, actual.Related.person.Id);

                if (expected.StartOn.Value != null)
                {
                    Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
                }
                if (expected.EndOn.Value != null)
                {
                    Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationships_KeyNotFoundException()
        {
            //
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personRelationshipsController.GetPersonRelationshipsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationships_PermissionsException()
        {

            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await personRelationshipsController.GetPersonRelationshipsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationships_ArgumentException()
        {

            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await personRelationshipsController.GetPersonRelationshipsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationships_RepositoryException()
        {

            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await personRelationshipsController.GetPersonRelationshipsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationships_IntegrationApiException()
        {

            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await personRelationshipsController.GetPersonRelationshipsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        public async Task PersonRelationshipsController_GetPersonRelationshipsByGuidAsync_ValidateFields()
        {
            var expected = personRelationshipsCollection.FirstOrDefault();
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await personRelationshipsController.GetPersonRelationshipsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.SubjectPerson, actual.SubjectPerson);
            Assert.AreEqual(expected.Related.person.Id, actual.Related.person.Id);

            if (expected.StartOn.Value != null)
            {
                Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
            }
            if (expected.EndOn.Value != null)
            {
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationships_Exception()
        {
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personRelationshipsController.GetPersonRelationshipsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationshipsByGuidAsync_Exception()
        {
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personRelationshipsController.GetPersonRelationshipsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationshipsByGuid_KeyNotFoundException()
        {
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personRelationshipsController.GetPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationshipsByGuid_PermissionsException()
        {
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await personRelationshipsController.GetPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationshipsByGuid_ArgumentException()
        {
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await personRelationshipsController.GetPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationshipsByGuid_RepositoryException()
        {
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await personRelationshipsController.GetPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationshipsByGuid_IntegrationApiException()
        {
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await personRelationshipsController.GetPersonRelationshipsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelationshipsController_GetPersonRelationshipsByGuid_Exception()
        {
            personRelationshipsServiceMock.Setup(x => x.GetPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await personRelationshipsController.GetPersonRelationshipsByGuidAsync(expectedGuid);
        }
    }

    [TestClass]
    public class PersonRelationshipsControllerTests_POST_V13
    {
        #region DECLARATION

        public TestContext TestContext { get; set; }

        private Mock<IPersonRelationshipsService> personRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        private PersonRelationshipsController personRelationshipsController;

        private PersonRelationships personRelationships;

        private string guid = "1adc2629-e8a7-410e-b4df-572d02822f8b";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            personRelationshipsServiceMock = new Mock<IPersonRelationshipsService>();

            InitializeTestData();

            personRelationshipsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            personRelationshipsController = new PersonRelationshipsController(personRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            personRelationshipsServiceMock = null;
            personRelationshipsController = null;
        }

        private void InitializeTestData()
        {
            personRelationships = new PersonRelationships()
            {
                Id = guid
            };
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_Dto_Null()
        {
            await personRelationshipsController.PostPersonRelationshipsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_Dto_Id_Null()
        {
            await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_Dto_Id_Not_Empty_Guid()
        {
            await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_KeyNotFoundException()
        {
            personRelationshipsServiceMock.Setup(s => s.CreatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new KeyNotFoundException());
            await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_PermissionsException()
        {
            personRelationshipsServiceMock.Setup(s => s.CreatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new PermissionsException());
            await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_ArgumentException()
        {
            personRelationshipsServiceMock.Setup(s => s.CreatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new ArgumentException());
            await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_RepositoryException()
        {
            personRelationshipsServiceMock.Setup(s => s.CreatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new RepositoryException());
            await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_IntegrationApiException()
        {
            personRelationshipsServiceMock.Setup(s => s.CreatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new IntegrationApiException());
            await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_ConfigurationException()
        {
            personRelationshipsServiceMock.Setup(s => s.CreatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new ConfigurationException());
            await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonRelationshipsAsync_Exception()
        {
            personRelationshipsServiceMock.Setup(s => s.CreatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new Exception());
            await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        public async Task PersonRelController_PostPersonRelationshipsAsync()
        {
            personRelationshipsServiceMock.Setup(s => s.CreatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ReturnsAsync(personRelationships);
            var result = await personRelationshipsController.PostPersonRelationshipsAsync(new PersonRelationships() { Id = Guid.Empty.ToString() });

            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
        }
    }

    [TestClass]
    public class PersonRelationshipsControllerTests_PUT_V13
    {
        #region DECLARATION

        public TestContext TestContext { get; set; }

        private Mock<IPersonRelationshipsService> personRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        private PersonRelationshipsController personRelationshipsController;

        private PersonRelationships personRelationships;

        private string guid = "1adc2629-e8a7-410e-b4df-572d02822f8b";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            personRelationshipsServiceMock = new Mock<IPersonRelationshipsService>();

            InitializeTestData();

            personRelationshipsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            personRelationshipsController = new PersonRelationshipsController(personRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            personRelationshipsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            personRelationshipsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(personRelationships));
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            personRelationshipsServiceMock = null;
            personRelationshipsController = null;
        }

        private void InitializeTestData()
        {
            personRelationships = new PersonRelationships()
            {
                Id = guid,
                Comment = "Comment"
            };
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_Guid_NullOrEmpty()
        {
            await personRelationshipsController.PutPersonRelationshipsAsync(null, new PersonRelationships() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_Dto_Null()
        {
            await personRelationshipsController.PutPersonRelationshipsAsync(guid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_Guid_As_Empty()
        {
            await personRelationshipsController.PutPersonRelationshipsAsync(Guid.Empty.ToString(), new PersonRelationships() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_Guid_Not_Matching_With_Dto_Id()
        {
            await personRelationshipsController.PutPersonRelationshipsAsync(guid, new PersonRelationships() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_PermissionsException()
        {
            personRelationshipsServiceMock.Setup(s => s.UpdatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new PermissionsException());
            await personRelationshipsController.PutPersonRelationshipsAsync(guid, new PersonRelationships() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_ArgumentException()
        {
            personRelationshipsServiceMock.Setup(s => s.UpdatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new ArgumentException());
            await personRelationshipsController.PutPersonRelationshipsAsync(guid, new PersonRelationships() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_RepositoryException()
        {
            personRelationshipsServiceMock.Setup(s => s.UpdatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new RepositoryException());
            await personRelationshipsController.PutPersonRelationshipsAsync(guid, new PersonRelationships() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_IntegrationApiException()
        {
            personRelationshipsServiceMock.Setup(s => s.UpdatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new IntegrationApiException());
            await personRelationshipsController.PutPersonRelationshipsAsync(guid, new PersonRelationships() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_ConfigurationException()
        {
            personRelationshipsServiceMock.Setup(s => s.UpdatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new ConfigurationException());
            await personRelationshipsController.PutPersonRelationshipsAsync(guid, new PersonRelationships() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_KeyNotFoundException()
        {
            personRelationshipsServiceMock.Setup(s => s.UpdatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new KeyNotFoundException());
            await personRelationshipsController.PutPersonRelationshipsAsync(guid, new PersonRelationships() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonRelationshipsAsync_Exception()
        {
            personRelationshipsServiceMock.Setup(s => s.UpdatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ThrowsAsync(new Exception());
            await personRelationshipsController.PutPersonRelationshipsAsync(guid, new PersonRelationships() { Id = guid });
        }

        [TestMethod]
        public async Task PersonRelController_PutPersonRelationshipsAsync()
        {
            personRelationships.Comment = "Updated Comment";
            personRelationshipsServiceMock.Setup(s => s.UpdatePersonRelationshipsAsync(It.IsAny<PersonRelationships>())).ReturnsAsync(personRelationships);

            var result = await personRelationshipsController.PutPersonRelationshipsAsync(guid, new PersonRelationships() { Id = guid });

            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
            Assert.AreEqual("Updated Comment", result.Comment);
        }
    }

    [TestClass]
    public class PersonRelationshipsControllerTests_DELETE_V13
    {
        #region DECLARATION

        public TestContext TestContext { get; set; }

        private Mock<IPersonRelationshipsService> personRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        private PersonRelationshipsController personRelationshipsController;

        private PersonRelationships personRelationships;

        private string guid = "1adc2629-e8a7-410e-b4df-572d02822f8b";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            personRelationshipsServiceMock = new Mock<IPersonRelationshipsService>();

            personRelationshipsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            personRelationshipsController = new PersonRelationshipsController(personRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            personRelationshipsServiceMock = null;
            personRelationshipsController = null;
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonRelationshipsAsync_Guid_Null()
        {
            await personRelationshipsController.DeletePersonRelationshipsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonRelationshipsAsync_PermissionsException()
        {
            personRelationshipsServiceMock.Setup(s => s.DeletePersonRelationshipsAsync(It.IsAny<String>())).Throws(new PermissionsException());
            await personRelationshipsController.DeletePersonRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonRelationshipsAsync_KeyNotFoundException()
        {
            personRelationshipsServiceMock.Setup(s => s.DeletePersonRelationshipsAsync(It.IsAny<String>())).Throws(new KeyNotFoundException());
            await personRelationshipsController.DeletePersonRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonRelationshipsAsync_IntegrationApiException()
        {
            personRelationshipsServiceMock.Setup(s => s.DeletePersonRelationshipsAsync(It.IsAny<String>())).Throws(new IntegrationApiException());
            await personRelationshipsController.DeletePersonRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonRelationshipsAsync_ArgumentException()
        {
            personRelationshipsServiceMock.Setup(s => s.DeletePersonRelationshipsAsync(It.IsAny<String>())).Throws(new ArgumentException());
            await personRelationshipsController.DeletePersonRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonRelationshipsAsync_InvalidOperationException()
        {
            personRelationshipsServiceMock.Setup(s => s.DeletePersonRelationshipsAsync(It.IsAny<String>())).Throws(new InvalidOperationException());
            await personRelationshipsController.DeletePersonRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonRelationshipsAsync_RepositoryException()
        {
            personRelationshipsServiceMock.Setup(s => s.DeletePersonRelationshipsAsync(It.IsAny<String>())).Throws(new RepositoryException());
            await personRelationshipsController.DeletePersonRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonRelationshipsAsync_Exception()
        {
            personRelationshipsServiceMock.Setup(s => s.DeletePersonRelationshipsAsync(It.IsAny<String>())).Throws(new Exception());
            await personRelationshipsController.DeletePersonRelationshipsAsync(guid);
        }
    }
}