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
    public class SectionGradeTypesControllerTests
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

        private string sectionGradeTypeId;

        private SectionGradeType expectedSectionGradeType;
        private SectionGradeType testSectionGradeType;
        private SectionGradeType actualSectionGradeType;

        private SectionGradeTypesController sectionGradeTypesController;


        public async Task<List<SectionGradeType>> getActualSectionGradeTypes()
        {
            return (await sectionGradeTypesController.GetSectionGradeTypesAsync()).ToList();
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

            sectionGradeTypeId = "idc2935b-29e8-675f-907b-15a34da4f433";

            expectedSectionGradeType = new SectionGradeType()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Code = "MID1",
                Title = "Midterm Grade 1",
                Description = null,
            };

            testSectionGradeType = new SectionGradeType();
            foreach (var property in typeof(SectionGradeType).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testSectionGradeType, property.GetValue(expectedSectionGradeType, null), null);
            }
            curriculumServiceMock.Setup<Task<SectionGradeType>>(s => s.GetSectionGradeTypeByGuidAsync(sectionGradeTypeId)).Returns(Task.FromResult(testSectionGradeType));

            sectionGradeTypesController = new SectionGradeTypesController(adapterRegistryMock.Object, studentReferenceDataRepositoryMock.Object, curriculumServiceMock.Object, loggerMock.Object);
            actualSectionGradeType = await sectionGradeTypesController.GetSectionGradeTypeByIdAsync(sectionGradeTypeId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            curriculumServiceMock = null;
            studentReferenceDataRepositoryMock = null;
            sectionGradeTypeId = null;
            expectedSectionGradeType = null;
            testSectionGradeType = null;
            actualSectionGradeType = null;
            sectionGradeTypesController = null;
        }

        [TestMethod]
        public void SectionGradeTypesTypeTest()
        {
            Assert.AreEqual(typeof(SectionGradeType), actualSectionGradeType.GetType());
            Assert.AreEqual(expectedSectionGradeType.GetType(), actualSectionGradeType.GetType());
        }

        [TestMethod]
        public void NumberOfKnownPropertiesTest()
        {
            var sectionGradeTypeProperties = typeof(SectionGradeType).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(5, sectionGradeTypeProperties.Length);
        }
    }

    [TestClass]
    public class SectionGradeTypesController_GetAllTests
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

        private SectionGradeTypesController SectionGradeTypeController;
        private Mock<ICurriculumService> CurriculumServiceMock;
        private Mock<IStudentReferenceDataRepository> StudentReferenceDataRepositoryMock;
        private ICurriculumService CurriculumService;
        private IAdapterRegistry AdapterRegistry;
        private List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType> allSectionGradeTypes;
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
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType, SectionGradeType>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            allSectionGradeTypes = new TestStudentReferenceDataRepository().GetSectionGradeTypesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType>;
            var SectionGradeTypesList = new List<SectionGradeType>();

            SectionGradeTypeController = new SectionGradeTypesController(AdapterRegistry, StudentReferenceDataRepositoryMock.Object, CurriculumService, logger);
            SectionGradeTypeController.Request = new HttpRequestMessage();
            SectionGradeTypeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var sectionGradeType in allSectionGradeTypes)
            {
                SectionGradeType target = ConvertSectionGradeTypesEntitytoSectionGradeTypeDto(sectionGradeType);
                SectionGradeTypesList.Add(target);
            }

            CurriculumServiceMock.Setup<Task<IEnumerable<SectionGradeType>>>(s => s.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(SectionGradeTypesList);
        }

        [TestCleanup]
        public void Cleanup()
        {
            SectionGradeTypeController = null;
            CurriculumService = null;
        }

        [TestMethod]
        public async Task ReturnsAllSectionGradeTypes()
        {
            List<SectionGradeType> SectionGradeTypes = await SectionGradeTypeController.GetSectionGradeTypesAsync() as List<SectionGradeType>;
            Assert.AreEqual(SectionGradeTypes.Count, allSectionGradeTypes.Count);
        }

        [TestMethod]
        public async Task GetSectionGradeTypes_Properties()
        {
            List<SectionGradeType> SectionGradeTypes = await SectionGradeTypeController.GetSectionGradeTypesAsync() as List<SectionGradeType>;
            SectionGradeType sg = SectionGradeTypes.Where(a => a.Code == "MID1").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.SectionGradeType sgt = allSectionGradeTypes.Where(a => a.Code == "MID1").FirstOrDefault();
            Assert.AreEqual(sgt.Code, sg.Code);
            Assert.AreEqual(sgt.Description, sg.Title);
        }

        [TestMethod]
        public async Task SecGrdTypeController_GetHedmAsync_CacheControlNotNull()
        {
            SectionGradeTypeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            List<SectionGradeType> SectionGradeTypes = await SectionGradeTypeController.GetSectionGradeTypesAsync() as List<SectionGradeType>;
            SectionGradeType sg = SectionGradeTypes.Where(a => a.Code == "MID1").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.SectionGradeType sgt = allSectionGradeTypes.Where(a => a.Code == "MID1").FirstOrDefault();
            Assert.AreEqual(sgt.Code, sg.Code);
            Assert.AreEqual(sgt.Description, sg.Title);
        }

        [TestMethod]
        public async Task SecGrdTypeController_GetHedmAsync_NoCache()
        {
            SectionGradeTypeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            SectionGradeTypeController.Request.Headers.CacheControl.NoCache = true;

            List<SectionGradeType> SectionGradeTypes = await SectionGradeTypeController.GetSectionGradeTypesAsync() as List<SectionGradeType>;
            SectionGradeType sg = SectionGradeTypes.Where(a => a.Code == "MID1").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.SectionGradeType sgt = allSectionGradeTypes.Where(a => a.Code == "MID1").FirstOrDefault();
            Assert.AreEqual(sgt.Code, sg.Code);
            Assert.AreEqual(sgt.Description, sg.Title);
        }

        [TestMethod]
        public async Task SecGrdTypeController_GetHedmAsync_Cache()
        {
            SectionGradeTypeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            SectionGradeTypeController.Request.Headers.CacheControl.NoCache = false;

            List<SectionGradeType> SectionGradeTypes = await SectionGradeTypeController.GetSectionGradeTypesAsync() as List<SectionGradeType>;
            SectionGradeType sg = SectionGradeTypes.Where(a => a.Code == "MID1").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.SectionGradeType sgt = allSectionGradeTypes.Where(a => a.Code == "MID1").FirstOrDefault();
            Assert.AreEqual(sgt.Code, sg.Code);
            Assert.AreEqual(sgt.Description, sg.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SecGrdTypeController_GetThrowsIntAppiExc()
        {
            CurriculumServiceMock.Setup(sg => sg.GetSectionGradeTypesAsync(It.IsAny<bool>())).Throws<Exception>();

            await SectionGradeTypeController.GetSectionGradeTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SecGrdTypeController_GetByIdThrowsIntAppiExc()
        {
            CurriculumServiceMock.Setup(sg => sg.GetSectionGradeTypeByGuidAsync(It.IsAny<string>())).Throws<Exception>();

            await SectionGradeTypeController.GetSectionGradeTypeByIdAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SecGradeTypeController_DeleteThrowsIntApiExc()
        {
            await SectionGradeTypeController.DeleteSectionGradeTypesAsync("MID1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SecGradeTypeController_PostThrowsIntAppiExc()
        {
            SectionGradeType sgtDTO = await SectionGradeTypeController.PostSectionGradeTypesAsync(new SectionGradeType());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SecGradeTypeController_PutThrowsIntApiExc()
        {
            SectionGradeType sgtDTO = await SectionGradeTypeController.PutSectionGradeTypesAsync("hdgs9093hf", new SectionGradeType());
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a SectionGradeType domain entity to its corresponding Section Grade Type DTO
        /// </summary>
        /// <param name="source">Section Grade Type domain entity</param>
        /// <returns>SectionGradeType DTO</returns>
        private Dtos.SectionGradeType ConvertSectionGradeTypesEntitytoSectionGradeTypeDto(Domain.Student.Entities.SectionGradeType source)
        {
            var sectionGradeType = new Dtos.SectionGradeType();
            sectionGradeType.Id = source.Guid;
            sectionGradeType.Code = source.Code;
            sectionGradeType.Title = source.Description;
            sectionGradeType.Description = null;
            return sectionGradeType;
        }
    }
}
