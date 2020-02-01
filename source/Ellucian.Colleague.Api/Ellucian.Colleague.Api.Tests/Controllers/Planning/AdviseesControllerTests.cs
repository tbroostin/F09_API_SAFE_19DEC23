// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers.Planning;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Adapters;
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
    public class AdviseesControllerTests
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

        private AdviseesController adviseesController;
        private Mock<IAdvisorService> AdvisorServiceMock;
        private IAdvisorService advisorService;
        private IAdapterRegistry adapterRegistry;
        ILogger logger = new Mock<ILogger>().Object;
        private Ellucian.Colleague.Dtos.Planning.Advisor advisor = null;

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

            adviseesController = new AdviseesController(advisorService, logger);
            Mapper.CreateMap<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>();

            // mock an advisee dtos to return
            List<Advisee> advisees = new List<Advisee>() { new Advisee() { LastName = "Smith", FirstName = "John" }, new Advisee() { LastName = "Smith", FirstName = "Paul" } };
            var privacyWrapper = new PrivacyWrapper<List<Advisee>>(advisees, true);

            AdvisorServiceMock.Setup(x => x.Search3Async(It.IsAny<AdviseeSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(privacyWrapper);

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adviseesController = null;
            advisorService = null;
        }

        [TestMethod]
        public async Task Advisee_Search()
        {
            AdviseeSearchCriteria criteria = new AdviseeSearchCriteria() { AdviseeKeyword = "Smith" };
            var searchList = await adviseesController.QueryAdviseesByPost2Async(criteria);
            Assert.IsTrue(searchList.Count() == 2);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task Search_ExceptionReturnsHttpResponseException()
        {
            var crit = new AdviseeSearchCriteria();
            AdvisorServiceMock.Setup(x => x.Search3Async(crit, It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("some kind of error"));
            var adv = await adviseesController.QueryAdviseesByPost2Async(crit);
        }


    }
}
