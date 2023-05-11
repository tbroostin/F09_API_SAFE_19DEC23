using Ellucian.Colleague.Api.Controllers.TimeManagement;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Dtos.TimeManagement;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.TimeManagement
{
    [TestClass]
    public class TimeEntryCommentsControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<ITimeEntryCommentsService> commentsService;
        public TimeEntryCommentsController commentsController;
        public List<TimeEntryComments> commentsDtos;
        public FunctionEqualityComparer<TimeEntryComments> commentsDtoComparer;

        public void TimeEntryCommentsTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            commentsService = new Mock<ITimeEntryCommentsService>();
            commentsDtoComparer = new FunctionEqualityComparer<TimeEntryComments>(
                (c1, c2) => 
                    c1.Id == c2.Id &&
                    c1.EmployeeId == c2.EmployeeId &&
                    c1.PayCycleId == c2.PayCycleId &&
                    c1.PayPeriodEndDate == c2.PayPeriodEndDate &&
                    c1.TimecardId == c2.TimecardId &&
                    c1.TimecardStatusId == c2.TimecardStatusId &&
                    c1.Comments == c2.Comments, 
                    c => c.Id.GetHashCode()
            );
            commentsDtos = new List<TimeEntryComments>()
            {
                new TimeEntryComments()
                {
                    Id = "001",
                    EmployeeId = "24601",
                    Comments = "api intgrations starting",
                    PayCycleId = "A01",
                    TimecardId = "123",
                    PayPeriodEndDate = DateTime.UtcNow,
                    TimecardStatusId = "43",
                    CommentsTimestamp = new Dtos.Base.Timestamp()
                },
                new TimeEntryComments()
                {
                    Id = "001",
                    EmployeeId = "24601",
                    Comments = "ipa antgrations stirring",
                    PayCycleId = "A01",
                    TimecardId = "234",
                    PayPeriodEndDate = DateTime.UtcNow,
                    TimecardStatusId = "34",
                    CommentsTimestamp = new Dtos.Base.Timestamp()
                },
            };
            commentsController = new TimeEntryCommentsController(loggerMock.Object, commentsService.Object);
        }

        [TestClass]
        public class GetCommentsTests : TimeEntryCommentsControllerTests
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

            [TestInitialize]
            public void Initialize()
            {
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                base.TimeEntryCommentsTestsInitialize();
                commentsService.Setup(cs => cs.GetCommentsAsync())
                    .ReturnsAsync(commentsDtos);
            }

            [TestMethod]
            public async Task CreatedStatusIsReturnedTest()
            {
                var expected = commentsDtos;
                var actual = await commentsController.GetTimeEntryCommentsAsync();
                CollectionAssert.AreEqual(expected.ToList(),actual.ToList(), commentsDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionExceptionTest()
            {
                commentsService.Setup(cs => cs.GetCommentsAsync()).Throws(new PermissionsException());
                try
                {
                    await commentsController.GetTimeEntryCommentsAsync();
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(err => err.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(hre.Response.StatusCode, HttpStatusCode.Forbidden);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExcpetionTest()
            {
                commentsService.Setup(cs => cs.GetCommentsAsync()).Throws(new Exception());
                try
                {
                    await commentsController.GetTimeEntryCommentsAsync();
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(err => err.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(hre.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }
        }

        [TestClass]
        public class CreateCommentsTests : TimeEntryCommentsControllerTests
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

            [TestInitialize]
            public void Initialize()
            {
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                base.TimeEntryCommentsTestsInitialize();
                commentsService.Setup(cs => cs.CreateCommentsAsync(It.IsAny<TimeEntryComments>()))
                    .Returns<TimeEntryComments>(com => Task.FromResult(com));
            }

            [TestMethod]
            public async Task CreatedStatusIsReturnedTest()
            {
                var expected = commentsDtos[0];
                var actual = await commentsController.CreateTimeEntryCommentsAsync(commentsDtos[0]);
                Assert.AreEqual(0, commentsDtoComparer.Compare(expected,actual));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionExceptionTest()
            {
                commentsService.Setup(cs => cs.CreateCommentsAsync(It.IsAny<TimeEntryComments>()))
                    .Throws(new PermissionsException());

                try
                {
                    await commentsController.CreateTimeEntryCommentsAsync(commentsDtos[0]);
                }
                catch(HttpResponseException hre)
                {
                    loggerMock.Verify(err => err.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(hre.Response.StatusCode, HttpStatusCode.Forbidden); 
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExcpetionTest()
            {
                commentsService.Setup(cs => cs.CreateCommentsAsync(It.IsAny<TimeEntryComments>()))
                    .Throws(new Exception());
                try
                {
                    await commentsController.CreateTimeEntryCommentsAsync(commentsDtos[0]);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(err => err.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(hre.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }
        }
    }
}
