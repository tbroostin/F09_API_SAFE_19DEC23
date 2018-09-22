// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdmissionApplicationTypesControllerTests
    {
        [TestClass]
        public class AdmissionApplicationTypesControllerGet
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

            private AdmissionApplicationTypesController AdmissionApplicationTypesController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IAdmissionApplicationTypesService> AdmissionApplicationTypesService;
            List<AdmissionApplicationTypes> admissionApplicationTypeDtoList;
            private string admissionApplicationTypeGuid = "03ef76f3-61be-4990-8a9d-9a80282fc420";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                AdmissionApplicationTypesService = new Mock<IAdmissionApplicationTypesService>();

                BuildData();

                AdmissionApplicationTypesController = new AdmissionApplicationTypesController(AdmissionApplicationTypesService.Object, logger); AdmissionApplicationTypesController.Request = new HttpRequestMessage();
                AdmissionApplicationTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                AdmissionApplicationTypesController = null;
                AdmissionApplicationTypesService = null;
                admissionApplicationTypeDtoList = null;
            }

            #region GET ALL Tests
            [TestMethod]
            public async Task AdmissionApplicationTypes_GetAll_Async()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionApplicationTypeDtoList);

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypesAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationTypes_GetAll_TrueCache_Async()
            {
                AdmissionApplicationTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                AdmissionApplicationTypesController.Request.Headers.CacheControl.NoCache = true;

                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesAsync(true)).ReturnsAsync(admissionApplicationTypeDtoList);

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypesAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            #region Get ALL Exception Tests

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAll_Exception()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAll__PermissionsException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAll_ArgumentException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypesAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAll_RepositoryException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypesAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAll_IntegrationApiException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypesAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAll_KeyNotFoundException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypesAsync();
            }

            #endregion
            #endregion

            #region GET ID TESTS
            [TestMethod]
            public async Task AdmissionApplicationTypes_GetById_Async()
            {
                var expected = admissionApplicationTypeDtoList.FirstOrDefault(i => i.Id.Equals(admissionApplicationTypeGuid));

                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesByGuidAsync(admissionApplicationTypeGuid)).ReturnsAsync(expected);

                var actual = await AdmissionApplicationTypesController.GetAdmissionApplicationTypeByIdAsync(admissionApplicationTypeGuid);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
            }



            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAById_Exception()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAById_KeyNotFoundException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAById__PermissionsException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAById_ArgumentException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypeByIdAsync(It.IsAny<string>());
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAById_RepositoryException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypeByIdAsync(It.IsAny<string>());
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationTypes_GetAById_IntegrationApiException()
            {
                AdmissionApplicationTypesService.Setup(x => x.GetAdmissionApplicationTypesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

                var actuals = await AdmissionApplicationTypesController.GetAdmissionApplicationTypeByIdAsync(It.IsAny<string>());
            }
            #endregion

            private void BuildData()
            {
                admissionApplicationTypeDtoList = new List<AdmissionApplicationTypes>() 
                {
                    new AdmissionApplicationTypes(){Id = "03ef76f3-61be-4990-8a9d-9a80282fc420", Code = "ST", Description = null, Title = "Standard"}
                };
            }
        }
    }
}