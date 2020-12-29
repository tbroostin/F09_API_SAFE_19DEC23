//Copyright 2018 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentTranscriptGradesOptionsServiceTests : StudentUserFactory
    {
        protected Ellucian.Colleague.Domain.Entities.Role viewStudentTranscriptGrades = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.TRANSCRIPT.GRADES");

        private const string studentTranscriptGradesOptionsGuid = "55afd535-2101-40c7-9ac2-70a10721db26";
  
        private List<Domain.Student.Entities.StudentTranscriptGradesOptions> _studentTranscriptGradesOptionsCollection;
        private Tuple<IEnumerable<StudentTranscriptGradesOptions>, int> studentTranscriptGradesOptionsTuple;
        private StudentTranscriptGradesOptionsService _studentTranscriptGradesOptionsService;
        private Ellucian.Colleague.Domain.Entities.Permission perm = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentTranscriptGrades);

        private Mock<IStudentTranscriptGradesOptionsRepository> _studentTranscriptGradesOptionsRepositoryMock;
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

        private string personGuid;
        private string studentCourseSecGuid;
        private string courseGuid;

        [TestInitialize]
        public void Initialize()
        {
            _studentTranscriptGradesOptionsRepositoryMock = new Mock<IStudentTranscriptGradesOptionsRepository>();
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
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentTranscriptGrades });

            BuildData();

            BuildMocks();

            _studentTranscriptGradesOptionsService = new StudentTranscriptGradesOptionsService(_studentTranscriptGradesOptionsRepositoryMock.Object,
                _referenceRepositoryMock.Object, _studentReferenceRepositoryMock.Object, _personRepositoryMock.Object, _gradeRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactory, _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        private void BuildMocks()
        {

            personGuid = Guid.NewGuid().ToString();
            studentCourseSecGuid = Guid.NewGuid().ToString();
            courseGuid = Guid.NewGuid().ToString();

            _roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
            {
                viewStudentTranscriptGrades
            });
            _studentTranscriptGradesOptionsRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentTranscriptGradesOptionsTuple);
            _studentTranscriptGradesOptionsRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>()))
                .ReturnsAsync(_studentTranscriptGradesOptionsCollection[0]);

            _gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(_gradeCollection);

            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(_gradeSchemeCollection);
        }

        private async void BuildData()
        {
            _gradeSchemeCollection = (await new TestStudentReferenceDataRepository().GetGradeSchemesAsync()).ToList();
            _gradeCollection = (await new TestGradeRepository().GetHedmAsync()).ToList();

            _studentTranscriptGradesOptionsCollection = new List<StudentTranscriptGradesOptions>()
                {
                    new StudentTranscriptGradesOptions("1", studentTranscriptGradesOptionsGuid)
                    {
                        GradeSchemeCode = "UG"
                    },
                    new StudentTranscriptGradesOptions("2", Guid.NewGuid().ToString())
                    {
                        GradeSchemeCode = "GR"
                    }
                };
            studentTranscriptGradesOptionsTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentTranscriptGradesOptions>, int>(_studentTranscriptGradesOptionsCollection, _studentTranscriptGradesOptionsCollection.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentTranscriptGradesOptionsService = null;
            _studentTranscriptGradesOptionsCollection = null;
            _studentReferenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptionsAsync()
        {
            var results = await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            var actuals = results.Item1.ToList();
            for (int i = 0; i < results.Item1.Count(); i++)
            {
                var expected = _studentTranscriptGradesOptionsCollection[i];
                var actual = actuals[i];

                Assert.AreEqual(expected.Guid, actual.Id, "Guid");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptions_GradeSchemes_Null()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptions_GradeSchemes_NotFound()
        {
            _gradeSchemeCollection = new List<GradeScheme>() { new GradeScheme("invalid", "1", "x") };
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(_gradeSchemeCollection);

            await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptions_Grade_Null()
        {
            _gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptions_Grade_NotFound()
        {
            _gradeCollection = new List<Grade>() { new Grade("invalid", "1", "x", "x", "x", "x") };
            _gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(_gradeCollection);

            await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>());
        }

        [TestMethod]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptions_StudentFilter_Null()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                .ReturnsAsync(null);
            var studentFilter = new Dtos.Filters.StudentFilter() { Student = new Dtos.GuidObject2(Guid.NewGuid().ToString()) };

            var actual = await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), studentFilter, It.IsAny<bool>());

            Assert.IsInstanceOfType(actual, typeof(Tuple<IEnumerable<Dtos.StudentTranscriptGradesOptions>, int>));
            Assert.AreEqual(actual.Item1.Count(), 0);
            Assert.AreEqual(actual.Item2, 0);
        }

        [TestMethod]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptions_GetPersonGuidsCollection_NotFound()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                .Throws(new KeyNotFoundException());
            var studentFilter = new Dtos.Filters.StudentFilter() { Student = new Dtos.GuidObject2(Guid.NewGuid().ToString()) };

            var actual = await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), studentFilter, It.IsAny<bool>());

            Assert.IsInstanceOfType(actual, typeof(Tuple<IEnumerable<Dtos.StudentTranscriptGradesOptions>, int>));
            Assert.AreEqual(actual.Item1.Count(), 0);
            Assert.AreEqual(actual.Item2, 0);
        }

        [TestMethod]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptionsByGuidAsync()
        {
            var actual = await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsByGuidAsync(studentTranscriptGradesOptionsGuid);
            var expected = _studentTranscriptGradesOptionsCollection[0];

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Guid, actual.Id, "Guid");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptionsByGuidAsync_EmptyString()
        {
            await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsByGuidAsync("");         
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptionsByGuidAsync_Null()
        {
            await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptionsByGuidAsync_KeyNotFoundException()
        {
            _studentTranscriptGradesOptionsRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsByGuidAsync(studentTranscriptGradesOptionsGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task StudentTranscriptGradesOptionsService_GetStudentTranscriptGradesOptionsByGuidAsync_InvalidOperationException()
        {
            _studentTranscriptGradesOptionsRepositoryMock.Setup(repo => repo.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException());
            var actual = await _studentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsByGuidAsync(studentTranscriptGradesOptionsGuid);
        }
    }
}