// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class InstitutionJobSupervisorsControllerTests
    {
        [TestClass]
        public class InstitutionJobSupervisorsControllerGet
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

            private InstitutionJobSupervisorsController InstitutionJobSupervisorsController;
            private Mock<IInstitutionJobsRepository> institutionJobRepositoryMock;
            private IInstitutionJobsRepository institutionJobRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs> allInstitutionJobEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IInstitutionJobSupervisorsService> InstitutionJobSupervisorServiceMock;
            private IInstitutionJobSupervisorsService institutionJobSupervisorService;
            List<Ellucian.Colleague.Dtos.InstitutionJobSupervisors> InstitutionJobSupervisorList;
            private string institutionJobSupervisorsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                institutionJobRepositoryMock = new Mock<IInstitutionJobsRepository>();
                institutionJobRepository = institutionJobRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs, Dtos.InstitutionJobSupervisors>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                InstitutionJobSupervisorServiceMock = new Mock<IInstitutionJobSupervisorsService>();
                institutionJobSupervisorService = InstitutionJobSupervisorServiceMock.Object;

                allInstitutionJobEntities = new TestInstitutionJobsRepository().GetInstitutionJobs();
                InstitutionJobSupervisorList = new List<Dtos.InstitutionJobSupervisors>();

                InstitutionJobSupervisorsController = new InstitutionJobSupervisorsController(institutionJobSupervisorService, logger);
                InstitutionJobSupervisorsController.Request = new HttpRequestMessage();
                InstitutionJobSupervisorsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var institutionJob in allInstitutionJobEntities)
                {
                    Dtos.InstitutionJobSupervisors target = ConvertInstitutionJobsEntityToDto(institutionJob);
                    InstitutionJobSupervisorList.Add(target);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                InstitutionJobSupervisorsController = null;
                institutionJobRepository = null;
            }

            [TestMethod]
            public async Task InstitutionJobSupervisorsController_GetInstitutionJobSupervisorsAsync()
            {
                InstitutionJobSupervisorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(InstitutionJobSupervisorList, 5);

                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                var institutionJobSupervisors = await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(10, 0));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobSupervisors.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstitutionJobSupervisors> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobSupervisors>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(institutionJobSupervisors is IHttpActionResult);

                foreach (var institutionJobSupervisorsDto in InstitutionJobSupervisorList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == institutionJobSupervisorsDto.Id);

                    Assert.AreEqual(institutionJobSupervisorsDto.Id, emp.Id);
                    Assert.AreEqual(institutionJobSupervisorsDto.Person, emp.Person);
                }
            }

            //[TestMethod]
            //public async Task InstitutionJobSupervisorsController_GetInstitutionJobSupervisorsAsyncFilters()
            //{
            //    InstitutionJobSupervisorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            //    var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(InstitutionJobSupervisorList, 5);

            //    InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            //    var criteria = "{\"person\":\"e9e6837f-2c51-431b-9069-4ac4c0da3041\"";
            //    criteria += ",\"employer\":\"27447903-1c15-41e2-b997-a3a308a262d8\"";
            //    criteria += ",\"position\":\"4784012f-97d6-43e7-8dca-80e2362e200c\"";
            //    criteria += ",\"department\":\"Math\"";
            //    criteria += ",\"startOn\":\"2000-01-01 00:00:00.000\"";
            //    criteria += ",\"endOn\":\"2020-12-31 00:00:00.000\"";
            //    criteria += ",\"classification\":\"0f24ecb6-e3bf-4a76-9582-463a2bced2a7\"";
            //    criteria += ",\"preference\":\"primary\"";
            //    criteria += ",\"status\":\"active\"}";

            //    var institutionJobs = await InstitutionJobSupervisorsController.GetInstitutionJobsAsync(new Paging(10, 0), criteria);

            //    var cancelToken = new System.Threading.CancellationToken(false);

            //    System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobs.ExecuteAsync(cancelToken);

            //    IEnumerable<Dtos.InstitutionJobs> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobs>;

            //    var result = results.FirstOrDefault();

            //    Assert.IsTrue(institutionJobs is IHttpActionResult);

            //    foreach (var institutionJobsDto in InstitutionJobSupervisorList)
            //    {
            //        var emp = results.FirstOrDefault(i => i.Id == institutionJobsDto.Id);

            //        Assert.AreEqual(institutionJobsDto.Id, emp.Id);
            //        Assert.AreEqual(institutionJobsDto.Person, emp.Person);
            //    }
            //}

            [TestMethod]
            public async Task GetInstitutionJobSupervisorsByGuidAsync_Validate()
            {
                var thisInstitutionJobSupervisor = InstitutionJobSupervisorList.Where(m => m.Id == institutionJobSupervisorsGuid).FirstOrDefault();

                InstitutionJobSupervisorServiceMock.Setup(x => x.GetInstitutionJobSupervisorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisInstitutionJobSupervisor);

                var institutionJobSupervisor = await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsByGuidAsync(institutionJobSupervisorsGuid);
                Assert.AreEqual(thisInstitutionJobSupervisor.Id, institutionJobSupervisor.Id);
                Assert.AreEqual(thisInstitutionJobSupervisor.Person, institutionJobSupervisor.Person);
            }

            [TestMethod]
            public async Task InstitutionJobSupervisorsController_GetHedmAsync_CacheControlNotNull()
            {
                InstitutionJobSupervisorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                InstitutionJobSupervisorsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(InstitutionJobSupervisorList, 5);

                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                var institutionJobSupervisors = await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(10, 0));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobSupervisors.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstitutionJobSupervisors> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobSupervisors>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(institutionJobSupervisors is IHttpActionResult);

                foreach (var InstitutionJobSupervisorsDto in InstitutionJobSupervisorList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == InstitutionJobSupervisorsDto.Id);

                    Assert.AreEqual(InstitutionJobSupervisorsDto.Id, emp.Id);
                    Assert.AreEqual(InstitutionJobSupervisorsDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task InstitutionJobSupervisorsController_GetHedmAsync_NoCache()
            {
                InstitutionJobSupervisorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                InstitutionJobSupervisorsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                InstitutionJobSupervisorsController.Request.Headers.CacheControl.NoCache = true;

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(InstitutionJobSupervisorList, 5);

                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                var institutionJobSupervisors = await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(10, 0));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobSupervisors.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstitutionJobSupervisors> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobSupervisors>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(institutionJobSupervisors is IHttpActionResult);

                foreach (var institutionJobSupervisorsDto in InstitutionJobSupervisorList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == institutionJobSupervisorsDto.Id);

                    Assert.AreEqual(institutionJobSupervisorsDto.Id, emp.Id);
                    Assert.AreEqual(institutionJobSupervisorsDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task InstitutionJobSupervisorsController_GetHedmAsync_Cache()
            {
                InstitutionJobSupervisorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                InstitutionJobSupervisorsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                InstitutionJobSupervisorsController.Request.Headers.CacheControl.NoCache = false;

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(InstitutionJobSupervisorList, 5);

                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                var institutionJobSupervisors = await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(10, 0));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await institutionJobSupervisors.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstitutionJobSupervisors> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstitutionJobSupervisors>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(institutionJobSupervisors is IHttpActionResult);

                foreach (var institutionJobSupervisorsDto in InstitutionJobSupervisorList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == institutionJobSupervisorsDto.Id);

                    Assert.AreEqual(institutionJobSupervisorsDto.Id, emp.Id);
                    Assert.AreEqual(institutionJobSupervisorsDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task InstitutionJobSupervisorsController_GetByIdHedmAsync()
            {
                var thisInstitutionJobSupervisor = InstitutionJobSupervisorList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                InstitutionJobSupervisorServiceMock.Setup(x => x.GetInstitutionJobSupervisorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisInstitutionJobSupervisor);

                var institutionJobSupervisor = await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsByGuidAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisInstitutionJobSupervisor.Id, institutionJobSupervisor.Id);
                Assert.AreEqual(thisInstitutionJobSupervisor.Person, institutionJobSupervisor.Person);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetThrowsIntAppiExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<Exception>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetThrowsIntAppiKeyNotFoundExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetThrowsIntAppiArgumentExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<ArgumentException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetThrowsIntAppiRepositoryExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<RepositoryException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetThrowsIntAppiIntegrationExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetThrowsIntAppiPermissionExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(s => s.GetInstitutionJobSupervisorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<PermissionsException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetByIdThrowsExc()
            {
                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetByIdThrowsIntAppiExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(gc => gc.GetInstitutionJobSupervisorsByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsByGuidAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetByIdThrowsIntAppiPermissionExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(gc => gc.GetInstitutionJobSupervisorsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsByGuidAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetByIdThrowsIntAppiKeyNotFoundExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(gc => gc.GetInstitutionJobSupervisorsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsByGuidAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetByIdThrowsIntAppiIntegrationExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(gc => gc.GetInstitutionJobSupervisorsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsByGuidAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetByIdThrowsIntAppiArgumentExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(gc => gc.GetInstitutionJobSupervisorsByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsByGuidAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_GetByIdThrowsIntAppiRepositoryExc()
            {
                InstitutionJobSupervisorServiceMock.Setup(gc => gc.GetInstitutionJobSupervisorsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();

                await InstitutionJobSupervisorsController.GetInstitutionJobSupervisorsByGuidAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_PostThrowsIntAppiExc()
            {
                await InstitutionJobSupervisorsController.PostInstitutionJobSupervisorsAsync(InstitutionJobSupervisorList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_PutThrowsIntAppiExc()
            {
                var result = await InstitutionJobSupervisorsController.PutInstitutionJobSupervisorsAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023", InstitutionJobSupervisorList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionJobSupervisorsController_DeleteThrowsIntAppiExc()
            {
                await InstitutionJobSupervisorsController.DeleteInstitutionJobSupervisorsAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a InstitutionJobSupervisor domain entity to its corresponding InstitutionJobSupervisor DTO
            /// </summary>
            /// <param name="source">InstitutionJobSupervisor domain entity</param>
            /// <returns>InstitutionJobSupervisor DTO</returns>
            private Ellucian.Colleague.Dtos.InstitutionJobSupervisors ConvertInstitutionJobsEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs source)
            {
                var institutionJobSupervisor = new Ellucian.Colleague.Dtos.InstitutionJobSupervisors();
                institutionJobSupervisor.Id = source.Guid;
                institutionJobSupervisor.Person = new Dtos.GuidObject2(source.PersonId);

                return institutionJobSupervisor;
            }
        }
    }
}