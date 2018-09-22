//Copyright 2017 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AssessmentCalculationMethodsServiceTests
    {
        private const string assessmentCalculationMethodsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string assessmentCalculationMethodsCode = "LAST";
        private ICollection<NonCourseGradeUses> _nonCourseGradeUsesCollection;
        private AssessmentCalculationMethodsService _assessmentCalculationMethodsService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _nonCourseGradeUsesCollection = new List<NonCourseGradeUses>()
                {
                    new NonCourseGradeUses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LAST", "Use most recent grade"),
                    new NonCourseGradeUses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "BEST", "Use best grade"),
                    new NonCourseGradeUses("d2253ac7-9931-4560-b42f-1fccd43c952e", "SUB", "Use best subtest")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetNonCourseGradeUsesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_nonCourseGradeUsesCollection);

            _assessmentCalculationMethodsService = new AssessmentCalculationMethodsService(_referenceRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _assessmentCalculationMethodsService = null;
            _nonCourseGradeUsesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task AssessmentCalculationMethodsService_GetAssessmentCalculationMethodsAsync()
        {
            var results = await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsAsync(true);
            Assert.IsTrue(results is IEnumerable<AssessmentCalculationMethods>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AssessmentCalculationMethodsService_GetAssessmentCalculationMethodsAsync_Count()
        {
            var results = await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AssessmentCalculationMethodsService_GetAssessmentCalculationMethodsAsync_Properties()
        {
            var result =
                (await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsAsync(true)).FirstOrDefault(x => x.Code == assessmentCalculationMethodsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AssessmentCalculationMethodsService_GetAssessmentCalculationMethodsAsync_Expected()
        {
            var expectedResults = _nonCourseGradeUsesCollection.FirstOrDefault(c => c.Guid == assessmentCalculationMethodsGuid);
            var actualResult =
                (await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsAsync(true)).FirstOrDefault(x => x.Id == assessmentCalculationMethodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AssessmentCalculationMethodsService_GetAssessmentCalculationMethodsByGuidAsync_Empty()
        {
            await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AssessmentCalculationMethodsService_GetAssessmentCalculationMethodsByGuidAsync_Null()
        {
            await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AssessmentCalculationMethodsService_GetAssessmentCalculationMethodsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetNonCourseGradeUsesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsByGuidAsync("99");
        }

        [TestMethod]
        public async Task AssessmentCalculationMethodsService_GetAssessmentCalculationMethodsByGuidAsync_Expected()
        {
            var expectedResults =
                _nonCourseGradeUsesCollection.First(c => c.Guid == assessmentCalculationMethodsGuid);
            var actualResult =
                await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsByGuidAsync(assessmentCalculationMethodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AssessmentCalculationMethodsService_GetAssessmentCalculationMethodsByGuidAsync_Properties()
        {
            var result =
                await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsByGuidAsync(assessmentCalculationMethodsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}