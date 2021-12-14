// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.AnonymousGrading;
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
    public class SectionFacultyUserFactory : ICurrentUserFactory
    {
        public ICurrentUser CurrentUser
        {
            get
            {
                return new CurrentUser(new Claims()
                {
                    ControlId = "678",
                    Name = "Samwise",
                    PersonId = "0000678",
                    SecurityToken = "321",
                    SessionTimeout = 30,
                    UserName = "Samwise-faculty",
                    Roles = new List<string>() { },
                    SessionFixationId = "abc123"
                });
            }
        }
    }

    [TestClass]
    public class PreliminaryAnonymousGradeServiceTests
    {
        public PreliminaryAnonymousGradeService service;

        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IStudentRepository> studentRepositoryMock;
        public Mock<IConfigurationRepository> configurationRepositoryMock;
        public Mock<ISectionRepository> sectionRepositoryMock;
        public Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        public Mock<IPreliminaryAnonymousGradeRepository> preliminaryAnonymousGradeRepositoryMock;

        public IAdapterRegistry adapterRegistry;
        public ICurrentUserFactory currentUserFactory;
        public IRoleRepository roleRepository;
        public ILogger logger;
        public IStudentRepository studentRepository;
        public IConfigurationRepository configurationRepository;
        public ISectionRepository sectionRepository;
        public IStudentConfigurationRepository studentConfigurationRepository;
        public IPreliminaryAnonymousGradeRepository preliminaryAnonymousGradeRepository;

        public string sectionId;
        public AcademicRecordConfiguration academicRecordConfigurationEntity;
        public Section sectionEntity;

        [TestInitialize]
        public void PreliminaryAnonymousGradeServiceTests_Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            currentUserFactory = new SectionFacultyUserFactory();

            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepository = roleRepositoryMock.Object;

            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;

            studentRepositoryMock = new Mock<IStudentRepository>();
            studentRepository = studentRepositoryMock.Object;

            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            configurationRepository = configurationRepositoryMock.Object;

            sectionRepositoryMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepositoryMock.Object;

            sectionId = "12345";
            sectionEntity = new Section(sectionId, "100", "01", DateTime.Today.AddDays(-60), 3m, null, "Section to be Graded", "IN",
                new List<OfferingDepartment>()
                {
                                new OfferingDepartment("UG", 100)
                }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-90)) })
            {
                GradeByRandomId = true
            };
            sectionEntity.AddFaculty(currentUserFactory.CurrentUser.PersonId);
            sectionRepositoryMock.Setup(repo => repo.GetSectionAsync(sectionId)).ReturnsAsync(sectionEntity);

            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            studentConfigurationRepository = studentConfigurationRepositoryMock.Object;

            academicRecordConfigurationEntity = new AcademicRecordConfiguration(AnonymousGradingType.Section);
            studentConfigurationRepositoryMock.Setup(repo => repo.GetAcademicRecordConfigurationAsync()).ReturnsAsync(academicRecordConfigurationEntity);

            preliminaryAnonymousGradeRepositoryMock = new Mock<IPreliminaryAnonymousGradeRepository>();
            preliminaryAnonymousGradeRepository = preliminaryAnonymousGradeRepositoryMock.Object;

            service = new PreliminaryAnonymousGradeService(adapterRegistry, currentUserFactory, roleRepository, logger,
                studentRepository, configurationRepository, sectionRepository, studentConfigurationRepository, preliminaryAnonymousGradeRepository);
        }
    }

    [TestClass]
    public class GetPreliminaryAnonymousGradesBySectionIdAsyncTests : PreliminaryAnonymousGradeServiceTests
    {
        public Domain.Student.Entities.AnonymousGrading.SectionPreliminaryAnonymousGrading sectionPreliminaryAnonymousGradingEntity;
        public List<string> crossListedSectionIds;

        [TestInitialize]
        public void GetPreliminaryAnonymousGradesBySectionIdAsyncTests_Initialize()
        {
            base.PreliminaryAnonymousGradeServiceTests_Initialize();
            crossListedSectionIds = new List<string>()
            {
                "12346"
            };
            sectionPreliminaryAnonymousGradingEntity = new Domain.Student.Entities.AnonymousGrading.SectionPreliminaryAnonymousGrading(sectionId);
            sectionPreliminaryAnonymousGradingEntity.AddAnonymousGradeForSection(new Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade("12345", "1", sectionId, "23456", null));
            sectionPreliminaryAnonymousGradingEntity.AddAnonymousGradeForCrosslistedSection(new Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade("12346", "2", crossListedSectionIds[0], "23457", DateTime.Today.AddDays(90)));
            sectionPreliminaryAnonymousGradingEntity.AddError(new Domain.Student.Entities.AnonymousGrading.AnonymousGradeError("23458", "34567", "23458", "0001234*2020/FA*UG", "No random ID assigned."));
            preliminaryAnonymousGradeRepositoryMock.Setup(repo => repo.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId, It.IsAny<IEnumerable<string>>())).ReturnsAsync(sectionPreliminaryAnonymousGradingEntity);

            SectionPreliminaryAnonymousGradingAdapter adapter = new SectionPreliminaryAnonymousGradingAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.AnonymousGrading.SectionPreliminaryAnonymousGrading, SectionPreliminaryAnonymousGrading>()).Returns(adapter);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_null_sectionId_ArgumentNullException()
        {
            var dtos = await service.GetPreliminaryAnonymousGradesBySectionIdAsync(null);
        }

        [ExpectedException(typeof(ConfigurationException))]
        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_null_AcademicRecordConfiguration_ConfigurationException()
        {
            academicRecordConfigurationEntity = null;
            studentConfigurationRepositoryMock.Setup(repo => repo.GetAcademicRecordConfigurationAsync()).ReturnsAsync(academicRecordConfigurationEntity);
            var dtos = await service.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
        }

        [ExpectedException(typeof(ConfigurationException))]
        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_AcademicRecordConfiguration_AnonymousGradingType_None_ConfigurationException()
        {
            academicRecordConfigurationEntity = new AcademicRecordConfiguration(AnonymousGradingType.None);
            studentConfigurationRepositoryMock.Setup(repo => repo.GetAcademicRecordConfigurationAsync()).ReturnsAsync(academicRecordConfigurationEntity);
            var dtos = await service.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_null_Section_entity_KeyNotFoundException()
        {
            sectionEntity = null;
            sectionRepositoryMock.Setup(repo => repo.GetSectionAsync(sectionId)).ReturnsAsync(sectionEntity);
            var dtos = await service.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
        }

        [ExpectedException(typeof(PermissionsException))]
        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_Section_entity_CurrentUser_not_in_FacultyIds_PermissionsException()
        {
            sectionEntity = new Section(sectionId, "100", "01", DateTime.Today.AddDays(-60), 3m, null, "Section to be Graded", "IN",
                new List<OfferingDepartment>()
                {
                                new OfferingDepartment("UG", 100)
                }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-90)) });
            sectionRepositoryMock.Setup(repo => repo.GetSectionAsync(sectionId)).ReturnsAsync(sectionEntity);
            var dtos = await service.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
        }

        [ExpectedException(typeof(ConfigurationException))]
        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_Section_entity_GradeByRandomId_false_ConfigurationException()
        {
            sectionEntity.GradeByRandomId = false;
            sectionRepositoryMock.Setup(repo => repo.GetSectionAsync(sectionId)).ReturnsAsync(sectionEntity);
            var dtos = await service.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_valid_no_crosslist()
        {
            sectionPreliminaryAnonymousGradingEntity = new Domain.Student.Entities.AnonymousGrading.SectionPreliminaryAnonymousGrading(sectionId);
            sectionPreliminaryAnonymousGradingEntity.AddAnonymousGradeForSection(new Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade("12345", "1", sectionId, "23456", null));
            sectionPreliminaryAnonymousGradingEntity.AddError(new Domain.Student.Entities.AnonymousGrading.AnonymousGradeError("23458", "34567", "23458", "0001234*2020/FA*UG", "No random ID assigned."));
            preliminaryAnonymousGradeRepositoryMock.Setup(repo => repo.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId, It.IsAny<IEnumerable<string>>())).ReturnsAsync(sectionPreliminaryAnonymousGradingEntity);

            var dtos = await service.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
            Assert.AreEqual(1, dtos.AnonymousGradesForSection.Count());
            Assert.AreEqual(0, dtos.AnonymousGradesForCrosslistedSections.Count());
            Assert.AreEqual(1, dtos.Errors.Count());
            Assert.AreEqual(sectionId, dtos.SectionId);
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_valid_with_crosslist()
        {
            studentConfigurationRepositoryMock.Setup(repo => repo.GetFacultyGradingConfigurationAsync()).ReturnsAsync(new FacultyGradingConfiguration()
            {
                IncludeCrosslistedStudents = true
            });
            var sectionEntity2 = new Section(crossListedSectionIds[0], "100", "01", DateTime.Today.AddDays(-60), 3m, null, "Crosslisted section to be Graded", "IN",
                new List<OfferingDepartment>()
                {
                    new OfferingDepartment("UG", 100)
                }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-90)) });
            sectionEntity2.GradeByRandomId = true;
            sectionRepositoryMock.Setup(repo => repo.GetSectionAsync(crossListedSectionIds[0])).ReturnsAsync(sectionEntity2);

            var dtos = await service.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
            Assert.AreEqual(1, dtos.AnonymousGradesForSection.Count());
            Assert.AreEqual(1, dtos.AnonymousGradesForCrosslistedSections.Count());
            Assert.AreEqual(1, dtos.Errors.Count());
            Assert.AreEqual(sectionId, dtos.SectionId);
        }
    }

    [TestClass]
    public class UpdatePreliminaryAnonymousGradesBySectionIdAsyncTests : PreliminaryAnonymousGradeServiceTests
    {
        public List<PreliminaryAnonymousGrade> preliminaryAnonymousGrades;
        public List<Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGradeUpdateResult> updateResultEntity;

        [TestInitialize]
        public void UpdatePreliminaryAnonymousGradesBySectionIdAsyncTests_Initialize()
        {
            base.PreliminaryAnonymousGradeServiceTests_Initialize();
            sectionId = "12345";
            preliminaryAnonymousGrades = new List<PreliminaryAnonymousGrade>()
            {
                new PreliminaryAnonymousGrade()
                {
                    AnonymousGradingId = "12345",
                    CourseSectionId = sectionId,
                    FinalGradeExpirationDate = null,
                    FinalGradeId = "1",
                    StudentCourseSectionId = "23456"
                },
                new PreliminaryAnonymousGrade()
                {
                    AnonymousGradingId = "12346",
                    CourseSectionId = sectionId,
                    FinalGradeExpirationDate = null,
                    FinalGradeId = "2",
                    StudentCourseSectionId = "23457"
                }
            };

            updateResultEntity = new List<Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGradeUpdateResult>();
            updateResultEntity.Add(new Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGradeUpdateResult(preliminaryAnonymousGrades[0].StudentCourseSectionId, 
                Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGradeUpdateStatus.Success, 
                string.Empty));
            updateResultEntity.Add(new Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGradeUpdateResult(preliminaryAnonymousGrades[1].StudentCourseSectionId, 
                Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGradeUpdateStatus.Failure, 
                "Invalid grade ID"));
            preliminaryAnonymousGradeRepositoryMock.Setup(repo => repo.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, It.IsAny<IEnumerable<Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade>>())).ReturnsAsync(updateResultEntity);

            adapterRegistryMock.Setup(reg => reg.GetAdapter<PreliminaryAnonymousGrade, Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade>()).Returns(new PreliminaryAnonymousGradeDtoToEntityAdapter(adapterRegistry, logger));
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGradeUpdateResult, PreliminaryAnonymousGradeUpdateResult>()).Returns(new AutoMapperAdapter<Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGradeUpdateResult, PreliminaryAnonymousGradeUpdateResult>(adapterRegistry, logger));
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_null_sectionId_ArgumentNullException()
        {
            var dtos = await service.UpdatePreliminaryAnonymousGradesBySectionIdAsync(null, preliminaryAnonymousGrades);
        }

        [ExpectedException(typeof(ConfigurationException))]
        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_null_AcademicRecordConfiguration_ConfigurationException()
        {
            academicRecordConfigurationEntity = null;
            studentConfigurationRepositoryMock.Setup(repo => repo.GetAcademicRecordConfigurationAsync()).ReturnsAsync(academicRecordConfigurationEntity);
            var dtos = await service.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [ExpectedException(typeof(ConfigurationException))]
        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_AcademicRecordConfiguration_AnonymousGradingType_None_ConfigurationException()
        {
            academicRecordConfigurationEntity = new AcademicRecordConfiguration(AnonymousGradingType.None);
            studentConfigurationRepositoryMock.Setup(repo => repo.GetAcademicRecordConfigurationAsync()).ReturnsAsync(academicRecordConfigurationEntity);
            var dtos = await service.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_null_Section_entity_KeyNotFoundException()
        {
            sectionEntity = null;
            sectionRepositoryMock.Setup(repo => repo.GetSectionAsync(sectionId)).ReturnsAsync(sectionEntity);
            var dtos = await service.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [ExpectedException(typeof(PermissionsException))]
        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Section_entity_CurrentUser_not_in_FacultyIds_PermissionsException()
        {
            sectionEntity = new Section(sectionId, "100", "01", DateTime.Today.AddDays(-60), 3m, null, "Section to be Graded", "IN",
                new List<OfferingDepartment>()
                {
                                new OfferingDepartment("UG", 100)
                }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-90)) });
            sectionRepositoryMock.Setup(repo => repo.GetSectionAsync(sectionId)).ReturnsAsync(sectionEntity);
            var dtos = await service.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [ExpectedException(typeof(ConfigurationException))]
        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Section_entity_GradeByRandomId_false_ConfigurationException()
        {
            sectionEntity.GradeByRandomId = false;
            sectionRepositoryMock.Setup(repo => repo.GetSectionAsync(sectionId)).ReturnsAsync(sectionEntity);
            var dtos = await service.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_valid()
        {
            var dtos = await service.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
            Assert.AreEqual(2, dtos.Count());
        }
    }
}
