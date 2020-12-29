/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
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
    public class PersonEmploymentStatusesControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IPersonEmploymentStatusService> personEmploymentStatusServiceMock;

        public PersonEmploymentStatusesController controllerUnderTest;

        public FunctionEqualityComparer<PersonEmploymentStatus> personEmploymentStatusDtoComparer;

        public void PersonEmploymentStatusesControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            personEmploymentStatusServiceMock = new Mock<IPersonEmploymentStatusService>();

            personEmploymentStatusDtoComparer = new FunctionEqualityComparer<PersonEmploymentStatus>(
                (p1, p2) =>
                    p1.Id == p2.Id &&
                    p1.PersonId == p2.PersonId &&
                    p1.PersonPositionId == p2.PersonPositionId &&
                    p1.PrimaryPositionId == p2.PrimaryPositionId &&
                    p1.StartDate == p2.StartDate &&
                    p1.EndDate == p2.EndDate,
                (p) => p.Id.GetHashCode());

            controllerUnderTest = new PersonEmploymentStatusesController(loggerMock.Object, personEmploymentStatusServiceMock.Object);
        }

        [TestClass]
        public class GetPersonEmploymentStatusesTests : PersonEmploymentStatusesControllerTests
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

            public async Task<List<PersonEmploymentStatus>> getExpectedEmploymentStatuses()
            {
                return await Task.FromResult(new List<PersonEmploymentStatus>() {
                    new PersonEmploymentStatus() {
                        Id = "001",
                        PersonId = "24601",
                        PersonPositionId = "1987",
                        PrimaryPositionId = "MANAFACTURER",
                        StartDate = new DateTime(1800,1,1),
                        EndDate = null
                    }
                });
            }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                PersonEmploymentStatusesControllerTestsInitialize();

                personEmploymentStatusServiceMock.Setup(s => s.GetPersonEmploymentStatusesAsync(null, null))
                    .Returns(async () => await getExpectedEmploymentStatuses());
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = await getExpectedEmploymentStatuses();
                var actual = await controllerUnderTest.GetPersonEmploymentStatusesAsync();

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), personEmploymentStatusDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                personEmploymentStatusServiceMock.Setup(s => s.GetPersonEmploymentStatusesAsync(null, null)).Throws(new Exception());

                try
                {
                    await controllerUnderTest.GetPersonEmploymentStatusesAsync();
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }

        }

    }
}
