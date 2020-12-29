// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
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
                var result = await CommerceTaxCodesController.PutCommerceTaxCodeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023", CommerceTaxCodeList[0]);
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

        [TestClass]
        public class CommerceTaxCodeController_CommerceTaxCodeRatesTests
        {
            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext { get; set; }

            private Mock<ICommerceTaxCodeService> commerceTaxCodeServiceMock;
            private Mock<ILogger> loggerMock;
            private CommerceTaxCodesController commerceTaxCodeController;
            private IEnumerable<Domain.Base.Entities.CommerceTaxCodeRate> allCommerceTaxCodeRates;
            private List<Dtos.CommerceTaxCodeRates> commerceTaxCodeRatesCollection;
            private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                commerceTaxCodeServiceMock = new Mock<ICommerceTaxCodeService>();
                loggerMock = new Mock<ILogger>();
                commerceTaxCodeRatesCollection = new List<Dtos.CommerceTaxCodeRates>();

                allCommerceTaxCodeRates = new List<Domain.Base.Entities.CommerceTaxCodeRate>()
                {
                    new Domain.Base.Entities.CommerceTaxCodeRate("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "FL2", "FL2 Bookstore Tax Code 2%"),
                    new Domain.Base.Entities.CommerceTaxCodeRate("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "ST", "Sales Tax"),
                    new Domain.Base.Entities.CommerceTaxCodeRate("d2253ac7-9931-4560-b42f-1fccd43c952e", "G1", "GST 85% rebate")
                };

                foreach (var source in allCommerceTaxCodeRates)
                {
                    var commerceTaxCodeRates = new Ellucian.Colleague.Dtos.CommerceTaxCodeRates
                    {
                        Id = source.Guid,
                        Code = source.Code,
                        Title = source.Description,
                        Description = null
                    };
                    commerceTaxCodeRatesCollection.Add(commerceTaxCodeRates);
                }

                commerceTaxCodeController = new CommerceTaxCodesController(commerceTaxCodeServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
                commerceTaxCodeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                commerceTaxCodeController = null;
                allCommerceTaxCodeRates = null;
                commerceTaxCodeRatesCollection = null;
                loggerMock = null;
                commerceTaxCodeServiceMock = null;
            }

            [TestMethod]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRates_ValidateFields_Nocache()
            {
                commerceTaxCodeController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesAsync(false)).ReturnsAsync(commerceTaxCodeRatesCollection);

                var sourceContexts = (await commerceTaxCodeController.GetCommerceTaxCodeRatesAsync()).ToList();
                Assert.AreEqual(commerceTaxCodeRatesCollection.Count, sourceContexts.Count);
                for (var i = 0; i < sourceContexts.Count; i++)
                {
                    var expected = commerceTaxCodeRatesCollection[i];
                    var actual = sourceContexts[i];
                    Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                    Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                    Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                }
            }

            [TestMethod]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRates_ValidateFields_Cache()
            {
                commerceTaxCodeController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesAsync(true)).ReturnsAsync(commerceTaxCodeRatesCollection);

                var sourceContexts = (await commerceTaxCodeController.GetCommerceTaxCodeRatesAsync()).ToList();
                Assert.AreEqual(commerceTaxCodeRatesCollection.Count, sourceContexts.Count);
                for (var i = 0; i < sourceContexts.Count; i++)
                {
                    var expected = commerceTaxCodeRatesCollection[i];
                    var actual = sourceContexts[i];
                    Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                    Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                    Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRates_KeyNotFoundException()
            {
                //
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesAsync(false))
                    .Throws<KeyNotFoundException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRates_PermissionsException()
            {

                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesAsync(false))
                    .Throws<PermissionsException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRates_ArgumentException()
            {

                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesAsync(false))
                    .Throws<ArgumentException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRates_RepositoryException()
            {

                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesAsync(false))
                    .Throws<RepositoryException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRates_IntegrationApiException()
            {

                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesAsync(false))
                    .Throws<IntegrationApiException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesAsync();
            }

            [TestMethod]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRatesByGuidAsync_ValidateFields()
            {
                var expected = commerceTaxCodeRatesCollection.FirstOrDefault();
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

                var actual = await commerceTaxCodeController.GetCommerceTaxCodeRatesByGuidAsync(expected.Id);

                Assert.AreEqual(expected.Id, actual.Id, "Id");
                Assert.AreEqual(expected.Title, actual.Title, "Title");
                Assert.AreEqual(expected.Code, actual.Code, "Code");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRates_Exception()
            {
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesAsync(false)).Throws<Exception>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRatesByGuidAsync_Exception()
            {
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesByGuidAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRatesByGuid_KeyNotFoundException()
            {
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<KeyNotFoundException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRatesByGuid_PermissionsException()
            {
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<PermissionsException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRatesByGuid_ArgumentException()
            {
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<ArgumentException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRatesByGuid_RepositoryException()
            {
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<RepositoryException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRatesByGuid_IntegrationApiException()
            {
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<IntegrationApiException>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_GetCommerceTaxCodeRatesByGuid_Exception()
            {
                commerceTaxCodeServiceMock.Setup(x => x.GetCommerceTaxCodeRatesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<Exception>();
                await commerceTaxCodeController.GetCommerceTaxCodeRatesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_PostCommerceTaxCodeRatesAsync_Exception()
            {
                await commerceTaxCodeController.PostCommerceTaxCodeRatesAsync(commerceTaxCodeRatesCollection.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_PutCommerceTaxCodeRatesAsync_Exception()
            {
                var sourceContext = commerceTaxCodeRatesCollection.FirstOrDefault();
                await commerceTaxCodeController.PutCommerceTaxCodeRatesAsync(sourceContext.Id, sourceContext);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CommerceTaxCodeController_DeleteCommerceTaxCodeRatesAsync_Exception()
            {
                await commerceTaxCodeController.DeleteCommerceTaxCodeRatesAsync(commerceTaxCodeRatesCollection.FirstOrDefault().Id);
            }
        }
    }
}