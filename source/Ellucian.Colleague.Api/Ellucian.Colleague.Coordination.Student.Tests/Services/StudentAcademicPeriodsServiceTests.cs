// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
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
    public class StudentAcademicPeriodsServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewStudentAcadPeriodsRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.ACADEMIC.PERIODS");
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
                            Roles = new List<string>() { "VIEW.STUDENT.ACADEMIC.PERIODS" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }


        [TestClass]
        public class StudentAcademicPeriodsServiceTests_Get : CurrentUserSetup
        {
            #region private variables
            private Mock<IStudentAcademicPeriodRepository> studentAcademicPeriodRepositoryMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;

            private ICurrentUserFactory currentUserFactory;
            private StudentAcademicPeriodsService studentAcademicPeriodsService;
            private List<AcademicPeriod> allAcademicPeriods = null;
            private string acadPeriodGuid = string.Empty;
            private string studentStatusGuid = string.Empty;
            private List<Domain.Student.Entities.Student> allStudents = null;
            private List<Domain.Student.Entities.StudentAcademicPeriod> allStudentAcadPeriodEntities;
            private List<Dtos.StudentAcademicPeriods> studentAcademicPeriodsCollection;
            private List<AcademicLevel> allAcademicLevels;
            private Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriods>, int> stuAcadPeriodProfilsDtoTuple;
            private Tuple<IEnumerable<StudentAcademicPeriod>, int> stuAcademicPeriodsTuple;
            private Domain.Student.Entities.Student student;
            private IEnumerable<StudentTerm> studentTermEntities;
            private IEnumerable<Domain.Student.Entities.StudentStatus> studentStatuses;
            private IEnumerable<Domain.Student.Entities.StudentLoad> allStudentLoadEntities;
            private IEnumerable<Domain.Student.Entities.StudentProgramStatus> studentProgramStatusEntities;
            private IEnumerable<Domain.Student.Entities.Term> termEntities;

            private string personGuid = "ed809943-eb26-42d0-9a95-d8db912a581f";
               #endregion 

            [TestInitialize]
            public async void Initialize()
            {
                studentAcademicPeriodRepositoryMock = new Mock<IStudentAcademicPeriodRepository>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

                termRepositoryMock = new Mock<ITermRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();

                roleRepositoryMock = new Mock<IRoleRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                BuildData();
                BuildMocks();
                studentAcademicPeriodsService = new StudentAcademicPeriodsService(
                    adapterRegistryMock.Object,
                    studentRepositoryMock.Object,
                    studentProgramRepositoryMock.Object,
                    studentAcademicPeriodRepositoryMock.Object,
                    termRepositoryMock.Object,
                    studentReferenceDataRepositoryMock.Object,
                    personRepositoryMock.Object,
                    referenceDataRepositoryMock.Object,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    configurationRepositoryMock.Object, loggerMock.Object);

            }

            private async void BuildData()
            {

                studentAcademicPeriodsCollection = new List<StudentAcademicPeriods>();

                studentStatuses = new List<Domain.Student.Entities.StudentStatus>()
                {
                    new Domain.Student.Entities.StudentStatus("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "Code1", "title1"),
                    new Domain.Student.Entities.StudentStatus("bd54668d-50d9-416c-81e9-2318e88571a1", "Code2", "title2"),
                    new Domain.Student.Entities.StudentStatus("5eed2bea-8948-439b-b5c5-779d84724a38", "Code3", "title3"),
                    new Domain.Student.Entities.StudentStatus("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "Code4", "title4")
                };

                allStudentLoadEntities = new List<Domain.Student.Entities.StudentLoad>()
                {
                    new Domain.Student.Entities.StudentLoad("F", "Full Time"){Sp1 = "1"},
                    new Domain.Student.Entities.StudentLoad("P", "Part Time"){Sp1 = "2"},
                    new Domain.Student.Entities.StudentLoad("O", "Overload"){Sp1 = "3" },
                    new Domain.Student.Entities.StudentLoad("H", "Half Time"){Sp1 = "0"},
                    new Domain.Student.Entities.StudentLoad("L", "Less than Half Time"){Sp1 = "0"}
                };
                termEntities = new List<Term>()
               {
                   new Term("b9691210-8516-45ca-9cd1-7e5aa1777234", "2000RSU", "RSU 2000", new DateTime(2000, 01,01), new DateTime(2000, 05,01), 2000, 1, false, false, "Spring", false),
                   new Term("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2000/S1", "Spring 1 2000",  new DateTime(2000, 09,01), new DateTime(2000, 10,15), 2000, 2, false, false, "Fall", false),
                   new Term("8f3aac22-e0b5-4159-b4e2-da158362c41b", "22000CS1", "CS1 2000", new DateTime(2000, 01,01), new DateTime(2000, 05,01), 2000, 3, false, false, "Spring", false),
                   new Term("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", "Fall 2017", new DateTime(2017, 09,01), new DateTime(2017, 12,31), 2017, 4, false, false, "Fall", false)

               };

                studentProgramStatusEntities = new List<Domain.Student.Entities.StudentProgramStatus>()
                {
                    new Domain.Student.Entities.StudentProgramStatus("W", new DateTime(2017, 03, 02)),
                    new Domain.Student.Entities.StudentProgramStatus("A", new DateTime(2017, 03, 01)),
                    new Domain.Student.Entities.StudentProgramStatus("N", new DateTime(2016, 03, 01))
                };


                allAcademicPeriods = new TestAcademicPeriodRepository().Get().ToList();
                allStudents = (await new TestStudentRepository().GetAllAsync()).ToList(); ;
                allAcademicLevels = (await new TestAcademicLevelRepository().GetAsync()).ToList();
                var acadLevelGrad = allAcademicLevels.FirstOrDefault(a1 => a1.Code.Equals("GR"));
                var acadLevelUnderGrad = allAcademicLevels.FirstOrDefault(a2 => a2.Code.Equals("UG"));

                var acadPeriod = termEntities.FirstOrDefault();
                acadPeriodGuid = acadPeriod.RecordGuid;

                var studentStatus = studentStatuses.FirstOrDefault();
                studentStatusGuid = studentStatus.Guid;

                allStudentAcadPeriodEntities = new List<Domain.Student.Entities.StudentAcademicPeriod>();

                var student1 = allStudents.FirstOrDefault(s1 => s1.Id.Equals("00004001"));

                allStudentAcadPeriodEntities.Add(new StudentAcademicPeriod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", student1.Id,
                    acadPeriod.Code)
                {
                    StudentTerms = new List<StudentTerm>
                { new StudentTerm(Guid.NewGuid().ToString(), student1.Id, acadPeriod.Code, acadLevelGrad.Code) }
                });

                var student2 = allStudents.FirstOrDefault(s2 => s2.Id.Equals("00004002"));

                allStudentAcadPeriodEntities.Add(new StudentAcademicPeriod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", student2.Id,
                      acadPeriod.Code)
                {
                    StudentTerms = new List<StudentTerm>
                {
                    new StudentTerm(Guid.NewGuid().ToString(), student2.Id, acadPeriod.Code, acadLevelGrad.Code) }
                });

                var student3 = allStudents.FirstOrDefault(s3 => s3.Id.Equals("00004003"));

                allStudentAcadPeriodEntities.Add(new StudentAcademicPeriod("d2253ac7-9931-4560-b42f-1fccd43c952e", student3.Id,
                    acadPeriod.Code)
                {
                    StudentTerms = new List<StudentTerm>
                { new StudentTerm(Guid.NewGuid().ToString(), student3.Id, acadPeriod.Code, acadLevelUnderGrad.Code) }
                });

                foreach (var source in allStudentAcadPeriodEntities)
                {
                    var studentAcademicPeriod = new Ellucian.Colleague.Dtos.StudentAcademicPeriods
                    {
                        Id = source.Guid,
                    };
                    student = allStudents.FirstOrDefault(s => s.Id.Equals(source.StudentId));
                    studentAcademicPeriod.Person = new GuidObject2(student.StudentGuid);
                    studentAcademicPeriod.AcademicPeriod = new GuidObject2(acadPeriodGuid);
                    var studentAcademicPeriodAcademicLevels = new List<StudentAcademicPeriodsAcademicLevels>();
                    var studentAcademicPeriodsAcademicStatuses = new List<StudentAcademicPeriodsAcademicStatuses>();

                    foreach (var level in source.StudentTerms)
                    {
                        var studentAcademicPeriodsAcademicLevel = new StudentAcademicPeriodsAcademicLevels();
                        var studentAcademicPeriodsAcademicStatus = new StudentAcademicPeriodsAcademicStatuses();

                        var acadLevelGuid = string.Empty;
                        if (level.Equals(acadLevelGrad.Code))
                            acadLevelGuid = acadLevelGrad.Guid;
                        else if (level.Equals(acadLevelUnderGrad.Code))
                            acadLevelGuid = acadLevelUnderGrad.Guid;
                        studentAcademicPeriodsAcademicLevel.AcademicLevel = new GuidObject2(acadLevelGuid);
                        studentAcademicPeriodAcademicLevels.Add(studentAcademicPeriodsAcademicLevel);

                        studentAcademicPeriodsAcademicStatus.AcademicLevel = new GuidObject2(acadLevelGuid);
                        studentAcademicPeriodsAcademicStatus.Basis = Dtos.EnumProperties.StudentAcademicPeriodsBasis.ByLevel;
                        studentAcademicPeriodsAcademicStatus.AcademicPeriodStatus = new GuidObject2(studentStatusGuid);
                        studentAcademicPeriodsAcademicStatuses.Add(studentAcademicPeriodsAcademicStatus);
                    }
                    studentAcademicPeriod.AcademicStatuses = studentAcademicPeriodsAcademicStatuses;
                    studentAcademicPeriod.AcademicLevels = studentAcademicPeriodAcademicLevels;
                    studentAcademicPeriodsCollection.Add(studentAcademicPeriod);
                }

                stuAcadPeriodProfilsDtoTuple = new Tuple<IEnumerable<StudentAcademicPeriods>, int>(studentAcademicPeriodsCollection.AsEnumerable(), studentAcademicPeriodsCollection.Count());

                stuAcademicPeriodsTuple = new Tuple<IEnumerable<StudentAcademicPeriod>, int>(allStudentAcadPeriodEntities, allStudentAcadPeriodEntities.Count());
                
                studentRepositoryMock.Setup(i => i.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
            }

            private void BuildMocks()
            {
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(personGuid)).ReturnsAsync("1");

                termRepositoryMock.Setup(i => i.GetAsync(It.IsAny<bool>())).ReturnsAsync(termEntities);
                foreach (var term in termEntities)
                {
                    termRepositoryMock.Setup(i => i.GetAcademicPeriodsGuidAsync(term.Code)).ReturnsAsync(term.RecordGuid);
                }

                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(allAcademicLevels);
                foreach (var entity in allAcademicLevels)
                {
                    studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicLevelsGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(studentStatuses);
                foreach (var entity in studentStatuses)
                {
                    studentReferenceDataRepositoryMock.Setup(i => i.GetStudentStatusesGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
                }

                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentLoadsAsync()).ReturnsAsync(allStudentLoadEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAcademicPeriodsService = null;
                studentAcademicPeriodRepositoryMock = null;
                studentRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                termRepositoryMock = null;
                personRepositoryMock = null;
                referenceDataRepositoryMock = null;
                configurationRepositoryMock = null;
                studentProgramRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task StudentAcademicPeriodsService_GetStudentAcademicPeriodsAsync()
            {
                viewStudentAcadPeriodsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriods));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodsRole });

                studentAcademicPeriodRepositoryMock.Setup(s => s.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ReturnsAsync(stuAcademicPeriodsTuple);

                Dictionary<string, string> dict = new Dictionary<string, string>();
                int i = 1;
                foreach (var student in allStudents)
                {
                    dict.Add(string.Format("0000400{0}", i.ToString()), student.StudentGuid);
                    i++;
                }
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                var actuals = await studentAcademicPeriodsService.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), It.IsAny<bool>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = stuAcadPeriodProfilsDtoTuple.Item1.FirstOrDefault(s => s.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id, "academic-period");

                    Assert.AreEqual(expected.Id, actual.Id, "guid");
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id, "person.id");
                    Assert.IsNotNull(actual.AcademicLevels);
                }
            }

            [TestMethod]
            public async Task StudentAcademicPeriodService_GetStudentAcademicPeriodsAsync_GetById()
            {
                viewStudentAcadPeriodsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriods));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodsRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a" };
                var expectedEntity = stuAcademicPeriodsTuple.Item1.FirstOrDefault();
                studentAcademicPeriodRepositoryMock.Setup(s => s.GetStudentAcademicPeriodByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                var dict = new Dictionary<string, string>();
                int i = 1;
                foreach (var student in allStudents)
                {
                    dict.Add(string.Format("0000400{0}", i.ToString()), student.StudentGuid);
                    i++;
                }
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                var actual = await studentAcademicPeriodsService.GetStudentAcademicPeriodsByGuidAsync("cf4d47eb-f06b-4add-b5bf-d9529742387a", false);
                Assert.IsNotNull(actual);
                var expected = stuAcadPeriodProfilsDtoTuple.Item1.FirstOrDefault(s => s.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.IsNotNull(actual.AcademicLevels);
               
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentAcademicPeriodService_GetStudentAcademicPeriodsAsync_GetById_RepoException()
            {
                viewStudentAcadPeriodsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriods));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodsRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a" };
                var expectedEntity = stuAcademicPeriodsTuple.Item1.FirstOrDefault();
                studentAcademicPeriodRepositoryMock.Setup(s => s.GetStudentAcademicPeriodByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new RepositoryException());
                var dict = new Dictionary<string, string>();
                int i = 1;
                foreach (var student in allStudents)
                {
                    dict.Add(string.Format("0000400{0}", i.ToString()), student.StudentGuid);
                    i++;
                }
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                await studentAcademicPeriodsService.GetStudentAcademicPeriodsByGuidAsync("cf4d47eb-f06b-4add-b5bf-d9529742387a", false);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicPeriodService_StudentAcademicPeriodAsync_RepoException()
            {
                viewStudentAcadPeriodsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriods));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodsRole });

                studentAcademicPeriodRepositoryMock.Setup(s => s.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ThrowsAsync(new RepositoryException());

                Dictionary<string, string> dict = new Dictionary<string, string>();
                int i = 1;
                foreach (var student in allStudents)
                {
                    dict.Add(string.Format("0000400{0}", i.ToString()), student.StudentGuid);
                    i++;
                }
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                await studentAcademicPeriodsService.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), It.IsAny<bool>());
                
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicPeriodService_StudentAcademicPeriodAsync_InvalidPersonGuids()
            {
                viewStudentAcadPeriodsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriods));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodsRole });

                studentAcademicPeriodRepositoryMock.Setup(s => s.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ReturnsAsync(stuAcademicPeriodsTuple);

                Dictionary<string, string> dict = new Dictionary<string, string>();
                int i = 1;
                foreach (var student in allStudents)
                {
                    dict.Add(string.Format("0000400{0}", i.ToString()), student.StudentGuid);
                    i++;
                }
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new IntegrationApiException());

                 await studentAcademicPeriodsService.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), It.IsAny<bool>());              
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentAcademicPeriodService_StudentAcademicPeriodAsync_GetById_InvalidPersonGuids()
            {
                viewStudentAcadPeriodsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriods));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodsRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a" };
                var expectedEntity = stuAcademicPeriodsTuple.Item1.FirstOrDefault();
                studentAcademicPeriodRepositoryMock.Setup(i => i.GetStudentAcademicPeriodByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                await studentAcademicPeriodsService.GetStudentAcademicPeriodsByGuidAsync("cf4d47eb-f06b-4add-b5bf-d9529742387a", false);

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentAcademicPeriodService_StudentAcademicPeriodAsync_GetById_Permissions()
            {
                viewStudentAcadPeriodsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodsRole });
                var guids = new[] { "cf4d47eb-f06b-4add-b5bf-d9529742387a" };
                var expectedEntity = stuAcademicPeriodsTuple.Item1.FirstOrDefault();
                studentAcademicPeriodRepositoryMock.Setup(i => i.GetStudentAcademicPeriodByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                await studentAcademicPeriodsService.GetStudentAcademicPeriodsByGuidAsync("cf4d47eb-f06b-4add-b5bf-d9529742387a", false);

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentAcademicPeriodService_StudentAcademicPeriodAsync_PermissionsException()
            {
                viewStudentAcadPeriodsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicPeriodProfile));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAcadPeriodsRole });

                studentAcademicPeriodRepositoryMock.Setup(i => i.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ReturnsAsync(stuAcademicPeriodsTuple);
                await studentAcademicPeriodsService.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), It.IsAny<bool>());
            }
        }
    }
}