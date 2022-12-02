// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CountiesControllerTests
    {
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

        private IReferenceDataRepository referenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private CountiesController countiesController;
        private IEnumerable<Domain.Base.Entities.County> counties;
        private IEnumerable<Domain.Base.Entities.County> counties2;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.County, Dtos.Base.County>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.County, Dtos.Base.County>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            counties = BuildCounties();
            referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(counties);
            countiesController = new CountiesController(adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestMethod]
        public async Task CountiesController_GetAsync_returns_County_DTOs()
        {
            var countyDtos = await countiesController.GetAsync();
            Assert.AreEqual(countyDtos.Count(), counties.Count());
            for(int i = 0; i < counties.Count(); i++)
            {
                Assert.AreEqual(counties.ElementAt(i).Code, countyDtos.ElementAt(i).Code);
                Assert.AreEqual(counties.ElementAt(i).Description, countyDtos.ElementAt(i).Description);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountiesController_GetAsync_throws_exception_when_exception_caught()
        {
            referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            var counties = await countiesController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountiesController_GetAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                await countiesController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw;
            }
        }

        private IEnumerable<Domain.Base.Entities.County> BuildCounties()
        {
            var counties = new List<Domain.Base.Entities.County>()
                {
                    new Domain.Base.Entities.County(new Guid().ToString(), "BUT", "Butler"),
                    new Domain.Base.Entities.County(new Guid().ToString(), "HAM", "Hamilton")
                };

            return counties;
        }
    }
}
