// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using AcademicPeriodEnrollmentStatus = Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AcademicPeriodEnrollmentStatusesControllerTests
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private AcademicPeriodEnrollmentStatusesController _academicPeriodEnrollmentStatusesController;
        //private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
        private Mock<ICurriculumService> _curriculumServiceMock;
        private IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger = new Mock<ILogger>().Object;

        private List<AcademicPeriodEnrollmentStatus> acadPeriodEnrlStatuss = new List<AcademicPeriodEnrollmentStatus>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            //_studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _curriculumServiceMock = new Mock<ICurriculumService>();

            var adapters = new HashSet<ITypeAdapter>();
            _adapterRegistry = new AdapterRegistry(adapters, _logger);

            acadPeriodEnrlStatuss = BuildStatuses();

            _academicPeriodEnrollmentStatusesController = new AcademicPeriodEnrollmentStatusesController(_adapterRegistry, _curriculumServiceMock.Object,  _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicPeriodEnrollmentStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());           
        }

        private List<AcademicPeriodEnrollmentStatus> BuildStatuses()
        {
            List<AcademicPeriodEnrollmentStatus> statuses = new List<AcademicPeriodEnrollmentStatus>() 
            { 
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "A",
                    Title = "Active",
                    Description = "Active",
                    Id = "891ab399-259f-4597-aca7-57b1a7f31626"
                },
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "P",
                    Title = "Potential",
                    Description = "Potential",
                    Id = "e37f3d90-622e-4636-9110-bdb18ad538e5"
                },
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "W",
                    Title = "Withdrawn",
                    Description = "Withdrawn",
                    Id = "0d383982-fa39-4c3d-9a50-28223014c3c6"
                },
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "G",
                    Title = "Graduated",
                    Description = "Graduated",
                    Id = "3a8500a2-9d16-4be4-8d08-3a86199cf9a9"
                },
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "c",
                    Title = "Changed Program",
                    Description = "Changed Program",
                    Id = "3c6d72b0-1915-42a7-abe4-cac21d3166b2"
                },
            };

            return statuses;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicPeriodEnrollmentStatusesController = null;
            _curriculumServiceMock = null;
            _adapterRegistry = null;
        }

        [TestMethod]
        public async Task AcadPeriodEnrllStatusesController_GetAcademicPeriodEnrollmentStatusesAsync_Cache_True()
        {
            _academicPeriodEnrollmentStatusesController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            _curriculumServiceMock.Setup(i => i.GetAcademicPeriodEnrollmentStatusesAsync(true)).ReturnsAsync(acadPeriodEnrlStatuss);
            var actual = await _academicPeriodEnrollmentStatusesController.GetAcademicPeriodEnrollmentStatusesAsync();

            Assert.AreEqual(actual.Count(), 5); 
        }

        [TestMethod]
        public async Task AcadPeriodEnrllStatusesController_GetAcademicPeriodEnrollmentStatusesAsync_Cache_False()
        {
            _academicPeriodEnrollmentStatusesController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false
            };
            _curriculumServiceMock.Setup(i => i.GetAcademicPeriodEnrollmentStatusesAsync(false)).ReturnsAsync(acadPeriodEnrlStatuss);
            var actual = await _academicPeriodEnrollmentStatusesController.GetAcademicPeriodEnrollmentStatusesAsync();

            Assert.AreEqual(actual.Count(), 5);
        }

        [TestMethod]
        public async Task AcadPeriodEnrllStatusesController_GetAcademicPeriodEnrollmentStatusByIdAsync()
        {
            string guid = "e37f3d90-622e-4636-9110-bdb18ad538e5";
            var expected = acadPeriodEnrlStatuss.FirstOrDefault(item => item.Id.Equals(guid, StringComparison.OrdinalIgnoreCase));
            _curriculumServiceMock.Setup(i => i.GetAcademicPeriodEnrollmentStatusByGuidAsync(guid)).ReturnsAsync(expected);
            var actual = await _academicPeriodEnrollmentStatusesController.GetAcademicPeriodEnrollmentStatusByIdAsync(guid);

            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Title, actual.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcadPeriodEnrllStatusesController_GetAcademicPeriodEnrollmentStatusesAsync_Exception()
        {
            _curriculumServiceMock.Setup(i => i.GetAcademicPeriodEnrollmentStatusesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            var actual = await _academicPeriodEnrollmentStatusesController.GetAcademicPeriodEnrollmentStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcadPeriodEnrllStatusesController_GetAcademicPeriodEnrollmentStatusByIdAsync_Exception()
        {
            _curriculumServiceMock.Setup(i => i.GetAcademicPeriodEnrollmentStatusByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var actual = await _academicPeriodEnrollmentStatusesController.GetAcademicPeriodEnrollmentStatusByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcadPeriodEnrllStatusesController_PUT_Exception()
        {
            var actual = await _academicPeriodEnrollmentStatusesController.PutAcademicPeriodEnrollmentStatusAsync(It.IsAny<string>(), It.IsAny<AcademicPeriodEnrollmentStatus>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcadPeriodEnrllStatusesController_POST_Exception()
        {
            var actual = await _academicPeriodEnrollmentStatusesController.PostAcademicPeriodEnrollmentStatusAsync(It.IsAny<AcademicPeriodEnrollmentStatus>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcadPeriodEnrllStatusesController_DELETE_Exception()
        {
            await _academicPeriodEnrollmentStatusesController.DeleteAcademicPeriodEnrollmentStatusAsync(It.IsAny<string>());
        }
    }   
}