// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonalRelationshipsControllerTests
    {
        private TestContext testContextInstance2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance2;
            }
            set
            {
                testContextInstance2 = value;
            }
        }

        private PersonalRelationshipsController personalRelationshipsController;
        Mock<ILogger> loggerMock = new Mock<ILogger>();
        IAdapterRegistry AdapterRegistry;
        Mock<IPersonalRelationshipsService> personalRelationshipsServiceeMock = new Mock<IPersonalRelationshipsService>();

        Ellucian.Colleague.Dtos.PersonalRelationship personalRelationship;
        List<Dtos.PersonalRelationship> personalRelationshipsDtos = new List<PersonalRelationship>();
        Tuple<IEnumerable<Dtos.PersonalRelationship>, int> personalRelationshipTuple;
        private Paging page;
        private int limit;
        private int offset;

        string id = "9eeb5365-9478-4b40-8463-1e1d0ecf8956";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);

            personalRelationshipsDtos = new TestPersonalRelationshipsRepository().GetPersonalRelationships().ToList();
            limit = 3;
            offset = 0;
            page = new Paging(limit, offset);

            personalRelationshipTuple = new Tuple<IEnumerable<PersonalRelationship>,int>(personalRelationshipsDtos, 3);

            personalRelationshipsController = new PersonalRelationshipsController(AdapterRegistry, personalRelationshipsServiceeMock.Object, loggerMock.Object);
            personalRelationshipsController.Request = new HttpRequestMessage();
            personalRelationshipsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

        }

        [TestCleanup]
        public void Cleanup()
        {
            personalRelationshipsController = null;
            personalRelationship = null;
        }       

        #region Exceptions Testing

        #region GET

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_ArgumentNullException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentNullException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_PermissionsException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_RepositoryException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_IntegrationApiException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_KeyNotFoundException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_Exception()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipByIdAsync_ArgumentNullException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentNullException());
            var result = await personalRelationshipsController.GetPersonalRelationshipByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipByIdAsync_InvalidOperationException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException());
            var result = await personalRelationshipsController.GetPersonalRelationshipByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipByIdAsync_KeyNotFoundException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException());
            var result = await personalRelationshipsController.GetPersonalRelationshipByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipByIdAsync_Exception()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());
            var result = await personalRelationshipsController.GetPersonalRelationshipByIdAsync("1234");
        }

        [TestMethod]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync()
        {
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personalRelationshipsController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personalRelationshipsServiceeMock.Setup(x => x.GetAllPersonalRelationshipsAsync(page.Offset, page.Limit, It.IsAny<bool>())).ReturnsAsync(personalRelationshipTuple);

            var results = await personalRelationshipsController.GetPersonalRelationshipsAsync(page);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonalRelationship> personalRelationshipResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationship>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonalRelationship>;

            Assert.AreEqual(personalRelationshipsDtos.Count(), 3);
            Assert.AreEqual(personalRelationshipResults.Count(), 3);
            int resultCounts = personalRelationshipResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personalRelationshipsDtos[i];
                var actual = personalRelationshipResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.DirectRelationship.Detail.Id, actual.DirectRelationship.Detail.Id);
                Assert.AreEqual(expected.DirectRelationship.RelationshipType, actual.DirectRelationship.RelationshipType);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.PersonalRelationshipStatus.Id, actual.PersonalRelationshipStatus.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.Detail.Id, actual.ReciprocalRelationship.Detail.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.RelationshipType, actual.ReciprocalRelationship.RelationshipType);
                Assert.AreEqual(expected.RelatedPerson.Id, actual.RelatedPerson.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
            }
        }

        [TestMethod]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_WithNullPage()
        {
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personalRelationshipsController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personalRelationshipsServiceeMock.Setup(x => x.GetAllPersonalRelationshipsAsync(0, 200, It.IsAny<bool>())).ReturnsAsync(personalRelationshipTuple);

            var results = await personalRelationshipsController.GetPersonalRelationshipsAsync(null);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonalRelationship> personalRelationshipResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationship>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonalRelationship>;

            Assert.AreEqual(personalRelationshipsDtos.Count(), 3);
            Assert.AreEqual(personalRelationshipResults.Count(), 3);
            int resultCounts = personalRelationshipResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personalRelationshipsDtos[i];
                var actual = personalRelationshipResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.DirectRelationship.Detail.Id, actual.DirectRelationship.Detail.Id);
                Assert.AreEqual(expected.DirectRelationship.RelationshipType, actual.DirectRelationship.RelationshipType);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.PersonalRelationshipStatus.Id, actual.PersonalRelationshipStatus.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.Detail.Id, actual.ReciprocalRelationship.Detail.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.RelationshipType, actual.ReciprocalRelationship.RelationshipType);
                Assert.AreEqual(expected.RelatedPerson.Id, actual.RelatedPerson.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
            }
        }

        [TestMethod]
        public async Task PersonVisasController_GetPersonalRelationshipByIdAsync()
        {
            var expected = personalRelationshipsDtos[1];
            personalRelationshipsServiceeMock.Setup(x => x.GetPersonalRelationshipByIdAsync(id)).ReturnsAsync(expected);

            var actual = await personalRelationshipsController.GetPersonalRelationshipByIdAsync(id);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Comment, actual.Comment);
            Assert.AreEqual(expected.DirectRelationship.Detail.Id, actual.DirectRelationship.Detail.Id);
            Assert.AreEqual(expected.DirectRelationship.RelationshipType, actual.DirectRelationship.RelationshipType);
            Assert.AreEqual(expected.EndOn, actual.EndOn);
            Assert.AreEqual(expected.PersonalRelationshipStatus.Id, actual.PersonalRelationshipStatus.Id);
            Assert.AreEqual(expected.ReciprocalRelationship.Detail.Id, actual.ReciprocalRelationship.Detail.Id);
            Assert.AreEqual(expected.ReciprocalRelationship.RelationshipType, actual.ReciprocalRelationship.RelationshipType);
            Assert.AreEqual(expected.RelatedPerson.Id, actual.RelatedPerson.Id);
            Assert.AreEqual(expected.StartOn, actual.StartOn);
            Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
        }

        [TestMethod]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync()
        {
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            personalRelationshipsServiceeMock.Setup(x => x.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(personalRelationshipTuple);

            var results = await personalRelationshipsController.GetPersonalRelationshipsAsync(null, "test");

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonalRelationship> personalRelationshipResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationship>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonalRelationship>;

            Assert.AreEqual(personalRelationshipsDtos.Count(), 3);
            Assert.AreEqual(personalRelationshipResults.Count(), 3);
            int resultCounts = personalRelationshipResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personalRelationshipsDtos[i];
                var actual = personalRelationshipResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.DirectRelationship.Detail.Id, actual.DirectRelationship.Detail.Id);
                Assert.AreEqual(expected.DirectRelationship.RelationshipType, actual.DirectRelationship.RelationshipType);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.PersonalRelationshipStatus.Id, actual.PersonalRelationshipStatus.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.Detail.Id, actual.ReciprocalRelationship.Detail.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.RelationshipType, actual.ReciprocalRelationship.RelationshipType);
                Assert.AreEqual(expected.RelatedPerson.Id, actual.RelatedPerson.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_PermissionException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new PermissionsException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_ArgumentException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_RepositoryException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new RepositoryException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_IntegrationApiException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new IntegrationApiException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_Exception()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        #endregion

        #region POST
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_PostPersonalRelationshipAsync_Exception()
        {
            await personalRelationshipsController.PostPersonalRelationshipAsync(It.IsAny<PersonalRelationship>());
        }
        #endregion

        #region PUT
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_PutPersonalRelationshipAsync_Exception()
        {
            await personalRelationshipsController.PutPersonalRelationshipAsync(It.IsAny<string>(), It.IsAny<PersonalRelationship>());
        }
        #endregion

        #region DELETE
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_DeletePersonRelationshipAsync_Exception()
        {
            await personalRelationshipsController.DeletePersonRelationshipAsync(It.IsAny<string>());
        }
        #endregion

        #endregion
    }
}
