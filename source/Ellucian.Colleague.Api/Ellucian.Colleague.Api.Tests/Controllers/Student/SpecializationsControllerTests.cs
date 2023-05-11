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
    public class SpecializationsControllerTests
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

        private SpecializationsController specializationsController;
        private IEnumerable<Domain.Student.Entities.Specialization> specializations;

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
            var adapter = new AutoMapperAdapter<Domain.Student.Entities.Specialization, Specialization>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Specialization, Specialization>()).Returns(adapter);

            specializations = BuildSpecializations();
            specializationsController = new SpecializationsController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            specializationsController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task SpecializationsController_ReturnsSpecializationeDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetSpecializationsAsync()).Returns(Task.FromResult(specializations));

            var majorDtos = await specializationsController.GetAsync();

            Assert.IsTrue(majorDtos is IEnumerable<Dtos.Student.Minor>);
            Assert.AreEqual(specializations.Count(), majorDtos.Count());
        }

        [TestMethod]
        public async Task SpecializationsController_NullRepositoryResponse_ReturnsEmptySpecializationDtos()
        {
            IEnumerable<Domain.Student.Entities.Specialization> nullSpecializationEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetSpecializationsAsync()).Returns(Task.FromResult(nullSpecializationEntities));

            var majorDtos = await specializationsController.GetAsync();

            Assert.IsTrue(majorDtos is IEnumerable<Dtos.Student.Minor>);
            Assert.AreEqual(0, majorDtos.Count());
        }

        [TestMethod]
        public async Task SpecializationsController_EmptyRepositoryResponse_ReturnsEmptyCapSizeDtos()
        {
            IEnumerable<Domain.Student.Entities.Specialization> emptySpecializationEntities = new List<Domain.Student.Entities.Specialization>();
            referenceDataRepositoryMock.Setup(x => x.GetSpecializationsAsync()).Returns(Task.FromResult(emptySpecializationEntities));

            var majorDtos = await specializationsController.GetAsync();

            Assert.IsTrue(majorDtos is IEnumerable<Dtos.Student.Minor>);
            Assert.AreEqual(0, majorDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SpecializationsController_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetSpecializationsAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                await specializationsController.GetAsync();
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
        public async Task SpecializationsController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetSpecializationsAsync()).Throws(new ApplicationException());
                await specializationsController.GetAsync();
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

        private IEnumerable<Domain.Student.Entities.Specialization> BuildSpecializations()
        {
            var specializations = new List<Domain.Student.Entities.Specialization>()
                {
                    new Domain.Student.Entities.Specialization("AVIA","Aviation"),
                    new Domain.Student.Entities.Specialization("BUSN","Business Administration"),
                    new Domain.Student.Entities.Specialization("COMP","Computer Science"),
                    new Domain.Student.Entities.Specialization("POLI","Political Science"),
                };
            return specializations;
        }
    }
}
