// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentWaiverReasonsControllerTests
    {
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

        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private StudentWaiverReasonsController waiverReasonsController;
        private List<Domain.Student.Entities.StudentWaiverReason> waiverReasons;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentWaiverReason, StudentWaiverReason>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentWaiverReason,StudentWaiverReason>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            waiverReasons = BuildWaiverReasons();
            waiverReasonsController = new StudentWaiverReasonsController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            waiverReasonsController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task WaiverReasonsController_ReturnsWaiverReasonDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetStudentWaiverReasonsAsync()).Returns(Task.FromResult(waiverReasons.AsEnumerable()));
            var waiverReasonDtos = await waiverReasonsController.GetAsync();
            Assert.IsTrue(waiverReasonDtos is IEnumerable<Dtos.Student.StudentWaiverReason>);
            Assert.AreEqual(2, waiverReasonDtos.Count());
        }

        [TestMethod]
        public async Task WaiverReasonsController_NullRepositoryResponse_ReturnsEmptyWaiverReasonDtos()
        {
            List<Domain.Student.Entities.StudentWaiverReason> nullWaiverReasonEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetStudentWaiverReasonsAsync()).Returns(Task.FromResult(nullWaiverReasonEntities.AsEnumerable()));
            var waiverReasonDtos = await waiverReasonsController.GetAsync();
            Assert.IsTrue(waiverReasonDtos is IEnumerable<Dtos.Student.StudentWaiverReason>);
            Assert.AreEqual(0, waiverReasonDtos.Count());
        }

        [TestMethod]
        public async Task WaiverReasonsController_EmptyRepositoryResponse_ReturnsEmptyWaiverReasonDtos()
        {
            List<Domain.Student.Entities.StudentWaiverReason> emptyWaiverReasonEntities = new List<Domain.Student.Entities.StudentWaiverReason>();
            referenceDataRepositoryMock.Setup(x => x.GetStudentWaiverReasonsAsync()).Returns(Task.FromResult(emptyWaiverReasonEntities.AsEnumerable()));
            var waiverReasonDtos = await waiverReasonsController.GetAsync();
            Assert.IsTrue(waiverReasonDtos is IEnumerable<Dtos.Student.StudentWaiverReason>);
            Assert.AreEqual(0, waiverReasonDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task WaiverReasonsController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetStudentWaiverReasonsAsync()).Throws(new ApplicationException());
                var waiverReasons = await waiverReasonsController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        private List<Domain.Student.Entities.StudentWaiverReason> BuildWaiverReasons()
        {
                var waiverReasons = new List<Domain.Student.Entities.StudentWaiverReason>()
                {
                    new Domain.Student.Entities.StudentWaiverReason("LIFE", "Life Experience"),
                    new Domain.Student.Entities.StudentWaiverReason("OTHER", "Other reason")
                };

            return waiverReasons;
        }
    }
}

