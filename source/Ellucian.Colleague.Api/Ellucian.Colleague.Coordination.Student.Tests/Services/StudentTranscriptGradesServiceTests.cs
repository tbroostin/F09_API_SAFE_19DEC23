//Copyright 2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Colleague.Domain.Student;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.Exceptions;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentTranscriptGradesServiceTests : StudentUserFactory
    {
        protected Ellucian.Colleague.Domain.Entities.Role viewStudentTranscriptGrades = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.TRANSCRIPT.GRADES");

        private const string studentTranscriptGradesGuid = "55afd535-2101-40c7-9ac2-70a10721db26";
  
        private List<Domain.Student.Entities.StudentTranscriptGrades> _studentTranscriptGradesCollection;
        private List<Dtos.StudentTranscriptGradesAdjustments> _studentTranscriptGradesAdjustmentsCollection;
        private Tuple<IEnumerable<StudentTranscriptGrades>, int> studentTranscriptGradesTuple;
        private StudentTranscriptGradesService _studentTranscriptGradesService;
        private Ellucian.Colleague.Domain.Entities.Permission perm = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentTranscriptGrades);
        private Ellucian.Colleague.Domain.Entities.Permission updatePerm = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments);

        private Mock<IStudentTranscriptGradesRepository> _studentTranscriptGradesRepositoryMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IGradeRepository> _gradeRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        private ICollection<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> _gradeSchemeCollection;
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.Grade> _gradeCollection;
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.CreditCategory> _creditCategoriesCollection;
        private ICollection<Ellucian.Colleague.Domain.Base.Entities.GradeChangeReason> _gradeChangeReasonsCollection;

        private string personGuid, courseSectionGuid, studentCourseSecGuid, courseGuid;

        [TestInitialize]
        public void Initialize()
        {
            _studentTranscriptGradesRepositoryMock = new Mock<IStudentTranscriptGradesRepository>();
            _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _gradeRepositoryMock = new Mock<IGradeRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();

            _currentUserFactory = new StudentTranscriptGradesUser();

            viewStudentTranscriptGrades.AddPermission(perm);
            viewStudentTranscriptGrades.AddPermission(updatePerm);
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentTranscriptGrades });

            BuildData();

            BuildMocks();

            _studentTranscriptGradesService = new StudentTranscriptGradesService(_studentTranscriptGradesRepositoryMock.Object,
                _referenceRepositoryMock.Object, _studentReferenceRepositoryMock.Object, _personRepositoryMock.Object, _gradeRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactory, _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        private void BuildMocks()
        {

            personGuid = Guid.NewGuid().ToString();
            studentCourseSecGuid = Guid.NewGuid().ToString();
            courseGuid = Guid.NewGuid().ToString();
            courseSectionGuid = Guid.NewGuid().ToString();

            _roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
            {
                viewStudentTranscriptGrades//new Domain.Entities.Role(1, "VIEW.STUDENT.COURSE.TRANSFERS")
            });
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentTranscriptGradesTuple);
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>()))
                .ReturnsAsync(_studentTranscriptGradesCollection[0]);
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesIdFromGuidAsync(It.IsAny<string>()))
                .ReturnsAsync("1");
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.UpdateStudentTranscriptGradesAdjustmentsAsync(It.IsAny<StudentTranscriptGradesAdjustments>()))
                .ReturnsAsync(_studentTranscriptGradesCollection[0]);

            _gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(_gradeCollection);

            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(_gradeSchemeCollection);

            _studentReferenceRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(_creditCategoriesCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetGradeChangeReasonAsync(It.IsAny<bool>())).ReturnsAsync(_gradeChangeReasonsCollection);

            var personGuidDictionary = new Dictionary<string, string>() { };
            personGuidDictionary.Add("0000111", personGuid);
            _personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(personGuidDictionary);

            var studentCourseSecGuidDictionary = new Dictionary<string, string>() { };
            studentCourseSecGuidDictionary.Add("0000005", studentCourseSecGuid);
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetGuidsCollectionAsync(It.IsAny<List<string>>(), "STUDENT.COURSE.SEC"))
                .ReturnsAsync(studentCourseSecGuidDictionary);

            var courseGuidDictionary = new Dictionary<string, string>() { };
            courseGuidDictionary.Add("MATH-101", courseGuid);
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetGuidsCollectionAsync(It.IsAny<List<string>>(), "COURSES"))
                .ReturnsAsync(courseGuidDictionary);

            var courseSectionGuidDictionary = new Dictionary<string, string>() { };
            courseSectionGuidDictionary.Add("1", courseSectionGuid);
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetGuidsCollectionAsync(It.IsAny<List<string>>(), "COURSE.SECTIONS"))
                .ReturnsAsync(courseSectionGuidDictionary);

        }

        private async void BuildData()
        {
            _gradeSchemeCollection = (await new TestStudentReferenceDataRepository().GetGradeSchemesAsync()).ToList();
            _gradeCollection = (await new TestGradeRepository().GetHedmAsync()).ToList();
            _creditCategoriesCollection = (await new TestStudentReferenceDataRepository().GetCreditCategoriesAsync()).ToList();
            _gradeChangeReasonsCollection = new List<Ellucian.Colleague.Domain.Base.Entities.GradeChangeReason>()
            {
                new Domain.Base.Entities.GradeChangeReason(Guid.NewGuid().ToString(), "IC", "Instructor Consent"),
                new Domain.Base.Entities.GradeChangeReason(Guid.NewGuid().ToString(), "AO", "Administrative Override")
            };

            _studentTranscriptGradesCollection = new List<StudentTranscriptGrades>()
                {
                    new StudentTranscriptGrades("1", studentTranscriptGradesGuid)
                    {
                       AltcumContribCmplCredits = 1.0m,
                       AltcumContribGpaCredits = 2.0m,
                       AltcumContribGradePts = 3.0m,
                       AttemptedCeus = 1.0m,
                       AttemptedCredit = 1.0m,
                       CompletedCeus = 1.0m,
                       CompletedCredit = 1.0m,
                       ContribCmplCredits = 1.0m,
                       ContribGpaCredits = 1.0m,
                       ContribGradePoints = 1.0m,
                       Course = "MATH-101",
                       CourseName = "MATH-101",
                       CourseSection = "1",
                       CreditType = "TR",
                       FinalGradeExpirationDate = DateTime.Now,
                       GradePoints = 2.0m,
                       GradeSchemeCode = "UG",
                       RepeatAcademicCreditIds = new List<string> { "1", "2"},
                       ReplCode = "Y",
                       StudentCourseSectionId = "0000005",

                       StudentId = "0000111",
                       StwebTranAltcumFlag = false,
                       Title = "title",
                       VerifiedGrade = "2",
                       VerifiedGradeDate = DateTime.Now,
                       StudentTranscriptGradesHistory = new List<StudentTranscriptGradesHistory>()
                       {
                           new StudentTranscriptGradesHistory("IC", "1", DateTime.Now)
                       }
                    }
                };
            studentTranscriptGradesTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentTranscriptGrades>, int>(_studentTranscriptGradesCollection, _studentTranscriptGradesCollection.Count());

            // Adjustments
            _studentTranscriptGradesAdjustmentsCollection = new List<Dtos.StudentTranscriptGradesAdjustments>()
            {
                new Dtos.StudentTranscriptGradesAdjustments()
                {
                    Id = studentTranscriptGradesGuid,
                    Detail = new Dtos.StudentTranscriptGradesAdjustmentsDetail()
                    {
                        ChangeReason = new Dtos.GuidObject2(_gradeChangeReasonsCollection.Where(gc => gc.Code == "IC").FirstOrDefault().Guid),
                        Grade = new Dtos.GuidObject2(_gradeCollection.Where(gc => gc.Id == "I").FirstOrDefault().Guid),
                        IncompleteGrade = new Dtos.StudentTranscriptGradesIncompleteGradeDtoProperty()
                        {
                            FinalGrade = new Dtos.GuidObject2(_gradeCollection.Where(gc => gc.Id == "F").FirstOrDefault().Guid),
                            ExtensionDate = new DateTime(2020, 10, 31)
                        }
                    }
                },
                new Dtos.StudentTranscriptGradesAdjustments()
                {
                    Id = studentTranscriptGradesGuid,
                    Detail = new Dtos.StudentTranscriptGradesAdjustmentsDetail()
                    {
                        ChangeReason = new Dtos.GuidObject2(_gradeChangeReasonsCollection.Where(gc => gc.Code == "IC").FirstOrDefault().Guid),
                        Grade = new Dtos.GuidObject2(_gradeCollection.Where(gc => gc.Id == "A").FirstOrDefault().Guid)
                    }
                },
                new Dtos.StudentTranscriptGradesAdjustments()
                {
                    Id = studentTranscriptGradesGuid,
                    Detail = new Dtos.StudentTranscriptGradesAdjustmentsDetail()
                    {
                        ChangeReason = new Dtos.GuidObject2(Guid.NewGuid().ToString()),
                        Grade = new Dtos.GuidObject2(_gradeCollection.Where(gc => gc.Id == "I").FirstOrDefault().Guid)
                    }
                },
                new Dtos.StudentTranscriptGradesAdjustments()
                {
                    Id = studentTranscriptGradesGuid,
                    Detail = new Dtos.StudentTranscriptGradesAdjustmentsDetail()
                    {
                        ChangeReason = new Dtos.GuidObject2(_gradeChangeReasonsCollection.Where(gc => gc.Code == "IC").FirstOrDefault().Guid)
                    }
                }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentTranscriptGradesService = null;
            _studentTranscriptGradesCollection = null;
            _studentTranscriptGradesAdjustmentsCollection = null;
            _studentReferenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        #region student-transcript-grades

        [TestMethod]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesAsync()
        {
            var results = await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            var actuals = results.Item1.ToList();
            for (int i = 0; i < results.Item1.Count(); i++)
            {
                var expected = _studentTranscriptGradesCollection[i];
                var actual = actuals[i];

                Assert.AreEqual(expected.Guid, actual.Id, "Guid");
                var awardGradeScheme = this._gradeSchemeCollection.FirstOrDefault(x => x.Code == expected.GradeSchemeCode);
                Assert.AreEqual(awardGradeScheme.Guid, actual.AwardGradeScheme.Id, "AwardGradeScheme");
                Assert.AreEqual(expected.AttemptedCredit, actual.Credit.AttemptedCredit, "AttemptedCredit");
                Assert.AreEqual(expected.CompletedCredit, actual.Credit.EarnedCredit, "EarnedCredit");
                Assert.AreEqual(expected.ContribGpaCredits, actual.Credit.QualityPoint.Gpa, "Gpa");
                Assert.AreEqual(expected.GradePoints, actual.Credit.QualityPoint.NonWeighted, "nonWeighted");
                Assert.AreEqual(Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.RepeatedIncludeBoth, actual.Credit.RepeatedSection, "RepeatedSection");

                var creditCategory = this._creditCategoriesCollection.FirstOrDefault(x => x.Code == expected.CreditType);
                Assert.AreEqual(creditCategory.Guid, actual.CreditCategory.Id, "CreditCategory");

                var gradeDefinitions = this._gradeCollection.FirstOrDefault(x => x.Id == expected.VerifiedGrade);
                Assert.AreEqual(gradeDefinitions.Guid, actual.Grade.Id, "Grade");

                Assert.AreEqual(personGuid, actual.Student.Id, "Student");
                Assert.AreEqual(studentCourseSecGuid, actual.UnverifiedGrade.Id, "Unverified grade");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGrades_CreditCategories_Null()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGrades_CreditCategories_NotFound()
        {
            _creditCategoriesCollection = new List<CreditCategory>() { new CreditCategory("invalid", "x", "x", CreditType.None) };
            _studentReferenceRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(_creditCategoriesCollection);

            try
            {
                await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex.Errors);
                Assert.AreEqual("Credit Categories not found for key: 'TR'.", ex.Errors[0].Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGrades_Grade_Null()
        {
            _gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGrades_Grade_NotFound()
        {
            _gradeCollection = new List<Grade>() { new Grade("invalid", "1", "x", "x", "x", "x") };
            _gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(_gradeCollection);

            await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGrades_GetPersonGuidsCollection_Null()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(null);
            await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());          
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGrades_GetPersonGuidsCollection_NotFound()
        {
            var personGuidDictionary = new Dictionary<string, string>() { };
            personGuidDictionary.Add("Invalid", personGuid);
            _personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(personGuidDictionary);
            await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGrades_GetStudentCourseSecGuidsCollection_Null()
        {
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetGuidsCollectionAsync(It.IsAny<List<string>>(), "STUDENT.COURSE.SEC"))
                .ReturnsAsync(null);
            await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGrades_GetStudentCourseSecGuidsCollection_NotFound()
        {
            var studentCourseSecGuidDictionary = new Dictionary<string, string>() { };
            studentCourseSecGuidDictionary.Add("Invalid", studentCourseSecGuid);
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetGuidsCollectionAsync(It.IsAny<List<string>>(), "STUDENT.COURSE.SEC"))
                .ReturnsAsync(studentCourseSecGuidDictionary);
            await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());
        }

        [TestMethod]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesByGuidAsync()
        {
            var actual = await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync(studentTranscriptGradesGuid);
            var expected = _studentTranscriptGradesCollection[0];

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Guid, actual.Id, "Guid");
            var awardGradeScheme = this._gradeSchemeCollection.FirstOrDefault(x => x.Code == expected.GradeSchemeCode);
            Assert.AreEqual(awardGradeScheme.Guid, actual.AwardGradeScheme.Id, "AwardGradeScheme");
            Assert.AreEqual(expected.AttemptedCredit, actual.Credit.AttemptedCredit, "AttemptedCredit");
            Assert.AreEqual(expected.CompletedCredit, actual.Credit.EarnedCredit, "EarnedCredit");
            Assert.AreEqual(expected.ContribGpaCredits, actual.Credit.QualityPoint.Gpa, "Gpa");
            Assert.AreEqual(expected.GradePoints, actual.Credit.QualityPoint.NonWeighted, "nonWeighted");
            Assert.AreEqual(Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.RepeatedIncludeBoth, actual.Credit.RepeatedSection, "RepeatedSection");
            var creditCategory = this._creditCategoriesCollection.FirstOrDefault(x => x.Code == expected.CreditType);
            Assert.AreEqual(creditCategory.Guid, actual.CreditCategory.Id, "CreditCategory");
            var gradeDefinitions = this._gradeCollection.FirstOrDefault(x => x.Id == expected.VerifiedGrade);
            Assert.AreEqual(gradeDefinitions.Guid, actual.Grade.Id, "Grade");
            Assert.AreEqual(personGuid, actual.Student.Id, "Student");
            Assert.AreEqual(studentCourseSecGuid, actual.UnverifiedGrade.Id, "Unverified grade");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesByGuidAsync_EmptyString()
        {
            await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync("");         
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesByGuidAsync_Null()
        {
            await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesByIdAsync_PermissionException()
        {
            viewStudentTranscriptGrades.RemovePermission(perm);
            viewStudentTranscriptGrades.RemovePermission(updatePerm);
            var results = await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesAsync_PermissionException()
        {
            viewStudentTranscriptGrades.RemovePermission(perm);
            viewStudentTranscriptGrades.RemovePermission(updatePerm);
            var results = await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.StudentTranscriptGrades(), It.IsAny<bool>());
        }

        [TestMethod]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesAsyncByGuid_UpdatePermissionOnView()
        {
            viewStudentTranscriptGrades.RemovePermission(perm);
            var results = await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesByGuidAsync_KeyNotFoundException()
        {
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync(studentTranscriptGradesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesByGuidAsync_InvalidOperationException()
        {
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException());
            var actual = await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync(studentTranscriptGradesGuid);
        }

        #endregion

        #region student-transcript-grades-adjustments

        [TestMethod]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesAdjustmetsByGuidAsync()
        {
            var actual = await _studentTranscriptGradesService.GetStudentTranscriptGradesAdjustmentsByGuidAsync(studentTranscriptGradesGuid);
            var expected = _studentTranscriptGradesAdjustmentsCollection[0];

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id, "Guid");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesAdjustmentsByGuidAsync_EmptyString()
        {
            await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesAdjustmentsByGuidAsync_Null()
        {
            await _studentTranscriptGradesService.GetStudentTranscriptGradesAdjustmentsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTranscriptGradesService_GetStudentTranscriptGradesAdjustmentsByGuidAsync_KeyNotFoundException()
        {
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await _studentTranscriptGradesService.GetStudentTranscriptGradesAdjustmentsByGuidAsync(studentTranscriptGradesGuid);
        }

        [TestMethod]
        public async Task StudentTranscriptGradesService_UpdateStudentTranscriptGradesAdjustmetsAsync()
        {
            var actual = await _studentTranscriptGradesService.UpdateStudentTranscriptGradesAdjustmentsAsync(_studentTranscriptGradesAdjustmentsCollection[1]);
            var expected = _studentTranscriptGradesCollection[0];

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Guid, actual.Id, "Guid");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTranscriptGradesService_UpdateStudentTranscriptGradesAdjustmentsByGuidAsync_KeyNotFoundException()
        {
            _studentTranscriptGradesRepositoryMock.Setup(repo => repo.UpdateStudentTranscriptGradesAdjustmentsAsync(It.IsAny<StudentTranscriptGradesAdjustments>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await _studentTranscriptGradesService.UpdateStudentTranscriptGradesAdjustmentsAsync(_studentTranscriptGradesAdjustmentsCollection[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTranscriptGradesService_UpdateStudentTranscriptGradesAdjustmentsAsync_Null()
        {
            await _studentTranscriptGradesService.UpdateStudentTranscriptGradesAdjustmentsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_UpdateStudentTranscriptGradesAdjustmentsAsync_ArgumentException()
        {
            var actual = await _studentTranscriptGradesService.UpdateStudentTranscriptGradesAdjustmentsAsync(_studentTranscriptGradesAdjustmentsCollection[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesService_UpdateStudentTranscriptGradesAdjustmentsAsync_GradeChangeReasonException()
        {
            var actual = await _studentTranscriptGradesService.UpdateStudentTranscriptGradesAdjustmentsAsync(_studentTranscriptGradesAdjustmentsCollection[2]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTranscriptGradesService_UpdateStudentTranscriptGradesAdjustmentsAsync_ArgumentNullException()
        {
            var actual = await _studentTranscriptGradesService.UpdateStudentTranscriptGradesAdjustmentsAsync(_studentTranscriptGradesAdjustmentsCollection[3]);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task StudentTranscriptGradesService_UpdateStudentTranscriptGradesAdjustmentsAsync_PermissionException()
        {
            viewStudentTranscriptGrades.RemovePermission(updatePerm);
            var results = await _studentTranscriptGradesService.UpdateStudentTranscriptGradesAdjustmentsAsync(_studentTranscriptGradesAdjustmentsCollection[0], false);
        }

        #endregion
    }
}