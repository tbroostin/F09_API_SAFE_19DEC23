/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PayCyclesControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IPayCycleService> payCycleRepositoryMock;
        public Mock<IAdapterRegistry> adapterRegistryMock;

        public TestPayCycleRepository testPayCycleRepository;
        public PayCyclesController controllerUnderTest;

        public PayCycleEntityToDtoAdapter payCycleEntityToDtoAdapter;

        public FunctionEqualityComparer<PayCycle> payCycleDtoComparer;

        public void PayCyclesControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            payCycleRepositoryMock = new Mock<IPayCycleService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            payCycleEntityToDtoAdapter = new PayCycleEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            testPayCycleRepository = new TestPayCycleRepository();

            //payCycleRepositoryMock.Setup(r => r.GetPayCyclesAsync()).Returns(() =>
            //    testPayCycleRepository.GetPayCyclesAsync());

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.HumanResources.Entities.PayCycle, Dtos.HumanResources.PayCycle>())
                .Returns(() => (ITypeAdapter<Domain.HumanResources.Entities.PayCycle, Dtos.HumanResources.PayCycle>)payCycleEntityToDtoAdapter);

            controllerUnderTest = new PayCyclesController(loggerMock.Object, adapterRegistryMock.Object, payCycleRepositoryMock.Object);

            payCycleDtoComparer = new FunctionEqualityComparer<PayCycle>(
                (pc1, pc2) => pc1.Id == pc2.Id && pc1.Description == pc2.Description,
                (pc) => pc.Id.GetHashCode());
        }

        [TestClass]
        public class GetPayCyclesTests : PayCyclesControllerTests
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

            public async Task<List<PayCycle>> getExpectedPayCycles()
            {
                return (await testPayCycleRepository.GetPayCyclesAsync()).Select(pc => payCycleEntityToDtoAdapter.MapToType(pc)).ToList();
            }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                PayCyclesControllerTestsInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                //var expected = await getExpectedPayCycles();
                //var actualPayCycles = await controllerUnderTest.GetPayCyclesAsync();
                //CollectionAssert.AreEqual(expected.ToArray(), actualPayCycles.ToArray(), payCycleDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                payCycleRepositoryMock.Setup(r => r.GetPayCyclesAsync()).Throws(new Exception());
                try
                {
                    await controllerUnderTest.GetPayCyclesAsync();
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw hre;
                }
            }
        }
    }

    [TestClass]
    public class PayCycles2ControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPayCycleService> payCyclesServiceMock;
        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterMock;
        private PayCyclesController payCyclesController;
        private IEnumerable<Domain.HumanResources.Entities.PayCycle2> allPaycycle;
        private List<Dtos.PayCycles> payCyclesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            payCyclesServiceMock = new Mock<IPayCycleService>();
            loggerMock = new Mock<ILogger>();
            adapterMock = new Mock<IAdapterRegistry>();
            payCyclesCollection = new List<Dtos.PayCycles>();

            allPaycycle = new List<Domain.HumanResources.Entities.PayCycle2>()
                {
                    new Domain.HumanResources.Entities.PayCycle2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.PayCycle2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.PayCycle2("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allPaycycle)
            {
                var payCycles = new Ellucian.Colleague.Dtos.PayCycles
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                payCyclesCollection.Add(payCycles);
            }

            payCyclesController = new PayCyclesController(loggerMock.Object, adapterMock.Object, payCyclesServiceMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            payCyclesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            payCyclesController = null;
            allPaycycle = null;
            payCyclesCollection = null;
            loggerMock = null;
            payCyclesServiceMock = null;
        }

        [TestMethod]
        public async Task PayCyclesController_GetPayCycles_ValidateFields_Nocache()
        {
            payCyclesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            payCyclesServiceMock.Setup(x => x.GetPayCyclesAsync(false)).ReturnsAsync(payCyclesCollection);

            var sourceContexts = (await payCyclesController.GetPayCycles2Async()).ToList();
            Assert.AreEqual(payCyclesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payCyclesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task PayCyclesController_GetPayCycles_ValidateFields_Cache()
        {
            payCyclesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            payCyclesServiceMock.Setup(x => x.GetPayCyclesAsync(true)).ReturnsAsync(payCyclesCollection);

            var sourceContexts = (await payCyclesController.GetPayCycles2Async()).ToList();
            Assert.AreEqual(payCyclesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payCyclesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCycles_KeyNotFoundException()
        {
            //
            payCyclesServiceMock.Setup(x => x.GetPayCyclesAsync(false))
                .Throws<KeyNotFoundException>();
            await payCyclesController.GetPayCycles2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCycles_PermissionsException()
        {

            payCyclesServiceMock.Setup(x => x.GetPayCyclesAsync(false))
                .Throws<PermissionsException>();
            await payCyclesController.GetPayCycles2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCycles_ArgumentException()
        {

            payCyclesServiceMock.Setup(x => x.GetPayCyclesAsync(false))
                .Throws<ArgumentException>();
            await payCyclesController.GetPayCycles2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCycles_RepositoryException()
        {

            payCyclesServiceMock.Setup(x => x.GetPayCyclesAsync(false))
                .Throws<RepositoryException>();
            await payCyclesController.GetPayCycles2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCycles_IntegrationApiException()
        {

            payCyclesServiceMock.Setup(x => x.GetPayCyclesAsync(false))
                .Throws<IntegrationApiException>();
            await payCyclesController.GetPayCycles2Async();
        }

        [TestMethod]
        public async Task PayCyclesController_GetPayCyclesByGuidAsync_ValidateFields()
        {
            var expected = payCyclesCollection.FirstOrDefault();
            payCyclesServiceMock.Setup(x => x.GetPayCyclesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await payCyclesController.GetPayCyclesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCycles_Exception()
        {
            payCyclesServiceMock.Setup(x => x.GetPayCyclesAsync(false)).Throws<Exception>();
            await payCyclesController.GetPayCycles2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCyclesByGuidAsync_Exception()
        {
            payCyclesServiceMock.Setup(x => x.GetPayCyclesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await payCyclesController.GetPayCyclesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCyclesByGuid_KeyNotFoundException()
        {
            payCyclesServiceMock.Setup(x => x.GetPayCyclesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await payCyclesController.GetPayCyclesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCyclesByGuid_PermissionsException()
        {
            payCyclesServiceMock.Setup(x => x.GetPayCyclesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await payCyclesController.GetPayCyclesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCyclesByGuid_ArgumentException()
        {
            payCyclesServiceMock.Setup(x => x.GetPayCyclesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await payCyclesController.GetPayCyclesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCyclesByGuid_RepositoryException()
        {
            payCyclesServiceMock.Setup(x => x.GetPayCyclesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await payCyclesController.GetPayCyclesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCyclesByGuid_IntegrationApiException()
        {
            payCyclesServiceMock.Setup(x => x.GetPayCyclesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await payCyclesController.GetPayCyclesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_GetPayCyclesByGuid_Exception()
        {
            payCyclesServiceMock.Setup(x => x.GetPayCyclesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await payCyclesController.GetPayCyclesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_PostPayCyclesAsync_Exception()
        {
            await payCyclesController.PostPayCyclesAsync(payCyclesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_PutPayCyclesAsync_Exception()
        {
            var sourceContext = payCyclesCollection.FirstOrDefault();
            await payCyclesController.PutPayCyclesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayCyclesController_DeletePayCyclesAsync_Exception()
        {
            await payCyclesController.DeletePayCyclesAsync(payCyclesCollection.FirstOrDefault().Id);
        }
    }
}
