// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using slf4net;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentPetitionReasonsControllerTests
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
        private StudentPetitionReasonsController studentPetitionReasonsController;
        private List<Domain.Student.Entities.StudentPetitionReason> studentPeitionReasonsEntities;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
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
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetitionReason, StudentPetitionReason>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetitionReason, StudentPetitionReason>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            studentPeitionReasonsEntities = BuildStudentPetitionReasons();
            studentPetitionReasonsController = new StudentPetitionReasonsController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentPetitionReasonsController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task StudentPetitionReasonsController_ReturnsStudentPetitionReasonDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetStudentPetitionReasonsAsync()).Returns(Task.FromResult(studentPeitionReasonsEntities.AsEnumerable()));
            var studentPetitionReasons = await studentPetitionReasonsController.GetAsync();
            Assert.IsTrue(studentPetitionReasons is IEnumerable<Dtos.Student.StudentPetitionReason>);
            Assert.AreEqual(2, studentPetitionReasons.Count());
        }

        [TestMethod]
        public async Task StudentPetitionReasonsController_NullRepositoryResponse_ReturnsEmptyStudentPetitionReasonDtos()
        {
            List<Domain.Student.Entities.StudentPetitionReason> nullPetitionReasonEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetStudentPetitionReasonsAsync()).Returns(Task.FromResult(nullPetitionReasonEntities.AsEnumerable()));
            var studentPetitionReasons = await studentPetitionReasonsController.GetAsync();
            Assert.IsTrue(studentPetitionReasons is IEnumerable<Dtos.Student.StudentPetitionReason>);
            Assert.AreEqual(0, studentPetitionReasons.Count());
        }

        [TestMethod]
        public async Task StudentPetitionReasonsController_EmptyRepositoryResponse_ReturnsEmptyStudentPetitionReasonDtos()
        {
            List<Domain.Student.Entities.StudentPetitionReason> emptyPetitionReasonEntities = new List<Domain.Student.Entities.StudentPetitionReason>();
            referenceDataRepositoryMock.Setup(x => x.GetStudentPetitionReasonsAsync()).Returns(Task.FromResult(emptyPetitionReasonEntities.AsEnumerable()));
            var studentPetitionReasons = await studentPetitionReasonsController.GetAsync();
            Assert.IsTrue(studentPetitionReasons is IEnumerable<Dtos.Student.StudentPetitionReason>);
            Assert.AreEqual(0, studentPetitionReasons.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPetitionReasonsController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetStudentPetitionReasonsAsync()).Throws(new ApplicationException());
                var studentPetitionReasons = await studentPetitionReasonsController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        private List<Domain.Student.Entities.StudentPetitionReason> BuildStudentPetitionReasons()
        {
            var studentPetitionReasons = new List<Domain.Student.Entities.StudentPetitionReason>()
                {
                    new Domain.Student.Entities.StudentPetitionReason("ICJI", "I can handle it"),
                    new Domain.Student.Entities.StudentPetitionReason("OVMH", "Over my head")
                };

            return studentPetitionReasons;
        }
    }
}
