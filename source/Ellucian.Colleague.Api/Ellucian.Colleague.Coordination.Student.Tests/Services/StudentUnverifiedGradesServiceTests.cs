//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{


    [TestClass]
    public class StudentUnverifiedGradesServiceTests
    {

        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewStudentUnverifiedGradeRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.UNVERIFIED.GRADES");
            protected Ellucian.Colleague.Domain.Entities.Role viewStudentUnverifiedGradeSubmissionRole = new Ellucian.Colleague.Domain.Entities.Role(1, "UPDATE.STUDENT.UNVERIFIED.GRADES.SUBMISSIONS");
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
                            Roles = new List<string>() { "VIEW.STUDENT.UNVERIFIED.GRADES", "UPDATE.STUDENT.UNVERIFIED.GRADES.SUBMISSIONS" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentUnverifiedGrades_Get : CurrentUserSetup
        {
            private const string studentAdvisorRelationshipsGuid = "635a3ad5-59ab-47ca-af87-8538c2ad727f";
            private ICollection<Domain.Student.Entities.StudentUnverifiedGrades> _studentUnverifiedGradesCollection;
            private StudentUnverifiedGradesService _studentUnverifiedGradesService;
            private Mock<ILogger> _loggerMock;
            private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
            private Mock<IStudentUnverifiedGradesRepository> _studentUnverifiedGradesRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<ISectionRepository> _sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> _sectionRegistrationRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IGradeRepository> _gradeRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            ICurrentUserFactory curntUserFactory;
  
            private List<Dtos.StudentUnverifiedGrades> _dtoStudentUnverifiedGradesList;
            private List<Domain.Student.Entities.Grade> gradeList;
            private List<Domain.Student.Entities.SectionGradeType> sectionGradeTypeList;
            private List<Domain.Student.Entities.GradeScheme> gradeSchemeList;

            private readonly DateTime _currentDate = DateTime.Now;
            private const string StudentUnverifiedGradesGuid = "3632ece0-8b9e-495f-a697-b5c9e053aad5";
            private Dtos.StudentUnverifiedGradesSubmissions _studentUnverifiedGradesSubmissions;

            [TestInitialize]
            public void Initialize()
            {
                _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _loggerMock = new Mock<ILogger>();
                _studentUnverifiedGradesRepositoryMock = new Mock<IStudentUnverifiedGradesRepository>();
                _sectionRepositoryMock = new Mock<ISectionRepository>();
                _sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                //_advisorTypesServiceMock = new Mock<IAdvisorTypesService>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _gradeRepoMock = new Mock<IGradeRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                viewStudentUnverifiedGradeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGrades));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeRole });


                _studentUnverifiedGradesSubmissions = new Dtos.StudentUnverifiedGradesSubmissions()
                {
                    Id = StudentUnverifiedGradesGuid,

                    SectionRegistration = new Dtos.GuidObject2("secGuid1"),
                    Grade = new Dtos.StudentUnverifiedGradesGradeDtoProperty()
                    {
                        Grade = new Dtos.GuidObject2("progguid1"),
                        Type = new Dtos.GuidObject2("typeguid1"),
                        IncompleteGrade = new Dtos.StudentUnverifiedGradesIncompleteGradeDtoProperty()
                        {
                            ExtensionDate = _currentDate,
                            FinalGrade = new Dtos.GuidObject2("progguid2")
                        }
                    },
                    LastAttendance = new Dtos.StudentUnverifiedGradesLastAttendanceDtoProperty()
                    {
                        Date = _currentDate,
                        Status = Dtos.EnumProperties.StudentUnverifiedGradesStatus.NotSet
                    }
                };

                _studentUnverifiedGradesCollection = new List<Domain.Student.Entities.StudentUnverifiedGrades>()
                {
                    new Domain.Student.Entities.StudentUnverifiedGrades("3632ece0-8b9e-495f-a697-b5c9e053aad5", "stu1") { StudentId = "stu1", StudentAcadaCredId = "secGuid1", GradeScheme = "grd1", FinalGrade = "ProgCode1",
                        HasNeverAttended = false, LastAttendDate = DateTime.Today },
                    new Domain.Student.Entities.StudentUnverifiedGrades("176d35fb-5f7a-4c06-b3ae-65a7662c8b43", "stu2") { StudentId = "stu2", StudentAcadaCredId = "secGuid2", GradeScheme = "grd2" },
                    new Domain.Student.Entities.StudentUnverifiedGrades("635a3ad5-59ab-47ca-af87-8538c2ad727f", "stu3") { StudentId = "stu3", StudentAcadaCredId = "secGuid3", GradeScheme = "grd3" },
                };

                _dtoStudentUnverifiedGradesList = new List<Dtos.StudentUnverifiedGrades>()
            {
                new Dtos.StudentUnverifiedGrades()
                {
                    Id = "3632ece0-8b9e-495f-a697-b5c9e053aad5",
                    Student = new GuidObject2("stuGuid1"),
                    AwardGradeScheme = new GuidObject2("grdguid1"),
                    SectionRegistration = new GuidObject2("secGuid1"),
                    Details = new StudentUnverifiedGradesDetails()
                    {  Grades = new List<StudentUnverifiedGradesGrades>()
                        {
                            new StudentUnverifiedGradesGrades()
                            {
                                Grade = new Dtos.GuidObject2("progguid1"),
                                Type = new Dtos.GuidObject2("typeguid1")
                            }
                        }
                    }
                },
                new Dtos.StudentUnverifiedGrades()
                {
                    Id = "176d35fb-5f7a-4c06-b3ae-65a7662c8b43",
                    Student = new GuidObject2("stuGuid2"),
                    AwardGradeScheme = new GuidObject2("grdguid2"),
                    SectionRegistration = new GuidObject2("secGuid2")
                },
                new Dtos.StudentUnverifiedGrades()
                {
                    Id = "635a3ad5-59ab-47ca-af87-8538c2ad727f",
                    Student = new GuidObject2("stuGuid3"),
                    AwardGradeScheme = new GuidObject2("grdguid3"),
                    SectionRegistration = new GuidObject2("secGuid3")
                },

            };

                gradeList = new List<Domain.Student.Entities.Grade>()
            {
                new Domain.Student.Entities.Grade("progguid1", "ProgCode1", "A", "1",  "Prog description 1", "grd1"),
                new Domain.Student.Entities.Grade("progguid2", "ProgCode2", "A", "2",  "Prog description 2", "grd2"),
                new Domain.Student.Entities.Grade("progguid3", "ProgCode3", "A", "3",  "Prog description 3", "grd3"),
                new Domain.Student.Entities.Grade("progguid4", "ProgCode4", "A", "4",  "Prog description 4", "grd4"),
                new Domain.Student.Entities.Grade("progguid5", "ProgCode5", "A", "5",  "Prog description 5", "grd5"),
                new Domain.Student.Entities.Grade("progguid6", "ProgCode6", "A", "6",  "Prog description 6", "grd6"),
                };

                sectionGradeTypeList = new List<Domain.Student.Entities.SectionGradeType>()
            {
                new Domain.Student.Entities.SectionGradeType("typeguid1", "Type1", "Ad type Descpt"),
                new Domain.Student.Entities.SectionGradeType("typeguid2", "FINAL", "Final Grade"),
                new Domain.Student.Entities.SectionGradeType("typeguid3", "MID1", "Midterm Grade 1"),
                new Domain.Student.Entities.SectionGradeType("typeguid4", "MID2", "Midterm Grade 2"),
                new Domain.Student.Entities.SectionGradeType("typeguid5", "MID3", "Midterm Grade 3"),
                new Domain.Student.Entities.SectionGradeType("typeguid6", "MID4", "Midterm Grade 4"),
                new Domain.Student.Entities.SectionGradeType("typeguid7", "MID5", "Midterm Grade 5"),
                new Domain.Student.Entities.SectionGradeType("typeguid8", "MID6", "Midterm Grade 6"),
            };

                gradeSchemeList = new List<Domain.Student.Entities.GradeScheme>()
            {
                new Domain.Student.Entities.GradeScheme("grdGuid1", "grd1", "Ad type Descpt"),
                new Domain.Student.Entities.GradeScheme("grdGuid2", "grd2", "Ad type Descpt"),
                new Domain.Student.Entities.GradeScheme("grdGuid3", "grd3", "Ad type Descpt"),
            };

                _studentUnverifiedGradesService = new StudentUnverifiedGradesService(_referenceRepositoryMock.Object, _personRepositoryMock.Object,
                    _gradeRepoMock.Object, _sectionRepositoryMock.Object, _studentUnverifiedGradesRepositoryMock.Object, _sectionRegistrationRepositoryMock.Object,
                    _adapterRegistryMock.Object, curntUserFactory, _roleRepositoryMock.Object, baseConfigurationRepository, _loggerMock.Object);

                _gradeRepoMock.Setup(x => x.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeList);

                _referenceRepositoryMock.Setup(x => x.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(sectionGradeTypeList);
                _referenceRepositoryMock.Setup(x => x.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemeList);


                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("sec1")).ReturnsAsync("secGuid1");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("sec2")).ReturnsAsync("secGuid2");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("sec3")).ReturnsAsync("secGuid3");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("stu1")).ReturnsAsync("stuGuid1");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("stu2")).ReturnsAsync("stuGuid2");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("stu3")).ReturnsAsync("stuGuid3");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("secGuid1")).ReturnsAsync("sec1");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("secGuid2")).ReturnsAsync("sec2");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("secGuid3")).ReturnsAsync("sec3");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("stuGuid1")).ReturnsAsync("stu1");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("stuGuid2")).ReturnsAsync("stu2");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("stuGuid3")).ReturnsAsync("stu3");
                var studentDictionary = new Dictionary<string, string>();
                studentDictionary.Add("stu1", "stuGuid1");
                studentDictionary.Add("stu2", "stuGuid2");
                studentDictionary.Add("stu3", "stuGuid3");
                _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(studentDictionary);
                var studentAcadCredDictionary = new Dictionary<string, string>();
                studentAcadCredDictionary.Add("secGuid1", "secGuid1");
                studentAcadCredDictionary.Add("secGuid2", "secGuid2");
                studentAcadCredDictionary.Add("secGuid3", "secGuid3");
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetGuidsCollectionAsync(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(studentAcadCredDictionary);

                var returnTuple = new Tuple<string, string, string>("stu1", "secGuid1", "grd1");
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredDataFromIdAsync(It.IsAny<string>())).ReturnsAsync(returnTuple);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _studentUnverifiedGradesService = null;
                _studentUnverifiedGradesCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
            }

            #region GET

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetStudentAdvisorRelationshipsAsync()
            {
                Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int> tupleResult = new Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int>(_studentUnverifiedGradesCollection, 3);

                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleResult);

                var results = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesAsync(0, 100);

                Assert.IsNotNull(results);
                Assert.AreEqual(3, results.Item2);
                Assert.AreEqual(results.Item1.Count(), results.Item2);

                foreach (var actual in results.Item1)
                {
                    var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == actual.Id);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                    if (actual.SectionRegistration != null || expected.SectionRegistration != null)
                    {
                        Assert.AreEqual(expected.SectionRegistration.Id, actual.SectionRegistration.Id);
                    }
                    if (actual.AwardGradeScheme != null || expected.AwardGradeScheme != null)
                    {
                        Assert.AreEqual(expected.AwardGradeScheme.Id, actual.AwardGradeScheme.Id);
                    }
                }

            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesAsync_filters()
            {
                Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int> tupleResult = new Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int>(_studentUnverifiedGradesCollection, 3);

                _sectionRegistrationRepositoryMock.Setup(x => x.GetSectionRegistrationIdFromGuidAsync("secGuid1")).ReturnsAsync("sec1");
                _sectionRepositoryMock.Setup(x => x.GetSectionIdFromGuidAsync("secGuid1")).ReturnsAsync("sec1");

                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), "stu1", "sec1", "sec1")).ReturnsAsync(tupleResult);

                var results = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesAsync(0, 100,
                    "stuGuid1", "secGuid1", "secGuid1", false);

                Assert.IsNotNull(results);
                Assert.AreEqual(3, results.Item2);
                Assert.AreEqual(results.Item1.Count(), results.Item2);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesByGuidAsync()
            {
                Domain.Student.Entities.StudentUnverifiedGrades entity = _studentUnverifiedGradesCollection.First();
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == entity.Guid);

                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesByGuidAsync(entity.Guid);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                if (actual.SectionRegistration != null || expected.SectionRegistration != null)
                {
                    Assert.AreEqual(expected.SectionRegistration.Id, actual.SectionRegistration.Id);
                }
                if (actual.AwardGradeScheme != null || expected.AwardGradeScheme != null)
                {
                    Assert.AreEqual(expected.AwardGradeScheme.Id, actual.AwardGradeScheme.Id);
                }
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesByGuidAsync_NeverAttended()
            {
                Domain.Student.Entities.StudentUnverifiedGrades entity = _studentUnverifiedGradesCollection.First();
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == entity.Guid);
                entity.HasNeverAttended = true;
                entity.FinalGrade = "";
                entity.LastAttendDate = null;

                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesByGuidAsync(entity.Guid);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                if (actual.SectionRegistration != null || expected.SectionRegistration != null)
                {
                    Assert.AreEqual(expected.SectionRegistration.Id, actual.SectionRegistration.Id);
                }
                if (actual.AwardGradeScheme != null || expected.AwardGradeScheme != null)
                {
                    Assert.AreEqual(expected.AwardGradeScheme.Id, actual.AwardGradeScheme.Id);
                }
                Assert.AreEqual(StudentUnverifiedGradesStatus.Neverattended, actual.Details.LastAttendance.Status);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesAsync_FailStudentfilters()
            {
                Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int> tupleResult = new Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int>(_studentUnverifiedGradesCollection, 3);

                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleResult);

                var results = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesAsync(0, 100,
                    "anything", "", "", false);

                Assert.AreEqual(results.Item1.Count(), 0);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesAsync_FailSectionRegistrationfilters()
            {
                Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int> tupleResult = new Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int>(_studentUnverifiedGradesCollection, 3);

                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleResult);

                var results = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesAsync(0, 100,
                    "", "anything", "", false);

                Assert.AreEqual(results.Item1.Count(), 0);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesAsync_FailSectionfilters()
            {
                Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int> tupleResult = new Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int>(_studentUnverifiedGradesCollection, 3);

                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleResult);

                var results = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesAsync(0, 100,
                    "", "", "anything", false);

                Assert.AreEqual(results.Item1.Count(), 0);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesByGuidAsync_InvalidId()
            {
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

                await _studentUnverifiedGradesService.GetStudentUnverifiedGradesByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesByGuidAsync_InvalidOperationException()
            {
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).Throws<InvalidOperationException>();

                try
                {
                    await _studentUnverifiedGradesService.GetStudentUnverifiedGradesByGuidAsync("99");
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Operation is not valid due to the current state of the object.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesByGuidAsync_NoStudentGuidFound()
            {
                Domain.Student.Entities.StudentUnverifiedGrades entity = _studentUnverifiedGradesCollection.First();
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.StudentId = "notFoundID";

                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                try
                {
                    await _studentUnverifiedGradesService.GetStudentUnverifiedGradesByGuidAsync("99");
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Unable to locate PERSONS guid for id 'notFoundID'.", ex.Errors[0].Message);
                    throw;
                }
            }
       
            #endregion

            #region StudentUnverifiedGradesSubmissions PUT/POST Validations

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Validate_Id()
            {
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesSubmissions.Id = null;
                await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Validate_SectionReg()
            {
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesSubmissions.SectionRegistration = null;
                await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Validate_SectionRegId()
            {
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesSubmissions.SectionRegistration = new GuidObject2();
                await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
            }

         
            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Validate_GradeTypeId()
            {
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesSubmissions.Grade = new StudentUnverifiedGradesGradeDtoProperty()
                {
                    Type = new GuidObject2()
                };
                await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Validate_FinalGradeID()
            {
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesSubmissions.Grade = new StudentUnverifiedGradesGradeDtoProperty()
                {
                    IncompleteGrade = new StudentUnverifiedGradesIncompleteGradeDtoProperty()
                    {
                        FinalGrade = new GuidObject2()
                    }
                };
                await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Validate_LastAttend()
            {
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesSubmissions.LastAttendance = new StudentUnverifiedGradesLastAttendanceDtoProperty()
                {
                    Date = null,
                    Status = Dtos.EnumProperties.StudentUnverifiedGradesStatus.NotSet
                };
                await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
            }

            #endregion

            #region StudentUnverifiedGradesSubmissions PUT

           
            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Put_InvalidId()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                // var studentUnverifiedGradesSubmissionsEntityId = await _studentUnverifiedGradesRepository.GetStudentUnverifiedGradesIdFromGuidAsync(studentUnverifiedGradesSubmissions.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                try
                {
                    await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Unable to obtain id for SectionRegistration:  secGuid1", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Put_InvalidGradeID()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);

                // var studentUnverifiedGradesSubmissionsEntityId = await _studentUnverifiedGradesRepository.GetStudentUnverifiedGradesIdFromGuidAsync(studentUnverifiedGradesSubmissions.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);

                // var studentAcadCredId = await this._studentUnverifiedGradesRepository.GetStudentAcademicCredIdFromGuidAsync(source.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);
                
                _studentUnverifiedGradesSubmissions.Grade.Grade.Id = "invalidGrade";

                try
                {
                    var actual = await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Unable to retrieve grade definition for id: 'invalidGrade'.", ex.Errors[0].Message);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Put_FinalGradeExpirationDate()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                try
                {
                    await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Incomplete grade details only apply to final grades.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Put_FinalGradesException()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade.ExtensionDate = null;

                try
                {
                    await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Incomplete grade details only apply to final grades.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Put_RepositoryException()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade = null;
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Domain.Student.Entities.StudentUnverifiedGrades>()))
                    .Throws<RepositoryException>();

                try
                {
                    var actual = await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Repository exception", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentUnverifiedGradesService_Put_KeyNotFoundException()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade = null;
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Domain.Student.Entities.StudentUnverifiedGrades>()))
                    .Throws<KeyNotFoundException>();

                var actual = await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Put_ArgumentException()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade = null;
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Domain.Student.Entities.StudentUnverifiedGrades>()))
                    .Throws<ArgumentException>();

                try
                {
                    var actual = await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Value does not fall within the expected range.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_Put_Success()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade = null;
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Domain.Student.Entities.StudentUnverifiedGrades>())).ReturnsAsync(expectedEntity);

                var actual = await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                Assert.AreEqual(expected.SectionRegistration.Id, actual.SectionRegistration.Id);
                //Assert.AreEqual(expected.Details.Grades[0].Grade.Id, actual.Details.Grades[0].Grade.Id);
                //Assert.AreEqual(expected.Details.Grades[0].Type.Id, actual.Details.Grades[0].Type.Id);
            }

            #endregion

            #region StudentUnverifiedGradesSubmissions POST

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Post_InvalidId()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                // var studentUnverifiedGradesSubmissionsEntityId = await _studentUnverifiedGradesRepository.GetStudentUnverifiedGradesIdFromGuidAsync(studentUnverifiedGradesSubmissions.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);
                try
                {
                    await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Unable to obtain id for SectionRegistration:  secGuid1", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Post_InvalidGradeID()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);

                // var studentUnverifiedGradesSubmissionsEntityId = await _studentUnverifiedGradesRepository.GetStudentUnverifiedGradesIdFromGuidAsync(studentUnverifiedGradesSubmissions.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);

                // var studentAcadCredId = await this._studentUnverifiedGradesRepository.GetStudentAcademicCredIdFromGuidAsync(source.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.Grade.Id = "invalidGrade";

                try
                {
                    var actual = await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Unable to retrieve grade definition for id: 'invalidGrade'.", ex.Errors[0].Message);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Post_FinalGradeExpirationDate()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                try
                {
                    await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Incomplete grade details only apply to final grades.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Post_FinalGradesException()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade.ExtensionDate = null;

                try
                {
                    await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Incomplete grade details only apply to final grades.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Post_RepositoryException()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade = null;
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Domain.Student.Entities.StudentUnverifiedGrades>()))
                    .Throws<RepositoryException>();

                try
                {
                    var actual = await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Repository exception", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Post_KeyNotFoundException()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade = null;
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Domain.Student.Entities.StudentUnverifiedGrades>()))
                    .Throws<KeyNotFoundException>();

                try
                {
                    var actual = await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("The given key was not present in the dictionary.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentUnverifiedGradesService_Post_ArgumentException()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade = null;
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Domain.Student.Entities.StudentUnverifiedGrades>()))
                    .Throws<ArgumentException>();

                try
                {
                    var actual = await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Value does not fall within the expected range.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_Post_Success()
            {
                viewStudentUnverifiedGradeSubmissionRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentUnverifiedGradeSubmissionRole });

                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expected = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);

                _studentUnverifiedGradesSubmissions.Grade.IncompleteGrade = null;
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Domain.Student.Entities.StudentUnverifiedGrades>())).ReturnsAsync(expectedEntity);

                var actual = await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                Assert.AreEqual(expected.SectionRegistration.Id, actual.SectionRegistration.Id);
                //Assert.AreEqual(expected.Details.Grades[0].Grade.Id, actual.Details.Grades[0].Grade.Id);
                //Assert.AreEqual(expected.Details.Grades[0].Type.Id, actual.Details.Grades[0].Type.Id);
            }
            #endregion

            #region StudentUnverifiedGradesSubmissions GetStudentUnverifiedGradesSubmissionsByGuid


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentUnverifiedGradesService_GetId_KeyNotFoundException()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

                await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentUnverifiedGradesService_GetId_InvalidOperationException()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcademicCredIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentAcadaCredId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).Throws<InvalidOperationException>();

                await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetId_Success()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(_studentUnverifiedGradesSubmissions.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                
                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);

                Assert.AreEqual(_studentUnverifiedGradesSubmissions.Id, actual.Id);
                Assert.AreEqual(_studentUnverifiedGradesSubmissions.SectionRegistration.Id, actual.SectionRegistration.Id);
                //Assert.AreEqual(_studentUnverifiedGradesSubmissions.Grade.Grade.Id, actual.Grade.Grade.Id);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetId_FinalGrade()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(_studentUnverifiedGradesSubmissions.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                expectedEntity.FinalGrade = "ProgCode1";
                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);

                Assert.AreEqual(_studentUnverifiedGradesSubmissions.Id, actual.Id);
                Assert.AreEqual(_studentUnverifiedGradesSubmissions.SectionRegistration.Id, actual.SectionRegistration.Id);
                Assert.AreEqual(_studentUnverifiedGradesSubmissions.Grade.Grade.Id, actual.Grade.Grade.Id);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetId_Midterm1()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(_studentUnverifiedGradesSubmissions.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                expectedEntity.FinalGrade = null;
                expectedEntity.MidtermGrade1 = "ProgCode1";
                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);

                Assert.AreEqual(_studentUnverifiedGradesSubmissions.Id, actual.Id);
                Assert.AreEqual(_studentUnverifiedGradesSubmissions.SectionRegistration.Id, actual.SectionRegistration.Id);
                Assert.AreEqual("progguid1", actual.Grade.Grade.Id);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetId_Midterm2()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(_studentUnverifiedGradesSubmissions.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                expectedEntity.FinalGrade = null; 
                expectedEntity.MidtermGrade2 = "ProgCode2";
                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);

                Assert.AreEqual(_studentUnverifiedGradesSubmissions.Id, actual.Id);
                Assert.AreEqual(_studentUnverifiedGradesSubmissions.SectionRegistration.Id, actual.SectionRegistration.Id);
                Assert.AreEqual("progguid2", actual.Grade.Grade.Id);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetId_Midterm3()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(_studentUnverifiedGradesSubmissions.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                expectedEntity.FinalGrade = null;
                expectedEntity.MidtermGrade2 = "ProgCode3";
                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);

                Assert.AreEqual(_studentUnverifiedGradesSubmissions.Id, actual.Id);
                Assert.AreEqual(_studentUnverifiedGradesSubmissions.SectionRegistration.Id, actual.SectionRegistration.Id);
                Assert.AreEqual("progguid3", actual.Grade.Grade.Id);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetId_Midterm4()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(_studentUnverifiedGradesSubmissions.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                expectedEntity.FinalGrade = null;
                expectedEntity.MidtermGrade2 = "ProgCode4";
                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);

                Assert.AreEqual(_studentUnverifiedGradesSubmissions.Id, actual.Id);
                Assert.AreEqual(_studentUnverifiedGradesSubmissions.SectionRegistration.Id, actual.SectionRegistration.Id);
                Assert.AreEqual("progguid4", actual.Grade.Grade.Id);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetId_Midterm5()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(_studentUnverifiedGradesSubmissions.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                expectedEntity.FinalGrade = null;
                expectedEntity.MidtermGrade2 = "ProgCode5";
                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);

                Assert.AreEqual(_studentUnverifiedGradesSubmissions.Id, actual.Id);
                Assert.AreEqual(_studentUnverifiedGradesSubmissions.SectionRegistration.Id, actual.SectionRegistration.Id);
                Assert.AreEqual("progguid5", actual.Grade.Grade.Id);
            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetId_Midterm6()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(_studentUnverifiedGradesSubmissions.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                expectedEntity.FinalGrade = null;
                expectedEntity.MidtermGrade2 = "ProgCode6";
                var actual = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);

                Assert.AreEqual(_studentUnverifiedGradesSubmissions.Id, actual.Id);
                Assert.AreEqual(_studentUnverifiedGradesSubmissions.SectionRegistration.Id, actual.SectionRegistration.Id);
                Assert.AreEqual("progguid6", actual.Grade.Grade.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentUnverifiedGradesService_GetId_Midterm7_invalid()
            {
                var expectedEntity = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                var expectedDto = _dtoStudentUnverifiedGradesList.FirstOrDefault(x => x.Id == StudentUnverifiedGradesGuid);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity.StudentCourseSecId);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentAcadCredGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(_studentUnverifiedGradesSubmissions.SectionRegistration.Id);
                _studentUnverifiedGradesRepositoryMock.Setup(x => x.GetStudentUnverifiedGradeByGuidAsync(It.IsAny<string>())).ReturnsAsync(expectedEntity);
                expectedEntity.FinalGrade = null;
                expectedEntity.MidtermGrade2 = "ProgCode7";
                await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(_studentUnverifiedGradesSubmissions.Id);
            }

            #endregion
        }
    }
}
 