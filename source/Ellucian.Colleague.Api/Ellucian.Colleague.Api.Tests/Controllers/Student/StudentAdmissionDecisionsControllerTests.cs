// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentAdmissionDecisionsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            StudentAdmissionDecisionsController admissionDecisionsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                admissionDecisionsController = new StudentAdmissionDecisionsController(loggerMock.Object) { Request = new HttpRequestMessage() };
                admissionDecisionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                admissionDecisionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionDecisionsController = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task StudentAdmissionDecisionsController_GetAll_True()
            {
                var admissionDecisions = await admissionDecisionsController.GetStudentAdmissionDecisionsAsync();

                Assert.IsTrue(admissionDecisions.Count().Equals(0));
             
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAdmissionDecisionsController_GETBYID_Not_Supported()
            {
                var actual = await admissionDecisionsController.GetStudentAdmissionDecisionByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAdmissionDecisionsController_PUT_Not_Supported()
            {
                var actual = await admissionDecisionsController.PutStudentAdmissionDecisionsAsync(It.IsAny<Dtos.StudentAdmissionDecisions>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAdmissionDecisionsController_POST_Not_Supported()
            {
                var actual = await admissionDecisionsController.PostStudentAdmissionDecisionsAsync(It.IsAny<Dtos.StudentAdmissionDecisions>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAdmissionDecisionsController_DELETE_Not_Supported()
            {
                await admissionDecisionsController.DeleteStudentAdmissionDecisionsAsync(It.IsAny<string>());
            }

        }
    }
}
