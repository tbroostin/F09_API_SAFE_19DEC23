//Copyright 2017-18 Ellucian Company L.P. and its affiliates.

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
using StudentCourseTransferEntity = Ellucian.Colleague.Domain.Student.Entities.StudentCourseTransfer;
using Ellucian.Colleague.Domain.Student.Tests;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentCourseTransfersServiceTests : StudentUserFactory
    {
        protected Ellucian.Colleague.Domain.Entities.Role viewStudentCourseTransfer = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.COURSE.TRANSFERS");

        private const string studentCourseTransfersGuid = "55afd535-2101-40c7-9ac2-70a10721db26";
        private const string studentCourseTransfersCode = "AT";
        private List<Domain.Student.Entities.StudentCourseTransfer> _studentCourseTransfersCollection;
        private Tuple<IEnumerable<StudentCourseTransferEntity>, int> studentTransferCreditTuple;
        private StudentCourseTransfersService _studentCourseTransfersService;
        private Ellucian.Colleague.Domain.Entities.Permission perm = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentCourseTransfers);
        private List<Dtos.StudentCourseTransfer> _studentCourseTransferDtoCollection;

        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAcademicCreditRepository> _academicCreditRepositoryMock;
        private Mock<ICourseRepository> _courseRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IGradeRepository> _gradeRepositoryMock;
        private Mock<ITermRepository> _termRepositoryMock;

        private ICollection<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> _academicLevelCollection;
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.Grade> _gradeCollection;
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> _gradeSchemeCollection;
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.AcademicProgram> _academicProgramCollection;
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.CreditCategory> _creditCategoryCollection;

        private string personGuid;
        private string institutionGuid;
        private string courseGuid;
        
        [TestInitialize]
        public void Initialize()
        {            
            _academicCreditRepositoryMock = new Mock<IAcademicCreditRepository>();
            _courseRepositoryMock = new Mock<ICourseRepository>();
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _gradeRepositoryMock = new Mock<IGradeRepository>();
            _termRepositoryMock = new Mock<ITermRepository>();

            _currentUserFactory = new StudentCourseTransferUser();

            viewStudentCourseTransfer.AddPermission(perm);
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentCourseTransfer });

            personGuid = Guid.NewGuid().ToString();
            institutionGuid = Guid.NewGuid().ToString();
            courseGuid = Guid.NewGuid().ToString();

            BuildData();

            BuildMocks();

            _studentCourseTransfersService = new StudentCourseTransfersService(_personRepositoryMock.Object, _academicCreditRepositoryMock.Object,
                _courseRepositoryMock.Object, _referenceRepositoryMock.Object, _gradeRepositoryMock.Object, _termRepositoryMock.Object, 
                _adapterRegistryMock.Object, _currentUserFactory, _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        private void BuildMocks()
        {
            _roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
            {
                viewStudentCourseTransfer//new Domain.Entities.Role(1, "VIEW.STUDENT.COURSE.TRANSFERS")
            });
            _academicCreditRepositoryMock.Setup(repo => repo.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(studentTransferCreditTuple);
            _academicCreditRepositoryMock.Setup(repo => repo.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(_studentCourseTransfersCollection[0]);


            _referenceRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(_academicLevelCollection);
            _referenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(_gradeSchemeCollection);
            _referenceRepositoryMock.Setup(repo => repo.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(_academicProgramCollection);
            _referenceRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(_creditCategoryCollection);
            _gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(_gradeCollection);

            var personGuidDictionary = new Dictionary<string, string>() { };
            personGuidDictionary.Add("0003977", personGuid);
            personGuidDictionary.Add("0000043", institutionGuid);
            _personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(personGuidDictionary);
            
            var courseGuidDictionary = new Dictionary<string, string>() { };
            courseGuidDictionary.Add("12345", courseGuid);
            _courseRepositoryMock.Setup(repo => repo.GetGuidsCollectionAsync(It.IsAny<List<string>>(), "COURSES"))
                .ReturnsAsync(courseGuidDictionary);
        }

        private async void BuildData()
        {
            _academicLevelCollection = new TestStudentReferenceDataRepository().GetAcademicLevelsAsync().Result.ToList();
            _gradeSchemeCollection = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result.ToList();
            _gradeCollection = new TestGradeRepository().GetHedmAsync().Result.ToList();
            _creditCategoryCollection = new TestStudentReferenceDataRepository().GetCreditCategoriesAsync().Result.ToList();
          
            _studentCourseTransferDtoCollection = new List<Dtos.StudentCourseTransfer>()
            {   
                new Dtos.StudentCourseTransfer()
                {
                    Student = new Dtos.GuidObject2(personGuid),
                    TransferredFrom = new Dtos.GuidObject2(institutionGuid),
                    EquivalentCourse = new Dtos.GuidObject2(courseGuid),               
                    AcademicLevel = new Dtos.GuidObject2("d118f007-c914-465e-80dc-49d39209b24f")
                }
            };

            _studentCourseTransfersCollection = new List<StudentCourseTransferEntity>()
                {
                    new StudentCourseTransferEntity()
                    {
                        AcademicLevel = "UG",
                        AcademicPrograms = new List<string>() { "BA.ENGL", "BS.MATH" },
                        AcademicPeriod = "2018/FA",
                        AwardedCredit = 2.00m,
                        Course = "12345",
                        //DetailGuid = "2a6cbd21-bb2f-433a-ac95-cb4dc1f29c18",
                        EquivalencyAppliedOn = DateTime.Today,
                        Grade = "17",
                        GradeScheme = "UG",
                        Guid = "84f2c406-3741-4098-a454-e27fb5d2f9a1",
                        Id = studentCourseTransfersGuid,
                        QualityPoints = 10.00m,
                        CreditType = "I",
                        Student = "0003977",
                        TransferredFromInstitution = "0000043"

                    }
                };
            studentTransferCreditTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentCourseTransfer>, int>(_studentCourseTransfersCollection, _studentCourseTransfersCollection.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentCourseTransfersService = null;
            _studentCourseTransfersCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task StudentCourseTransfersService_GetStudentCourseTransfersAsync()
        {
            
            var results = await _studentCourseTransfersService.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            var actuals = results.Item1.ToList();
            for (int i = 0; i < results.Item1.Count(); i++)
            {
                var expected = _studentCourseTransferDtoCollection[i];
                var actual = actuals[i];
                Assert.AreEqual(expected.AcademicLevel.Id, actual.AcademicLevel.Id);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                Assert.AreEqual(expected.TransferredFrom.Id, actual.TransferredFrom.Id);
                Assert.AreEqual(expected.EquivalentCourse.Id, actual.EquivalentCourse.Id);
            }
        }

        [TestMethod]
        public async Task StudentCourseTransfersService_GetStudentCourseTransferByGuidAsync()
        {
            var actual = await _studentCourseTransfersService.GetStudentCourseTransferByGuidAsync(studentCourseTransfersGuid);
            var expected = _studentCourseTransferDtoCollection[0];
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.AcademicLevel.Id, actual.AcademicLevel.Id);
            Assert.AreEqual(expected.EquivalentCourse.Id, actual.EquivalentCourse.Id);
            Assert.AreEqual(expected.Student.Id, actual.Student.Id);
            Assert.AreEqual(expected.TransferredFrom.Id, actual.TransferredFrom.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task StudentCourseTransfersService_GetStudentCourseTransfersAsync_PermissionException()
        {
            viewStudentCourseTransfer.RemovePermission(perm);
            var results = await _studentCourseTransfersService.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentCourseTransfersService_GetStudentCourseTransferByGuidAsync_KeyNotFoundException()
        {
            _academicCreditRepositoryMock.Setup(repo => repo.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await _studentCourseTransfersService.GetStudentCourseTransferByGuidAsync(studentCourseTransfersGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task StudentCourseTransfersService_GetStudentCourseTransferByGuidAsync_InvalidOperationException()
        {
            _academicCreditRepositoryMock.Setup(repo => repo.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException());
            var actual = await _studentCourseTransfersService.GetStudentCourseTransferByGuidAsync(studentCourseTransfersGuid);
        }

        //Version 13

        [TestMethod]
        public async Task StudentCourseTransfersService_GetStudentCourseTransfers2Async()
        {
            var results = await _studentCourseTransfersService.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            var actuals = results.Item1.ToList();
            for (int i = 0; i < results.Item1.Count(); i++)
            {
                var expected = _studentCourseTransferDtoCollection[i];
                var actual = actuals[i];
                Assert.AreEqual(expected.AcademicLevel.Id, actual.AcademicLevel.Id);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                Assert.AreEqual(expected.TransferredFrom.Id, actual.TransferredFrom.Id);
                Assert.AreEqual(expected.EquivalentCourse.Id, actual.EquivalentCourse.Id);
            }
        }

        [TestMethod]
        public async Task StudentCourseTransfersService_GetStudentCourseTransfer2ByGuidAsync()
        {
            var actual = await _studentCourseTransfersService.GetStudentCourseTransfer2ByGuidAsync(studentCourseTransfersGuid);
            var expected = _studentCourseTransferDtoCollection[0];
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.AcademicLevel.Id, actual.AcademicLevel.Id);
            Assert.AreEqual(expected.EquivalentCourse.Id, actual.EquivalentCourse.Id);
            Assert.AreEqual(expected.Student.Id, actual.Student.Id);
            Assert.AreEqual(expected.TransferredFrom.Id, actual.TransferredFrom.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task StudentCourseTransfersService_GetStudentCourseTransfers2Async_PermissionException()
        {
            viewStudentCourseTransfer.RemovePermission(perm);
            var results = await _studentCourseTransfersService.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentCourseTransfersService_GetStudentCourseTransfer2ByGuidAsync_KeyNotFoundException()
        {
            _academicCreditRepositoryMock.Setup(repo => repo.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await _studentCourseTransfersService.GetStudentCourseTransfer2ByGuidAsync(studentCourseTransfersGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task StudentCourseTransfersService_GetStudentCourseTransfer2ByGuidAsync_InvalidOperationException()
        {
            _academicCreditRepositoryMock.Setup(repo => repo.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException());
            var actual = await _studentCourseTransfersService.GetStudentCourseTransfer2ByGuidAsync(studentCourseTransfersGuid);
        }
    }
}