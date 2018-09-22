// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class RehireTypesControllerTests
    {
        [TestClass]
        public class RehireTypesControllerGet
        {
            #region Test Context

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

            #endregion

            private RehireTypesController RehireTypesController;
            private Mock<IHumanResourcesReferenceDataRepository> ReferenceRepositoryMock;
            private IHumanResourcesReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType> allRehireTypeEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IRehireTypeService> demographicsServiceMock;
            private IRehireTypeService demographicsService;
            List<Dtos.RehireType> RehireTypeList;
            private string rehireTypesGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType, Dtos.RehireType>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicsServiceMock = new Mock<IRehireTypeService>();
                demographicsService = demographicsServiceMock.Object;

                allRehireTypeEntities = new TestRehireTypeRepository().GetRehireTypes();
                RehireTypeList = new List<Dtos.RehireType>();

                RehireTypesController = new RehireTypesController(demographicsService, logger);
                RehireTypesController.Request = new HttpRequestMessage();
                RehireTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var rehireType in allRehireTypeEntities)
                {
                    Dtos.RehireType target = ConvertRehireTypeEntityToDto(rehireType);
                    RehireTypeList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetRehireTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRehireTypeEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                RehireTypesController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetRehireTypesByGuidAsync_Validate()
            {
                var thisRehireType = RehireTypeList.Where(m => m.Id == rehireTypesGuid).FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetRehireTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisRehireType);

                var rehireType = await RehireTypesController.GetRehireTypeByIdAsync(rehireTypesGuid);
                Assert.AreEqual(thisRehireType.Id, rehireType.Id);
                Assert.AreEqual(thisRehireType.Code, rehireType.Code);
                Assert.AreEqual(thisRehireType.Description, rehireType.Description);
            }

            [TestMethod]
            public async Task RehireTypesController_GetHedmAsync()
            {
                demographicsServiceMock.Setup(gc => gc.GetRehireTypesAsync(It.IsAny<bool>())).ReturnsAsync(RehireTypeList);

                var result = await RehireTypesController.GetRehireTypesAsync();
                Assert.AreEqual(result.Count(), allRehireTypeEntities.Count());

                int count = allRehireTypeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = RehireTypeList[i];
                    var actual = allRehireTypeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task RehireTypesController_GetHedmAsync_CacheControlNotNull()
            {
                RehireTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicsServiceMock.Setup(gc => gc.GetRehireTypesAsync(It.IsAny<bool>())).ReturnsAsync(RehireTypeList);

                var result = await RehireTypesController.GetRehireTypesAsync();
                Assert.AreEqual(result.Count(), allRehireTypeEntities.Count());

                int count = allRehireTypeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = RehireTypeList[i];
                    var actual = allRehireTypeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task RehireTypesController_GetHedmAsync_NoCache()
            {
                RehireTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                RehireTypesController.Request.Headers.CacheControl.NoCache = true;

                demographicsServiceMock.Setup(gc => gc.GetRehireTypesAsync(It.IsAny<bool>())).ReturnsAsync(RehireTypeList);

                var result = await RehireTypesController.GetRehireTypesAsync();
                Assert.AreEqual(result.Count(), allRehireTypeEntities.Count());

                int count = allRehireTypeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = RehireTypeList[i];
                    var actual = allRehireTypeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task RehireTypesController_GetHedmAsync_Cache()
            {
                RehireTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                RehireTypesController.Request.Headers.CacheControl.NoCache = false;

                demographicsServiceMock.Setup(gc => gc.GetRehireTypesAsync(It.IsAny<bool>())).ReturnsAsync(RehireTypeList);

                var result = await RehireTypesController.GetRehireTypesAsync();
                Assert.AreEqual(result.Count(), allRehireTypeEntities.Count());

                int count = allRehireTypeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = RehireTypeList[i];
                    var actual = allRehireTypeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task RehireTypesController_GetByIdHedmAsync()
            {
                var thisRehireType = RehireTypeList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetRehireTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisRehireType);

                var rehireType = await RehireTypesController.GetRehireTypeByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisRehireType.Id, rehireType.Id);
                Assert.AreEqual(thisRehireType.Code, rehireType.Code);
                Assert.AreEqual(thisRehireType.Description, rehireType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RehireTypesController_GetThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetRehireTypesAsync(It.IsAny<bool>())).Throws<Exception>();

                await RehireTypesController.GetRehireTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RehireTypesController_GetByIdThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetRehireTypeByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await RehireTypesController.GetRehireTypeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RehireTypesController_PostThrowsIntAppiExc()
            {
                await RehireTypesController.PostRehireTypeAsync(RehireTypeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RehireTypesController_PutThrowsIntAppiExc()
            {
                var result = await RehireTypesController.PutRehireTypeAsync(RehireTypeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RehireTypesController_DeleteThrowsIntAppiExc()
            {
                await RehireTypesController.DeleteRehireTypeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a RehireType domain entity to its corresponding RehireType DTO
            /// </summary>
            /// <param name="source">RehireType domain entity</param>
            /// <returns>RehireType DTO</returns>
            private Ellucian.Colleague.Dtos.RehireType ConvertRehireTypeEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.RehireType source)
            {
                var rehireType = new Ellucian.Colleague.Dtos.RehireType();
                rehireType.Id = source.Guid;
                rehireType.Code = source.Code;
                rehireType.Title = source.Description;
                rehireType.Description = null;

                return rehireType;
            }
        }
    }
}