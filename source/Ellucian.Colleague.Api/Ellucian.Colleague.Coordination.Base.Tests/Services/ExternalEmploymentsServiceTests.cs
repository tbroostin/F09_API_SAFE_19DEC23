// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using System;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class ExternalEmploymentsServiceTests
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class ExternalEmployments : CurrentUserSetup
        {
            Mock<IExternalEmploymentsRepository> externalEmploymentRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IReferenceDataRepository> referenceRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            //Mock<ITermRepository> termRepositoryMock;
            Mock<IRoomRepository> roomRepositoryMock;
            Mock<ICurrentUserFactory> userFactoryMock;
            ICurrentUserFactory userFactory;
            Mock<IRoleRepository> roleRepoMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            //Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private ILogger logger;

            ExternalEmploymentsService externalEmploymentService;

            IEnumerable<Domain.Base.Entities.ExternalEmployments> externalEmploymentEntities;
            IEnumerable<Dtos.ExternalEmployments> externalEmploymentDtos;
            Dictionary<string, string> personGuids = new Dictionary<string, string>();

            private IEnumerable<Positions> _externalPositions;
            private IEnumerable<ExternalEmploymentStatus> _externalEmployStatuses;
            private IEnumerable<Vocation> _employmentVocations;
            //IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriodEntities;
            //IEnumerable<Domain.Student.Entities.Term> termEntities;
            //IEnumerable<Domain.Student.Entities.AdmissionApplicationStatusType> admissionStatusTypeEntities;
            //IEnumerable<Domain.Student.Entities.ApplicationSource> applicationSourceEntities;
            //IEnumerable<Domain.Student.Entities.AdmissionPopulation> admissionPopulationEntities;
            //IEnumerable<Domain.Student.Entities.AdmissionResidencyType> admissionResidencyEntities;
            //IEnumerable<Domain.Student.Entities.AcademicLevel> academicLevelEntities;
            //IEnumerable<Domain.Student.Entities.AcademicProgram> academicProgramEntities;
            //IEnumerable<Domain.Student.Entities.WithdrawReason> withdrawReasonEntities;
            //IEnumerable<Domain.Base.Entities.Location> locationEntities;
            //IEnumerable<Domain.Base.Entities.AcademicDiscipline> academicDisciplineEntities;
            //IEnumerable<Domain.Base.Entities.School> schoolEntities;

            Tuple<IEnumerable<Domain.Base.Entities.ExternalEmployments>, int> externalEmploymentEntitiesTuple;
            Tuple<IEnumerable<Dtos.ExternalEmployments>, int> externalEmploymentDtoTuple;

            int offset = 0;
            int limit = 200;

            private Domain.Entities.Permission permissionViewAnyApplication;


            [TestInitialize]
            public void Initialize()
            {
                externalEmploymentRepositoryMock = new Mock<IExternalEmploymentsRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                //termRepositoryMock = new Mock<ITermRepository>();
                referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                roomRepositoryMock = new Mock<IRoomRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                userFactoryMock = new Mock<ICurrentUserFactory>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;

                // Set up current user
                userFactory = userFactoryMock.Object;
                userFactory = new CurrentUserSetup.PersonUserFactory();
                // Mock permissions
                permissionViewAnyApplication = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyExternalEmployments);
                personRole.AddPermission(permissionViewAnyApplication);

                BuildData();
                BuildMocks();

                externalEmploymentService = new ExternalEmploymentsService(externalEmploymentRepositoryMock.Object, adapterRegistryMock.Object, personRepositoryMock.Object, referenceRepositoryMock.Object, userFactory, roleRepoMock.Object, configurationRepositoryMock.Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                externalEmploymentRepositoryMock = null;
                referenceDataRepositoryMock = null;
                externalEmploymentEntities = null;
                externalEmploymentDtos = null;
            }

            private void BuildData()
            {
                //Dtos
                externalEmploymentDtos = new List<Dtos.ExternalEmployments>()
                {
                    new Dtos.ExternalEmployments()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        //Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                        //    new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                        //        AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"),
                        //        AdmissionApplicationsStatusStartOn = new DateTime(2016, 09, 03),
                        //        AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.DecisionMade
                        //    }
                        //},
                        //Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.ExternalEmployments()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        //Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                        //    new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                        //        AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        //        AdmissionApplicationsStatusStartOn = new DateTime(2016, 09, 01),
                        //        AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.EnrollmentCompleted
                        //    }
                        //},
                        //Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.ExternalEmployments()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        //Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                        //    new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                        //        AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        //        AdmissionApplicationsStatusStartOn = new DateTime(2016, 04, 01),
                        //        AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.ReadyForReview
                        //    }
                        //},
                        //Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.ExternalEmployments()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        //Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                        //    new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                        //        AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"),
                        //        AdmissionApplicationsStatusStartOn = new DateTime(2016, 02, 01),
                        //        AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.ReadyForReview
                        //    }
                        //},
                        //Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };

                //Entities
                externalEmploymentEntities = new List<Domain.Base.Entities.ExternalEmployments>()
                {
                    new Domain.Base.Entities.ExternalEmployments("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", "1", "1", "DESC1", "CODE1")
                    {
                        PositionId = "CODE1",
                        OrganizationId = "ORG1",
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now,
                        JobTitle = "JOB",
                        PrincipalEmployment = "Y",
                        HoursWorked = (decimal) 40,
                        Vocations = new List<string>() { "CODE1", "CODE2", "CODE3" },
                        comments = "COMMENT",
                        Supervisors = new List<ExternalEmploymentSupervisors>() 
                        {
                            new ExternalEmploymentSupervisors("first", "last", "phone", "email")
                        }
                    },
                    new Domain.Base.Entities.ExternalEmployments("3f67b180-ce1d-4552-8d81-feb96b9fea5b", "2", "2", "DESC2", "CODE2")
                    {
                        PositionId = "CODE2",
                        selfEmployed = "Y",
                        PrincipalEmployment = "N",
                        Supervisors = new List<ExternalEmploymentSupervisors>() 
                        {
                            new ExternalEmploymentSupervisors("first", "", "phone", "email")
                        }
                    },
                    new Domain.Base.Entities.ExternalEmployments("bf67e156-8f5d-402b-8101-81b0a2796873", "3", "3", "DESC3", "CODE3")
                    {
                        PositionId = "CODE3",
                        unknownEmployer = "Y",
                        Supervisors = new List<ExternalEmploymentSupervisors>() 
                        {
                            new ExternalEmploymentSupervisors("", "last", "phone", "email")
                        }
                    },
                    new Domain.Base.Entities.ExternalEmployments("0111d6ef-5a86-465f-ac58-4265a997c136", "4", "4", "DESC4", "CODE4")
                    {
                        PositionId = "CODE1",
                    },
                };

                //Persons
                personGuids.Add("1", "d190d4b5-03b5-41aa-99b8-b8286717c956");
                personGuids.Add("2", "cecdce5a-54a7-45fb-a975-5392a579e5bf");

                //External Positions
                _externalPositions = new List<Positions>()
                {
                    new Positions("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Positions("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Positions("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Positions("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4"),
                };

                //External Employment Status
                _externalEmployStatuses = new List<ExternalEmploymentStatus>()
                {
                    new ExternalEmploymentStatus("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new ExternalEmploymentStatus("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new ExternalEmploymentStatus("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new ExternalEmploymentStatus("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4"),
                };

                //Vocation
                _employmentVocations = new List<Vocation>()
                {
                    new Vocation("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Vocation("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Vocation("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Vocation("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4"),
                };

                //Tuple
                externalEmploymentEntitiesTuple = new Tuple<IEnumerable<Domain.Base.Entities.ExternalEmployments>, int>(externalEmploymentEntities, externalEmploymentEntities.Count());
                externalEmploymentDtoTuple = new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentDtos, externalEmploymentDtos.Count());
            }

            private void BuildMocks()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                referenceRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<bool>())).ReturnsAsync(_externalPositions);
                referenceRepositoryMock.Setup(i => i.GetExternalEmploymentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(_externalEmployStatuses);
                referenceRepositoryMock.Setup(i => i.GetVocationsAsync(It.IsAny<bool>())).ReturnsAsync(_employmentVocations);

                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(personGuids.FirstOrDefault().Value);

                var personGuidDictionary = new Dictionary<string, string>() { };
                personGuidDictionary.Add("1", "1dd56e2d-9b99-4a5b-ab84-55131a31f2e3");
                personGuidDictionary.Add("2", "a7cbdbbe-131e-4b91-9c99-d9b65c41f1c8");
                personGuidDictionary.Add("3", "ae91ddf9-0b25-4008-97c5-76ac5fe570a3");
                personGuidDictionary.Add("4", "9we1ddf9-0b25-4008-97c5-76ac5fe570a3");
                personGuidDictionary.Add("ORG1", "149195b8-fe43-4538-aa90-16fbe240a2d5");

                personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>()))
                    .ReturnsAsync(personGuidDictionary);
            }

            [TestMethod]
            public async Task ExternalEmploymentService__GetExternalEmploymentsAsync()
            {
                externalEmploymentRepositoryMock.Setup(i => i.GetExternalEmploymentsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(externalEmploymentEntitiesTuple);
                var externalEmployments = await externalEmploymentService.GetExternalEmploymentsAsync(offset, limit, false);

                Assert.IsNotNull(externalEmployments);

                foreach (var actual in externalEmployments.Item1)
                {
                    var expected = externalEmploymentDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    //Assert.AreEqual(expected.Applicant.Id, actual.Applicant.Id);
                }
            }

            [TestMethod]
            public async Task ExternalEmploymentService__GetExternalEmploymentByGuidAsync()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = externalEmploymentEntities.FirstOrDefault(i => i.Guid.Equals(id));
                externalEmploymentRepositoryMock.Setup(i => i.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                var actual = await externalEmploymentService.GetExternalEmploymentsByGuidAsync(id);

                Assert.IsNotNull(actual);

                var expected = externalEmploymentDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                //Assert.AreEqual(expected.Applicant.Id, actual.Applicant.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ExternalEmployments_GetById_Exception()
            {
                externalEmploymentRepositoryMock.Setup(i => i.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actual = await externalEmploymentService.GetExternalEmploymentsByGuidAsync("abc");
            }
        }

        //[TestClass]
        //public class HousingRequests_POST : CurrentUserSetup
        //{
        //    Mock<IHousingRequestRepository> housingRepositoryMock;
        //    Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        //    Mock<IAdapterRegistry> adapterRegistryMock;
        //    Mock<IReferenceDataRepository> referenceRepositoryMock;
        //    Mock<IPersonRepository> personRepositoryMock;
        //    Mock<ITermRepository> termRepositoryMock;
        //    Mock<IRoomRepository> roomRepositoryMock;
        //    Mock<ICurrentUserFactory> userFactoryMock;
        //    ICurrentUserFactory userFactory;
        //    Mock<IRoleRepository> roleRepoMock;
        //    //Mock<IPersonBaseRepository> personBaseRepositoryMock;
        //    private ILogger logger;

        //    HousingRequestsService housingRequestService;

        //    IEnumerable<Domain.Student.Entities.HousingRequest> housingEntities;
        //    IEnumerable<Dtos.HousingRequest> housingDtos;
        //    Dictionary<string, string> personGuids = new Dictionary<string, string>();
        //    IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriodEntities;
        //    IEnumerable<Domain.Student.Entities.Term> termEntities;
        //    IEnumerable<Domain.Base.Entities.RoomCharacteristic> roomCharacteristicEntities;
        //    IEnumerable<Domain.Student.Entities.RoommateCharacteristics> roommateCharacteristicEntities;
        //    IEnumerable<Domain.Student.Entities.FloorCharacteristics> floorCharacteristicEntities;
        //    IEnumerable<Domain.Base.Entities.Location> locationEntities;
        //    IEnumerable<Domain.Base.Entities.Room> roomEntities;
        //    IEnumerable<Domain.Base.Entities.RoomWing> roomWingEntities;
        //    IEnumerable<Domain.Base.Entities.Building> buildingEntities;

        //    Tuple<IEnumerable<Domain.Student.Entities.HousingRequest>, int> housingEntitiesTuple;
        //    Tuple<IEnumerable<Dtos.HousingRequest>, int> housingDtoTuple;

        //    int offset = 0;
        //    int limit = 200;

        //    private Domain.Entities.Permission permissionCreateAnyApplication;


        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        housingRepositoryMock = new Mock<IHousingRequestRepository>();
        //        studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
        //        personRepositoryMock = new Mock<IPersonRepository>();
        //        termRepositoryMock = new Mock<ITermRepository>();
        //        referenceRepositoryMock = new Mock<IReferenceDataRepository>();
        //        roomRepositoryMock = new Mock<IRoomRepository>();
        //        roleRepoMock = new Mock<IRoleRepository>();
        //        userFactoryMock = new Mock<ICurrentUserFactory>();
        //        adapterRegistryMock = new Mock<IAdapterRegistry>();
        //        logger = new Mock<ILogger>().Object;

        //        // Set up current user
        //        userFactory = userFactoryMock.Object;
        //        userFactory = new CurrentUserSetup.PersonUserFactory();
        //        // Mock permissions
        //        permissionCreateAnyApplication = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateHousingRequest);
        //        personRole.AddPermission(permissionCreateAnyApplication);

        //        BuildData();
        //        BuildMocks();

        //        housingRequestService = new HousingRequestsService(housingRepositoryMock.Object, personRepositoryMock.Object, termRepositoryMock.Object, roomRepositoryMock.Object, referenceRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, adapterRegistryMock.Object, userFactory, roleRepoMock.Object, logger);
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        housingRepositoryMock = null;
        //        studentReferenceDataRepositoryMock = null;
        //        housingEntities = null;
        //        housingDtos = null;
        //    }

        //    private void BuildData()
        //    {
        //        //Dtos
        //        housingDtos = new List<Dtos.HousingRequest>()
        //        {
        //            new Dtos.HousingRequest()
        //            {
        //                Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
        //                Person = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
        //                Status = HousingRequestsStatus.NotSet,
        //                StartOn = new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)),
        //                AcademicPeriods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f") },
        //                Preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>() 
        //                { 
        //                    new Dtos.DtoProperties.HousingRequestPreferenceProperty() 
        //                    { 
        //                        Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Wing = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Floor = new Dtos.DtoProperties.HousingFloorPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = "e0c0c94c-53a7-46b7-96c4-76b12512c323", 
        //                            Required = RequiredPreference.Mandatory 
        //                        } 
        //                    } 
        //                }
        //            },
        //            new Dtos.HousingRequest()
        //            {
        //                Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
        //                Person = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
        //                Status = HousingRequestsStatus.Rejected,
        //                StartOn = new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)),
        //                AcademicPeriods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f") },
        //                Preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>() 
        //                { 
        //                    new Dtos.DtoProperties.HousingRequestPreferenceProperty() 
        //                    { 
        //                        Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Room = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        } 
        //                    } 
        //                }
        //            },
        //            new Dtos.HousingRequest()
        //            {
        //                Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
        //                Person = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
        //                Status = HousingRequestsStatus.Submitted,
        //                StartOn = new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)),
        //                AcademicPeriods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f") },
        //                Preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>() 
        //                { 
        //                    new Dtos.DtoProperties.HousingRequestPreferenceProperty() 
        //                    { 
        //                        Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Wing = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Floor = new Dtos.DtoProperties.HousingFloorPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = "e0c0c94c-53a7-46b7-96c4-76b12512c323", 
        //                            Required = RequiredPreference.Mandatory 
        //                        } 
        //                    } 
        //                }
        //            },
        //            new Dtos.HousingRequest()
        //            {
        //                Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
        //                Person = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
        //                Status = HousingRequestsStatus.Withdrawn,
        //                StartOn = new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)),
        //                AcademicPeriods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f") },
        //                Preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>() 
        //                { 
        //                    new Dtos.DtoProperties.HousingRequestPreferenceProperty() 
        //                    { 
        //                        Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        Room = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        }
        //                    } 
        //                },
        //                RoomCharacteristics = new List<Dtos.DtoProperties.HousingPreferenceRequiredProperty>() {
        //                    new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
        //                    {
        //                        Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                        Required = RequiredPreference.Mandatory 
        //                    }
        //                },
        //                FloorCharacteristics = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                {
        //                    Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                    Required = RequiredPreference.Mandatory 
        //                },
        //                RoommatePreferences = new List<Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty>() {
        //                    new Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty()
        //                    {
        //                        Roommate = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        },
        //                        RoommateCharacteristic = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
        //                        { 
        //                            Preferred = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"), 
        //                            Required = RequiredPreference.Mandatory 
        //                        }

        //                    }
        //                },
        //            },
        //        };

        //        //Entities
        //        housingEntities = new List<Domain.Student.Entities.HousingRequest>()
        //        {
        //            new Domain.Student.Entities.HousingRequest("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", "1", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "R")
        //            {
        //                Term = "CODE1",
        //                PersonId = "1",
        //            },
        //            new Domain.Student.Entities.HousingRequest("3f67b180-ce1d-4552-8d81-feb96b9fea5b", "1", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "A")
        //            {
        //                Term = "CODE2",
        //                PersonId = "1",
        //            },
        //            new Domain.Student.Entities.HousingRequest("bf67e156-8f5d-402b-8101-81b0a2796873", "3", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "C")
        //            {
        //                Term = "CODE3",
        //                PersonId = "2",
        //            },
        //            new Domain.Student.Entities.HousingRequest("0111d6ef-5a86-465f-ac58-4265a997c136", "3", new DateTimeOffset(2015, 12, 23, 01, 23, 43, new TimeSpan(12, 00, 00)), "T")
        //            {
        //                Term = "CODE4",
        //                PersonId = "2",
        //            },
        //        };

        //        //Persons
        //        personGuids.Add("1", "d190d4b5-03b5-41aa-99b8-b8286717c956");
        //        personGuids.Add("2", "cecdce5a-54a7-45fb-a975-5392a579e5bf");

        //        //Terms
        //        termEntities = new List<Domain.Student.Entities.Term>()
        //        {
        //            new Domain.Student.Entities.Term("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", new DateTime(2016, 02, 01), new DateTime(2016, 03, 01), 2016, 1, false, false, "WINTER", false),
        //            new Domain.Student.Entities.Term("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", new DateTime(2016, 03, 01), new DateTime(2016, 04, 01), 2016, 2, false, false, "WINTER", false),
        //            new Domain.Student.Entities.Term("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", new DateTime(2016, 04, 01), new DateTime(2016, 05, 01), 2016, 3, false, false, "WINTER", false),
        //            new Domain.Student.Entities.Term("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, false, false, "WINTER", false)
        //        };

        //        ////AcademicPeriods
        //        academicPeriodEntities = new List<Domain.Student.Entities.AcademicPeriod>()
        //        {
        //            new Domain.Student.Entities.AcademicPeriod("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "0", "2", new List<Domain.Student.Entities.RegistrationDate>()),
        //            new Domain.Student.Entities.AcademicPeriod("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "1", "3", new List<Domain.Student.Entities.RegistrationDate>()),
        //            new Domain.Student.Entities.AcademicPeriod("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "2", "4", new List<Domain.Student.Entities.RegistrationDate>()),
        //            new Domain.Student.Entities.AcademicPeriod("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, "WINTER", "3", "5", new List<Domain.Student.Entities.RegistrationDate>())
        //        };

        //        ////RoomCharacteristics
        //        roomCharacteristicEntities = new List<Domain.Base.Entities.RoomCharacteristic>()
        //        {
        //            new Domain.Base.Entities.RoomCharacteristic("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
        //            new Domain.Base.Entities.RoomCharacteristic("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
        //            new Domain.Base.Entities.RoomCharacteristic("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3")
        //        };

        //        ////RoommateCharacteristics
        //        roommateCharacteristicEntities = new List<Domain.Student.Entities.RoommateCharacteristics>()
        //        {
        //            new Domain.Student.Entities.RoommateCharacteristics("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
        //            new Domain.Student.Entities.RoommateCharacteristics("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
        //            new Domain.Student.Entities.RoommateCharacteristics("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3")
        //        };

        //        //FloorCharacteristics
        //        floorCharacteristicEntities = new List<Domain.Student.Entities.FloorCharacteristics>()
        //        {
        //            new Domain.Student.Entities.FloorCharacteristics("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
        //            new Domain.Student.Entities.FloorCharacteristics("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
        //            new Domain.Student.Entities.FloorCharacteristics("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3")
        //        };

        //        //Locations
        //        locationEntities = new List<Domain.Base.Entities.Location>()
        //        {
        //            new Domain.Base.Entities.Location("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
        //            new Domain.Base.Entities.Location("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
        //            new Domain.Base.Entities.Location("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
        //            new Domain.Base.Entities.Location("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
        //        };

        //        //Rooms
        //        roomEntities = new List<Domain.Base.Entities.Room>()
        //        {
        //            new Domain.Base.Entities.Room("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1*CODE1", "DESC1"),
        //            new Domain.Base.Entities.Room("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2*CODE2", "DESC2"),
        //            new Domain.Base.Entities.Room("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3*CODE3", "DESC3"),
        //            new Domain.Base.Entities.Room("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4*CODE4", "DESC4")
        //        };

        //        //RoomWings
        //        roomWingEntities = new List<Domain.Base.Entities.RoomWing>()
        //        {
        //            new Domain.Base.Entities.RoomWing("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1"),
        //            new Domain.Base.Entities.RoomWing("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
        //            new Domain.Base.Entities.RoomWing("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
        //            new Domain.Base.Entities.RoomWing("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
        //        };

        //        //Buildings
        //        buildingEntities = new List<Domain.Base.Entities.Building>()
        //        {
        //            new Domain.Base.Entities.Building("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1", "CODE1", "BT", "LD", new List<string>() { "ALINE" }, "CI", "ST", "PC", "CO", (decimal) 2.0, (decimal) 2.0, "IU", "AS", "VFM"),
        //            new Domain.Base.Entities.Building("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", "CODE2", "BT", "LD", new List<string>() { "ALINE" }, "CI", "ST", "PC", "CO", (decimal) 2.0, (decimal) 2.0, "IU", "AS", "VFM"),
        //            new Domain.Base.Entities.Building("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", "CODE3", "BT", "LD", new List<string>() { "ALINE" }, "CI", "ST", "PC", "CO", (decimal) 2.0, (decimal) 2.0, "IU", "AS", "VFM"),
        //            new Domain.Base.Entities.Building("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", "CODE4", "BT", "LD", new List<string>() { "ALINE" }, "CI", "ST", "PC", "CO", (decimal) 2.0, (decimal) 2.0, "IU", "AS", "VFM")
        //        };

        //        //Tuple
        //        housingEntitiesTuple = new Tuple<IEnumerable<Domain.Student.Entities.HousingRequest>, int>(housingEntities, housingEntities.Count());
        //        housingDtoTuple = new Tuple<IEnumerable<Dtos.HousingRequest>, int>(housingDtos, housingDtos.Count());
        //    }

        //    private void BuildMocks()
        //    {
        //        roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

        //        housingRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuids);
        //        personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personGuids.FirstOrDefault().Key);
        //        housingRepositoryMock.Setup(i => i.GetHousingRequestKeyAsync(It.IsAny<string>())).ReturnsAsync(housingEntities.FirstOrDefault().RecordKey);
        //        termRepositoryMock.Setup(i => i.GetAsync()).ReturnsAsync(termEntities);
        //        termRepositoryMock.Setup(i => i.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(academicPeriodEntities);
        //        referenceRepositoryMock.Setup(i => i.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roomCharacteristicEntities);
        //        studentReferenceDataRepositoryMock.Setup(i => i.GetRoommateCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roommateCharacteristicEntities);
        //        studentReferenceDataRepositoryMock.Setup(i => i.GetFloorCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(floorCharacteristicEntities);
        //        referenceRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locationEntities);
        //        referenceRepositoryMock.Setup(i => i.GetRoomWingsAsync(It.IsAny<bool>())).ReturnsAsync(roomWingEntities);
        //        referenceRepositoryMock.Setup(i => i.GetBuildingsAsync(It.IsAny<bool>())).ReturnsAsync(buildingEntities);
        //        roomRepositoryMock.Setup(i => i.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(roomEntities);
        //    }

        //    [TestMethod]
        //    public async Task HousingRequestService__UpdateHousingRequestByGuidAsync()
        //    {
        //        var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
        //        var housingEntity = housingEntities.FirstOrDefault(i => i.Guid.Equals(id));
        //        housingRepositoryMock.Setup(i => i.UpdateHousingRequestAsync(It.IsAny<Domain.Student.Entities.HousingRequest>())).ReturnsAsync(housingEntity);

        //        var expected = housingDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        //        Assert.IsNotNull(expected);

        //        var actual = await housingRequestService.UpdateHousingRequestAsync(id, expected);

        //        Assert.IsNotNull(actual);

        //        Assert.AreEqual(expected.Id, actual.Id);
        //    }

        //}
    }
}
