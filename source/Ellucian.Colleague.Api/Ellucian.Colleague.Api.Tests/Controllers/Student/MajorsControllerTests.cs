// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    public class MajorsControllerTests
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
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        private MajorsController majorsController;
        private IEnumerable<Domain.Student.Entities.Major> majors;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;
            logger = new Mock<ILogger>().Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Domain.Student.Entities.Major, Major>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Major, Major>()).Returns(adapter);

            majors = BuildMajors();
            majorsController = new MajorsController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            majorsController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task MajorsController_ReturnsMajorDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetMajorsAsync(It.IsAny<bool>())).Returns(Task.FromResult(majors));

            var majorDtos = await majorsController.GetAsync();

            Assert.IsTrue(majorDtos is IEnumerable<Dtos.Student.Major>);
            Assert.AreEqual(majors.Count(), majorDtos.Count());
        }

        [TestMethod]
        public async Task MajorsController_NullRepositoryResponse_ReturnsEmptyMajorDtos()
        {
            IEnumerable<Domain.Student.Entities.Major> nullMajorEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetMajorsAsync(It.IsAny<bool>())).Returns(Task.FromResult(nullMajorEntities));
            
            var majorDtos = await majorsController.GetAsync();
            
            Assert.IsTrue(majorDtos is IEnumerable<Dtos.Student.Major>);
            Assert.AreEqual(0, majorDtos.Count());
        }

        [TestMethod]
        public async Task MajorsController_EmptyRepositoryResponse_ReturnsEmptyMajorDtos()
        {
            IEnumerable<Domain.Student.Entities.Major> emptyMajorEntities = new List<Domain.Student.Entities.Major>();
            referenceDataRepositoryMock.Setup(x => x.GetMajorsAsync(It.IsAny<bool>())).Returns(Task.FromResult(emptyMajorEntities));
            
            var majorDtos = await majorsController.GetAsync();
            
            Assert.IsTrue(majorDtos is IEnumerable<Dtos.Student.Major>);
            Assert.AreEqual(0, majorDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MajorsController_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetMajorsAsync(It.IsAny<bool>())).Throws(new ColleagueSessionExpiredException("session expired"));
                await majorsController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MajorsController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetMajorsAsync(It.IsAny<bool>())).Throws(new ApplicationException());
                await majorsController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        private IEnumerable<Domain.Student.Entities.Major> BuildMajors()
        {
            var majors = new List<Domain.Student.Entities.Major>()
                {
                    new Domain.Student.Entities.Major("AVIA","Aviation"),
                    new Domain.Student.Entities.Major("BUSN","Business Administration"),
                    new Domain.Student.Entities.Major("COMP","Computer Science"),
                    new Domain.Student.Entities.Major("POLI","Political Science"),
                };
            return majors;
        }
    }
}