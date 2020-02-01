// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Planning
{
    [TestClass]
    public class AdvisorsControllerTests
    {
        [TestClass]
        public class AdvisorControllerGetAndSearch
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

            private AdvisorsController advisorsController;
            private Mock<IAdvisorService> AdvisorServiceMock;
            private IAdvisorService advisorService;
            private IAdapterRegistry adapterRegistry;
            ILogger logger = new Mock<ILogger>().Object;
            private Ellucian.Colleague.Dtos.Planning.Advisor advisor1 = null;
            private Ellucian.Colleague.Dtos.Planning.Advisor advisor2 = null;
            private Dtos.Planning.AdvisorQueryCriteria advisorQueryCriteria;

            private HttpResponse response;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                AdvisorServiceMock = new Mock<IAdvisorService>();
                advisorService = AdvisorServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Planning.Entities.Advisor, Advisor>(adapterRegistry, logger);
                adapterRegistry.AddAdapter(testAdapter);

                advisorsController = new AdvisorsController(advisorService, logger);
                Mapper.CreateMap<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>();

                // mock an advisor dto to return
                advisor1 = new Dtos.Planning.Advisor();
                advisor1.FirstName = "Sally";
                advisor1.LastName = "Smith";
                advisor1.MiddleName = "Margaret";
                advisor1.Id = "0000001";
                advisor2 = new Dtos.Planning.Advisor();
                advisor2.FirstName = "James";
                advisor2.LastName = "Blue";
                advisor2.EmailAddresses = new List<string>() { "jblue@xmail.com" };
                advisor2.Id = "0000002";
                AdvisorServiceMock.Setup(x => x.GetAdvisorAsync("0000001")).ReturnsAsync(advisor1);
                advisorQueryCriteria = new AdvisorQueryCriteria() { AdvisorIds = new List<string>() { "0000001", "0000002" } };
                AdvisorServiceMock.Setup(x => x.GetAdvisorsAsync(advisorQueryCriteria))
                    .ReturnsAsync(new List<Dtos.Planning.Advisor>() { advisor1, advisor2 });
                AdvisorServiceMock.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<string>() { "123", "123" });
                AdvisorServiceMock.Setup(x => x.SearchAsync("ERRORTEST", It.IsAny<int>(), It.IsAny<int>())).Throws(new Exception("some kind of error"));

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
            }

            [TestCleanup]
            public void Cleanup()
            {
                advisorsController = null;
                advisorService = null;
            }
            [TestMethod]
            public async Task GetAdvisor_ReturnsAdvisor_Success()
            {
                var adv = await advisorsController.GetAdvisorAsync("0000001");
                Assert.AreEqual("0000001", adv.Id);
                Assert.AreEqual("Smith", adv.LastName);
                Assert.AreEqual("Sally", adv.FirstName);
                Assert.AreEqual("Margaret", adv.MiddleName);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdvisor_InvalidAdvisor_Exception()
            {
                AdvisorServiceMock.Setup(x => x.GetAdvisorAsync("9999999")).ThrowsAsync(new Exception());
                var advisees = await advisorsController.GetAdvisorAsync("9999999");
            }

            [TestMethod]
            public async Task QueryAdvisors_ReturnsAdvisors_Success()
            {
                var advisorDtos = await advisorsController.QueryAdvisorsByPostAsync(advisorQueryCriteria);
                Assert.AreEqual(2, advisorDtos.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryAdvisors_PermissionsException()
            {
                AdvisorServiceMock.Setup(x => x.GetAdvisorsAsync(advisorQueryCriteria))
                    .ThrowsAsync(new PermissionsException());
                var advisorDtos = await advisorsController.QueryAdvisorsByPostAsync(advisorQueryCriteria);
            }
        }

        [TestClass]
        public class AdvisorsController_QueryAdvisorsByPostAsync
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

            private AdvisorsController advisorsController;
            private Mock<IAdvisorService> AdvisorServiceMock;
            private IAdvisorService advisorService;
            private IAdapterRegistry adapterRegistry;
            ILogger logger = new Mock<ILogger>().Object;
            private Ellucian.Colleague.Dtos.Planning.Advisor advisor1 = null;
            private Ellucian.Colleague.Dtos.Planning.Advisor advisor2 = null;
            private List<string> advisorIds;

            private HttpResponse response;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                AdvisorServiceMock = new Mock<IAdvisorService>();
                advisorService = AdvisorServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Planning.Entities.Advisor, Advisor>(adapterRegistry, logger);
                adapterRegistry.AddAdapter(testAdapter);

                advisorsController = new AdvisorsController(advisorService, logger);
                Mapper.CreateMap<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>();

                // mock an advisor dto to return
                advisor1 = new Dtos.Planning.Advisor();
                advisor1.FirstName = "Sally";
                advisor1.LastName = "Smith";
                advisor1.MiddleName = "Margaret";
                advisor1.Id = "0000001";
                advisor2 = new Dtos.Planning.Advisor();
                advisor2.FirstName = "James";
                advisor2.LastName = "Blue";
                advisor2.EmailAddresses = new List<string>() { "jblue@xmail.com" };
                advisor2.Id = "0000002";
                AdvisorServiceMock.Setup(x => x.GetAdvisorAsync("0000001")).ReturnsAsync(advisor1);
                advisorIds = new List<string>() { "0000001", "0000002" };
                AdvisorServiceMock.Setup(x => x.QueryAdvisorsByPostAsync(advisorIds))
                    .ReturnsAsync(new List<Dtos.Planning.Advisor>() { advisor1, advisor2 });
                AdvisorServiceMock.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<string>() { "123", "123" });
                AdvisorServiceMock.Setup(x => x.SearchAsync("ERRORTEST", It.IsAny<int>(), It.IsAny<int>())).Throws(new Exception("some kind of error"));

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
            }

            [TestCleanup]
            public void Cleanup()
            {
                advisorsController = null;
                advisorService = null;
            }

            [TestMethod]
            public async Task QueryAdvisorsByPost2Async_ReturnsAdvisors_Success()
            {
                var advisorDtos = await advisorsController.QueryAdvisorsByPost2Async(advisorIds);
                Assert.AreEqual(2, advisorDtos.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryAdvisorsByPost2Async_Null_AdvisorIds()
            {
                var advisorDtos = await advisorsController.QueryAdvisorsByPost2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryAdvisorsByPost2Async_empty_AdvisorIds()
            {
                var advisorDtos = await advisorsController.QueryAdvisorsByPost2Async(new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryAdvisorsByPost2Async_PermissionsException()
            {
                AdvisorServiceMock.Setup(x => x.QueryAdvisorsByPostAsync(advisorIds))
                    .ThrowsAsync(new PermissionsException());
                var advisorDtos = await advisorsController.QueryAdvisorsByPost2Async(advisorIds);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryAdvisorsByPost2Async_generic_Exception()
            {
                AdvisorServiceMock.Setup(x => x.QueryAdvisorsByPostAsync(advisorIds))
                    .ThrowsAsync(new ApplicationException());
                var advisorDtos = await advisorsController.QueryAdvisorsByPost2Async(advisorIds);
            }
        }

        [TestClass]
        public class AdvisorControllerGetAdvisees
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

            private AdvisorsController advisorsController;
            private Mock<IAdvisorService> AdvisorServiceMock;
            private IAdvisorService advisorService;
            ILogger logger = new Mock<ILogger>().Object;
            private List<Ellucian.Colleague.Dtos.Planning.Advisee> advisees = new List<Ellucian.Colleague.Dtos.Planning.Advisee>();

            private HttpResponse response;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                AdvisorServiceMock = new Mock<IAdvisorService>();
                advisorService = AdvisorServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();

                advisorsController = new AdvisorsController(advisorService, logger);
                Mapper.CreateMap<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>();

                // mock a list of advisee DTOs to return
                Ellucian.Colleague.Dtos.Planning.Advisee advisee1 = new Ellucian.Colleague.Dtos.Planning.Advisee();
                advisee1.DegreePlanId = 1;
                advisee1.Id = "1111111";
                advisee1.FirstName = "Sally";
                advisee1.MiddleName = "Margaret";
                advisee1.LastName = "Smith";
                advisee1.ApprovalRequested = true;
                advisees.Add(advisee1);
                Ellucian.Colleague.Dtos.Planning.Advisee advisee2 = new Ellucian.Colleague.Dtos.Planning.Advisee();
                advisee2.DegreePlanId = 1;
                advisee2.Id = "2222222";
                advisee2.FirstName = "Dick";
                advisee2.LastName = "Jones";
                advisee2.ApprovalRequested = false;
                advisees.Add(advisee2);

                var privacyWrapper = new PrivacyWrapper<List<Advisee>>(advisees, true);
                AdvisorServiceMock.Setup(x => x.GetAdviseesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
            }

            [TestCleanup]
            public void Cleanup()
            {
                advisorsController = null;
                advisorService = null;
            }
            [TestMethod]
            public async Task ReturnsAdvisees_Success()
            {
                var returnedAdvisees = await advisorsController.GetAdvisees2Async("0000001");
                Assert.AreEqual(2, returnedAdvisees.Count());
                Assert.AreEqual("1111111", returnedAdvisees.ElementAt(0).Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InvalidAdvisor_Exception()
            {
                AdvisorServiceMock.Setup(x => x.GetAdviseesAsync("9999999", 1, 1, false)).ThrowsAsync(new Exception());
                var returnedAdvisees = await advisorsController.GetAdvisees2Async("9999999", 1, 1);
            }

        }

        [TestClass]
        public class AdvisorsController_GetPermissionsAsync_Tests
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

            private AdvisorsController advisorsController;
            private Mock<IAdvisorService> advisorServiceMock;
            private IAdvisorService advisorService;
            private IAdapterRegistry adapterRegistry;
            ILogger logger = new Mock<ILogger>().Object;
            private List<string> dto;

            private HttpResponse response;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                advisorServiceMock = new Mock<IAdvisorService>();
                advisorService = advisorServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Planning.Entities.Advisor, Advisor>(adapterRegistry, logger);
                adapterRegistry.AddAdapter(testAdapter);

                advisorsController = new AdvisorsController(advisorService, logger);
                Mapper.CreateMap<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>();

                dto = new List<string>()
                {
                    "VIEW.ANY.ADVISEE"
                };

                // mock an AdvisingPermissions DTO to return
                advisorServiceMock.Setup(x => x.GetAdvisorPermissionsAsync()).ReturnsAsync(dto);

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
            }

            [TestCleanup]
            public void Cleanup()
            {
                advisorsController = null;
                advisorService = null;
            }

            [TestMethod]
            public async Task GetPermissionsAsync_Success()
            {
                var response = await advisorsController.GetPermissionsAsync();
                Assert.IsNotNull(response);
                Assert.IsInstanceOfType(response, typeof(IEnumerable<string>));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPermissionsAsync_Exception()
            {
                advisorServiceMock.Setup(x => x.GetAdvisorPermissionsAsync()).ThrowsAsync(new ApplicationException());
                var response = await advisorsController.GetPermissionsAsync();
            }

        }


        [TestClass]
        public class AdvisorsController_GetAdvisingPermissions2Async_Tests
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

            private AdvisorsController advisorsController;
            private Mock<IAdvisorService> advisorServiceMock;
            private IAdvisorService advisorService;
            private IAdapterRegistry adapterRegistry;
            private Mock<ILogger> loggerMock;
            ILogger logger;
            private AdvisingPermissions dto;

            private HttpResponse response;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                advisorServiceMock = new Mock<IAdvisorService>();
                advisorService = advisorServiceMock.Object;
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Planning.Entities.Advisor, Advisor>(adapterRegistry, logger);
                adapterRegistry.AddAdapter(testAdapter);

                advisorsController = new AdvisorsController(advisorService, logger);
                Mapper.CreateMap<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>();

                dto = new AdvisingPermissions()
                {
                    CanReviewAnyAdvisee = true,
                    CanReviewAssignedAdvisees = true,
                    CanUpdateAnyAdvisee = true,
                    CanUpdateAssignedAdvisees = true,
                    CanViewAnyAdvisee = true,
                    CanViewAssignedAdvisees = true,
                    HasFullAccessForAnyAdvisee = true,
                    HasFullAccessForAssignedAdvisees = true
                };

                // mock an AdvisingPermissions DTO to return
                advisorServiceMock.Setup(x => x.GetAdvisingPermissions2Async()).ReturnsAsync(dto);

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
            }

            [TestCleanup]
            public void Cleanup()
            {
                advisorsController = null;
                advisorService = null;
            }

            [TestMethod]
            public async Task GetAdvisingPermissions2Async_Success()
            {
                var response = await advisorsController.GetAdvisingPermissions2Async();
                Assert.IsNotNull(response);
                Assert.IsInstanceOfType(response, typeof(AdvisingPermissions));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAdvisingPermissions2Async_Exception()
            {
                ApplicationException thrownEx = new ApplicationException("Error lower in call stack.");
                advisorServiceMock.Setup(x => x.GetAdvisingPermissions2Async()).ThrowsAsync(thrownEx);
                var response = await advisorsController.GetAdvisingPermissions2Async();
                loggerMock.Verify(l => l.Error(thrownEx.ToString()));

            }

        }
    }
}
