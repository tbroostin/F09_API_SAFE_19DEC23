//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
    public class StudentFinancialAidNeedSummariesControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IStudentFinancialAidNeedSummaryService> studentFinancialAidNeedSummaryServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            StudentFinancialAidNeedSummariesController studentFinancialAidNeedSummariesController;
            List<Dtos.StudentFinancialAidNeedSummary> studentFinancialAidNeedSummaryDtos;
            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                studentFinancialAidNeedSummaryServiceMock = new Mock<IStudentFinancialAidNeedSummaryService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                studentFinancialAidNeedSummaryDtos = BuildData();

                studentFinancialAidNeedSummariesController = new StudentFinancialAidNeedSummariesController(studentFinancialAidNeedSummaryServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                studentFinancialAidNeedSummariesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                studentFinancialAidNeedSummariesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private List<Dtos.StudentFinancialAidNeedSummary> BuildData()
            {
                List<Dtos.StudentFinancialAidNeedSummary> studentFinancialAidNeedSummaries = new List<Dtos.StudentFinancialAidNeedSummary>()
                {
                    new Dtos.StudentFinancialAidNeedSummary()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        AidYear = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.StudentFinancialAidNeedSummary()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        AidYear = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff")
                    },
                    new Dtos.StudentFinancialAidNeedSummary()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        AidYear = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.StudentFinancialAidNeedSummary()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        AidYear = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
                return studentFinancialAidNeedSummaries;
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentFinancialAidNeedSummariesController = null;
                studentFinancialAidNeedSummaryDtos = null;
                studentFinancialAidNeedSummaryServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummariesController_GetAll_NoCache_True()
            {
                studentFinancialAidNeedSummariesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 4);
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetAsync(offset, limit, true)).ReturnsAsync(tuple);
                var studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidNeedSummaries.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidNeedSummary> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidNeedSummary>;


                Assert.AreEqual(studentFinancialAidNeedSummaryDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidNeedSummaryDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    //Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    //Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummariesController_GetAll_NoCache_False()
            {
                studentFinancialAidNeedSummariesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 4);
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetAsync(offset, limit, false)).ReturnsAsync(tuple);
                var studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidNeedSummaries.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidNeedSummary> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidNeedSummary>;


                Assert.AreEqual(studentFinancialAidNeedSummaryDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidNeedSummaryDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    //Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    //Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummariesController_GetAll_NullPage()
            {
                studentFinancialAidNeedSummariesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 4);
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(tuple);
                var studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesAsync(null);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidNeedSummaries.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidNeedSummary> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidNeedSummary>;


                Assert.AreEqual(studentFinancialAidNeedSummaryDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidNeedSummaryDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    //Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    //Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummariesController_GetById()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var studentFinancialAidNeedSummary = studentFinancialAidNeedSummaryDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetByIdAsync(id)).ReturnsAsync(studentFinancialAidNeedSummary);

                var actual = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesByGuidAsync(id);

                var expected = studentFinancialAidNeedSummaryDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AidYear, actual.AidYear);
                //Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                //Assert.AreEqual(expected.Student, actual.Student);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetAll_PermissionException()
            {
                studentFinancialAidNeedSummariesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 4);
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new PermissionsException());
                var studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetAll_ArgumentException()
            {
                studentFinancialAidNeedSummariesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 4);
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new ArgumentException());
                var studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetAll_KeyNotFoundException()
            {
                studentFinancialAidNeedSummariesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 4);
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new KeyNotFoundException());
                var studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetAll_RepositoryException()
            {
                studentFinancialAidNeedSummariesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 4);
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new RepositoryException());
                var studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetAll_IntegerationApiException()
            {
                studentFinancialAidNeedSummariesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 4);
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new IntegrationApiException());
                var studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetAll_Exception()
            {
                studentFinancialAidNeedSummariesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 4);
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetAsync(offset, limit, false)).ThrowsAsync(new Exception());
                var studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetById_PermissionsException()
            {
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

                var actual = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesByGuidAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetById_RepositoryException()
            {
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                var actual = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesByGuidAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetById_ArgumentException()
            {
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var actual = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetById_IntegrationApiException()
            {
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

                var actual = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetById_Exception()
            {
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actual = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesByGuidAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_GetById_KeyNotFoundException()
            {
                studentFinancialAidNeedSummaryServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await studentFinancialAidNeedSummariesController.GetStudentFinancialAidNeedSummariesByGuidAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_PUT_Not_Supported()
            {
                var actual = await studentFinancialAidNeedSummariesController.UpdateAsync(It.IsAny<string>(), It.IsAny<Dtos.StudentFinancialAidNeedSummary>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_POST_Not_Supported()
            {
                var actual = await studentFinancialAidNeedSummariesController.CreateAsync(It.IsAny<Dtos.StudentFinancialAidNeedSummary>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidNeedSummariesController_DELETE_Not_Supported()
            {
                await studentFinancialAidNeedSummariesController.DeleteAsync(It.IsAny<string>());
            }
        }
    }
}