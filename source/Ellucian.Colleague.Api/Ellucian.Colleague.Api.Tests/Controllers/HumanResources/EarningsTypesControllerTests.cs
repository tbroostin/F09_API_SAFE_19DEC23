/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EarningsTypesControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IEarningsTypeRepository> earningsTypeRepositoryMock;
        public Mock<IEarningTypesService> earningsTypeServiceMock;
        public Mock<IAdapterRegistry> adapterRegistryMock;

        public TestEarningsTypeRepository earningsTypeBuilder;
        public EarningTypesController controllerUnderTest;

        public AutoMapperAdapter<Domain.HumanResources.Entities.EarningsType, Dtos.HumanResources.EarningsType> earningsTypeEntityToDtoAdapter;

        public FunctionEqualityComparer<EarningsType> earningsTypeDtoComparer;

        public void EarningsTypesControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            earningsTypeRepositoryMock = new Mock<IEarningsTypeRepository>();
            earningsTypeServiceMock = new Mock<IEarningTypesService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            earningsTypeEntityToDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.EarningsType, EarningsType>(adapterRegistryMock.Object, loggerMock.Object);
            earningsTypeBuilder = new TestEarningsTypeRepository();

            earningsTypeRepositoryMock.Setup(r => r.GetEarningsTypesAsync()).Returns(() =>
                earningsTypeBuilder.GetEarningsTypesAsync());

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.HumanResources.Entities.EarningsType, Dtos.HumanResources.EarningsType>())
                .Returns(() => earningsTypeEntityToDtoAdapter);

            controllerUnderTest = new EarningTypesController(loggerMock.Object, adapterRegistryMock.Object, earningsTypeRepositoryMock.Object, earningsTypeServiceMock.Object);

            earningsTypeDtoComparer = new FunctionEqualityComparer<EarningsType>(
                (et1, et2) => et1.Id == et2.Id && et1.Description == et2.Description && et1.IsActive == et2.IsActive,
                (et) => et.Id.GetHashCode());
        }

        [TestClass]
        public class GetEarningsTypesTests : EarningsTypesControllerTests
        {
            #region Test Context
            private TestContext testContextInstance;
            
            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
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

            public async Task<List<EarningsType>> getExpectedEarningsTypes()
            {
                return (await earningsTypeBuilder.GetEarningsTypesAsync()).Select(et => earningsTypeEntityToDtoAdapter.MapToType(et)).ToList();
            }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                EarningsTypesControllerTestsInitialize();
            }

            [TestMethod]
            public async Task GetEarningTypesTest()
            {
                var actualEarningsTypes = await controllerUnderTest.GetEarningsTypesAsync();
                CollectionAssert.AreEqual(await getExpectedEarningsTypes(), actualEarningsTypes.ToList(), earningsTypeDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                earningsTypeRepositoryMock.Setup(r => r.GetEarningsTypesAsync()).Throws(new Exception());
                try
                {
                    await controllerUnderTest.GetEarningsTypesAsync();
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
}
