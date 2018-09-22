// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdmissionApplicationSupportingItemsControllerTests_V12
    {
        [TestClass]
        public class AdmissionApplicationSupportingItemsControllerTests_GET_And_GET_ALL
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private AdmissionApplicationSupportingItemsController admissionApplicationSupportingItemsController;
            private Mock<IAdmissionApplicationSupportingItemsService> admissionApplicationSupportingItemsServiceMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<AdmissionApplicationSupportingItems> admissionApplicationSupportingItemsCollection;
            private Tuple<IEnumerable<AdmissionApplicationSupportingItems>, int> admissionApplicationSupportingItemsTuple;

            String guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                admissionApplicationSupportingItemsServiceMock = new Mock<IAdmissionApplicationSupportingItemsService>();
                loggerMock = new Mock<ILogger>();

                admissionApplicationSupportingItemsController = new AdmissionApplicationSupportingItemsController(admissionApplicationSupportingItemsServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                admissionApplicationSupportingItemsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                admissionApplicationSupportingItemsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                InitializeTestData();
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationSupportingItemsController = null;
                admissionApplicationSupportingItemsServiceMock = null;
                loggerMock = null;
                TestContext = null;
            }

            private void InitializeTestData()
            {
                admissionApplicationSupportingItemsCollection = new List<AdmissionApplicationSupportingItems>()
                {
                    new AdmissionApplicationSupportingItems()
                    {
                        Id = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                        Application = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e871"),
                        Status = new AdmissionApplicationSupportingItemsStatus()
                        {
                            Detail = new GuidObject2("2a49eed8-5fe7-4120-b1cf-f23266b9e871"),
                            Type = Dtos.EnumProperties.AdmissionApplicationSupportingItemsType.Received
                        }

                    },
                    new AdmissionApplicationSupportingItems()
                    {
                        Id = "1b49eed8-5fe7-4120-b1cf-f23266b9e874",
                        Application = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e872"),
                        Status = new AdmissionApplicationSupportingItemsStatus()
                        {
                            Detail = new GuidObject2("2a49eed8-5fe7-4120-b1cf-f23266b9e872"),
                            Type = Dtos.EnumProperties.AdmissionApplicationSupportingItemsType.Waived
                        }
                    }
                };

                admissionApplicationSupportingItemsTuple = new Tuple<IEnumerable<AdmissionApplicationSupportingItems>, int>(admissionApplicationSupportingItemsCollection, admissionApplicationSupportingItemsCollection.Count());
            }

            #endregion

            #region CACHE-NOCACHE

            [TestMethod]
            public async Task AdmissionApplicationSupportingItemsController_Get_All_ValidateFields_Nocache()
            {
                admissionApplicationSupportingItemsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false, Public = true };

                admissionApplicationSupportingItemsServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemsAsync(0, 10, false)).ReturnsAsync(admissionApplicationSupportingItemsTuple);

                var results = await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsAsync(new Paging(10, 0));

                Assert.IsNotNull(results);

                var cancelToken = new CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                IEnumerable<AdmissionApplicationSupportingItems> actuals =
                    ((ObjectContent<IEnumerable<AdmissionApplicationSupportingItems>>)httpResponseMessage.Content).Value as IEnumerable<AdmissionApplicationSupportingItems>;

                Assert.AreEqual(admissionApplicationSupportingItemsCollection.Count(), actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationSupportingItemsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Status.Type, actual.Status.Type);
                    Assert.AreEqual(expected.Status.Detail.Id, actual.Status.Detail.Id);
                    Assert.AreEqual(expected.Application.Id, actual.Application.Id);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationSupportingItemsController_Get_All_ValidateFields_Cache()
            {
                admissionApplicationSupportingItemsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

                admissionApplicationSupportingItemsServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemsAsync(0, 10, true)).ReturnsAsync(admissionApplicationSupportingItemsTuple);

                var results = await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsAsync(new Paging(10, 0));

                Assert.IsNotNull(results);

                var cancelToken = new CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                IEnumerable<AdmissionApplicationSupportingItems> actuals =
                    ((ObjectContent<IEnumerable<AdmissionApplicationSupportingItems>>)httpResponseMessage.Content).Value as IEnumerable<AdmissionApplicationSupportingItems>;

                Assert.AreEqual(admissionApplicationSupportingItemsCollection.Count(), actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationSupportingItemsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Status.Type, actual.Status.Type);
                    Assert.AreEqual(expected.Status.Detail.Id, actual.Status.Detail.Id);
                    Assert.AreEqual(expected.Application.Id, actual.Application.Id);
                }
            }

            #endregion

            #region GETALL

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsAsync_keyNotFoundException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsAsync_PermissionsException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsAsync(new Paging(0, 100));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsAsync_RepositoryException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsAsync(new Paging(0, 100));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsAsync_IntegrationApiException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsAsync(new Paging(0, 100));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsAsync_Exception()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsAsync(new Paging(0, 100));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsAsync_ArgumentException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsAsync(new Paging(0, 100));
            }

            [TestMethod]
            public async Task AdmsApplSuprtngItmsController_GetAdmissionApplicationSupportingItemsAsync()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).
                    ReturnsAsync(admissionApplicationSupportingItemsTuple);

                var results = await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsAsync(new Paging(10, 100));

                Assert.IsNotNull(results);

                var cancelToken = new CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                IEnumerable<AdmissionApplicationSupportingItems> actuals =
                    ((ObjectContent<IEnumerable<AdmissionApplicationSupportingItems>>)httpResponseMessage.Content).Value as IEnumerable<AdmissionApplicationSupportingItems>;

                Assert.AreEqual(admissionApplicationSupportingItemsCollection.Count(), actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationSupportingItemsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Status.Type, actual.Status.Type);
                    Assert.AreEqual(expected.Status.Detail.Id, actual.Status.Detail.Id);
                    Assert.AreEqual(expected.Application.Id, actual.Application.Id);
                }
            }

            #endregion

            #region GETBYID

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_Guid_As_Null()
            {
                admissionApplicationSupportingItemsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_KeyNotFoundException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_PermissionsException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_ArgumentException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_RepositoryException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_IntegrationApiException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_Exception()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
            }

            [TestMethod]
            public async Task AdmsApplSuprtngItmsController_GetAdmissionApplicationSupportingItemsByGuidAsync()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(admissionApplicationSupportingItemsCollection.FirstOrDefault());

                var actual = await admissionApplicationSupportingItemsController.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);

                Assert.IsNotNull(actual);

                var expected = admissionApplicationSupportingItemsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Status.Type, actual.Status.Type);
                Assert.AreEqual(expected.Status.Detail.Id, actual.Status.Detail.Id);
                Assert.AreEqual(expected.Application.Id, actual.Application.Id);
            }

            #endregion

            #region UNSUPPORTED

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_UnSupported_Exception()
            {
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(new AdmissionApplicationSupportingItems() { });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeleteAdmissionApplicationSupportingItemsAsync_UnSupported_Exception()
            {
                await admissionApplicationSupportingItemsController.DeleteAdmissionApplicationSupportingItemsAsync(guid);
            }

            #endregion
        }

        [TestClass]
        public class AdmissionApplicationSupportingItemsControllerTests_POST_And_PUT
        {
            #region DECLARATIONS
            public TestContext TestContext { get; set; }

            private AdmissionApplicationSupportingItemsController admissionApplicationSupportingItemsController;
            private Mock<IAdmissionApplicationSupportingItemsService> admissionApplicationSupportingItemsServiceMock;
            private Mock<ILogger> loggerMock;
            private Dtos.AdmissionApplicationSupportingItems admissionApplicationSupportingItems;
            string guid = "11182180-b897-46f3-8435-df25caaca920"; 
            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                admissionApplicationSupportingItemsServiceMock = new Mock<IAdmissionApplicationSupportingItemsService>();
                loggerMock = new Mock<ILogger>();

                admissionApplicationSupportingItemsController = new AdmissionApplicationSupportingItemsController(admissionApplicationSupportingItemsServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                admissionApplicationSupportingItemsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                admissionApplicationSupportingItemsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                InitializeTestData();
                admissionApplicationSupportingItemsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(admissionApplicationSupportingItems));
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationSupportingItemsController = null;
                admissionApplicationSupportingItemsServiceMock = null;
                loggerMock = null;
                TestContext = null;
            }
            private void InitializeTestData()
            {
                admissionApplicationSupportingItems = new AdmissionApplicationSupportingItems() {
                    Id = Guid.Empty.ToString(), //"11182180-b897-46f3-8435-df25caaca920",
                    Application = new GuidObject2("1a082180-b897-46f3-8435-df25caaca920"),
                    AssignedOn = DateTime.Now, ExternalReference = "ref_001",
                    ReceivedOn = DateTime.Now,
                    Required = new Dtos.EnumProperties.AdmissionApplicationSupportingItemsRequired() { },
                    RequiredByDate = DateTime.Now,
                    Status = new AdmissionApplicationSupportingItemsStatus() { Detail = new GuidObject2("1a082180-b897-46f3-8435-df25caaca921"), Type = Dtos.EnumProperties.AdmissionApplicationSupportingItemsType.Received },
                    Type = new GuidObject2("1a082180-b897-46f3-8435-df25caaca923")
                };
            }
            #endregion

            #region POST

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Request_Body()
            {
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Id_Null()
            {
                admissionApplicationSupportingItems.Id = null;
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Id_EmptyGUID()
            {
                admissionApplicationSupportingItems.Id = "1a082180-b897-46f3-8435-df25caaca920";
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Throws_KeyNotFoundException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(x=>x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new KeyNotFoundException());
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Throws_PermissionsException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new PermissionsException());
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Throws_ArgumentException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new ArgumentException());
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Throws_RepositoryException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new RepositoryException());
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Throws_IntegrationApiException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new IntegrationApiException());
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Throws_ConfigurationException()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new ConfigurationException());
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationSupportingItemsAsync_Throws_Exception()
            {
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new Exception());
                await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            public async Task PostAdmissionApplicationSupportingItemsAsync()
            {
                admissionApplicationSupportingItems.Id = Guid.Empty.ToString();
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ReturnsAsync(admissionApplicationSupportingItems);
                var result = await admissionApplicationSupportingItemsController.PostAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplicationSupportingItems.Id);
            }


            #endregion

            #region PUT

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_Id_Null()
            {
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(null,admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_When_Object_IsNull()
            {
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid,null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_When_Guid_empty()
            {
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(Guid.Empty.ToString(), admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_With_Different_Id()
            {
                admissionApplicationSupportingItems.Id = "2a082180-b897-46f3-8435-df25caaca921";
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid, admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_PermissionsException()
            {
                admissionApplicationSupportingItems.Id = "11182180-b897-46f3-8435-df25caaca920";
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new PermissionsException());
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid, admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_ArgumentException()
            {
                admissionApplicationSupportingItems.Id = "11182180-b897-46f3-8435-df25caaca920";
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new ArgumentException());
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid, admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_RepositoryException()
            {
                admissionApplicationSupportingItems.Id = "11182180-b897-46f3-8435-df25caaca920";
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new RepositoryException());
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid, admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_IntegrationApiException()
            {
                admissionApplicationSupportingItems.Id = "11182180-b897-46f3-8435-df25caaca920";
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new IntegrationApiException());
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid, admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_ConfigurationException()
            {
                admissionApplicationSupportingItems.Id = "11182180-b897-46f3-8435-df25caaca920";
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new ConfigurationException());
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid, admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_KeyNotFoundException()
            {
                admissionApplicationSupportingItems.Id = "11182180-b897-46f3-8435-df25caaca920";
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new KeyNotFoundException());
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid, admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationSupportingItemsAsync_Exception()
            {
                admissionApplicationSupportingItems.Id = "11182180-b897-46f3-8435-df25caaca920";
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ThrowsAsync(new Exception());
                await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid, admissionApplicationSupportingItems);
            }

            [TestMethod]
            public async Task PutAdmissionApplicationSupportingItemsAsync()
            {
                admissionApplicationSupportingItems.Id = "11182180-b897-46f3-8435-df25caaca920";

                admissionApplicationSupportingItemsServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>(), true)).ReturnsAsync(admissionApplicationSupportingItems);
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.GetDataPrivacyListByApi(It.IsAny<string>(), true)).ReturnsAsync(new List<string>());
                admissionApplicationSupportingItemsServiceMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItems>())).ReturnsAsync(admissionApplicationSupportingItems);
                
                var result = await admissionApplicationSupportingItemsController.PutAdmissionApplicationSupportingItemsAsync(guid,admissionApplicationSupportingItems);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplicationSupportingItems.Id);
            }

            #endregion


        }

    }
}
