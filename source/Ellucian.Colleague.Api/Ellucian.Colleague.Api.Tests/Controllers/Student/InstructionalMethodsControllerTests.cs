// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class InstructionalMethodsControllerTests
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

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private Mock<ICurriculumService> curriculumServiceMock;
        private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

        private string instructionalMethodId;

        private InstructionalMethod2 expectedInstructionalMethod;
        private InstructionalMethod2 testInstructionalMethod;
        private InstructionalMethod2 actualInstructionalMethod;

        private InstructionalMethodsController instructionalMethodsController;


        public async Task<List<InstructionalMethod2>> getActualInstructionalMethods()
        {
            IEnumerable<InstructionalMethod2> instructionalMethodList = await instructionalMethodsController.GetInstructionalMethods2Async();
            return (await instructionalMethodsController.GetInstructionalMethods2Async()).ToList();
        }

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            curriculumServiceMock = new Mock<ICurriculumService>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

            instructionalMethodId = "idc2935b-29e8-675f-907b-15a34da4f433";

            expectedInstructionalMethod = new InstructionalMethod2()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Code = "AAAA",
                Title = "Academic Administration",
                Description = null,
            };

            testInstructionalMethod = new InstructionalMethod2();
            foreach (var property in typeof(InstructionalMethod2).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testInstructionalMethod, property.GetValue(expectedInstructionalMethod, null), null);
            }
            curriculumServiceMock.Setup<Task<InstructionalMethod2>>(s => s.GetInstructionalMethodById2Async(instructionalMethodId)).Returns(Task.FromResult(testInstructionalMethod));

            instructionalMethodsController = new InstructionalMethodsController(adapterRegistryMock.Object, studentReferenceDataRepositoryMock.Object, curriculumServiceMock.Object, loggerMock.Object);
            actualInstructionalMethod = await instructionalMethodsController.GetInstructionalMethodById2Async(instructionalMethodId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            curriculumServiceMock = null;
            studentReferenceDataRepositoryMock = null;
            instructionalMethodId = null;
            expectedInstructionalMethod = null;
            testInstructionalMethod = null;
            actualInstructionalMethod = null;
            instructionalMethodsController = null;
        }

        //[TestMethod]
        //public void InstructionalMethodsTypeTest()
        //{
        //    Assert.AreEqual(typeof(InstructionalMethod2), actualInstructionalMethod.GetType());
        //    Assert.AreEqual(expectedInstructionalMethod.GetType(), actualInstructionalMethod.GetType());
        //}

        [TestMethod]
        public void NumberOfKnownPropertiesTest()
        {
            var instructionalMethodProperties = typeof(InstructionalMethod2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(5, instructionalMethodProperties.Length);
        }
    }

    [TestClass]
    public class InstructionalMethodsController_GetAllTests
    {
        private TestContext testContextInstance2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance2;
            }
            set
            {
                testContextInstance2 = value;
            }
        }

        private InstructionalMethodsController InstructionalMethodController;
        private Mock<ICurriculumService> CurriculumServiceMock;
        private Mock<IStudentReferenceDataRepository> StudentReferenceDataRepositoryMock;
        private ICurriculumService CurriculumService;
        private IAdapterRegistry AdapterRegistry;
        private List<Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod> allInstructionalMethods;
        ILogger logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            CurriculumServiceMock = new Mock<ICurriculumService>();
            CurriculumService = CurriculumServiceMock.Object;

            StudentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod, InstructionalMethod2>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            allInstructionalMethods = new TestStudentReferenceDataRepository().GetInstructionalMethodsAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod>;
            var InstructionalMethodsList = new List<InstructionalMethod2>();

            InstructionalMethodController = new InstructionalMethodsController(AdapterRegistry, StudentReferenceDataRepositoryMock.Object, CurriculumService, logger);
            InstructionalMethodController.Request = new HttpRequestMessage();
            InstructionalMethodController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var instructionalMethod in allInstructionalMethods)
            {
                InstructionalMethod2 target = ConvertOtherMajorsEntitytoInstructionalMethodDto(instructionalMethod);
                InstructionalMethodsList.Add(target);
            }

            CurriculumServiceMock.Setup<Task<IEnumerable<InstructionalMethod2>>>(s => s.GetInstructionalMethods2Async(It.IsAny<bool>())).ReturnsAsync(InstructionalMethodsList);
        }

        [TestCleanup]
        public void Cleanup()
        {
            InstructionalMethodController = null;
            CurriculumService = null;
        }

        [TestMethod]
        public async Task ReturnsAllInstructionalMethods()
        {
            List<InstructionalMethod2> InstructionalMethods = await InstructionalMethodController.GetInstructionalMethods2Async() as List<InstructionalMethod2>;
            Assert.AreEqual(InstructionalMethods.Count, allInstructionalMethods.Count);
        }

        [TestMethod]
        public async Task GetInstructionalMethods_Properties()
        {
            List<InstructionalMethod2> InstructionalMethods = await InstructionalMethodController.GetInstructionalMethods2Async() as List<InstructionalMethod2>;
            InstructionalMethod2 al = InstructionalMethods.Where(a => a.Code == "02").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod alt = allInstructionalMethods.Where(a => a.Code == "02").FirstOrDefault();
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        [TestMethod]
        public async Task InstrMethController_GetHedmAsync_CacheControlNotNull()
        {
            InstructionalMethodController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            List<InstructionalMethod2> InstructionalMethods = await InstructionalMethodController.GetInstructionalMethods2Async() as List<InstructionalMethod2>;
            InstructionalMethod2 al = InstructionalMethods.Where(a => a.Code == "02").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod alt = allInstructionalMethods.Where(a => a.Code == "02").FirstOrDefault();
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        [TestMethod]
        public async Task InstrMethController_GetHedmAsync_NoCache()
        {
            InstructionalMethodController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            InstructionalMethodController.Request.Headers.CacheControl.NoCache = true;

            List<InstructionalMethod2> InstructionalMethods = await InstructionalMethodController.GetInstructionalMethods2Async() as List<InstructionalMethod2>;
            InstructionalMethod2 al = InstructionalMethods.Where(a => a.Code == "02").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod alt = allInstructionalMethods.Where(a => a.Code == "02").FirstOrDefault();
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        [TestMethod]
        public async Task InstrMethController_GetHedmAsync_Cache()
        {
            InstructionalMethodController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            InstructionalMethodController.Request.Headers.CacheControl.NoCache = false;

            List<InstructionalMethod2> InstructionalMethods = await InstructionalMethodController.GetInstructionalMethods2Async() as List<InstructionalMethod2>;
            InstructionalMethod2 al = InstructionalMethods.Where(a => a.Code == "02").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod alt = allInstructionalMethods.Where(a => a.Code == "02").FirstOrDefault();
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstrMethController_GetThrowsIntAppiExc()
        {
            CurriculumServiceMock.Setup(gc => gc.GetInstructionalMethods2Async(It.IsAny<bool>())).Throws<Exception>();

            await InstructionalMethodController.GetInstructionalMethods2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstrMethController_GetByIdThrowsIntAppiExc()
        {
            CurriculumServiceMock.Setup(gc => gc.GetInstructionalMethodById2Async(It.IsAny<string>())).Throws<Exception>();

            await InstructionalMethodController.GetInstructionalMethodById2Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstrMethController_DeleteThrowsIntApiExc()
        {
            await InstructionalMethodController.DeleteInstructionalMethodsAsync("SEC100");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstrMethController_PostThrowsIntAppiExc()
        {
            InstructionalMethod2 imDTO = await InstructionalMethodController.PostInstructionalMethodsAsync(new InstructionalMethod2());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstrMethController_PutThrowsIntAppiExc()
        {
            InstructionalMethod2 imDTO = await InstructionalMethodController.PutInstructionalMethodsAsync("hdgs9093hf", new InstructionalMethod2());
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a InstructionalMethod domain entity to its corresponding Instructional Method DTO
        /// </summary>
        /// <param name="source">Instructional Method domain entity</param>
        /// <returns>InstructionalMethod2 DTO</returns>
        private Dtos.InstructionalMethod2 ConvertOtherMajorsEntitytoInstructionalMethodDto(Domain.Student.Entities.InstructionalMethod source)
        {
            var instructionalMethod = new Dtos.InstructionalMethod2();
            instructionalMethod.Id = source.Guid;
            instructionalMethod.Code = source.Code;
            instructionalMethod.Title = source.Description;
            instructionalMethod.Description = null;
            return instructionalMethod;
        }
    }
}
