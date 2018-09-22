// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PayrollDeductionArrangementChangeReasonsControllerTests
    {
        [TestClass]
        public class PayrollDeductionArrangementChangeReasonsControllerGet
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

            private PayrollDeductionArrangementChangeReasonsController payrollDeductionArrangementChangeReasonsController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IPayrollDeductionArrangementChangeReasonsService> payrollDeductionArrangementChangeReasonsService;
            List<PayrollDeductionArrangementChangeReason> payrollDeductionArrangementChangeReasonDtoList;
            private string payrollDeductionArrangementChangeReasonGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                payrollDeductionArrangementChangeReasonsService = new Mock<IPayrollDeductionArrangementChangeReasonsService>();

                BuildData();

                payrollDeductionArrangementChangeReasonsController = new PayrollDeductionArrangementChangeReasonsController(logger, payrollDeductionArrangementChangeReasonsService.Object);
                payrollDeductionArrangementChangeReasonsController.Request = new HttpRequestMessage();
                payrollDeductionArrangementChangeReasonsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                payrollDeductionArrangementChangeReasonsController = null;
                payrollDeductionArrangementChangeReasonsService = null;
                payrollDeductionArrangementChangeReasonDtoList = null;
            }

            [TestMethod]
            public async Task PayrollDeductionArrangementChangeReason_GetAll_Async()
            {
                payrollDeductionArrangementChangeReasonsService.Setup(x => x.GetPayrollDeductionArrangementChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(payrollDeductionArrangementChangeReasonDtoList);

                var actuals = await payrollDeductionArrangementChangeReasonsController.GetAllPayrollDeductionArrangementChangeReasonsAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = payrollDeductionArrangementChangeReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task PayrollDeductionArrangementChangeReason_GetAll_TrueCache_Async()
            {
                payrollDeductionArrangementChangeReasonsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                payrollDeductionArrangementChangeReasonsController.Request.Headers.CacheControl.NoCache = true;

                payrollDeductionArrangementChangeReasonsService.Setup(x => x.GetPayrollDeductionArrangementChangeReasonsAsync(true)).ReturnsAsync(payrollDeductionArrangementChangeReasonDtoList);

                var actuals = await payrollDeductionArrangementChangeReasonsController.GetAllPayrollDeductionArrangementChangeReasonsAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = payrollDeductionArrangementChangeReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }           

            [TestMethod]
            public async Task PayrollDeductionArrangementChangeReason_GetById_Async()
            {
                var expected = payrollDeductionArrangementChangeReasonDtoList.FirstOrDefault(i => i.Id.Equals(payrollDeductionArrangementChangeReasonGuid));

                payrollDeductionArrangementChangeReasonsService.Setup(x => x.GetPayrollDeductionArrangementChangeReasonByIdAsync(payrollDeductionArrangementChangeReasonGuid)).ReturnsAsync(expected);

                var actual = await payrollDeductionArrangementChangeReasonsController.GetPayrollDeductionArrangementChangeReasonByIdAsync(payrollDeductionArrangementChangeReasonGuid);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PayrollDeductionArrangementChangeReason_GetAll_Exception()
            {
                payrollDeductionArrangementChangeReasonsService.Setup(x => x.GetPayrollDeductionArrangementChangeReasonsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await payrollDeductionArrangementChangeReasonsController.GetAllPayrollDeductionArrangementChangeReasonsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PayrollDeductionArrangementChangeReason_GetAById_Exception()
            {
                payrollDeductionArrangementChangeReasonsService.Setup(x => x.GetPayrollDeductionArrangementChangeReasonByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actuals = await payrollDeductionArrangementChangeReasonsController.GetPayrollDeductionArrangementChangeReasonByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PayrollDeductionArrangementChangeReason_GetAById_KeyNotFoundException()
            {
                payrollDeductionArrangementChangeReasonsService.Setup(x => x.GetPayrollDeductionArrangementChangeReasonByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await payrollDeductionArrangementChangeReasonsController.GetPayrollDeductionArrangementChangeReasonByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PayrollDeductionArrangementChangeReasonsController_PostThrowsIntAppiExc()
            {
                await payrollDeductionArrangementChangeReasonsController.PostPayrollDeductionArrangementChangeReasonAsync(payrollDeductionArrangementChangeReasonDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PayrollDeductionArrangementChangeReasonsController_PutThrowsIntAppiExc()
            {
                var result = await payrollDeductionArrangementChangeReasonsController.PutPayrollDeductionArrangementChangeReasonAsync(payrollDeductionArrangementChangeReasonDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PayrollDeductionArrangementChangeReasonsController_DeleteThrowsIntAppiExc()
            {
                await payrollDeductionArrangementChangeReasonsController.DeletePayrollDeductionArrangementChangeReasonAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            private void BuildData()
            {
                payrollDeductionArrangementChangeReasonDtoList = new List<PayrollDeductionArrangementChangeReason>() 
                {
                    new PayrollDeductionArrangementChangeReason(){Id = "625c69ff-280b-4ed3-9474-662a43616a8a", Code = "MAR", Description = null, Title = "Marriage"},
                    new PayrollDeductionArrangementChangeReason(){Id = "bfea651b-8e27-4fcd-abe3-04573443c04c", Code = "BOC", Description = null, Title = "Birth of Child"},
                    new PayrollDeductionArrangementChangeReason(){Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023", Code = "SJC", Description = null, Title = "Spouse Job Change"},
                    new PayrollDeductionArrangementChangeReason(){Id = "e9e6837f-2c51-431b-9069-4ac4c0da3041", Code = "DIV", Description = null, Title = "Divorce"},
                    new PayrollDeductionArrangementChangeReason(){Id = "80779c4f-b2ac-4ad4-a970-ca5699d9891f", Code = "ADP", Description = null, Title = "Adoption"},
                    new PayrollDeductionArrangementChangeReason(){Id = "ae21110e-991e-405e-9d8b-47eeff210a2d", Code = "DEA", Description = null, Title = "Death"}
                };
            }
        }
    }
}