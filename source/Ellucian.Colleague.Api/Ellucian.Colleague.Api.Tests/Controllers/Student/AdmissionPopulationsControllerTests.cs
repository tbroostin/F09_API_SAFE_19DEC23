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
    public class AdmissionPopulationsControllerTests
    {
        [TestClass]
        public class AdmissionPopulationsControllerGet
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

            private AdmissionPopulationsController AdmissionPopulationsController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IAdmissionPopulationsService> AdmissionPopulationsService;
            List<AdmissionPopulations> admissionPopulationDtoList;
            private string admissionPopulationGuid = "03ef76f3-61be-4990-8a9d-9a80282fc420";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                AdmissionPopulationsService = new Mock<IAdmissionPopulationsService>();

                BuildData();

                AdmissionPopulationsController = new AdmissionPopulationsController(AdmissionPopulationsService.Object, logger); AdmissionPopulationsController.Request = new HttpRequestMessage();
                AdmissionPopulationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                AdmissionPopulationsController = null;
                AdmissionPopulationsService = null;
                admissionPopulationDtoList = null;
            }

            #region GET ALL Tests
            [TestMethod]
            public async Task AdmissionPopulations_GetAll_Async()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulationDtoList);

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationsAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = admissionPopulationDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task AdmissionPopulations_GetAll_TrueCache_Async()
            {
                AdmissionPopulationsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                AdmissionPopulationsController.Request.Headers.CacheControl.NoCache = true;

                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsAsync(true)).ReturnsAsync(admissionPopulationDtoList);

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationsAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = admissionPopulationDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
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
            public async Task AdmissionPopulations_GetAll_Exception()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAll__PermissionsException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAll_ArgumentException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationsAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAll_RepositoryException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationsAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAll_IntegrationApiException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationsAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAll_KeyNotFoundException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationsAsync();
            }

            #endregion
            #endregion

            #region GET ID TESTS
            [TestMethod]
            public async Task AdmissionPopulations_GetById_Async()
            {
                var expected = admissionPopulationDtoList.FirstOrDefault(i => i.Id.Equals(admissionPopulationGuid));

                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsByGuidAsync(admissionPopulationGuid)).ReturnsAsync(expected);

                var actual = await AdmissionPopulationsController.GetAdmissionPopulationByIdAsync(admissionPopulationGuid);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
            }



            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAById_Exception()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAById_KeyNotFoundException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAById__PermissionsException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAById_ArgumentException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationByIdAsync(It.IsAny<string>());
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAById_RepositoryException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationByIdAsync(It.IsAny<string>());
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionPopulations_GetAById_IntegrationApiException()
            {
                AdmissionPopulationsService.Setup(x => x.GetAdmissionPopulationsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

                var actuals = await AdmissionPopulationsController.GetAdmissionPopulationByIdAsync(It.IsAny<string>());
            }
            #endregion

            private void BuildData()
            {
                admissionPopulationDtoList = new List<AdmissionPopulations>() 
                {
                    new AdmissionPopulations(){Id = "03ef76f3-61be-4990-8a9d-9a80282fc420", Code = "CR", Description = null, Title = "Certificate"},
                    new AdmissionPopulations(){Id = "d2f4f0af-6714-48c7-88dd-1c40cb407b6c", Code = "FH", Description = null, Title = "Freshman Honors"},
                    new AdmissionPopulations(){Id = "c517d7a5-f06a-42c8-85ad-b6320e1c0c2a", Code = "FR", Description = null, Title = "First Time Freshman"},
                    new AdmissionPopulations(){Id = "6c591aaa-5d33-4b19-b5ed-f6cf8956ef0a", Code = "GD", Description = null, Title = "Graduate"},
                    new AdmissionPopulations(){Id = "81cd5b52-9705-4b1b-8eed-669c63db05e2", Code = "ND", Description = null, Title = "Non-Degree"},
                    new AdmissionPopulations(){Id = "164dc1ad-4d72-4dae-987d-52f761bb0132", Code = "TR", Description = null, Title = "Transfer"}
                };
            }
        }
    }
}