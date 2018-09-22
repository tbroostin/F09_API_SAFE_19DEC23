﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
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
    public class AdmissionApplicationsServiceTests
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
        public class AdmissionApplications : CurrentUserSetup
        {
            Mock<IAdmissionApplicationsRepository> admissionRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IReferenceDataRepository> referenceRepositoryMock;
            Mock<ITermRepository> termRepositoryMock;
            Mock<IInstitutionRepository> institutionRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<ICurrentUserFactory> userFactoryMock;
            ICurrentUserFactory userFactory;
            Mock<IRoleRepository> roleRepoMock;
            Mock<IConfigurationRepository> configRepoMock;
            //Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private ILogger logger;

            AdmissionApplicationsService admissionApplicationService;

            IEnumerable<Domain.Student.Entities.AdmissionApplication> admissionEntities;
            IEnumerable<Dtos.AdmissionApplication> admissionDtos;
            IEnumerable<Dtos.AdmissionApplication2> admissionDtosV11;
            IEnumerable<Dtos.AdmissionApplication> admissionDtos2;
            Dictionary<string, string> personGuids = new Dictionary<string, string>();
            IEnumerable<Domain.Student.Entities.AdmissionApplicationType> admissionTypeEntities;
            IEnumerable<Domain.Student.Entities.Term> termEntities;
            IEnumerable<Domain.Student.Entities.AdmissionDecisionType> admissionStatusTypeEntities;
            IEnumerable<Domain.Student.Entities.ApplicationSource> applicationSourceEntities;
            IEnumerable<Domain.Student.Entities.AdmissionPopulation> admissionPopulationEntities;
            IEnumerable<Domain.Student.Entities.AdmissionResidencyType> admissionResidencyEntities;
            IEnumerable<Domain.Student.Entities.AcademicLevel> academicLevelEntities;
            IEnumerable<Domain.Student.Entities.AcademicProgram> academicProgramEntities;
            IEnumerable<Domain.Student.Entities.WithdrawReason> withdrawReasonEntities;
            IEnumerable<Domain.Base.Entities.Location> locationEntities;
            IEnumerable<Domain.Base.Entities.AcademicDiscipline> academicDisciplineEntities;
            IEnumerable<Domain.Base.Entities.School> schoolEntities;
            IEnumerable<Domain.Student.Entities.AdmissionApplicationStatusType> admissionApplicationStatusTypes;

            Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplication>, int> admissionEntitiesTuple;
            Tuple<IEnumerable<Dtos.AdmissionApplication>, int> admissionDtoTuple;
            Tuple<IEnumerable<Dtos.AdmissionApplication2>, int> admissionDtoTuple2;

            int offset = 0;
            int limit = 200;

            private Domain.Entities.Permission permissionViewAnyApplication;


            [TestInitialize]
            public void Initialize()
            {
                admissionRepositoryMock = new Mock<IAdmissionApplicationsRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                institutionRepositoryMock = new Mock<IInstitutionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                userFactoryMock = new Mock<ICurrentUserFactory>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;
                configRepoMock = new Mock<IConfigurationRepository>();

                // Set up current user
                userFactory = userFactoryMock.Object;
                userFactory = new CurrentUserSetup.PersonUserFactory();
                // Mock permissions
                permissionViewAnyApplication = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewApplications);
                personRole.AddPermission(permissionViewAnyApplication);

                BuildData();
                BuildMocks();

                admissionApplicationService = new AdmissionApplicationsService(admissionRepositoryMock.Object, termRepositoryMock.Object, institutionRepositoryMock.Object, personRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, referenceRepositoryMock.Object, configRepoMock.Object, adapterRegistryMock.Object, userFactory, roleRepoMock.Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                admissionEntities = null;
                admissionDtos = null;
            }

            private void BuildData()
            {
                //Dtos
                admissionDtos = new List<Dtos.AdmissionApplication>()
                {
                    new Dtos.AdmissionApplication()
                    {
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                            new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                                AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"),
                                AdmissionApplicationsStatusStartOn = new DateTime(2016, 09, 03),
                                AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.DecisionMade
                            }
                        },
                        Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.AdmissionApplication()
                    {
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                            new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                                AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                                AdmissionApplicationsStatusStartOn = new DateTime(2016, 09, 01),
                                AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.EnrollmentCompleted
                            }
                        },
                        Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.AdmissionApplication()
                    {
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                            new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                                AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                                AdmissionApplicationsStatusStartOn = new DateTime(2016, 04, 01),
                                AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.ReadyForReview
                            }
                        },
                        Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.AdmissionApplication()
                    {
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                            new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                                AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"),
                                AdmissionApplicationsStatusStartOn = new DateTime(2016, 02, 01),
                                AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.ReadyForReview
                            }
                        },
                        Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };

                admissionDtos2 = new List<Dtos.AdmissionApplication>()
                {
                    new Dtos.AdmissionApplication()
                    {
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                            new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                                AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"),
                                AdmissionApplicationsStatusStartOn = new DateTime(2016, 09, 03),
                                AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.DecisionMade
                            }
                        },
                        Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.AdmissionApplication()
                    {
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                            new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                                AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                                AdmissionApplicationsStatusStartOn = new DateTime(2016, 09, 01),
                                AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.EnrollmentCompleted
                            }
                        },
                        Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.AdmissionApplication()
                    {
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                            new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                                AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                                AdmissionApplicationsStatusStartOn = new DateTime(2016, 04, 01),
                                AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.ReadyForReview
                            }
                        },
                        Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.AdmissionApplication()
                    {
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        Statuses = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>() {
                            new Dtos.DtoProperties.AdmissionApplicationsStatus() {
                                AdmissionApplicationsStatusDetail = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"),
                                AdmissionApplicationsStatusStartOn = new DateTime(2016, 02, 01),
                                AdmissionApplicationsStatusType = Dtos.EnumProperties.AdmissionApplicationsStatusType.ReadyForReview
                            }
                        },
                        Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };

                admissionDtosV11 = new List<Dtos.AdmissionApplication2>()
                {
                    new Dtos.AdmissionApplication2()
                    {
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        AcademicPeriod = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        AcademicLoad = Dtos.EnumProperties.AdmissionApplicationsAcademicLoadType.FullTime,
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                        ,AppliedOn = new DateTime(2016, 02, 01),
                        AdmittedOn = new DateTime(2016, 02, 01),
                        MatriculatedOn = new DateTime(2016, 02, 01)
                    },
                    new Dtos.AdmissionApplication2()
                    {
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                        ,AdmissionPopulation = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                        ,AppliedOn = new DateTime(2016, 02, 01),
                        AdmittedOn = new DateTime(2016, 02, 01)
                    },
                    new Dtos.AdmissionApplication2()
                    {
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512"),
                        Level = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52")
                        ,ResidencyType = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Program = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52")
                        ,AppliedOn = new DateTime(2016, 02, 01),
                        AdmittedOn = new DateTime(2016, 02, 01)
                    },
                    new Dtos.AdmissionApplication2()
                    {
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b"),
                        Site = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        School = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                        ,AppliedOn = new DateTime(2016, 02, 01),
                        AdmittedOn = new DateTime(2016, 02, 01)
                    },
                };

                //Entities
                admissionEntities = new List<Domain.Student.Entities.AdmissionApplication>()
                {
                    new Domain.Student.Entities.AdmissionApplication("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", "1")
                    {
                        ApplicantPersonId = "1",
                        AdmissionApplicationStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>() {
                            new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE1", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) },
                            new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE2", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) }
                        },
                        ApplicationStartTerm = "CODE1",
                        ApplicationStudentLoadIntent = "F"
                    },
                    new Domain.Student.Entities.AdmissionApplication("3f67b180-ce1d-4552-8d81-feb96b9fea5b", "1")
                    {
                        ApplicantPersonId = "1",
                        AdmissionApplicationStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>() {
                            new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE1", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) }
                        },
                        ApplicationAdmitStatus = "CODE1"
                    },
                    new Domain.Student.Entities.AdmissionApplication("bf67e156-8f5d-402b-8101-81b0a2796873", "3")
                    {
                        ApplicantPersonId = "2",
                        AdmissionApplicationStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>() {
                            new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE1", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) }
                        },
                        ApplicationResidencyStatus = "CODE1"
                        ,ApplicationAcadProgram = "CODE2"
                    },
                    new Domain.Student.Entities.AdmissionApplication("0111d6ef-5a86-465f-ac58-4265a997c136", "3")
                    {
                        ApplicantPersonId = "2",
                        AdmissionApplicationStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>() {
                            new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE1", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) }
                        },
                        ApplicationLocations = new List<string>() { "CODE1" },
                        ApplicationSchool = "CODE1"
                    }
                };

                //Persons
                personGuids.Add("1", "d190d4b5-03b5-41aa-99b8-b8286717c956");
                personGuids.Add("2", "cecdce5a-54a7-45fb-a975-5392a579e5bf");


                //AdmissionTypes
                admissionTypeEntities = new List<Domain.Student.Entities.AdmissionApplicationType>()
                {
                    new Domain.Student.Entities.AdmissionApplicationType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AdmissionApplicationType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AdmissionApplicationType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AdmissionApplicationType("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //Terms
                termEntities = new List<Domain.Student.Entities.Term>()
                {
                    new Domain.Student.Entities.Term("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", new DateTime(2016, 02, 01), new DateTime(2016, 03, 01), 2016, 1, false, false, "WINTER", false),
                    new Domain.Student.Entities.Term("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", new DateTime(2016, 03, 01), new DateTime(2016, 04, 01), 2016, 2, false, false, "WINTER", false),
                    new Domain.Student.Entities.Term("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", new DateTime(2016, 04, 01), new DateTime(2016, 05, 01), 2016, 3, false, false, "WINTER", false),
                    new Domain.Student.Entities.Term("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, false, false, "WINTER", false)
                };

                //AdmissionApplicationStatusTypes
                admissionStatusTypeEntities = new List<Domain.Student.Entities.AdmissionDecisionType>()
                {
                    new Domain.Student.Entities.AdmissionDecisionType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AdmissionDecisionType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AdmissionDecisionType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AdmissionDecisionType("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //ApplicationSources
                applicationSourceEntities = new List<Domain.Student.Entities.ApplicationSource>()
                {
                    new Domain.Student.Entities.ApplicationSource("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.ApplicationSource("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.ApplicationSource("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.ApplicationSource("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //AdmissionPopulations
                admissionPopulationEntities = new List<Domain.Student.Entities.AdmissionPopulation>()
                {
                    new Domain.Student.Entities.AdmissionPopulation("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AdmissionPopulation("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AdmissionPopulation("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AdmissionPopulation("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //AdmissionResidencyTypes
                admissionResidencyEntities = new List<Domain.Student.Entities.AdmissionResidencyType>()
                {
                    new Domain.Student.Entities.AdmissionResidencyType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AdmissionResidencyType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AdmissionResidencyType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AdmissionResidencyType("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //AcademicLevels
                academicLevelEntities = new List<Domain.Student.Entities.AcademicLevel>()
                {
                    new Domain.Student.Entities.AcademicLevel("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AcademicLevel("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AcademicLevel("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AcademicLevel("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //AcademicPrograms
                academicProgramEntities = new List<Domain.Student.Entities.AcademicProgram>()
                {
                    new Domain.Student.Entities.AcademicProgram("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1") {AcadLevelCode = "CODE1" },
                    new Domain.Student.Entities.AcademicProgram("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"){AcadLevelCode = "CODE2" },
                    new Domain.Student.Entities.AcademicProgram("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"){AcadLevelCode = "CODE3" },
                    new Domain.Student.Entities.AcademicProgram("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4"){AcadLevelCode = "CODE4" }
                };

                //WithdrawReasons
                withdrawReasonEntities = new List<Domain.Student.Entities.WithdrawReason>()
                {
                    new Domain.Student.Entities.WithdrawReason("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.WithdrawReason("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.WithdrawReason("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.WithdrawReason("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //Locations
                locationEntities = new List<Domain.Base.Entities.Location>()
                {
                    new Domain.Base.Entities.Location("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Base.Entities.Location("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Base.Entities.Location("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Base.Entities.Location("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //AcademicDisciplines
                academicDisciplineEntities = new List<Domain.Base.Entities.AcademicDiscipline>()
                {
                    new Domain.Base.Entities.AcademicDiscipline("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", Domain.Base.Entities.AcademicDisciplineType.Concentration),
                    new Domain.Base.Entities.AcademicDiscipline("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", Domain.Base.Entities.AcademicDisciplineType.Major),
                    new Domain.Base.Entities.AcademicDiscipline("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", Domain.Base.Entities.AcademicDisciplineType.Minor),
                    new Domain.Base.Entities.AcademicDiscipline("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", Domain.Base.Entities.AcademicDisciplineType.Concentration)
                };

                //Schools
                schoolEntities = new List<Domain.Base.Entities.School>()
                {
                    new Domain.Base.Entities.School("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Base.Entities.School("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Base.Entities.School("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Base.Entities.School("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //Application status types
                admissionApplicationStatusTypes = new List<Domain.Student.Entities.AdmissionApplicationStatusType>()
                {
                    new Domain.Student.Entities.AdmissionApplicationStatusType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1") {SpecialProcessingCode = "AC" },
                    new Domain.Student.Entities.AdmissionApplicationStatusType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"){SpecialProcessingCode = "MS" },
                    new Domain.Student.Entities.AdmissionApplicationStatusType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"){SpecialProcessingCode = "WI" },
                    new Domain.Student.Entities.AdmissionApplicationStatusType("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4"){SpecialProcessingCode = "" }
                };

                //Tuple
                admissionEntitiesTuple = new Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplication>, int>(admissionEntities, admissionEntities.Count());
                admissionDtoTuple = new Tuple<IEnumerable<Dtos.AdmissionApplication>, int>(admissionDtos, admissionDtos.Count());
                admissionDtoTuple2 = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionDtosV11, admissionDtosV11.Count());
            }

            private void BuildMocks()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                admissionRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuids);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionTypeEntities);
                termRepositoryMock.Setup(i => i.GetAsync(It.IsAny<bool>())).ReturnsAsync(termEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionDecisionTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionStatusTypeEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionDecisionTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionStatusTypeEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetApplicationSourcesAsync(It.IsAny<bool>())).ReturnsAsync(applicationSourceEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulationEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionResidencyEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevelEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(academicProgramEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetWithdrawReasonsAsync(It.IsAny<bool>())).ReturnsAsync(withdrawReasonEntities);
                referenceRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locationEntities);
                referenceRepositoryMock.Setup(i => i.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(academicDisciplineEntities);
                referenceRepositoryMock.Setup(i => i.GetSchoolsAsync(It.IsAny<bool>())).ReturnsAsync(schoolEntities);

                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionApplicationStatusTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionApplicationStatusTypes);
            }

            [TestMethod]
            public async Task AdmissionApplicationService__GetAdmissionApplicationsAsync()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(admissionEntitiesTuple);
                var admissionApplications = await admissionApplicationService.GetAdmissionApplicationsAsync(offset, limit, false);

                Assert.IsNotNull(admissionApplications);
                Assert.AreEqual(4, admissionApplications.Item2);

                foreach (var actual in admissionApplications.Item1)
                {
                    var expected = admissionDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant.Id, actual.Applicant.Id);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationService__GetAdmissionApplications2Async()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(admissionEntitiesTuple);
                var admissionApplications = await admissionApplicationService.GetAdmissionApplications2Async(offset, limit, false);

                Assert.IsNotNull(admissionApplications);
                Assert.AreEqual(4, admissionApplications.Item2);

                foreach (var actual in admissionApplications.Item1)
                {
                    var expected = admissionDtosV11.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant.Id, actual.Applicant.Id);

                    if(expected.AcademicPeriod != null || actual.AcademicPeriod != null)
                        Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                    if (expected.Type != null || actual.Type != null)
                        Assert.AreEqual(expected.Type.Id, actual.Type.Id);
                    Assert.AreEqual(expected.AppliedOn, actual.AppliedOn);
                    Assert.AreEqual(expected.AdmittedOn, actual.AdmittedOn);
                    Assert.AreEqual(expected.MatriculatedOn, actual.MatriculatedOn);
                    if (expected.Program != null || actual.Program != null)
                        Assert.AreEqual(expected.Program.Id, actual.Program.Id);
                    if (expected.AdmissionPopulation != null || actual.AdmissionPopulation != null)
                        Assert.AreEqual(expected.AdmissionPopulation.Id, actual.AdmissionPopulation.Id);
                    if (expected.Level != null || actual.Level != null)
                        Assert.AreEqual(expected.Level.Id, actual.Level.Id);
                    if (expected.ResidencyType != null || actual.ResidencyType != null)
                        Assert.AreEqual(expected.ResidencyType.Id, actual.ResidencyType.Id);
                    if (expected.Site != null || actual.Site != null)
                        Assert.AreEqual(expected.Site.Id, actual.Site.Id);
                    if (expected.School != null || actual.School != null)
                        Assert.AreEqual(expected.School.Id, actual.School.Id);

                }
            }

            [TestMethod]
            public async Task AdmissionApplicationService__GetAdmissionApplicationByGuidAsync()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync(id);

                Assert.IsNotNull(actual);

                var expected = admissionDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Applicant.Id, actual.Applicant.Id);
            }

            [TestMethod]
            public async Task AdmissionApplicationService__GetAdmissionApplicationByGuid2Async()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async(id);

                Assert.IsNotNull(actual);

                var expected = admissionDtos2.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Applicant.Id, actual.Applicant.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AdmissionApplications_GetAdmissionApplicationsAsync_Exception()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actual = await admissionApplicationService.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AdmissionApplications_GetAdmissionApplications2Async_Exception()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actual = await admissionApplicationService.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplications_GetAdmissionApplicationsAsync_ArgumentNullException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplications_GetAdmissionApplications2Async_ArgumentNullException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var actual = await admissionApplicationService.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplications_GetAdmissionApplicationsAsync_KeyNotFoundException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplications_GetAdmissionApplications2Async_KeyNotFoundException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await admissionApplicationService.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdmissionApplications_GetAdmissionApplicationsAsync_PermissionsException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdmissionApplications_GetAdmissionApplications2Async_PermissionsException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                var actual = await admissionApplicationService.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetAdmissionApplicationsAsync_InvalidOperationException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetAdmissionApplications2Async_InvalidOperationException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException());
                var actual = await admissionApplicationService.GetAdmissionApplications2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AdmissionApplications_GetById_Exception()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AdmissionApplications_GetById2_Exception()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplications_GetById_ArgumentNullException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplications_GetById2_ArgumentNullException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplications_GetById_KeyNotFoundException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplications_GetById2_KeyNotFoundException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdmissionApplications_GetById_PermissionsException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdmissionApplications_GetById2_PermissionsException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetById_InvalidOperationException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetById2_InvalidOperationException()
            {
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetById_ApplicantPersonId_Null_InvalidOperationException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                addmissionEntity.ApplicantPersonId = null;
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetById2_ApplicantPersonId_Null_InvalidOperationException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                addmissionEntity.ApplicantPersonId = null;
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetById_AdmissionApplicationStatuses_Null_InvalidOperationException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                addmissionEntity.AdmissionApplicationStatuses = null;
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetById2_AdmissionApplicationStatuses_Null_InvalidOperationException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                addmissionEntity.AdmissionApplicationStatuses = null;
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplications_GetById_Catch_ArgumentNullException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                admissionRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new ArgumentNullException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplications_GetById2_Catch_ArgumentNullException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                admissionRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new ArgumentNullException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetById_Catch_InvalidOperationException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                admissionRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new InvalidOperationException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplications_GetById2_Catch_InvalidOperationException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                admissionRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new InvalidOperationException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplications_GetById_Catch_KeyNotFoundException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                admissionRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplications_GetById2_Catch_KeyNotFoundException()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                admissionRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuid2Async("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AdmissionApplications_GetById_Catch_Exception()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var addmissionEntity = admissionEntities.FirstOrDefault(i => i.Guid.Equals(id));
                admissionRepositoryMock.Setup(i => i.GetAdmissionApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(addmissionEntity);

                admissionRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new Exception());
                var actual = await admissionApplicationService.GetAdmissionApplicationsByGuidAsync("abc");
            }
        }

        [TestClass]
        public class AdmissionApplicationsServiceTests_POST_PUT : StudentUserFactory
        {
            #region DECLARATIONS

            protected Domain.Entities.Role createOrUpdateAdmissionApplications = new Domain.Entities.Role(1, "UPDATE.APPLICATIONS");
            Mock<IAdmissionApplicationsRepository> admissionApplicationsRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IReferenceDataRepository> referenceRepositoryMock;
            Mock<ITermRepository> termRepositoryMock;
            Mock<IInstitutionRepository> institutionRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<IConfigurationRepository> configRepoMock;
            private Mock<ILogger> loggerMock;

            private AdmissionApplicationUser currentUserFactory;

            AdmissionApplicationsService admissionApplicationService;

            IEnumerable<Domain.Student.Entities.AdmissionApplication> admissionEntities;
            IEnumerable<Dtos.AdmissionApplication2> admissionDtosV11;
            Dictionary<string, string> personGuids = new Dictionary<string, string>();
            IEnumerable<Domain.Student.Entities.AdmissionApplicationType> admissionTypeEntities;
            IEnumerable<Domain.Student.Entities.Term> termEntities;
            IEnumerable<Domain.Student.Entities.AdmissionDecisionType> admissionStatusTypeEntities;
            IEnumerable<Domain.Student.Entities.ApplicationSource> applicationSourceEntities;
            IEnumerable<Domain.Student.Entities.AdmissionPopulation> admissionPopulationEntities;
            IEnumerable<Domain.Student.Entities.AdmissionResidencyType> admissionResidencyEntities;
            IEnumerable<Domain.Student.Entities.AcademicLevel> academicLevelEntities;
            IEnumerable<Domain.Student.Entities.AcademicProgram> academicProgramEntities;
            IEnumerable<Domain.Student.Entities.WithdrawReason> withdrawReasonEntities;
            IEnumerable<Domain.Base.Entities.Location> locationEntities;
            IEnumerable<Domain.Base.Entities.AcademicDiscipline> academicDisciplineEntities;
            IEnumerable<Domain.Base.Entities.School> schoolEntities;
            IEnumerable<Domain.Student.Entities.AdmissionApplicationStatusType> admissionApplicationStatusTypes;
            
            string guid = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                admissionApplicationsRepositoryMock = new Mock<IAdmissionApplicationsRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                institutionRepositoryMock = new Mock<IInstitutionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                configRepoMock = new Mock<IConfigurationRepository>();
                loggerMock = new Mock<ILogger>();
                currentUserFactory = new AdmissionApplicationUser();


                InitializeTestData();
                InitializeTestMock();

                admissionApplicationService = new AdmissionApplicationsService(admissionApplicationsRepositoryMock.Object, termRepositoryMock.Object, institutionRepositoryMock.Object, personRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, referenceRepositoryMock.Object, configRepoMock.Object, adapterRegistryMock.Object,currentUserFactory,roleRepositoryMock.Object,loggerMock.Object);
            }

            private void InitializeTestData() {
                admissionDtosV11 = new List<Dtos.AdmissionApplication2>()
                {
                    new Dtos.AdmissionApplication2()
                    {
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        AcademicPeriod = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        AcademicLoad = Dtos.EnumProperties.AdmissionApplicationsAcademicLoadType.FullTime,
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        AppliedOn = new DateTime(2016, 02, 01),
                        AdmittedOn = new DateTime(2016, 02, 01),
                        MatriculatedOn = new DateTime(2016, 02, 01),
                        Source = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        AdmissionPopulation = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Site = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        ResidencyType = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Program = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Level = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        School = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Withdrawal = new Dtos.DtoProperties.AdmissionApplicationsWithdrawal2(){ WithdrawnOn = DateTime.Now, WithdrawalReason = new GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), InstitutionAttended = new Dtos.DtoProperties.AdmissionApplicationInstitutionAttendedDtoProperty(){ Id = "b90812ee-b573-4acb-88b0-6999a050be4f" } },
                        Comment = "Comment_001",
                        ReferenceID = "Ref_001"

                    }
            };

                admissionEntities = new List<Domain.Student.Entities.AdmissionApplication>()
                {
                    new Domain.Student.Entities.AdmissionApplication("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", "1")
                    {
                        ApplicantPersonId = "1",
                        AdmissionApplicationStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>() {
                            new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE1", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) },
                            new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE2", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) },
                            new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE3", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) },
                            new Domain.Student.Entities.AdmissionApplicationStatus() { ApplicationStatusDate = new DateTime(2016, 02, 01), ApplicationDecisionBy = "1", ApplicationStatus = "CODE4", ApplicationStatusTime = new DateTime(1900, 01, 01, 21, 36, 56, 000) }
                        },
                        ApplicationStartTerm = "CODE1",
                        ApplicationStudentLoadIntent = "F", ApplicationAdmissionsRep = "Rep_001", ApplicationSource="CODE1",
                        ApplicationResidencyStatus = "CODE1",
                        ApplicationAcadProgram = "CODE2", ApplicationAdmitStatus = "CODE1", ApplicationWithdrawReason="CODE1",ApplicationWithdrawDate=DateTime.Now, ApplicationAttendedInstead="1",ApplicationIntgType="CODE1"
                    }
                };

                admissionStatusTypeEntities = new List<Domain.Student.Entities.AdmissionDecisionType>()
                {
                    new Domain.Student.Entities.AdmissionDecisionType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AdmissionDecisionType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AdmissionDecisionType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AdmissionDecisionType("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                termEntities = new List<Domain.Student.Entities.Term>()
                {
                    new Domain.Student.Entities.Term("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", new DateTime(2016, 02, 01), new DateTime(2016, 03, 01), 2016, 1, false, false, "WINTER", false),
                    new Domain.Student.Entities.Term("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", new DateTime(2016, 03, 01), new DateTime(2016, 04, 01), 2016, 2, false, false, "WINTER", false),
                    new Domain.Student.Entities.Term("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", new DateTime(2016, 04, 01), new DateTime(2016, 05, 01), 2016, 3, false, false, "WINTER", false),
                    new Domain.Student.Entities.Term("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, false, false, "WINTER", false)
                };

                applicationSourceEntities = new List<Domain.Student.Entities.ApplicationSource>()
                {
                    new Domain.Student.Entities.ApplicationSource("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.ApplicationSource("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.ApplicationSource("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.ApplicationSource("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                admissionPopulationEntities = new List<Domain.Student.Entities.AdmissionPopulation>()
                {
                    new Domain.Student.Entities.AdmissionPopulation("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AdmissionPopulation("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AdmissionPopulation("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AdmissionPopulation("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                locationEntities = new List<Domain.Base.Entities.Location>()
                {
                    new Domain.Base.Entities.Location("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Base.Entities.Location("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Base.Entities.Location("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Base.Entities.Location("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                admissionResidencyEntities = new List<Domain.Student.Entities.AdmissionResidencyType>()
                {
                    new Domain.Student.Entities.AdmissionResidencyType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AdmissionResidencyType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AdmissionResidencyType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AdmissionResidencyType("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                academicProgramEntities = new List<Domain.Student.Entities.AcademicProgram>()
                {
                    new Domain.Student.Entities.AcademicProgram("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1") {AcadLevelCode = "CODE1" },
                    new Domain.Student.Entities.AcademicProgram("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"){AcadLevelCode = "CODE2" },
                    new Domain.Student.Entities.AcademicProgram("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"){AcadLevelCode = "CODE3" },
                    new Domain.Student.Entities.AcademicProgram("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4"){AcadLevelCode = "CODE4" }
                };

                academicDisciplineEntities = new List<Domain.Base.Entities.AcademicDiscipline>()
                {
                    new Domain.Base.Entities.AcademicDiscipline("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", Domain.Base.Entities.AcademicDisciplineType.Concentration),
                    new Domain.Base.Entities.AcademicDiscipline("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", Domain.Base.Entities.AcademicDisciplineType.Major),
                    new Domain.Base.Entities.AcademicDiscipline("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", Domain.Base.Entities.AcademicDisciplineType.Minor),
                    new Domain.Base.Entities.AcademicDiscipline("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", Domain.Base.Entities.AcademicDisciplineType.Concentration)
                };

                withdrawReasonEntities = new List<Domain.Student.Entities.WithdrawReason>()
                {
                    new Domain.Student.Entities.WithdrawReason("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.WithdrawReason("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.WithdrawReason("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.WithdrawReason("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                admissionTypeEntities = new List<Domain.Student.Entities.AdmissionApplicationType>()
                {
                    new Domain.Student.Entities.AdmissionApplicationType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AdmissionApplicationType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AdmissionApplicationType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AdmissionApplicationType("2158ad73-3416-467b-99d5-1b7b92599389", "ST", "DESC4")
                };

                academicLevelEntities = new List<Domain.Student.Entities.AcademicLevel>()
                {
                    new Domain.Student.Entities.AcademicLevel("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AcademicLevel("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AcademicLevel("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Student.Entities.AcademicLevel("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                schoolEntities = new List<Domain.Base.Entities.School>()
                {
                    new Domain.Base.Entities.School("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new Domain.Base.Entities.School("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new Domain.Base.Entities.School("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new Domain.Base.Entities.School("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                personGuids.Add("1", "d190d4b5-03b5-41aa-99b8-b8286717c956");
                personGuids.Add("2", "cecdce5a-54a7-45fb-a975-5392a579e5bf");

                admissionApplicationStatusTypes = new List<Domain.Student.Entities.AdmissionApplicationStatusType>()
                {
                    new Domain.Student.Entities.AdmissionApplicationStatusType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1") {SpecialProcessingCode = "AC" },
                    new Domain.Student.Entities.AdmissionApplicationStatusType("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"){SpecialProcessingCode = "MS" },
                    new Domain.Student.Entities.AdmissionApplicationStatusType("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"){SpecialProcessingCode = "WI" },
                    new Domain.Student.Entities.AdmissionApplicationStatusType("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4"){SpecialProcessingCode = "" }
                };

            }

            private void InitializeTestMock() {

                createOrUpdateAdmissionApplications.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.UpdateApplications));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createOrUpdateAdmissionApplications });
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { createOrUpdateAdmissionApplications });
                
                personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionDecisionTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionStatusTypeEntities);
                termRepositoryMock.Setup(i => i.GetAsync(It.IsAny<bool>())).ReturnsAsync(termEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetApplicationSourcesAsync(It.IsAny<bool>())).ReturnsAsync(applicationSourceEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulationEntities);
                referenceRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locationEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionResidencyEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(academicProgramEntities);
                referenceRepositoryMock.Setup(i => i.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(academicDisciplineEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetWithdrawReasonsAsync(It.IsAny<bool>())).ReturnsAsync(withdrawReasonEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionTypeEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevelEntities);
                referenceRepositoryMock.Setup(i => i.GetSchoolsAsync(It.IsAny<bool>())).ReturnsAsync(schoolEntities);
                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());
                admissionApplicationsRepositoryMock.Setup(x => x.GetStaffOperIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuids);
                admissionApplicationsRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuids);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionApplicationStatusTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionApplicationStatusTypes);
                personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("d190d4b5-03b5-41aa-99b8-b8286717c956");
                admissionApplicationsRepositoryMock.Setup(x => x.GetRecordKeyAsync(It.IsAny<string>())).ReturnsAsync("1");
                admissionApplicationsRepositoryMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

            }
            
            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationsRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                admissionEntities = null;
                studentReferenceDataRepositoryMock = null;
                termRepositoryMock = null;
                referenceRepositoryMock = null;
                institutionRepositoryMock = null;
                personRepositoryMock = null;
                roleRepositoryMock = null;
                adapterRegistryMock = null;
                configRepoMock = null;
                loggerMock = null;
                currentUserFactory = null;
            }
            #endregion

            #region POST

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_Dtos_Null() {

                await admissionApplicationService.CreateAdmissionApplicationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().Id = null;
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_PermissionsException()
            {
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() {  });
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_ApplicantKey_Null()
            {
                personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_ApplicantionType_Null()
            {
                admissionDtosV11.FirstOrDefault().Type.Id = Guid.Empty.ToString();
                
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_AcademicPeriod_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().AcademicPeriod.Id = Guid.Empty.ToString();

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }
            
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_Owner_Id_Null()
            {
                AdmissionApplication2 admissionApplication = new Dtos.AdmissionApplication2()
                {
                    Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                    Owner = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                    AcademicPeriod = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                    AcademicLoad = Dtos.EnumProperties.AdmissionApplicationsAcademicLoadType.FullTime,
                    Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                    AppliedOn = new DateTime(2016, 02, 01),
                    AdmittedOn = new DateTime(2016, 02, 01),
                    MatriculatedOn = new DateTime(2016, 02, 01),
                    Source = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),   
                };

                personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_AdmissionPopulation_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().AdmissionPopulation.Id = Guid.Empty.ToString();

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_ResidencyType_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().ResidencyType.Id = Guid.Empty.ToString();

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_Program_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().Program.Id = Guid.Empty.ToString();

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_Level_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().Level.Id = Guid.Empty.ToString();

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_School_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().School.Id = Guid.Empty.ToString();

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_Source_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().Source.Id = Guid.Empty.ToString();

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_Site_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().Site.Id = Guid.Empty.ToString();

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_WithdrawalReason_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().Withdrawal.WithdrawalReason.Id = Guid.Empty.ToString();

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_InstitutionAttended_Id_Null()
            {

                admissionDtosV11.FirstOrDefault().Applicant = null;
                admissionDtosV11.FirstOrDefault().Owner = null;

                personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_ApplicantPersonId_Null()
            {
                admissionEntities.FirstOrDefault().ApplicantPersonId = null;
                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_ApplicantPersonKey_Null()
            {
                personGuids = new Dictionary<string, string>() { { "4", "1110d4b5-03b5-41aa-99b8-b8286717c956" }, { "5", "222dce5a-54a7-45fb-a975-5392a579e5bf" } };
                
                admissionApplicationsRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuids);
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_ArgumentNullException()
            {
                admissionApplicationsRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new ArgumentNullException());
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_InvalidOperationException()
            {
                admissionDtosV11.FirstOrDefault().Type = null;
            
                admissionApplicationsRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new InvalidOperationException());
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(NullReferenceException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_ApplicationType_Null()
            {
                admissionDtosV11.FirstOrDefault().Type = null;
                admissionTypeEntities = new List<Domain.Student.Entities.AdmissionApplicationType>()
                {
                    new Domain.Student.Entities.AdmissionApplicationType("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1")
                };
                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionTypeEntities);
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_KeyNotFoundException()
            {
                admissionApplicationsRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new KeyNotFoundException());
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync_Exception()
            {
                admissionApplicationsRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new Exception());
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_School_Null()
            {
                admissionEntities.FirstOrDefault().ApplicationSchool = "1";

                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_Term_Null()
            {
                admissionEntities.FirstOrDefault().ApplicationStartTerm = "1";

                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_Source_Null()
            {
                admissionEntities.FirstOrDefault().ApplicationSource = "1";

                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_Status_Null()
            {
                admissionEntities.FirstOrDefault().ApplicationAdmitStatus = "1";

                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_Location_Null()
            {
                admissionEntities.FirstOrDefault().ApplicationLocations = new List<string>() { "1" };

                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_ResidencyStatus_Null()
            {
                admissionEntities.FirstOrDefault().ApplicationResidencyStatus = "1";
                admissionEntities.FirstOrDefault().ApplicationStudentLoadIntent = "1";

                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_Program_Null()
            {
                admissionEntities.FirstOrDefault().ApplicationAcadProgram = "1";
                
                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_Level_Null()
            {
                academicLevelEntities = new List<Domain.Student.Entities.AcademicLevel>()
                {
                    new Domain.Student.Entities.AcademicLevel("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE11", "DESC1"),
                    new Domain.Student.Entities.AcademicLevel("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE22", "DESC2"),
                    new Domain.Student.Entities.AcademicLevel("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE33", "DESC3"),
                    new Domain.Student.Entities.AcademicLevel("2158ad73-3416-467b-99d5-1b7b92599389", "CODE44", "DESC4")
                };

                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevelEntities);
                
                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_WithDrawal_Null()
            {
                admissionEntities.FirstOrDefault().ApplicationWithdrawReason = "1";
                admissionEntities.FirstOrDefault().ApplicationStudentLoadIntent = "P";

                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_Attended_Null()
            {
                admissionDtosV11.FirstOrDefault().Applicant = null;
                admissionDtosV11.FirstOrDefault().Withdrawal = null;
                admissionDtosV11.FirstOrDefault().Owner = null;
                admissionEntities.FirstOrDefault().ApplicationStudentLoadIntent = "L";
                admissionEntities.FirstOrDefault().ApplicationAttendedInstead = "1";
                admissionEntities.FirstOrDefault().ApplicationWithdrawDate = null;

                personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);
                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdmissionApplicationService_ConvertAdmissionApplicationsEntityToDto2_Type_Null()
            {
                admissionEntities.FirstOrDefault().ApplicationIntgType = "1";

                admissionApplicationsRepositoryMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(admissionEntities.FirstOrDefault());

                await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());
            }



            [TestMethod]
            public async Task AdmissionApplicationService_CreateAdmissionApplicationAsync()
            {
                var result = await admissionApplicationService.CreateAdmissionApplicationAsync(admissionDtosV11.FirstOrDefault());

                Assert.IsNotNull(result);

                Assert.AreEqual(result.Id, admissionDtosV11.FirstOrDefault().Id);
            }

            #endregion

            #region PUT

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplicationService_UpdateAdmissionApplicationAsync_Dtos_Null()
            {
                await admissionApplicationService.UpdateAdmissionApplicationAsync(guid,null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplicationService_UpdateAdmissionApplicationAsync_Id_Null()
            {
                admissionDtosV11.FirstOrDefault().Id = null;
                await admissionApplicationService.UpdateAdmissionApplicationAsync(guid, admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdmissionApplicationService_UpdateAdmissionApplicationAsync_PermissionsException()
            {
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });
                await admissionApplicationService.UpdateAdmissionApplicationAsync(guid,admissionDtosV11.FirstOrDefault());
            }

            [TestMethod]
            public async Task AdmissionApplicationService_UpdateAdmissionApplicationAsync()
            {
                admissionApplicationsRepositoryMock.Setup(x => x.GetRecordKeyAsync(It.IsAny<string>())).ReturnsAsync(Guid.Empty.ToString());
                var result = await admissionApplicationService.UpdateAdmissionApplicationAsync(guid,admissionDtosV11.FirstOrDefault());

                Assert.IsNotNull(result);

                Assert.AreEqual(result.Id, admissionDtosV11.FirstOrDefault().Id);
            }

            [TestMethod]
            public async Task AdmissionApplicationService_UpdateAdmissionApplicationAsync_Create()
            {
                admissionApplicationsRepositoryMock.Setup(x => x.GetRecordKeyAsync(It.IsAny<string>())).ReturnsAsync(null);

                var result = await admissionApplicationService.UpdateAdmissionApplicationAsync(guid, admissionDtosV11.FirstOrDefault());

                Assert.IsNotNull(result);

                Assert.AreEqual(result.Id, admissionDtosV11.FirstOrDefault().Id);
            }


            #endregion


        }
    }
}
