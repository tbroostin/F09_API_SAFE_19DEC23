// Copyright 2020 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class EthosApiBuilderControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEthosApiBuilderService> ethosApiBuilderServiceMock;
        private Mock<ILogger> loggerMock;

        private EthosApiBuilderController ethosApiBuilderController;

        private List<Dtos.EthosApiBuilder> ethosApiBuilderCollection;
        private Web.Http.EthosExtend.EthosApiConfiguration ethosApiConfiguration;
        private Web.Http.EthosExtend.EthosExtensibleData ethosExtensibleData;

        private const string ethosApiBuilderSubjectAreaGuid = "a830e686-7692-4012-8da5-b1b5d44389b4"; //BU

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            ethosApiBuilderServiceMock = new Mock<IEthosApiBuilderService>();
            loggerMock = new Mock<ILogger>();

            ethosApiBuilderCollection = new List<Dtos.EthosApiBuilder>();
            var testConfigurationRepository = new TestConfigurationRepository();

            var ethosApiConfigurationEntity = testConfigurationRepository.GetEthosApiConfigurationByResource("x-person-health", false).GetAwaiter().GetResult();

            ethosApiConfiguration = new Web.Http.EthosExtend.EthosApiConfiguration()
            {
                ResourceName = ethosApiConfigurationEntity.ResourceName,
                PrimaryEntity = ethosApiConfigurationEntity.PrimaryEntity,
                PrimaryApplication = ethosApiConfigurationEntity.PrimaryApplication,
                PrimaryTableName = ethosApiConfigurationEntity.PrimaryTableName,
                PrimaryGuidSource = ethosApiConfigurationEntity.PrimaryGuidSource,
                PrimaryGuidDbType = ethosApiConfigurationEntity.PrimaryGuidDbType,
                PrimaryGuidFileName = ethosApiConfigurationEntity.PrimaryGuidFileName,
                PageLimit = ethosApiConfigurationEntity.PageLimit,
                SelectFileName = ethosApiConfigurationEntity.SelectFileName,
                SelectSubroutineName = ethosApiConfigurationEntity.SelectSubroutineName,
                SavingField = ethosApiConfigurationEntity.SavingField,
                SavingOption = ethosApiConfigurationEntity.SavingOption,
                SelectColumnName = ethosApiConfigurationEntity.SelectColumnName,
                SelectRules = ethosApiConfigurationEntity.SelectRules,
                SelectParagraph = ethosApiConfigurationEntity.SelectParagraph,
                HttpMethods = new List<Web.Http.EthosExtend.EthosApiSupportedMethods>(),
                SelectionCriteria = new List<Web.Http.EthosExtend.EthosApiSelectCriteria>(),
                SortColumns = new List<Web.Http.EthosExtend.EthosApiSortCriteria>()
            };
            foreach (var method in ethosApiConfigurationEntity.HttpMethods)
            {
                ethosApiConfiguration.HttpMethods.Add(new Web.Http.EthosExtend.EthosApiSupportedMethods(method.Method, method.Permission));
            }
            foreach (var select in ethosApiConfigurationEntity.SelectionCriteria)
            {
                ethosApiConfiguration.SelectionCriteria.Add(
                    new Web.Http.EthosExtend.EthosApiSelectCriteria(select.SelectConnector,
                    select.SelectColumn,
                    select.SelectOper,
                    select.SelectValue)
                );
            }
            foreach (var sort in ethosApiConfigurationEntity.SortColumns)
            {
                ethosApiConfiguration.SortColumns.Add(new Web.Http.EthosExtend.EthosApiSortCriteria(sort.SortColumn, sort.SortSequence));
            }

            var ethosExtensibleDataEntity = testConfigurationRepository.GetExtendedEthosDataByResource("x-person-health", "1.0.0", "141", new List<string>() { "1" }, true, false).GetAwaiter().GetResult().FirstOrDefault();
            ethosExtensibleData = new Web.Http.EthosExtend.EthosExtensibleData()
            {
                ApiResourceName = ethosExtensibleDataEntity.ApiResourceName,
                ApiVersionNumber = ethosExtensibleDataEntity.ApiVersionNumber,
                ColleagueTimeZone = ethosExtensibleDataEntity.ColleagueTimeZone,
                ResourceId = ethosExtensibleDataEntity.ResourceId,
                ExtendedSchemaType = ethosExtensibleDataEntity.ExtendedSchemaType
            };

            var ethosApiBuilder = new Dtos.EthosApiBuilder
            {
                Id = ethosExtensibleDataEntity.ResourceId
            };
            ethosApiBuilderCollection.Add(ethosApiBuilder);

            var expected = ethosApiBuilderCollection.FirstOrDefault();

            ethosApiBuilderController = new EthosApiBuilderController(ethosApiBuilderServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            ethosApiBuilderController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            ethosApiBuilderController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost/x-person-health") };
            ethosApiBuilderController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(expected));
            ethosApiBuilderController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false, Public = true };
            ethosApiBuilderController.Request.Headers.Add("Accept", "application/vnd.hedtech.integration.v1+json");

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiConfigurationByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<bool>())).ReturnsAsync(ethosApiConfiguration);
            ethosApiBuilderServiceMock.Setup(x => x.GetExtendedEthosConfigurationByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<bool>())).ReturnsAsync(ethosExtensibleData);
        }

        [TestCleanup]
        public void Cleanup()
        {
            ethosApiBuilderController = null;
            ethosApiBuilderCollection = null;
            loggerMock = null;
            ethosApiBuilderServiceMock = null;
        }

        #region EthosApiBuilder

        [TestMethod]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_GetAll()
        {
            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            var expectedCollection = new Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>(ethosApiBuilderCollection, Limit);
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).ReturnsAsync(expectedCollection);

            Paging paging = new Paging(Limit, offset);
            var ethosApiBuilder = (await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", paging));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await ethosApiBuilder.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.EthosApiBuilder> results = ((ObjectContent<IEnumerable<Dtos.EthosApiBuilder>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.EthosApiBuilder>;

            Assert.IsNotNull(results);
            Assert.AreEqual(Limit, results.Count());

            foreach (var actual in results)
            {
                var expected = ethosApiBuilderCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_String_ArgumentNullException()
        {
            await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_PermissionsException()
        {
            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<PermissionsException>();
            await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_ArgumentException()
        {
            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<ArgumentException>();
            await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_RepositoryException()
        {

            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<RepositoryException>();
            await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_IntegrationApiException()
        {

            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<IntegrationApiException>();
            await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_Exception()
        {

            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<Exception>();
            await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        }

        #endregion GetEthosApiBuilder

        #region GetEthosApiBuilderByGuid

        [TestMethod]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).ReturnsAsync(expected);

            var result = (await ethosApiBuilderController.GetEthosApiBuilderByIdAsync("x-person-health", "1"));
            Assert.AreEqual(expected.Id, result.Id, "Id");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_NullArgument()
        {
            await ethosApiBuilderController.GetEthosApiBuilderByIdAsync("x-person-health", null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_EmptyArgument()
        {
            await ethosApiBuilderController.GetEthosApiBuilderByIdAsync("x-person-health", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_PermissionsException()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<PermissionsException>();
            
            await ethosApiBuilderController.GetEthosApiBuilderByIdAsync("x-person-health", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_ArgumentException()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<ArgumentException>();
            
            await ethosApiBuilderController.GetEthosApiBuilderByIdAsync("x-person-health", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_RepositoryException()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<RepositoryException>();
            
            await ethosApiBuilderController.GetEthosApiBuilderByIdAsync("x-person-health", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_IntegrationApiException()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<IntegrationApiException>();
            
            await ethosApiBuilderController.GetEthosApiBuilderByIdAsync("x-person-health", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_Exception()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<Exception>();
            
            await ethosApiBuilderController.GetEthosApiBuilderByIdAsync("x-person-health", "1");
        }

        #endregion GetEthosApiBuilderByGuid

        //#region Put

        //[TestMethod]
        //public async Task EthosApiBuilderController_PutEthosApiBuilder()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PutCommentAsync(expected.Id, It.IsAny<Dtos.EthosApiBuilder>())).ReturnsAsync(expected);
        //    ethosApiBuilderServiceMock.Setup(x => x.GetCommentByIdAsync(expected.Id)).ReturnsAsync(expected);

        //    var actual = (await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>()));
        //    Assert.AreEqual(expected.Id, actual.Id, "Id");
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PutEthosApiBuilder_NullArgument()
        //{
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PutEthosApiBuilder_EmptyArgument()
        //{
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PutEthosApiBuilder_PermissionsException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<PermissionsException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());

        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PutEthosApiBuilder_ArgumentException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<ArgumentException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PutEthosApiBuilder_RepositoryException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<RepositoryException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PutEthosApiBuilder_IntegrationApiException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<IntegrationApiException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PutEthosApiBuilder_ConfigurationException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<ConfigurationException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PutEthosApiBuilder_Exception()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<Exception>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}
        //#endregion

        //#region Post

        //[TestMethod]
        //public async Task EthosApiBuilderController_PostEthosApiBuilder()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PostCommentAsync(expected)).ReturnsAsync(expected);

        //    var actual = (await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>()));
        //    Assert.AreEqual(expected.Id, actual.Id, "Id");
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PostEthosApiBuilder_NullArgument()
        //{
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PostEthosApiBuilder_PermissionsException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<PermissionsException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());

        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PostEthosApiBuilder_ArgumentException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<ArgumentException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PostEthosApiBuilder_RepositoryException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<RepositoryException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PostEthosApiBuilder_IntegrationApiException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<IntegrationApiException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PostEthosApiBuilder_ConfigurationException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<ConfigurationException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_PostEthosApiBuilder_Exception()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<Exception>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}
        //#endregion

        //#region Delete

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_DeleteEthosApiBuilder_EmptyArgument()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_DeleteEthosApiBuilder_PermissionsException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<PermissionsException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_DeleteEthosApiBuilder_ArgumentException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<ArgumentException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_DeleteEthosApiBuilder_RepositoryException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<RepositoryException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_DeleteEthosApiBuilder_IntegrationApiException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<IntegrationApiException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_DeleteEthosApiBuilder_ConfigurationException()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<ConfigurationException>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task EthosApiBuilderController_DeleteEthosApiBuilder_Exception()
        //{
        //    var expected = ethosApiBuilderCollection.FirstOrDefault();
        //    ethosApiBuilderServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<Exception>();
        //    await ethosApiBuilderController.GetEthosApiBuilderAsync("x-person-health", It.IsAny<Paging>());
        //}
        //#endregion
    }
}