// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Dtos;
using System;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using System.Diagnostics;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class IdentityDocumentTypesControllerTests
    {
        [TestClass]
        public class IdentityDocumentTypesControllerGet
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

            private IdentityDocumentTypesController IdentityDocumentTypesController;
            private Mock<IReferenceDataRepository> ReferenceRepositoryMock;
            private IReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;   
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.IdentityDocumentType> allIdentityDocumentTypes;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDemographicService> demographicsServiceMock;
            private IDemographicService demographicsService;
            List<IdentityDocumentType> IdentityDocumentTypeList;
            private string identityDocumentTypesGuid = "4236641d-5c29-4884-9a17-530820ec9d29";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.IdentityDocumentType, IdentityDocumentType>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicsServiceMock = new Mock<IDemographicService>();
                demographicsService = demographicsServiceMock.Object;

                allIdentityDocumentTypes = new TestIdentityDocumentTypeRepository().Get();
                IdentityDocumentTypeList = new List<IdentityDocumentType>();

                IdentityDocumentTypesController = new IdentityDocumentTypesController(demographicsService, logger);
                IdentityDocumentTypesController.Request = new HttpRequestMessage();
                IdentityDocumentTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var identityDocumentType in allIdentityDocumentTypes)
                {
                    IdentityDocumentType target = ConvertIdentityDocumentTypeEntityToDto(identityDocumentType);
                    IdentityDocumentTypeList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(allIdentityDocumentTypes);
            }

            [TestCleanup]
            public void Cleanup()
            {
                IdentityDocumentTypesController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetIdentityDocumentTypesByGuidAsync_Validate()
            {
                var thisIdentityDocumentType = IdentityDocumentTypeList.Where(m => m.Id == identityDocumentTypesGuid).FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetIdentityDocumentTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisIdentityDocumentType);

                var identityDocumentType = await IdentityDocumentTypesController.GetIdentityDocumentTypeByIdAsync(identityDocumentTypesGuid);
                Assert.AreEqual(thisIdentityDocumentType.Id, identityDocumentType.Id);
                Assert.AreEqual(thisIdentityDocumentType.Code, identityDocumentType.Code);
                Assert.AreEqual(thisIdentityDocumentType.Description, identityDocumentType.Description);
                Assert.AreEqual(thisIdentityDocumentType.identityDocumentTypeCategory, identityDocumentType.identityDocumentTypeCategory);
            }

            [TestMethod]
            public async Task IdentityDocumentTypesController_GetHedmAsync()
            {
                demographicsServiceMock.Setup(gc => gc.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(IdentityDocumentTypeList);

                var result = await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
                Assert.AreEqual(result.Count(), allIdentityDocumentTypes.Count());

                int count = allIdentityDocumentTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = IdentityDocumentTypeList[i];
                    var actual = allIdentityDocumentTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task IdentityDocumentTypesController_GetHedmAsync_CacheControlNotNull()
            {
                IdentityDocumentTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicsServiceMock.Setup(gc => gc.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(IdentityDocumentTypeList);

                var result = await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
                Assert.AreEqual(result.Count(), allIdentityDocumentTypes.Count());

                int count = allIdentityDocumentTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = IdentityDocumentTypeList[i];
                    var actual = allIdentityDocumentTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task IdentityDocumentTypesController_GetHedmAsync_NoCache()
            {
                IdentityDocumentTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                IdentityDocumentTypesController.Request.Headers.CacheControl.NoCache = true;

                demographicsServiceMock.Setup(gc => gc.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(IdentityDocumentTypeList);

                var result = await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
                Assert.AreEqual(result.Count(), allIdentityDocumentTypes.Count());

                int count = allIdentityDocumentTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = IdentityDocumentTypeList[i];
                    var actual = allIdentityDocumentTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task IdentityDocumentTypesController_GetHedmAsync_Cache()
            {
                IdentityDocumentTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                IdentityDocumentTypesController.Request.Headers.CacheControl.NoCache = false;

                demographicsServiceMock.Setup(gc => gc.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(IdentityDocumentTypeList);

                var result = await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
                Assert.AreEqual(result.Count(), allIdentityDocumentTypes.Count());

                int count = allIdentityDocumentTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = IdentityDocumentTypeList[i];
                    var actual = allIdentityDocumentTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task IdentityDocumentTypesController_GetByIdHedmAsync()
            {
                var thisIdentityDocumentType = IdentityDocumentTypeList.Where(m => m.Id == "4236641d-5c29-4884-9a17-530820ec9d29").FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetIdentityDocumentTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisIdentityDocumentType);

                var identityDocumentType = await IdentityDocumentTypesController.GetIdentityDocumentTypeByIdAsync("4236641d-5c29-4884-9a17-530820ec9d29");
                Assert.AreEqual(thisIdentityDocumentType.Id, identityDocumentType.Id);
                Assert.AreEqual(thisIdentityDocumentType.Code, identityDocumentType.Code);
                Assert.AreEqual(thisIdentityDocumentType.Description, identityDocumentType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypesController_GetThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).Throws<Exception>();

                await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypesController_GetByIdThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetIdentityDocumentTypeByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await IdentityDocumentTypesController.GetIdentityDocumentTypeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypeController_GetIdentityDocumentTypeByIdAsync_IntegrationApiException()
            {
                var expected = IdentityDocumentTypeList.FirstOrDefault();
                demographicsServiceMock.Setup(x => x.GetIdentityDocumentTypeByGuidAsync(expected.Id)).Throws<IntegrationApiException>();
                Debug.Assert(expected != null, "expected != null");
                await IdentityDocumentTypesController.GetIdentityDocumentTypeByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypeController_GetIdentityDocumentTypesAsync_IntegrationApiException()
            {
                demographicsServiceMock.Setup(s => s.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
                await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypeController_GetIdentityDocumentTypeByIdAsync_PermissionsException()
            {
                var expected = IdentityDocumentTypeList.FirstOrDefault();
                demographicsServiceMock.Setup(x => x.GetIdentityDocumentTypeByGuidAsync(expected.Id)).Throws<PermissionsException>();
                Debug.Assert(expected != null, "expected != null");
                await IdentityDocumentTypesController.GetIdentityDocumentTypeByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypeController_GetIdentityDocumentTypesAsync_PermissionsException()
            {
                demographicsServiceMock.Setup(s => s.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
                await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypeController_GetIdentityDocumentTypeByIdAsync_KeyNotFoundException()
            {
                var expected = IdentityDocumentTypeList.FirstOrDefault();
                demographicsServiceMock.Setup(x => x.GetIdentityDocumentTypeByGuidAsync(expected.Id)).Throws<KeyNotFoundException>();
                Debug.Assert(expected != null, "expected != null");
                await IdentityDocumentTypesController.GetIdentityDocumentTypeByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypeController_GetIdentityDocumentTypesAsync_KeyNotFoundException()
            {
                demographicsServiceMock.Setup(s => s.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypeController_GetIdentityDocumentTypesAsync_ArgumentNullException()
            {
                demographicsServiceMock.Setup(s => s.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).Throws<ArgumentNullException>();
                await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypeController_GetIdentityDocumentTypesAsync_RepositoryException()
            {
                demographicsServiceMock.Setup(s => s.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
                await IdentityDocumentTypesController.GetIdentityDocumentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypesController_PostThrowsIntAppiExc()
            {
                await IdentityDocumentTypesController.PostIdentityDocumentTypeAsync(IdentityDocumentTypeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypesController_PutThrowsIntAppiExc()
            {
                var result = await IdentityDocumentTypesController.PutIdentityDocumentTypeAsync(IdentityDocumentTypeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdentityDocumentTypesController_DeleteThrowsIntAppiExc()
            {
                await IdentityDocumentTypesController.DeleteIdentityDocumentTypeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
            /// <summary>
            /// Converts a IdentityDocumentType domain entity to its corresponding IdentityDocumentType DTO
            /// </summary>
            /// <param name="source">IdentityDocumentType domain entity</param>
            /// <returns>IdentityDocumentType DTO</returns>
            private Ellucian.Colleague.Dtos.IdentityDocumentType ConvertIdentityDocumentTypeEntityToDto(Ellucian.Colleague.Domain.Base.Entities.IdentityDocumentType source)
            {
                var identityDocumentType = new Ellucian.Colleague.Dtos.IdentityDocumentType();
                identityDocumentType.Id = source.Guid;
                identityDocumentType.Code = source.Code;
                identityDocumentType.Title = source.Description;
                identityDocumentType.Description = null;

                return identityDocumentType;
            }
        }
    }
}