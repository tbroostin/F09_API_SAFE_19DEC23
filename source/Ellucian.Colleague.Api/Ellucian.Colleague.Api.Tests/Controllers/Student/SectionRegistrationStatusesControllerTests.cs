// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SectionRegistrationStatusesControllerTests
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
        private Mock<ILogger> loggerMock;
        private Mock<ICurriculumService> curriculumServiceMock;
        private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

        private string sectionRegistrationStatusId;

        private SectionRegistrationStatusItem2 expectedSectionRegistrationStatus;
        private SectionRegistrationStatusItem2 testSectionRegistrationStatus;
        private SectionRegistrationStatusItem2 actualSectionRegistrationStatus;

        private SectionRegistrationStatusesController sectionRegistrationStatusesController;


        public async Task<List<SectionRegistrationStatusItem2>> getActualSectioinRegistrationStatuses()
        {
            IEnumerable<SectionRegistrationStatusItem2> sectionRegistrationStatusList = await sectionRegistrationStatusesController.GetSectionRegistrationStatuses2Async();
            return (await sectionRegistrationStatusesController.GetSectionRegistrationStatuses2Async()).ToList();
        }

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            curriculumServiceMock = new Mock<ICurriculumService>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

            sectionRegistrationStatusId = "idc2935b-29e8-675f-907b-15a34da4f433";

            expectedSectionRegistrationStatus = new SectionRegistrationStatusItem2()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Code = "A",
                Title = "Add",
                Description = null,
            };

            testSectionRegistrationStatus = new SectionRegistrationStatusItem2();
            foreach (var property in typeof(SectionRegistrationStatusItem2).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testSectionRegistrationStatus, property.GetValue(expectedSectionRegistrationStatus, null), null);
            }
            curriculumServiceMock.Setup<Task<SectionRegistrationStatusItem2>>(s => s.GetSectionRegistrationStatusById2Async(sectionRegistrationStatusId)).Returns(Task.FromResult(testSectionRegistrationStatus));

            sectionRegistrationStatusesController = new SectionRegistrationStatusesController(adapterRegistryMock.Object, studentReferenceDataRepositoryMock.Object, curriculumServiceMock.Object, loggerMock.Object);
            actualSectionRegistrationStatus = await sectionRegistrationStatusesController.GetSectionRegistrationStatusById2Async(sectionRegistrationStatusId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            curriculumServiceMock = null;
            studentReferenceDataRepositoryMock = null;
            sectionRegistrationStatusId = null;
            expectedSectionRegistrationStatus = null;
            testSectionRegistrationStatus = null;
            actualSectionRegistrationStatus = null;
            sectionRegistrationStatusesController = null;
        }

        [TestMethod]
        public void SectionRegistrationStatusesTypeTest()
        {
            Assert.AreEqual(typeof(SectionRegistrationStatusItem2), actualSectionRegistrationStatus.GetType());
            Assert.AreEqual(expectedSectionRegistrationStatus.GetType(), actualSectionRegistrationStatus.GetType());
        }

        [TestMethod]
        public void NumberOfKnownPropertiesTest()
        {
            var sectionRegistrationStatusProperties = typeof(SectionRegistrationStatusItem2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(6, sectionRegistrationStatusProperties.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationStatusesController_GetSectionRegistrationStatuses2Async_Exception()
        {
            curriculumServiceMock.Setup<Task<IEnumerable<SectionRegistrationStatusItem2>>>(s => s.GetSectionRegistrationStatuses2Async(It.IsAny<bool>())).ThrowsAsync(new Exception());

            var actuals = await sectionRegistrationStatusesController.GetSectionRegistrationStatuses2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationStatusesController_GetSectionRegistrationStatusById2Async_Exception()
        {
            curriculumServiceMock.Setup(s => s.GetSectionRegistrationStatusById2Async(It.IsAny<string>())).ThrowsAsync(new Exception());

            var actual = await sectionRegistrationStatusesController.GetSectionRegistrationStatusById2Async(It.IsAny<string>());
        }
    }

    [TestClass]
    public class SectionRegistratioinStatusesController_GetAllTests
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

        private SectionRegistrationStatusesController SectionRegistrationStatusController;
        private Mock<ICurriculumService> CurriculumServiceMock;
        private Mock<IStudentReferenceDataRepository> StudentReferenceDataRepositoryMock;
        private ICurriculumService CurriculumService;
        private IAdapterRegistry AdapterRegistry;
        private List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem> allSectionRegistrationStatuses;
        ILogger logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            CurriculumServiceMock = new Mock<ICurriculumService>();
            CurriculumService = CurriculumServiceMock.Object;

            StudentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem, SectionRegistrationStatusItem2>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

                        allSectionRegistrationStatuses = new TestStudentReferenceDataRepository().GetStudentAcademicCreditStatusesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem>;
            var SectionRegistrationStatusesList = new List<SectionRegistrationStatusItem2>();

            SectionRegistrationStatusController = new SectionRegistrationStatusesController(AdapterRegistry, StudentReferenceDataRepositoryMock.Object, CurriculumService, logger);
            SectionRegistrationStatusController.Request = new HttpRequestMessage();
            SectionRegistrationStatusController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var sectionRegistrationStatus in allSectionRegistrationStatuses)
            {
                SectionRegistrationStatusItem2 target = ConvertSectionRegistrationStatusesEntitytoSectionRegistrationStatusDto(sectionRegistrationStatus);
                SectionRegistrationStatusesList.Add(target);
            }

            CurriculumServiceMock.Setup<Task<IEnumerable<SectionRegistrationStatusItem2>>>(s => s.GetSectionRegistrationStatuses2Async(false)).ReturnsAsync(SectionRegistrationStatusesList);
        }

        [TestCleanup]
        public void Cleanup()
        {
            SectionRegistrationStatusController = null;
            CurriculumService = null;
        }

        [TestMethod]
        public async Task ReturnsAllSectionRegistrationStatuses()
        {
            List<SectionRegistrationStatusItem2> SectionRegistrationStatuses = await SectionRegistrationStatusController.GetSectionRegistrationStatuses2Async() as List<SectionRegistrationStatusItem2>;
            Assert.AreEqual(SectionRegistrationStatuses.Count, allSectionRegistrationStatuses.Count);
        }

        [TestMethod]
        public async Task GetSectionRegistrationStatuses_Properties()
        {
            List<SectionRegistrationStatusItem2> SectionRegistrationStatuses = await SectionRegistrationStatusController.GetSectionRegistrationStatuses2Async() as List<SectionRegistrationStatusItem2>;
            SectionRegistrationStatusItem2 al = SectionRegistrationStatuses.Where(a => a.Code == "N").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem alt = allSectionRegistrationStatuses.Where(a => a.Code == "N").FirstOrDefault();
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SecRegStatController_DeleteThrowsIntApiExc()
        {
            await SectionRegistrationStatusController.DeleteSectionRegistrationStatusesAsync("SEC100");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SecRegStatController_PostThrowsIntAppiExc()
        {
            SectionRegistrationStatusItem2 srsDTO = await SectionRegistrationStatusController.PostSectionRegistrationStatusesAsync(new SectionRegistrationStatusItem2());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SecRegStatController_PutThrowsIntApiExc()
        {
            SectionRegistrationStatusItem2 srsDTO = await SectionRegistrationStatusController.PutSectionRegistrationStatusesAsync(new SectionRegistrationStatusItem2());
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a SectionRegistrationStatusItem domain entity to its corresponding Section Registration Status Item DTO
        /// </summary>
        /// <param name="source">Section Registration Status Item domain entity</param>
        /// <returns>SectionRegistrationStatusItem2 DTO</returns>
        private Dtos.SectionRegistrationStatusItem2 ConvertSectionRegistrationStatusesEntitytoSectionRegistrationStatusDto(Domain.Student.Entities.SectionRegistrationStatusItem source)
        {
            var sectionRegistrationStatus = new Dtos.SectionRegistrationStatusItem2();
            sectionRegistrationStatus.Id = source.Guid;
            sectionRegistrationStatus.Code = source.Code;
            sectionRegistrationStatus.Title = source.Description;
            sectionRegistrationStatus.Description = null;
            return sectionRegistrationStatus;
        }
    }

    [TestClass]
    public class SectionRegistrationStatusesController_GET_V8
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

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private Mock<ICurriculumService> curriculumServiceMock;
        private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

        List<SectionRegistrationStatusItem3> sectionRegistrationStatusesDtos = new List<SectionRegistrationStatusItem3>();
        private SectionRegistrationStatusesController sectionRegistrationStatusController;
        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            curriculumServiceMock = new Mock<ICurriculumService>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            BuildData();
            sectionRegistrationStatusController = new SectionRegistrationStatusesController(adapterRegistryMock.Object, studentReferenceDataRepositoryMock.Object, curriculumServiceMock.Object, loggerMock.Object);
            sectionRegistrationStatusController.Request = new HttpRequestMessage();
            sectionRegistrationStatusController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            curriculumServiceMock = null;
            studentReferenceDataRepositoryMock = null;
            sectionRegistrationStatusesDtos = null;
            sectionRegistrationStatusController = null;
        }

        [TestMethod]
        public async Task SectionRegistrationStatusesController_GetSectionRegistrationStatuses3Async()
        {
            sectionRegistrationStatusController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            curriculumServiceMock.Setup(s => s.GetSectionRegistrationStatuses3Async(It.IsAny<bool>())).ReturnsAsync(sectionRegistrationStatusesDtos);

            var actuals = await sectionRegistrationStatusController.GetSectionRegistrationStatuses3Async();

            Assert.IsNotNull(actuals);

            Assert.AreEqual(actuals.Count(), sectionRegistrationStatusesDtos.Count());

            foreach (var actual in actuals)
            {
                var expected = sectionRegistrationStatusesDtos.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.Title, actual.Title);
            }
        }

        [TestMethod]
        public async Task SectionRegistrationStatusesController_GetSectionRegistrationStatusById3Async()
        {
            var sectionRegistrationStatus = sectionRegistrationStatusesDtos.FirstOrDefault();
            curriculumServiceMock.Setup(s => s.GetSectionRegistrationStatusById3Async(It.IsAny<string>())).ReturnsAsync(sectionRegistrationStatus);

            var actual = await sectionRegistrationStatusController.GetSectionRegistrationStatusById3Async(It.IsAny<string>());

            var expected = sectionRegistrationStatusesDtos.FirstOrDefault(i => i.Id.Equals(actual.Id));
            Assert.IsNotNull(expected);

            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.Title, actual.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationStatusesController_GetSectionRegistrationStatuses3Async_Exception()
        {
            curriculumServiceMock.Setup<Task<IEnumerable<SectionRegistrationStatusItem3>>>(s => s.GetSectionRegistrationStatuses3Async(It.IsAny<bool>())).ThrowsAsync(new Exception());

            var actuals = await sectionRegistrationStatusController.GetSectionRegistrationStatuses3Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionRegistrationStatusesController_GetSectionRegistrationStatusById3Async_Exception()
        {
            curriculumServiceMock.Setup(s => s.GetSectionRegistrationStatusById3Async(It.IsAny<string>())).ThrowsAsync(new Exception());

            var actual = await sectionRegistrationStatusController.GetSectionRegistrationStatusById3Async(It.IsAny<string>());
        }

        private void BuildData()
        {
            sectionRegistrationStatusesDtos = new List<SectionRegistrationStatusItem3>() 
            {
                new SectionRegistrationStatusItem3()
                { 
                    Code = "N", 
                    Description = "New", 
                    Id = "4e0288fd-1bcb-4ea9-a1bd-be623ea3a3db", 
                    Status = new SectionRegistrationStatusItemStatus()
                    { 
                        RegistrationStatus= RegistrationStatus2.Registered,
                        SectionRegistrationStatusReason = RegistrationStatusReason2.Registered
                    }, 
                    HeadCountStatus = HeadCountStatus.Include, 
                    Title = "New"
                },
                new SectionRegistrationStatusItem3()
                { 
                    Code = "A", 
                    Description = "Add", 
                    Id = "dc390aa1-a460-4c13-8360-b6300b6de394", 
                    Status = new SectionRegistrationStatusItemStatus()
                    { 
                        RegistrationStatus= RegistrationStatus2.Registered,
                        SectionRegistrationStatusReason = RegistrationStatusReason2.Registered
                    }, 
                    HeadCountStatus = HeadCountStatus.Include, 
                    Title = "Add"
                },
                new SectionRegistrationStatusItem3()
                { 
                    Code = "D", 
                    Description = "Dropped", 
                    Id = "4102eac9-8646-4d64-bbe2-2164564b77d4", 
                    Status = new SectionRegistrationStatusItemStatus()
                    { 
                        RegistrationStatus= RegistrationStatus2.NotRegistered,
                        SectionRegistrationStatusReason = RegistrationStatusReason2.Dropped
                    }, 
                    HeadCountStatus = HeadCountStatus.Exclude, 
                    Title = "Dropped"
                },
                new SectionRegistrationStatusItem3()
                { 
                    Code = "W", 
                    Description = "Withdrawn", 
                    Id = "5ce32cba-16e2-4c5e-9796-3d1b59610ec4", 
                    Status = new SectionRegistrationStatusItemStatus()
                    { 
                        RegistrationStatus= RegistrationStatus2.NotRegistered,
                        SectionRegistrationStatusReason = RegistrationStatusReason2.Withdrawn
                    }, 
                    HeadCountStatus = HeadCountStatus.Exclude, 
                    Title = "Withdrawn"
                },
            };
        }
    }
}
