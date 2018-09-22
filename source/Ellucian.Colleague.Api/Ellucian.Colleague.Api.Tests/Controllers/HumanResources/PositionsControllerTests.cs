/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PositionsControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IPositionRepository> positionRepositoryMock;
        public Mock<IAdapterRegistry> adapterRegistryMock;

        public TestPositionRepository positionBuilder;
        public PositionsController controllerUnderTest;

        public AutoMapperAdapter<Domain.HumanResources.Entities.Position, Dtos.HumanResources.Position> positionEntityToDtoAdapter;

        public FunctionEqualityComparer<Position> positionDtoComparer;

        public void PositionsControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            positionRepositoryMock = new Mock<IPositionRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            
            positionEntityToDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.Position,Position>(adapterRegistryMock.Object, loggerMock.Object);
            positionBuilder = new TestPositionRepository();

            positionRepositoryMock.Setup(r => r.GetPositionsAsync()).Returns(() =>
                positionBuilder.GetPositionsAsync());

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.HumanResources.Entities.Position, Dtos.HumanResources.Position>())
                .Returns(() => positionEntityToDtoAdapter);

            controllerUnderTest = new PositionsController(loggerMock.Object, adapterRegistryMock.Object, positionRepositoryMock.Object);

            positionDtoComparer = new FunctionEqualityComparer<Position>(
                (p1, p2) => p1.Id == p2.Id && p1.Title == p2.Title && p1.IsSalary == p2.IsSalary,
                (p) => p.Id.GetHashCode());
        }

        [TestClass]
        public class GetPositionsTests : PositionsControllerTests
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

            public async Task<List<Position>> getExpectedPositions()
            {
                return (await positionBuilder.GetPositionsAsync()).Select(pos => positionEntityToDtoAdapter.MapToType(pos)).ToList();
            }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                PositionsControllerTestsInitialize();                
            }

            [TestMethod]
            public async Task GetPositionsTest()
            {
                var actualPositions = await controllerUnderTest.GetPositionsAsync();
                CollectionAssert.AreEqual(await getExpectedPositions(), actualPositions.ToList(), positionDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                positionRepositoryMock.Setup(r => r.GetPositionsAsync()).Throws(new Exception());
                try
                {
                    await controllerUnderTest.GetPositionsAsync();
                }
                catch(HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }
    }
}
