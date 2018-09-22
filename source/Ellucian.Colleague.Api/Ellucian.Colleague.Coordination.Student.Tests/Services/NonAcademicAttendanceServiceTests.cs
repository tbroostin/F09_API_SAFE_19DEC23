// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class NonAcademicAttendanceServiceTests
    {
        private List<NonAcademicAttendanceRequirement> nonAcademicAttendanceRequirementEntities;
        private List<NonAcademicAttendance> nonAcademicAttendanceEntities;
        public class StudentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Matt",
                        PersonId = "0000001",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public IAdapterRegistry adapterRegistry;
        public Mock<IRoleRepository> roleRepositoryMock;
        public IRoleRepository roleRepository;
        public Mock<ILogger> loggerMock;
        public ILogger logger;
        public ICurrentUserFactory currentUserFactory;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private IConfigurationRepository configurationRepository;
        private Mock<INonAcademicAttendanceRepository> nonAcademicAttendanceRepositoryMock;
        private INonAcademicAttendanceRepository nonAcademicAttendanceRepository;
        private Mock<IStudentRepository> studentRepositoryMock;
        private IStudentRepository studentRepository;
        private NonAcademicAttendanceService service;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            loggerMock.Setup(lgr => lgr.IsDebugEnabled).Returns(true);
            logger = loggerMock.Object;

            currentUserFactory = new StudentUserFactory();

            SetupAdapters();
            SetupData();
            SetupRepositories();

            service = new NonAcademicAttendanceService(adapterRegistry, nonAcademicAttendanceRepository, currentUserFactory, roleRepository,
                logger, studentRepository, configurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            currentUserFactory = null;
            configurationRepositoryMock = null;
            configurationRepository = null;
            nonAcademicAttendanceRepositoryMock = null;
            nonAcademicAttendanceRepository = null;
            studentRepositoryMock = null;
            studentRepository = null;
        }

        #region GetNonAcademicAttendanceRequirementsAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendanceRequirementsAsync_Null_PersonId()
        {
            var dtos = await service.GetNonAcademicAttendanceRequirementsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendanceRequirementsAsync_Permissions_Exception()
        {
            var dtos = await service.GetNonAcademicAttendanceRequirementsAsync(currentUserFactory.CurrentUser.PersonId + "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendanceRequirementsAsync_Adapter_Retrieval_Exception()
        {
            AutoMapperAdapter<NonAcademicAttendanceRequirement, Dtos.Student.NonAcademicAttendanceRequirement> adapter = null;
            adapterRegistryMock.Setup(reg => reg.GetAdapter<NonAcademicAttendanceRequirement, Dtos.Student.NonAcademicAttendanceRequirement>()).Returns(adapter);
            adapterRegistry = adapterRegistryMock.Object;
            service = new NonAcademicAttendanceService(adapterRegistry, nonAcademicAttendanceRepository, currentUserFactory, roleRepository,
                logger, studentRepository, configurationRepository);

            var dtos = await service.GetNonAcademicAttendanceRequirementsAsync(currentUserFactory.CurrentUser.PersonId);
        }

        [TestMethod]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendanceRequirementsAsync_Repository_returns_null()
        {
            nonAcademicAttendanceRequirementEntities = null;
            nonAcademicAttendanceRepositoryMock.Setup(repo => repo.GetNonacademicAttendanceRequirementsAsync(It.IsAny<string>())).ReturnsAsync(nonAcademicAttendanceRequirementEntities);
            nonAcademicAttendanceRepository = nonAcademicAttendanceRepositoryMock.Object;
            service = new NonAcademicAttendanceService(adapterRegistry, nonAcademicAttendanceRepository, currentUserFactory, roleRepository,
                logger, studentRepository, configurationRepository);

            var dtos = await service.GetNonAcademicAttendanceRequirementsAsync(currentUserFactory.CurrentUser.PersonId);
            CollectionAssert.AreEqual(new List<Dtos.Student.NonAcademicAttendanceRequirement>(), dtos.ToList());
        }

        [TestMethod]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendanceRequirementsAsync_returns_DTOs()
        {
            var dtos = await service.GetNonAcademicAttendanceRequirementsAsync(currentUserFactory.CurrentUser.PersonId);
            Assert.AreEqual(nonAcademicAttendanceRequirementEntities.Count, dtos.Count());
            for (int i = 0; i < nonAcademicAttendanceRequirementEntities.Count; i++)
            {
                Assert.AreEqual(nonAcademicAttendanceRequirementEntities[i].DefaultRequiredUnits, dtos.ElementAt(i).DefaultRequiredUnits);
                Assert.AreEqual(nonAcademicAttendanceRequirementEntities[i].Id, dtos.ElementAt(i).Id);
                CollectionAssert.AreEqual(nonAcademicAttendanceRequirementEntities[i].NonAcademicAttendanceIds, dtos.ElementAt(i).NonAcademicAttendanceIds.ToList());
                Assert.AreEqual(nonAcademicAttendanceRequirementEntities[i].PersonId, dtos.ElementAt(i).PersonId);
                Assert.AreEqual(nonAcademicAttendanceRequirementEntities[i].RequiredUnits, dtos.ElementAt(i).RequiredUnits);
                Assert.AreEqual(nonAcademicAttendanceRequirementEntities[i].RequiredUnitsOverride, dtos.ElementAt(i).RequiredUnitsOverride);
                Assert.AreEqual(nonAcademicAttendanceRequirementEntities[i].TermCode, dtos.ElementAt(i).TermCode);
            }
        }

        #endregion

        #region GetNonAcademicAttendancesAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendancesAsync_Null_PersonId()
        {
            var dtos = await service.GetNonAcademicAttendancesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendancesAsync_Permissions_Exception()
        {
            var dtos = await service.GetNonAcademicAttendancesAsync(currentUserFactory.CurrentUser.PersonId + "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendancesAsync_Adapter_Retrieval_Exception()
        {
            AutoMapperAdapter<NonAcademicAttendance, Dtos.Student.NonAcademicAttendance> adapter = null;
            adapterRegistryMock.Setup(reg => reg.GetAdapter<NonAcademicAttendance, Dtos.Student.NonAcademicAttendance>()).Returns(adapter);
            adapterRegistry = adapterRegistryMock.Object;
            service = new NonAcademicAttendanceService(adapterRegistry, nonAcademicAttendanceRepository, currentUserFactory, roleRepository,
                logger, studentRepository, configurationRepository);

            var dtos = await service.GetNonAcademicAttendancesAsync(currentUserFactory.CurrentUser.PersonId);
        }

        [TestMethod]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendancesAsync_Repository_returns_null()
        {
            nonAcademicAttendanceEntities = null;
            nonAcademicAttendanceRepositoryMock.Setup(repo => repo.GetNonacademicAttendancesAsync(It.IsAny<string>())).ReturnsAsync(nonAcademicAttendanceEntities);
            nonAcademicAttendanceRepository = nonAcademicAttendanceRepositoryMock.Object;
            service = new NonAcademicAttendanceService(adapterRegistry, nonAcademicAttendanceRepository, currentUserFactory, roleRepository,
                logger, studentRepository, configurationRepository);

            var dtos = await service.GetNonAcademicAttendancesAsync(currentUserFactory.CurrentUser.PersonId);
            CollectionAssert.AreEqual(new List<Dtos.Student.NonAcademicAttendance>(), dtos.ToList());
        }

        [TestMethod]
        public async Task NonAcademicAttendanceService_GetNonAcademicAttendancesAsync_returns_DTOs()
        {
            var dtos = await service.GetNonAcademicAttendancesAsync(currentUserFactory.CurrentUser.PersonId);
            Assert.AreEqual(nonAcademicAttendanceEntities.Count, dtos.Count());
            for (int i = 0; i < nonAcademicAttendanceEntities.Count; i++)
            {
                Assert.AreEqual(nonAcademicAttendanceEntities[i].Id, dtos.ElementAt(i).Id);
                Assert.AreEqual(nonAcademicAttendanceEntities[i].PersonId, dtos.ElementAt(i).PersonId);
                Assert.AreEqual(nonAcademicAttendanceEntities[i].EventId, dtos.ElementAt(i).EventId);
                Assert.AreEqual(nonAcademicAttendanceEntities[i].UnitsEarned, dtos.ElementAt(i).UnitsEarned);
            }
        }

        #endregion

        /// <summary>
        /// Sets up the adapter registry
        /// </summary>
        private void SetupAdapters()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            var nonAcademicAttendanceRequirementAdapter = new AutoMapperAdapter<NonAcademicAttendanceRequirement, Dtos.Student.NonAcademicAttendanceRequirement>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<NonAcademicAttendanceRequirement, Dtos.Student.NonAcademicAttendanceRequirement>()).Returns(nonAcademicAttendanceRequirementAdapter);

            var nonAcademicAttendanceAdapter = new AutoMapperAdapter<NonAcademicAttendance, Dtos.Student.NonAcademicAttendance>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<NonAcademicAttendance, Dtos.Student.NonAcademicAttendance>()).Returns(nonAcademicAttendanceAdapter);

            adapterRegistry = adapterRegistryMock.Object;
        }

        /// <summary>
        /// Sets up test data
        /// </summary>
        private void SetupData()
        {
            nonAcademicAttendanceRequirementEntities = new List<NonAcademicAttendanceRequirement>()
            {
                new NonAcademicAttendanceRequirement("1", currentUserFactory.CurrentUser.PersonId, "TERM", new List<string>() { "11", "12" }, 30m, 24m),
                new NonAcademicAttendanceRequirement("2", currentUserFactory.CurrentUser.PersonId, "TERM2")
            };
            nonAcademicAttendanceEntities = new List<NonAcademicAttendance>()
            {
                new NonAcademicAttendance("111", currentUserFactory.CurrentUser.PersonId, "11", 3m),
                new NonAcademicAttendance("112", currentUserFactory.CurrentUser.PersonId, "12"),
            };
        }

        /// <summary>
        /// Sets up repositories
        /// </summary>
        private void SetupRepositories()
        {
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepository = roleRepositoryMock.Object;

            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            configurationRepository = configurationRepositoryMock.Object;

            nonAcademicAttendanceRepositoryMock = new Mock<INonAcademicAttendanceRepository>();
            nonAcademicAttendanceRepositoryMock.Setup(repo => repo.GetNonacademicAttendanceRequirementsAsync(It.IsAny<string>())).ReturnsAsync(nonAcademicAttendanceRequirementEntities);
            nonAcademicAttendanceRepositoryMock.Setup(repo => repo.GetNonacademicAttendancesAsync(It.IsAny<string>())).ReturnsAsync(nonAcademicAttendanceEntities);
            nonAcademicAttendanceRepository = nonAcademicAttendanceRepositoryMock.Object;

            studentRepositoryMock = new Mock<IStudentRepository>();
            studentRepository = studentRepositoryMock.Object;
        }
    }
}
