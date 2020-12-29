// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class EthnicitiesControllerTests
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

        private EthnicitiesController EthnicityController;
        private Mock<IDemographicService> DemographicServiceMock;
        private Mock<IReferenceDataRepository> ReferenceDataRepositoryMock;
        private IDemographicService DemographicService;
        private IAdapterRegistry AdapterRegistry;
        private List<Ellucian.Colleague.Domain.Base.Entities.Ethnicity> allEthnicities;
        ILogger logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            DemographicServiceMock = new Mock<IDemographicService>();
            DemographicService = DemographicServiceMock.Object;

            ReferenceDataRepositoryMock = new Mock<IReferenceDataRepository>();

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Ethnicity, Ethnicity2>(AdapterRegistry, logger);
            var entityAdapter = new Ellucian.Colleague.Coordination.Base.Adapters.EthnicityEntityAdapter(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);
            AdapterRegistry.AddAdapter(entityAdapter);

            allEthnicities = new TestEthnicityRepository().Get() as List<Ellucian.Colleague.Domain.Base.Entities.Ethnicity>;
            ReferenceDataRepositoryMock.Setup(r => r.EthnicitiesAsync()).ReturnsAsync(allEthnicities);
            var EthnicitiesList = new List<Ethnicity2>();

            EthnicityController = new EthnicitiesController(AdapterRegistry, ReferenceDataRepositoryMock.Object, DemographicService, logger);
            EthnicityController.Request = new HttpRequestMessage();
            EthnicityController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var ethnicities in allEthnicities)
            {
                Ethnicity2 target = ConvertEthnicityEntitytoEthnicityDto(ethnicities);
                EthnicitiesList.Add(target);
            }

            DemographicServiceMock.Setup<Task<IEnumerable<Ethnicity2>>>(s => s.GetEthnicities2Async(false)).ReturnsAsync(EthnicitiesList);
        }

        [TestCleanup]
        public void Cleanup()
        {
            EthnicityController = null;
            DemographicService = null;
        }

        [TestMethod]
        public async Task GetAsync_returns_all_ethnicities()
        {
            var ethnicityDtos = await EthnicityController.GetAsync();
            Assert.IsNotNull(ethnicityDtos);
            Assert.AreEqual(allEthnicities.Count(), ethnicityDtos.Count());
            for(int i = 0; i < allEthnicities.Count(); i++)
            {
                Assert.AreEqual(allEthnicities.ElementAt(i).Code, ethnicityDtos.ElementAt(i).Code);
                Assert.AreEqual(allEthnicities.ElementAt(i).Description, ethnicityDtos.ElementAt(i).Description);
                Assert.AreEqual(allEthnicities.ElementAt(i).Type.ToString(), ethnicityDtos.ElementAt(i).Type.ToString());
            }
        }

        [TestMethod]
        public async Task ReturnsAllEthnicities()
        {
            List<Ethnicity2> Ethnicities = await EthnicityController.GetEthnicities2Async() as List<Ethnicity2>;
            Assert.AreEqual(Ethnicities.Count, allEthnicities.Count);
        }

        [TestMethod]
        public async Task GetEthnicities_LevelProperties()
        {
            List<Ethnicity2> Ethnicities = await EthnicityController.GetEthnicities2Async() as List<Ethnicity2>;
            Ethnicity2 eth = Ethnicities.Where(a => a.Code == "HIS").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Ethnicity et = allEthnicities.Where(a => a.Code == "HIS").FirstOrDefault();
            Assert.AreEqual(et.Code, eth.Code);
            Assert.AreEqual(et.Description, eth.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthnicityController_DeleteThrowsIntApiExc()
        {
            await EthnicityController.DeleteEthnicitiesAsync("HIS");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthnicityController_PostThrowsIntAppiExc()
        {
            Ethnicity2 etDTO = await EthnicityController.PostEthnicitiesAsync(new Ethnicity2());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthnicityController_PutThrowsIntAppiExc()
        {
            Ethnicity2 etDTO = await EthnicityController.PutEthnicitiesAsync(new Ethnicity2());
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Ethnicity domain entity to its corresponding Ethnicity DTO
        /// </summary>
        /// <param name="source">Ethnicity domain entity</param>
        /// <returns>Ethnicity2 DTO</returns>
        private Dtos.Ethnicity2 ConvertEthnicityEntitytoEthnicityDto(Domain.Base.Entities.Ethnicity source)
        {
            var ethnicity = new Dtos.Ethnicity2();
            ethnicity.Id = source.Guid;
            ethnicity.Code = source.Code;
            ethnicity.Title = source.Description;
            ethnicity.Description = null;
            return ethnicity;
        }
    }

    [TestClass]
    public class EthnicitiesControllerTests_2
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
        private Mock<IDemographicService> demographicServiceMock;
        private Mock<IReferenceDataRepository> ReferenceDataRepositoryMock;

        private string ethnicityId;

        private Ethnicity2 expectedEthnicity;
        private Ethnicity2 testEthnicity;
        private Ethnicity2 actualEthnicity;

        private EthnicitiesController ethnicitiesController;


        public async Task<List<Ethnicity2>> getActualEthnicities()
        {
            return (await ethnicitiesController.GetEthnicities2Async()).ToList();
        }

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            demographicServiceMock = new Mock<IDemographicService>();
            ReferenceDataRepositoryMock = new Mock<IReferenceDataRepository>();

            ethnicityId = "idc2935b-29e8-675f-907b-15a34da4f433";

            expectedEthnicity = new Ethnicity2()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Code = "HIS",
                Title = "Hispanic/Latino",
                Description = null,
            };

            testEthnicity = new Ethnicity2();
            foreach (var property in typeof(Ethnicity2).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testEthnicity, property.GetValue(expectedEthnicity, null), null);
            }
            demographicServiceMock.Setup<Task<Ethnicity2>>(s => s.GetEthnicityById2Async(ethnicityId)).Returns(Task.FromResult(testEthnicity));

            ethnicitiesController = new EthnicitiesController(adapterRegistryMock.Object, ReferenceDataRepositoryMock.Object, demographicServiceMock.Object, loggerMock.Object);
            actualEthnicity = await ethnicitiesController.GetEthnicityById2Async(ethnicityId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            demographicServiceMock = null;
            ReferenceDataRepositoryMock = null;
            ethnicityId = null;
            expectedEthnicity = null;
            testEthnicity = null;
            actualEthnicity = null;
            ethnicitiesController = null;
        }

        [TestMethod]
        public void EthnicitiesTypeTest()
        {
            Assert.AreEqual(typeof(Ethnicity2), actualEthnicity.GetType());
            Assert.AreEqual(expectedEthnicity.GetType(), actualEthnicity.GetType());
        }

        [TestMethod]
        public void NumberOfKnownPropertiesTest()
        {
            var ethnicityProperties = typeof(Ethnicity2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(6, ethnicityProperties.Length);
        }
    }
}