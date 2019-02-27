//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SectionRegistrationsGradeOptionsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ISectionRegistrationService> sectionRegistrationsGradeOptionsServiceMock;
        private Mock<ILogger> loggerMock;
        private SectionRegistrationsGradeOptionsController sectionRegistrationsGradeOptionsController;      
        private IEnumerable<Domain.Student.Entities.StudentAcadCredCourseSecInfo> allStudentAcadCred;
        private List<Dtos.SectionRegistrationsGradeOptions> sectionRegistrationsGradeOptionsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            sectionRegistrationsGradeOptionsServiceMock = new Mock<ISectionRegistrationService>();
            loggerMock = new Mock<ILogger>();
            sectionRegistrationsGradeOptionsCollection = new List<Dtos.SectionRegistrationsGradeOptions>();

            allStudentAcadCred  = new List<Domain.Student.Entities.StudentAcadCredCourseSecInfo>()
                {
                    new Domain.Student.Entities.StudentAcadCredCourseSecInfo("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.StudentAcadCredCourseSecInfo("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.StudentAcadCredCourseSecInfo("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allStudentAcadCred)
            {
                var sectionRegistrationsGradeOptions = new Ellucian.Colleague.Dtos.SectionRegistrationsGradeOptions
                {
                    Id = source.RecordGuid
                };
                sectionRegistrationsGradeOptionsCollection.Add(sectionRegistrationsGradeOptions);
            }
            Tuple<IEnumerable<Dtos.SectionRegistrationsGradeOptions>, int> tuple = new Tuple<IEnumerable<Dtos.SectionRegistrationsGradeOptions>, int>(sectionRegistrationsGradeOptionsCollection, sectionRegistrationsGradeOptionsCollection.Count());
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Dtos.SectionRegistrationsGradeOptions>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            sectionRegistrationsGradeOptionsController = new SectionRegistrationsGradeOptionsController(sectionRegistrationsGradeOptionsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            sectionRegistrationsGradeOptionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionRegistrationsGradeOptionsController = null;
            allStudentAcadCred = null;
            sectionRegistrationsGradeOptionsCollection = null;
            loggerMock = null;
            sectionRegistrationsGradeOptionsServiceMock = null;
        }

        [TestMethod]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptions_ValidateFields_Cache()
        {
            sectionRegistrationsGradeOptionsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            var cancelToken = new System.Threading.CancellationToken(false);

            var sourceContexts = await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsAsync(new Web.Http.Models.Paging(100, 0), new Web.Http.Models.QueryStringFilter("criteria", ""));

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.SectionRegistrationsGradeOptions> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistrationsGradeOptions>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.SectionRegistrationsGradeOptions>;

            var entities = results.ToList();

            Assert.AreEqual(sectionRegistrationsGradeOptionsCollection.Count, entities.Count());
            for (var i = 0; i < entities.Count(); i++)
            {
                var expected = sectionRegistrationsGradeOptionsCollection[i];
                var actual = entities[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                //Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                //Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptions_CheckForEmptyFilterParameters()
        {
            sectionRegistrationsGradeOptionsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            var cancelToken = new System.Threading.CancellationToken(false);
            //EmptyFilterProperties
            sectionRegistrationsGradeOptionsController.Request.Properties.Add("EmptyFilterProperties", true);
            var sourceContexts = await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsAsync(new Web.Http.Models.Paging(100, 0), new Web.Http.Models.QueryStringFilter("criteria", ""));

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.SectionRegistrationsGradeOptions> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistrationsGradeOptions>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.SectionRegistrationsGradeOptions>;

            var entities = results.ToList();

            Assert.AreEqual(0, entities.Count());            
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptions_KeyNotFoundException()
        {
            //
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.SectionRegistrationsGradeOptions>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsAsync(new Web.Http.Models.Paging(100, 0), new Web.Http.Models.QueryStringFilter("criteria", ""));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptions_PermissionsException()
        {

            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.SectionRegistrationsGradeOptions>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsAsync(new Web.Http.Models.Paging(100, 0), new Web.Http.Models.QueryStringFilter("criteria", ""));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptions_ArgumentException()
        {

            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.SectionRegistrationsGradeOptions>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsAsync(new Web.Http.Models.Paging(100, 0), new Web.Http.Models.QueryStringFilter("criteria", ""));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptions_RepositoryException()
        {

            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.SectionRegistrationsGradeOptions>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsAsync(new Web.Http.Models.Paging(100, 0), new Web.Http.Models.QueryStringFilter("criteria", ""));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptions_IntegrationApiException()
        {

            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.SectionRegistrationsGradeOptions>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsAsync(new Web.Http.Models.Paging(100, 0), new Web.Http.Models.QueryStringFilter("criteria", ""));
        }

        [TestMethod]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptionsByGuidAsync_ValidateFields()
        {
            sectionRegistrationsGradeOptionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};
            var expected = sectionRegistrationsGradeOptionsCollection.FirstOrDefault();
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptions_Exception()
        {
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.SectionRegistrationsGradeOptions>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptionsByGuidAsync_Exception()
        {
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptionsByGuid_KeyNotFoundException()
        {
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptionsByGuid_PermissionsException()
        {
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptionsByGuid_ArgumentException()
        {
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptionsByGuid_RepositoryException()
        {
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptionsByGuid_IntegrationApiException()
        {
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_GetSectionRegistrationsGradeOptionsByGuid_Exception()
        {
            sectionRegistrationsGradeOptionsServiceMock.Setup(x => x.GetSectionRegistrationsGradeOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await sectionRegistrationsGradeOptionsController.GetSectionRegistrationsGradeOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_PostSectionRegistrationsGradeOptionsAsync_Exception()
        {
            await sectionRegistrationsGradeOptionsController.PostSectionRegistrationsGradeOptionsAsync(sectionRegistrationsGradeOptionsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_PutSectionRegistrationsGradeOptionsAsync_Exception()
        {
            var sourceContext = sectionRegistrationsGradeOptionsCollection.FirstOrDefault();
            await sectionRegistrationsGradeOptionsController.PutSectionRegistrationsGradeOptionsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationsGradeOptionsController_DeleteSectionRegistrationsGradeOptionsAsync_Exception()
        {
            await sectionRegistrationsGradeOptionsController.DeleteSectionRegistrationsGradeOptionsAsync(sectionRegistrationsGradeOptionsCollection.FirstOrDefault().Id);
        }
    }
}