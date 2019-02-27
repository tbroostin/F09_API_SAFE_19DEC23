// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AcademicCatalogControllerTests
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }


        private Mock<ILogger> _loggerMock;
        private Mock<IAcademicCatalogService> _academicCatalogServiceMock;

        private string _academicCatalogId;

        private AcademicCatalog _expectedAcademicCatalog;
        private AcademicCatalog _testAcademicCatalog;
        private AcademicCatalog _actualAcademicCatalog;

        private AcademicCatalogController _academicCatalogsController;


        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));


            _loggerMock = new Mock<ILogger>();
            _academicCatalogServiceMock = new Mock<IAcademicCatalogService>();

            _academicCatalogId = "idc2935b-29e8-675f-907b-15a34da4f433";

            _expectedAcademicCatalog = new AcademicCatalog
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Code = "1999",
                StartDate = new DateTime(1999, 02, 01),
                EndDate = DateTime.Today,
                Title = "1999 catalog",
                status = LifeCycleStatus.Active
            };

            _testAcademicCatalog = new AcademicCatalog();
            foreach (var property in typeof (AcademicCatalog).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                )
            {
                property.SetValue(_testAcademicCatalog, property.GetValue(_expectedAcademicCatalog, null), null);
            }

            _academicCatalogsController = new AcademicCatalogController(_academicCatalogServiceMock.Object,
                _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerMock = null;
            _academicCatalogServiceMock = null;
            _academicCatalogId = null;
            _expectedAcademicCatalog = null;
            _testAcademicCatalog = null;
            _actualAcademicCatalog = null;
            _academicCatalogsController = null;
        }

        [TestMethod]
        public void AcademicCatalogController_NumberOfKnownPropertiesTest()
        {
            var academicCatalogProperties =
                typeof (AcademicCatalog).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(8, academicCatalogProperties.Length);
        }
    }

    [TestClass]
    public class AcademicCatalogController_AcademicCatalog
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private AcademicCatalogController _academicCatalogController;
        private Mock<IAcademicCatalogService> _academicCatalogServiceMock;
        private IAcademicCatalogService _academicCatalogService;
        private List<Catalog> _allAcademicCatalogsEntities;
        private List<AcademicCatalog> _allAcademicCatalogsDtos;
        private readonly ILogger _logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _academicCatalogServiceMock = new Mock<IAcademicCatalogService>();
            _academicCatalogService = _academicCatalogServiceMock.Object;

            _allAcademicCatalogsEntities = new TestCatalogRepository().GetAsync().Result as List<Catalog>;

            _academicCatalogController = new AcademicCatalogController(_academicCatalogService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicCatalogController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            _academicCatalogController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            Debug.Assert(_allAcademicCatalogsEntities != null, "allAcademicCatalogsEntities != null");

            _allAcademicCatalogsDtos = new List<AcademicCatalog>();

            foreach (var academicCatalog in _allAcademicCatalogsEntities)
            {
                AcademicCatalog target = ConvertCatalogEntitytoAcademicCatalogDto(academicCatalog);
                _allAcademicCatalogsDtos.Add(target);
            }

        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicCatalogController = null;
            _academicCatalogService = null;
            _allAcademicCatalogsDtos = null;
            _allAcademicCatalogsEntities = null;
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public void AcademicCatalogController_PostAcademicCatalogs()
        {
            _academicCatalogController.PostAcademicCatalogs(_allAcademicCatalogsDtos.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public void AcademicCatalogController_PutAcademicCatalogs()
        {
            var academicCatalogs = _allAcademicCatalogsDtos.FirstOrDefault();
            Debug.Assert(academicCatalogs != null, "academicCatalogs != null");
            _academicCatalogController.PutAcademicCatalogs(academicCatalogs.Id, academicCatalogs);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public void AcademicCatalogController_DeleteCreditCategoryAsync()
        {
            Debug.Assert(_allAcademicCatalogsDtos != null, "_allAcademicCatalogsDtos != null");
            _academicCatalogController.DeleteAcademicCatalogs(_allAcademicCatalogsDtos.FirstOrDefault().Id);
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Catalog domain entity to its corresponding Academic Catalog DTO
        /// </summary>
        /// <param name="source">Academic Catalog domain entity</param>
        /// <returns>AcademicCatalog DTO</returns>
        private Dtos.AcademicCatalog ConvertCatalogEntitytoAcademicCatalogDto(Domain.Student.Entities.Requirements.Catalog source)
        {
            var acadedmicCatalog = new Dtos.AcademicCatalog();
            acadedmicCatalog.Id = source.Guid;
            acadedmicCatalog.StartDate = source.StartDate;
            acadedmicCatalog.EndDate = source.EndDate;
            acadedmicCatalog.Code = source.Code;
            acadedmicCatalog.Title = source.Description;
            acadedmicCatalog.status = source.IsActive ? LifeCycleStatus.Active : LifeCycleStatus.Inactive;
            return acadedmicCatalog;
        }
    }

    [TestClass]
    public class AcademicCatalogController_AcademicCatalog2
    {

        public TestContext TestContext { get; set; }

        private AcademicCatalogController _academicCatalogController;
        private Mock<IAcademicCatalogService> _academicCatalogServiceMock;
        private IAcademicCatalogService _academicCatalogService;
        private List<Catalog> _allAcademicCatalogsEntities;
        private List<AcademicCatalog2> _allAcademicCatalogsDtos;
        private readonly ILogger _logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _academicCatalogServiceMock = new Mock<IAcademicCatalogService>();
            _academicCatalogService = _academicCatalogServiceMock.Object;

            _allAcademicCatalogsEntities = new TestCatalogRepository().GetAsync().Result as List<Catalog>;

            _academicCatalogController = new AcademicCatalogController(_academicCatalogService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicCatalogController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            _academicCatalogController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            Debug.Assert(_allAcademicCatalogsEntities != null, "allAcademicCatalogsEntities != null");

            _allAcademicCatalogsDtos = new List<AcademicCatalog2>();

            foreach (var academicCatalog in _allAcademicCatalogsEntities)
            {
                AcademicCatalog2 target = ConvertCatalogEntitytoAcademicCatalog2Dto(academicCatalog);
                if (target != null)
                {
                    _allAcademicCatalogsDtos.Add(target);
                }
            }

            _academicCatalogServiceMock.Setup(s => s.GetAcademicCatalogs2Async(It.IsAny<bool>()))
                .ReturnsAsync(_allAcademicCatalogsDtos);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicCatalogController = null;
            _academicCatalogService = null;
            _allAcademicCatalogsDtos = null;
            _allAcademicCatalogsEntities = null;
        }

        [TestMethod]
        public async Task AcademicCatalogController_ReturnsAllAcademicCatalogs2()
        {
            var academicCatalogs = await _academicCatalogController.GetAcademicCatalogs2Async() as List<AcademicCatalog2>;
            Assert.AreEqual(academicCatalogs.Count, _allAcademicCatalogsEntities.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicCatalogController_ReturnsAllAcademicCatalogs2_Exception()
        {
            _academicCatalogServiceMock.Setup(s => s.GetAcademicCatalogs2Async(It.IsAny<bool>())).Throws<Exception>();
            await _academicCatalogController.GetAcademicCatalogs2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicCatalogController_GetAcademicCatalogs2ById_Exception()
        {
            _academicCatalogServiceMock.Setup(x => x.GetAcademicCatalogByGuid2Async(It.IsAny<string>()))
                .Throws<Exception>();
            await _academicCatalogController.GetAcademicCatalogById2Async(string.Empty);
        }

        [TestMethod]
        public async Task AcademicCatalogController_GetAcademicCatalogs2_CatalogProperties()
        {
            var academicCatalogs = await _academicCatalogController.GetAcademicCatalogs2Async() as List<AcademicCatalog2>;

            var al = academicCatalogs.FirstOrDefault(a => a.Code == "2010");
            var alt = _allAcademicCatalogsEntities.FirstOrDefault(a => a.Code == "2010");

            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Description);
            Assert.AreEqual(alt.Code, al.Code);
        }

        private Dtos.AcademicCatalog2 ConvertCatalogEntitytoAcademicCatalog2Dto(Domain.Student.Entities.Requirements.Catalog source)
        {
            var acadedmicCatalog = new Dtos.AcademicCatalog2();
            acadedmicCatalog.Id = source.Guid;
            acadedmicCatalog.StartDate = source.StartDate;
            acadedmicCatalog.EndDate = source.EndDate;
            acadedmicCatalog.Code = source.Code;
            acadedmicCatalog.Title = source.Description;
            acadedmicCatalog.status = source.IsActive ? LifeCycleStatus.Active : LifeCycleStatus.Inactive;
            return acadedmicCatalog;
        }
    }
    
    [TestClass]
    public class AcademicCatalogController_AllAcademicCatalog
    {
        public TestContext TestContext { get; set; }

        private AcademicCatalogController _academicCatalogController;
        private Mock<IAcademicCatalogService> _academicCatalogServiceMock;
        private IAcademicCatalogService _academicCatalogService;
        private List<Catalog> _allAcademicCatalogsEntities;
        private List<Dtos.Student.Catalog> _allCatalogsEntities;
        private readonly ILogger _logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _academicCatalogServiceMock = new Mock<IAcademicCatalogService>();
            _academicCatalogService = _academicCatalogServiceMock.Object;

            _allAcademicCatalogsEntities = new TestCatalogRepository().GetAsync().Result as List<Catalog>;
            _allCatalogsEntities = new List<Dtos.Student.Catalog>();

            _academicCatalogController = new AcademicCatalogController(_academicCatalogService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicCatalogController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            _academicCatalogController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            Debug.Assert(_allAcademicCatalogsEntities != null, "allAcademicCatalogsEntities != null");           
            

            foreach (var academicCatalog in _allAcademicCatalogsEntities)
            {
                Dtos.Student.Catalog target = ConvertCatalogEntitytoCatalogDto(academicCatalog);
                if (target != null)
                {
                    _allCatalogsEntities.Add(target);
                }
            }
            _academicCatalogServiceMock.Setup(s => s.GetAllAcademicCatalogsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allCatalogsEntities);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicCatalogController = null;
            _academicCatalogService = null;
            _allAcademicCatalogsEntities = null;
            _allCatalogsEntities = null;
        }

        [TestMethod]
        public async Task AcademicCatalogController_ReturnsAllAcademicCatalogs3()
        {
            var academicCatalogs = await _academicCatalogController.GetAllAcademicCatalogsAsync() as List<Dtos.Student.Catalog>;
            Assert.AreEqual(academicCatalogs.Count, _allCatalogsEntities.Count);
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicCatalogController_ReturnsAllAcademicCatalogs3_Exception()
        {
            _academicCatalogServiceMock.Setup(s => s.GetAllAcademicCatalogsAsync(It.IsAny<bool>())).Throws<Exception>();
            await _academicCatalogController.GetAllAcademicCatalogsAsync();
        }
        
        [TestMethod]
        public async Task AcademicCatalogController_GetAllAcademicCatalogs3_CatalogProperties()
        {
            var academicCatalogs = await _academicCatalogController.GetAllAcademicCatalogsAsync() as List<Dtos.Student.Catalog>;

            var al = academicCatalogs.FirstOrDefault(a => a.HideInWhatIf == false);
            var alt = _allAcademicCatalogsEntities.FirstOrDefault(a => a.HideInWhatIf == false);

            Assert.AreEqual(alt.HideInWhatIf, al.HideInWhatIf);
            Assert.AreEqual(alt.Code, al.CatalogYear);
        }
        private Dtos.Student.Catalog ConvertCatalogEntitytoCatalogDto(Domain.Student.Entities.Requirements.Catalog source)
        {
            var acadedmicCatalog = new Dtos.Student.Catalog();
            acadedmicCatalog.CatalogYear = source.Code; acadedmicCatalog.HideInWhatIf = source.HideInWhatIf;
            acadedmicCatalog.CatalogStartDate = source.StartDate.ToString();
            return acadedmicCatalog;
        }
    }
}