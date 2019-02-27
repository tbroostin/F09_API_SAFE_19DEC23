// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentAcademicPeriodProfilesServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewStudentAcadPeriodProfileRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.ACADEMIC.PERIOD.PROFILE");
            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "STU1",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            // Represents a third party system like ILP
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ILP",
                            PersonId = "ILP",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ILP",
                            Roles = new List<string>() { "VIEW.STUDENT.ACADEMIC.PERIOD.PROFILE" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        #region GetTests
        [TestClass]
        public class StudentAcademicPeriodProfilesServiceTests_Get : CurrentUserSetup
        {
            private Mock<IStudentTermRepository> studentTermRepositoryMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICatalogRepository> catalogRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private Mock<IAcademicCreditRepository> academicCreditRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private ICurrentUserFactory currentUserFactory;
            private StudentAcademicPeriodProfilesService studentAcademicPeriodProfilesService;

            

            //StudentProgram response;
            Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles>, int> stuAcadPeriodProfilsDtoTuple;
            Tuple<IEnumerable<StudentTerm>, int> stuTermsTuple;

            IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles> stuAcadPeriodProfileDtos;

            Domain.Student.Entities.Student student;
            IEnumerable<StudentTerm> studentTermEntities;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentType> studentTypesEntities;
            IEnumerable<Domain.Student.Entities.AcademicProgram> acadProgramEntities;
            IEnumerable<Domain.Student.Entities.StudentProgram> studentProgramEntities;
            IEnumerable<Domain.Student.Entities.StudentTermStatus> studentTermStatuseEntities;
            IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriodEntities;
            IEnumerable<Domain.Student.Entities.StudentStatus> studentStatuses;
            IEnumerable<Domain.Student.Entities.AcademicCredit> acadCredits;
            IEnumerable<Domain.Student.Entities.AcademicCreditMinimum> acadCredits2;
            IEnumerable<Domain.Student.Entities.AcademicLevel> acadLevelEntities;
            IEnumerable<Domain.Student.Entities.StudentClassification> studentClassificationEntities;
            IEnumerable<Domain.Student.Entities.StudentLoad> allStudentLoadEntities;
            IEnumerable<Domain.Student.Entities.StudentProgramStatus> studentProgramStatusEntities;
            IEnumerable<Domain.Student.Entities.EnrollmentStatus> enrollmentStatusEntities;
            IEnumerable<Domain.Student.Entities.ResidencyStatus> residencyStatusEntities;

            IEnumerable<Domain.Student.Entities.Term> termEntities;

            string personGuid = "ed809943-eb26-42d0-9a95-d8db912a581f";
            string acadPeriod = "b9691210-8516-45ca-9cd1-7e5aa1777234";

            int offSet = 0;
            int limit = 2;


            [TestInitialize]
            public void Initialize()
            {
                studentTermRepositoryMock = new Mock<IStudentTermRepository>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                catalogRepositoryMock = new Mock<ICatalogRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                academicCreditRepositoryMock = new Mock<IAcademicCreditRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                BuildData();
                BuildMocks();
                studentAcademicPeriodProfilesService = new StudentAcademicPeriodProfilesService(adapterRegistryMock.Object, studentRepositoryMock.Object, studentProgramRepositoryMock.Object,
                                                         studentTermRepositoryMock.Object, academicCreditRepositoryMock.Object, termRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                         catalogRepositoryMock.Object, personRepositoryMock.Object, referenceDataRepositoryMock.Object, currentUserFactory, roleRepositoryMock.Object, 
                                                         configurationRepositoryMock.Object,  loggerMock.Object);

            }

            private void BuildData()
            {
                #region dto
                stuAcadPeriodProfileDtos = new List<StudentAcademicPeriodProfiles>() 
                {
                    new StudentAcademicPeriodProfiles(){ 
                        Id = "af4d47eb-f06b-4add-b5bf-d9529742387a", 
                        AcademicLoad = Dtos.EnumProperties.AcademicLoad.FullTime, 
                        AcademicPeriod = new Dtos.GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                        AcademicPeriodEnrollmentStatus = new Dtos.GuidObject2("2e5ff61-2f5b-4d4f-8396-b671184bdbd8"),
                        Measures = new List<Dtos.DtoProperties.PerformanceMeasureDtoProperty>()
                        {
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Level = new Dtos.GuidObject2("d118f007-c914-465e-80dc-49d39209b24f"), PerformanceMeasure = "3" }
                        },
                        Person = new Dtos.GuidObject2("ed809943-eb26-42d0-9a95-d8db912a581f"),
                        Residency = new Dtos.GuidObject2("dc5331ff-eea2-4294-8d3c-3e48876fbf09"),
                        StudentStatus = new Dtos.GuidObject2("82f74c63-df5b-4e56-8ef0-e871ccc789e8"),
                        Tags = new List<Dtos.GuidObject2>()
                        {
                            new Dtos.GuidObject2("c63c8b8b-484d-4c98-bee5-b83a7f4911f3"),
                            new Dtos.GuidObject2("59a99a2f-3402-4eb8-b96e-06d145237aa4")
                        },
                        Type = new Dtos.GuidObject2("82f74c63-df5b-4e56-8ef0-e871ccc789e8")
                    },
                    new StudentAcademicPeriodProfiles(){ 
                        Id = "bf4d47eb-f06b-4add-b5bf-d9529742387a", 
                        AcademicLoad = Dtos.EnumProperties.AcademicLoad.OverLoad, 
                        AcademicPeriod = new Dtos.GuidObject2("7f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        AcademicPeriodEnrollmentStatus = new Dtos.GuidObject2("62e5ff61-2f5b-4d4f-8396-b671184bdbd8"),
                        Measures = new List<Dtos.DtoProperties.PerformanceMeasureDtoProperty>()
                        {
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Level = new Dtos.GuidObject2("d118f007-c914-465e-80dc-49d39209b24f"), PerformanceMeasure = "3" }
                        },
                        Person = new Dtos.GuidObject2("6f11fcd7-40bf-4c24-8e97-602c363eb8cf"),
                        Residency = new Dtos.GuidObject2("378af5dc-85fa-4588-8987-9e6c90ffb8fe"),
                        StudentStatus = new Dtos.GuidObject2("82f74c63-df5b-4e56-8ef0-e871ccc789e8"),
                        Tags = new List<Dtos.GuidObject2>()
                        {
                            new Dtos.GuidObject2("1a6de707-cfe8-4ba3-b41d-2ff9e6800fe4"),
                            new Dtos.GuidObject2("beb84bb8-82f5-4b62-9de0-8a294558ecf3")
                        },
                        Type = new Dtos.GuidObject2("82f74c63-df5b-4e56-8ef0-e871ccc789e8")
                    },

                    new StudentAcademicPeriodProfiles(){ 
                        Id = "cf4d47eb-f06b-4add-b5bf-d9529742387a", 
                        AcademicLoad = Dtos.EnumProperties.AcademicLoad.PartTime, 
                        AcademicPeriod = new Dtos.GuidObject2("8f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        AcademicPeriodEnrollmentStatus = new Dtos.GuidObject2("891ab399-259f-4597-aca7-57b1a7f31626"),
                        Measures = new List<Dtos.DtoProperties.PerformanceMeasureDtoProperty>()
                        {
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Level = new Dtos.GuidObject2("d118f007-c914-465e-80dc-49d39209b24f"), PerformanceMeasure = "3" }
                        },
                        Person = new Dtos.GuidObject2("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"),
                        Residency = new Dtos.GuidObject2("478af5dc-85fa-4588-8987-9e6c90ffb8fe"),
                        StudentStatus = new Dtos.GuidObject2("82f74c63-df5b-4e56-8ef0-e871ccc789e8"),
                        Tags = new List<Dtos.GuidObject2>()
                        {
                            new Dtos.GuidObject2("2a6de707-cfe8-4ba3-b41d-2ff9e6800fe4"),
                            new Dtos.GuidObject2("ceb84bb8-82f5-4b62-9de0-8a294558ecf3")
                        },
                        Type = new Dtos.GuidObject2("82f74c63-df5b-4e56-8ef0-e871ccc789e8")
                    },


                    new StudentAcademicPeriodProfiles(){ 
                        Id = "df4d47eb-f06b-4add-b5bf-d9529742387a", 
                        AcademicLoad = Dtos.EnumProperties.AcademicLoad.FullTime, 
                        AcademicPeriod = new Dtos.GuidObject2("8f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        AcademicPeriodEnrollmentStatus = new Dtos.GuidObject2("82e5ff61-2f5b-4d4f-8396-b671184bdbd8"),
                        Measures = new List<Dtos.DtoProperties.PerformanceMeasureDtoProperty>()
                        {
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Level = new Dtos.GuidObject2("d118f007-c914-465e-80dc-49d39209b24f"), PerformanceMeasure = "3" }
                        },
                        Person = new Dtos.GuidObject2("2f11fcd7-40bf-4c24-8e97-602c363eb8cf"),
                        Residency = new Dtos.GuidObject2("578af5dc-85fa-4588-8987-9e6c90ffb8fe"),
                        StudentStatus = new Dtos.GuidObject2("82f74c63-df5b-4e56-8ef0-e871ccc789e8"),
                        Tags = new List<Dtos.GuidObject2>()
                        {
                            new Dtos.GuidObject2("3a6de707-cfe8-4ba3-b41d-2ff9e6800fe4"),
                            new Dtos.GuidObject2("deb84bb8-82f5-4b62-9de0-8a294558ecf3")
                        },
                        Type = new Dtos.GuidObject2("82f74c63-df5b-4e56-8ef0-e871ccc789e8")
                    }                    
                };
                #endregion
                stuAcadPeriodProfilsDtoTuple = new Tuple<IEnumerable<StudentAcademicPeriodProfiles>, int>(stuAcadPeriodProfileDtos, stuAcadPeriodProfileDtos.Count());

                termEntities = new List<Term>() 
               {
                   new Term("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", new DateTime(2016, 01,01), new DateTime(2016, 05,01), 2016, 1, false, false, "Spring", false),
                   new Term("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2016/Fall", new DateTime(2016, 09,01), new DateTime(2016, 10,15), 2016, 2, false, false, "Fall", false),
                   new Term("8f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Spr", new DateTime(2017, 01,01), new DateTime(2017, 05,01), 2017, 3, false, false, "Spring", false),
                   new Term("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", new DateTime(2017, 09,01), new DateTime(2017, 12,31), 2017, 4, false, false, "Fall", false)
               };
                academicPeriodEntities = new List<Domain.Student.Entities.AcademicPeriod>() 
                {
                    new Domain.Student.Entities.AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01,01), new DateTime(2016, 05,01), 2016, 1, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2016/Fall", "2016 Fall", new DateTime(2016, 09,01), new DateTime(2016, 10,15), 2016, 2, "Fall", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("8f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Spr", "2017 Spring", new DateTime(2017, 01,01), new DateTime(2017, 05,01), 2017, 3, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", "2017 Fall", new DateTime(2017, 09,01), new DateTime(2017, 12,31), 2017, 4, "Fall", "", "", null)
                };
                acadLevelEntities = new TestStudentReferenceDataRepository().GetAcademicLevelsAsync().Result.ToList();
                studentStatuses = new List<Domain.Student.Entities.StudentStatus>()
                {
                    new Domain.Student.Entities.StudentStatus("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "Code1", "title1"),
                    new Domain.Student.Entities.StudentStatus("bd54668d-50d9-416c-81e9-2318e88571a1", "Code2", "title2"),
                    new Domain.Student.Entities.StudentStatus("5eed2bea-8948-439b-b5c5-779d84724a38", "Code3", "title3"),
                    new Domain.Student.Entities.StudentStatus("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "Code4", "title4")
                };

                residencyStatusEntities = new List<Domain.Student.Entities.ResidencyStatus>()
                {
                    new Domain.Student.Entities.ResidencyStatus("b28138bd-d0ba-4c83-bab9-ba321835ece1", "INST", "In State"),
                    new Domain.Student.Entities.ResidencyStatus("fe75893d-2c35-41b0-8e78-3c8c35b7122d", "OUTST", "Out of State")
                };

                studentTermStatuseEntities = new List<Domain.Student.Entities.StudentTermStatus>() 
                {
                   new Domain.Student.Entities.StudentTermStatus("Code1", new DateTime(2015, 12, 1)),
                   new Domain.Student.Entities.StudentTermStatus("Code2", new DateTime(2016, 01, 1)),
                   new Domain.Student.Entities.StudentTermStatus("Code3", new DateTime(2016, 05, 1)),
                   new Domain.Student.Entities.StudentTermStatus("Code4", new DateTime(2016, 12, 1)),

                };

                studentTypesEntities = new TestStudentReferenceDataRepository().GetStudentTypesAsync().Result;
                string[] studentTypes = studentTypesEntities.Select(i => i.Code).ToArray();

                acadCredits = new List<Domain.Student.Entities.AcademicCredit>() 
                {
                    new Domain.Student.Entities.AcademicCredit("1"){ GradePoints = 3.5m, GpaCredit = 1.0m},
                    new Domain.Student.Entities.AcademicCredit( "2"){ GradePoints = 2.5m, GpaCredit = 1.0m},
                    new Domain.Student.Entities.AcademicCredit("3"){ GradePoints = 4.0m, GpaCredit = 1.0m},
                    new Domain.Student.Entities.AcademicCredit("4"){ GradePoints = 1.5m, GpaCredit = 1.0m},
                    new Domain.Student.Entities.AcademicCredit("5"){ GradePoints = 2.5m, GpaCredit = null},
                    new Domain.Student.Entities.AcademicCredit("6"){ GradePoints = 3.5m, GpaCredit = 2.0m},
                };

                acadCredits2 = new List<Domain.Student.Entities.AcademicCreditMinimum>()
                {
                    new Domain.Student.Entities.AcademicCreditMinimum("1"){ GradePoints = 3.5m, GpaCredit = 1.0m},
                    new Domain.Student.Entities.AcademicCreditMinimum( "2"){ GradePoints = 2.5m, GpaCredit = 1.0m},
                    new Domain.Student.Entities.AcademicCreditMinimum("3"){ GradePoints = 4.0m, GpaCredit = 1.0m},
                    new Domain.Student.Entities.AcademicCreditMinimum("4"){ GradePoints = 1.5m, GpaCredit = 1.0m},
                    new Domain.Student.Entities.AcademicCreditMinimum("5"){ GradePoints = 2.5m, GpaCredit = null},
                    new Domain.Student.Entities.AcademicCreditMinimum("6"){ GradePoints = 3.5m, GpaCredit = 2.0m},
                };

                studentClassificationEntities = new List<Ellucian.Colleague.Domain.Student.Entities.StudentClassification>() 
                {
                    new Ellucian.Colleague.Domain.Student.Entities.StudentClassification("3b8f02a3-d349-46b5-a0df-710121fa1f64", "1G", "First Year Graduate"),
                    new Ellucian.Colleague.Domain.Student.Entities.StudentClassification("7b8c4ba7-ea28-4604-bca7-da7223f6e2b3", "1L", "First Year Law"),
                    new Ellucian.Colleague.Domain.Student.Entities.StudentClassification("bd98c3ed-6adb-4c7c-bc80-7507ea868a23", "2A", "Second Year"),
                    new Ellucian.Colleague.Domain.Student.Entities.StudentClassification("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31", "2G", "Second Year Graduate"),
                    new Ellucian.Colleague.Domain.Student.Entities.StudentClassification("7e990bda-9427-4de6-b0ef-bba9b015e399", "2L", "Second Year Law"),
                };
                allStudentLoadEntities = new List<Domain.Student.Entities.StudentLoad>()
                {
                    new Domain.Student.Entities.StudentLoad("F", "Full Time"){Sp1 = "1"},
                    new Domain.Student.Entities.StudentLoad("P", "Part Time"){Sp1 = "2"},
                    new Domain.Student.Entities.StudentLoad("O", "Overload"){Sp1 = "3" },
                    new Domain.Student.Entities.StudentLoad("H", "Half Time"){Sp1 = "0"},
                    new Domain.Student.Entities.StudentLoad("L", "Less than Half Time"){Sp1 = "0"}
                };

                studentTermEntities = new List<StudentTerm>()  
                {
                    new StudentTerm("af4d47eb-f06b-4add-b5bf-d9529742387a", "1", "2016/Spr", "UG"){ StudentLoad = "F", StudentTermStatuses = studentTermStatuseEntities.ToList(), StudentAcademicCredentials = new List<string>(){"1", "2"}},
                    new StudentTerm("bf4d47eb-f06b-4add-b5bf-d9529742387a", "2", "2016/Fall", "UG"){StudentLoad = "O", StudentTermStatuses = studentTermStatuseEntities.ToList(), StudentAcademicCredentials = new List<string>(){"1", "2"}},
                    new StudentTerm("cf4d47eb-f06b-4add-b5bf-d9529742387a", "3", "2017/Spr", "UG"){ StudentLoad = "P", StudentTermStatuses = studentTermStatuseEntities.ToList(), StudentAcademicCredentials = new List<string>(){"1", "2"}},
                    new StudentTerm("df4d47eb-f06b-4add-b5bf-d9529742387a", "4", "2017/Spr", "UG"){ StudentLoad = "F", StudentTermStatuses = studentTermStatuseEntities.ToList(), StudentAcademicCredentials = new List<string>(){"1", "2"}}
                };
                stuTermsTuple = new Tuple<IEnumerable<StudentTerm>, int>(studentTermEntities, 4);

                List<StudentTypeInfo> studentTypeInfoList = new List<StudentTypeInfo>() 
                {
                    new StudentTypeInfo("code1", new DateTime(2015, 01, 01)),
                    new StudentTypeInfo("code2", new DateTime(2016, 01, 01)),
                    new StudentTypeInfo("code3", new DateTime(2016, 05, 01)),
                    new StudentTypeInfo("code4", new DateTime(2016, 12, 01)),
                };

                studentProgramStatusEntities = new List<Domain.Student.Entities.StudentProgramStatus>() 
                {
                    new Domain.Student.Entities.StudentProgramStatus("W", new DateTime(2017, 03, 02)),
                    new Domain.Student.Entities.StudentProgramStatus("A", new DateTime(2017, 03, 01)),
                    new Domain.Student.Entities.StudentProgramStatus("N", new DateTime(2016, 03, 01))
                };

                studentProgramEntities = new List<StudentProgram>() 
                {
                    new StudentProgram("1", "BA-MATH", "Code1" ){ StudentProgramStatuses = studentProgramStatusEntities.ToList() }
                };

                enrollmentStatusEntities = new List<Domain.Student.Entities.EnrollmentStatus>() 
                {
                    new Domain.Student.Entities.EnrollmentStatus("891ab399-259f-4597-aca7-57b1a7f31626", "A", "Active", Domain.Student.Entities.EnrollmentStatusType.active),
                    new Domain.Student.Entities.EnrollmentStatus("0d383982-fa39-4c3d-9a50-28223014c3c6", "W", "Withdrawn", Domain.Student.Entities.EnrollmentStatusType.inactive)
                };

                acadProgramEntities = new TestAcademicProgramRepository().GetAsync().Result as List<Domain.Student.Entities.AcademicProgram>;
                List<string> acadIds = acadProgramEntities.Select(i => i.Code).ToList();
                var students = new List<Domain.Student.Entities.Student>();
                for (int num = 1; num < 5; num++)
                {
                    student = new Domain.Student.Entities.Student("90019376-01a1-4149-89b9-2b755a4afe43", num.ToString(), "Bhole", 1, acadIds, new List<string>());
                    student.StudentTypeInfo = studentTypeInfoList;
                    student.ClassLevelCodes = studentClassificationEntities.Select(i => i.Code).ToList();
                    student.StudentAcademicLevels = new List<StudentAcademicLevel>() { new StudentAcademicLevel("UG", "A", "1G", "2016/Spr", null, true) };
                    student.StudentResidencies = new List<StudentResidency>() { new StudentResidency("INST", new DateTime(2016, 01, 01)) };
                    students.Add(student);
                }
                studentRepositoryMock.Setup(i => i.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                studentRepositoryMock.Setup(i => i.GetStudentAcademicPeriodProfileStudentInfoAsync(It.IsAny<List<string>>())).ReturnsAsync(students);
            }

            private void BuildMocks()
            {
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(personGuid)).ReturnsAsync("1");

                termRepositoryMock.Setup(i => i.GetAsync(It.IsAny<bool>())).ReturnsAsync(termEntities);
                termRepositoryMock.Setup(i => i.GetAcademicPeriods(termEntities)).Returns(academicPeriodEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentTypesAsync(It.IsAny<bool>())).ReturnsAsync(studentTypesEntities);
                foreach (var entity in studentTypesEntities)
                {
                    studentReferenceDataRepositoryMock.Setup(i => i.GetStudentTypesGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(acadLevelEntities);
                foreach (var entity in acadLevelEntities)
                {
                    studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicLevelsGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(studentStatuses);
                foreach (var entity in studentStatuses)
                {
                    studentReferenceDataRepositoryMock.Setup(i => i.GetStudentStatusesGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(i => i.GetAllStudentClassificationAsync(It.IsAny<bool>())).ReturnsAsync(studentClassificationEntities);
                foreach (var entity in studentClassificationEntities)
                {
                    studentReferenceDataRepositoryMock.Setup(i => i.GetStudentClassificationGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentLoadsAsync()).ReturnsAsync(allStudentLoadEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(acadProgramEntities);
                foreach (var entity in acadProgramEntities)
                {
                    studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicProgramsGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(i => i.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatusEntities);

                studentRepositoryMock.Setup(i => i.GetResidencyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(residencyStatusEntities);
                foreach (var entity in residencyStatusEntities)
                {
                    studentRepositoryMock.Setup(i => i.GetResidencyStatusGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
                }

                //studentProgramRepositoryMock.Setup(i => i.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentProgramEntities.First());

                studentProgramRepositoryMock.Setup(i => i.GetStudentAcademicPeriodProfileStudentProgramInfoAsync(It.IsAny<List<string>>())).ReturnsAsync(studentProgramEntities.ToList());

                IEnumerable<Domain.Student.Entities.AcademicCredit> filteredAcadCredit = acadCredits.Where(i => i.Id.Equals("1") || i.Id.Equals("2"));
                IEnumerable<Domain.Student.Entities.AcademicCreditMinimum> filteredAcadCredit2 = acadCredits2.Where(i => i.Id.Equals("1") || i.Id.Equals("2"));
                academicCreditRepositoryMock.Setup(i => i.GetAcademicCreditMinimumAsync(new Collection<string>() { "1", "2" }, It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(filteredAcadCredit2);
                academicCreditRepositoryMock.Setup(i => i.GetAcademicCreditMinimumAsync(new List<string>() { "1", "2" }, It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(filteredAcadCredit2);
            }
           
            [TestCleanup]
            public void Cleanup()
            {
                studentAcademicPeriodProfilesService = null;
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync()
            {
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });

                studentTermRepositoryMock.Setup(i => i.GetStudentTermsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuTermsTuple);

                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("1", "ed809943-eb26-42d0-9a95-d8db912a581f");
                dict.Add("2", "6f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("3", "1f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("4", "2f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                //personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                //    .Returns(Task.FromResult("ed809943-eb26-42d0-9a95-d8db912a581f"))
                //    .Returns(Task.FromResult("6f11fcd7-40bf-4c24-8e97-602c363eb8cf"))
                //    .Returns(Task.FromResult("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"))
                //    .Returns(Task.FromResult("2f11fcd7-40bf-4c24-8e97-602c363eb8cf"));

                var actuals = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = stuAcadPeriodProfileDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    //Code commented out
                    //Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, result.AcademicPeriodEnrollmentStatus.Id);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                    Assert.AreEqual(expected.StudentStatus.Id, actual.StudentStatus.Id);
                    

                    //Check Collections
                    Assert.AreEqual(expected.Measures.Count(), actual.Measures.Count());
                    foreach (var actualMeasure in actual.Measures)
                    {
                        var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Level.Id.Equals(actualMeasure.Level.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedMeasure);

                        Assert.AreEqual(expectedMeasure.Classification.Id, actualMeasure.Classification.Id);
                        Assert.AreEqual(expectedMeasure.Level.Id, actualMeasure.Level.Id);
                        Assert.AreEqual(expectedMeasure.PerformanceMeasure, actualMeasure.PerformanceMeasure);
                    }
                }
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_When_AcadCredentials_GPACredit_IsNUll()
            {
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });

                studentTermRepositoryMock.Setup(i => i.GetStudentTermsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuTermsTuple);
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("1", "ed809943-eb26-42d0-9a95-d8db912a581f");
                dict.Add("2", "6f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("3", "1f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("4", "2f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                //personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                //    .Returns(Task.FromResult("ed809943-eb26-42d0-9a95-d8db912a581f"))
                //    .Returns(Task.FromResult("6f11fcd7-40bf-4c24-8e97-602c363eb8cf"))
                //    .Returns(Task.FromResult("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"))
                //    .Returns(Task.FromResult("2f11fcd7-40bf-4c24-8e97-602c363eb8cf"));
                //override academicCredits mock here
                IEnumerable<Domain.Student.Entities.AcademicCredit> filteredAcadCredit = acadCredits.Where(i => i.Id.Equals("5") || i.Id.Equals("6"));
                academicCreditRepositoryMock.Setup(i => i.GetAsync(new Collection<string>() { "5", "6" }, false, true, It.IsAny<bool>())).ReturnsAsync(filteredAcadCredit);

                var actuals = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = stuAcadPeriodProfileDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    //Code commented out
                    //Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, result.AcademicPeriodEnrollmentStatus.Id);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                    Assert.AreEqual(expected.StudentStatus.Id, actual.StudentStatus.Id);


                    //Check Collections
                    Assert.AreEqual(expected.Measures.Count(), actual.Measures.Count());
                    foreach (var actualMeasure in actual.Measures)
                    {
                        var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Level.Id.Equals(actualMeasure.Level.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedMeasure);

                        Assert.AreEqual(expectedMeasure.Classification.Id, actualMeasure.Classification.Id);
                        Assert.AreEqual(expectedMeasure.Level.Id, actualMeasure.Level.Id);
                        Assert.AreEqual(expectedMeasure.PerformanceMeasure, actualMeasure.PerformanceMeasure);
                    }
                }
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_GetById()
            {
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a" };
                var tempStudentTermEntity = studentTermEntities.FirstOrDefault(i => guids.Contains(i.Guid));
                var expected = stuAcadPeriodProfileDtos.FirstOrDefault(i => i.Id.Equals("cf4d47eb-f06b-4add-b5bf-d9529742387a", StringComparison.OrdinalIgnoreCase));

                studentTermRepositoryMock.Setup(i => i.GetStudentTermByGuidAsync("cf4d47eb-f06b-4add-b5bf-d9529742387a")).ReturnsAsync(tempStudentTermEntity);
                //personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                //    .Returns(Task.FromResult("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"));
                Dictionary<string, string> dict = new Dictionary<string, string>();
                //dict.Add("1", "ed809943-eb26-42d0-9a95-d8db912a581f");
                //dict.Add("2", "6f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("3", "1f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                //dict.Add("4", "2f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);


                var actual = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfileByGuidAsync("cf4d47eb-f06b-4add-b5bf-d9529742387a");
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                //Code commented out
                //Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, result.AcademicPeriodEnrollmentStatus.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StudentStatus.Id, actual.StudentStatus.Id);
                Assert.AreEqual(expected.Type.Id, actual.Type.Id);

                //Check Collections
                Assert.AreEqual(expected.Measures.Count(), actual.Measures.Count());
                foreach (var actualMeasure in actual.Measures)
                {
                    var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Level.Id.Equals(actualMeasure.Level.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expectedMeasure);

                    Assert.AreEqual(expectedMeasure.Classification.Id, actualMeasure.Classification.Id);
                    Assert.AreEqual(expectedMeasure.Level.Id, actualMeasure.Level.Id);
                    Assert.AreEqual(expectedMeasure.PerformanceMeasure, actualMeasure.PerformanceMeasure);
                }
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_AcademicPeriodFilter()
            {
                var acadPeriodGuid = "8f3aac22-e0b5-4159-b4e2-da158362c41b";
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a", "df4d47eb-f06b-4add-b5bf-d9529742387a" };
                var tempStudentTermEntities = studentTermEntities.Where(i => guids.Contains(i.Guid));

                stuTermsTuple = new Tuple<IEnumerable<StudentTerm>, int>(tempStudentTermEntities, 2);
                studentTermRepositoryMock.Setup(i => i.GetStudentTermsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuTermsTuple);
                //personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                //    .Returns(Task.FromResult("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"))
                //    .Returns(Task.FromResult("2f11fcd7-40bf-4c24-8e97-602c363eb8cf"));

                Dictionary<string, string> dict = new Dictionary<string, string>();
                //dict.Add("1", "ed809943-eb26-42d0-9a95-d8db912a581f");
                //dict.Add("2", "6f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("3", "1f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("4", "2f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);


                var actuals = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), acadPeriodGuid);
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = stuAcadPeriodProfileDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    //Code commented out
                    //Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, result.AcademicPeriodEnrollmentStatus.Id);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                    Assert.AreEqual(expected.StudentStatus.Id, actual.StudentStatus.Id);
                    Assert.AreEqual(expected.Type.Id, actual.Type.Id);

                    //Check Collections
                    Assert.AreEqual(expected.Measures.Count(), actual.Measures.Count());
                    foreach (var actualMeasure in actual.Measures)
                    {
                        var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Level.Id.Equals(actualMeasure.Level.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedMeasure);

                        Assert.AreEqual(expectedMeasure.Classification.Id, actualMeasure.Classification.Id);
                        Assert.AreEqual(expectedMeasure.Level.Id, actualMeasure.Level.Id);
                        Assert.AreEqual(expectedMeasure.PerformanceMeasure, actualMeasure.PerformanceMeasure);
                    }
                }
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_PersonFilter()
            {
                var personGuid = "ed809943-eb26-42d0-9a95-d8db912a581f";
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a", "df4d47eb-f06b-4add-b5bf-d9529742387a" };
                var tempStudentTermEntities = studentTermEntities.Where(i => guids.Contains(i.Guid));

                stuTermsTuple = new Tuple<IEnumerable<StudentTerm>, int>(tempStudentTermEntities, 2);
                studentTermRepositoryMock.Setup(i => i.GetStudentTermsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuTermsTuple);
                //personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                //    .Returns(Task.FromResult("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"))
                //    .Returns(Task.FromResult("2f11fcd7-40bf-4c24-8e97-602c363eb8cf"));

                Dictionary<string, string> dict = new Dictionary<string, string>();
                //dict.Add("1", "ed809943-eb26-42d0-9a95-d8db912a581f");
                //dict.Add("2", "6f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("3", "1f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("4", "2f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);


                var actuals = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), personGuid, It.IsAny<string>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = stuAcadPeriodProfileDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    //Code commented out
                    //Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, result.AcademicPeriodEnrollmentStatus.Id);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                    Assert.AreEqual(expected.StudentStatus.Id, actual.StudentStatus.Id);
                    Assert.AreEqual(expected.Type.Id, actual.Type.Id);

                    //Check Collections
                    Assert.AreEqual(expected.Measures.Count(), actual.Measures.Count());
                    foreach (var actualMeasure in actual.Measures)
                    {
                        var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Level.Id.Equals(actualMeasure.Level.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedMeasure);

                        Assert.AreEqual(expectedMeasure.Classification.Id, actualMeasure.Classification.Id);
                        Assert.AreEqual(expectedMeasure.Level.Id, actualMeasure.Level.Id);
                        Assert.AreEqual(expectedMeasure.PerformanceMeasure, actualMeasure.PerformanceMeasure);
                    }
                }
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_EmptyTuple()
            {
                var acadPeriodGuid = "8f3aac22-e0b5-4159-b4e2-da158362c41b";
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a", "df4d47eb-f06b-4add-b5bf-d9529742387a" };
                var tempStudentTermEntities = studentTermEntities.Where(i => guids.Contains(i.Guid));

                stuTermsTuple = new Tuple<IEnumerable<StudentTerm>, int>(new List<StudentTerm>() { }, 0);
                studentTermRepositoryMock.Setup(i => i.GetStudentTermsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuTermsTuple);
                personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"))
                    .Returns(Task.FromResult("2f11fcd7-40bf-4c24-8e97-602c363eb8cf"));

                var actuals = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), acadPeriodGuid);
                Assert.IsNotNull(actuals);               
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_NullTuple()
            {
                var acadPeriodGuid = "8f3aac22-e0b5-4159-b4e2-da158362c41b";
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a", "df4d47eb-f06b-4add-b5bf-d9529742387a" };
                var tempStudentTermEntities = studentTermEntities.Where(i => guids.Contains(i.Guid));
                
                stuTermsTuple = null;
                studentTermRepositoryMock.Setup(i => i.GetStudentTermsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuTermsTuple);
                personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"))
                    .Returns(Task.FromResult("2f11fcd7-40bf-4c24-8e97-602c363eb8cf"));

                var actuals = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), acadPeriodGuid);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_KeyNotFoundException()
            {
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });
                studentTermRepositoryMock.Setup(i => i.GetStudentTermsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuTermsTuple);
                loggerMock.Setup(i => i.IsDebugEnabled).Returns(true);
                personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).Throws(new KeyNotFoundException()).Throws(new ArgumentException());

                var actuals = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>());                
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_PermissionsException()
            {
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                
                studentTermRepositoryMock.Setup(i => i.GetStudentTermsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuTermsTuple);

                var actuals = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_GetById_ArgumentNullException()
            {
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a" };
                var tempStudentTermEntity = studentTermEntities.FirstOrDefault(i => guids.Contains(i.Guid));
                var expected = stuAcadPeriodProfileDtos.FirstOrDefault(i => i.Id.Equals("cf4d47eb-f06b-4add-b5bf-d9529742387a", StringComparison.OrdinalIgnoreCase));

                studentTermRepositoryMock.Setup(i => i.GetStudentTermByGuidAsync("cf4d47eb-f06b-4add-b5bf-d9529742387a")).ReturnsAsync(tempStudentTermEntity);
                personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"));

                var actual = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfileByGuidAsync("");              
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_GetById_KeyNotFoundException()
            {
                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });

                studentTermRepositoryMock.Setup(i => i.GetStudentTermByGuidAsync("1234")).ReturnsAsync(null);

                var actual = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfileByGuidAsync("1234");
            }


            [TestMethod]
            public async Task StudentAcademicPeriodProfileService_GetStudentAcademicPerioProfileAsync_GetById_EnrollmentStatus()
            {
                studentProgramStatusEntities = new List<Domain.Student.Entities.StudentProgramStatus>()
                {
                    new Domain.Student.Entities.StudentProgramStatus("W", new DateTime(2017, 03, 02)),
                    new Domain.Student.Entities.StudentProgramStatus("A", new DateTime(2017, 03, 01))
                };
                studentProgramEntities = new List<StudentProgram>()
                {
                    new StudentProgram("3", "BA-MATH", "Code1" ){ StudentProgramStatuses = studentProgramStatusEntities.ToList() }
                };

                studentProgramRepositoryMock.Setup(i => i.GetStudentAcademicPeriodProfileStudentProgramInfoAsync(It.IsAny<List<string>>())).ReturnsAsync(studentProgramEntities.ToList());

                viewStudentAcadPeriodProfileRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodProfileRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a" };
                var tempStudentTermEntity = studentTermEntities.FirstOrDefault(i => guids.Contains(i.Guid));
                var expected = stuAcadPeriodProfileDtos.FirstOrDefault(i => i.Id.Equals("cf4d47eb-f06b-4add-b5bf-d9529742387a", StringComparison.OrdinalIgnoreCase));

                studentTermRepositoryMock.Setup(i => i.GetStudentTermByGuidAsync("cf4d47eb-f06b-4add-b5bf-d9529742387a")).ReturnsAsync(tempStudentTermEntity);
                Dictionary<string, string> dict = new Dictionary<string, string>();
                //dict.Add("1", "ed809943-eb26-42d0-9a95-d8db912a581f");
                //dict.Add("2", "6f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("3", "1f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                //dict.Add("4", "2f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                //personRepositoryMock.SetupSequence(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                //    .Returns(Task.FromResult("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"));

                var actual = await studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfileByGuidAsync("cf4d47eb-f06b-4add-b5bf-d9529742387a");
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                
                Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, actual.AcademicPeriodEnrollmentStatus.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StudentStatus.Id, actual.StudentStatus.Id);
                Assert.AreEqual(expected.Type.Id, actual.Type.Id);

                //Check Collections
                Assert.AreEqual(expected.Measures.Count(), actual.Measures.Count());
                foreach (var actualMeasure in actual.Measures)
                {
                    var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Level.Id.Equals(actualMeasure.Level.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expectedMeasure);

                    Assert.AreEqual(expectedMeasure.Classification.Id, actualMeasure.Classification.Id);
                    Assert.AreEqual(expectedMeasure.Level.Id, actualMeasure.Level.Id);
                    Assert.AreEqual(expectedMeasure.PerformanceMeasure, actualMeasure.PerformanceMeasure);
                }
            }

        }

        #endregion
    }
}