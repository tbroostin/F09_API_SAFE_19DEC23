// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Reflection;
using Ellucian.Colleague.Domain.Base.Tests;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using System.Net;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Api.Controllers.Student;
using System;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AcademicDisciplinesControllerTests
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
        private Mock<IAcademicDisciplineService> academicDisciplineServiceMock;

        private string academicDisciplineId;

        private AcademicDiscipline expectedAcademicDiscipline;
        private AcademicDiscipline testAcademicDiscipline;
        private AcademicDiscipline actualAcademicDiscipline;

        private AcademicDiscipline2 expectedAcademicDiscipline2;
        private AcademicDiscipline2 testAcademicDiscipline2;
        private AcademicDiscipline2 actualAcademicDiscipline2;

        private AcademicDisciplinesController academicDisciplinesController;


        public async Task<List<AcademicDiscipline>> getActualAcademicDisciplines()
        {
            IEnumerable<AcademicDiscipline> academicDisciplineList = await academicDisciplinesController.GetAcademicDisciplinesAsync();
            return (await academicDisciplinesController.GetAcademicDisciplinesAsync()).ToList();
        }

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            academicDisciplineServiceMock = new Mock<IAcademicDisciplineService>();

            academicDisciplineId = "idc2935b-29e8-675f-907b-15a34da4f433";

            expectedAcademicDiscipline = new AcademicDiscipline()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433", //Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Abbreviation = "AAAA", //Code = "AAAA",
                Title = "Academic Administration",
                Description = null,
                Type = AcademicDisciplineType.Major,
            };

            expectedAcademicDiscipline2 = new AcademicDiscipline2()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433", //Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Abbreviation = "AAAA", //Code = "AAAA",
                Title = "Academic Administration",
                Description = null,
                Type = AcademicDisciplineType.Major,
                Reporting = new List<ReportingDtoProperty>() 
                { 
                    new ReportingDtoProperty() { Value = new ReportingCountryDtoProperty() { Code = Dtos.EnumProperties.IsoCode.USA, Value = "99.9999" }}
                }
            };

            testAcademicDiscipline = new AcademicDiscipline();
            testAcademicDiscipline2 = new AcademicDiscipline2();


            foreach (var property in typeof(AcademicDiscipline).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testAcademicDiscipline, property.GetValue(expectedAcademicDiscipline, null), null);
            }
            foreach (var property in typeof(AcademicDiscipline2).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testAcademicDiscipline2, property.GetValue(expectedAcademicDiscipline2, null), null);
            }



            academicDisciplineServiceMock.Setup<Task<AcademicDiscipline>>(s => s.GetAcademicDisciplineByGuidAsync(academicDisciplineId)).Returns(Task.FromResult(testAcademicDiscipline));
            academicDisciplineServiceMock.Setup<Task<AcademicDiscipline2>>(s => s.GetAcademicDiscipline2ByGuidAsync(academicDisciplineId, It.IsAny<bool>())).Returns(Task.FromResult(testAcademicDiscipline2));
            academicDisciplineServiceMock.Setup(s => s.GetExtendedEthosDataByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(new List<Web.Http.EthosExtend.EthosExtensibleData>());
            academicDisciplinesController = new AcademicDisciplinesController(adapterRegistryMock.Object, academicDisciplineServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            academicDisciplinesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            academicDisciplinesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            
            //actualAcademicDiscipline = await academicDisciplinesController.GetAcademicDisciplineByIdAsync(academicDisciplineId);
            //actualAcademicDiscipline2 = await academicDisciplinesController.GetAcademicDiscipline2ByIdAsync(academicDisciplineId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            academicDisciplineServiceMock = null;
            academicDisciplineId = null;
            expectedAcademicDiscipline = null;
            expectedAcademicDiscipline2 = null;
            testAcademicDiscipline = null;
            testAcademicDiscipline2 = null;
            actualAcademicDiscipline = null;
            actualAcademicDiscipline2 = null;
            academicDisciplinesController = null;
        }

        [TestMethod]
        public async Task AcademicDisciplinesTypeTest()
        {
            actualAcademicDiscipline = await academicDisciplinesController.GetAcademicDisciplineByIdAsync(academicDisciplineId);
            Assert.AreEqual(typeof(AcademicDiscipline), actualAcademicDiscipline.GetType());
            Assert.AreEqual(expectedAcademicDiscipline.GetType(), actualAcademicDiscipline.GetType());
        }
        [TestMethod]
        public async Task AcademicDisciplinesTypeTest2()
        {
            actualAcademicDiscipline2 = await academicDisciplinesController.GetAcademicDiscipline2ByIdAsync(academicDisciplineId);
            Assert.AreEqual(typeof(AcademicDiscipline2), actualAcademicDiscipline2.GetType());
            Assert.AreEqual(expectedAcademicDiscipline2.GetType(), actualAcademicDiscipline2.GetType());
        }
        [TestMethod]
        public void NumberOfKnownPropertiesTest()
        {
            var academicDisciplineProperties = typeof(AcademicDiscipline).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(6, academicDisciplineProperties.Length);
        }
        [TestMethod]
        public void NumberOfKnownPropertiesTest2()
        {
            var academicDisciplineProperties = typeof(AcademicDiscipline2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(7, academicDisciplineProperties.Length);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void AcademicDisciplinesController_PostThrowsIntAppiExc()
        {
            var result = academicDisciplinesController.PostAcademicDisciplines(actualAcademicDiscipline);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicDisciplinesController_PutThrowsIntAppiExc()
        {
            actualAcademicDiscipline = await academicDisciplinesController.GetAcademicDisciplineByIdAsync(academicDisciplineId);
            var result = academicDisciplinesController.PutAcademicDisciplines(actualAcademicDiscipline.Id, actualAcademicDiscipline);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void AcademicDisciplinesController_DeleteThrowsIntAppiExc()
        {
            academicDisciplinesController.DeleteAcademicDisciplines(expectedAcademicDiscipline.Id);
        }
    }

    [TestClass]
    public class AcademicDisciplineController_GetAllTests  // not including filter/query
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

        private AcademicDisciplinesController AcademicDisciplineController;
        private Mock<IAcademicDisciplineService> AcademicDisciplineServiceMock;
        private IAcademicDisciplineService AcademicDisciplineService;
        private IAdapterRegistry AdapterRegistry;
        private List<Ellucian.Colleague.Domain.Base.Entities.OtherMajor> allOtherMajorsEntities;
        private List<Ellucian.Colleague.Domain.Base.Entities.OtherMinor> allOtherMinorsEntities;
        private List<Ellucian.Colleague.Domain.Base.Entities.OtherSpecial> allOtherSpecialsEntities;
        ILogger logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            AcademicDisciplineServiceMock = new Mock<IAcademicDisciplineService>();
            AcademicDisciplineService = AcademicDisciplineServiceMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.OtherMajor, AcademicDiscipline>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            allOtherMajorsEntities = new TestAcademicDisciplineRepository().GetOtherMajors() as List<Ellucian.Colleague.Domain.Base.Entities.OtherMajor>;
            var AcademicDisciplinesList = new List<AcademicDiscipline>();
            var AcademicDisciplinesList2 = new List<AcademicDiscipline2>();

            AcademicDisciplineController = new AcademicDisciplinesController(AdapterRegistry, AcademicDisciplineService, logger)
            {
                Request = new HttpRequestMessage()
            };

            AcademicDisciplineController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            AcademicDisciplineController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            //actualAcademicDiscipline = await academicDisciplinesController.GetAcademicDisciplineByIdAsync(academicDisciplineId);
            //actualAcademicDiscipline2 = await academicDisciplinesController.GetAcademicDiscipline2ByIdAsync(academicDisciplineId);

            foreach (var academicDiscipline in allOtherMajorsEntities)
            {
                AcademicDiscipline target = ConvertOtherMajorsEntitytoAcademicDisciplineDto(academicDiscipline);
                AcademicDisciplinesList.Add(target);

                AcademicDiscipline2 target2 = ConvertOtherMajorsEntitytoAcademicDiscipline2Dto(academicDiscipline);
                AcademicDisciplinesList2.Add(target2);

            }

            AcademicDisciplineServiceMock.Setup<Task<IEnumerable<AcademicDiscipline>>>(s => s.GetAcademicDisciplinesAsync(false)).ReturnsAsync(AcademicDisciplinesList);
            AcademicDisciplineServiceMock.Setup<Task<IEnumerable<AcademicDiscipline2>>>(s => s.GetAcademicDisciplines2Async(false)).ReturnsAsync(AcademicDisciplinesList2);
            

        }

        [TestCleanup]
        public void Cleanup()
        {
            AcademicDisciplineController = null;
            AcademicDisciplineService = null;
        }

        [TestMethod]
        public async Task ReturnsAllAcademicDisciplines()
        {
            List<AcademicDiscipline> AcademicDisciplines = await AcademicDisciplineController.GetAcademicDisciplinesAsync() as List<AcademicDiscipline>;
            Assert.AreEqual(AcademicDisciplines.Count, allOtherMajorsEntities.Count);
        }

        [TestMethod]
        public async Task GetAcademicDisciplines_DisciplineProperties()
        {
            List<AcademicDiscipline> AcademicDisciplines = await AcademicDisciplineController.GetAcademicDisciplinesAsync() as List<AcademicDiscipline>;
            AcademicDiscipline al = AcademicDisciplines.Where(a => a.Abbreviation == "ENGL").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.OtherMajor alt = allOtherMajorsEntities.Where(a => a.Code == "ENGL").FirstOrDefault();
            Assert.AreEqual(alt.Code, al.Abbreviation);
            Assert.AreEqual(alt.Description, al.Title);
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a OtherMajor domain entity to its corresponding Academic Discipline DTO
        /// </summary>
        /// <param name="source">Other Major domain entity</param>
        /// <returns>AcademicDiscipline DTO</returns>
        private Dtos.AcademicDiscipline ConvertOtherMajorsEntitytoAcademicDisciplineDto(Domain.Base.Entities.OtherMajor source)
        {
            var academicDiscipline = new Dtos.AcademicDiscipline();
            academicDiscipline.Id = source.Guid;
            academicDiscipline.Abbreviation = source.Code;
            academicDiscipline.Title = source.Description;
            academicDiscipline.Description = null;
            academicDiscipline.Type = Dtos.AcademicDisciplineType.Major;
            return academicDiscipline;
        }
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a OtherMajor domain entity to its corresponding Academic Discipline2 DTO
        /// </summary>
        /// <param name="source">Other Major domain entity</param>
        /// <returns>AcademicDiscipline DTO</returns>
        private Dtos.AcademicDiscipline2 ConvertOtherMajorsEntitytoAcademicDiscipline2Dto(Domain.Base.Entities.OtherMajor source)
        {
            var academicDiscipline = new Dtos.AcademicDiscipline2();
            academicDiscipline.Id = source.Guid;
            academicDiscipline.Abbreviation = source.Code;
            academicDiscipline.Title = source.Description;
            academicDiscipline.Description = null;
            academicDiscipline.Type = Dtos.AcademicDisciplineType.Major;
            return academicDiscipline;
        }
    }

    [TestClass]
    public class AcademicDisciplineController_GetByGuidTests
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
        private Mock<IAcademicDisciplineService> academicDisciplineServiceMock;

        private string academicDisciplineId;

        private AcademicDiscipline expectedAcademicDiscipline;
        private AcademicDiscipline testAcademicDiscipline;

        private AcademicDiscipline2 expectedAcademicDiscipline2;
        private AcademicDiscipline2 testAcademicDiscipline2;

        private AcademicDisciplinesController academicDisciplinesController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            academicDisciplineServiceMock = new Mock<IAcademicDisciplineService>();

            academicDisciplineId = "idc2935b-29e8-675f-907b-15a34da4f433";

            expectedAcademicDiscipline = new AcademicDiscipline()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433", //Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Abbreviation = "AAAA", //Code = "AAAA",
                Title = "Academic Administration",
                Description = null,
                Type = AcademicDisciplineType.Major,
            };

            expectedAcademicDiscipline2 = new AcademicDiscipline2()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433", //Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Abbreviation = "AAAA", //Code = "AAAA",
                Title = "Academic Administration",
                Description = null,
                Type = AcademicDisciplineType.Major,
                Reporting = new List<ReportingDtoProperty>() 
                { 
                    new ReportingDtoProperty() { Value = new ReportingCountryDtoProperty() { Code = Dtos.EnumProperties.IsoCode.USA, Value = "99.9999" }}
                }
            };

            testAcademicDiscipline = new AcademicDiscipline();
            testAcademicDiscipline2 = new AcademicDiscipline2();


            foreach (var property in typeof(AcademicDiscipline).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testAcademicDiscipline, property.GetValue(expectedAcademicDiscipline, null), null);
            }
            foreach (var property in typeof(AcademicDiscipline2).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testAcademicDiscipline2, property.GetValue(expectedAcademicDiscipline2, null), null);
            }



            academicDisciplineServiceMock.Setup<Task<AcademicDiscipline>>(s => s.GetAcademicDisciplineByGuidAsync(academicDisciplineId)).Returns(Task.FromResult(testAcademicDiscipline));
            academicDisciplineServiceMock.Setup<Task<AcademicDiscipline2>>(s => s.GetAcademicDiscipline2ByGuidAsync(academicDisciplineId, It.IsAny<bool>())).Returns(Task.FromResult(testAcademicDiscipline2));

            academicDisciplinesController = new AcademicDisciplinesController(adapterRegistryMock.Object, academicDisciplineServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            academicDisciplinesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            academicDisciplinesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            
        }

        [TestCleanup]
        public void Cleanup()
        {
            academicDisciplinesController = null;
            academicDisciplineServiceMock = null;
        }

        [TestMethod]
        public async Task ReturnsAcademicDiscipline()
        {
            var actualAcademicDiscipline = await academicDisciplinesController.GetAcademicDisciplineByIdAsync(academicDisciplineId);
            Assert.AreEqual(actualAcademicDiscipline.Id, testAcademicDiscipline.Id);
        }
        [TestMethod]
        public async Task ReturnsAcademicDiscipline2()
        {
            var actualAcademicDiscipline = await academicDisciplinesController.GetAcademicDiscipline2ByIdAsync(academicDisciplineId);
            Assert.AreEqual(actualAcademicDiscipline.Id, testAcademicDiscipline.Id);
        }
        [TestMethod]
        public async Task GetAcademicDisciplineByIdAsync_BadGuid_ReturnsNotFound()
        {
            try
            {
                var actualAcademicDiscipline = await academicDisciplinesController.GetAcademicDisciplineByIdAsync("Bad ID");
            }
            catch (HttpResponseException ex)
            {
                Assert.IsTrue(ex.Response.StatusCode == HttpStatusCode.NotFound);
            }
            
        }
        [TestMethod]
        public async Task GetAcademicDiscipline2ByIdAsync_BadGuid_ReturnsNotFound()
        {
            try
            {
                var actualAcademicDiscipline = await academicDisciplinesController.GetAcademicDiscipline2ByIdAsync("Bad ID");
            }
            catch (HttpResponseException ex)
            {
                Assert.IsTrue(ex.Response.StatusCode == HttpStatusCode.NotFound);
            }
        }

    }


    [TestClass]
    public class AcademicDisciplineController_GetAllFiltersQueries
    {
        private TestContext testContextInstance2;
        private QueryStringFilter majorFilter, minorFilter, concentrationFilter, majorStatusActiveQueryFilter, majorStatusInactiveQueryFilter;

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

        private AcademicDisciplinesController AcademicDisciplineController;
        private Mock<IAcademicDisciplineService> AcademicDisciplineServiceMock;
        private IAcademicDisciplineService AcademicDisciplineService;
        private IAdapterRegistry AdapterRegistry;
        private List<Ellucian.Colleague.Domain.Base.Entities.OtherMajor> allOtherMajorsEntities;
        private List<Ellucian.Colleague.Domain.Base.Entities.OtherMinor> allOtherMinorsEntities;
        private List<Ellucian.Colleague.Domain.Base.Entities.OtherSpecial> allOtherSpecialsEntities;
        ILogger logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            AcademicDisciplineServiceMock = new Mock<IAcademicDisciplineService>();
            AcademicDisciplineService = AcademicDisciplineServiceMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.OtherMajor, AcademicDiscipline>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            allOtherMajorsEntities = new TestAcademicDisciplineRepository().GetOtherMajors() as List<Ellucian.Colleague.Domain.Base.Entities.OtherMajor>;
            allOtherMinorsEntities = new TestAcademicDisciplineRepository().GetOtherMinors() as List<Ellucian.Colleague.Domain.Base.Entities.OtherMinor>;
            allOtherSpecialsEntities = new TestAcademicDisciplineRepository().GetOtherSpecials() as List<Ellucian.Colleague.Domain.Base.Entities.OtherSpecial>;

            var academicDisciplinesList = new List<AcademicDiscipline3>();
            var minorsList = new List<AcademicDiscipline3>();
            var majorsList = new List<AcademicDiscipline3>();
            var specialsList = new List<AcademicDiscipline3>();
            var activeMajorsList = new List<AcademicDiscipline3>();
            var inactiveMajorsList = new List<AcademicDiscipline3>();


            AcademicDisciplineController = new AcademicDisciplinesController(AdapterRegistry, AcademicDisciplineService, logger)
            {
                Request = new HttpRequestMessage()
            };

            AcademicDisciplineController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            AcademicDisciplineController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            
            foreach (var academicDiscipline in allOtherMajorsEntities)
            {
                AcademicDiscipline3 target = ConvertOtherMajorsEntitytoAcademicDiscipline2Dto(academicDiscipline);
                majorsList.Add(target);
            }

            foreach (var academicDiscipline in allOtherMinorsEntities)
            {
                AcademicDiscipline3 target = ConvertOtherMinorsEntitytoAcademicDiscipline2Dto(academicDiscipline);
                minorsList.Add(target);
            }
            foreach (var academicDiscipline in allOtherSpecialsEntities)
            {
                AcademicDiscipline3 target = ConvertOtherSpecialsEntitytoAcademicDiscipline2Dto(academicDiscipline);
                specialsList.Add(target);

            }
            academicDisciplinesList.AddRange(majorsList);
            academicDisciplinesList.AddRange(minorsList);
            academicDisciplinesList.AddRange(specialsList);
            activeMajorsList.Add(majorsList.ElementAt(0));   // arbitrary, just the first major
            inactiveMajorsList.Add(majorsList.ElementAt(1)); //    ditto,  just the second

            // Mock Service responses
            AcademicDisciplineServiceMock.Setup<Task<IEnumerable<AcademicDiscipline3>>>(s => s.GetAcademicDisciplines3Async(MajorStatus.NotSet,"",It.IsAny<bool>())).ReturnsAsync(academicDisciplinesList);
            AcademicDisciplineServiceMock.Setup<Task<IEnumerable<AcademicDiscipline3>>>(s => s.GetAcademicDisciplines3Async(MajorStatus.NotSet, "major",It.IsAny<bool>())).ReturnsAsync(majorsList);
            AcademicDisciplineServiceMock.Setup<Task<IEnumerable<AcademicDiscipline3>>>(s => s.GetAcademicDisciplines3Async(MajorStatus.NotSet, "minor", It.IsAny<bool>())).ReturnsAsync(minorsList);
            AcademicDisciplineServiceMock.Setup<Task<IEnumerable<AcademicDiscipline3>>>(s => s.GetAcademicDisciplines3Async(MajorStatus.NotSet, "concentration", It.IsAny<bool>())).ReturnsAsync(specialsList);
            AcademicDisciplineServiceMock.Setup<Task<IEnumerable<AcademicDiscipline3>>>(s => s.GetAcademicDisciplines3Async(MajorStatus.Active, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(activeMajorsList);
            AcademicDisciplineServiceMock.Setup<Task<IEnumerable<AcademicDiscipline3>>>(s => s.GetAcademicDisciplines3Async(MajorStatus.Inactive, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(inactiveMajorsList);
            

            // Build QueryStringFilters
            majorFilter = new QueryStringFilter("criteria", "{\"type\":\"major\" ");
            minorFilter = new QueryStringFilter("criteria", "{\"type\":\"minor\" ");
            concentrationFilter = new QueryStringFilter("criteria", "{\"type\":\"concentration\" ");
            majorStatusActiveQueryFilter = new QueryStringFilter("majorStatus", "{\"majorStatus\":\"active\" ");
            majorStatusActiveQueryFilter = new QueryStringFilter("majorStatus", "{\"majorStatus\":\"inactive\" ");


        }

        [TestCleanup]
        public void Cleanup()
        {
            AcademicDisciplineController = null;
            AcademicDisciplineService = null;
        }

        [TestMethod]
        public async Task ReturnsAllAcademicDisciplines()
        {
            var AcademicDisciplines = await AcademicDisciplineController.GetAcademicDisciplines3Async(null,null);
            Assert.AreEqual(AcademicDisciplines.Count(), allOtherMajorsEntities.Count + allOtherMinorsEntities.Count + allOtherSpecialsEntities.Count);
        }

        [TestMethod]
        public async Task GetAcademicDisciplines_ByTypeMajor()
        {
           
            AcademicDisciplineController.Request.Properties.Add(string.Format("FilterObject{0}", "criteria"),
                new AcademicDiscipline3() { Type = AcademicDisciplineType2.Major });


            var actualDisciplines = await AcademicDisciplineController.GetAcademicDisciplines3Async(majorFilter, null);
            foreach (var discipline in actualDisciplines)
            {
                Assert.AreEqual(discipline.Type, AcademicDisciplineType2.Major);
            }
            Assert.AreEqual(allOtherMajorsEntities.Count, actualDisciplines.Count());
        }


        [TestMethod]
        public async Task GetAcademicDisciplines_ByTypeMinor()
        {


            AcademicDisciplineController.Request.Properties.Add(string.Format("FilterObject{0}", "criteria"),
                new AcademicDiscipline3() { Type = AcademicDisciplineType2.Minor });

            var actualDisciplines = await AcademicDisciplineController.GetAcademicDisciplines3Async(minorFilter, null);
            foreach (var discipline in actualDisciplines)
            {
                Assert.AreEqual(discipline.Type, AcademicDisciplineType2.Minor);
            }
            Assert.AreEqual(allOtherMinorsEntities.Count, actualDisciplines.Count());
        }

        [TestMethod]
        public async Task GetAcademicDisciplines_ByTypeConcentration()
        {


            AcademicDisciplineController.Request.Properties.Add(string.Format("FilterObject{0}", "criteria"),
                new AcademicDiscipline3() { Type = AcademicDisciplineType2.Concentration });

            var actualDisciplines = await AcademicDisciplineController.GetAcademicDisciplines3Async(concentrationFilter, null);


            foreach (var discipline in actualDisciplines)
            {
                Assert.AreEqual(discipline.Type, AcademicDisciplineType2.Concentration);
            }
            Assert.AreEqual(allOtherSpecialsEntities.Count, actualDisciplines.Count());
        }


        [TestMethod]
        public async Task GetAcademicDisciplines_ByMajorStatusActive()
        {
            AcademicDisciplineController.Request.Properties.Add(string.Format("FilterObject{0}", "majorStatus"),
                new Dtos.Filters.MajorStatusFilter() { MajorStatus = MajorStatus.Active});

            var actualDisciplines = await AcademicDisciplineController.GetAcademicDisciplines3Async(concentrationFilter, null);
            
            Assert.AreEqual(1, actualDisciplines.Count());
            Assert.AreEqual("ENGL", actualDisciplines.First().Abbreviation);
        }
        [TestMethod]
        public async Task GetAcademicDisciplines_ByMajorStatusInactive()
        {
            AcademicDisciplineController.Request.Properties.Add(string.Format("FilterObject{0}", "majorStatus"),
                new Dtos.Filters.MajorStatusFilter() { MajorStatus = MajorStatus.Inactive});

            var actualDisciplines = await AcademicDisciplineController.GetAcademicDisciplines3Async(concentrationFilter, null);
            
            Assert.AreEqual(1, actualDisciplines.Count());
            Assert.AreEqual("MATH", actualDisciplines.First().Abbreviation);
        }






        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a OtherMajor domain entity to its corresponding Academic Discipline DTO
        /// </summary>
        /// <param name="source">Other Major domain entity</param>
        /// <returns>AcademicDiscipline DTO</returns>
        private Dtos.AcademicDiscipline ConvertOtherMajorsEntitytoAcademicDisciplineDto(Domain.Base.Entities.OtherMajor source)
        {
            var academicDiscipline = new Dtos.AcademicDiscipline();
            academicDiscipline.Id = source.Guid;
            academicDiscipline.Abbreviation = source.Code;
            academicDiscipline.Title = source.Description;
            academicDiscipline.Description = null;
            academicDiscipline.Type = Dtos.AcademicDisciplineType.Major;
            return academicDiscipline;
        }
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a OtherMajor domain entity to its corresponding Academic Discipline2 DTO
        /// </summary>
        /// <param name="source">Other Major domain entity</param>
        /// <returns>AcademicDiscipline DTO</returns>
        private Dtos.AcademicDiscipline3 ConvertOtherMajorsEntitytoAcademicDiscipline2Dto(Domain.Base.Entities.OtherMajor source)
        {
            var academicDiscipline = new Dtos.AcademicDiscipline3();
            academicDiscipline.Id = source.Guid;
            academicDiscipline.Abbreviation = source.Code;
            academicDiscipline.Title = source.Description;
            academicDiscipline.Description = null;
            academicDiscipline.Type = AcademicDisciplineType2.Major;
            return academicDiscipline;
        }
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a OtherMinor domain entity to its corresponding Academic Discipline2 DTO
        /// </summary>
        /// <param name="source">Other Minor domain entity</param>
        /// <returns>AcademicDiscipline DTO</returns>
        private Dtos.AcademicDiscipline3 ConvertOtherMinorsEntitytoAcademicDiscipline2Dto(Domain.Base.Entities.OtherMinor source)
        {
            var academicDiscipline = new Dtos.AcademicDiscipline3();
            academicDiscipline.Id = source.Guid;
            academicDiscipline.Abbreviation = source.Code;
            academicDiscipline.Title = source.Description;
            academicDiscipline.Description = null;
            academicDiscipline.Type = AcademicDisciplineType2.Minor;
            return academicDiscipline;
        }
        
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a OtherSpecial domain entity to its corresponding Academic Discipline2 DTO
        /// </summary>
        /// <param name="source">Other Special domain entity</param>
        /// <returns>AcademicDiscipline DTO</returns>
        private Dtos.AcademicDiscipline3 ConvertOtherSpecialsEntitytoAcademicDiscipline2Dto(Domain.Base.Entities.OtherSpecial source)
        {
            var academicDiscipline = new Dtos.AcademicDiscipline3();
            academicDiscipline.Id = source.Guid;
            academicDiscipline.Abbreviation = source.Code;
            academicDiscipline.Title = source.Description;
            academicDiscipline.Description = null;
            academicDiscipline.Type = AcademicDisciplineType2.Concentration;
            return academicDiscipline;
        }
    }
    
}

