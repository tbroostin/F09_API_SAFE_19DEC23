//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using System.Configuration;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonExternalEducationCredentialsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPersonExternalEducationCredentialsService> personExternalEducationCredentialsServiceMock;
        private Mock<ILogger> loggerMock;
        private PersonExternalEducationCredentialsController personExternalEducationCredentialsController;
        private IEnumerable<Domain.Base.Entities.ExternalEducation> allExternalEducationEntities;
        private List<Dtos.PersonExternalEducationCredentials> personExternalEducationCredentialsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Ellucian.Web.Http.Models.QueryStringFilter studentCriteriaFilter
         = new Web.Http.Models.QueryStringFilter("student", "");
        private string studentFilter;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            personExternalEducationCredentialsServiceMock = new Mock<IPersonExternalEducationCredentialsService>();
            loggerMock = new Mock<ILogger>();
            personExternalEducationCredentialsCollection = new List<Dtos.PersonExternalEducationCredentials>();


            var personExternalEducationCredentials1 = new Ellucian.Colleague.Dtos.PersonExternalEducationCredentials()
            {
                Id = "506d7c79-ebf3-4557-821c-f70afaf8c9dd",
                AttendancePeriods = new List<Dtos.DtoProperties.PersonExternalEducationCredentialsAttendanceperiodsDtoProperty>()
                    {
                        new Dtos.DtoProperties.PersonExternalEducationCredentialsAttendanceperiodsDtoProperty()
                        {
                            StartOn = DateTime.Now.AddYears(-1),
                            EndOn = DateTime.Now.AddYears(1)
                        }
                    },
                ClassPercentile = 10m,
                ClassRank = 10,
                ClassSize = 100,
                Credential = new GuidObject2("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),
                Disciplines = new List<GuidObject2>() { new GuidObject2("9b3976ad-8b28-42fb-a35d-680a57bf8c93") },
                EarnedOn = DateTime.Now.AddYears(1),
                ExternalEducation = new GuidObject2("0487232e-f38b-4fe6-a020-539622bd8ea1"),
                PerformanceMeasure = "100",
                Recognitions = new List<GuidObject2>() { new GuidObject2("e9c7646b-d922-4c15-adaf-2b2e30fdc8e2") },
                ThesisTitle = "test",
                SupplementalCredentials = new List<Dtos.DtoProperties.PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty>()
                    {  new Dtos.DtoProperties.PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty()
                    { Credential = new GuidObject2("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),
                    EarnedOn = DateTime.Now.AddYears(1)} }

            };
            personExternalEducationCredentialsCollection.Add(personExternalEducationCredentials1);

            var personExternalEducationCredentials2 = new Ellucian.Colleague.Dtos.PersonExternalEducationCredentials()
            {
                Id = "606d7c79-ebf3-4557-821c-f70afaf8c9dd",
                AttendancePeriods = new List<Dtos.DtoProperties.PersonExternalEducationCredentialsAttendanceperiodsDtoProperty>()
                    {
                        new Dtos.DtoProperties.PersonExternalEducationCredentialsAttendanceperiodsDtoProperty()
                        {
                            StartOn = DateTime.Now.AddDays(1),
                            EndOn = DateTime.Now.AddDays(5)
                        }
                    },
                ClassPercentile = 10m,
                ClassRank = 10,
                ClassSize = 100,
                Credential = new GuidObject2("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),
                Disciplines = new List<GuidObject2>() { new GuidObject2("9b3976ad-8b28-42fb-a35d-680a57bf8c93") },
                EarnedOn = DateTime.Now.AddYears(1),
                ExternalEducation = new GuidObject2("0487232e-f38b-4fe6-a020-539622bd8ea1"),
                PerformanceMeasure = "100",
                Recognitions = new List<GuidObject2>() { new GuidObject2("e9c7646b-d922-4c15-adaf-2b2e30fdc8e2") },
                ThesisTitle = "test",
                SupplementalCredentials = new List<Dtos.DtoProperties.PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty>()
                    {  new Dtos.DtoProperties.PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty()
                    { Credential = new GuidObject2("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),
                    EarnedOn = DateTime.Now.AddYears(1)} }

            };
            personExternalEducationCredentialsCollection.Add(personExternalEducationCredentials2);

            var personExternalEducationCredentials3 = new Ellucian.Colleague.Dtos.PersonExternalEducationCredentials()
            {
                Id = "256d7c79-ebf3-4557-821c-f70afaf8c9dc",
                AttendancePeriods = new List<Dtos.DtoProperties.PersonExternalEducationCredentialsAttendanceperiodsDtoProperty>()
                    {
                        new Dtos.DtoProperties.PersonExternalEducationCredentialsAttendanceperiodsDtoProperty()
                        {
                            StartOn = DateTime.Now.AddYears(-1),
                            EndOn = DateTime.Now.AddYears(2)
                        }
                    },
                ClassPercentile = 10m,
                ClassRank = 10,
                ClassSize = 100,
                Credential = new GuidObject2("2e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),
                Disciplines = new List<GuidObject2>() { new GuidObject2("9b3976ad-8b28-42fb-a35d-680a57bf8c93") },
                EarnedOn = DateTime.Now.AddYears(2),
                ExternalEducation = new GuidObject2("1487232e-f38b-4fe6-a020-539622bd8ea1"),
                PerformanceMeasure = "100",
                Recognitions = new List<GuidObject2>() { new GuidObject2("f9c7646b-d922-4c15-adaf-2b2e30fdc8e2") },
                ThesisTitle = "test",
                SupplementalCredentials = new List<Dtos.DtoProperties.PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty>()
                    {  new Dtos.DtoProperties.PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty()
                    { Credential = new GuidObject2("2e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),
                    EarnedOn = DateTime.Now.AddYears(2)} }

            };
            personExternalEducationCredentialsCollection.Add(personExternalEducationCredentials3);

            var gradeOptionsTuple = new Tuple<IEnumerable<Dtos.PersonExternalEducationCredentials>, int>(personExternalEducationCredentialsCollection, 3);
            studentFilter = string.Empty;
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(gradeOptionsTuple);

            personExternalEducationCredentialsController = new PersonExternalEducationCredentialsController(personExternalEducationCredentialsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            personExternalEducationCredentialsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            personExternalEducationCredentialsController = null;
            allExternalEducationEntities = null;
            personExternalEducationCredentialsCollection = null;
            loggerMock = null;
            personExternalEducationCredentialsServiceMock = null;
        }

        #region GET

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentialsByGuidAsync_Exception()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentialsByGuid_KeyNotFoundException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentialsByGuid_PermissionsException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentialsByGuid_ArgumentException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentialsByGuid_RepositoryException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentialsByGuid_IntegrationApiException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentialsByGuid_Exception()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        public async Task GetPersonExternalEducationCredentialsAsync_NoCache()
        {
            //personExternalEducationCredentialsController.Request.Headers.CacheControl =
            //     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personExternalEducationCredentialsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personExternalEducationCredentialsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            personExternalEducationCredentialsController.Request.Headers.CacheControl.NoCache = true;

            Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials>, int> studentAdvRelTuple
                = new Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials>, int>(personExternalEducationCredentialsCollection, 3);

            // studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAdvRelTuple);

            var sourceContexts = await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsAsync(new Paging(1, 0), studentCriteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials>>)httpResponseMessage.Content).Value as IEnumerable<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials>;

            Assert.IsTrue(sourceContexts is IHttpActionResult);
            Assert.AreEqual(personExternalEducationCredentialsCollection.Count, results.ToList().Count());
            foreach (var result in results.ToList())
            {
                var expected = personExternalEducationCredentialsCollection.FirstOrDefault(s => s.Id == result.Id);
                Assert.AreEqual(expected.Id, result.Id);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentialsAsync_Exception()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentials_KeyNotFoundException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentials_PermissionsException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetStudentTranscriptGradesOptionns_ArgumentException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentials_RepositoryException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentials_IntegrationApiException()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_GetPersonExternalEducationCredentials_Exception()
        {
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personExternalEducationCredentialsController.GetPersonExternalEducationCredentialsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_DeletePersonExternalEducationCredentialsAsync_Exception()
        {
            await personExternalEducationCredentialsController.DeletePersonExternalEducationCredentialsAsync(personExternalEducationCredentialsCollection.FirstOrDefault().Id);
        }

        #endregion

        #region PUT

        [TestMethod]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            personExternalEducationCredentialsServiceMock.Setup(x => x.UpdatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>())).ReturnsAsync(expected);

            var actual = await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(expected.Id, expected);

            Assert.AreEqual(expected.Id, actual.Id, "Id");

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_EmtpyGuid()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync("", expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_EmtpyBody()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync("invalid", null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_NilGuid()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();


            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(Guid.Empty.ToString(), expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_GuidMisMatch()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();


            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(Guid.NewGuid().ToString(), expected);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_InvalidKey()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(expected.Id, It.IsAny<bool>()))
                     .Throws<KeyNotFoundException>();

            personExternalEducationCredentialsServiceMock.Setup(x => x.UpdatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>()))
               .Throws<KeyNotFoundException>();

            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_RepositoryExcecption()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<RepositoryException>();

            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_PermissionsException()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<PermissionsException>();

            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_ArgumentException()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<ArgumentException>();

            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_IntegrationApiException()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<IntegrationApiException>();

            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_ConfigurationException()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<ConfigurationException>();

            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PutPersonExternalEducationCredentialsAsync_Exception()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<Exception>();

            await personExternalEducationCredentialsController.PutPersonExternalEducationCredentialsAsync(expected.Id, expected);
        }

        #endregion

        #region POST

        [TestMethod]
        public async Task PersonExternalEducationCredentialsController_PostPersonExternalEducationCredentialsAsync()
        {
            var record = personExternalEducationCredentialsCollection.FirstOrDefault();
           
            personExternalEducationCredentialsServiceMock.Setup(x => x.GetPersonExternalEducationCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(record);
            personExternalEducationCredentialsServiceMock.Setup(x => x.CreatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>())).ReturnsAsync(record);

            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();
            expected.Id = Guid.Empty.ToString();
            var actual = await personExternalEducationCredentialsController.PostPersonExternalEducationCredentialsAsync(expected);

            Assert.AreEqual(expected.Id, actual.Id, "Id");

        }

      
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PostPersonExternalEducationCredentialsAsync_EmtpyBody()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();

            await personExternalEducationCredentialsController.PostPersonExternalEducationCredentialsAsync(null);
        }

      

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PostPersonExternalEducationCredentialsAsync_InvalidKey()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();
            expected.Id = Guid.Empty.ToString();
            personExternalEducationCredentialsServiceMock.Setup(x => x.CreatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>()))
                     .Throws<KeyNotFoundException>();

            personExternalEducationCredentialsServiceMock.Setup(x => x.UpdatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>()))
               .Throws<KeyNotFoundException>();

            await personExternalEducationCredentialsController.PostPersonExternalEducationCredentialsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PostPersonExternalEducationCredentialsAsync_RepositoryExcecption()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();
            expected.Id = Guid.Empty.ToString();
            personExternalEducationCredentialsServiceMock.Setup(x => x.CreatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>()))
                .Throws<RepositoryException>();

            await personExternalEducationCredentialsController.PostPersonExternalEducationCredentialsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PostPersonExternalEducationCredentialsAsync_PermissionsException()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();
            expected.Id = Guid.Empty.ToString();
            personExternalEducationCredentialsServiceMock.Setup(x => x.CreatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>()))
                 .Throws<PermissionsException>();

            await personExternalEducationCredentialsController.PostPersonExternalEducationCredentialsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PostPersonExternalEducationCredentialsAsync_ArgumentException()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();
            expected.Id = Guid.Empty.ToString();
            personExternalEducationCredentialsServiceMock.Setup(x => x.CreatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>()))
                .Throws<ArgumentException>();

            await personExternalEducationCredentialsController.PostPersonExternalEducationCredentialsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PostPersonExternalEducationCredentialsAsync_IntegrationApiException()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();
            expected.Id = Guid.Empty.ToString();
            personExternalEducationCredentialsServiceMock.Setup(x => x.CreatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>()))
                .Throws<IntegrationApiException>();

            await personExternalEducationCredentialsController.PostPersonExternalEducationCredentialsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PostPersonExternalEducationCredentialsAsync_ConfigurationException()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();
            expected.Id = Guid.Empty.ToString();
            personExternalEducationCredentialsServiceMock.Setup(x => x.CreatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>()))
                .Throws<ConfigurationException>();

            await personExternalEducationCredentialsController.PostPersonExternalEducationCredentialsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationCredentialsController_PostPersonExternalEducationCredentialsAsync_Exception()
        {
            var expected = personExternalEducationCredentialsCollection.FirstOrDefault();
            expected.Id = Guid.Empty.ToString();
            personExternalEducationCredentialsServiceMock.Setup(x => x.CreatePersonExternalEducationCredentialsAsync(It.IsAny<PersonExternalEducationCredentials>()))
                .Throws<Exception>();

            await personExternalEducationCredentialsController.PostPersonExternalEducationCredentialsAsync(expected);
        }

        #endregion


    }
}