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
    public class PersonPositionWagesControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IPersonPositionWageService> personPositionWageServiceMock;

        public PersonPositionWagesController controllerUnderTest;

        public FunctionEqualityComparer<PersonPositionWage> personPositionWageDtoComparer;
 
        public void PersonPositionWagesControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            personPositionWageServiceMock = new Mock<IPersonPositionWageService>();

            personPositionWageDtoComparer = new FunctionEqualityComparer<PersonPositionWage>(
                (p1, p2) => p1.Id == p2.Id && p1.PersonId == p2.PersonId && p1.PositionId == p2.PositionId,
                (p) => p.Id.GetHashCode());

            controllerUnderTest = new PersonPositionWagesController(loggerMock.Object, personPositionWageServiceMock.Object);
        }

        [TestClass]
        public class GetPersonPositionWagesTests : PersonPositionWagesControllerTests
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

            public async Task<List<PersonPositionWage>> getExpectedPersonPositionWages()
            {
                return await Task.FromResult(new List<PersonPositionWage>() {
                    new PersonPositionWage() {
                        Id = "foo",
                        PersonId = "0003914",
                        PositionId = "MUSICPROF12345",
                        PersonPositionId = "54321",
                        PayClassId = "BM",
                        PayCycleId = "CM",
                        PositionPayDefaultId = "12345",
                        RegularWorkEarningsTypeId = "REG",
                        StartDate = new DateTime(2010, 1, 1),
                        EndDate = null,
                        IsPaySuspended = false,
                        FundingSources = new List<PositionFundingSource>()
                        {
                            new PositionFundingSource() {FundingSourceId = "FUND", FundingOrder = 0, ProjectId = "PROJ1"}
                        }
                    }
                });
            }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                PersonPositionWagesControllerTestsInitialize();

                personPositionWageServiceMock.Setup(s => s.GetPersonPositionWagesAsync(null, null))
                    .Returns(async () => await getExpectedPersonPositionWages());
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = await getExpectedPersonPositionWages();
                var actual = await controllerUnderTest.GetPersonPositionWagesAsync();

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), personPositionWageDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                personPositionWageServiceMock.Setup(s => s.GetPersonPositionWagesAsync(null, null)).Throws(new Exception());

                try
                {
                    await controllerUnderTest.GetPersonPositionWagesAsync();
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
