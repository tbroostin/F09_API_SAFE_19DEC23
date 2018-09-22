// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class WorkTasksControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IWorkTaskService> workTaskServiceMock;
        public WorkTasksController controllerUnderTest;
        public Collection<WorkTask> workTaskDto;
        public string personId;

        public void WorkTasksControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            workTaskServiceMock = new Mock<IWorkTaskService>();

            controllerUnderTest = new WorkTasksController(workTaskServiceMock.Object, loggerMock.Object);

        }

        [TestClass]
        public class GetAsyncTests : WorkTasksControllerTests
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
                personId = "0000123";
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                base.WorkTasksControllerTestsInitialize();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAsyncNoPersonId_ReturnsHttpBadRequest()
            {
                try
                {
                    await controllerUnderTest.GetAsync(null);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAsync_ServiceException_ReturnsHttpBadRequest()
            {
                workTaskServiceMock.Setup(s => s.GetAsync(personId))
                    .Throws(new Exception());

                try
                {
                    await controllerUnderTest.GetAsync(personId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAsync_ServicePermissionsException_ReturnsHttpForbidden()
            {
                workTaskServiceMock.Setup(s => s.GetAsync(personId))
                    .Throws(new PermissionsException());

                try
                {
                    await controllerUnderTest.GetAsync(personId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }
        }
    }
}
