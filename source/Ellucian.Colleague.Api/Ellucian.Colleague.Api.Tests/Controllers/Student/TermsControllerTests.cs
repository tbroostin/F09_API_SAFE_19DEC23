// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class TermsControllerTests
    {
        [TestClass]
        public class TermsControllerAllTerms
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

            private TermsController TermsController;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> allTerms;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                logger = new Mock<ILogger>().Object;
                allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allTerms));

                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Term>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Term>()).Returns(adapter);
                var registrationDateAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationDate, RegistrationDate>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationDate, RegistrationDate>()).Returns(registrationDateAdapter);
                TermsController = new TermsController(adapterRegistry, termRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                TermsController = null;
                termRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAllTermsAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    termRepoMock.Setup(x => x.GetAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                    await TermsController.GetAllTermsAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAllTermsAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    termRepoMock.Setup(x => x.GetAsync()).Throws(new ArgumentException());
                    await TermsController.GetAllTermsAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }


            [TestMethod]
            public async Task GetAllTerms_ReturnsAllTerms()
            {
                var termsResponse = await TermsController.GetAllTermsAsync();
                Assert.AreEqual(allTerms.Count(), termsResponse.Count());
            }

            [TestMethod]
            public async Task GetAllTerms_WithDate_ReturnTermsStartingOnOrAfter()
            {
                var startsOnOrAfterDate = new DateTime(2015, 05, 12); //2015-05-12
                var expectedlTerms = allTerms.Where(term => term.StartDate >= startsOnOrAfterDate);
                var termsResponse = await TermsController.GetAllTermsAsync(startsOnOrAfterDate);

                Assert.AreEqual(expectedlTerms.Count(), termsResponse.Count());
            }
        }

        [TestClass]
        public class TermsControllerGetPlanningTerms
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

            private TermsController TermsController;

            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> allTerms;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                logger = new Mock<ILogger>().Object;

                allTerms = await new TestTermRepository().GetAsync();

                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync((allTerms));

                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Term>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Term>()).Returns(adapter);
                var registrationDateAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationDate, RegistrationDate>(adapterRegistry, logger);       
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationDate, RegistrationDate>()).Returns(registrationDateAdapter);
                
                TermsController = new TermsController(adapterRegistry, termRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                TermsController = null;
                termRepo = null;
            }

            [TestMethod]
            public async Task ReturnsPlanningTerms()
            {
                var termsResponse = await TermsController.GetPlanningTermsAsync();
                Assert.AreEqual(allTerms.Where(t => t.ForPlanning == true).Count(), termsResponse.Count());
            }
        }

        [TestClass]
        public class TermsControllerGetRegistrationTerms
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

            private TermsController TermsController;

            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                logger = new Mock<ILogger>().Object;
                List<string> termIds = new List<string>();
                termIds.Add("2012/SP");
                termIds.Add("2012/FA");
                regTerms = await new TestTermRepository().GetAsync(termIds);

                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Term>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Term>()).Returns(adapter);
                var registrationDateAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationDate, RegistrationDate>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationDate, RegistrationDate>()).Returns(registrationDateAdapter);
                TermsController = new TermsController(adapterRegistry, termRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                TermsController = null;
                termRepo = null;
            }

            [TestMethod]
            public async Task ReturnsPlanningTerms()
            {
                var termsResponse =await TermsController.GetRegistrationTermsAsync();
                Assert.AreEqual(2, termsResponse.Count());
            }
        }

        [TestClass]
        public class TermsControllerGetTerm
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

            private TermsController TermsController;

            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Student.Entities.Term term;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                logger = new Mock<ILogger>().Object;

                term = new TestTermRepository().Get("2012/FA");

                termRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(term);

                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Term>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Term>()).Returns(adapter);
                var registrationDateAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationDate, RegistrationDate>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationDate, RegistrationDate>()).Returns(registrationDateAdapter);
                TermsController = new TermsController(adapterRegistry, termRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                TermsController = null;
                termRepo = null;
            }

            [TestMethod]
            public async Task ReturnsATerm_TestProperties()
            {
                var termResponse = await TermsController.GetAsync("2012_~FA");
                Assert.AreEqual(term.Description, termResponse.Description);
                Assert.AreEqual(term.Code, termResponse.Code);
                Assert.AreEqual(term.StartDate, termResponse.StartDate);
                Assert.AreEqual(term.EndDate, termResponse.EndDate);
                Assert.AreEqual(term.ReportingYear, termResponse.ReportingYear);
                Assert.AreEqual(term.Sequence, termResponse.Sequence);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReturnsExceptionWhenStringNull()
            {
                Ellucian.Colleague.Domain.Student.Entities.Term noTerm = null;
                termRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(noTerm);
                string q = null;
                var termsResponse = await TermsController.GetAsync(q);
            }
        }
    }
}
