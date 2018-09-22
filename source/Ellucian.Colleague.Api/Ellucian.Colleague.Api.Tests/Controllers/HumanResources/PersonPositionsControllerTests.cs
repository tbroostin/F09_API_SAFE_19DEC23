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
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PersonPositionsControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IPersonPositionService> personPositionServiceMock;

        public PersonPositionsController controllerUnderTest;      

        public FunctionEqualityComparer<PersonPosition> personPositionDtoComparer;

        public void PersonPositionsControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            personPositionServiceMock = new Mock<IPersonPositionService>();

            personPositionDtoComparer = new FunctionEqualityComparer<PersonPosition>(
                (p1, p2) => p1.Id == p2.Id && p1.PersonId == p2.PersonId && p1.PositionId == p2.PositionId,
                (p) => p.Id.GetHashCode());

            controllerUnderTest = new PersonPositionsController(loggerMock.Object, personPositionServiceMock.Object);
        }

        [TestClass]
        public class GetPersonPositionsTests : PersonPositionsControllerTests
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

            public async Task<List<PersonPosition>> getExpectedPositions()
            {
                return await Task.FromResult(new List<PersonPosition>() {
                    new PersonPosition() {
                        Id = "foo",
                        PersonId = "0003914",
                        PositionId = "ZAM1234BUSIC1234",
                        StartDate = new DateTime(2010, 1, 1)
                    }
                });
            }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                PersonPositionsControllerTestsInitialize();

                personPositionServiceMock.Setup(s => s.GetPersonPositionsAsync(null))
                    .Returns(async () => await getExpectedPositions());
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = await getExpectedPositions();
                var actual = await controllerUnderTest.GetPersonPositionsAsync();

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), personPositionDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                personPositionServiceMock.Setup(s => s.GetPersonPositionsAsync(null)).Throws(new Exception());

                try
                {
                    await controllerUnderTest.GetPersonPositionsAsync();
                }
                catch(HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }

        }

    }
}
