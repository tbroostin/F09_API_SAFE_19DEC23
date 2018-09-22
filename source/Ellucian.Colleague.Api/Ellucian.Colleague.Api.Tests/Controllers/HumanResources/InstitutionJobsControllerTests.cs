// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class InstitutionJobsControllerTestsv8
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

        private InstitutionJobsController InstitutionJobsController;
        private Mock<IInstitutionJobsRepository> institutionJobRepositoryMock;
        private IInstitutionJobsRepository institutionJobRepository;
        private IAdapterRegistry AdapterRegistry;
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs> allInstitutionJobEntities;
        ILogger logger = new Mock<ILogger>().Object;
        private Mock<IInstitutionJobsService> institutionJobServiceMock;
        private IInstitutionJobsService institutionJobService;
        List<Ellucian.Colleague.Dtos.InstitutionJobs> InstitutionJobList;
        private string institutionJobsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            institutionJobRepositoryMock = new Mock<IInstitutionJobsRepository>();
            institutionJobRepository = institutionJobRepositoryMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs, Dtos.InstitutionJobs>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            institutionJobServiceMock = new Mock<IInstitutionJobsService>();
            institutionJobService = institutionJobServiceMock.Object;

            allInstitutionJobEntities = new TestInstitutionJobsRepository().GetInstitutionJobs();
            InstitutionJobList = new List<Dtos.InstitutionJobs>();

            InstitutionJobsController = new InstitutionJobsController(institutionJobService, logger);
            InstitutionJobsController.Request = new HttpRequestMessage();
            InstitutionJobsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var institutionJob in allInstitutionJobEntities)
            {
                Dtos.InstitutionJobs target = ConvertInstitutionJobsEntityToDto(institutionJob);
                InstitutionJobList.Add(target);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            InstitutionJobsController = null;
            institutionJobRepository = null;
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.InstitutionJobs> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsyncFilters()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs() { 
                      Person = new Dtos.GuidObject2("e9e6837f-2c51-431b-9069-4ac4c0da3041"),
                      Employer = new Dtos.GuidObject2("27447903-1c15-41e2-b997-a3a308a262d8"),
                      Position = new Dtos.GuidObject2("4784012f-97d6-43e7-8dca-80e2362e200c"),
                      Department = "Math",
                      StartOn = new DateTime(2000, 01, 01, 00, 00, 00),
                      EndOn = new DateTime(2020, 12, 31, 00, 00, 00),
                      Classification = new Dtos.GuidObject2("0f24ecb6-e3bf-4a76-9582-463a2bced2a7"),
                      Preference = Dtos.EnumProperties.JobPreference.Primary,
                      Status = Dtos.EnumProperties.InstitutionJobsStatus.Active
                    });

            var institutionJobs = await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.InstitutionJobs> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }


        [TestMethod]
        public async Task GetInstitutionJobsByGuidAsync_Validate()
        {
            var thisInstitutionJob = InstitutionJobList.Where(m => m.Id == institutionJobsGuid).FirstOrDefault();

            institutionJobServiceMock.Setup(x => x.GetInstitutionJobsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(thisInstitutionJob);

            var institutionJob = await InstitutionJobsController.GetInstitutionJobsByGuidAsync(institutionJobsGuid);
            Assert.AreEqual(thisInstitutionJob.Id, institutionJob.Id);
            Assert.AreEqual(thisInstitutionJob.Person, institutionJob.Person);
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetHedmAsync_CacheControlNotNull()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            InstitutionJobsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.InstitutionJobs> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetHedmAsync_NoCache()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            InstitutionJobsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            InstitutionJobsController.Request.Headers.CacheControl.NoCache = true;

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.InstitutionJobs> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetHedmAsync_Cache()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            InstitutionJobsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            InstitutionJobsController.Request.Headers.CacheControl.NoCache = false;

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.InstitutionJobs> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetByIdHedmAsync()
        {
            var thisInstitutionJob = InstitutionJobList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

            institutionJobServiceMock.Setup(x => x.GetInstitutionJobsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(thisInstitutionJob);

            var institutionJob = await InstitutionJobsController.GetInstitutionJobsByGuidAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
            Assert.AreEqual(thisInstitutionJob.Id, institutionJob.Id);
            Assert.AreEqual(thisInstitutionJob.Person, institutionJob.Person);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiKeyNotFoundExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();

            await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiArgumentExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();

            await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiRepositoryExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();

            await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiIntegrationExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

            await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiPermissionExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();

            await InstitutionJobsController.GetInstitutionJobsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsExc()
        {
            await InstitutionJobsController.GetInstitutionJobsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            await InstitutionJobsController.GetInstitutionJobsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiPermissionExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();

            await InstitutionJobsController.GetInstitutionJobsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiKeyNotFoundExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();

            await InstitutionJobsController.GetInstitutionJobsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiIntegrationExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

            await InstitutionJobsController.GetInstitutionJobsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiArgumentExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();

            await InstitutionJobsController.GetInstitutionJobsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiRepositoryExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();

            await InstitutionJobsController.GetInstitutionJobsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostThrowsIntAppiExc()
        {
            await InstitutionJobsController.PostInstitutionJobsAsync(InstitutionJobList[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutThrowsIntAppiExc()
        {
            var result = await InstitutionJobsController.PutInstitutionJobsAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023", InstitutionJobList[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_DeleteThrowsIntAppiExc()
        {
            await InstitutionJobsController.DeleteInstitutionJobsAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstitutionJob domain entity to its corresponding InstitutionJob DTO
        /// </summary>
        /// <param name="source">InstitutionJob domain entity</param>
        /// <returns>InstitutionJob DTO</returns>
        private Ellucian.Colleague.Dtos.InstitutionJobs ConvertInstitutionJobsEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs source)
        {
            var institutionJob = new Ellucian.Colleague.Dtos.InstitutionJobs();
            institutionJob.Id = source.Guid;
            institutionJob.Person = new Dtos.GuidObject2(source.PersonId);

            return institutionJob;
        }
    }

    [TestClass]
    public class InstitutionJobsControllerTestsv11
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

        private InstitutionJobsController InstitutionJobsController;
        private Mock<IInstitutionJobsRepository> institutionJobRepositoryMock;
        private IInstitutionJobsRepository institutionJobRepository;
        private IAdapterRegistry AdapterRegistry;
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs> allInstitutionJobEntities;
        ILogger logger = new Mock<ILogger>().Object;
        private Mock<IInstitutionJobsService> institutionJobServiceMock;
        private IInstitutionJobsService institutionJobService;
        List<Ellucian.Colleague.Dtos.InstitutionJobs2> InstitutionJobList;
        private string institutionJobsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            institutionJobRepositoryMock = new Mock<IInstitutionJobsRepository>();
            institutionJobRepository = institutionJobRepositoryMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs, Dtos.InstitutionJobs>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            institutionJobServiceMock = new Mock<IInstitutionJobsService>();
            institutionJobService = institutionJobServiceMock.Object;

            allInstitutionJobEntities = new TestInstitutionJobsRepository().GetInstitutionJobs();
            InstitutionJobList = new List<Dtos.InstitutionJobs2>();

            InstitutionJobsController = new InstitutionJobsController(institutionJobService, logger);
            InstitutionJobsController.Request = new HttpRequestMessage();
            InstitutionJobsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            
            foreach (var institutionJob in allInstitutionJobEntities)
            {
                var target = ConvertInstitutionJobsEntityToDto(institutionJob);
                InstitutionJobList.Add(target);
            }

            InstitutionJobsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(InstitutionJobList.FirstOrDefault()));
        }

        [TestCleanup]
        public void Cleanup()
        {
            InstitutionJobsController = null;
            institutionJobRepository = null;
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs2>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsyncFilters()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs2()
                  {
                      Person = new Dtos.GuidObject2("e9e6837f-2c51-431b-9069-4ac4c0da3041"),
                      Employer = new Dtos.GuidObject2("27447903-1c15-41e2-b997-a3a308a262d8"),
                      Position = new Dtos.GuidObject2("4784012f-97d6-43e7-8dca-80e2362e200c"),
                      Department = "Math",
                      StartOn = new DateTime(2000, 01, 01, 00, 00, 00),
                      EndOn = new DateTime(2020, 12, 31, 00, 00, 00),
                      Classification = new Dtos.GuidObject2("0f24ecb6-e3bf-4a76-9582-463a2bced2a7"),
                      Preference = Dtos.EnumProperties.JobPreference.Primary,
                      Status = Dtos.EnumProperties.InstitutionJobsStatus.Active
                  });

            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs2>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }


        [TestMethod]
        public async Task GetInstitutionJobsByGuidAsync_Validate()
        {
            var thisInstitutionJob = InstitutionJobList.Where(m => m.Id == institutionJobsGuid).FirstOrDefault();

            institutionJobServiceMock.Setup(x => x.GetInstitutionJobsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(thisInstitutionJob);

            var institutionJob = await InstitutionJobsController.GetInstitutionJobsByGuid2Async(institutionJobsGuid);
            Assert.AreEqual(thisInstitutionJob.Id, institutionJob.Id);
            Assert.AreEqual(thisInstitutionJob.Person, institutionJob.Person);
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetHedmAsync_CacheControlNotNull()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            InstitutionJobsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs2>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetHedmAsync_NoCache()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            InstitutionJobsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            InstitutionJobsController.Request.Headers.CacheControl.NoCache = true;

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs2>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetHedmAsync_Cache()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            InstitutionJobsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            InstitutionJobsController.Request.Headers.CacheControl.NoCache = false;

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs2>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetByIdHedmAsync()
        {
            var thisInstitutionJob = InstitutionJobList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

            institutionJobServiceMock.Setup(x => x.GetInstitutionJobsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(thisInstitutionJob);

            var institutionJob = await InstitutionJobsController.GetInstitutionJobsByGuid2Async("625c69ff-280b-4ed3-9474-662a43616a8a");
            Assert.AreEqual(thisInstitutionJob.Id, institutionJob.Id);
            Assert.AreEqual(thisInstitutionJob.Person, institutionJob.Person);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiKeyNotFoundExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();

            await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiArgumentExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();

            await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiRepositoryExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();

            await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiIntegrationExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

            await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiPermissionExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();

            await InstitutionJobsController.GetInstitutionJobs2Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsExc()
        {
            await InstitutionJobsController.GetInstitutionJobsByGuid2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            await InstitutionJobsController.GetInstitutionJobsByGuid2Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiPermissionExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid2Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiKeyNotFoundExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid2Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiIntegrationExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid2Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiArgumentExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid2Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiRepositoryExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid2Async("sdjfh");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async_PermissionsException()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = null };
            institutionJobServiceMock.Setup(gc => gc.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<PermissionsException>();
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async_KeyNotFoundException()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            institutionJobServiceMock.Setup(gc => gc.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<KeyNotFoundException>();
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async_ArgumentException()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            institutionJobServiceMock.Setup(gc => gc.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<ArgumentException>();
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async_RepositoryException()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            institutionJobServiceMock.Setup(gc => gc.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<RepositoryException>();
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async_RepositoryException_With_Errors()
        {
            var exception = new RepositoryException() { };
            exception.AddErrors(new List<Domain.Entities.RepositoryError>() { new Domain.Entities.RepositoryError("ERROR") });

            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            institutionJobServiceMock.Setup(gc => gc.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).ThrowsAsync(exception);
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async_IntegrationApiException()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            institutionJobServiceMock.Setup(gc => gc.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<IntegrationApiException>();
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async_ConfigurationException()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            institutionJobServiceMock.Setup(gc => gc.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<ConfigurationException>();
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async_Exception()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            institutionJobServiceMock.Setup(gc => gc.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<Exception>();
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);
        }

        [TestMethod]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            institutionJobServiceMock.Setup(gc => gc.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).ReturnsAsync(dto);
            var result = await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);

            Assert.IsNotNull(result);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstitutionJob domain entity to its corresponding InstitutionJob DTO
        /// </summary>
        /// <param name="source">InstitutionJob domain entity</param>
        /// <returns>InstitutionJob DTO</returns>
        private Ellucian.Colleague.Dtos.InstitutionJobs2 ConvertInstitutionJobsEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs source)
        {
            var institutionJob = new Ellucian.Colleague.Dtos.InstitutionJobs2();
            institutionJob.Id = source.Guid;
            institutionJob.Person = new Dtos.GuidObject2(source.PersonId);

            return institutionJob;
        }
    }

    [TestClass]
    public class InstitutionJobsControllerTestsv12
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

        private InstitutionJobsController InstitutionJobsController;
        private Mock<IInstitutionJobsRepository> institutionJobRepositoryMock;
        private IInstitutionJobsRepository institutionJobRepository;
        private IAdapterRegistry AdapterRegistry;
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs> allInstitutionJobEntities;
        ILogger logger = new Mock<ILogger>().Object;
        private Mock<IInstitutionJobsService> institutionJobServiceMock;
        private IInstitutionJobsService institutionJobService;
        List<Ellucian.Colleague.Dtos.InstitutionJobs3> InstitutionJobList;
        private string institutionJobsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            institutionJobRepositoryMock = new Mock<IInstitutionJobsRepository>();
            institutionJobRepository = institutionJobRepositoryMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs, Dtos.InstitutionJobs>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            institutionJobServiceMock = new Mock<IInstitutionJobsService>();
            institutionJobService = institutionJobServiceMock.Object;

            allInstitutionJobEntities = new TestInstitutionJobsRepository().GetInstitutionJobs();
            InstitutionJobList = new List<Dtos.InstitutionJobs3>();

            InstitutionJobsController = new InstitutionJobsController(institutionJobService, logger);
            InstitutionJobsController.Request = new HttpRequestMessage();
            InstitutionJobsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var institutionJob in allInstitutionJobEntities)
            {
                var target = ConvertInstitutionJobsEntityToDto(institutionJob);
                InstitutionJobList.Add(target);
            }

            InstitutionJobsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(InstitutionJobList.FirstOrDefault()));
        }

        [TestCleanup]
        public void Cleanup()
        {
            InstitutionJobsController = null;
            institutionJobRepository = null;
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync_PersonFilters()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs3()
                  {
                      Person = new Dtos.GuidObject2("e9e6837f-2c51-431b-9069-4ac4c0da3041")
                  });

            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync_EmployerFilters()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs3()
                  {
                      Employer = new Dtos.GuidObject2("e9e6837f-2c51-431b-9069-4ac4c0da3041")
                  });

            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync_DepartmentFilters()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs3()
                  {
                      Department = new Dtos.GuidObject2("e9e6837f-2c51-431b-9069-4ac4c0da3041")
                  });
            
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync_ClassificationFilters()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs3()
                  {
                      Classification = new Dtos.GuidObject2("e9e6837f-2c51-431b-9069-4ac4c0da3041")
                  });
            
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync_StartOnFilter()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs3()
                  {
                      StartOn = new DateTime(2016, 05, 01)
                  });
            
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync_EndOnFilter()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs3()
                  {
                      EndOn = new DateTime(2016, 05, 01)
                  });
            
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync_StatusFilter()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs3()
                  {
                      Status = Dtos.EnumProperties.InstitutionJobsStatus.Active
                  });
            
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        
        [TestMethod]
        public async Task InstitutionJobsController_GetInstitutionJobsAsync_PreferenceFilter()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            var filterGroupName = "criteria";
            InstitutionJobsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.InstitutionJobs3()
                  {
                      Preference = Dtos.EnumProperties.JobPreference2.Primary
                  });
            
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task GetInstitutionJobsByGuidAsync_Validate()
        {
            var thisInstitutionJob = InstitutionJobList.Where(m => m.Id == institutionJobsGuid).FirstOrDefault();

            institutionJobServiceMock.Setup(x => x.GetInstitutionJobsByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(thisInstitutionJob);

            var institutionJob = await InstitutionJobsController.GetInstitutionJobsByGuid3Async(institutionJobsGuid);
            Assert.AreEqual(thisInstitutionJob.Id, institutionJob.Id);
            Assert.AreEqual(thisInstitutionJob.Person, institutionJob.Person);
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetHedmAsync_CacheControlNotNull()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            InstitutionJobsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetHedmAsync_NoCache()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            InstitutionJobsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            InstitutionJobsController.Request.Headers.CacheControl.NoCache = true;

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetHedmAsync_Cache()
        {
            InstitutionJobsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            InstitutionJobsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            InstitutionJobsController.Request.Headers.CacheControl.NoCache = false;

            var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(InstitutionJobList, 5);

            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var institutionJobs = await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs3>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(institutionJobs is IHttpActionResult);

            foreach (var institutionJobsDto in InstitutionJobList)
            {
                var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

                Assert.AreEqual(institutionJobsDto.Id, emp.Id);
                Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            }
        }

        [TestMethod]
        public async Task InstitutionJobsController_GetByIdHedmAsync()
        {
            var thisInstitutionJob = InstitutionJobList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

            institutionJobServiceMock.Setup(x => x.GetInstitutionJobsByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(thisInstitutionJob);

            var institutionJob = await InstitutionJobsController.GetInstitutionJobsByGuid3Async("625c69ff-280b-4ed3-9474-662a43616a8a");
            Assert.AreEqual(thisInstitutionJob.Id, institutionJob.Id);
            Assert.AreEqual(thisInstitutionJob.Person, institutionJob.Person);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiKeyNotFoundExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();

            await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiArgumentExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();

            await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiRepositoryExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();

            await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiIntegrationExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

            await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetThrowsIntAppiPermissionExc()
        {
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobs3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();

            await InstitutionJobsController.GetInstitutionJobs3Async(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsExc()
        {
            await InstitutionJobsController.GetInstitutionJobsByGuid3Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            await InstitutionJobsController.GetInstitutionJobsByGuid3Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiPermissionExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid3Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiKeyNotFoundExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid3Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiIntegrationExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid3Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiArgumentExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid3Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_GetByIdThrowsIntAppiRepositoryExc()
        {
            institutionJobServiceMock.Setup(gc => gc.GetInstitutionJobsByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();

            await InstitutionJobsController.GetInstitutionJobsByGuid3Async("sdjfh");
        }

        #region v12 Post InstitutionJobs

        [TestMethod]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async()
        {
            var thisInstitutionJob = InstitutionJobList.Where(m => m.Id == institutionJobsGuid).FirstOrDefault();
            thisInstitutionJob.Id = "00000000-0000-0000-0000-000000000000";

            institutionJobServiceMock.Setup(x => x.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).ReturnsAsync(thisInstitutionJob);

            var institutionJob = await InstitutionJobsController.PostInstitutionJobs3Async(thisInstitutionJob);
            Assert.AreEqual(thisInstitutionJob.Id, institutionJob.Id);
            Assert.AreEqual(thisInstitutionJob.Person, institutionJob.Person);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async_notNillGuid()
        {
            var thisInstitutionJob = InstitutionJobList.Where(m => m.Id == institutionJobsGuid).FirstOrDefault();

            institutionJobServiceMock.Setup(x => x.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).ReturnsAsync(thisInstitutionJob);

            var institutionJob = await InstitutionJobsController.PostInstitutionJobs3Async(thisInstitutionJob);
            Assert.AreEqual(thisInstitutionJob.Id, institutionJob.Id);
            Assert.AreEqual(thisInstitutionJob.Person, institutionJob.Person);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async_Dto_Null()
        {
            await InstitutionJobsController.PostInstitutionJobs3Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async_PermissionsException()
        {
            institutionJobServiceMock.Setup(gc => gc.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<PermissionsException>();
            await InstitutionJobsController.PostInstitutionJobs3Async(new Dtos.InstitutionJobs3() { Id = institutionJobsGuid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async_KeyNotFoundException()
        {
            institutionJobServiceMock.Setup(gc => gc.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<KeyNotFoundException>();
            await InstitutionJobsController.PostInstitutionJobs3Async(new Dtos.InstitutionJobs3() { Id = institutionJobsGuid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async_ArgumentException()
        {
            institutionJobServiceMock.Setup(gc => gc.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<ArgumentException>();
            await InstitutionJobsController.PostInstitutionJobs3Async(new Dtos.InstitutionJobs3() { Id = institutionJobsGuid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async_RepositoryException()
        {
            institutionJobServiceMock.Setup(gc => gc.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<RepositoryException>();
            await InstitutionJobsController.PostInstitutionJobs3Async(new Dtos.InstitutionJobs3() { Id = institutionJobsGuid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async_RepositoryException_With_Errors()
        {
            var exception = new RepositoryException() { };
            exception.AddErrors(new List<Domain.Entities.RepositoryError>() { new Domain.Entities.RepositoryError("ERROR") });

            institutionJobServiceMock.Setup(gc => gc.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).ThrowsAsync(exception);
            await InstitutionJobsController.PostInstitutionJobs3Async(new Dtos.InstitutionJobs3() { Id = institutionJobsGuid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs32Async_IntegrationApiException()
        {
            institutionJobServiceMock.Setup(gc => gc.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<IntegrationApiException>();
            await InstitutionJobsController.PostInstitutionJobs3Async(new Dtos.InstitutionJobs3() { Id = institutionJobsGuid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async_ConfigurationException()
        {
            institutionJobServiceMock.Setup(gc => gc.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<ConfigurationException>();
            await InstitutionJobsController.PostInstitutionJobs3Async(new Dtos.InstitutionJobs3() { Id = institutionJobsGuid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PostInstitutionJobs3Async_Exception()
        {
            institutionJobServiceMock.Setup(gc => gc.PostInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).Throws<Exception>();
            await InstitutionJobsController.PostInstitutionJobs3Async(new Dtos.InstitutionJobs3() { Id = institutionJobsGuid });
        }
        
        #endregion

        #region v12 PUT institutionJobs


        [TestMethod]
        public async Task InstitutionJobsController_PutInstitutionJobs3Async()
        {
            var thisInstitutionJob = InstitutionJobList.Where(m => m.Id == institutionJobsGuid).FirstOrDefault();
            var guid = thisInstitutionJob.Id;
            institutionJobServiceMock.Setup(s => s.PutInstitutionJobsAsync(It.IsAny<Dtos.InstitutionJobs3>())).ReturnsAsync(thisInstitutionJob);
            institutionJobServiceMock.Setup(s => s.GetInstitutionJobsByGuid3Async(guid,true)).ReturnsAsync(thisInstitutionJob);

            var result = await InstitutionJobsController.PutInstitutionJobs3Async(guid, thisInstitutionJob);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs2Async_Guid_Null()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };

            await InstitutionJobsController.PutInstitutionJobs3Async(null, dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs2Async_Dto_Null()
        {
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs2Async_Empty_Guid()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            await InstitutionJobsController.PutInstitutionJobs3Async(Guid.Empty.ToString(), dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs2Async_Dto_Empty_Guid()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = Guid.Empty.ToString() };
            await InstitutionJobsController.PutInstitutionJobs3Async(institutionJobsGuid, dto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstitutionJobsController_PutInstitutionJobs2Async_Guid_And_Dto_Guid_NotSame()
        {
            var dto = new Dtos.InstitutionJobs3() { Id = institutionJobsGuid };
            await InstitutionJobsController.PutInstitutionJobs3Async(Guid.NewGuid().ToString(), dto);
        }

        #endregion


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstitutionJob domain entity to its corresponding InstitutionJob DTO
        /// </summary>
        /// <param name="source">InstitutionJob domain entity</param>
        /// <returns>InstitutionJob DTO</returns>
        private Ellucian.Colleague.Dtos.InstitutionJobs3 ConvertInstitutionJobsEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs source)
        {
            var institutionJob = new Ellucian.Colleague.Dtos.InstitutionJobs3();
            institutionJob.Id = source.Guid;
            institutionJob.Person = new Dtos.GuidObject2(source.PersonId);

            return institutionJob;
        }

    }
}