// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CommerceTaxCodesControllerTests
    {
        [TestClass]
        public class CommerceTaxCodesControllerGet
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

            private CommerceTaxCodesController CommerceTaxCodesController;
            private Mock<IReferenceDataRepository> ReferenceRepositoryMock;
            private IReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CommerceTaxCode> allCommerceTaxCodeEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<ICommerceTaxCodeService> commerceTaxCodesServiceMock;
            private ICommerceTaxCodeService commerceTaxCodesService;
            List<CommerceTaxCode> CommerceTaxCodeList;
            private string commerceTaxCodesGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.CommerceTaxCode, CommerceTaxCode>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                commerceTaxCodesServiceMock = new Mock<ICommerceTaxCodeService>();
                commerceTaxCodesService = commerceTaxCodesServiceMock.Object;

                allCommerceTaxCodeEntities = new TestCommerceTaxCodesRepository().GetCommerceTaxCodes();
                CommerceTaxCodeList = new List<CommerceTaxCode>();

                CommerceTaxCodesController = new CommerceTaxCodesController(commerceTaxCodesService, logger);
                CommerceTaxCodesController.Request = new HttpRequestMessage();
                CommerceTaxCodesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var denomination in allCommerceTaxCodeEntities)
                {
                    CommerceTaxCode target = ConvertCommerceTaxCodeEntityToDto(denomination);
                    CommerceTaxCodeList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(allCommerceTaxCodeEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                CommerceTaxCodesController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetCommerceTaxCodesByGuidAsync_Validate()
            {
                var thisCommerceTaxCode = CommerceTaxCodeList.Where(m => m.Id == commerceTaxCodesGuid).FirstOrDefault();

                commerceTaxCodesServiceMock.Setup(x => x.GetCommerceTaxCodeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisCommerceTaxCode);

                var commerceTaxCode = await CommerceTaxCodesController.GetCommerceTaxCodeByIdAsync(commerceTaxCodesGuid);
                Assert.AreEqual(thisCommerceTaxCode.Id, commerceTaxCode.Id);
                Assert.AreEqual(thisCommerceTaxCode.Code, commerceTaxCode.Code);
                Assert.AreEqual(thisCommerceTaxCode.Description, commerceTaxCode.Description);
            }

            [TestMethod]
            public async Task CommerceTaxCodesController_GetHedmAsync()
            {
                commerceTaxCodesServiceMock.Setup(gc => gc.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(CommerceTaxCodeList);

                var result = await CommerceTaxCodesController.GetCommerceTaxCodesAsync();
                Assert.AreEqual(result.Count(), allCommerceTaxCodeEntities.Count());

                int count = allCommerceTaxCodeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = CommerceTaxCodeList[i];
                    var actual = allCommerceTaxCodeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task CommerceTaxCodesController_GetHedmAsync_CacheControlNotNull()
            {
                CommerceTaxCodesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                commerceTaxCodesServiceMock.Setup(gc => gc.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(CommerceTaxCodeList);

                var result = await CommerceTaxCodesController.GetCommerceTaxCodesAsync();
                Assert.AreEqual(result.Count(), allCommerceTaxCodeEntities.Count());

                int count = allCommerceTaxCodeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = CommerceTaxCodeList[i];
                    var actual = allCommerceTaxCodeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task CommerceTaxCodesController_GetHedmAsync_NoCache()
            {
                CommerceTaxCodesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                CommerceTaxCodesController.Request.Headers.CacheControl.NoCache = true;

                commerceTaxCodesServiceMock.Setup(gc => gc.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(CommerceTaxCodeList);

                var result = await CommerceTaxCodesController.GetCommerceTaxCodesAsync();
                Assert.AreEqual(result.Count(), allCommerceTaxCodeEntities.Count());

                int count = allCommerceTaxCodeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = CommerceTaxCodeList[i];
                    var actual = allCommerceTaxCodeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task CommerceTaxCodesController_GetHedmAsync_Cache()
            {
                CommerceTaxCodesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                CommerceTaxCodesController.Request.Headers.CacheControl.NoCache = false;

                commerceTaxCodesServiceMock.Setup(gc => gc.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(CommerceTaxCodeList);

                var result = await CommerceTaxCodesController.GetCommerceTaxCodesAsync();
                Assert.AreEqual(result.Count(), allCommerceTaxCodeEntities.Count());

                int count = allCommerceTaxCodeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = CommerceTaxCodeList[i];
                    var actual = allCommerceTaxCodeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task CommerceTaxCodesController_GetByIdHedmAsync()
            {
                CommerceTaxCodesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                CommerceTaxCodesController.Request.Headers.CacheControl.NoCache = true;

                var thisCommerceTaxCode = CommerceTaxCodeList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                commerceTaxCodesServiceMock.Setup(x => x.GetCommerceTaxCodeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisCommerceTaxCode);

                var commerceTaxCode = await CommerceTaxCodesController.GetCommerceTaxCodeByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisCommerceTaxCode.Id, commerceTaxCode.Id);
                Assert.AreEqual(thisCommerceTaxCode.Code, commerceTaxCode.Code);
                Assert.AreEqual(thisCommerceTaxCode.Description, commerceTaxCode.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodesController_GetThrowsIntAppiExc()
            {
                commerceTaxCodesServiceMock.Setup(gc => gc.GetCommerceTaxCodesAsync(It.IsAny<bool>())).Throws<Exception>();

                await CommerceTaxCodesController.GetCommerceTaxCodesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodesController_GetByIdThrowsIntAppiExc_NullId()
            {
                await CommerceTaxCodesController.GetCommerceTaxCodeByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodesController_GetByIdThrowsIntAppiExc()
            {
                commerceTaxCodesServiceMock.Setup(gc => gc.GetCommerceTaxCodeByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await CommerceTaxCodesController.GetCommerceTaxCodeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodesController_PostThrowsIntAppiExc()
            {
                await CommerceTaxCodesController.PostCommerceTaxCodeAsync(CommerceTaxCodeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodesController_PutThrowsIntAppiExc()
            {
                var result = await CommerceTaxCodesController.PutCommerceTaxCodeAsync(CommerceTaxCodeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodesController_DeleteThrowsIntAppiExc()
            {
                await CommerceTaxCodesController.DeleteCommerceTaxCodeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a CommerceTaxCode domain entity to its corresponding CommerceTaxCode DTO
            /// </summary>
            /// <param name="source">CommerceTaxCode domain entity</param>
            /// <returns>CommerceTaxCode DTO</returns>
            private Ellucian.Colleague.Dtos.CommerceTaxCode ConvertCommerceTaxCodeEntityToDto(Ellucian.Colleague.Domain.Base.Entities.CommerceTaxCode source)
            {
                var commerceTaxCode = new Ellucian.Colleague.Dtos.CommerceTaxCode();
                commerceTaxCode.Id = source.Guid;
                commerceTaxCode.Code = source.Code;
                commerceTaxCode.Title = source.Description;
                commerceTaxCode.Description = null;

                return commerceTaxCode;
            }
        }
    }
}