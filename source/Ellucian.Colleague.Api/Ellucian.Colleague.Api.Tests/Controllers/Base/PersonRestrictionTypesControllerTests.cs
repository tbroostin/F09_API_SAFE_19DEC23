// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
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
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonRestrictionTypesControllerTests
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
        private Mock<IRestrictionTypeService> restrictionTypeServiceMock;

        private string restrictionTypeId;

        private RestrictionType2 expectedRestrictionType;
        private RestrictionType2 testRestrictionType;
        private RestrictionType2 actualRestrictionType;

        private PersonRestrictionTypesController restrictionTypesController;


        public async Task<List<RestrictionType2>> getActualRestrictionTypes()
        {
            return (await restrictionTypesController.GetRestrictionTypes2Async()).ToList();
        }

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            restrictionTypeServiceMock = new Mock<IRestrictionTypeService>();

            restrictionTypeId = "idc2935b-29e8-675f-907b-15a34da4f433";

            expectedRestrictionType = new RestrictionType2()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Code = "AAAA",
                Title = "Academic Administration",
                Description = null,
            };

            testRestrictionType = new RestrictionType2();
            foreach (var property in typeof(RestrictionType2).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testRestrictionType, property.GetValue(expectedRestrictionType, null), null);
            }
            restrictionTypeServiceMock.Setup<Task<RestrictionType2>>(s => s.GetRestrictionTypeByGuid2Async(restrictionTypeId)).Returns(Task.FromResult(testRestrictionType));

            restrictionTypesController = new PersonRestrictionTypesController(adapterRegistryMock.Object, restrictionTypeServiceMock.Object, loggerMock.Object);
            actualRestrictionType = await restrictionTypesController.GetRestrictionTypeById2Async(restrictionTypeId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            restrictionTypeServiceMock = null;
            restrictionTypeId = null;
            expectedRestrictionType = null;
            testRestrictionType = null;
            actualRestrictionType = null;
            restrictionTypesController = null;
        }

        [TestMethod]
        public void RestrictionTypesTypeTest()
        {
            Assert.AreEqual(typeof(RestrictionType2), actualRestrictionType.GetType());
            Assert.AreEqual(expectedRestrictionType.GetType(), actualRestrictionType.GetType());
        }

        [TestMethod]
        public void NumberOfKnownPropertiesTest()
        {
            var restrictionTypeProperties = typeof(RestrictionType2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(5, restrictionTypeProperties.Length);
        }
    }

    [TestClass]
    public class PersonRestrictionTypesController_GetAllTests
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

        private PersonRestrictionTypesController RestrictionTypeController;
        private Mock<IRestrictionTypeService> RestrictionTypeServiceMock;
        private IRestrictionTypeService RestrictionTypeService;
        private IAdapterRegistry AdapterRegistry;
        private List<Ellucian.Colleague.Domain.Base.Entities.Restriction> allRestrictionTypesEntities;
        ILogger logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            RestrictionTypeServiceMock = new Mock<IRestrictionTypeService>();
            RestrictionTypeService = RestrictionTypeServiceMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            allRestrictionTypesEntities = new TestRestrictionRepository().Get() as List<Ellucian.Colleague.Domain.Base.Entities.Restriction>;
            var RestrictionTypesList = new List<RestrictionType2>();

            RestrictionTypeController = new PersonRestrictionTypesController(AdapterRegistry, RestrictionTypeService, logger);
            RestrictionTypeController.Request = new HttpRequestMessage();
            RestrictionTypeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var restrictionType in allRestrictionTypesEntities)
            {
                RestrictionType2 target = ConvertRestrictionEntitytoRestrictionTypeDto(restrictionType);
                RestrictionTypesList.Add(target);
            }

            RestrictionTypeServiceMock.Setup<Task<IEnumerable<RestrictionType2>>>(s => s.GetRestrictionTypes2Async(It.IsAny<bool>())).ReturnsAsync(RestrictionTypesList);
        }


        [TestCleanup]
        public void Cleanup()
        {
            RestrictionTypeController = null;
            RestrictionTypeService = null;
        }

        [TestMethod]
        public async Task ReturnsAllRestrictionTypes()
        {
            List<RestrictionType2> RestrictionTypes = await RestrictionTypeController.GetRestrictionTypes2Async() as List<RestrictionType2>;
            Assert.AreEqual(RestrictionTypes.Count, allRestrictionTypesEntities.Count);
        }

        [TestMethod]
        public async Task GetRestrictionTypes_Properties()
        {
            List<RestrictionType2> RestrictionTypes = await RestrictionTypeController.GetRestrictionTypes2Async() as List<RestrictionType2>;
            RestrictionType2 al = RestrictionTypes.Where(a => a.Code == "ACC30").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Restriction alt = allRestrictionTypesEntities.Where(a => a.Code == "ACC30").FirstOrDefault();
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        [TestMethod]
        public async Task RestrictionTypesController_GetHedmAsync_CacheControlNotNull()
        {
            RestrictionTypeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            List<RestrictionType2> RestrictionTypes = await RestrictionTypeController.GetRestrictionTypes2Async() as List<RestrictionType2>;
            RestrictionType2 rt = RestrictionTypes.Where(a => a.Code == "ACC30").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Restriction ret = allRestrictionTypesEntities.Where(a => a.Code == "ACC30").FirstOrDefault();
            Assert.AreEqual(ret.Code, rt.Code);
            Assert.AreEqual(ret.Description, rt.Title);
        }

        [TestMethod]
        public async Task RestrictionTypesController_GetHedmAsync_NoCache()
        {
            RestrictionTypeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            RestrictionTypeController.Request.Headers.CacheControl.NoCache = true;

            List<RestrictionType2> RestrictionTypes = await RestrictionTypeController.GetRestrictionTypes2Async() as List<RestrictionType2>;
            RestrictionType2 rt = RestrictionTypes.Where(a => a.Code == "ACC30").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Restriction ret = allRestrictionTypesEntities.Where(a => a.Code == "ACC30").FirstOrDefault();
            Assert.AreEqual(ret.Code, rt.Code);
            Assert.AreEqual(ret.Description, rt.Title);
        }

        [TestMethod]
        public async Task RestrictionTypesController_GetHedmAsync_Cache()
        {
            RestrictionTypeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            RestrictionTypeController.Request.Headers.CacheControl.NoCache = false;

            List<RestrictionType2> RestrictionTypes = await RestrictionTypeController.GetRestrictionTypes2Async() as List<RestrictionType2>;
            RestrictionType2 rt = RestrictionTypes.Where(a => a.Code == "ACC30").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Restriction ret = allRestrictionTypesEntities.Where(a => a.Code == "ACC30").FirstOrDefault();
            Assert.AreEqual(ret.Code, rt.Code);
            Assert.AreEqual(ret.Description, rt.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RestrictionTypeController_GetThrowsIntAppiExc()
        {
            RestrictionTypeServiceMock.Setup(gc => gc.GetRestrictionTypes2Async(It.IsAny<bool>())).Throws<Exception>();

            await RestrictionTypeController.GetRestrictionTypes2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RestrictionTypeController_GetByIdThrowsIntAppiExc()
        {
            RestrictionTypeServiceMock.Setup(gc => gc.GetRestrictionTypeByGuid2Async(It.IsAny<string>())).Throws<Exception>();

            await RestrictionTypeController.GetRestrictionTypeById2Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PerRestTypeController_DeleteThrowsIntApiExc()
        {
            await RestrictionTypeController.DeleteRestrictionTypesAsync("ACC30");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PerRestTypeController_PostThrowsIntAppiExc()
        {
            RestrictionType2 rtDTO = await RestrictionTypeController.PostRestrictionTypesAsync(new RestrictionType2());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PerRestTypeController_PutThrowsIntAppiExc()
        {
            RestrictionType2 rtDTO = await RestrictionTypeController.PutRestrictionTypesAsync(new RestrictionType2());
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Restriction domain entity to its corresponding Restriction Type DTO
        /// </summary>
        /// <param name="source">Restriction domain entity</param>
        /// <returns>RestrictionType DTO</returns>
        private Dtos.RestrictionType2 ConvertRestrictionEntitytoRestrictionTypeDto(Domain.Base.Entities.Restriction source)
        {
            var restrictionType = new Dtos.RestrictionType2();
            restrictionType.Id = source.Guid;
            restrictionType.Code = source.Code;
            restrictionType.Title = source.Description;
            restrictionType.Description = null;
            return restrictionType;
        }
    }
}
