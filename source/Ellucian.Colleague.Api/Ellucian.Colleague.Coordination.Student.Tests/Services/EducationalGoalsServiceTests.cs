//Copyright 2020 Ellucian Company L.P. and its affiliates.


using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class EducationalGoalsServiceTests
    {
        private const string educationalGoalsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string educationalGoalsCode = "AT";
        private ICollection<EducationGoals> _educationalGoalsCollection;
        private EducationalGoalsService _educationalGoalsService;
        
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _educationalGoalsCollection = new List<EducationGoals>()
                {
                    new EducationGoals("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new EducationGoals("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new EducationGoals("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetEducationGoalsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_educationalGoalsCollection);

            _educationalGoalsService = new EducationalGoalsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _educationalGoalsService = null;
            _educationalGoalsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task EducationalGoalsService_GetEducationalGoalsAsync()
        {
            var results = await _educationalGoalsService.GetEducationalGoalsAsync(true);
            Assert.IsTrue(results is IEnumerable<EducationalGoals>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task EducationalGoalsService_GetEducationalGoalsAsync_Count()
        {
            var results = await _educationalGoalsService.GetEducationalGoalsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task EducationalGoalsService_GetEducationalGoalsAsync_Properties()
        {
            var result =
                (await _educationalGoalsService.GetEducationalGoalsAsync(true)).FirstOrDefault(x => x.Code == educationalGoalsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task EducationalGoalsService_GetEducationalGoalsAsync_Expected()
        {
            var expectedResults = _educationalGoalsCollection.FirstOrDefault(c => c.Guid == educationalGoalsGuid);
            var actualResult =
                (await _educationalGoalsService.GetEducationalGoalsAsync(true)).FirstOrDefault(x => x.Id == educationalGoalsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EducationalGoalsService_GetEducationalGoalsByGuidAsync_Empty()
        {
            await _educationalGoalsService.GetEducationalGoalsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EducationalGoalsService_GetEducationalGoalsByGuidAsync_Null()
        {
            await _educationalGoalsService.GetEducationalGoalsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EducationalGoalsService_GetEducationalGoalsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetEducationGoalsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _educationalGoalsService.GetEducationalGoalsByGuidAsync("99");
        }

        [TestMethod]
        public async Task EducationalGoalsService_GetEducationalGoalsByGuidAsync_Expected()
        {
            var expectedResults =
                _educationalGoalsCollection.First(c => c.Guid == educationalGoalsGuid);
            var actualResult =
                await _educationalGoalsService.GetEducationalGoalsByGuidAsync(educationalGoalsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task EducationalGoalsService_GetEducationalGoalsByGuidAsync_Properties()
        {
            var result =
                await _educationalGoalsService.GetEducationalGoalsByGuidAsync(educationalGoalsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}