//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentAptitudeAssessmentsServiceTests
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");
            protected Domain.Entities.Role updateTestScoresRole = new Domain.Entities.Role(1, "UPDATE.STUDENT.TEST.SCORES");
            protected Domain.Entities.Role deleteStudentAptitudeAssessmentsServiceRole = new Ellucian.Colleague.Domain.Entities.Role(1, "DELETE.STUDENT.TEST.SCORES");

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
                            Roles = new List<string>() { "Faculty", "DELETE.STUDENT.TEST.SCORES" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentAptitudeAssessmentsServiceTests_Get : CurrentUserSetup
        {
            private StudentAptitudeAssessmentsService _studentAptitudeAssessmentsService;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<ILogger> _loggerMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IStudentRepository> _studentRepositoryMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
            private Mock<IStudentTestScoresRepository> _studentAptitudeAssessmentRepositoryMock;
            private Mock<IAptitudeAssessmentsRepository> _aptitudeAssessmentsRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private const string studentAptitudeAssessmentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string studentAptitudeAssessmentsCode = "ACT";
            private ICollection<StudentTestScores> _studentNonCoursesCollection;
            private List<StudentAptitudeAssessments> _studentAptitudeAssessmentsDtos;
            private Tuple<IEnumerable<StudentAptitudeAssessments>, int> _studentAptitudeAssessmentsDtoTuple;

            private List<NonCourse> _aptitudeAssementEntities;
            private IEnumerable<Domain.Student.Entities.AssessmentSpecialCircumstance> _assessmentSpecialCircumstances;
            private IEnumerable<IntgTestPercentileType> _assesmentPercentileTypes;
            private IEnumerable<TestSource> _testSource;



            int offset = 0;
            int limit = 200;

            private Domain.Entities.Permission permissionViewAnyStudentAptitudeAssessment;
            private Domain.Entities.Permission permissionDeleteAnyStudentAptitudeAssessment;


            [TestInitialize]
            public void Initialize()
            {
                _personRepositoryMock = new Mock<IPersonRepository>();
                _studentRepositoryMock = new Mock<IStudentRepository>();
                _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _studentAptitudeAssessmentRepositoryMock = new Mock<IStudentTestScoresRepository>();
                _aptitudeAssessmentsRepositoryMock = new Mock<IAptitudeAssessmentsRepository>();
                _loggerMock = new Mock<ILogger>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                currentUserFactory = currentUserFactoryMock.Object;
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                BuildMocks();

                // Mock permissions
                permissionViewAnyStudentAptitudeAssessment = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAptitudeAssessmentsConsent);
                permissionDeleteAnyStudentAptitudeAssessment = new Domain.Entities.Permission(StudentPermissionCodes.DeleteStudentAptitudeAssessmentsConsent);
                personRole.AddPermission(permissionViewAnyStudentAptitudeAssessment);
                deleteStudentAptitudeAssessmentsServiceRole.AddPermission(permissionDeleteAnyStudentAptitudeAssessment);

                _studentAptitudeAssessmentsService = new StudentAptitudeAssessmentsService(
                    _studentAptitudeAssessmentRepositoryMock.Object, _personRepositoryMock.Object,
                    _studentReferenceRepositoryMock.Object, _aptitudeAssessmentsRepositoryMock.Object,
                    adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object,
                    _studentRepositoryMock.Object, _loggerMock.Object, baseConfigurationRepository);
            }
            private void BuildData()
            {
                #region dto

                _studentAptitudeAssessmentsDtos = new List<StudentAptitudeAssessments>()
                {
                    new Dtos.StudentAptitudeAssessments()
                    {
                        Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                        AssessedOn = new DateTimeOffset(DateTime.Today),
                        Assessment = new GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                        Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "1" },
                        Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Value = 79 }, new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("7b8c4ba7-ea28-4604-bca7-da7223f6e2b3"), Value = 33 } },
                        Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.Primary,
                        Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                        Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 200 },
                        Source = new GuidObject2("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31"),
                        SpecialCircumstances = new List<GuidObject2>() {  new GuidObject2("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09"), new GuidObject2("5eed2bea-8948-439b-b5c5-779d84724a38") },
                        Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active,
                        Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Original
                    },
                    new Dtos.StudentAptitudeAssessments()
                    {
                        Id = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                        AssessedOn = new DateTimeOffset(DateTime.Today),
                        Assessment = new GuidObject2("7f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "2" },
                        Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Value = 79 } },
                        Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.Primary,
                        Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                        Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 190 },
                        Source = new GuidObject2("7e990bda-9427-4de6-b0ef-bba9b015e399"),
                        SpecialCircumstances = new List<GuidObject2>() { new GuidObject2("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09"), new GuidObject2("5eed2bea-8948-439b-b5c5-779d84724a38") },
                        Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Inactive,
                        Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Revised
                    },
                    new Dtos.StudentAptitudeAssessments()
                    {
                        Id = "d2253ac7-9931-4560-b42f-1fccd43c952e",
                        AssessedOn = new DateTimeOffset(DateTime.Today),
                        Assessment = new GuidObject2("8f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "3" },
                        Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Value = 79 } },
                        Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.NotSet,
                        Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                        Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 190 },
                        Source = new GuidObject2("7e990bda-9427-4de6-b0ef-bba9b015e399"),
                        SpecialCircumstances = new List<GuidObject2>() { new GuidObject2("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09"), new GuidObject2("5eed2bea-8948-439b-b5c5-779d84724a38") },
                        Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active,
                        Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Recentered
                    }
                };
                _studentAptitudeAssessmentsDtoTuple = new Tuple<IEnumerable<StudentAptitudeAssessments>, int>(_studentAptitudeAssessmentsDtos, _studentAptitudeAssessmentsDtos.Count());

                #endregion

                _studentNonCoursesCollection = new List<StudentTestScores>()
                {
                    new StudentTestScores("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "0003784", "ACT", "ACT Test", DateTime.Today)
                    {
                        FormName = "ACT",
                        FormNo = "1",
                        Percentile1 = 79,
                        Percentile2 = 33,
                        Score = 200,
                        Source = "ACT",
                        SpecialFactors = new List<string>() { "A", "D" },
                        StatusCode = "A",
                        StatusCodeSpProcessing = "2",
                        StatusDate = new DateTime(2017, 12, 11)
                    },
                    new StudentTestScores("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "0003784", "SAT", "SAT Test", DateTime.Today)
                    {
                        FormName = "SAT",
                        FormNo = "494",
                        Percentile1 = 79,
                        Score = 1200,
                        Source = "SAT",
                        SpecialFactors = new List<string>() { "A", "D" },
                        StatusCode = "A",
                        StatusCodeSpProcessing = "3",
                        StatusDate = new DateTime(2017, 12, 11)
                    },
                    new StudentTestScores("d2253ac7-9931-4560-b42f-1fccd43c952e", "0003784", "ACT.M", "ACT Math", DateTime.Today)
                    {
                        FormName = "ACT.M",
                        FormNo = "700",
                        Percentile1 = 79,
                        Score = 1200,
                        Source = "SAT",
                        SpecialFactors = new List<string>() { "A", "D" },
                        StatusCode = "A",
                        StatusCodeSpProcessing = "1",
                        StatusDate = new DateTime(2017, 12, 11)
                    }
                };

                _aptitudeAssementEntities = new List<NonCourse>()
                {
                   new NonCourse("b9691210-8516-45ca-9cd1-7e5aa1777234", "ACT"),
                   new NonCourse("7f3aac22-e0b5-4159-b4e2-da158362c41b", "SAT"),
                   new NonCourse("8f3aac22-e0b5-4159-b4e2-da158362c41b", "ACT.M")
                };

                _assessmentSpecialCircumstances = new List<Domain.Student.Entities.AssessmentSpecialCircumstance>()
                {
                    new Domain.Student.Entities.AssessmentSpecialCircumstance("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "A", "title1"),
                    new Domain.Student.Entities.AssessmentSpecialCircumstance("bd54668d-50d9-416c-81e9-2318e88571a1", "B", "title2"),
                    new Domain.Student.Entities.AssessmentSpecialCircumstance("5eed2bea-8948-439b-b5c5-779d84724a38", "D", "title3")
                };

                _assesmentPercentileTypes = new List<IntgTestPercentileType>()
                {
                    new IntgTestPercentileType("3b8f02a3-d349-46b5-a0df-710121fa1f64", "1", "Title 1"),
                    new IntgTestPercentileType("7b8c4ba7-ea28-4604-bca7-da7223f6e2b3", "2", "Title 2")
                };

                _testSource = new List<TestSource>()
                {
                    new TestSource("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31", "ACT", "Title 1"),
                    new TestSource("7e990bda-9427-4de6-b0ef-bba9b015e399", "SAT", "Title 2")
                };
            }

            private void BuildMocks()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });
                 roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    deleteStudentAptitudeAssessmentsServiceRole
                });
                //roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { deleteStudentAptitudeAssessmentsServiceRole });
               

                var studentAptitudeAssessmentTuple = new Tuple<IEnumerable<StudentTestScores>, int>(_studentNonCoursesCollection, 3);
                _studentAptitudeAssessmentRepositoryMock.Setup(rp => rp.GetStudentTestScoresAsync("", offset, limit, It.IsAny<bool>()))
                    .ReturnsAsync(studentAptitudeAssessmentTuple);
                _studentAptitudeAssessmentRepositoryMock.Setup(rp => rp.GetStudentTestScoresByGuidAsync(It.IsAny<string>())).ReturnsAsync(_studentNonCoursesCollection.FirstOrDefault(c => c.Guid == studentAptitudeAssessmentsGuid));

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("0003784")).ReturnsAsync("1df164eb-8178-4321-a9f7-24f27f3991d8");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1df164eb-8178-4321-a9f7-24f27f3991d8");

                _aptitudeAssessmentsRepositoryMock.Setup(i => i.GetAptitudeAssessmentsAsync(It.IsAny<bool>())).ReturnsAsync(_aptitudeAssementEntities);
                _studentReferenceRepositoryMock.Setup(i => i.GetAssessmentSpecialCircumstancesAsync(It.IsAny<bool>())).ReturnsAsync(_assessmentSpecialCircumstances);
                _studentReferenceRepositoryMock.Setup(i => i.GetIntgTestPercentileTypesAsync(It.IsAny<bool>())).ReturnsAsync(_assesmentPercentileTypes);
                _studentReferenceRepositoryMock.Setup(i => i.GetTestSourcesAsync(It.IsAny<bool>())).ReturnsAsync(_testSource);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personRepositoryMock = null;
                _studentRepositoryMock = null;
                _studentReferenceRepositoryMock = null;
                _studentAptitudeAssessmentRepositoryMock = null;
                _aptitudeAssessmentsRepositoryMock = null;
                _loggerMock = null;
                roleRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;

                _studentAptitudeAssessmentsService = null;
                _studentNonCoursesCollection = null;
            }

            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_GetStudentAptitudeAssessmentsAsync()
            {
                var pageOfItems = await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsAsync(offset, limit, true);
                var results = pageOfItems.Item1;
                Assert.IsTrue(results is IEnumerable<StudentAptitudeAssessments>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_GetStudentAptitudeAssessmentsAsync_Count()
            {
                var pageOfItems = await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsAsync(offset, limit, true);
                var results = pageOfItems.Item1;
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_GetStudentAptitudeAssessmentsAsync_Properties()
            {
                var result =
                    (await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsAsync(offset, limit, true)).Item1.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                Assert.IsNotNull(result.Id);
            }

            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_GetStudentAptitudeAssessmentsAsync_Expected()
            {
                var expectedResults = _studentNonCoursesCollection.FirstOrDefault(c => c.Guid == studentAptitudeAssessmentsGuid);
                var actualResult =
                    (await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsAsync(offset, limit, true)).Item1.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                Assert.AreEqual(expectedResults.Guid, actualResult.Id);

            }

            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_DeleteStudentAptitudeAssessmentAsync_Response()
            {
                await _studentAptitudeAssessmentsService.DeleteStudentAptitudeAssessmentAsync("48285630-3f7f-4b16-b73d-1b1a4bdd27ee");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAptitudeAssessmentsService_DeleteStudentAptitudeAssessmentAsync_ArgumentNullException()
            {
                await _studentAptitudeAssessmentsService.DeleteStudentAptitudeAssessmentAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAptitudeAssessmentsService_GetStudentAptitudeAssessmentsByGuidAsync_Empty()
            {
                await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAptitudeAssessmentsService_GetStudentAptitudeAssessmentsByGuidAsync_Null()
            {
                await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAptitudeAssessmentsService_GetStudentAptitudeAssessmentsByGuidAsync_InvalidId()
            {
                _studentAptitudeAssessmentRepositoryMock.Setup(rp => rp.GetStudentTestScoresByGuidAsync("ABC")).ReturnsAsync(null);
                await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuidAsync("ABC");
            }

            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_GetStudentAptitudeAssessmentsByGuidAsync_Expected()
            {
                var expectedResults =
                    _studentNonCoursesCollection.First(c => c.Guid == studentAptitudeAssessmentsGuid);
                var actualResult =
                    await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuidAsync(studentAptitudeAssessmentsGuid);
                Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            }

            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_GetStudentAptitudeAssessmentsByGuidAsync_Properties()
            {
                var expected = _studentAptitudeAssessmentsDtos.First(nc => nc.Id == studentAptitudeAssessmentsGuid);
                var result =
                    await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuidAsync(studentAptitudeAssessmentsGuid);
                Assert.IsNotNull(result.Id);
                Assert.AreEqual(result.AssessedOn, expected.AssessedOn);
                Assert.AreEqual(result.Assessment.Id, expected.Assessment.Id);
                Assert.AreEqual(result.Form.Name, expected.Form.Name);
                Assert.AreEqual(result.Form.Number, expected.Form.Number);
                Assert.AreEqual(result.Percentile.Count, expected.Percentile.Count);
                var idx = 0;
                foreach (var percentile in result.Percentile)
                {
                    Assert.AreEqual(percentile.Type.Id, expected.Percentile[idx].Type.Id);
                    Assert.AreEqual(percentile.Value, expected.Percentile[idx].Value);
                    idx += 1;
                }
                Assert.AreEqual(result.Preference, expected.Preference);
                Assert.AreEqual(result.Reported, expected.Reported);
                Assert.AreEqual(result.Score.Type, expected.Score.Type);
                Assert.AreEqual(result.Score.Value, expected.Score.Value);
                Assert.AreEqual(result.Source.Id, expected.Source.Id);
                Assert.AreEqual(result.SpecialCircumstances.Count, expected.SpecialCircumstances.Count);
                var ctr = 0;
                foreach (var special in result.SpecialCircumstances)
                {
                    Assert.AreEqual(special.Id, expected.SpecialCircumstances[ctr].Id);
                    ctr += 1;
                }
                Assert.AreEqual(result.Status, expected.Status);
                Assert.AreEqual(result.Student.Id, expected.Student.Id);
            }
        }

        [TestClass]
        public class StudentAptitudeAssessmentsServiceTests_PutPost : CurrentUserSetup
        {
            private StudentAptitudeAssessmentsService _studentAptitudeAssessmentsService;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<ILogger> _loggerMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IStudentRepository> _studentRepositoryMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
            private Mock<IStudentTestScoresRepository> _studentAptitudeAssessmentRepositoryMock;
            private Mock<IAptitudeAssessmentsRepository> _aptitudeAssessmentsRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private const string studentAptitudeAssessmentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string studentAptitudeAssessmentsCode = "ACT";
            private ICollection<StudentTestScores> _studentNonCoursesCollection;
            private List<StudentAptitudeAssessments> _studentAptitudeAssessmentsDtos;
            private Tuple<IEnumerable<StudentAptitudeAssessments>, int> _studentAptitudeAssessmentsDtoTuple;

            private List<NonCourse> _aptitudeAssementEntities;
            private IEnumerable<Domain.Student.Entities.AssessmentSpecialCircumstance> _assessmentSpecialCircumstances;
            private IEnumerable<IntgTestPercentileType> _assesmentPercentileTypes;
            private IEnumerable<TestSource> _testSource;

            int offset = 0;
            int limit = 200;

            private Domain.Entities.Permission permissionUpdateAnyStudentAptitudeAssessment;

            [TestInitialize]
            public void Initialize()
            {
                _personRepositoryMock = new Mock<IPersonRepository>();
                _studentRepositoryMock = new Mock<IStudentRepository>();
                _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _studentAptitudeAssessmentRepositoryMock = new Mock<IStudentTestScoresRepository>();
                _aptitudeAssessmentsRepositoryMock = new Mock<IAptitudeAssessmentsRepository>();
                _loggerMock = new Mock<ILogger>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                currentUserFactory = currentUserFactoryMock.Object;
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                BuildMocks();

                // Mock permissions
                permissionUpdateAnyStudentAptitudeAssessment = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentAptitudeAssessmentsConsent);
                personRole.AddPermission(permissionUpdateAnyStudentAptitudeAssessment);

                _studentAptitudeAssessmentsService = new StudentAptitudeAssessmentsService(
                    _studentAptitudeAssessmentRepositoryMock.Object, _personRepositoryMock.Object,
                    _studentReferenceRepositoryMock.Object, _aptitudeAssessmentsRepositoryMock.Object,
                    adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object,
                    _studentRepositoryMock.Object, _loggerMock.Object, baseConfigurationRepository);
            }
            private void BuildData()
            {
                #region dto

                _studentAptitudeAssessmentsDtos = new List<StudentAptitudeAssessments>()
                {
                    new Dtos.StudentAptitudeAssessments()
                    {

                        Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                        AssessedOn = new DateTimeOffset(DateTime.Today),
                        Assessment = new GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                        Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "1" },
                        Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Value = 79 }, new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("7b8c4ba7-ea28-4604-bca7-da7223f6e2b3"), Value = 33 } },
                        Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.Primary,
                        Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                        Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 200 },
                        Source = new GuidObject2("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31"),
                        SpecialCircumstances = new List<GuidObject2>() {  new GuidObject2("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09"), new GuidObject2("5eed2bea-8948-439b-b5c5-779d84724a38") },
                        Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active,
                        Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Original
                    },
                    new Dtos.StudentAptitudeAssessments()
                    {
                        Id = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                        AssessedOn = new DateTimeOffset(DateTime.Today),
                        Assessment = new GuidObject2("7f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "2" },
                        Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Value = 79 } },
                        Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.Primary,
                        Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                        Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 190 },
                        Source = new GuidObject2("7e990bda-9427-4de6-b0ef-bba9b015e399"),
                        SpecialCircumstances = new List<GuidObject2>() { new GuidObject2("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09"), new GuidObject2("5eed2bea-8948-439b-b5c5-779d84724a38") },
                        Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Inactive,
                        Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Revised
                    },
                    new Dtos.StudentAptitudeAssessments()
                    {
                        Id = "d2253ac7-9931-4560-b42f-1fccd43c952e",
                        AssessedOn = new DateTimeOffset(DateTime.Today),
                        Assessment = new GuidObject2("8f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "3" },
                        Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("3b8f02a3-d349-46b5-a0df-710121fa1f64"), Value = 79 } },
                        Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.NotSet,
                        Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                        Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 190 },
                        Source = new GuidObject2("7e990bda-9427-4de6-b0ef-bba9b015e399"),
                        SpecialCircumstances = new List<GuidObject2>() { new GuidObject2("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09"), new GuidObject2("5eed2bea-8948-439b-b5c5-779d84724a38") },
                        Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active,
                        Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Recentered
                    }
                };
                _studentAptitudeAssessmentsDtoTuple = new Tuple<IEnumerable<StudentAptitudeAssessments>, int>(_studentAptitudeAssessmentsDtos, _studentAptitudeAssessmentsDtos.Count());

                #endregion

                _studentNonCoursesCollection = new List<StudentTestScores>()
                {
                    new StudentTestScores("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "0003784", "ACT", "ACT Test", DateTime.Today)
                    {

                        FormName = "ACT",
                        FormNo = "1",
                        Percentile1 = 79,
                        Percentile2 = 33,
                        Score = 200,
                        Source = "ACT",
                        SpecialFactors = new List<string>() { "A", "D" },
                        StatusCode = "A",
                        StatusCodeSpProcessing = "2",
                        StatusDate = new DateTime(2017, 12, 11)
                    },
                    new StudentTestScores("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2", "0003784", "SAT", "SAT Test", DateTime.Today)
                    {
                        FormName = "SAT",
                        FormNo = "494",
                        Percentile1 = 79,
                        Score = 1200,
                        Source = "SAT",
                        SpecialFactors = new List<string>() { "A", "D" },
                        StatusCode = "A",
                        StatusCodeSpProcessing = "3",
                        StatusDate = new DateTime(2017, 12, 11)
                    },
                    new StudentTestScores("d2253ac7-9931-4560-b42f-1fccd43c952e", "3", "0003784", "ACT.M", "ACT Math", DateTime.Today)
                    {
                        FormName = "ACT.M",
                        FormNo = "700",
                        Percentile1 = 79,
                        Score = 1200,
                        Source = "SAT",
                        SpecialFactors = new List<string>() { "A", "D" },
                        StatusCode = "A",
                        StatusCodeSpProcessing = "1",
                        StatusDate = new DateTime(2017, 12, 11)
                    }
                };

                _aptitudeAssementEntities = new List<NonCourse>()
                {
                   new NonCourse("b9691210-8516-45ca-9cd1-7e5aa1777234", "ACT"),
                   new NonCourse("7f3aac22-e0b5-4159-b4e2-da158362c41b", "SAT"),
                   new NonCourse("8f3aac22-e0b5-4159-b4e2-da158362c41b", "ACT.M")
                };

                _assessmentSpecialCircumstances = new List<Domain.Student.Entities.AssessmentSpecialCircumstance>()
                {
                    new Domain.Student.Entities.AssessmentSpecialCircumstance("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "A", "title1"),
                    new Domain.Student.Entities.AssessmentSpecialCircumstance("bd54668d-50d9-416c-81e9-2318e88571a1", "B", "title2"),
                    new Domain.Student.Entities.AssessmentSpecialCircumstance("5eed2bea-8948-439b-b5c5-779d84724a38", "D", "title3")
                };

                _assesmentPercentileTypes = new List<IntgTestPercentileType>()
                {
                    new IntgTestPercentileType("3b8f02a3-d349-46b5-a0df-710121fa1f64", "1", "Title 1"),
                    new IntgTestPercentileType("7b8c4ba7-ea28-4604-bca7-da7223f6e2b3", "2", "Title 2")
                };

                _testSource = new List<TestSource>()
                {
                    new TestSource("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31", "ACT", "Title 1"),
                    new TestSource("7e990bda-9427-4de6-b0ef-bba9b015e399", "SAT", "Title 2")
                };
            }

            private void BuildMocks()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                var studentAptitudeAssessmentTuple = new Tuple<IEnumerable<StudentTestScores>, int>(_studentNonCoursesCollection, 3);
                _studentAptitudeAssessmentRepositoryMock.Setup(rp => rp.GetStudentTestScoresAsync("", offset, limit, It.IsAny<bool>()))
                    .ReturnsAsync(studentAptitudeAssessmentTuple);
                _studentAptitudeAssessmentRepositoryMock.Setup(rp => rp.GetStudentTestScoresByGuidAsync(It.IsAny<string>())).ReturnsAsync(_studentNonCoursesCollection.FirstOrDefault(c => c.Guid == studentAptitudeAssessmentsGuid));

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("0003784")).ReturnsAsync("1df164eb-8178-4321-a9f7-24f27f3991d8");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1df164eb-8178-4321-a9f7-24f27f3991d8");

                _aptitudeAssessmentsRepositoryMock.Setup(i => i.GetAptitudeAssessmentsAsync(It.IsAny<bool>())).ReturnsAsync(_aptitudeAssementEntities);
                _studentReferenceRepositoryMock.Setup(i => i.GetAssessmentSpecialCircumstancesAsync(It.IsAny<bool>())).ReturnsAsync(_assessmentSpecialCircumstances);
                _studentReferenceRepositoryMock.Setup(i => i.GetIntgTestPercentileTypesAsync(It.IsAny<bool>())).ReturnsAsync(_assesmentPercentileTypes);
                _studentReferenceRepositoryMock.Setup(i => i.GetTestSourcesAsync(It.IsAny<bool>())).ReturnsAsync(_testSource);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personRepositoryMock = null;
                _studentRepositoryMock = null;
                _studentReferenceRepositoryMock = null;
                _studentAptitudeAssessmentRepositoryMock = null;
                _aptitudeAssessmentsRepositoryMock = null;
                _loggerMock = null;
                roleRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;

                _studentAptitudeAssessmentsService = null;
                _studentNonCoursesCollection = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAptitudeAssessmentsService_CreateStudentAptitudeAssessments_ArgumentNullException()
            {
                await _studentAptitudeAssessmentsService.CreateStudentAptitudeAssessmentsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAptitudeAssessmentsService_Post_PopulateStatus()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.RecordKey == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.CreateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active;

                await _studentAptitudeAssessmentsService.CreateStudentAptitudeAssessmentsAsync(null);
            }

            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_Post()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.Guid == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.CreateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);

                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                var result = await _studentAptitudeAssessmentsService.CreateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);

                Assert.IsNotNull(result.Id);
                Assert.AreEqual(result.AssessedOn, studentAptitudeAssessment.AssessedOn);
                Assert.AreEqual(result.Assessment.Id, studentAptitudeAssessment.Assessment.Id);
                Assert.AreEqual(result.Form.Name, studentAptitudeAssessment.Form.Name);
                Assert.AreEqual(result.Form.Number, studentAptitudeAssessment.Form.Number);
                Assert.AreEqual(result.Percentile.Count, studentAptitudeAssessment.Percentile.Count);
                var idx = 0;
                foreach (var percentile in result.Percentile)
                {
                    Assert.AreEqual(percentile.Type.Id, studentAptitudeAssessment.Percentile[idx].Type.Id);
                    Assert.AreEqual(percentile.Value, studentAptitudeAssessment.Percentile[idx].Value);
                    idx += 1;
                }
                Assert.AreEqual(result.Preference, studentAptitudeAssessment.Preference);
                Assert.AreEqual(result.Reported, studentAptitudeAssessment.Reported);
                Assert.AreEqual(result.Score.Type, studentAptitudeAssessment.Score.Type);
                Assert.AreEqual(result.Score.Value, studentAptitudeAssessment.Score.Value);
                Assert.AreEqual(result.Source.Id, studentAptitudeAssessment.Source.Id);
                Assert.AreEqual(result.SpecialCircumstances.Count, studentAptitudeAssessment.SpecialCircumstances.Count);
                var ctr = 0;
                foreach (var special in result.SpecialCircumstances)
                {
                    Assert.AreEqual(special.Id, studentAptitudeAssessment.SpecialCircumstances[ctr].Id);
                    ctr += 1;
                }
                //Assert.AreEqual(result.Status, studentAptitudeAssessment.Status);
                Assert.AreEqual(result.Student.Id, studentAptitudeAssessment.Student.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAptitudeAssessmentsService_Post_NullId()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.RecordKey == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.CreateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);

                studentAptitudeAssessment.Id = null;

                await _studentAptitudeAssessmentsService.CreateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAptitudeAssessmentsService_Put_PopulateStatus()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.RecordKey == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active;
                await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAptitudeAssessmentsService_Put_NullId()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.RecordKey == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active;
                studentAptitudeAssessment.Id = null;
                await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAptitudeAssessmentsService_Put_RepositoryException()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.RecordKey == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ThrowsAsync(new RepositoryException());
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.CreateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ThrowsAsync(new KeyNotFoundException());
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAptitudeAssessmentsService_Put_KeyNotFoundException()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.RecordKey == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ThrowsAsync(new KeyNotFoundException());
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.CreateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ThrowsAsync(new KeyNotFoundException());
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAptitudeAssessmentsService_Put_ArgumentException()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.RecordKey == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ThrowsAsync(new ArgumentException());
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.CreateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ThrowsAsync(new KeyNotFoundException());
                await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAptitudeAssessmentsService_Put_Exception()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.RecordKey == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ThrowsAsync(new Exception());
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.CreateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ThrowsAsync(new KeyNotFoundException());
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAptitudeAssessmentsService_Put_NullArguments()
            {
                await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAptitudeAssessmentsService_Put_EmptyStudent()
            {

                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.Guid == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);

                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.GetStudentTestScoresIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(studentTestScore.RecordKey);

                studentAptitudeAssessment.Student = new GuidObject2();
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                var result = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }          

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAptitudeAssessmentsService_Put_EmptyAssessment()
            {

                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.Guid == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);

                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.GetStudentTestScoresIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(studentTestScore.RecordKey);

                studentAptitudeAssessment.Assessment = new GuidObject2();
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                var result = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAptitudeAssessmentsService_Put_InvalidAssessment()
            {

                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.Guid == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);

                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.GetStudentTestScoresIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(studentTestScore.RecordKey);

                studentAptitudeAssessment.Assessment = new GuidObject2("INVALID");
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                var result = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAptitudeAssessmentsService_Put_EmptyAssessedOn()
            {

                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.Guid == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);

                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.GetStudentTestScoresIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(studentTestScore.RecordKey);

                studentAptitudeAssessment.AssessedOn = null;
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                var result = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAptitudeAssessmentsService_Put_EmptyScore()
            {

                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.Guid == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);

                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.GetStudentTestScoresIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(studentTestScore.RecordKey);

                studentAptitudeAssessment.Score = new StudentAptitudeAssessmentsScore();
                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                var result = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

           
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAptitudeAssessmentsService_Put_InvalidSource()
            {

                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.Guid == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);

                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.GetStudentTestScoresIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(studentTestScore.RecordKey);

                studentAptitudeAssessment.Source = new GuidObject2("INVALID");
                var result = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);
            }

            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_Put()
            {

                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.Guid == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.UpdateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);

                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.GetStudentTestScoresIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(studentTestScore.RecordKey);

                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                var result = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);

                Assert.IsNotNull(result.Id);
                Assert.AreEqual(result.AssessedOn, studentAptitudeAssessment.AssessedOn);
                Assert.AreEqual(result.Assessment.Id, studentAptitudeAssessment.Assessment.Id);
                Assert.AreEqual(result.Form.Name, studentAptitudeAssessment.Form.Name);
                Assert.AreEqual(result.Form.Number, studentAptitudeAssessment.Form.Number);
                Assert.AreEqual(result.Percentile.Count, studentAptitudeAssessment.Percentile.Count);
                var idx = 0;
                foreach (var percentile in result.Percentile)
                {
                    Assert.AreEqual(percentile.Type.Id, studentAptitudeAssessment.Percentile[idx].Type.Id);
                    Assert.AreEqual(percentile.Value, studentAptitudeAssessment.Percentile[idx].Value);
                    idx += 1;
                }
                Assert.AreEqual(result.Preference, studentAptitudeAssessment.Preference);
                Assert.AreEqual(result.Reported, studentAptitudeAssessment.Reported);
                Assert.AreEqual(result.Score.Type, studentAptitudeAssessment.Score.Type);
                Assert.AreEqual(result.Score.Value, studentAptitudeAssessment.Score.Value);
                Assert.AreEqual(result.Source.Id, studentAptitudeAssessment.Source.Id);
                Assert.AreEqual(result.SpecialCircumstances.Count, studentAptitudeAssessment.SpecialCircumstances.Count);
                var ctr = 0;
                foreach (var special in result.SpecialCircumstances)
                {
                    Assert.AreEqual(special.Id, studentAptitudeAssessment.SpecialCircumstances[ctr].Id);
                    ctr += 1;
                }
                
                Assert.AreEqual(result.Student.Id, studentAptitudeAssessment.Student.Id);
            }


            [TestMethod]
            public async Task StudentAptitudeAssessmentsService_Put_NoRecordKey()
            {
                var studentAptitudeAssessment = _studentAptitudeAssessmentsDtos.FirstOrDefault(x => x.Id == studentAptitudeAssessmentsGuid);
                var studentTestScore = _studentNonCoursesCollection.FirstOrDefault(x => x.Guid == studentAptitudeAssessmentsGuid);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.CreateStudentTestScoresAsync(It.IsAny<StudentTestScores>())).ReturnsAsync(studentTestScore);
                _studentAptitudeAssessmentRepositoryMock.Setup(x => x.GetStudentTestScoresIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

                studentAptitudeAssessment.Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.NotSet;
                var result = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(studentAptitudeAssessment);

                Assert.IsNotNull(result.Id);
                Assert.AreEqual(result.AssessedOn, studentAptitudeAssessment.AssessedOn);
                Assert.AreEqual(result.Assessment.Id, studentAptitudeAssessment.Assessment.Id);
                Assert.AreEqual(result.Form.Name, studentAptitudeAssessment.Form.Name);
                Assert.AreEqual(result.Form.Number, studentAptitudeAssessment.Form.Number);
                Assert.AreEqual(result.Percentile.Count, studentAptitudeAssessment.Percentile.Count);
                var idx = 0;
                foreach (var percentile in result.Percentile)
                {
                    Assert.AreEqual(percentile.Type.Id, studentAptitudeAssessment.Percentile[idx].Type.Id);
                    Assert.AreEqual(percentile.Value, studentAptitudeAssessment.Percentile[idx].Value);
                    idx += 1;
                }
                Assert.AreEqual(result.Preference, studentAptitudeAssessment.Preference);
                Assert.AreEqual(result.Reported, studentAptitudeAssessment.Reported);
                Assert.AreEqual(result.Score.Type, studentAptitudeAssessment.Score.Type);
                Assert.AreEqual(result.Score.Value, studentAptitudeAssessment.Score.Value);
                Assert.AreEqual(result.Source.Id, studentAptitudeAssessment.Source.Id);
                Assert.AreEqual(result.SpecialCircumstances.Count, studentAptitudeAssessment.SpecialCircumstances.Count);
                var ctr = 0;
                foreach (var special in result.SpecialCircumstances)
                {
                    Assert.AreEqual(special.Id, studentAptitudeAssessment.SpecialCircumstances[ctr].Id);
                    ctr += 1;
                }
                
                Assert.AreEqual(result.Student.Id, studentAptitudeAssessment.Student.Id);
            }
        }
    }
}
