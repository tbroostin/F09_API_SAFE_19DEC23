// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using System;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    //[TestClass]
    //public class HousingRequestsServiceTests
    //{
    //    // sets up a current user
    //    public abstract class CurrentUserSetup
    //    {
    //        protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

    //        public class PersonUserFactory : ICurrentUserFactory
    //        {
    //            public ICurrentUser CurrentUser
    //            {
    //                get
    //                {
    //                    return new CurrentUser(new Claims()
    //                    {
    //                        ControlId = "123",
    //                        Name = "George",
    //                        PersonId = "0000015",
    //                        SecurityToken = "321",
    //                        SessionTimeout = 30,
    //                        UserName = "Faculty",
    //                        Roles = new List<string>() { "Faculty" },
    //                        SessionFixationId = "abc123",
    //                    });
    //                }
    //            }
    //        }
    //    }

    //    [TestClass]
    //    public class HousingRequests : CurrentUserSetup
    //    {
    //        Mock<IHousingRequestRepository> housingRequestRepositoryMock;
    //        Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
    //        Mock<IAdapterRegistry> adapterRegistryMock;
    //        Mock<IReferenceDataRepository> referenceRepositoryMock;
    //        Mock<IPersonRepository> personRepositoryMock;
    //        Mock<ITermRepository> termRepositoryMock;
    //        Mock<IRoomRepository> roomRepositoryMock;
    //        Mock<ICurrentUserFactory> userFactoryMock;
    //        ICurrentUserFactory userFactory;
    //        Mock<IRoleRepository> roleRepoMock;
    //        //Mock<IPersonBaseRepository> personBaseRepositoryMock;
    //        private ILogger logger;

    //        HousingRequestsService housingRequestService;

    //        IEnumerable<Domain.Student.Entities.HousingRequest> housingRequestEntities;
    //        IEnumerable<Dtos.HousingRequest> housingRequestDtos;
    //        Dictionary<string, string> personGuids = new Dictionary<string, string>();
    //        IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriodEntities;
    //        IEnumerable<Domain.Student.Entities.Term> termEntities;
    //        //IEnumerable<Domain.Student.Entities.AdmissionApplicationStatusType> admissionStatusTypeEntities;
    //        //IEnumerable<Domain.Student.Entities.ApplicationSource> applicationSourceEntities;
    //        //IEnumerable<Domain.Student.Entities.AdmissionPopulation> admissionPopulationEntities;
    //        //IEnumerable<Domain.Student.Entities.AdmissionResidencyType> admissionResidencyEntities;
    //        //IEnumerable<Domain.Student.Entities.AcademicLevel> academicLevelEntities;
    //        //IEnumerable<Domain.Student.Entities.AcademicProgram> academicProgramEntities;
    //        //IEnumerable<Domain.Student.Entities.WithdrawReason> withdrawReasonEntities;
    //        //IEnumerable<Domain.Base.Entities.Location> locationEntities;
    //        //IEnumerable<Domain.Base.Entities.AcademicDiscipline> academicDisciplineEntities;
    //        //IEnumerable<Domain.Base.Entities.School> schoolEntities;

    //        Tuple<IEnumerable<Domain.Student.Entities.HousingRequest>, int> housingRequestEntitiesTuple;
    //        Tuple<IEnumerable<Dtos.HousingRequest>, int> housingRequestDtoTuple;

    //        int offset = 0;
    //        int limit = 200;

    //        private Domain.Entities.Permission permissionViewAnyApplication;


    //        [TestInitialize]
    //        public void Initialize()
    //        {
    //            housingRequestRepositoryMock = new Mock<IHousingRequestRepository>();
    //            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
    //            personRepositoryMock = new Mock<IPersonRepository>();
    //            termRepositoryMock = new Mock<ITermRepository>();
    //            referenceRepositoryMock = new Mock<IReferenceDataRepository>();
    //            roomRepositoryMock = new Mock<IRoomRepository>();
    //            roleRepoMock = new Mock<IRoleRepository>();
    //            userFactoryMock = new Mock<ICurrentUserFactory>();
    //            adapterRegistryMock = new Mock<IAdapterRegistry>();
    //            logger = new Mock<ILogger>().Object;

    //            // Set up current user
    //            userFactory = userFactoryMock.Object;
    //            userFactory = new CurrentUserSetup.PersonUserFactory();
    //            // Mock permissions
    //            permissionViewAnyApplication = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewHousingRequest);
    //            personRole.AddPermission(permissionViewAnyApplication);

    //            BuildData();
    //            BuildMocks();

    //            housingRequestService = new HousingRequestsService(housingRequestRepositoryMock.Object, personRepositoryMock.Object, termRepositoryMock.Object, roomRepositoryMock.Object, referenceRepositoryMock.Object, referenceDataRepositoryMock.Object, adapterRegistryMock.Object, userFactory, roleRepoMock.Object, logger);
    //        }

    //        [TestCleanup]
    //        public void Cleanup()
    //        {
    //            housingRequestRepositoryMock = null;
    //            referenceDataRepositoryMock = null;
    //            housingRequestEntities = null;
    //            housingRequestDtos = null;
    //        }

    //        private void BuildData()
    //        {
    //            //Dtos
    //            housingRequestDtos = new List<Dtos.HousingRequest>()
    //            {
    //                new Dtos.HousingRequest()
    //                {
    //                    //Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
    //                    Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
    //                    //Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
    //                    //    new Dtos.DtoProperties.AdmissionApplicationsStatus() {
    //                    //        AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"),
    //                    //        AdmissionApplicationsStatusStartOn = new DateTime(2016, 09, 03),
    //                    //        AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.DecisionMade
    //                    //    }
    //                    //},
    //                    //Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
    //                },
    //                new Dtos.HousingRequest()
    //                {
    //                    //Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
    //                    Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
    //                    //Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
    //                    //    new Dtos.DtoProperties.AdmissionApplicationsStatus() {
    //                    //        AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
    //                    //        AdmissionApplicationsStatusStartOn = new DateTime(2016, 09, 01),
    //                    //        AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.EnrollmentCompleted
    //                    //    }
    //                    //},
    //                    //Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
    //                },
    //                new Dtos.HousingRequest()
    //                {
    //                    //Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
    //                    Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
    //                    //Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
    //                    //    new Dtos.DtoProperties.AdmissionApplicationsStatus() {
    //                    //        AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
    //                    //        AdmissionApplicationsStatusStartOn = new DateTime(2016, 04, 01),
    //                    //        AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.ReadyForReview
    //                    //    }
    //                    //},
    //                    //Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
    //                },
    //                new Dtos.HousingRequest()
    //                {
    //                    //Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
    //                    Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
    //                    //Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
    //                    //    new Dtos.DtoProperties.AdmissionApplicationsStatus() {
    //                    //        AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"),
    //                    //        AdmissionApplicationsStatusStartOn = new DateTime(2016, 02, 01),
    //                    //        AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.ReadyForReview
    //                    //    }
    //                    //},
    //                    //Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
    //                },
    //            };

    //            //Entities
    //            housingRequestEntities = new List<Domain.Student.Entities.HousingRequest>()
    //            {
    //                new Domain.Student.Entities.HousingRequest("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", "1", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "R")
    //                {
    //                    Term = "CODE1",
    //                    PersonId = "1",
    //                    //AdmissionApplicationStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>() {
    //                    //    new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE1", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) }
    //                    //},
    //                },
    //                new Domain.Student.Entities.HousingRequest("3f67b180-ce1d-4552-8d81-feb96b9fea5b", "1", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "A")
    //                {
    //                    Term = "CODE2",
    //                    PersonId = "1",
    //                    //AdmissionApplicationStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>() {
    //                    //    new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE1", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) }
    //                    //},
    //                },
    //                new Domain.Student.Entities.HousingRequest("bf67e156-8f5d-402b-8101-81b0a2796873", "3", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "C")
    //                {
    //                    Term = "CODE3",
    //                    PersonId = "2",
    //                    //AdmissionApplicationStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>() {
    //                    //    new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE1", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) }
    //                    //},
    //                },
    //                new Domain.Student.Entities.HousingRequest("0111d6ef-5a86-465f-ac58-4265a997c136", "3", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "T")
    //                {
    //                    Term = "CODE4",
    //                    PersonId = "2",
    //                    //AdmissionApplicationStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>() {
    //                    //    new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE1", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) }
    //                    //},
    //                },
    //            };

    //            //Persons
    //            personGuids.Add("1", "d190d4b5-03b5-41aa-99b8-b8286717c956");
    //            personGuids.Add("2", "cecdce5a-54a7-45fb-a975-5392a579e5bf");

    //            //Terms
    //            termEntities = new List<Domain.Student.Entities.Term>()
    //            {
    //                new Domain.Student.Entities.Term("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", new DateTime(2016, 02, 01), new DateTime(2016, 03, 01), 2016, 1, false, false, "WINTER", false),
    //                new Domain.Student.Entities.Term("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", new DateTime(2016, 03, 01), new DateTime(2016, 04, 01), 2016, 2, false, false, "WINTER", false),
    //                new Domain.Student.Entities.Term("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", new DateTime(2016, 04, 01), new DateTime(2016, 05, 01), 2016, 3, false, false, "WINTER", false),
    //                new Domain.Student.Entities.Term("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, false, false, "WINTER", false)
    //            };

    //            ////AcademicPeriods
    //            academicPeriodEntities = new List<Domain.Student.Entities.AcademicPeriod>()
    //            {
    //                new Domain.Student.Entities.AcademicPeriod("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "0", "2", new List<Domain.Student.Entities.RegistrationDate>()),
    //                new Domain.Student.Entities.AcademicPeriod("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "1", "3", new List<Domain.Student.Entities.RegistrationDate>()),
    //                new Domain.Student.Entities.AcademicPeriod("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "2", "4", new List<Domain.Student.Entities.RegistrationDate>()),
    //                new Domain.Student.Entities.AcademicPeriod("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "3", "5", new List<Domain.Student.Entities.RegistrationDate>())
    //            };

    //            ////AdmissionApplicationStatusTypes
    //            //admissionStatusTypeEntities = new List<Domain.Student.Entities.AdmissionApplicationStatusType>()
    //            //{
    //            //    new Domain.Student.Entities.AdmissionApplicationStatusType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
    //            //    new Domain.Student.Entities.AdmissionApplicationStatusType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //            //    new Domain.Student.Entities.AdmissionApplicationStatusType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //            //    new Domain.Student.Entities.AdmissionApplicationStatusType("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //            //};

    //            ////ApplicationSources
    //            //applicationSourceEntities = new List<Domain.Student.Entities.ApplicationSource>()
    //            //{
    //            //    new Domain.Student.Entities.ApplicationSource("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
    //            //    new Domain.Student.Entities.ApplicationSource("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //            //    new Domain.Student.Entities.ApplicationSource("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //            //    new Domain.Student.Entities.ApplicationSource("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //            //};

    //            ////AdmissionPopulations
    //            //admissionPopulationEntities = new List<Domain.Student.Entities.AdmissionPopulation>()
    //            //{
    //            //    new Domain.Student.Entities.AdmissionPopulation("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
    //            //    new Domain.Student.Entities.AdmissionPopulation("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //            //    new Domain.Student.Entities.AdmissionPopulation("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //            //    new Domain.Student.Entities.AdmissionPopulation("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //            //};

    //            ////AdmissionResidencyTypes
    //            //admissionResidencyEntities = new List<Domain.Student.Entities.AdmissionResidencyType>()
    //            //{
    //            //    new Domain.Student.Entities.AdmissionResidencyType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
    //            //    new Domain.Student.Entities.AdmissionResidencyType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //            //    new Domain.Student.Entities.AdmissionResidencyType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //            //    new Domain.Student.Entities.AdmissionResidencyType("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //            //};

    //            ////AcademicLevels
    //            //academicLevelEntities = new List<Domain.Student.Entities.AcademicLevel>()
    //            //{
    //            //    new Domain.Student.Entities.AcademicLevel("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
    //            //    new Domain.Student.Entities.AcademicLevel("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //            //    new Domain.Student.Entities.AcademicLevel("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //            //    new Domain.Student.Entities.AcademicLevel("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //            //};

    //            ////AcademicPrograms
    //            //academicProgramEntities = new List<Domain.Student.Entities.AcademicProgram>()
    //            //{
    //            //    new Domain.Student.Entities.AcademicProgram("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
    //            //    new Domain.Student.Entities.AcademicProgram("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //            //    new Domain.Student.Entities.AcademicProgram("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //            //    new Domain.Student.Entities.AcademicProgram("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //            //};

    //            ////WithdrawReasons
    //            //withdrawReasonEntities = new List<Domain.Student.Entities.WithdrawReason>()
    //            //{
    //            //    new Domain.Student.Entities.WithdrawReason("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
    //            //    new Domain.Student.Entities.WithdrawReason("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //            //    new Domain.Student.Entities.WithdrawReason("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //            //    new Domain.Student.Entities.WithdrawReason("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //            //};

    //            ////Locations
    //            //locationEntities = new List<Domain.Base.Entities.Location>()
    //            //{
    //            //    new Domain.Base.Entities.Location("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
    //            //    new Domain.Base.Entities.Location("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //            //    new Domain.Base.Entities.Location("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //            //    new Domain.Base.Entities.Location("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //            //};

    //            ////AcademicDisciplines
    //            //academicDisciplineEntities = new List<Domain.Base.Entities.AcademicDiscipline>()
    //            //{
    //            //    new Domain.Base.Entities.AcademicDiscipline("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", Domain.Base.Entities.AcademicDisciplineType.Concentration),
    //            //    new Domain.Base.Entities.AcademicDiscipline("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", Domain.Base.Entities.AcademicDisciplineType.Major),
    //            //    new Domain.Base.Entities.AcademicDiscipline("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", Domain.Base.Entities.AcademicDisciplineType.Minor),
    //            //    new Domain.Base.Entities.AcademicDiscipline("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", Domain.Base.Entities.AcademicDisciplineType.Concentration)
    //            //};

    //            ////Schools
    //            //schoolEntities = new List<Domain.Base.Entities.School>()
    //            //{
    //            //    new Domain.Base.Entities.School("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
    //            //    new Domain.Base.Entities.School("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //            //    new Domain.Base.Entities.School("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //            //    new Domain.Base.Entities.School("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //            //};

    //            //Tuple
    //            housingRequestEntitiesTuple = new Tuple<IEnumerable<Domain.Student.Entities.HousingRequest>, int>(housingRequestEntities, housingRequestEntities.Count());
    //            housingRequestDtoTuple = new Tuple<IEnumerable<Dtos.HousingRequest>, int>(housingRequestDtos, housingRequestDtos.Count());
    //        }

    //        private void BuildMocks()
    //        {
    //            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

    //            housingRequestRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuids);
    //            termRepositoryMock.Setup(i => i.GetAsync()).ReturnsAsync(termEntities);
    //            termRepositoryMock.Setup(i => i.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(academicPeriodEntities);
    //            //studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionTypeEntities);
    //            //studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionApplicationStatusTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionStatusTypeEntities);
    //            //studentReferenceDataRepositoryMock.Setup(i => i.GetApplicationSourcesAsync(It.IsAny<bool>())).ReturnsAsync(applicationSourceEntities);
    //            //studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulationEntities);
    //            //studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionResidencyEntities);
    //            //studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevelEntities);
    //            //studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(academicProgramEntities);
    //            //studentReferenceDataRepositoryMock.Setup(i => i.GetWithdrawReasonsAsync(It.IsAny<bool>())).ReturnsAsync(withdrawReasonEntities);
    //            //referenceRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locationEntities);
    //            //referenceRepositoryMock.Setup(i => i.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(academicDisciplineEntities);
    //            //referenceRepositoryMock.Setup(i => i.GetSchoolsAsync(It.IsAny<bool>())).ReturnsAsync(schoolEntities);
    //        }

    //        [TestMethod]
    //        public async Task HousingRequestService__GetHousingRequestsAsync()
    //        {
    //            housingRequestRepositoryMock.Setup(i => i.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(housingRequestEntitiesTuple);
    //            var housingRequests = await housingRequestService.GetHousingRequestsAsync(offset, limit, false);

    //            Assert.IsNotNull(housingRequests);

    //            foreach (var actual in housingRequests.Item1)
    //            {
    //                var expected = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
    //                Assert.IsNotNull(expected);

    //                Assert.AreEqual(expected.Id, actual.Id);
    //                //Assert.AreEqual(expected.Applicant.Id, actual.Applicant.Id);
    //            }
    //        }

    //        [TestMethod]
    //        public async Task HousingRequestService__GetHousingRequestByGuidAsync()
    //        {
    //            var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
    //            var addmissionEntity = housingRequestEntities.FirstOrDefault(i => i.Guid.Equals(id));
    //            housingRequestRepositoryMock.Setup(i => i.GetHousingRequestByGuidAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

    //            var actual = await housingRequestService.GetHousingRequestByGuidAsync(id);

    //            Assert.IsNotNull(actual);

    //            var expected = housingRequestDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
    //            Assert.IsNotNull(expected);

    //            Assert.AreEqual(expected.Id, actual.Id);
    //            //Assert.AreEqual(expected.Applicant.Id, actual.Applicant.Id);
    //        }

    //        [TestMethod]
    //        [ExpectedException(typeof(Exception))]
    //        public async Task HousingRequests_GetById_Exception()
    //        {
    //            housingRequestRepositoryMock.Setup(i => i.GetHousingRequestByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
    //            var actual = await housingRequestService.GetHousingRequestByGuidAsync("abc");
    //        }
    //    }

    //    //[TestClass]
    //    //public class HousingRequests_POST : CurrentUserSetup
    //    //{
    //    //    Mock<IHousingRequestRepository> housingRepositoryMock;
    //    //    Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
    //    //    Mock<IAdapterRegistry> adapterRegistryMock;
    //    //    Mock<IReferenceDataRepository> referenceRepositoryMock;
    //    //    Mock<IPersonRepository> personRepositoryMock;
    //    //    Mock<ITermRepository> termRepositoryMock;
    //    //    Mock<IRoomRepository> roomRepositoryMock;
    //    //    Mock<ICurrentUserFactory> userFactoryMock;
    //    //    ICurrentUserFactory userFactory;
    //    //    Mock<IRoleRepository> roleRepoMock;
    //    //    //Mock<IPersonBaseRepository> personBaseRepositoryMock;
    //    //    private ILogger logger;

    //    //    HousingRequestsService housingRequestService;

    //    //    IEnumerable<Domain.Student.Entities.HousingRequest> housingEntities;
    //    //    IEnumerable<Dtos.HousingRequest> housingDtos;
    //    //    Dictionary<string, string> personGuids = new Dictionary<string, string>();
    //    //    IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriodEntities;
    //    //    IEnumerable<Domain.Student.Entities.Term> termEntities;
    //    //    IEnumerable<Domain.Base.Entities.RoomCharacteristic> roomCharacteristicEntities;
    //    //    IEnumerable<Domain.Student.Entities.RoommateCharacteristics> roommateCharacteristicEntities;
    //    //    IEnumerable<Domain.Student.Entities.FloorCharacteristics> floorCharacteristicEntities;
    //    //    IEnumerable<Domain.Base.Entities.Location> locationEntities;
    //    //    IEnumerable<Domain.Base.Entities.Room> roomEntities;
    //    //    IEnumerable<Domain.Base.Entities.RoomWing> roomWingEntities;
    //    //    IEnumerable<Domain.Base.Entities.Building> buildingEntities;

    //    //    Tuple<IEnumerable<Domain.Student.Entities.HousingRequest>, int> housingEntitiesTuple;
    //    //    Tuple<IEnumerable<Dtos.HousingRequest>, int> housingDtoTuple;

    //    //    int offset = 0;
    //    //    int limit = 200;

    //    //    private Domain.Entities.Permission permissionCreateAnyApplication;


    //    //    [TestInitialize]
    //    //    public void Initialize()
    //    //    {
    //    //        housingRepositoryMock = new Mock<IHousingRequestRepository>();
    //    //        studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
    //    //        personRepositoryMock = new Mock<IPersonRepository>();
    //    //        termRepositoryMock = new Mock<ITermRepository>();
    //    //        referenceRepositoryMock = new Mock<IReferenceDataRepository>();
    //    //        roomRepositoryMock = new Mock<IRoomRepository>();
    //    //        roleRepoMock = new Mock<IRoleRepository>();
    //    //        userFactoryMock = new Mock<ICurrentUserFactory>();
    //    //        adapterRegistryMock = new Mock<IAdapterRegistry>();
    //    //        logger = new Mock<ILogger>().Object;

    //    //        // Set up current user
    //    //        userFactory = userFactoryMock.Object;
    //    //        userFactory = new CurrentUserSetup.PersonUserFactory();
    //    //        // Mock permissions
    //    //        permissionCreateAnyApplication = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateHousingRequest);
    //    //        personRole.AddPermission(permissionCreateAnyApplication);

    //    //        BuildData();
    //    //        BuildMocks();

    //    //        housingRequestService = new HousingRequestsService(housingRepositoryMock.Object, personRepositoryMock.Object, termRepositoryMock.Object, roomRepositoryMock.Object, referenceRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, adapterRegistryMock.Object, userFactory, roleRepoMock.Object, logger);
    //    //    }

    //    //    [TestCleanup]
    //    //    public void Cleanup()
    //    //    {
    //    //        housingRepositoryMock = null;
    //    //        studentReferenceDataRepositoryMock = null;
    //    //        housingEntities = null;
    //    //        housingDtos = null;
    //    //    }

    //    //    private void BuildData()
    //    //    {
    //    //        //Dtos
    //    //        housingDtos = new List<Dtos.HousingRequest>()
    //    //        {
    //    //            new Dtos.HousingRequest()
    //    //            {
    //    //                Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
    //    //                Person = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
    //    //                Status = HousingRequestsStatus.NotSet,
    //    //                StartOn = new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)),
    //    //                AcademicPeriods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f") },
    //    //                Preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>() 
    //    //                { 
    //    //                    new Dtos.DtoProperties.HousingRequestPreferenceProperty() 
    //    //                    { 
    //    //                        Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Wing = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Floor = new Dtos.DtoProperties.HousingFloorPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = "e0c0c94c-53a7-46b7-96c4-76b12512c323", 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        } 
    //    //                    } 
    //    //                }
    //    //            },
    //    //            new Dtos.HousingRequest()
    //    //            {
    //    //                Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
    //    //                Person = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
    //    //                Status = HousingRequestsStatus.Rejected,
    //    //                StartOn = new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)),
    //    //                AcademicPeriods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f") },
    //    //                Preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>() 
    //    //                { 
    //    //                    new Dtos.DtoProperties.HousingRequestPreferenceProperty() 
    //    //                    { 
    //    //                        Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Room = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        } 
    //    //                    } 
    //    //                }
    //    //            },
    //    //            new Dtos.HousingRequest()
    //    //            {
    //    //                Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
    //    //                Person = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
    //    //                Status = HousingRequestsStatus.Submitted,
    //    //                StartOn = new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)),
    //    //                AcademicPeriods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f") },
    //    //                Preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>() 
    //    //                { 
    //    //                    new Dtos.DtoProperties.HousingRequestPreferenceProperty() 
    //    //                    { 
    //    //                        Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Wing = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Floor = new Dtos.DtoProperties.HousingFloorPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = "e0c0c94c-53a7-46b7-96c4-76b12512c323", 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        } 
    //    //                    } 
    //    //                }
    //    //            },
    //    //            new Dtos.HousingRequest()
    //    //            {
    //    //                Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
    //    //                Person = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
    //    //                Status = HousingRequestsStatus.Withdrawn,
    //    //                StartOn = new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)),
    //    //                AcademicPeriods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f") },
    //    //                Preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>() 
    //    //                { 
    //    //                    new Dtos.DtoProperties.HousingRequestPreferenceProperty() 
    //    //                    { 
    //    //                        Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        Room = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        }
    //    //                    } 
    //    //                },
    //    //                RoomCharacteristics = new List<Dtos.DtoProperties.HousingPreferenceRequiredProperty>() {
    //    //                    new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
    //    //                    {
    //    //                        Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                        Required = RequiredPreference.Mandatory 
    //    //                    }
    //    //                },
    //    //                FloorCharacteristics = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                {
    //    //                    Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                    Required = RequiredPreference.Mandatory 
    //    //                },
    //    //                RoommatePreferences = new List<Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty>() {
    //    //                    new Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty()
    //    //                    {
    //    //                        Roommate = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        },
    //    //                        RoommateCharacteristic = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
    //    //                        { 
    //    //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
    //    //                            Required = RequiredPreference.Mandatory 
    //    //                        }
                                 
    //    //                    }
    //    //                },
    //    //            },
    //    //        };

    //    //        //Entities
    //    //        housingEntities = new List<Domain.Student.Entities.HousingRequest>()
    //    //        {
    //    //            new Domain.Student.Entities.HousingRequest("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", "1", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "R")
    //    //            {
    //    //                Term = "CODE1",
    //    //                PersonId = "1",
    //    //            },
    //    //            new Domain.Student.Entities.HousingRequest("3f67b180-ce1d-4552-8d81-feb96b9fea5b", "1", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "A")
    //    //            {
    //    //                Term = "CODE2",
    //    //                PersonId = "1",
    //    //            },
    //    //            new Domain.Student.Entities.HousingRequest("bf67e156-8f5d-402b-8101-81b0a2796873", "3", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "C")
    //    //            {
    //    //                Term = "CODE3",
    //    //                PersonId = "2",
    //    //            },
    //    //            new Domain.Student.Entities.HousingRequest("0111d6ef-5a86-465f-ac58-4265a997c136", "3", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "T")
    //    //            {
    //    //                Term = "CODE4",
    //    //                PersonId = "2",
    //    //            },
    //    //        };

    //    //        //Persons
    //    //        personGuids.Add("1", "d190d4b5-03b5-41aa-99b8-b8286717c956");
    //    //        personGuids.Add("2", "cecdce5a-54a7-45fb-a975-5392a579e5bf");

    //    //        //Terms
    //    //        termEntities = new List<Domain.Student.Entities.Term>()
    //    //        {
    //    //            new Domain.Student.Entities.Term("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", new DateTime(2016, 02, 01), new DateTime(2016, 03, 01), 2016, 1, false, false, "WINTER", false),
    //    //            new Domain.Student.Entities.Term("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", new DateTime(2016, 03, 01), new DateTime(2016, 04, 01), 2016, 2, false, false, "WINTER", false),
    //    //            new Domain.Student.Entities.Term("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", new DateTime(2016, 04, 01), new DateTime(2016, 05, 01), 2016, 3, false, false, "WINTER", false),
    //    //            new Domain.Student.Entities.Term("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, false, false, "WINTER", false)
    //    //        };

    //    //        ////AcademicPeriods
    //    //        academicPeriodEntities = new List<Domain.Student.Entities.AcademicPeriod>()
    //    //        {
    //    //            new Domain.Student.Entities.AcademicPeriod("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "0", "2", new List<Domain.Student.Entities.RegistrationDate>()),
    //    //            new Domain.Student.Entities.AcademicPeriod("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "1", "3", new List<Domain.Student.Entities.RegistrationDate>()),
    //    //            new Domain.Student.Entities.AcademicPeriod("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "2", "4", new List<Domain.Student.Entities.RegistrationDate>()),
    //    //            new Domain.Student.Entities.AcademicPeriod("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "3", "5", new List<Domain.Student.Entities.RegistrationDate>())
    //    //        };

    //    //        ////RoomCharacteristics
    //    //        roomCharacteristicEntities = new List<Domain.Base.Entities.RoomCharacteristic>()
    //    //        {
    //    //            new Domain.Base.Entities.RoomCharacteristic("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
    //    //            new Domain.Base.Entities.RoomCharacteristic("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //    //            new Domain.Base.Entities.RoomCharacteristic("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3")
    //    //        };

    //    //        ////RoommateCharacteristics
    //    //        roommateCharacteristicEntities = new List<Domain.Student.Entities.RoommateCharacteristics>()
    //    //        {
    //    //            new Domain.Student.Entities.RoommateCharacteristics("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
    //    //            new Domain.Student.Entities.RoommateCharacteristics("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //    //            new Domain.Student.Entities.RoommateCharacteristics("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3")
    //    //        };

    //    //        //FloorCharacteristics
    //    //        floorCharacteristicEntities = new List<Domain.Student.Entities.FloorCharacteristics>()
    //    //        {
    //    //            new Domain.Student.Entities.FloorCharacteristics("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
    //    //            new Domain.Student.Entities.FloorCharacteristics("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //    //            new Domain.Student.Entities.FloorCharacteristics("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3")
    //    //        };

    //    //        //Locations
    //    //        locationEntities = new List<Domain.Base.Entities.Location>()
    //    //        {
    //    //            new Domain.Base.Entities.Location("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
    //    //            new Domain.Base.Entities.Location("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //    //            new Domain.Base.Entities.Location("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //    //            new Domain.Base.Entities.Location("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //    //        };

    //    //        //Rooms
    //    //        roomEntities = new List<Domain.Base.Entities.Room>()
    //    //        {
    //    //            new Domain.Base.Entities.Room("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1*CODE1", "DESC1"),
    //    //            new Domain.Base.Entities.Room("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2*CODE2", "DESC2"),
    //    //            new Domain.Base.Entities.Room("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3*CODE3", "DESC3"),
    //    //            new Domain.Base.Entities.Room("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4*CODE4", "DESC4")
    //    //        };

    //    //        //RoomWings
    //    //        roomWingEntities = new List<Domain.Base.Entities.RoomWing>()
    //    //        {
    //    //            new Domain.Base.Entities.RoomWing("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
    //    //            new Domain.Base.Entities.RoomWing("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
    //    //            new Domain.Base.Entities.RoomWing("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
    //    //            new Domain.Base.Entities.RoomWing("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
    //    //        };

    //    //        //Buildings
    //    //        buildingEntities = new List<Domain.Base.Entities.Building>()
    //    //        {
    //    //            new Domain.Base.Entities.Building("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1", "CODE1", "BT", "LD", new List<string>() { "ALINE" }, "CI", "ST", "PC", "CO", (decimal) 2.0, (decimal) 2.0, "IU", "AS", "VFM"),
    //    //            new Domain.Base.Entities.Building("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", "CODE2", "BT", "LD", new List<string>() { "ALINE" }, "CI", "ST", "PC", "CO", (decimal) 2.0, (decimal) 2.0, "IU", "AS", "VFM"),
    //    //            new Domain.Base.Entities.Building("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", "CODE3", "BT", "LD", new List<string>() { "ALINE" }, "CI", "ST", "PC", "CO", (decimal) 2.0, (decimal) 2.0, "IU", "AS", "VFM"),
    //    //            new Domain.Base.Entities.Building("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", "CODE4", "BT", "LD", new List<string>() { "ALINE" }, "CI", "ST", "PC", "CO", (decimal) 2.0, (decimal) 2.0, "IU", "AS", "VFM")
    //    //        };

    //    //        //Tuple
    //    //        housingEntitiesTuple = new Tuple<IEnumerable<Domain.Student.Entities.HousingRequest>, int>(housingEntities, housingEntities.Count());
    //    //        housingDtoTuple = new Tuple<IEnumerable<Dtos.HousingRequest>, int>(housingDtos, housingDtos.Count());
    //    //    }

    //    //    private void BuildMocks()
    //    //    {
    //    //        roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

    //    //        housingRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuids);
    //    //        personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personGuids.FirstOrDefault().Key);
    //    //        housingRepositoryMock.Setup(i => i.GetHousingRequestKeyAsync(It.IsAny<string>())).ReturnsAsync(housingEntities.FirstOrDefault().RecordKey);
    //    //        termRepositoryMock.Setup(i => i.GetAsync()).ReturnsAsync(termEntities);
    //    //        termRepositoryMock.Setup(i => i.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(academicPeriodEntities);
    //    //        referenceRepositoryMock.Setup(i => i.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roomCharacteristicEntities);
    //    //        studentReferenceDataRepositoryMock.Setup(i => i.GetRoommateCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roommateCharacteristicEntities);
    //    //        studentReferenceDataRepositoryMock.Setup(i => i.GetFloorCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(floorCharacteristicEntities);
    //    //        referenceRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locationEntities);
    //    //        referenceRepositoryMock.Setup(i => i.GetRoomWingsAsync(It.IsAny<bool>())).ReturnsAsync(roomWingEntities);
    //    //        referenceRepositoryMock.Setup(i => i.GetBuildingsAsync(It.IsAny<bool>())).ReturnsAsync(buildingEntities);
    //    //        roomRepositoryMock.Setup(i => i.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(roomEntities);
    //    //    }

    //    //    [TestMethod]
    //    //    public async Task HousingRequestService__UpdateHousingRequestByGuidAsync()
    //    //    {
    //    //        var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
    //    //        var housingEntity = housingEntities.FirstOrDefault(i => i.Guid.Equals(id));
    //    //        housingRepositoryMock.Setup(i => i.UpdateHousingRequestAsync(It.IsAny<Domain.Student.Entities.HousingRequest>())).ReturnsAsync(housingEntity);
                                
    //    //        var expected = housingDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    //    //        Assert.IsNotNull(expected);

    //    //        var actual = await housingRequestService.UpdateHousingRequestAsync(id, expected);

    //    //        Assert.IsNotNull(actual);

    //    //        Assert.AreEqual(expected.Id, actual.Id);
    //    //    }

    //    //}
    //}
}
