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
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AptitudeAssessmentsServiceTests
    {
        private const string aptitudeAssessmentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string aptitudeAssessmentsTitle = "Title 1";
        private ICollection<NonCourse> _nonCoursesCollection;
        private AptitudeAssessmentsService _aptitudeAssessmentsService;
        private Mock<ILogger> _loggerMock;
        private Mock<IAptitudeAssessmentsRepository> _aptitudeAssessmentsRepositoryMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _aptitudeAssessmentsRepositoryMock = new Mock<IAptitudeAssessmentsRepository>();
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configRepositoryMock = new Mock<IConfigurationRepository>();

            BuildData();

            _aptitudeAssessmentsService = new AptitudeAssessmentsService(_aptitudeAssessmentsRepositoryMock.Object, _referenceRepositoryMock.Object, _adapterRegistryMock.Object,
                _currentUserFactoryMock.Object, _configRepositoryMock.Object, _roleRepositoryMock.Object, _loggerMock.Object);
        }

        private void BuildData()
        {
            _nonCoursesCollection = new List<NonCourse>()
                {
                    new NonCourse("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                    {
                        AssessmentTypeId = "AD",
                        CalculationMethod = "",
                        Title = "",
                        Description = "Description 1",
                        ParentAssessmentId = " ",
                        ScoreMax = 100,
                        ScoreMin = 45,
                        
                    },
                    new NonCourse("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC")
                    {
                        AssessmentTypeId = "PL",
                        CalculationMethod = "Last",
                        Title = "Title 1",
                        Description = "Description 2",
                        ParentAssessmentId = "A",
                        ScoreMax = 100,
                        ScoreMin = 45,
                    },
                    new NonCourse("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU")
                    {
                        AssessmentTypeId = "R",
                        CalculationMethod = "Best",
                        Title = "Title 2",
                        Description = "Description 3",
                        ParentAssessmentId = "B",
                        ScoreMax = 100,
                        ScoreMin = 45,
                    },
                    new NonCourse("e2253ac7-9931-4560-b42f-1fccd43c952f", "CU")
                    {
                        AssessmentTypeId = "R",
                        CalculationMethod = "Sub",
                        Title = "Title 3",
                        Description = "Description 4",
                        ParentAssessmentId = "C",
                        ScoreMax = 100,
                        ScoreMin = 45,
                    }
                };
            Dictionary<string, string> assessGuids = new Dictionary<string,string>();
            assessGuids.Add("A", "91b650b6-77c5-4815-a759-18a9fc31e176");
            assessGuids.Add("B", "22d37ba4-156b-4f4b-a742-06940c45a8bd");
            assessGuids.Add("C", "2e7a9180-34fd-48f5-a8ba-3690a0b17225");
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(assessGuids);

            IEnumerable<NonCourseGradeUses> nonCourseGrdUses = new List<NonCourseGradeUses>() 
            {
                new NonCourseGradeUses("84da24fd-9b1a-49f4-baab-330883bf784b", "Last", "Use most recent grade"),
                new NonCourseGradeUses("5acb6ba9-7576-434e-b93b-714ad7ff1a59", "Best", "Use best grade"),
                new NonCourseGradeUses("b5f512d6-89e0-488f-8170-3ae12e095e62", "Sub", "Use best subtest")
            };
            _referenceRepositoryMock.Setup(repo => repo.GetNonCourseGradeUsesAsync(It.IsAny<bool>())).ReturnsAsync(nonCourseGrdUses);

            IEnumerable<NonCourseCategories> nonCourseCats = new List<NonCourseCategories>() 
            {
                new NonCourseCategories("fafab5c5-a46d-424c-9cdf-fc49e512e56c", "AD", "Admissions"),
                new NonCourseCategories("187e26b2-2a17-4362-b32a-24210fc467fd", "PL", "Placement"),
                new NonCourseCategories("b441699c-4045-4754-907d-8c723c42ae64", "R", "Re-Test for Placement")
            };
            _referenceRepositoryMock.Setup(repo => repo.GetNonCourseCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(nonCourseCats);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _aptitudeAssessmentsService = null;
            _nonCoursesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsAsync()
        {
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentsAsync(It.IsAny<bool>())).ReturnsAsync(_nonCoursesCollection);
            var results = await _aptitudeAssessmentsService.GetAptitudeAssessmentsAsync(true);
            Assert.IsTrue(results is IEnumerable<AptitudeAssessment>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsAsync_Count()
        {
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentsAsync(It.IsAny<bool>())).ReturnsAsync(_nonCoursesCollection);
            var results = await _aptitudeAssessmentsService.GetAptitudeAssessmentsAsync(true);
            Assert.AreEqual(4, results.Count());
        }

        [TestMethod]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsAsync_Properties()
        {
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentsAsync(It.IsAny<bool>())).ReturnsAsync(_nonCoursesCollection);
            var result =
                (await _aptitudeAssessmentsService.GetAptitudeAssessmentsAsync(true)).FirstOrDefault(x => x.Title == aptitudeAssessmentsTitle);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Description);

        }

        [TestMethod]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsAsync_Expected()
        {
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentsAsync(It.IsAny<bool>())).ReturnsAsync(_nonCoursesCollection);
            var expectedResults = _nonCoursesCollection.FirstOrDefault(c => c.Guid == aptitudeAssessmentsGuid);
            var actualResult =
                (await _aptitudeAssessmentsService.GetAptitudeAssessmentsAsync(true)).FirstOrDefault(x => x.Id == aptitudeAssessmentsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Description);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsByGuidAsync_Empty()
        {
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await _aptitudeAssessmentsService.GetAptitudeAssessmentsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsByGuidAsync_InvalidOperationException()
        {
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentByIdAsync(It.IsAny<string>())).Throws<InvalidOperationException>();
            await _aptitudeAssessmentsService.GetAptitudeAssessmentsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsByGuidAsync_Null()
        {
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await _aptitudeAssessmentsService.GetAptitudeAssessmentsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsByGuidAsync_InvalidId()
        {
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _aptitudeAssessmentsService.GetAptitudeAssessmentsByGuidAsync("99");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsByGuidAsync_Invalid_AssessmentType_KeyNotFoundException()
        {
            var expectedResults =
                _nonCoursesCollection.First(c => c.Guid == aptitudeAssessmentsGuid);

            expectedResults.AssessmentTypeId = "asdf";
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);

            var actualResult =
                await _aptitudeAssessmentsService.GetAptitudeAssessmentsByGuidAsync(aptitudeAssessmentsGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsByGuidAsync_Invalid_Calculation_KeyNotFoundException()
        {
            var expectedResults =
                _nonCoursesCollection.First(c => c.Guid == aptitudeAssessmentsGuid);

            expectedResults.CalculationMethod = "asdf";
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);

            var actualResult =
                await _aptitudeAssessmentsService.GetAptitudeAssessmentsByGuidAsync(aptitudeAssessmentsGuid);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsByGuidAsync_Expected()
        {
            var expectedResults =
                _nonCoursesCollection.First(c => c.Guid == aptitudeAssessmentsGuid);

            expectedResults.AssessmentTypeId = string.Empty;
            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);

            var actualResult =
                await _aptitudeAssessmentsService.GetAptitudeAssessmentsByGuidAsync(aptitudeAssessmentsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Description);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AptitudeAssessmentsService_GetAptitudeAssessmentsByGuidAsync_Properties()
        {
            var expectedResults =
                  _nonCoursesCollection.First(c => c.Guid == aptitudeAssessmentsGuid);

            _aptitudeAssessmentsRepositoryMock.Setup(repo => repo.GetAptitudeAssessmentByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);
            var result =
                await _aptitudeAssessmentsService.GetAptitudeAssessmentsByGuidAsync(aptitudeAssessmentsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}