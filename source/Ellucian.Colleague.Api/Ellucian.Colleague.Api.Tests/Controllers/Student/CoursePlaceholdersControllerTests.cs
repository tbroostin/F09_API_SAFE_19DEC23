// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CoursePlaceholdersControllerTests
    {
        [TestClass]
        public class CoursePlaceholdersControllerTestsGetAsync
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

            private CoursePlaceholdersController CoursePlaceholdersController;
            private Mock<ICoursePlaceholderService> coursePlaceholderServiceMock;
            private ICoursePlaceholderService coursePlaceholderService;
            ILogger logger = new Mock<ILogger>().Object;

            private List<CoursePlaceholder> cachedCoursePlaceholdersDtos;
            private List<CoursePlaceholder> nonCachedCoursePlaceholdersDtos;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                coursePlaceholderServiceMock = new Mock<ICoursePlaceholderService>();
                coursePlaceholderService = coursePlaceholderServiceMock.Object;

                cachedCoursePlaceholdersDtos = new List<CoursePlaceholder>()
                {
                    new CoursePlaceholder()
                    {
                        CreditInformation = "3 to 5 credits",
                        Description = "Placeholder",
                        EndDate = DateTime.Today.AddDays(7),
                        Id = "1",
                        StartDate = DateTime.Today.AddDays(-7),
                        Title = "Placeholder Title"
                    },
                    new CoursePlaceholder()
                    {
                        CreditInformation = "4 to 6 credits",
                        Description = "Placeholder 2",
                        EndDate = DateTime.Today.AddDays(5),
                        Id = "2",
                        StartDate = DateTime.Today.AddDays(-6),
                        Title = "Placeholder Title 2"
                    }                
                };
                nonCachedCoursePlaceholdersDtos = new List<CoursePlaceholder>()
                {
                    new CoursePlaceholder()
                    {
                        CreditInformation = "3 to 6 credits",
                        Description = "Placeholder Noncached",
                        EndDate = DateTime.Today.AddDays(17),
                        Id = "1",
                        StartDate = DateTime.Today.AddDays(-17),
                        Title = "Placeholder Title NonCached"
                    },
                    new CoursePlaceholder()
                    {
                        CreditInformation = "4 to 5 credits",
                        Description = "Placeholder Noncached 2",
                        EndDate = DateTime.Today.AddDays(9),
                        Id = "2",
                        StartDate = DateTime.Today.AddDays(-9),
                        Title = "Placeholder Title 2 Noncached"
                    }
                };

                CoursePlaceholdersController = new CoursePlaceholdersController(coursePlaceholderService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                CoursePlaceholdersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                CoursePlaceholdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                coursePlaceholderServiceMock.Setup(x => x.GetCoursePlaceholdersByIdsAsync(It.IsAny<IEnumerable<string>>(), false)).ReturnsAsync(cachedCoursePlaceholdersDtos);
                coursePlaceholderServiceMock.Setup(x => x.GetCoursePlaceholdersByIdsAsync(It.IsAny<IEnumerable<string>>(), true)).ReturnsAsync(nonCachedCoursePlaceholdersDtos);
            }

            [TestCleanup]
            public void Cleanup()
            {
                CoursePlaceholdersController = null;
                coursePlaceholderService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryCoursePlaceholdersByIdsAsync_null_CoursePlaceholderIds()
            {
                var cphs = await CoursePlaceholdersController.QueryCoursePlaceholdersByIdsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryCoursePlaceholdersByIdsAsync_empty_CoursePlaceholderIds()
            {
                var cphs = await CoursePlaceholdersController.QueryCoursePlaceholdersByIdsAsync(new List<string>());
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryCoursePlaceholdersByIdsAsync_KeyNotFoundException_thrown_by_coordination_service()
            {
                coursePlaceholderServiceMock.Setup(x => x.GetCoursePlaceholdersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var cphs = await CoursePlaceholdersController.QueryCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryCoursePlaceholdersByIdsAsync_Exception_thrown_by_coordination_service()
            {
                coursePlaceholderServiceMock.Setup(x => x.GetCoursePlaceholdersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentOutOfRangeException());
                var cphs = await CoursePlaceholdersController.QueryCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2" });
            }

            [TestMethod]
            public async Task QueryCoursePlaceholdersByIdsAsync_no_cache_returns_correct_data()
            {
                CoursePlaceholdersController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var cphs = await CoursePlaceholdersController.QueryCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2" });
                Assert.AreEqual(nonCachedCoursePlaceholdersDtos.Count, cphs.Count());
                for(var i = 0; i < nonCachedCoursePlaceholdersDtos.Count; i++)
                {
                    var expected = nonCachedCoursePlaceholdersDtos.ElementAt(i);
                    var actual = cphs.ElementAt(i);
                    Assert.AreEqual(expected.CreditInformation, actual.CreditInformation);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.EndDate, actual.EndDate);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.StartDate, actual.StartDate);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task QueryCoursePlaceholdersByIdsAsync_cache_returns_correct_data()
            {
                CoursePlaceholdersController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var cphs = await CoursePlaceholdersController.QueryCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2" });
                Assert.AreEqual(cachedCoursePlaceholdersDtos.Count, cphs.Count());
                for (var i = 0; i < cachedCoursePlaceholdersDtos.Count; i++)
                {
                    var expected = cachedCoursePlaceholdersDtos.ElementAt(i);
                    var actual = cphs.ElementAt(i);
                    Assert.AreEqual(expected.CreditInformation, actual.CreditInformation);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.EndDate, actual.EndDate);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.StartDate, actual.StartDate);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }
        }
    }
}