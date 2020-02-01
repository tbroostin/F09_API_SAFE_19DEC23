// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using AcademicPeriod = Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod;
using AutoMapper;
using System.Net.Http.Headers;
using System;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AcademicPeriodsControllerTests2
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private AcademicPeriodsController _academicPeriodController;
        private Mock<IAcademicPeriodService> _academicPeriodServiceMock;
        private IAcademicPeriodService _academicPeriodService;
        private IAdapterRegistry _adapterRegistry;
        private List<AcademicPeriod> _allAcademicPeriods;
        private List<AcademicPeriod2> _allAcademicPeriodsDto;
        private readonly ILogger _logger = new Mock<ILogger>().Object;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _academicPeriodServiceMock = new Mock<IAcademicPeriodService>();
            _academicPeriodService = _academicPeriodServiceMock.Object;

            _allAcademicPeriodsDto = new List<AcademicPeriod2>();
            var adapters = new HashSet<ITypeAdapter>();
            _adapterRegistry = new AdapterRegistry(adapters, _logger);
            var testAdapter = new AutoMapperAdapter<AcademicPeriod, AcademicPeriod2>(_adapterRegistry, _logger);
            _adapterRegistry.AddAdapter(testAdapter);

            _allAcademicPeriods = new TestAcademicPeriodRepository().Get() as List<AcademicPeriod>;

            _academicPeriodController = new AcademicPeriodsController(_adapterRegistry, _academicPeriodService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicPeriodController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            _academicPeriodController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            Mapper.CreateMap<AcademicPeriod, AcademicPeriod2>();
            Debug.Assert(_allAcademicPeriods != null, "_allAcademicPeriods != null");
            foreach (var academicPeriods in _allAcademicPeriods)
            {
                var target = Mapper.Map<AcademicPeriod, AcademicPeriod2>(academicPeriods);
                target.Id = academicPeriods.Guid;
                target.Title = academicPeriods.Description;
                _allAcademicPeriodsDto.Add(target);
            }
            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods2Async(It.IsAny<bool>()))
                .ReturnsAsync(_allAcademicPeriodsDto);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicPeriodController = null;
            _academicPeriodService = null;
            _allAcademicPeriods = null;
            _allAcademicPeriodsDto = null;
            _academicPeriodServiceMock = null;
        }

        [TestMethod]
        public async Task AcadePeriodController_ReturnsAllAcademicPeriods()
        {
            var academicPeriods = await _academicPeriodController.GetAcademicPeriods2Async(criteriaFilter) as List<AcademicPeriod2>;
            Debug.Assert(academicPeriods != null, "academicPeriods != null");
            Assert.AreEqual(academicPeriods.Count, _allAcademicPeriods.Count);
        }

        [TestMethod]
        public async Task AcadePeriodController_GetAcademicPeriodById2Async()
        {
            var expected = _allAcademicPeriodsDto.FirstOrDefault();
            _academicPeriodServiceMock.Setup(x => x.GetAcademicPeriodByGuid2Async(expected.Id)).ReturnsAsync(expected);

            Debug.Assert(expected != null, "expected != null");
            var actual = await _academicPeriodController.GetAcademicPeriodByGuid2Async(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code"); ;
        }


        [TestMethod]
        public async Task AcadePeriodController_GetAcademicPeriods_ValidateProperties()
        {
            var academicPeriods = await _academicPeriodController.GetAcademicPeriods2Async(criteriaFilter) as List<AcademicPeriod2>;
            Debug.Assert(academicPeriods != null, "academicPeriods != null");
            var al = academicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            var alt = _allAcademicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            Debug.Assert(alt != null, "alt != null");
            Debug.Assert(al != null, "al != null");
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        [TestMethod]
        public async Task AcadePeriodController_AcademicPeriodsTypeTest()
        {
            var academicPeriods = (await _academicPeriodController.GetAcademicPeriods2Async(criteriaFilter)).FirstOrDefault();
            Debug.Assert(academicPeriods != null, "academicPeriods != null");
            Assert.AreEqual(typeof (AcademicPeriod2), academicPeriods.GetType());
        }

        [TestMethod]
        public void AcadePeriodController_NumberOfKnownPropertiesTest()
        {
            var academicPeriodProperties =
                typeof (AcademicPeriod2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(8, academicPeriodProperties.Length);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AcadePeriodController_GetAcademicPeriodById2Async_Exception()
        {
            var expected = _allAcademicPeriodsDto.FirstOrDefault();
            _academicPeriodServiceMock.Setup(x => x.GetAcademicPeriodByGuid2Async(expected.Id)).Throws<Exception>();
            Debug.Assert(expected != null, "expected != null");
            await _academicPeriodController.GetAcademicPeriodByGuid2Async(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AcadePeriodController_GetAcademicPeriods2Async_Exception()
        {
            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods2Async(It.IsAny<bool>())).Throws<Exception>();
            await _academicPeriodController.GetAcademicPeriods2Async(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AcadPeriodController_DeleteThrowsIntApiExc()
        {
            await _academicPeriodController.DeleteAcademicPeriodByIdAsync("UG");
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AcadPeriodController_PostThrowsIntAppiExc()
        {
            await _academicPeriodController.PostAcademicPeriodAsync(new AcademicPeriod2());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AcadePeriodController_PutThrowsIntAppiExc()
        {
            await _academicPeriodController.PutAcademicPeriodAsync("UG", new AcademicPeriod2());
        }

    }

    [TestClass]
    public class AcademicPeriodsControllerTests3
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private AcademicPeriodsController _academicPeriodController;
        private Mock<IAcademicPeriodService> _academicPeriodServiceMock;
        private IAcademicPeriodService _academicPeriodService;
        private IAdapterRegistry _adapterRegistry;
        private List<AcademicPeriod> _allAcademicPeriods;
        private List<AcademicPeriod3> _allAcademicPeriodsDto;
        private readonly ILogger _logger = new Mock<ILogger>().Object;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _academicPeriodServiceMock = new Mock<IAcademicPeriodService>();
            _academicPeriodService = _academicPeriodServiceMock.Object;

            _allAcademicPeriodsDto = new List<AcademicPeriod3>();
            var adapters = new HashSet<ITypeAdapter>();
            _adapterRegistry = new AdapterRegistry(adapters, _logger);
            var testAdapter = new AutoMapperAdapter<AcademicPeriod, AcademicPeriod3>(_adapterRegistry, _logger);
            _adapterRegistry.AddAdapter(testAdapter);

            _allAcademicPeriods = new TestAcademicPeriodRepository().Get() as List<AcademicPeriod>;

            _academicPeriodController = new AcademicPeriodsController(_adapterRegistry, _academicPeriodService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicPeriodController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            _academicPeriodController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            Mapper.CreateMap<AcademicPeriod, AcademicPeriod3>();
            Debug.Assert(_allAcademicPeriods != null, "_allAcademicPeriods != null");
            foreach (var academicPeriods in _allAcademicPeriods)
            {
                var target = Mapper.Map<AcademicPeriod, AcademicPeriod3>(academicPeriods);
                target.Id = academicPeriods.Guid;
                target.Title = academicPeriods.Description;
                _allAcademicPeriodsDto.Add(target);
            }
            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods3Async(It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(_allAcademicPeriodsDto);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicPeriodController = null;
            _academicPeriodService = null;
            _allAcademicPeriods = null;
            _allAcademicPeriodsDto = null;
            _academicPeriodServiceMock = null;
        }

        [TestMethod]
        public async Task AcadePeriodController_ReturnsAllAcademicPeriods()
        {
            var academicPeriods = await _academicPeriodController.GetAcademicPeriods3Async(criteriaFilter) as List<AcademicPeriod3>;
            Debug.Assert(academicPeriods != null, "academicPeriods != null");
            Assert.AreEqual(academicPeriods.Count, _allAcademicPeriods.Count);
        }

        [TestMethod]
        public async Task AcadePeriodController_GetAcademicPeriodById3Async()
        {
            var expected = _allAcademicPeriodsDto.FirstOrDefault();
            _academicPeriodServiceMock.Setup(x => x.GetAcademicPeriodByGuid3Async(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            Debug.Assert(expected != null, "expected != null");
            var actual = await _academicPeriodController.GetAcademicPeriodByGuid3Async(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code"); ;
        }


        [TestMethod]
        public async Task AcadePeriodController_GetAcademicPeriods_ValidateProperties()
        {
            var academicPeriods = await _academicPeriodController.GetAcademicPeriods3Async(criteriaFilter) as List<AcademicPeriod3>;
            Debug.Assert(academicPeriods != null, "academicPeriods != null");
            var al = academicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            var alt = _allAcademicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            Debug.Assert(alt != null, "alt != null");
            Debug.Assert(al != null, "al != null");
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        // Removed since input for GetAcademicPeriods3Async no longer a string.
        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task AcadePeriodController_GetAcademicPeriods_InvalidFilter()
        //{
        //    var criteria = @"{'kitten':{'id':'7a6e9b82-e78b-47db-9f30-5becff004921'}}";
        //    var academicPeriods = await _academicPeriodController.GetAcademicPeriods3Async(criteria) as List<AcademicPeriod3>;
        //    Debug.Assert(academicPeriods == null, "academicPeriods == null");
        //}

        [TestMethod]
        public async Task AcadePeriodController_GetAcademicPeriods_Filter()
        {
            var filterGroupName = "criteria";
            _academicPeriodController.Request.Properties.Add(
             string.Format("FilterObject{0}", filterGroupName),
            new Dtos.AcademicPeriod3() { RegistrationStatus = Dtos.EnumProperties.TermRegistrationStatus.Open});

            var academicPeriods = await _academicPeriodController.GetAcademicPeriods3Async(criteriaFilter) as List<AcademicPeriod3>;
            Debug.Assert(academicPeriods != null, "academicPeriods != null");
            var al = academicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            var alt = _allAcademicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            Debug.Assert(alt != null, "alt != null");
            Debug.Assert(al != null, "al != null");
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }
        [TestMethod]
        public async Task AcadePeriodController_AcademicPeriodsTypeTest()
        {
            var academicPeriods = (await _academicPeriodController.GetAcademicPeriods3Async(criteriaFilter)).FirstOrDefault();
            Debug.Assert(academicPeriods != null, "academicPeriods != null");
            Assert.AreEqual(typeof(AcademicPeriod3), academicPeriods.GetType());
        }

        [TestMethod]
        public void AcadePeriodController_NumberOfKnownPropertiesTest()
        {
            var academicPeriodProperties =
                typeof(AcademicPeriod3).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(10, academicPeriodProperties.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcadePeriodController_GetAcademicPeriodById3Async_Exception()
        {
            var expected = _allAcademicPeriodsDto.FirstOrDefault();
            _academicPeriodServiceMock.Setup(x => x.GetAcademicPeriodByGuid3Async(expected.Id, It.IsAny<bool>())).Throws<Exception>();
            Debug.Assert(expected != null, "expected != null");
            await _academicPeriodController.GetAcademicPeriodByGuid3Async(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcadePeriodController_GetAcademicPeriods3Async_Exception()
        {
            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods3Async(It.IsAny<bool>(), It.IsAny<string>())).Throws<Exception>();
            await _academicPeriodController.GetAcademicPeriods3Async(criteriaFilter);
        }

    }

    [TestClass]
    public class AcademicPeriodsControllerTests4
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private AcademicPeriodsController _academicPeriodController;
        private Mock<IAcademicPeriodService> _academicPeriodServiceMock;
        private IAcademicPeriodService _academicPeriodService;
        private IAdapterRegistry _adapterRegistry;
        private List<AcademicPeriod> _allAcademicPeriods;
        private List<AcademicPeriod4> _allAcademicPeriodsDto;
        private readonly ILogger _logger = new Mock<ILogger>().Object;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _academicPeriodServiceMock = new Mock<IAcademicPeriodService>();
            _academicPeriodService = _academicPeriodServiceMock.Object;

            _allAcademicPeriodsDto = new List<AcademicPeriod4>();
            var adapters = new HashSet<ITypeAdapter>();
            _adapterRegistry = new AdapterRegistry(adapters, _logger);
            var testAdapter = new AutoMapperAdapter<AcademicPeriod, AcademicPeriod4>(_adapterRegistry, _logger);
            _adapterRegistry.AddAdapter(testAdapter);

            _allAcademicPeriods = new TestAcademicPeriodRepository().Get() as List<AcademicPeriod>;

            _academicPeriodController = new AcademicPeriodsController(_adapterRegistry, _academicPeriodService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicPeriodController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            _academicPeriodController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            Mapper.CreateMap<AcademicPeriod, AcademicPeriod4>();
            Debug.Assert(_allAcademicPeriods != null, "_allAcademicPeriods != null");
            foreach (var academicPeriods in _allAcademicPeriods)
            {
                var target = Mapper.Map<AcademicPeriod, AcademicPeriod4>(academicPeriods);
                target.Id = academicPeriods.Guid;
                target.Title = academicPeriods.Description;
                _allAcademicPeriodsDto.Add(target);
            }
            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods4Async(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(_allAcademicPeriodsDto);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicPeriodController = null;
            _academicPeriodService = null;
            _allAcademicPeriods = null;
            _allAcademicPeriodsDto = null;
            _academicPeriodServiceMock = null;
        }

        [TestMethod]
        public async Task AcadePeriodController_ReturnsAllAcademicPeriods()
        {
            Dictionary<string, string> qualifiers = new Dictionary<string, string>();
            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods4Async(true, "", "", "", null, null, qualifiers))
               .ReturnsAsync(_allAcademicPeriodsDto);
            var academicPeriods = await _academicPeriodController.GetAcademicPeriods4Async(criteriaFilter);
            Assert.AreEqual(academicPeriods.Count(), _allAcademicPeriods.Count);
        }

        [TestMethod]
        public async Task AcadePeriodController_GetAcademicPeriodById4Async()
        {
            var expected = _allAcademicPeriodsDto.FirstOrDefault();
            _academicPeriodServiceMock.Setup(x => x.GetAcademicPeriodByGuid4Async(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await _academicPeriodController.GetAcademicPeriodByGuid4Async(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code"); ;
        }


        [TestMethod]
        public async Task AcadePeriodController_GetAcademicPeriods_ValidateProperties()
        {
            Dictionary<string, string> qualifiers = new Dictionary<string, string>();
            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods4Async(true, "", "", "", null, null, qualifiers))
               .ReturnsAsync(_allAcademicPeriodsDto);

            var academicPeriods = await _academicPeriodController.GetAcademicPeriods4Async(It.IsAny< QueryStringFilter>());
            var al = academicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            var alt = _allAcademicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        [TestMethod]
        public async Task AcadePeriodController_GetAcademicPeriods_Filter()
        {
            var filterGroupName = "criteria";
            _academicPeriodController.Request.Properties.Add(
             string.Format("FilterObject{0}", filterGroupName),
            new Dtos.AcademicPeriod4() { RegistrationStatus = Dtos.EnumProperties.TermRegistrationStatus.Open });

            Dictionary<string, string> qualifiers = new Dictionary<string, string>();

            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods4Async(true, "Open", "", "", null, null, qualifiers))
               .ReturnsAsync(_allAcademicPeriodsDto);

            var academicPeriods = await _academicPeriodController.GetAcademicPeriods4Async(criteriaFilter);
            var al = academicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            var alt = _allAcademicPeriods.FirstOrDefault(a => a.Code == "2000RSU");
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }
        [TestMethod]
        public async Task AcadePeriodController_AcademicPeriodsTypeTest()
        {
            Dictionary<string, string> qualifiers = new Dictionary<string, string>();
            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods4Async(true, "", "", "", null, null, qualifiers))
               .ReturnsAsync(_allAcademicPeriodsDto);
            var academicPeriods = (await _academicPeriodController.GetAcademicPeriods4Async(criteriaFilter)).FirstOrDefault();
            Assert.AreEqual(typeof(AcademicPeriod4), academicPeriods.GetType());
        }

        [TestMethod]
        public void AcadePeriodController_NumberOfKnownPropertiesTest()
        {
            var academicPeriodProperties =
                typeof(AcademicPeriod4).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(10, academicPeriodProperties.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcadePeriodController_GetAcademicPeriodById4Async_Exception()
        {
            var expected = _allAcademicPeriodsDto.FirstOrDefault();
            _academicPeriodServiceMock.Setup(x => x.GetAcademicPeriodByGuid4Async(expected.Id, It.IsAny<bool>())).Throws<Exception>();
            await _academicPeriodController.GetAcademicPeriodByGuid4Async(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcadePeriodController_GetAcademicPeriods4Async_Exception()
        {
            Dictionary<string, string> qualifiers = new Dictionary<string, string>();
            _academicPeriodServiceMock.Setup(s => s.GetAcademicPeriods4Async(true, "", "", "", null, null, qualifiers))
               .Throws<Exception>();
            await _academicPeriodController.GetAcademicPeriods4Async(criteriaFilter);
        }

    }
}