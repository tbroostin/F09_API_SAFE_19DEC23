//// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Reflection;
//using System.Threading.Tasks;
//using System.Web.Http;
//using System.Web.Http.Hosting;
//using Ellucian.Colleague.Configuration.Licensing;
//using Ellucian.Colleague.Coordination.Student.Services;
//using Ellucian.Colleague.Domain.Student.Repositories;
//using Ellucian.Colleague.Domain.Student.Tests;
//using Ellucian.Colleague.Dtos;
//using Ellucian.Web.Adapters;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using slf4net;
//using AcademicLevel = Ellucian.Colleague.Domain.Student.Entities.AcademicLevel;
//using AutoMapper;
//using System.Net.Http.Headers;
//using System;
//using Ellucian.Colleague.Api.Controllers.Student;
//using Ellucian.Web.Http.Exceptions;
//using Ellucian.Web.Security;
//using Ellucian.Colleague.Domain.Exceptions;

//namespace Ellucian.Colleague.Api.Tests.Controllers.Student
//{
//    [TestClass]
//    public class AcademicLevelsControllerTests3
//    {
//        /// <summary>
//        ///     Gets or sets the test context which provides
//        ///     information about and functionality for the current test run.
//        /// </summary>
//        public TestContext TestContext { get; set; }

//        private AcademicLevelsController _academicLevelController;
//        private Mock<ICurriculumService> _curriculumServiceMock;
//        private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
//        private ICurriculumService _curriculumService;
//        private IAdapterRegistry _adapterRegistry;
//        private List<AcademicLevel> _allAcademicLevels;
//        private List<AcademicLevel2> _allAcademicLevelsDto;
//        private readonly ILogger _logger = new Mock<ILogger>().Object;

//        [TestInitialize]
//        public void Initialize()
//        {
//            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
//            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
//            _curriculumServiceMock = new Mock<ICurriculumService>();
//            _curriculumService = _curriculumServiceMock.Object;

//            _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

//            _allAcademicLevelsDto = new List<AcademicLevel2>();
//            var adapters = new HashSet<ITypeAdapter>();
//            _adapterRegistry = new AdapterRegistry(adapters, _logger);
//            var testAdapter = new AutoMapperAdapter<AcademicLevel, AcademicLevel2>(_adapterRegistry, _logger);
//            _adapterRegistry.AddAdapter(testAdapter);

//            _allAcademicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<AcademicLevel>;

//            _academicLevelController = new AcademicLevelsController(_adapterRegistry,
//                _studentReferenceDataRepositoryMock.Object, _curriculumService, _logger)
//            {
//                Request = new HttpRequestMessage()
//            };
//            _academicLevelController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
//                new HttpConfiguration());
//            _academicLevelController.Request.Headers.CacheControl = new CacheControlHeaderValue
//            {
//                NoCache = true,
//                Public = true
//            };
//            Mapper.CreateMap<AcademicLevel, AcademicLevel2>();
//            Debug.Assert(_allAcademicLevels != null, "_allAcademicLevels != null");
//            foreach (var academicLevels in _allAcademicLevels)
//            {
//                var target = Mapper.Map<AcademicLevel, AcademicLevel2>(academicLevels);
//                target.Id = academicLevels.Guid;
//                target.Title = academicLevels.Description;
//                _allAcademicLevelsDto.Add(target);
//            }
//            _curriculumServiceMock.Setup(s => s.GetAcademicLevels2Async(It.IsAny<bool>()))
//                .ReturnsAsync(_allAcademicLevelsDto);
//        }

//        [TestCleanup]
//        public void Cleanup()
//        {
//            _academicLevelController = null;
//            _curriculumService = null;
//            _allAcademicLevels = null;
//            _allAcademicLevelsDto = null;
//            _curriculumServiceMock = null;
//            _studentReferenceDataRepositoryMock = null;
//        }

//        [TestMethod]
//        public async Task AcadeLevelController_ReturnsAllAcademicLevels()
//        {
//            var academicLevels = await _academicLevelController.GetAcademicLevels3Async() as List<AcademicLevel2>;
//            Debug.Assert(academicLevels != null, "academicLevels != null");
//            Assert.AreEqual(academicLevels.Count, _allAcademicLevels.Count);
//        }

//        [TestMethod]
//        public async Task AcadeLevelController_GetAcademicLevelById3Async()
//        {
//            var expected = _allAcademicLevelsDto.FirstOrDefault();
//            _curriculumServiceMock.Setup(x => x.GetAcademicLevelById2Async(expected.Id)).ReturnsAsync(expected);

//            Debug.Assert(expected != null, "expected != null");
//            var actual = await _academicLevelController.GetAcademicLevelById3Async(expected.Id);

//            Assert.AreEqual(expected.Id, actual.Id, "Id");
//            Assert.AreEqual(expected.Title, actual.Title, "Title");
//            Assert.AreEqual(expected.Code, actual.Code, "Code"); ;
//        }


//        [TestMethod]
//        public async Task AcadeLevelController_GetAcademicLevels_ValidateProperties()
//        {
//            var academicLevels = await _academicLevelController.GetAcademicLevels3Async() as List<AcademicLevel2>;
//            Debug.Assert(academicLevels != null, "academicLevels != null");
//            var al = academicLevels.FirstOrDefault(a => a.Code == "UG");
//            var alt = _allAcademicLevels.FirstOrDefault(a => a.Code == "UG");
//            Debug.Assert(alt != null, "alt != null");
//            Debug.Assert(al != null, "al != null");
//            Assert.AreEqual(alt.Code, al.Code);
//            Assert.AreEqual(alt.Description, al.Title);
//        }

//        [TestMethod]
//        public async Task AcadeLevelController_AcademicLevelsTypeTest()
//        {
//            var academicLevels = (await _academicLevelController.GetAcademicLevels3Async()).FirstOrDefault();
//            Debug.Assert(academicLevels != null, "academicLevels != null");
//            Assert.AreEqual(typeof(AcademicLevel2), academicLevels.GetType());
//        }

//        [TestMethod]
//        public void AcadeLevelController_NumberOfKnownPropertiesTest()
//        {
//            var academicLevelProperties =
//                typeof(AcademicLevel2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
//            Assert.AreEqual(5, academicLevelProperties.Length);
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_GetAcademicLevelById3Async_Exception()
//        {
//            var expected = _allAcademicLevelsDto.FirstOrDefault();
//            _curriculumServiceMock.Setup(x => x.GetAcademicLevelById2Async(expected.Id)).Throws<Exception>();
//            Debug.Assert(expected != null, "expected != null");
//            await _academicLevelController.GetAcademicLevelById3Async(expected.Id);
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_GetAcademicLevels3Async_Exception()
//        {
//            _curriculumServiceMock.Setup(s => s.GetAcademicLevels2Async(It.IsAny<bool>())).Throws<Exception>();
//            await _academicLevelController.GetAcademicLevels3Async();
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_GetAcademicLevels3Async_IntegrationApiException()
//        {
//            _curriculumServiceMock.Setup(s => s.GetAcademicLevels2Async(It.IsAny<bool>())).Throws<IntegrationApiException>();
//            await _academicLevelController.GetAcademicLevels3Async();
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_GetAcademicLevelById3Async_PermissionsException()
//        {
//            var expected = _allAcademicLevelsDto.FirstOrDefault();
//            _curriculumServiceMock.Setup(x => x.GetAcademicLevelById2Async(expected.Id)).Throws<PermissionsException>();
//            Debug.Assert(expected != null, "expected != null");
//            await _academicLevelController.GetAcademicLevelById3Async(expected.Id);
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_GetAcademicLevels3Async_PermissionsException()
//        {
//            _curriculumServiceMock.Setup(s => s.GetAcademicLevels2Async(It.IsAny<bool>())).Throws<PermissionsException>();
//            await _academicLevelController.GetAcademicLevels3Async();
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_GetAcademicLevelById3Async_KeyNotFoundException()
//        {
//            var expected = _allAcademicLevelsDto.FirstOrDefault();
//            _curriculumServiceMock.Setup(x => x.GetAcademicLevelById2Async(expected.Id)).Throws<KeyNotFoundException>();
//            Debug.Assert(expected != null, "expected != null");
//            await _academicLevelController.GetAcademicLevelById3Async(expected.Id);
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_GetAcademicLevels3Async_KeyNotFoundException()
//        {
//            _curriculumServiceMock.Setup(s => s.GetAcademicLevels2Async(It.IsAny<bool>())).Throws<KeyNotFoundException>();
//            await _academicLevelController.GetAcademicLevels3Async();
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_GetAcademicLevels3Async_ArgumentNullException()
//        {
//            _curriculumServiceMock.Setup(s => s.GetAcademicLevels2Async(It.IsAny<bool>())).Throws<ArgumentNullException>();
//            await _academicLevelController.GetAcademicLevels3Async();
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_GetAcademicLevels3Async_RepositoryException()
//        {
//            _curriculumServiceMock.Setup(s => s.GetAcademicLevels2Async(It.IsAny<bool>())).Throws<RepositoryException>();
//            await _academicLevelController.GetAcademicLevels3Async();
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadLevelController_DeleteThrowsIntApiExc()
//        {
//            await _academicLevelController.DeleteAcademicLevels2Async("UG");
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadLevelController_PostThrowsIntAppiExc()
//        {
//            await _academicLevelController.PostAcademicLevels2Async(new AcademicLevel2());
//        }

//        [TestMethod]
//        [ExpectedException(typeof(HttpResponseException))]
//        public async Task AcadeLevelController_PutThrowsIntAppiExc()
//        {
//            await _academicLevelController.PutAcademicLevels2Async(new AcademicLevel2());
//        }

//    }
//}
