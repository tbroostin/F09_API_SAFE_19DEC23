// Copyright 2014-2018 Ellucian Company .P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class PreferredSectionsControllerTests
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

        private IPreferredSectionService preferredSectionService;
        private IPreferredSectionRepository preferredSectionRepo;
        private IAdapterRegistry adapterRegistry;
        private PreferredSectionsController preferredSectionsController;
        private ILogger logger;
        private string studentId = "STU001";
        private Dtos.Student.PreferredSectionsResponse preferredSectionsResponse;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            Mock<IPreferredSectionService> preferredSectionServiceMock = new Mock<IPreferredSectionService>();
            Mock<IPreferredSectionRepository> preferredSectionRepoMock = new Mock<IPreferredSectionRepository>();
            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();

            preferredSectionService = preferredSectionServiceMock.Object;
            preferredSectionRepo = preferredSectionRepoMock.Object;
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            // mock a preferredSectionService GET response
            preferredSectionsResponse = new PreferredSectionsResponse();
            preferredSectionsResponse.PreferredSections = new List<Dtos.Student.PreferredSection>();
            preferredSectionsResponse.Messages = new List<Dtos.Student.PreferredSectionMessage>();
            decimal? credits = decimal.Parse("3.00");
            Dtos.Student.PreferredSection ps1 = new Dtos.Student.PreferredSection(); ps1.StudentId = "STU001"; ps1.SectionId = "SEC001"; ps1.Credits = credits;
            Dtos.Student.PreferredSection ps2 = new Dtos.Student.PreferredSection(); ps2.StudentId = "STU001"; ps2.SectionId = "SEC002"; ps2.Credits = null;
            preferredSectionsResponse.PreferredSections.Add(ps1);
            preferredSectionsResponse.PreferredSections.Add(ps2);
            preferredSectionServiceMock.Setup(svc => svc.GetAsync("STU001")).Returns(Task.FromResult(preferredSectionsResponse));
            // mock a null student Id request
            preferredSectionServiceMock.Setup(svc => svc.GetAsync(null)).Throws(new ArgumentNullException(studentId, "Student ID is required."));
            // mock an empty student Id request
            preferredSectionServiceMock.Setup(svc => svc.GetAsync("")).Throws(new ArgumentNullException(studentId, "Student ID is required."));
            // mock a GET request with a bogus student Id
            preferredSectionServiceMock.Setup(svc => svc.GetAsync("BOGUS")).Throws(new KeyNotFoundException("Student not found in repository."));
            // mock a GET request for data other than the logged in user's
            preferredSectionServiceMock.Setup(svc => svc.GetAsync("STU002")).Throws(new PermissionsException());

            // mock a preferredSectionService POST (Update) response
            List<Dtos.Student.PreferredSectionMessage> prefSecMsgs = new List<Dtos.Student.PreferredSectionMessage>();
            prefSecMsgs.Add(new PreferredSectionMessage() { SectionId = "SEC001", Message = "Some message." });
            preferredSectionServiceMock.Setup(svc => svc.UpdateAsync("STU001", It.IsAny<List<Dtos.Student.PreferredSection>>())).Returns(Task.FromResult(prefSecMsgs.AsEnumerable()));
            // mock a POST with a null student Id
            preferredSectionServiceMock.Setup(svc => svc.UpdateAsync(null, It.IsAny<List<Dtos.Student.PreferredSection>>())).Throws(new ArgumentNullException(studentId, "Student ID is required."));
            // mock a POST with an empty student Id
            preferredSectionServiceMock.Setup(svc => svc.UpdateAsync("", It.IsAny<List<Dtos.Student.PreferredSection>>())).Throws(new ArgumentNullException(studentId, "Student ID is required."));

            // mock a POST with a bogus student Id
            preferredSectionServiceMock.Setup(svc => svc.UpdateAsync("BOGUS", It.IsAny<List<Dtos.Student.PreferredSection>>())).Throws(new KeyNotFoundException("Student not found in repository."));

            // mock a POST with mismatched student Id
            preferredSectionServiceMock.Setup(svc => svc.UpdateAsync("STU002", It.IsAny<List<Dtos.Student.PreferredSection>>())).Throws(new PermissionsException());

            // mock a POST with null preferred sections
            preferredSectionServiceMock.Setup(svc => svc.UpdateAsync("STU003", null)).Throws(new ArgumentException("One or more preferred sections are required"));
            // mock a POST with empty preferred sections
            preferredSectionServiceMock.Setup(svc => svc.UpdateAsync("STU004", new List<Dtos.Student.PreferredSection>())).Throws(new ArgumentException("One or more preferred sections are required"));

            // mock a successful DELETE response
            preferredSectionServiceMock.Setup(svc => svc.DeleteAsync("STU001", "SEC001")).Returns(Task.FromResult(prefSecMsgs.AsEnumerable()));
            // mock a DELETE WITH a bogus student Id
            preferredSectionServiceMock.Setup(svc => svc.DeleteAsync(null, "SEC001")).Throws(new ArgumentNullException(studentId, "Student ID is required."));
            preferredSectionServiceMock.Setup(svc => svc.DeleteAsync("", "SEC001")).Throws(new ArgumentNullException(studentId, "Student ID is required."));
            preferredSectionServiceMock.Setup(svc => svc.DeleteAsync("BOGUS", "SEC001")).Throws(new KeyNotFoundException("Student not found in repository."));
            // mock a DELETE with a bogus section Id
            preferredSectionServiceMock.Setup(svc => svc.DeleteAsync("STU001", null)).Throws(new ArgumentException("Section Id is required."));
            preferredSectionServiceMock.Setup(svc => svc.DeleteAsync("STU001", "")).Throws(new ArgumentException("Section Id is required."));
            // mock a DELETE with a mismatched student Id
            preferredSectionServiceMock.Setup(svc => svc.DeleteAsync("STU002", "SEC001")).Throws(new PermissionsException());


            // mock DTO adapters
            var prefSecRespAdapter = new AutoMapperAdapter<Domain.Student.Entities.PreferredSectionsResponse, Dtos.Student.PreferredSectionsResponse>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.PreferredSectionsResponse, Dtos.Student.PreferredSectionsResponse>()).Returns(prefSecRespAdapter);

            preferredSectionsController = new PreferredSectionsController(preferredSectionService, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            preferredSectionsController = null;
            adapterRegistry = null;
            preferredSectionRepo = null;
        }


        [TestMethod]
        public async Task PrefSecController_DeleteSuccess()
        {
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.DeletePreferredSectionsAsync("STU001", "SEC001");
            Assert.AreEqual(1, psmDTO.Count());
            Dtos.Student.PreferredSectionMessage psm = psmDTO.FirstOrDefault();
            Assert.AreEqual("SEC001", psm.SectionId);
            Assert.AreEqual("Some message.", psm.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_DeleteThrowsWhenNullStudentId()
        {
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.DeletePreferredSectionsAsync(null, "SEC001");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_DeleteThrowsWhenEmptyStudentId()
        {
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.DeletePreferredSectionsAsync("", "SEC001");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_DeleteThrowsWhenInvalidStudentId()
        {
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.DeletePreferredSectionsAsync("BOGUS", "SEC001");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_DeleteThrowsWhenNullSectionId()
        {
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.DeletePreferredSectionsAsync("BOGUS", "SEC001");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_DeleteThrowsWhenEmptySectionId()
        {
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.DeletePreferredSectionsAsync("BOGUS", "SEC001");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_DeleteThrowsWhenPermissionsException()
        {
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO =await preferredSectionsController.DeletePreferredSectionsAsync("STU002", "SEC001");
        }


        [TestMethod]
        public async Task PrefSecController_UpdateSuccess()
        {
            List<Dtos.Student.PreferredSection> prefSecs = new List<Dtos.Student.PreferredSection>();
            prefSecs.Add(new Dtos.Student.PreferredSection() { StudentId = "STU001", SectionId = "SEC002", Credits = null } );
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.UpdatePreferredSectionsAsync("STU001", prefSecs);
            Assert.AreEqual(1, psmDTO.Count());
            Dtos.Student.PreferredSectionMessage psm = psmDTO.FirstOrDefault();
            Assert.AreEqual("SEC001", psm.SectionId);
            Assert.AreEqual("Some message.", psm.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_UpdateThrowsWhenNullStudentId()
        {
            List<Dtos.Student.PreferredSection> prefSecs = new List<Dtos.Student.PreferredSection>();
            prefSecs.Add(new Dtos.Student.PreferredSection() { StudentId = "STU001", SectionId = "SEC002", Credits = null });
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.UpdatePreferredSectionsAsync(null, prefSecs);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_UpdateThrowsWhenEmptyStudentId()
        {
            List<Dtos.Student.PreferredSection> prefSecs = new List<Dtos.Student.PreferredSection>();
            prefSecs.Add(new Dtos.Student.PreferredSection() { StudentId = "STU001", SectionId = "SEC002", Credits = null });
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.UpdatePreferredSectionsAsync("", prefSecs);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_UpdateThrowsWhenInvalidStudent()
        {
            List<Dtos.Student.PreferredSection> prefSecs = new List<Dtos.Student.PreferredSection>();
            prefSecs.Add(new Dtos.Student.PreferredSection() { StudentId = "STU001", SectionId = "SEC002", Credits = null });
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.UpdatePreferredSectionsAsync("BOGUS", prefSecs);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_UpdateThrowsWhenPermissionsException()
        {
            List<Dtos.Student.PreferredSection> prefSecs = new List<Dtos.Student.PreferredSection>();
            prefSecs.Add(new Dtos.Student.PreferredSection() { StudentId = "STU001", SectionId = "SEC002", Credits = null });
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.UpdatePreferredSectionsAsync("STU002", prefSecs);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_UpdateThrowsWhenNullPreferredSections()
        {
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.UpdatePreferredSectionsAsync("STU003", null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_UpdateThrowsWhenEmptyPreferredSections()
        {
            IEnumerable<Dtos.Student.PreferredSectionMessage> psmDTO = await preferredSectionsController.UpdatePreferredSectionsAsync("STU004", new List<Dtos.Student.PreferredSection>());
        }


        [TestMethod]
        public async Task PrefSecController_GetSuccess()
        {
            Dtos.Student.PreferredSectionsResponse psrDTO = await preferredSectionsController.GetPreferredSectionsAsync("STU001");
            Assert.AreEqual(2, psrDTO.PreferredSections.Count());
            Assert.AreEqual(0, psrDTO.Messages.Count());
            Assert.AreEqual(preferredSectionsResponse.PreferredSections[0].StudentId, psrDTO.PreferredSections[0].StudentId);
            Assert.AreEqual(preferredSectionsResponse.PreferredSections[0].SectionId, psrDTO.PreferredSections[0].SectionId);
            Assert.AreEqual(preferredSectionsResponse.PreferredSections[0].Credits,   psrDTO.PreferredSections[0].Credits);
            Assert.AreEqual(preferredSectionsResponse.PreferredSections[1].SectionId, psrDTO.PreferredSections[1].SectionId);
            Assert.AreEqual(preferredSectionsResponse.PreferredSections[1].StudentId, psrDTO.PreferredSections[1].StudentId);
            Assert.AreEqual(preferredSectionsResponse.PreferredSections[1].Credits,   psrDTO.PreferredSections[1].Credits);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_GetThrowsWhenNullStudentId()
        {
            Dtos.Student.PreferredSectionsResponse psrDTO = await preferredSectionsController.GetPreferredSectionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_GetThrowsWhenEmptyStudentId()
        {
            Dtos.Student.PreferredSectionsResponse psrDTO = await preferredSectionsController.GetPreferredSectionsAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_GetThrowsWhenInvalidStudent()
        {
            Dtos.Student.PreferredSectionsResponse psrDTO = await preferredSectionsController.GetPreferredSectionsAsync("BOGUS");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PrefSecController_GetThrowsWhenPermissionsException()
        {
            Dtos.Student.PreferredSectionsResponse psrDTO = await preferredSectionsController.GetPreferredSectionsAsync("STU002");
        }

    }
}
