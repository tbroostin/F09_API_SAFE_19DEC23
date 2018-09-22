//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentFinancialAidApplicationsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IStudentFinancialAidApplicationService> financialAidApplicationService2Mock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            StudentFinancialAidApplicationsController financialAidApplicationsController;
            List<Dtos.FinancialAidApplication> financialAidApplicationDtos;
            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                financialAidApplicationService2Mock = new Mock<IStudentFinancialAidApplicationService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                financialAidApplicationDtos = BuildData();

                financialAidApplicationsController = new StudentFinancialAidApplicationsController(adapterRegistryMock.Object, financialAidApplicationService2Mock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                financialAidApplicationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                financialAidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private List<Dtos.FinancialAidApplication> BuildData()
            {
                List<Dtos.FinancialAidApplication> financialAidApplications = new List<Dtos.FinancialAidApplication>()
                {
                    new Dtos.FinancialAidApplication()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        AidYear = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.FinancialAidApplication()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        AidYear = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff")
                    },
                    new Dtos.FinancialAidApplication()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        AidYear = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.FinancialAidApplication()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        AidYear = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
                return financialAidApplications;
            }

            [TestCleanup]
            public void Cleanup()
            {
                financialAidApplicationsController = null;
                financialAidApplicationDtos = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task FinancialAidApplicationsController_GetAll_NoCache_True()
            {
                financialAidApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(financialAidApplicationDtos, 4);
                financialAidApplicationService2Mock.Setup(ci => ci.GetAsync(offset, limit, true)).ReturnsAsync(tuple);
                var financialAidApplications = await financialAidApplicationsController.GetAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await financialAidApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.FinancialAidApplication> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.FinancialAidApplication>;


                Assert.AreEqual(financialAidApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = financialAidApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                }
            }

            [TestMethod]
            public async Task FinancialAidApplicationsController_GetAll_NoCache_False()
            {
                financialAidApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(financialAidApplicationDtos, 4);
                financialAidApplicationService2Mock.Setup(ci => ci.GetAsync(offset, limit, false)).ReturnsAsync(tuple);
                var financialAidApplications = await financialAidApplicationsController.GetAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await financialAidApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.FinancialAidApplication> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.FinancialAidApplication>;


                Assert.AreEqual(financialAidApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = financialAidApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                }
            }

            [TestMethod]
            public async Task FinancialAidApplicationsController_GetAll_NullPage()
            {
                financialAidApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(financialAidApplicationDtos, 4);
                financialAidApplicationService2Mock.Setup(ci => ci.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(tuple);
                var financialAidApplications = await financialAidApplicationsController.GetAsync(null);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await financialAidApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.FinancialAidApplication> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.FinancialAidApplication>;


                Assert.AreEqual(financialAidApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = financialAidApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                }
            }

            [TestMethod]
            public async Task FinancialAidApplicationsController_GetById()
            {
                financialAidApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true
                };
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var financialAidApplication = financialAidApplicationDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                financialAidApplicationService2Mock.Setup(ci => ci.GetByIdAsync(id)).ReturnsAsync(financialAidApplication);

                var actual = await financialAidApplicationsController.GetByIdAsync(id);

                var expected = financialAidApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AidYear, actual.AidYear);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetAll_KeyNotFoundException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new KeyNotFoundException());
                var financialAidApplications = await financialAidApplicationsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetAll_PermissionException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new PermissionsException());
                var financialAidApplications = await financialAidApplicationsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetAll_ArgumentException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new ArgumentException());
                var financialAidApplications = await financialAidApplicationsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetAll_RepositoryException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new RepositoryException());
                var financialAidApplications = await financialAidApplicationsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetAll_IntegrationApiException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new IntegrationApiException());
                var financialAidApplications = await financialAidApplicationsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetAll_Exception()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new Exception());
                var financialAidApplications = await financialAidApplicationsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetById_NullOrEmptyId()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actual = await financialAidApplicationsController.GetByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetById_KeyNotFoundException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await financialAidApplicationsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetById_PermissionsException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

                var actual = await financialAidApplicationsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetById_ArgumentException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var actual = await financialAidApplicationsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetById_RepositoryException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                var actual = await financialAidApplicationsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetById_IntegrationApiException()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

                var actual = await financialAidApplicationsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_GetById_Exception()
            {
                financialAidApplicationService2Mock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actual = await financialAidApplicationsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_PUT_Not_Supported()
            {
                var actual = await financialAidApplicationsController.UpdateAsync(It.IsAny<string>(), It.IsAny<Dtos.FinancialAidApplication>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_POST_Not_Supported()
            {
                var actual = await financialAidApplicationsController.CreateAsync(It.IsAny<Dtos.FinancialAidApplication>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FinancialAidApplicationsController_DELETE_Not_Supported()
            {
                await financialAidApplicationsController.DeleteAsync(It.IsAny<string>());
            }
        }

    }
}
