// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers.Planning;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Planning
{
    [TestClass]
    public class PlanningConfigurationControllerTests
    {
        [TestClass]
        public class GetPlanningConfigurationAsync : PlanningConfigurationControllerTests
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

            private PlanningConfigurationController planningConfigurationController;
            private Mock<IPlanningConfigurationRepository> planningRepositoryMock;
            private IPlanningConfigurationRepository planningRepository;
            private IAdapterRegistry adapterRegistry;
            ILogger logger = new Mock<ILogger>().Object;

            private Domain.Planning.Entities.PlanningConfiguration entity;

            private HttpResponse response;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                planningRepositoryMock = new Mock<IPlanningConfigurationRepository>();
                planningRepository = planningRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Planning.Entities.PlanningConfiguration, PlanningConfiguration>(adapterRegistry, logger);
                adapterRegistry.AddAdapter(testAdapter);

                planningConfigurationController = new PlanningConfigurationController(planningRepository, logger, adapterRegistry);
                Mapper.CreateMap<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>();

                // mock an advisor dto to return
                entity = new Domain.Planning.Entities.PlanningConfiguration()
                {
                    DefaultCatalogPolicy = Domain.Planning.Entities.CatalogPolicy.StudentCatalogYear,
                    DefaultCurriculumTrack = "DEFAULT",
                    ShowAdvisementCompleteWorkflow = true
                };
                planningRepositoryMock.Setup(x => x.GetPlanningConfigurationAsync()).ReturnsAsync(entity);

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
            }

            [TestCleanup]
            public void Cleanup()
            {
                planningConfigurationController = null;
                planningRepository = null;
            }

            [TestMethod]
            public async Task GetPlanningConfigurationAsync_Success()
            {
                var config = await planningConfigurationController.GetPlanningConfigurationAsync();
                Assert.AreEqual(CatalogPolicy.StudentCatalogYear, config.DefaultCatalogPolicy);
                Assert.AreEqual(entity.DefaultCurriculumTrack, config.DefaultCurriculumTrack);
                Assert.AreEqual(entity.ShowAdvisementCompleteWorkflow, config.ShowAdvisementCompleteWorkflow);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPlanningConfigurationAsync_Exception()
            {
                planningRepositoryMock.Setup(x => x.GetPlanningConfigurationAsync()).ThrowsAsync(new Exception());
                var config = await planningConfigurationController.GetPlanningConfigurationAsync();
            }
        }
    }
}
