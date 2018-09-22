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
    public class DeductionTypesControllerTests
    {
        [TestClass]
        public class DeductionTypesControllerGet
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

            private DeductionTypesController deductionTypesController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDeductionTypesService> deductionTypesService;
            List<DeductionType> deductionTypeDtoList;
            private string deductionTypeGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                deductionTypesService = new Mock<IDeductionTypesService>();

                BuildData();

                deductionTypesController = new DeductionTypesController(logger, deductionTypesService.Object);
                deductionTypesController.Request = new HttpRequestMessage();
                deductionTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                deductionTypesController = null;
                deductionTypesService = null;
                deductionTypeDtoList = null;
            }

            [TestMethod]
            public async Task DeductionType_GetAll_Async()
            {
                deductionTypesService.Setup(x => x.GetDeductionTypesAsync(It.IsAny<bool>())).ReturnsAsync(deductionTypeDtoList);

                var actuals = await deductionTypesController.GetAllDeductionTypesAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task DeductionType_GetAll_TrueCache_Async()
            {
                deductionTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                deductionTypesController.Request.Headers.CacheControl.NoCache = true;

                deductionTypesService.Setup(x => x.GetDeductionTypesAsync(true)).ReturnsAsync(deductionTypeDtoList);

                var actuals = await deductionTypesController.GetAllDeductionTypesAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }           

            [TestMethod]
            public async Task DeductionType_GetById_Async()
            {
                var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(deductionTypeGuid));

                deductionTypesService.Setup(x => x.GetDeductionTypeByIdAsync(deductionTypeGuid)).ReturnsAsync(expected);

                var actual = await deductionTypesController.GetDeductionTypeByIdAsync(deductionTypeGuid);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionType_GetAll_Exception()
            {
                deductionTypesService.Setup(x => x.GetDeductionTypesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await deductionTypesController.GetAllDeductionTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionType_GetAById_Exception()
            {
                deductionTypesService.Setup(x => x.GetDeductionTypeByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actuals = await deductionTypesController.GetDeductionTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionType_GetAById_KeyNotFoundException()
            {
                deductionTypesService.Setup(x => x.GetDeductionTypeByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await deductionTypesController.GetDeductionTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionTypesController_PostThrowsIntAppiExc()
            {
                await deductionTypesController.PostDeductionTypeAsync(deductionTypeDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionTypesController_PutThrowsIntAppiExc()
            {
                var result = await deductionTypesController.PutDeductionTypeAsync(deductionTypeDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionTypesController_DeleteThrowsIntAppiExc()
            {
                await deductionTypesController.DeleteDeductionTypeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            private void BuildData()
            {
                deductionTypeDtoList = new List<DeductionType>() 
                {
                    new DeductionType(){Id = "625c69ff-280b-4ed3-9474-662a43616a8a", Code = "MAR", Description = null, Title = "Marriage"},
                    new DeductionType(){Id = "bfea651b-8e27-4fcd-abe3-04573443c04c", Code = "BOC", Description = null, Title = "Birth of Child"},
                    new DeductionType(){Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023", Code = "SJC", Description = null, Title = "Spouse Job Change"},
                    new DeductionType(){Id = "e9e6837f-2c51-431b-9069-4ac4c0da3041", Code = "DIV", Description = null, Title = "Divorce"},
                    new DeductionType(){Id = "80779c4f-b2ac-4ad4-a970-ca5699d9891f", Code = "ADP", Description = null, Title = "Adoption"},
                    new DeductionType(){Id = "ae21110e-991e-405e-9d8b-47eeff210a2d", Code = "DEA", Description = null, Title = "Death"}
                };
            }
        }

        [TestClass]
        public class DeductionTypesControllerGet_V11
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

            private DeductionTypesController deductionTypesController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDeductionTypesService> deductionTypesService;
            List<DeductionType2> deductionTypeDtoList;
            private string deductionTypeGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                deductionTypesService = new Mock<IDeductionTypesService>();

                BuildData();

                deductionTypesController = new DeductionTypesController(logger, deductionTypesService.Object);
                deductionTypesController.Request = new HttpRequestMessage();
                deductionTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                deductionTypesController = null;
                deductionTypesService = null;
                deductionTypeDtoList = null;
            }

            [TestMethod]
            public async Task DeductionType_GetAll_Async()
            {
                deductionTypesService.Setup(x => x.GetDeductionTypes2Async(It.IsAny<bool>())).ReturnsAsync(deductionTypeDtoList);

                var actuals = await deductionTypesController.GetAllDeductionTypes2Async();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task DeductionType_GetAll_TrueCache_Async()
            {
                deductionTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                deductionTypesController.Request.Headers.CacheControl.NoCache = true;

                deductionTypesService.Setup(x => x.GetDeductionTypes2Async(true)).ReturnsAsync(deductionTypeDtoList);

                var actuals = await deductionTypesController.GetAllDeductionTypes2Async();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task DeductionType_GetById_Async()
            {
                var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(deductionTypeGuid));

                deductionTypesService.Setup(x => x.GetDeductionTypeById2Async(deductionTypeGuid)).ReturnsAsync(expected);

                var actual = await deductionTypesController.GetDeductionTypeById2Async(deductionTypeGuid);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionType_GetAll_Exception()
            {
                deductionTypesService.Setup(x => x.GetDeductionTypes2Async(It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await deductionTypesController.GetAllDeductionTypes2Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionType_GetAById_Exception()
            {
                deductionTypesService.Setup(x => x.GetDeductionTypeById2Async(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actuals = await deductionTypesController.GetDeductionTypeById2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionType_GetAById_KeyNotFoundException()
            {
                deductionTypesService.Setup(x => x.GetDeductionTypeById2Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await deductionTypesController.GetDeductionTypeById2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionTypesController_PostThrowsIntAppiExc()
            {
                await deductionTypesController.PostDeductionType2Async(deductionTypeDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionTypesController_PutThrowsIntAppiExc()
            {
                var result = await deductionTypesController.PutDeductionType2Async(deductionTypeDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeductionTypesController_DeleteThrowsIntAppiExc()
            {
                await deductionTypesController.DeleteDeductionTypeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            private void BuildData()
            {
                deductionTypeDtoList = new List<DeductionType2>() 
                {
                    new DeductionType2(){Id = "625c69ff-280b-4ed3-9474-662a43616a8a", Code = "MAR", Description = null, Title = "Marriage"},
                    new DeductionType2(){Id = "bfea651b-8e27-4fcd-abe3-04573443c04c", Code = "BOC", Description = null, Title = "Birth of Child"},
                    new DeductionType2(){Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023", Code = "SJC", Description = null, Title = "Spouse Job Change"},
                    new DeductionType2(){Id = "e9e6837f-2c51-431b-9069-4ac4c0da3041", Code = "DIV", Description = null, Title = "Divorce"},
                    new DeductionType2(){Id = "80779c4f-b2ac-4ad4-a970-ca5699d9891f", Code = "ADP", Description = null, Title = "Adoption"},
                    new DeductionType2(){Id = "ae21110e-991e-405e-9d8b-47eeff210a2d", Code = "DEA", Description = null, Title = "Death"}
                };
            }
        }
    }
}