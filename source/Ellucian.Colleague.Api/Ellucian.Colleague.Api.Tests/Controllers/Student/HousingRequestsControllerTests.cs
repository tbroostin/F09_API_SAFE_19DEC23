// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using HousingRequest = Ellucian.Colleague.Dtos.HousingRequest;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class HousingRequestsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IHousingRequestService> housingRequestServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            HousingRequestsController housingRequestsController;
            List<Dtos.HousingRequest> housingRequestDtos;
            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                housingRequestServiceMock = new Mock<IHousingRequestService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                housingRequestDtos = BuildData();

                housingRequestsController = new HousingRequestsController(housingRequestServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                housingRequestsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                housingRequestsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private List<Dtos.HousingRequest> BuildData()
            {
                List<Dtos.HousingRequest> housingRequests = new List<Dtos.HousingRequest>()
                {
                    new Dtos.HousingRequest()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        Person = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        StartOn = DateTimeOffset.Now,
                        EndOn = DateTimeOffset.Now,
                        Status = Dtos.EnumProperties.HousingRequestsStatus.Rejected,
                        //Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        //Owner = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.HousingRequest()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        Person = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        StartOn = DateTimeOffset.Now,
                        EndOn = DateTimeOffset.Now,
                        Status = Dtos.EnumProperties.HousingRequestsStatus.Rejected,
                        //Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        //Owner = new Dtos.GuidObject2("0cva17h3-er23-5796-cb9a-32f5tdh065yf")
                    },
                    new Dtos.HousingRequest()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Person = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        StartOn = DateTimeOffset.Now,
                        EndOn = DateTimeOffset.Now,
                        Status = Dtos.EnumProperties.HousingRequestsStatus.Rejected,
                        //Type = new Dtos.GuidObject2("b83022ee-ufhs-3idd-88b0-3837a050be4f"),
                        //Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.HousingRequest()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        Person = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        StartOn = DateTimeOffset.Now,
                        EndOn = DateTimeOffset.Now,
                        Status = Dtos.EnumProperties.HousingRequestsStatus.Rejected,
                        //Type = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"), 
                        //Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
                return housingRequests;
            }

            [TestCleanup]
            public void Cleanup()
            {
                housingRequestsController = null;
                housingRequestDtos = null;
                housingRequestServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task HousingRequestsController_GetAll_NoCache_True()
            {
                housingRequestsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.HousingRequest>, int>(housingRequestDtos, 4);
                housingRequestServiceMock.Setup(ci => ci.GetHousingRequestsAsync(offset, limit, true)).ReturnsAsync(tuple);
                var housingRequests = await housingRequestsController.GetHousingRequestsAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await housingRequests.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.HousingRequest> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.HousingRequest>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.HousingRequest>;


                Assert.AreEqual(housingRequestDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Person, actual.Person);
                    //Assert.AreEqual(expected.Type, actual.Type);
                    //Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task HousingRequestsController_GetAll_NoCache_False()
            {
                housingRequestsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.HousingRequest>, int>(housingRequestDtos, 4);
                housingRequestServiceMock.Setup(ci => ci.GetHousingRequestsAsync(offset, limit, false)).ReturnsAsync(tuple);
                var housingRequests = await housingRequestsController.GetHousingRequestsAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await housingRequests.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.HousingRequest> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.HousingRequest>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.HousingRequest>;


                Assert.AreEqual(housingRequestDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Person, actual.Person);
                    //Assert.AreEqual(expected.Type, actual.Type);
                    //Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            //[TestMethod]
            //public async Task HousingRequestsController_GetAll_NullPage()
            //{
            //    housingRequestsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            //    {
            //        NoCache = true,
            //        Public = true
            //    };
            //    var tuple = new Tuple<IEnumerable<Dtos.HousingRequest>, int>(housingRequestDtos, 4);
            //    housingRequestServiceMock.Setup(ci => ci.GetHousingRequestsAsync(offset, limit, true)).ReturnsAsync(tuple);
            //    var housingRequests = await housingRequestsController.GetHousingRequestsAsync(null);

            //    var cancelToken = new System.Threading.CancellationToken(false);

            //    System.Net.Http.HttpResponseMessage httpResponseMessage = await housingRequests.ExecuteAsync(cancelToken);

            //    IEnumerable<Dtos.HousingRequest> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.HousingRequest>>)httpResponseMessage.Content)
            //                                                    .Value as IEnumerable<Dtos.HousingRequest>;


            //    Assert.AreEqual(housingRequestDtos.Count, actuals.Count());

            //    foreach (var actual in actuals)
            //    {
            //        var expected = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

            //        Assert.IsNotNull(expected);
            //        Assert.AreEqual(expected.Id, actual.Id);
            //        Assert.AreEqual(expected.Student, actual.Student);
            //        //Assert.AreEqual(expected.Type, actual.Type);
            //        //Assert.AreEqual(expected.Owner, actual.Owner);
            //    }
            //}

            [TestMethod]
            public async Task HousingRequestsController_GetById()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var housingRequest = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                housingRequestServiceMock.Setup(ci => ci.GetHousingRequestByGuidAsync(id, It.IsAny<bool>())).ReturnsAsync(housingRequest);

                var actual = await housingRequestsController.GetHousingRequestByGuidAsync(id);

                var expected = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person, actual.Person);
                //Assert.AreEqual(expected.Type, actual.Type);
                //Assert.AreEqual(expected.Owner, actual.Owner);
            }

            //[TestMethod]
            //public async Task HousingRequestsController_PUT()
            //{
            //    var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
            //    var housingRequest = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            //    housingRequestServiceMock.Setup(ci => ci.UpdateHousingRequestAsync(id, housingRequest)).ReturnsAsync(housingRequest);

            //    var actual = await housingRequestsController.PutHousingRequestAsync(id, housingRequest);

            //    var expected = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

            //    Assert.IsNotNull(expected);
            //    Assert.AreEqual(expected.Id, actual.Id);
            //    Assert.AreEqual(expected.Person, actual.Person);
            //    //Assert.AreEqual(expected.Type, actual.Type);
            //    //Assert.AreEqual(expected.Owner, actual.Owner);
            //}

            //[TestMethod]
            //public async Task HousingRequestsController_POST()
            //{
            //    var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
            //    var housingRequest = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            //    housingRequestServiceMock.Setup(ci => ci.CreateHousingRequestAsync(housingRequest)).ReturnsAsync(housingRequest);

            //    var actual = await housingRequestsController.PostHousingRequestAsync(housingRequest);

            //    var expected = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

            //    Assert.IsNotNull(expected);
            //    Assert.AreEqual(expected.Id, actual.Id);
            //    Assert.AreEqual(expected.Person, actual.Person);
            //    //Assert.AreEqual(expected.Type, actual.Type);
            //    //Assert.AreEqual(expected.Owner, actual.Owner);
            //}

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetAll_Exception()
            {
                housingRequestsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.HousingRequest>, int>(housingRequestDtos, 4);
                housingRequestServiceMock.Setup(ci => ci.GetHousingRequestsAsync(offset, limit, false)).ThrowsAsync(new Exception());
                var housingRequests = await housingRequestsController.GetHousingRequestsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetById_Exception()
            {
                housingRequestServiceMock.Setup(ci => ci.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actual = await housingRequestsController.GetHousingRequestByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetById_KeyNotFoundException()
            {
                housingRequestServiceMock.Setup(ci => ci.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await housingRequestsController.GetHousingRequestByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_PUT_Not_Supported()
            {
                var actual = await housingRequestsController.PutHousingRequestAsync(It.IsAny<string>(), It.IsAny<Dtos.HousingRequest>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_POST_Not_Supported()
            {
                var actual = await housingRequestsController.PostHousingRequestAsync(It.IsAny<Dtos.HousingRequest>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_DELETE_Not_Supported()
            {
                await housingRequestsController.DeleteHousingRequestsAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequests_KeyNotFoundException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestsAsync(offset, limit, It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await housingRequestsController.GetHousingRequestsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequests_PermissionsException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestsAsync(offset, limit, It.IsAny<bool>())).Throws<PermissionsException>();
                await housingRequestsController.GetHousingRequestsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequests_ArgumentException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestsAsync(offset, limit, It.IsAny<bool>())).Throws<ArgumentException>();
                await housingRequestsController.GetHousingRequestsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequests_RepositoryException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestsAsync(offset, limit, It.IsAny<bool>())).Throws<RepositoryException>();
                await housingRequestsController.GetHousingRequestsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequests_IntegrationApiException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestsAsync(offset, limit, It.IsAny<bool>())).Throws<IntegrationApiException>();
                await housingRequestsController.GetHousingRequestsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequestsByGuidAsync_KeyNotFoundException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await housingRequestsController.GetHousingRequestByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequestsByGuidAsync_ArgumentNullException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentNullException>();
                await housingRequestsController.GetHousingRequestByGuidAsync("1234");
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequestsByGuidAsync_PermissionsException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                await housingRequestsController.GetHousingRequestByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequestsByGuidAsync_ArgumentException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();
                await housingRequestsController.GetHousingRequestByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequestsByGuidAsync_RepositoryException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
                await housingRequestsController.GetHousingRequestByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequestsByGuidAsync_IntegrationApiException()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
                await housingRequestsController.GetHousingRequestByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingRequestsController_GetHousingRequestsByGuidAsync_NoId_Exception()
            {
                housingRequestServiceMock.Setup(x => x.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
                await housingRequestsController.GetHousingRequestByGuidAsync("");
            }
        }

        [TestClass]
        public class HousingRequestsControllerTests_V11
        {
            [TestClass]
            public class HosuingRequestsControllerTests_GET
            {
                #region DECLARATIONS

                public TestContext TestContext { get; set; }

                private HousingRequestsController housingRequestsController;
                private Mock<IHousingRequestService> housingRequestServiceMock;
                private Mock<ILogger> loggerMock;
                private IEnumerable<Dtos.HousingRequest> housingRequestCollection;
                private Tuple<IEnumerable<Dtos.HousingRequest>, int> housingRequestTuple;

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                    LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                    EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                    housingRequestServiceMock = new Mock<IHousingRequestService>();
                    loggerMock = new Mock<ILogger>();

                    housingRequestsController = new HousingRequestsController(housingRequestServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                    housingRequestsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                    housingRequestsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                    InitializeTestData();
                }

                [TestCleanup]
                public void Cleanup()
                {
                    housingRequestsController = null;
                    housingRequestServiceMock = null;
                    loggerMock = null;
                    TestContext = null;
                }

                private void InitializeTestData()
                {
                    housingRequestCollection = new List<Dtos.HousingRequest>()
                    {
                        new HousingRequest() { Id = "2a082180-b897-46f3-8435-df25caaca922" },
                        new HousingRequest() { Id = "2a082180-b897-46f3-8435-df25caaca923" },
                        new HousingRequest() { Id = "2a082180-b897-46f3-8435-df25caaca924" }
                    };

                    housingRequestTuple = new Tuple<IEnumerable<HousingRequest>, int>(housingRequestCollection, 3);
                }

                #endregion

                #region CACHE-NOCACHE

                [TestMethod]
                public async Task HousingRequestsController_GetHousingRequestsAsync_Nocache()
                {
                    housingRequestsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false, Public = true };

                    housingRequestServiceMock.Setup(x => x.GetHousingRequestsAsync(0, 10, false)).ReturnsAsync(housingRequestTuple);

                    var results = await housingRequestsController.GetHousingRequestsAsync(new Paging(10, 0));

                    Assert.IsNotNull(results);

                    var cancelToken = new CancellationToken(false);

                    HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                    IEnumerable<Dtos.HousingRequest> actuals =
                        ((ObjectContent<IEnumerable<Dtos.HousingRequest>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.HousingRequest>;

                    Assert.AreEqual(housingRequestCollection.Count(), actuals.Count());

                    foreach (var actual in actuals)
                    {
                        var expected = housingRequestCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                        Assert.IsNotNull(expected);
                        Assert.AreEqual(expected.Id, actual.Id);
                    }
                }

                [TestMethod]
                public async Task HousingRequestsController_GetHousingRequestsAsync_Cache()
                {
                    housingRequestsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

                    housingRequestServiceMock.Setup(x => x.GetHousingRequestsAsync(0, 10, true)).ReturnsAsync(housingRequestTuple);

                    var results = await housingRequestsController.GetHousingRequestsAsync(new Paging(10, 0));

                    Assert.IsNotNull(results);

                    var cancelToken = new CancellationToken(false);

                    HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                    IEnumerable<Dtos.HousingRequest> actuals =
                        ((ObjectContent<IEnumerable<Dtos.HousingRequest>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.HousingRequest>;

                    Assert.AreEqual(housingRequestCollection.Count(), actuals.Count());

                    foreach (var actual in actuals)
                    {
                        var expected = housingRequestCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                        Assert.IsNotNull(expected);
                        Assert.AreEqual(expected.Id, actual.Id);
                    }
                }

                #endregion

                #region GETALL

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestsAsync_KeyNotFoundException()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                    await housingRequestsController.GetHousingRequestsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestsAsync_PermissionsException()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    await housingRequestsController.GetHousingRequestsAsync(null);
                }


                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestsAsync_ArgumentException()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                    await housingRequestsController.GetHousingRequestsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestsAsync_RepositoryException()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                    await housingRequestsController.GetHousingRequestsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestsAsync_IntegrationApiException()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                    await housingRequestsController.GetHousingRequestsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestsAsync_ArgumentNullException()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                    await housingRequestsController.GetHousingRequestsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestsAsync_Exception()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                    await housingRequestsController.GetHousingRequestsAsync(null);
                }

                #endregion

                #region GETBYID

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestByGuidAsync_KeyNotFoundException()
                {
                    housingRequestsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false, Public = true };
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                    await housingRequestsController.GetHousingRequestByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestByGuidAsync_PermissionsException()
                {
                    housingRequestsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    await housingRequestsController.GetHousingRequestByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestByGuidAsync_ArgumentException()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                    await housingRequestsController.GetHousingRequestByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestByGuidAsync_RepositoryException()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                    await housingRequestsController.GetHousingRequestByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestByGuidAsync_IntegrationApiException()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                    await housingRequestsController.GetHousingRequestByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_GetHousingRequestByGuidAsync_Exception()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                    await housingRequestsController.GetHousingRequestByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                public async Task HousingRequestsController_GetHousingRequestByGuidAsync()
                {
                    housingRequestServiceMock.Setup(e => e.GetHousingRequestByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(housingRequestCollection.FirstOrDefault());
                    var result = await housingRequestsController.GetHousingRequestByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Id, housingRequestCollection.FirstOrDefault().Id);
                }

                #endregion
            }

            [TestClass]
            public class HosuingRequestsControllerTests_POST
            {
                #region DECLARATIONS

                public TestContext TestContext { get; set; }

                private HousingRequestsController housingRequestsController;
                private Mock<IHousingRequestService> housingRequestServiceMock;
                private Mock<ILogger> loggerMock;
                private Dtos.HousingRequest housingRequest;

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                    LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                    EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                    housingRequestServiceMock = new Mock<IHousingRequestService>();
                    loggerMock = new Mock<ILogger>();

                    housingRequestsController = new HousingRequestsController(housingRequestServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                    housingRequestsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                    housingRequestsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                    InitializeTestData();
                }

                [TestCleanup]
                public void Cleanup()
                {
                    housingRequestsController = null;
                    housingRequestServiceMock = null;
                    loggerMock = null;
                    TestContext = null;
                }

                private void InitializeTestData()
                {
                    housingRequest = new HousingRequest()
                    {
                        Id = Guid.Empty.ToString(),
                        StartOn = DateTime.Today,
                        Status = Dtos.EnumProperties.HousingRequestsStatus.Submitted,
                        Person = new Dtos.GuidObject2("2a082180-b897-46f3-8435-df25caaca920"),
                        RoomCharacteristics = new List<HousingPreferenceRequiredProperty>()
                        {
                            new HousingPreferenceRequiredProperty()
                            {
                                Preferred = new Dtos.GuidObject2("3a082180-b897-46f3-8435-df25caaca920"),
                                Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                            }
                        },
                        FloorCharacteristics = new HousingPreferenceRequiredProperty()
                        {
                            Preferred = new Dtos.GuidObject2("4a082180-b897-46f3-8435-df25caaca920"),
                            Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                        },
                        RoommatePreferences = new List<HousingRequestRoommatePreferenceProperty>()
                        {
                            new HousingRequestRoommatePreferenceProperty()
                            {
                                Roommate = new HousingPreferenceRequiredProperty()
                                { Preferred = new Dtos.GuidObject2("3a082180-b897-46f3-8435-df25caaca920"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory }
                            }
                        }
                    };
                }

                #endregion

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_Request_Body()
                {
                    await housingRequestsController.PostHousingRequestAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_Request_Id_Null()
                {
                    await housingRequestsController.PostHousingRequestAsync(new HousingRequest() { Id = null});
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_Request_With_Person_Null()
                {
                    housingRequest.Person = null;
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_Request_With_PersonId_Null()
                {
                    housingRequest.Person = new Dtos.GuidObject2();
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_Request_With_StartOn_Null()
                {
                    housingRequest.StartOn = null;
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_Request_With_InvalidStatus()
                {
                    housingRequest.Status = Dtos.EnumProperties.HousingRequestsStatus.NotSet;
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_StartOn_GreaterThan_EndOn()
                {
                    housingRequest.EndOn = DateTime.Today.AddDays(-10);
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_Status_As_Approved()
                {
                    housingRequest.Status = Dtos.EnumProperties.HousingRequestsStatus.Approved;
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PostHousingRequestAsync_RoomCharacterstics_PerferredId_As_Null()
                {
                    housingRequest.RoomCharacteristics.FirstOrDefault().Preferred = new Dtos.GuidObject2();
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PostHousingRequestAsync_RoomCharacterstics_Required_As_NotSet()
                {
                    housingRequest.RoomCharacteristics.FirstOrDefault().Required = Dtos.EnumProperties.RequiredPreference.NotSet;
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PostHousingRequestAsync_FloorCharacteristics_Preferred_As_Null()
                {
                    housingRequest.FloorCharacteristics.Preferred = null;
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PostHousingRequestAsync_FloorCharacteristics_PreferredId_As_Null()
                {
                    housingRequest.FloorCharacteristics.Preferred = new Dtos.GuidObject2();
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PostHousingRequestAsync_FloorCharacteristics_Required_As_NotSet()
                {
                    housingRequest.FloorCharacteristics.Required = Dtos.EnumProperties.RequiredPreference.NotSet;
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PostHousingRequestAsync_RoommatePreferences_PreferredId_As_Null()
                {
                    housingRequest.RoommatePreferences.FirstOrDefault().Roommate.Preferred = new Dtos.GuidObject2();
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PostHousingRequestAsync_RoommatePreferences_Required_As_NotSet()
                {
                    housingRequest.RoommatePreferences.FirstOrDefault().Roommate.Required = Dtos.EnumProperties.RequiredPreference.NotSet;
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_ArgumentNullException()
                {
                    housingRequestServiceMock.Setup(h => h.CreateHousingRequestAsync(It.IsAny<HousingRequest>())).ThrowsAsync(new ArgumentNullException());
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_ArgumentException()
                {
                    housingRequestServiceMock.Setup(h => h.CreateHousingRequestAsync(It.IsAny<HousingRequest>())).ThrowsAsync(new ArgumentException());
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_KeyNotFoundException()
                {
                    housingRequestServiceMock.Setup(h => h.CreateHousingRequestAsync(It.IsAny<HousingRequest>())).ThrowsAsync(new KeyNotFoundException());
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_InvalidOperationException()
                {
                    housingRequestServiceMock.Setup(h => h.CreateHousingRequestAsync(It.IsAny<HousingRequest>())).ThrowsAsync(new InvalidOperationException());
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_PermissionsException()
                {
                    housingRequestServiceMock.Setup(h => h.CreateHousingRequestAsync(It.IsAny<HousingRequest>())).ThrowsAsync(new PermissionsException());
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PostHousingRequestAsync_Exception()
                {
                    housingRequestServiceMock.Setup(h => h.CreateHousingRequestAsync(It.IsAny<HousingRequest>())).ThrowsAsync(new Exception());
                    await housingRequestsController.PostHousingRequestAsync(housingRequest);
                }

                [TestMethod]
                public async Task HousingRequestsController_PostHousingRequestAsync()
                {
                    housingRequestServiceMock.Setup(h => h.CreateHousingRequestAsync(It.IsAny<HousingRequest>())).ReturnsAsync(housingRequest);
                    var result = await housingRequestsController.PostHousingRequestAsync(housingRequest);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Id, housingRequest.Id);
                }
            }

            [TestClass]
            public class HosuingRequestsControllerTests_PUT
            {
                #region DECLARATIONS

                public TestContext TestContext { get; set; }

                private HousingRequestsController housingRequestsController;
                private Mock<IHousingRequestService> housingRequestServiceMock;
                private Mock<ILogger> loggerMock;
                private Dtos.HousingRequest housingRequest;

                private string guid = "1a082180-b897-46f3-8435-df25caaca920";

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                    LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                    EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                    housingRequestServiceMock = new Mock<IHousingRequestService>();
                    loggerMock = new Mock<ILogger>();

                    housingRequestsController = new HousingRequestsController(housingRequestServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                    housingRequestsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                    housingRequestsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                    InitializeTestData();

                    housingRequestsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(housingRequest));
                }

                [TestCleanup]
                public void Cleanup()
                {
                    housingRequestsController = null;
                    housingRequestServiceMock = null;
                    loggerMock = null;
                    TestContext = null;
                }

                private void InitializeTestData()
                {
                    housingRequest = new HousingRequest()
                    {
                        Id = "1a082180-b897-46f3-8435-df25caaca920",
                        StartOn = DateTime.Today,
                        Status = Dtos.EnumProperties.HousingRequestsStatus.Submitted,
                        Person = new Dtos.GuidObject2("2a082180-b897-46f3-8435-df25caaca920"),
                        RoomCharacteristics = new List<HousingPreferenceRequiredProperty>()
                        {
                            new HousingPreferenceRequiredProperty()
                            {
                                Preferred = new Dtos.GuidObject2("3a082180-b897-46f3-8435-df25caaca920"),
                                Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                            }
                        },
                        FloorCharacteristics = new HousingPreferenceRequiredProperty()
                        {
                            Preferred = new Dtos.GuidObject2("4a082180-b897-46f3-8435-df25caaca920"),
                            Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                        },
                        RoommatePreferences = new List<HousingRequestRoommatePreferenceProperty>()
                        {
                            new HousingRequestRoommatePreferenceProperty()
                            {
                                Roommate = new HousingPreferenceRequiredProperty()
                                { Preferred = new Dtos.GuidObject2("3a082180-b897-46f3-8435-df25caaca920"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory }
                            }
                        }
                    };
                }

                #endregion

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_Request_guid_Null()
                {
                    await housingRequestsController.PutHousingRequestAsync(null, new HousingRequest());
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_Request_Dto_Null()
                {
                    await housingRequestsController.PutHousingRequestAsync(guid, null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_Request_DtoId_Null()
                {
                    housingRequest.Id = null;
                    await housingRequestsController.PutHousingRequestAsync(guid, housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_Guid_As_Empty()
                {
                    await housingRequestsController.PutHousingRequestAsync(Guid.Empty.ToString(), housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_Guid_And_DtoId_NotSame()
                {
                    housingRequest.Id = Guid.NewGuid().ToString();
                    await housingRequestsController.PutHousingRequestAsync(guid, housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_ArgumentNullException()
                {
                    housingRequestServiceMock.Setup(h => h.UpdateHousingRequestAsync(It.IsAny<string>(), It.IsAny<HousingRequest>())).ThrowsAsync(new ArgumentNullException());
                    await housingRequestsController.PutHousingRequestAsync(guid, housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_ArgumentException()
                {
                    housingRequestServiceMock.Setup(h => h.UpdateHousingRequestAsync(It.IsAny<string>(), It.IsAny<HousingRequest>())).ThrowsAsync(new ArgumentException());
                    await housingRequestsController.PutHousingRequestAsync(guid, housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_KeyNotFoundException()
                {
                    housingRequestServiceMock.Setup(h => h.UpdateHousingRequestAsync(It.IsAny<string>(), It.IsAny<HousingRequest>())).ThrowsAsync(new KeyNotFoundException());
                    await housingRequestsController.PutHousingRequestAsync(guid, housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_InvalidOperationException()
                {
                    housingRequestServiceMock.Setup(h => h.UpdateHousingRequestAsync(It.IsAny<string>(), It.IsAny<HousingRequest>())).ThrowsAsync(new InvalidOperationException());
                    await housingRequestsController.PutHousingRequestAsync(guid, housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_PermissionsException()
                {
                    housingRequestServiceMock.Setup(h => h.UpdateHousingRequestAsync(It.IsAny<string>(), It.IsAny<HousingRequest>())).ThrowsAsync(new PermissionsException());
                    await housingRequestsController.PutHousingRequestAsync(guid, housingRequest);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task HousingRequestsController_PutHousingRequestAsync_Exception()
                {
                    housingRequestServiceMock.Setup(h => h.UpdateHousingRequestAsync(It.IsAny<string>(), It.IsAny<HousingRequest>())).ThrowsAsync(new Exception());
                    await housingRequestsController.PutHousingRequestAsync(guid, housingRequest);
                }

                [TestMethod]
                public async Task HousingRequestsController_PutHousingRequestAsync()
                {
                    housingRequestServiceMock.Setup(h => h.GetHousingRequestByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(housingRequest);
                    housingRequestServiceMock.Setup(h => h.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
                    housingRequestServiceMock.Setup(h => h.UpdateHousingRequestAsync(It.IsAny<string>(), It.IsAny<HousingRequest>())).ReturnsAsync(housingRequest);

                    var result = await housingRequestsController.PutHousingRequestAsync(guid, housingRequest);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Id, housingRequest.Id);
                }
            }
        }
    }
}
