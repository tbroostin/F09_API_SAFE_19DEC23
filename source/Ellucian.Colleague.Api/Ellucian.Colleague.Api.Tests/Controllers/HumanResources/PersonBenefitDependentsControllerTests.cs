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
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PersonBenefitDependentsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPersonBenefitDependentsService> personBenefitDependentsServiceMock;
        private Mock<ILogger> loggerMock;
        private PersonBenefitDependentsController personBenefitDependentsController;
        private IEnumerable<Domain.HumanResources.Entities.PersonBenefitDependent> allPerbens;
        private List<Dtos.PersonBenefitDependents> personBenefitDependentsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            personBenefitDependentsServiceMock = new Mock<IPersonBenefitDependentsService>();
            loggerMock = new Mock<ILogger>();
            personBenefitDependentsCollection = new List<Dtos.PersonBenefitDependents>();

            allPerbens = new List<Domain.HumanResources.Entities.PersonBenefitDependent>()
                {
                    new Domain.HumanResources.Entities.PersonBenefitDependent("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e"),
                    new Domain.HumanResources.Entities.PersonBenefitDependent("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    new Domain.HumanResources.Entities.PersonBenefitDependent("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                };

            foreach (var source in allPerbens)
            {
                var personBenefitDependents = new Ellucian.Colleague.Dtos.PersonBenefitDependents
                {
                    Id = source.Guid,
                    DeductionArrangement = new GuidObject2(source.DeductionArrangement),
                    Dependent = new Dtos.DtoProperties.PersonBenefitDependentsDependentDtoProperty() { Person = new GuidObject2(source.DependentPersonId) }
                };
                personBenefitDependentsCollection.Add(personBenefitDependents);
            }

            personBenefitDependentsController = new PersonBenefitDependentsController(personBenefitDependentsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            personBenefitDependentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            personBenefitDependentsController = null;
            allPerbens = null;
            personBenefitDependentsCollection = null;
            loggerMock = null;
            personBenefitDependentsServiceMock = null;
        }

        [TestMethod]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependents_ValidateFields_Nocache()
        {
            personBenefitDependentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personBenefitDependentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var personBenefitDependentsTuple
                    = new Tuple<IEnumerable<PersonBenefitDependents>, int>(personBenefitDependentsCollection, personBenefitDependentsCollection.Count);

            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(personBenefitDependentsTuple);

            Paging paging = new Paging(personBenefitDependentsCollection.Count(), 0);

            var sourceContexts = (await personBenefitDependentsController.GetPersonBenefitDependentsAsync(paging));

            var cancelToken = new System.Threading.CancellationToken(false);
            var httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            var results = ((ObjectContent<IEnumerable<Dtos.PersonBenefitDependents>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonBenefitDependents>;

            Assert.IsNotNull(results);
            Assert.AreEqual(personBenefitDependentsCollection.Count, results.Count());

            foreach (var actual in results)
            {
                var expected = personBenefitDependentsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.DeductionArrangement.Id, actual.DeductionArrangement.Id);
                Assert.AreEqual(expected.Dependent.Person.Id, actual.Dependent.Person.Id);
            }
        }

        [TestMethod]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependents_ValidateFields_Cache()
        {
            personBenefitDependentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personBenefitDependentsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var personBenefitDependentsTuple
                    = new Tuple<IEnumerable<PersonBenefitDependents>, int>(personBenefitDependentsCollection, personBenefitDependentsCollection.Count);

            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(personBenefitDependentsTuple);

            Paging paging = new Paging(personBenefitDependentsCollection.Count(), 0);

            var sourceContexts = (await personBenefitDependentsController.GetPersonBenefitDependentsAsync(paging));

            var cancelToken = new System.Threading.CancellationToken(false);
            var httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            var results = ((ObjectContent<IEnumerable<Dtos.PersonBenefitDependents>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonBenefitDependents>;

            Assert.IsNotNull(results);
            Assert.AreEqual(personBenefitDependentsCollection.Count, results.Count());

            foreach (var actual in results)
            {
                var expected = personBenefitDependentsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.DeductionArrangement.Id, actual.DeductionArrangement.Id);
                Assert.AreEqual(expected.Dependent.Person.Id, actual.Dependent.Person.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependents_KeyNotFoundException()
        {
            Paging paging = new Paging(personBenefitDependentsCollection.Count(), 0);

            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependents_PermissionsException()
        {
            Paging paging = new Paging(personBenefitDependentsCollection.Count(), 0);

            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependents_ArgumentException()
        {
            Paging paging = new Paging(personBenefitDependentsCollection.Count(), 0);

            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependents_RepositoryException()
        {
            Paging paging = new Paging(personBenefitDependentsCollection.Count(), 0);

            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependents_IntegrationApiException()
        {
            Paging paging = new Paging(personBenefitDependentsCollection.Count(), 0);

            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsAsync(paging);
        }

        [TestMethod]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependentsByGuidAsync_ValidateFields()
        {
            var expected = personBenefitDependentsCollection.FirstOrDefault();
            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await personBenefitDependentsController.GetPersonBenefitDependentsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.DeductionArrangement.Id, actual.DeductionArrangement.Id);
            Assert.AreEqual(expected.Dependent.Person.Id, actual.Dependent.Person.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependents_Exception()
        {
            Paging paging = new Paging(personBenefitDependentsCollection.Count(), 0);

            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<Exception>();
            await personBenefitDependentsController.GetPersonBenefitDependentsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependentsByGuidAsync_Exception()
        {
            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await personBenefitDependentsController.GetPersonBenefitDependentsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependentsByGuid_KeyNotFoundException()
        {
            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependentsByGuid_PermissionsException()
        {
            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependentsByGuid_ArgumentException()
        {
            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependentsByGuid_RepositoryException()
        {
            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependentsByGuid_IntegrationApiException()
        {
            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await personBenefitDependentsController.GetPersonBenefitDependentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_GetPersonBenefitDependentsByGuid_Exception()
        {
            personBenefitDependentsServiceMock.Setup(x => x.GetPersonBenefitDependentsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await personBenefitDependentsController.GetPersonBenefitDependentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_PostPersonBenefitDependentsAsync_Exception()
        {
            await personBenefitDependentsController.PostPersonBenefitDependentsAsync(personBenefitDependentsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_PutPersonBenefitDependentsAsync_Exception()
        {
            var sourceContext = personBenefitDependentsCollection.FirstOrDefault();
            await personBenefitDependentsController.PutPersonBenefitDependentsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonBenefitDependentsController_DeletePersonBenefitDependentsAsync_Exception()
        {
            await personBenefitDependentsController.DeletePersonBenefitDependentsAsync(personBenefitDependentsCollection.FirstOrDefault().Id);
        }
    }
}