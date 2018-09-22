// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.HumanResources.Base.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class JobChangeReasonsControllerTests
    {
        [TestClass]
        public class JobChangeReasonsControllerGet
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

            private JobChangeReasonsController JobChangeReasonsController;
            private Mock<IHumanResourcesReferenceDataRepository> hrReferenceRepositoryMock;
            private IHumanResourcesReferenceDataRepository hrReferenceRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.JobChangeReason> allJobChangeReasonEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IJobChangeReasonService> jobChangeReasonServiceMock;
            private IJobChangeReasonService jobChangeReasonService;
            List<Ellucian.Colleague.Dtos.JobChangeReason> JobChangeReasonList;
            private string jobChangeReasonsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                hrReferenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                hrReferenceRepository = hrReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.JobChangeReason, Dtos.JobChangeReason>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                jobChangeReasonServiceMock = new Mock<IJobChangeReasonService>();
                jobChangeReasonService = jobChangeReasonServiceMock.Object;

                allJobChangeReasonEntities = new TestJobChangeReasonRepository().GetJobChangeReasons();
                JobChangeReasonList = new List<Dtos.JobChangeReason>();

                JobChangeReasonsController = new JobChangeReasonsController(jobChangeReasonService, logger);
                JobChangeReasonsController.Request = new HttpRequestMessage();
                JobChangeReasonsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var jobChangeReason in allJobChangeReasonEntities)
                {
                    Dtos.JobChangeReason target = ConvertJobChangeReasonEntityToDto(jobChangeReason);
                    JobChangeReasonList.Add(target);
                }
                hrReferenceRepositoryMock.Setup(x => x.GetJobChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(allJobChangeReasonEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                JobChangeReasonsController = null;
                hrReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetJobChangeReasonsByGuidAsync_Validate()
            {
                var thisJobChangeReason = JobChangeReasonList.Where(m => m.Id == jobChangeReasonsGuid).FirstOrDefault();

                jobChangeReasonServiceMock.Setup(x => x.GetJobChangeReasonByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisJobChangeReason);

                var jobChangeReason = await JobChangeReasonsController.GetJobChangeReasonByIdAsync(jobChangeReasonsGuid);
                Assert.AreEqual(thisJobChangeReason.Id, jobChangeReason.Id);
                Assert.AreEqual(thisJobChangeReason.Code, jobChangeReason.Code);
                Assert.AreEqual(thisJobChangeReason.Description, jobChangeReason.Description);
            }

            [TestMethod]
            public async Task JobChangeReasonsController_GetHedmAsync()
            {
                jobChangeReasonServiceMock.Setup(gc => gc.GetJobChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(JobChangeReasonList);

                var result = await JobChangeReasonsController.GetJobChangeReasonsAsync();
                Assert.AreEqual(result.Count(), allJobChangeReasonEntities.Count());

                int count = allJobChangeReasonEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = JobChangeReasonList[i];
                    var actual = allJobChangeReasonEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task JobChangeReasonsController_GetHedmAsync_CacheControlNotNull()
            {
                JobChangeReasonsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                jobChangeReasonServiceMock.Setup(gc => gc.GetJobChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(JobChangeReasonList);

                var result = await JobChangeReasonsController.GetJobChangeReasonsAsync();
                Assert.AreEqual(result.Count(), allJobChangeReasonEntities.Count());

                int count = allJobChangeReasonEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = JobChangeReasonList[i];
                    var actual = allJobChangeReasonEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task JobChangeReasonsController_GetHedmAsync_NoCache()
            {
                JobChangeReasonsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                JobChangeReasonsController.Request.Headers.CacheControl.NoCache = true;

                jobChangeReasonServiceMock.Setup(gc => gc.GetJobChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(JobChangeReasonList);

                var result = await JobChangeReasonsController.GetJobChangeReasonsAsync();
                Assert.AreEqual(result.Count(), allJobChangeReasonEntities.Count());

                int count = allJobChangeReasonEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = JobChangeReasonList[i];
                    var actual = allJobChangeReasonEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task JobChangeReasonsController_GetHedmAsync_Cache()
            {
                JobChangeReasonsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                JobChangeReasonsController.Request.Headers.CacheControl.NoCache = false;

                jobChangeReasonServiceMock.Setup(gc => gc.GetJobChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(JobChangeReasonList);

                var result = await JobChangeReasonsController.GetJobChangeReasonsAsync();
                Assert.AreEqual(result.Count(), allJobChangeReasonEntities.Count());

                int count = allJobChangeReasonEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = JobChangeReasonList[i];
                    var actual = allJobChangeReasonEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task JobChangeReasonsController_GetByIdHedmAsync()
            {
                var thisJobChangeReason = JobChangeReasonList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                jobChangeReasonServiceMock.Setup(x => x.GetJobChangeReasonByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisJobChangeReason);

                var jobChangeReason = await JobChangeReasonsController.GetJobChangeReasonByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisJobChangeReason.Id, jobChangeReason.Id);
                Assert.AreEqual(thisJobChangeReason.Code, jobChangeReason.Code);
                Assert.AreEqual(thisJobChangeReason.Description, jobChangeReason.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task JobChangeReasonsController_GetThrowsIntAppiExc()
            {
                jobChangeReasonServiceMock.Setup(gc => gc.GetJobChangeReasonsAsync(It.IsAny<bool>())).Throws<Exception>();

                await JobChangeReasonsController.GetJobChangeReasonsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task JobChangeReasonsController_GetByIdThrowsIntAppiExc()
            {
                jobChangeReasonServiceMock.Setup(gc => gc.GetJobChangeReasonByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await JobChangeReasonsController.GetJobChangeReasonByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task JobChangeReasonsController_PostThrowsIntAppiExc()
            {
                await JobChangeReasonsController.PostJobChangeReasonAsync(JobChangeReasonList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task JobChangeReasonsController_PutThrowsIntAppiExc()
            {
                var result = await JobChangeReasonsController.PutJobChangeReasonAsync(JobChangeReasonList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task JobChangeReasonsController_DeleteThrowsIntAppiExc()
            {
                await JobChangeReasonsController.DeleteJobChangeReasonAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a JobChangeReason domain entity to its corresponding JobChangeReason DTO
            /// </summary>
            /// <param name="source">JobChangeReason domain entity</param>
            /// <returns>JobChangeReason DTO</returns>
            private Ellucian.Colleague.Dtos.JobChangeReason ConvertJobChangeReasonEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.JobChangeReason source)
            {
                var jobChangeReason = new Ellucian.Colleague.Dtos.JobChangeReason();
                jobChangeReason.Id = source.Guid;
                jobChangeReason.Code = source.Code;
                jobChangeReason.Title = source.Description;
                jobChangeReason.Description = null;

                return jobChangeReason;
            }
        }
    }
}