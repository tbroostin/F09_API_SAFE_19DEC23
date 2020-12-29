//Copyright 2018-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class FixedAssetsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFixedAssetsService> fixedAssetsServiceMock;
        private Mock<ILogger> loggerMock;
        private FixedAssetsController fixedAssetsController;
        private List<Dtos.FixedAssets> fixedAssetsCollection;
        private List<Colleague.Dtos.ColleagueFinance.FixedAssetsFlag> fixedAssetFlags;
        private Tuple<IEnumerable<Dtos.FixedAssets>, int> fixedAssetsCollectionTuple;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        int offset = 0;
        int limit = 2;
        private string guid = "83f78f38-cb00-403b-a107-557dabf0f451";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            fixedAssetsServiceMock = new Mock<IFixedAssetsService>();
            loggerMock = new Mock<ILogger>();
            fixedAssetsCollection = new List<Dtos.FixedAssets>();

            BuildData();

            fixedAssetsController = new FixedAssetsController(fixedAssetsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            fixedAssetsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            fixedAssetsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            fixedAssetsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(fixedAssetsCollection.FirstOrDefault()));
        }

        private void BuildData()
        {
            fixedAssetsCollection = new List<FixedAssets>()
            {
                new FixedAssets()
                {
                    Id = expectedGuid,
                    Description = "Fixed assets description",
                    Tag = "01",
                    Type = new GuidObject2("c89c9a38-e007-4d3e-83ee-59bc804e6bc0"),
                    Category = new GuidObject2("6c4670aa-e775-4f81-bfd9-18ce21463d5d"),
                    CapitalizationStatus = Dtos.EnumProperties.FixedAssetsCapitalizationStatus.Capitalized,
                    AcquisitionMethod = Dtos.EnumProperties.FixedAssetsAcquisitionMethod.Donation,
                    Building = new GuidObject2("cbe7c380-22ca-4380-b37b-1f6261d67177"),
                    Room = new GuidObject2("a1402bd0-d0a5-4ce1-ab13-fe7e259e97b9"),
                    AcquisitionCost = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                        Value = 8000                                      
                    },
                    AccumulatedDepreciation = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                        Value = 898
                    },
                    DepreciationMethod = "Straight Line",
                    SalvageValue = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                        Value = 7000
                    },
                    UsefulLife = 3,
                    DepreciationExpenseAccount = new GuidObject2("915a2d80-6e70-49b3-bccd-d80dd7fe6b08"),
                    RenewalCost = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                        Value = 700
                    },
                },
                new FixedAssets()
                {
                    Id = "0e86c3ce-9423-41a8-aba4-b038078da80c",
                    Description = "Fixed assets description",
                    Tag = "01",
                    Type = new GuidObject2("c89c9a38-e007-4d3e-83ee-59bc804e6bc0"),
                    Category = new GuidObject2("6c4670aa-e775-4f81-bfd9-18ce21463d5d"),
                    CapitalizationStatus = Dtos.EnumProperties.FixedAssetsCapitalizationStatus.Capitalized,
                    AcquisitionMethod = Dtos.EnumProperties.FixedAssetsAcquisitionMethod.Donation,
                    Building = new GuidObject2("cbe7c380-22ca-4380-b37b-1f6261d67177"),
                    Room = new GuidObject2("a1402bd0-d0a5-4ce1-ab13-fe7e259e97b9"),
                    AcquisitionCost = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                        Value = 8000
                    },
                    AccumulatedDepreciation = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                        Value = 898
                    },
                    DepreciationMethod = "Straight Line",
                    SalvageValue = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                        Value = 7000
                    },
                    UsefulLife = 3,
                    DepreciationExpenseAccount = new GuidObject2("915a2d80-6e70-49b3-bccd-d80dd7fe6b08"),
                    RenewalCost = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                        Value = 700
                    },
                }
            };

            fixedAssetFlags = new List<Dtos.ColleagueFinance.FixedAssetsFlag>()
            {
                new Dtos.ColleagueFinance.FixedAssetsFlag { Code = "S", Description = "Single" }, new Dtos.ColleagueFinance.FixedAssetsFlag { Code = "M", Description = "Multi-valued" }
            };

            fixedAssetsCollectionTuple = new Tuple<IEnumerable<FixedAssets>, int>(fixedAssetsCollection, fixedAssetsCollection.Count);
            fixedAssetsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            //fixedAssetsServiceMock.Setup(s => s.DoesUpdateViolateDataPrivacySettings(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).ReturnsAsync(true);
            fixedAssetsServiceMock.Setup(s => s.GetFixedAssetsByGuidAsync(It.IsAny<string>(), true)).ReturnsAsync(fixedAssetsCollection.FirstOrDefault());
            fixedAssetsServiceMock.Setup(s => s.GetFixedAssetTransferFlagsAsync()).ReturnsAsync(fixedAssetFlags);

        }

        [TestCleanup]
        public void Cleanup()
        {
            fixedAssetsController = null;
            fixedAssetsCollection = null;
            loggerMock = null;
            fixedAssetsServiceMock = null;
        }

        #region GET & GETALL

        [TestMethod]
        public async Task FixedAssetsController_GetFixedAssets_ValidateFields_Nocache()
        {
            fixedAssetsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsAsync(offset, limit, It.IsAny<bool>())).ReturnsAsync(fixedAssetsCollectionTuple);

            var fixedAssets = await fixedAssetsController.GetFixedAssetsAsync(new Web.Http.Models.Paging(limit, offset));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await fixedAssets.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.FixedAssets>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.FixedAssets>;

            Assert.AreEqual(fixedAssetsCollection.Count, actuals.Count());            
        }

        [TestMethod]
        public async Task FixedAssetsController_GetFixedAssets_ValidateFields_Cache()
        {
            fixedAssetsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsAsync(offset, limit, It.IsAny<bool>())).ReturnsAsync(fixedAssetsCollectionTuple);

            var fixedAssets = await fixedAssetsController.GetFixedAssetsAsync(new Web.Http.Models.Paging(limit, offset));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await fixedAssets.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Dtos.FixedAssets>>)httpResponseMessage.Content)
                                .Value as IEnumerable<Dtos.FixedAssets>;
            Assert.IsNotNull(actuals);

            Assert.AreEqual(fixedAssetsCollection.Count, actuals.Count());
        }

        [TestMethod]
        public async Task FixedAssetsController_GetFixedAssetsByGuidAsync_ValidateFields()
        {
            fixedAssetsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = fixedAssetsCollection.FirstOrDefault();
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsByGuidAsync(expected.Id, false)).ReturnsAsync(expected);

            var actual = await fixedAssetsController.GetFixedAssetsByGuidAsync(expected.Id);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Tag, actual.Tag);
            Assert.AreEqual(expected.Category, actual.Category);
            Assert.AreEqual(expected.CapitalizationStatus, actual.CapitalizationStatus);
            Assert.AreEqual(expected.AcquisitionMethod, actual.AcquisitionMethod);
            Assert.AreEqual(expected.Building, actual.Building);
            Assert.AreEqual(expected.Room, actual.Room);
            Assert.AreEqual(expected.AcquisitionCost, actual.AcquisitionCost);
            Assert.AreEqual(expected.AccumulatedDepreciation, actual.AccumulatedDepreciation);
            Assert.AreEqual(expected.DepreciationMethod, actual.DepreciationMethod);
            Assert.AreEqual(expected.SalvageValue, actual.SalvageValue);
            Assert.AreEqual(expected.UsefulLife, actual.UsefulLife);
            Assert.AreEqual(expected.DepreciationExpenseAccount, actual.DepreciationExpenseAccount);
            Assert.AreEqual(expected.RenewalCost, actual.RenewalCost);            
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssets_KeyNotFoundException()
        {            
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await fixedAssetsController.GetFixedAssetsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssets_PermissionsException()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await fixedAssetsController.GetFixedAssetsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssets_ArgumentException()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await fixedAssetsController.GetFixedAssetsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssets_RepositoryException()
        {

            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await fixedAssetsController.GetFixedAssetsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssets_IntegrationApiException()
        {

            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await fixedAssetsController.GetFixedAssetsAsync(It.IsAny<Web.Http.Models.Paging>());
        }       

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssets_Exception()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<Exception>();

            await fixedAssetsController.GetFixedAssetsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsByGuidAsync_Exception()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await fixedAssetsController.GetFixedAssetsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsByGuid_KeyNotFoundException()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await fixedAssetsController.GetFixedAssetsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsByGuid_PermissionsException()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await fixedAssetsController.GetFixedAssetsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsByGuid_ArgumentException()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await fixedAssetsController.GetFixedAssetsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsByGuid_RepositoryException()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await fixedAssetsController.GetFixedAssetsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsByGuid_IntegrationApiException()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await fixedAssetsController.GetFixedAssetsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsByGuid_Exception()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await fixedAssetsController.GetFixedAssetsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsAsync_Id_Null()
        {
            await fixedAssetsController.GetFixedAssetsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsAsync_ArgumentNullException()
        {
            fixedAssetsServiceMock.Setup(r => r.GetFixedAssetsAsync(offset, limit, It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
            await fixedAssetsController.GetFixedAssetsAsync(null);
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsAsync_Exception()
        {
            fixedAssetsServiceMock.Setup(r => r.GetFixedAssetsAsync(offset, limit, It.IsAny<bool>())).ThrowsAsync(new Exception());
            await fixedAssetsController.GetFixedAssetsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsAsync_KeyNotFoundException()
        {
            fixedAssetsServiceMock.Setup(r => r.GetFixedAssetsAsync(offset, limit, It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            await fixedAssetsController.GetFixedAssetsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetsAsync_ApplicationException()
        {
            fixedAssetsServiceMock.Setup(r => r.GetFixedAssetsAsync(offset, limit, It.IsAny<bool>())).ThrowsAsync(new ApplicationException());
            await fixedAssetsController.GetFixedAssetsAsync(null);
        }


        [TestMethod]
        public async Task FixedAssetsController_GetFixedAssetTransferFlagsAsync()
        {

            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetTransferFlagsAsync()).ReturnsAsync(fixedAssetFlags);

            var fxaFlags = (await fixedAssetsController.GetFixedAssetTransferFlagsAsync()).ToList();
            Assert.AreEqual(fixedAssetFlags.Count, fxaFlags.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetTransferFlagsAsync_KeyNotFoundException()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetTransferFlagsAsync())
                .Throws<KeyNotFoundException>();
            await fixedAssetsController.GetFixedAssetTransferFlagsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetTransferFlagsAsync_PermissionsException()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await fixedAssetsController.GetFixedAssetsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetsController_GetFixedAssetTransferFlagsAsync_Exception()
        {
            fixedAssetsServiceMock.Setup(x => x.GetFixedAssetTransferFlagsAsync())
                .Throws<Exception>();

            await fixedAssetsController.GetFixedAssetTransferFlagsAsync();
        }
        #endregion

    }
}