// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class EnrollmentStatusesControllerTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<ICurriculumService> curriculumServiceMock;
        private ICurriculumService curriculumService;
        private ILogger logger = new Mock<ILogger>().Object;

        private EnrollmentStatusesController enrollmentStatusesController;
        List<EnrollmentStatus> allEnrollmentStatuses = new List<EnrollmentStatus>();
        private List<Dtos.EnrollmentStatus> allEnrollmentStatusDtos = new List<Dtos.EnrollmentStatus>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            curriculumServiceMock = new Mock<ICurriculumService>();
            curriculumService = curriculumServiceMock.Object;

            allEnrollmentStatuses.Add(new Domain.Student.Entities.EnrollmentStatus("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "A", "Active", Domain.Student.Entities.EnrollmentStatusType.active));
            allEnrollmentStatuses.Add(new Domain.Student.Entities.EnrollmentStatus("73244057-D1EC-4094-A0B7-DE602533E3A6", "W", "Withdrawn", Domain.Student.Entities.EnrollmentStatusType.inactive));
            allEnrollmentStatuses.Add(new Domain.Student.Entities.EnrollmentStatus("1df164eb-8178-4321-a9f7-24f12d3991d8", "G", "Graduated", Domain.Student.Entities.EnrollmentStatusType.complete));

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus, Dtos.EnrollmentStatus>();
            foreach (var enrollmentStatus in allEnrollmentStatuses)
            {
                Dtos.EnrollmentStatus target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus, Dtos.EnrollmentStatus>(enrollmentStatus);
                allEnrollmentStatusDtos.Add(target);
            }

            enrollmentStatusesController = new EnrollmentStatusesController(adapterRegistry, referenceDataRepository, curriculumService, logger);
            enrollmentStatusesController.Request = new HttpRequestMessage();
            enrollmentStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            enrollmentStatusesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task EnrollmentStatusesController_GetEnrollmentStatusesAsync_ValidateFields()
        {
            curriculumServiceMock.Setup(x => x.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allEnrollmentStatusDtos);

            var enrollmentStatuses = (await enrollmentStatusesController.GetEnrollmentStatusesAsync()).ToList();
            Assert.AreEqual(allEnrollmentStatusDtos.Count, enrollmentStatuses.Count);
            for (int i = 0; i < enrollmentStatuses.Count; i++)
            {
                var expected = allEnrollmentStatusDtos[i];
                var actual = enrollmentStatuses[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.enrollmentStatusType, actual.enrollmentStatusType, "Enrollment Status Type, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EnrollmentStatusesController_GetEnrollmentStatusByIdAsync_ValidateFields()
        {
            var expected = allEnrollmentStatusDtos.FirstOrDefault();
            curriculumServiceMock.Setup(x => x.GetEnrollmentStatusByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var enrollmentStatuses = (await enrollmentStatusesController.GetEnrollmentStatusesAsync()).ToList();
            var actual = await enrollmentStatusesController.GetEnrollmentStatusByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.enrollmentStatusType, actual.enrollmentStatusType, "Enrollment Status Type");
        }

        [TestMethod]
        public async Task EnrollmentStatusesController_GetHedmAsync_CacheControlNotNull()
        {
            enrollmentStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            curriculumServiceMock.Setup(x => x.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allEnrollmentStatusDtos);

            List<Dtos.EnrollmentStatus> EnrollmentStatuses = await enrollmentStatusesController.GetEnrollmentStatusesAsync() as List<Dtos.EnrollmentStatus>;
            Dtos.EnrollmentStatus es = EnrollmentStatuses.Where(a => a.Code == "A").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus est = allEnrollmentStatuses.Where(a => a.Code == "A").FirstOrDefault();
            Assert.AreEqual(est.Code, es.Code);
            Assert.AreEqual(est.Description, es.Description);
        }

        [TestMethod]
        public async Task EnrollmentStatusesController_GetHedmAsync_NoCache()
        {
            enrollmentStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            enrollmentStatusesController.Request.Headers.CacheControl.NoCache = true;
            curriculumServiceMock.Setup(x => x.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allEnrollmentStatusDtos);

            List<Dtos.EnrollmentStatus> EnrollmentStatuses = await enrollmentStatusesController.GetEnrollmentStatusesAsync() as List<Dtos.EnrollmentStatus>;
            Dtos.EnrollmentStatus es = EnrollmentStatuses.Where(a => a.Code == "A").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus est = allEnrollmentStatuses.Where(a => a.Code == "A").FirstOrDefault();
            Assert.AreEqual(est.Code, es.Code);
            Assert.AreEqual(est.Description, es.Description);
        }

        [TestMethod]
        public async Task EnrollmentStatusesController_GetHedmAsync_Cache()
        {
            enrollmentStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            enrollmentStatusesController.Request.Headers.CacheControl.NoCache = false;
            curriculumServiceMock.Setup(x => x.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allEnrollmentStatusDtos);

            List<Dtos.EnrollmentStatus> EnrollmentStatuses = await enrollmentStatusesController.GetEnrollmentStatusesAsync() as List<Dtos.EnrollmentStatus>;
            Dtos.EnrollmentStatus es = EnrollmentStatuses.Where(a => a.Code == "A").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus est = allEnrollmentStatuses.Where(a => a.Code == "A").FirstOrDefault();
            Assert.AreEqual(est.Code, es.Code);
            Assert.AreEqual(est.Description, es.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EnrollmentStatusesController_GetThrowsIntAppiExc()
        {
            curriculumServiceMock.Setup(ge => ge.GetEnrollmentStatusesAsync(It.IsAny<bool>())).Throws<Exception>();

            await enrollmentStatusesController.GetEnrollmentStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EnrollmentStatusesController_GetByIdThrowsIntAppiExc()
        {
            curriculumServiceMock.Setup(gc => gc.GetEnrollmentStatusByGuidAsync(It.IsAny<string>())).Throws<Exception>();

            await enrollmentStatusesController.GetEnrollmentStatusByIdAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EnrollmentStatusesController_PostEnrollmentStatusAsync()
        {
            var response = await enrollmentStatusesController.PostEnrollmentStatusAsync(allEnrollmentStatusDtos.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EnrollmentStatusesController_PutEnrollmentStatusAsync()
        {
            var enrollmentStatus = allEnrollmentStatusDtos.FirstOrDefault();
            var response = await enrollmentStatusesController.PutEnrollmentStatusAsync(enrollmentStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EnrollmentStatusesController_DeleteEnrollmentStatusAsync()
        {
            await enrollmentStatusesController.DeleteEnrollmentStatusAsync(allEnrollmentStatusDtos.FirstOrDefault().Id);
        }
    }
}