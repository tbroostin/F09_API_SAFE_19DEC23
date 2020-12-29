// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AcademicProgramControllerGetAllNonHeDM
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private AcademicProgramsController _academicProgramController;
        private Mock<IAcademicProgramService> _academicProgramServiceMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAcademicProgramService _academicProgramService;
        private IAdapterRegistry _adapterRegistry;
        private List<Domain.Student.Entities.AcademicProgram> _allAcademicProgramsEntities;
        private List<Dtos.Student.AcademicProgram> _allAcademicProgramsDtos;
        private readonly ILogger _logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _academicProgramServiceMock = new Mock<IAcademicProgramService>();
            _academicProgramService = _academicProgramServiceMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            _allAcademicProgramsEntities = new TestAcademicProgramRepository().GetAsync().Result as List<Domain.Student.Entities.AcademicProgram>;

            _academicProgramController = new AcademicProgramsController(_adapterRegistry, _academicProgramService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicProgramController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            _academicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            _allAcademicProgramsDtos = new List<Dtos.Student.AcademicProgram>();
            Mapper.CreateMap<Domain.Student.Entities.AcademicProgram, Dtos.Student.AcademicProgram>();
            foreach (var academicProgram in _allAcademicProgramsEntities)
            {
                var target = Mapper.Map<Domain.Student.Entities.AcademicProgram, Dtos.Student.AcademicProgram>(academicProgram);
                target.Code = academicProgram.Code;
                target.Description = academicProgram.Description;
                _allAcademicProgramsDtos.Add(target);
            }

            _academicProgramServiceMock.Setup(s => s.GetAsync())
                .ReturnsAsync(_allAcademicProgramsDtos);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicProgramController = null;
            _academicProgramService = null;
            _allAcademicProgramsDtos = null;
            _allAcademicProgramsEntities = null;
        }

        [TestMethod]
        public async Task AcademicProgramController_ReturnsAllAcademicPrograms()
        {
            var academicPrograms = await _academicProgramController.GetAsync() as List<Dtos.Student.AcademicProgram>;
            Assert.AreEqual(academicPrograms.Count, _allAcademicProgramsEntities.Count);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AcademicProgramController_ReturnsAllAcademicPrograms_Exception()
        {
            _academicProgramServiceMock.Setup(s => s.GetAsync()).Throws<Exception>();
            await _academicProgramController.GetAsync();
        }
    }

    [TestClass]
    public class AcademicProgramControllerGetAllHeDM
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private AcademicProgramsController _academicProgramController;
        private Mock<IAcademicProgramService> _academicProgramServiceMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAcademicProgramService _academicProgramService;
        private IAdapterRegistry _adapterRegistry;
        private List<Domain.Student.Entities.AcademicProgram> _allAcademicProgramsEntities;
        private List<AcademicProgram> _allAcademicProgramsDtos;
        private readonly ILogger _logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _academicProgramServiceMock = new Mock<IAcademicProgramService>();
            _academicProgramService = _academicProgramServiceMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            _allAcademicProgramsEntities = new TestAcademicProgramRepository().GetAsync().Result as List<Domain.Student.Entities.AcademicProgram>;

            _academicProgramController = new AcademicProgramsController(_adapterRegistry, _academicProgramService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicProgramController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            _academicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            
            _allAcademicProgramsDtos = new List<AcademicProgram>();

            foreach (var academicProgram in _allAcademicProgramsEntities)
            {
                var target = new AcademicProgram();
                target.Code = academicProgram.Code;
                target.Title = academicProgram.Description;
                target.Description = academicProgram.LongDescription;
                target.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "1df164eb-8178-4321-a9f7-24f27f3991d8" } };
                target.Disciplines = new List<AcademicProgramDisciplines>() { new AcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "1df164eb-8178-5678-a9f7-24f27f3991d8" }}};
                target.AcademicLevel = new GuidObject2() { Id = "1df589eb-8178-4321-a9f7-24f43f3891d8" };
                _allAcademicProgramsDtos.Add(target);
            }

        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicProgramController = null;
            _academicProgramService = null;
            _allAcademicProgramsDtos = null;
            _allAcademicProgramsEntities = null;
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public void AcademicProgramController_PostAcademicPrograms()
        {
            _academicProgramController.PostAcademicProgram(_allAcademicProgramsDtos.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public void AcademicProgramController_PutAcademicPrograms()
        {
            var academicProgram = _allAcademicProgramsDtos.FirstOrDefault();
            _academicProgramController.PutAcademicProgram(academicProgram.Id, academicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public void AcademicProgramController_DeleteAcademicProgramAsync()
        {
            _academicProgramController.DeleteAcademicProgram(_allAcademicProgramsDtos.FirstOrDefault().Id);
        }
    }

#region HEDM V10 tests

    [TestClass]
    public class AcademicProgramsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAcademicProgramService> academicProgramsServiceMock;
        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;

        private AcademicProgramsController academicProgramsController;
        private IEnumerable<Domain.Student.Entities.AcademicProgram> allAcadPrograms;
        private List<Dtos.AcademicProgram3> academicProgramsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            academicProgramsServiceMock = new Mock<IAcademicProgramService>();
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            academicProgramsCollection = new List<Dtos.AcademicProgram3>();

            allAcadPrograms = new List<Domain.Student.Entities.AcademicProgram>()
                {
                    new Domain.Student.Entities.AcademicProgram("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.AcademicProgram("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.AcademicProgram("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allAcadPrograms)
            {
                var academicProgram = new Ellucian.Colleague.Dtos.AcademicProgram3
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                academicProgramsCollection.Add(academicProgram);
            }

            academicProgramsController = new AcademicProgramsController(adapterRegistryMock.Object, academicProgramsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            academicProgramsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            academicProgramsController = null;
            allAcadPrograms = null;
            academicProgramsCollection = null;
            loggerMock = null;
            academicProgramsServiceMock = null;
        }

        [TestMethod]
        public async Task AcademicProgramsController_GetAcademicPrograms_ValidateFields_Nocache()
        {
            academicProgramsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms3Async(false)).ReturnsAsync(academicProgramsCollection);

            var sourceContexts = (await academicProgramsController.GetAcademicPrograms3Async()).ToList();
            Assert.AreEqual(academicProgramsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = academicProgramsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AcademicProgramsController_GetAcademicPrograms_ValidateFields_Cache()
        {
            academicProgramsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms3Async(true)).ReturnsAsync(academicProgramsCollection);

            var sourceContexts = (await academicProgramsController.GetAcademicPrograms3Async()).ToList();
            Assert.AreEqual(academicProgramsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = academicProgramsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_KeyNotFoundException()
        {
            //
            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms3Async(false))
                .Throws<KeyNotFoundException>();
            await academicProgramsController.GetAcademicPrograms3Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_PermissionsException()
        {

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms3Async(false))
                .Throws<PermissionsException>();
            await academicProgramsController.GetAcademicPrograms3Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_ArgumentException()
        {

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms3Async(false))
                .Throws<ArgumentException>();
            await academicProgramsController.GetAcademicPrograms3Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_RepositoryException()
        {

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms3Async(false))
                .Throws<RepositoryException>();
            await academicProgramsController.GetAcademicPrograms3Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_IntegrationApiException()
        {

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms3Async(false))
                .Throws<IntegrationApiException>();
            await academicProgramsController.GetAcademicPrograms3Async();
        }

        [TestMethod]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuidAsync_ValidateFields()
        {
            var expected = academicProgramsCollection.FirstOrDefault();
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid3Async(expected.Id)).ReturnsAsync(expected);

            var actual = await academicProgramsController.GetAcademicProgramById3Async(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_Exception()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms3Async(false)).Throws<Exception>();
            await academicProgramsController.GetAcademicPrograms3Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuidAsync_Exception()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid3Async(It.IsAny<string>())).Throws<Exception>();
            await academicProgramsController.GetAcademicProgramById3Async(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_KeyNotFoundException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid3Async(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await academicProgramsController.GetAcademicProgramById3Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_PermissionsException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid3Async(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await academicProgramsController.GetAcademicProgramById3Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_ArgumentException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid3Async(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await academicProgramsController.GetAcademicProgramById3Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_RepositoryException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid3Async(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await academicProgramsController.GetAcademicProgramById3Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_IntegrationApiException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid3Async(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await academicProgramsController.GetAcademicProgramById3Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_Exception()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid3Async(It.IsAny<string>()))
                .Throws<Exception>();
            await academicProgramsController.GetAcademicProgramById3Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_PostAcademicProgramsAsync_Exception()
        {
            academicProgramsController.PostAcademicProgram3(academicProgramsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_PutAcademicProgramsAsync_Exception()
        {
            var sourceContext = academicProgramsCollection.FirstOrDefault();
            academicProgramsController.PutAcademicProgram3(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_DeleteAcademicProgramsAsync_Exception()
        {
            academicProgramsController.DeleteAcademicProgram(academicProgramsCollection.FirstOrDefault().Id);
        }
    }
    #endregion

    #region HEDM V15 tests

    [TestClass]
    public class AcademicProgramsV15ControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAcademicProgramService> academicProgramsServiceMock;
        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;

        private AcademicProgramsController academicProgramsController;
        private IEnumerable<Domain.Student.Entities.AcademicProgram> allAcadPrograms;
        private List<Dtos.AcademicProgram4> academicProgramsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            academicProgramsServiceMock = new Mock<IAcademicProgramService>();
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            academicProgramsCollection = new List<Dtos.AcademicProgram4>();

            allAcadPrograms = new List<Domain.Student.Entities.AcademicProgram>()
                {
                    new Domain.Student.Entities.AcademicProgram("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.AcademicProgram("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.AcademicProgram("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allAcadPrograms)
            {
                var academicProgram = new Ellucian.Colleague.Dtos.AcademicProgram4
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                academicProgramsCollection.Add(academicProgram);
            }

            academicProgramsController = new AcademicProgramsController(adapterRegistryMock.Object, academicProgramsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            academicProgramsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            academicProgramsController = null;
            allAcadPrograms = null;
            academicProgramsCollection = null;
            loggerMock = null;
            academicProgramsServiceMock = null;
        }

        [TestMethod]
        public async Task AcademicProgramsController_GetAcademicPrograms_ValidateFields_Nocache()
        {
            academicProgramsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AcademicProgram4>(), false)).ReturnsAsync(academicProgramsCollection);

            var sourceContexts = (await academicProgramsController.GetAcademicPrograms4Async(null, null, criteriaFilter)).ToList();
            Assert.AreEqual(academicProgramsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = academicProgramsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AcademicProgramsController_GetAcademicPrograms_ValidateFields_Cache()
        {
            academicProgramsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AcademicProgram4>(), true)).ReturnsAsync(academicProgramsCollection);

            var sourceContexts = (await academicProgramsController.GetAcademicPrograms4Async(null, null, criteriaFilter)).ToList();
            Assert.AreEqual(academicProgramsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = academicProgramsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_KeyNotFoundException()
        {
            //
            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AcademicProgram4>(), false))
                .Throws<KeyNotFoundException>();
            await academicProgramsController.GetAcademicPrograms4Async(null, null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_PermissionsException()
        {

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AcademicProgram4>(), false))
                .Throws<PermissionsException>();
            await academicProgramsController.GetAcademicPrograms4Async(null, null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_ArgumentException()
        {

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AcademicProgram4>(), false))
                .Throws<ArgumentException>();
            await academicProgramsController.GetAcademicPrograms4Async(null, null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_RepositoryException()
        {

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AcademicProgram4>(), false))
                .Throws<RepositoryException>();
            await academicProgramsController.GetAcademicPrograms4Async(null, null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_IntegrationApiException()
        {

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AcademicProgram4>(), false))
                .Throws<IntegrationApiException>();
            await academicProgramsController.GetAcademicPrograms4Async(null, null, criteriaFilter);
        }

        [TestMethod]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuidAsync_ValidateFields()
        {
            var expected = academicProgramsCollection.FirstOrDefault();
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid4Async(expected.Id)).ReturnsAsync(expected);

            var actual = await academicProgramsController.GetAcademicProgramById4Async(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicPrograms_Exception()
        {
            QueryStringFilter acadCatalog = new QueryStringFilter("academicCatalog", "{'academicCatalog':{'id':'6c669a92-9d61-42b6-b8cf-26b6dcd12419'}}");
            QueryStringFilter recruitmentProg = new QueryStringFilter("recruitmentProgram", "{'recruitmentProgram':'active'}");

            academicProgramsServiceMock.Setup(x => x.GetAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AcademicProgram4>(), false)).Throws<Exception>();
            await academicProgramsController.GetAcademicPrograms4Async(acadCatalog, recruitmentProg, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuidAsync_Exception()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid4Async(It.IsAny<string>())).Throws<Exception>();
            await academicProgramsController.GetAcademicProgramById4Async(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_KeyNotFoundException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid4Async(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await academicProgramsController.GetAcademicProgramById4Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_PermissionsException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid4Async(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await academicProgramsController.GetAcademicProgramById4Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_ArgumentException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid4Async(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await academicProgramsController.GetAcademicProgramById4Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_RepositoryException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid4Async(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await academicProgramsController.GetAcademicProgramById4Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_IntegrationApiException()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid4Async(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await academicProgramsController.GetAcademicProgramById4Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicProgramsController_GetAcademicProgramsByGuid_Exception()
        {
            academicProgramsServiceMock.Setup(x => x.GetAcademicProgramByGuid4Async(It.IsAny<string>()))
                .Throws<Exception>();
            await academicProgramsController.GetAcademicProgramById4Async(expectedGuid);
        }
    }
    #endregion


}