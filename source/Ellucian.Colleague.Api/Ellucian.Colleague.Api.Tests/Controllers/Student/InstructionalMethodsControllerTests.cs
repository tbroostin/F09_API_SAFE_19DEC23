// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class InstructionalMethodsControllerTests
    {
        [TestClass]
        public class InstructionalMethodsController_Get
        {
            #region TestContext
            private TestContext testContextInstance;
            public TestContext TestContext
            { get { return testContextInstance; } set { testContextInstance = value; } }
            #endregion TestContext

            private InstructionalMethodsController instructionalMethodsController;
            private Mock<ICurriculumService> curriculumServiceMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepostioryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private ICurriculumService curriculumService;
            private IAdapterRegistry adapterRegistry;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private IRoleRepository roleRepository;
            private IStudentReferenceDataRepository studentReferenceDataRepository;
            private ILogger logger = new Mock<ILogger>().Object;
            private ICurriculumService curSvc;
            private IEnumerable<InstructionalMethod2> allInstructionalMethods;
            private string instructionalMethodCode;
            private InstructionalMethod2 expectedInstructionalMethod;
            
            [TestInitialize]
            public void Initialize()
            {

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                curriculumServiceMock = new Mock<ICurriculumService>();
                curriculumService = curriculumServiceMock.Object;
                configurationRepostioryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepostioryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                currentUserFactory = currentUserFactoryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepository = studentReferenceDataRepositoryMock.Object;

                curSvc = new CurriculumService(new TestStudentReferenceDataRepository(), configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger);
                allInstructionalMethods = curSvc.GetInstructionalMethods2Async(false).Result;
                curriculumServiceMock.Setup<Task<IEnumerable<InstructionalMethod2>>>(s => s.GetInstructionalMethods2Async(It.IsAny<bool>())).ReturnsAsync(allInstructionalMethods);
                foreach (var instructionalMethod in allInstructionalMethods)
                {
                    curriculumServiceMock.Setup<Task<InstructionalMethod2>>(s => s.GetInstructionalMethodById2Async(instructionalMethod.Code)).ReturnsAsync(instructionalMethod);
                }
                curriculumServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());


                instructionalMethodsController = new InstructionalMethodsController(adapterRegistry, studentReferenceDataRepository, curriculumService, logger);
                instructionalMethodsController.Request = new HttpRequestMessage(/*HttpMethod.Get, "http://stuff"*/);
                instructionalMethodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                instructionalMethodsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                instructionalMethodCode = allInstructionalMethods.First().Code;
                expectedInstructionalMethod = allInstructionalMethods.First();


            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalMethodsController = null;
                curriculumServiceMock = null;
                curriculumService = null;
                configurationRepostioryMock = null;
                configurationRepository = null;
                adapterRegistryMock = null;
                adapterRegistry = null;
                currentUserFactoryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                roleRepository = null;
                studentReferenceDataRepositoryMock = null;
                studentReferenceDataRepository = null;
            }

            // GET ALL
            [TestMethod]
            public void NumberOfKnownPropertiesTest()
            {
                var instructionalMethodProperties = typeof(InstructionalMethod2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(5, instructionalMethodProperties.Length);
            }

            [TestMethod]
            public async Task ReturnsAllInstructionalMethods()
            {
                List<InstructionalMethod2> actualInstructionalMethods = await instructionalMethodsController.GetInstructionalMethods2Async() as List<InstructionalMethod2>;
                Assert.AreEqual(actualInstructionalMethods.Count, allInstructionalMethods.Count());
            }

            [TestMethod]
            public async Task GetInstructionalMethods_Properties_and_cache()
            {
                var actualInstructionalMethods = await instructionalMethodsController.GetInstructionalMethods2Async();
                foreach (var actual in actualInstructionalMethods)
                {
                    InstructionalMethod2 expected = allInstructionalMethods.Where(a => a.Code == actual.Code).FirstOrDefault();
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task InstrMethController_GetHedmAsync_CacheControlNull()
            {
                instructionalMethodsController.Request.Headers.CacheControl = null;
                var actualInstructionalMethods = await instructionalMethodsController.GetInstructionalMethods2Async();
                Assert.IsNotNull(actualInstructionalMethods);
            }

            [TestMethod]
            public async Task InstrMethController_GetHedmAsync_Cache()
            {

                instructionalMethodsController.Request.Headers.CacheControl.NoCache = false;
                instructionalMethodsController.Request.Headers.CacheControl = null;
                var actualInstructionalMethods = await instructionalMethodsController.GetInstructionalMethods2Async();
                Assert.IsNotNull(actualInstructionalMethods);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstrMethController_GetThrowsIntAppiExc()
            {
                curriculumServiceMock.Setup(gc => gc.GetInstructionalMethods2Async(It.IsAny<bool>())).Throws<Exception>();
                await instructionalMethodsController.GetInstructionalMethods2Async();
            }

            //GET

            [TestMethod]
            public async Task GetInstructionalMethodById2Async_Properties_and_cache()
            {
                var actualInstructionalMethod = await instructionalMethodsController.GetInstructionalMethodById2Async(instructionalMethodCode);
                Assert.AreEqual(expectedInstructionalMethod.Code, actualInstructionalMethod.Code);
                Assert.AreEqual(expectedInstructionalMethod.Title, actualInstructionalMethod.Title);
                Assert.AreEqual(expectedInstructionalMethod.Description, actualInstructionalMethod.Description);
                Assert.AreEqual(expectedInstructionalMethod.Id, actualInstructionalMethod.Id);
            }

            [TestMethod]
            public async Task GetInstructionalMethodById2Async_GetHedmAsync_CacheControlNull()
            {
                instructionalMethodsController.Request.Headers.CacheControl = null;
                var actualInstructionalMethods = await instructionalMethodsController.GetInstructionalMethodById2Async(instructionalMethodCode);
                Assert.IsNotNull(actualInstructionalMethods);
            }

            [TestMethod]
            public async Task GetInstructionalMethodById2Async_GetHedmAsync_Cache()
            {

                instructionalMethodsController.Request.Headers.CacheControl.NoCache = false;
                instructionalMethodsController.Request.Headers.CacheControl = null;
                var actualInstructionalMethods = await instructionalMethodsController.GetInstructionalMethodById2Async(instructionalMethodCode);
                Assert.IsNotNull(actualInstructionalMethods);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstructionalMethodById2Async_GetThrowsIntAppiExc()
            {
                curriculumServiceMock.Setup(gc => gc.GetInstructionalMethodById2Async(It.IsAny<string>())).Throws<Exception>();
                await instructionalMethodsController.GetInstructionalMethodById2Async(instructionalMethodCode);
            }

            //OTHER CRUD
            
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstrMethController_PostThrowsIntAppiExc()
            {
                InstructionalMethod2 imDTO = await instructionalMethodsController.PostInstructionalMethodsAsync(new InstructionalMethod2());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstrMethController_PutThrowsIntAppiExc()
            {
                InstructionalMethod2 imDTO = await instructionalMethodsController.PutInstructionalMethodsAsync(instructionalMethodCode, new InstructionalMethod2());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstrMethController_DeleteThrowsIntApiExc()
            {
                await instructionalMethodsController.DeleteInstructionalMethodsAsync(instructionalMethodCode);
            }

        }

    }
}
