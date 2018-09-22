/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class HumanResourcesControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IHumanResourceDemographicsService> humanResourceDemographicsServiceMock;
        public Mock<IAdapterRegistry> adapterRegistryMock;

        public HumanResourcesController controllerUnderTest;

        public PersonBaseEntityToHumanResourceDemographicsDtoAdapter personBaseEntityToHumanResourceDemographicsDtoAdapter;

        public TestPersonBaseRepository testPersonBaseRepository;

        public FunctionEqualityComparer<HumanResourceDemographics> humanResourceDemographicsDtoComparer;

        public void HumanResourcesControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            humanResourceDemographicsServiceMock = new Mock<IHumanResourceDemographicsService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            personBaseEntityToHumanResourceDemographicsDtoAdapter = new PersonBaseEntityToHumanResourceDemographicsDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            testPersonBaseRepository = new TestPersonBaseRepository();

            //mock up the Service
            humanResourceDemographicsServiceMock.Setup(r => r.GetHumanResourceDemographicsAsync(null))
                 .ReturnsAsync(testPersonBaseRepository.GetPersonBaseEntities().Select(x => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(x)).ToList());
            humanResourceDemographicsServiceMock.Setup(r => r.GetSpecificHumanResourceDemographicsAsync(It.IsAny<string>()))
                 .ReturnsAsync(testPersonBaseRepository.GetPersonBaseEntities().Select(x => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(x)).FirstOrDefault());
            humanResourceDemographicsServiceMock.Setup(r => r.QueryHumanResourceDemographicsAsync(It.IsAny<Dtos.Base.HumanResourceDemographicsQueryCriteria>(), It.IsAny<string>()))
                 .ReturnsAsync(testPersonBaseRepository.GetPersonBaseEntities().Select(x => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(x)).ToList());

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>())
            .Returns(() => (ITypeAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>)personBaseEntityToHumanResourceDemographicsDtoAdapter);
            controllerUnderTest = new HumanResourcesController(loggerMock.Object, adapterRegistryMock.Object, humanResourceDemographicsServiceMock.Object);

            humanResourceDemographicsDtoComparer = new FunctionEqualityComparer<HumanResourceDemographics>(
                   (hrd1, hrd2) => hrd1.Id == hrd2.Id && hrd1.FirstName == hrd2.FirstName && hrd1.LastName == hrd2.LastName,
                   (hrd) => hrd.Id.GetHashCode());
        }
    }

    [TestClass]
    public class GetHumanResourceDemographicsTests : HumanResourcesControllerTests
    {
        #region Test Context
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// informatin about and functionality for the current test run.
        /// </summary>
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
        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            HumanResourcesControllerTestsInitialize();
        }
        public List<HumanResourceDemographics> getExpectedHumanResourceDemographics()
        {
            return testPersonBaseRepository.GetPersonBaseEntities().Select(pbe => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(pbe)).ToList();
        }

        [TestMethod]
        public void ExpectedEqualsActualTest()
        {
            var expected = getExpectedHumanResourceDemographics();
            var actual = controllerUnderTest.GetHumanResourceDemographicsAsync().Result.ToList();
            CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), humanResourceDemographicsDtoComparer);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CatchPermissionsExceptionTest()
        {
            humanResourceDemographicsServiceMock.Setup(r => r.GetHumanResourceDemographicsAsync(null)).Throws(new PermissionsException());
            try
            {
                await controllerUnderTest.GetHumanResourceDemographicsAsync();
            }
            catch (HttpResponseException pe)
            {
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                Assert.AreEqual(HttpStatusCode.Forbidden, pe.Response.StatusCode);
                throw pe;
            }
        }
    }
    [TestClass]
    public class GetSpecificHumanResourceDemographicsTests : HumanResourcesControllerTests
    {
        #region Test Context
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// informatin about and functionality for the current test run.
        /// </summary>
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
        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            HumanResourcesControllerTestsInitialize();
        }
        public HumanResourceDemographics getExpectedSpecificHumanResourceDemographics()
        {
            return testPersonBaseRepository.GetPersonBaseEntities().Select(pbe => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(pbe)).FirstOrDefault();
        }
        [TestMethod]
        public void ExpectedEqualsActualTest()
        {
            var expected = getExpectedSpecificHumanResourceDemographics();
            var actual = controllerUnderTest.GetSpecificHumanResourceDemographicsAsync("111").Result;
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.FirstName, actual.FirstName);
            Assert.AreEqual(expected.LastName, actual.LastName);
            Assert.AreEqual(expected.PreferredName, actual.PreferredName);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CatchPermissionsExceptionTest()
        {
            humanResourceDemographicsServiceMock.Setup(r => r.GetSpecificHumanResourceDemographicsAsync(It.IsAny<string>())).Throws(new PermissionsException());
            try
            {
                await controllerUnderTest.GetSpecificHumanResourceDemographicsAsync("555");
            }
            catch (HttpResponseException pe)
            {
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                Assert.AreEqual(HttpStatusCode.Forbidden, pe.Response.StatusCode);
                throw pe;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CatchNotFoundExceptionTest()
        {
            humanResourceDemographicsServiceMock.Setup(r => r.GetSpecificHumanResourceDemographicsAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
            try
            {
                await controllerUnderTest.GetSpecificHumanResourceDemographicsAsync("111");
            }
            catch (HttpResponseException pe)
            {
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                Assert.AreEqual(HttpStatusCode.NotFound, pe.Response.StatusCode);
                throw;
            }
        }
    }

    [TestClass]
    public class QueryHumanResourceDemographicsAsyncTests : HumanResourcesControllerTests
    {
        #region Test Context
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// informatin about and functionality for the current test run.
        /// </summary>
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
        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            HumanResourcesControllerTestsInitialize();
        }

        public HumanResourceDemographics querySpecificHumanResourceDemographics()
        {
            return testPersonBaseRepository.GetPersonBaseEntities().Select(pbe => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(pbe)).FirstOrDefault();
        }

        [TestMethod]
        public void UserGettingOwnData()
        {
            var expected = querySpecificHumanResourceDemographics();

            Dtos.Base.HumanResourceDemographicsQueryCriteria hrCriteria = new Dtos.Base.HumanResourceDemographicsQueryCriteria
            {
                Ids = new List<string>() {"111" }
            };

            var actual = controllerUnderTest.QueryHumanResourceDemographicsAsync(hrCriteria, "111").Result.ToList();
            Assert.AreEqual(expected.Id, actual[0].Id);
            Assert.AreEqual(expected.FirstName, actual[0].FirstName);
            Assert.AreEqual(expected.LastName, actual[0].LastName);
            Assert.AreEqual(expected.PreferredName, actual[0].PreferredName);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CatchPermissionsExceptionTest()
        {
            humanResourceDemographicsServiceMock.Setup(r => r.QueryHumanResourceDemographicsAsync(It.IsAny<Dtos.Base.HumanResourceDemographicsQueryCriteria>(), It.IsAny<string>())).Throws(new PermissionsException());
            try
            {
                Dtos.Base.HumanResourceDemographicsQueryCriteria hrCriteria = new Dtos.Base.HumanResourceDemographicsQueryCriteria
                {
                    Ids = new List<string>() { "555" }
                };
                await controllerUnderTest.QueryHumanResourceDemographicsAsync(hrCriteria, "111");
            }
            catch (HttpResponseException pe)
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, pe.Response.StatusCode);
                throw pe;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CatchNotFoundExceptionTest()
        {
            humanResourceDemographicsServiceMock.Setup(r => r.QueryHumanResourceDemographicsAsync(It.IsAny<Dtos.Base.HumanResourceDemographicsQueryCriteria>(), It.IsAny<string>())).Throws(new Exception());
            try
            {
                Dtos.Base.HumanResourceDemographicsQueryCriteria hrCriteria = new Dtos.Base.HumanResourceDemographicsQueryCriteria
                {
                    Ids = new List<string>() { "555" }
                };
                await controllerUnderTest.QueryHumanResourceDemographicsAsync(hrCriteria, null);
            }
            catch (HttpResponseException pe)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, pe.Response.StatusCode);
                throw pe;
            }
        }
    }
}

