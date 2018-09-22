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
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmploymentTerminationReasonsControllerTests
    {
        [TestClass]
        public class EmploymentTerminationReasonsControllerGet
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

            private EmploymentTerminationReasonsController employmentTerminationReasonsController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IEmploymentStatusEndingReasonService> employmentStatusEndingReasonService;
            List<EmploymentStatusEndingReason> employmentStatusEndingReasonDtoList;
            private string employmentTerminationReasonsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                employmentStatusEndingReasonService = new Mock<IEmploymentStatusEndingReasonService>();

                BuildData();

                employmentTerminationReasonsController = new EmploymentTerminationReasonsController(employmentStatusEndingReasonService.Object, logger);
                employmentTerminationReasonsController.Request = new HttpRequestMessage();
                employmentTerminationReasonsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                employmentTerminationReasonsController = null;
                employmentStatusEndingReasonService = null;
                employmentStatusEndingReasonDtoList = null;
            }

            [TestMethod]
            public async Task GetEmploymentTerminationReason_GetAll_Async()
            {
                employmentStatusEndingReasonService.Setup(x => x.GetEmploymentStatusEndingReasonsAsync(It.IsAny<bool>())).ReturnsAsync(employmentStatusEndingReasonDtoList);

                var actuals = await employmentTerminationReasonsController.GetEmploymentTerminationReasonsAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = employmentStatusEndingReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task GetEmploymentTerminationReason_GetAll_TrueCache_Async()
            {
                employmentTerminationReasonsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                employmentTerminationReasonsController.Request.Headers.CacheControl.NoCache = true;

                employmentStatusEndingReasonService.Setup(x => x.GetEmploymentStatusEndingReasonsAsync(true)).ReturnsAsync(employmentStatusEndingReasonDtoList);

                var actuals = await employmentTerminationReasonsController.GetEmploymentTerminationReasonsAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = employmentStatusEndingReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }           

            [TestMethod]
            public async Task GetEmploymentTerminationReason_GetById_Async()
            {
                var expected = employmentStatusEndingReasonDtoList.FirstOrDefault(i => i.Id.Equals(employmentTerminationReasonsGuid));

                employmentStatusEndingReasonService.Setup(x => x.GetEmploymentStatusEndingReasonByIdAsync(employmentTerminationReasonsGuid)).ReturnsAsync(expected);

                var actual = await employmentTerminationReasonsController.GetEmploymentTerminationReasonByIdAsync(employmentTerminationReasonsGuid);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmploymentTerminationReason_GetAll_Exception()
            {
                employmentStatusEndingReasonService.Setup(x => x.GetEmploymentStatusEndingReasonsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await employmentTerminationReasonsController.GetEmploymentTerminationReasonsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmploymentTerminationReason_GetAById_Exception()
            {
                employmentStatusEndingReasonService.Setup(x => x.GetEmploymentStatusEndingReasonByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actuals = await employmentTerminationReasonsController.GetEmploymentTerminationReasonByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentTerminationReasonsController_PostThrowsIntAppiExc()
            {
                await employmentTerminationReasonsController.PostEmploymentTerminationReasonAsync(employmentStatusEndingReasonDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentTerminationReasonsController_PutThrowsIntAppiExc()
            {
                var result = await employmentTerminationReasonsController.PutEmploymentTerminationReasonAsync(employmentStatusEndingReasonDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentTerminationReasonsController_DeleteThrowsIntAppiExc()
            {
                await employmentTerminationReasonsController.DeleteEmploymentTerminationReasonAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            private void BuildData()
            {
                employmentStatusEndingReasonDtoList = new List<EmploymentStatusEndingReason>() 
                {
                    new EmploymentStatusEndingReason(){Id = "625c69ff-280b-4ed3-9474-662a43616a8a", Code = "TER", Description = null, Title = "Termination"},
                    new EmploymentStatusEndingReason(){Id = "bfea651b-8e27-4fcd-abe3-04573443c04c", Code = "LOA", Description = null, Title = "Leave of Absence"},
                    new EmploymentStatusEndingReason(){Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023", Code = "DEM", Description = null, Title = "Demotion"},
                    new EmploymentStatusEndingReason(){Id = "e9e6837f-2c51-431b-9069-4ac4c0da3041", Code = "RET", Description = null, Title = "Retired"},
                    new EmploymentStatusEndingReason(){Id = "80779c4f-b2ac-4ad4-a970-ca5699d9891f", Code = "EOC", Description = null, Title = "End of Contract"},
                    new EmploymentStatusEndingReason(){Id = "ae21110e-991e-405e-9d8b-47eeff210a2d", Code = "EEO", Description = null, Title = "Change EEO Information"}
                };
            }
        }
    }
}