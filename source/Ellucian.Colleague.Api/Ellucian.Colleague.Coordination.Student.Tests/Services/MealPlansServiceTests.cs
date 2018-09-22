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
    public class MealPlansServiceTests
    {
        private const string mealPlansGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string mealPlansCode = "AT";
        private ICollection<MealPlan> _mealPlansCollection;
        private ICollection<MealType> _mealTypesCollection;
        private ICollection<Domain.Base.Entities.Room> _diningsCollection;
        private ICollection<Domain.Base.Entities.Building> _buildingsCollection;
        private ICollection<Domain.Base.Entities.Location> _sitesCollection;
        private MealPlansService _mealPlansService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IRoomRepository> _roomRepositoryMock;
        private Mock<IStaffRepository> _staffRepositoryMock;
        private Mock<IStudentRepository> _studentRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _staffRepositoryMock = new Mock<IStaffRepository>();
            _studentRepositoryMock = new Mock<IStudentRepository>();
            _loggerMock = new Mock<ILogger>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            _mealPlansCollection = new List<MealPlan>()
                {
                    new MealPlan("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") { ComponentTimePeriod = "D", 
                        ComponentNumberOfUnits = (decimal) 7.0, MealTypes = new List<string>() { "CODE1" }, 
                        DiningFacilities = new List<string>() { "CO*DE1" }, Buildings = new List<string>() { "CODE1" }, 
                        Sites = new List<string>() { "CODE1" } },
                    new MealPlan("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new MealPlan("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            _mealTypesCollection = new List<MealType>()
                {
                    new MealType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "CODE1", "DESC1"),
                    new MealType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CODE2", "DESC2"),
                    new MealType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CODE3", "DESC3")
                };

            _diningsCollection = new List<Domain.Base.Entities.Room>()
                {
                    new Domain.Base.Entities.Room("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "CO*DE1", "DESC1"),
                    new Domain.Base.Entities.Room("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CO*DE2", "DESC2"),
                    new Domain.Base.Entities.Room("d2253ac7-9931-4560-b42f-1fccd43c952e", "CO*DE3", "DESC3")
                };

            _buildingsCollection = new List<Domain.Base.Entities.Building>()
                {
                    new Domain.Base.Entities.Building("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "CODE1", "DESC1"),
                    new Domain.Base.Entities.Building("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CODE2", "DESC2"),
                    new Domain.Base.Entities.Building("d2253ac7-9931-4560-b42f-1fccd43c952e", "CODE3", "DESC3")
                };

            _sitesCollection = new List<Domain.Base.Entities.Location>()
                {
                    new Domain.Base.Entities.Location("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "CODE1", "DESC1"),
                    new Domain.Base.Entities.Location("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CODE2", "DESC2"),
                    new Domain.Base.Entities.Location("d2253ac7-9931-4560-b42f-1fccd43c952e", "CODE3", "DESC3")
                };
           
            _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>()))
                .ReturnsAsync(_mealPlansCollection);

            _studentReferenceRepositoryMock.Setup(repo => repo.GetMealTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_mealTypesCollection);

            _roomRepositoryMock.Setup(repo => repo.GetRoomsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_diningsCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetBuildingsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_buildingsCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_sitesCollection);

            _mealPlansService = new MealPlansService(_studentReferenceRepositoryMock.Object, _referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentFactoryMock.Object, _roleRepositoryMock.Object, _studentRepositoryMock.Object, _staffRepositoryMock.Object, _roomRepositoryMock.Object, _loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mealPlansService = null;
            _mealPlansCollection = null;
            _studentReferenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task MealPlansService_GetMealPlansAsync()
        {
            var results = await _mealPlansService.GetMealPlansAsync(true);
            Assert.IsTrue(results is IEnumerable<MealPlans>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task MealPlansService_GetMealPlansAsync_Count()
        {
            var results = await _mealPlansService.GetMealPlansAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task MealPlansService_GetMealPlansAsync_Properties()
        {
            var result =
                (await _mealPlansService.GetMealPlansAsync(true)).FirstOrDefault(x => x.Code == mealPlansCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task MealPlansService_GetMealPlansAsync_Expected()
        {
            var expectedResults = _mealPlansCollection.FirstOrDefault(c => c.Guid == mealPlansGuid);
            var actualResult =
                (await _mealPlansService.GetMealPlansAsync(true)).FirstOrDefault(x => x.Id == mealPlansGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task MealPlansService_GetMealPlansByGuidAsync_Empty()
        {
            await _mealPlansService.GetMealPlansByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task MealPlansService_GetMealPlansByGuidAsync_Null()
        {
            await _mealPlansService.GetMealPlansByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task MealPlansService_GetMealPlansByGuidAsync_InvalidId()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _mealPlansService.GetMealPlansByGuidAsync("99");
        }

        [TestMethod]
        public async Task MealPlansService_GetMealPlansByGuidAsync_Expected()
        {
            var expectedResults =
                _mealPlansCollection.First(c => c.Guid == mealPlansGuid);
            var actualResult =
                await _mealPlansService.GetMealPlansByGuidAsync(mealPlansGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task MealPlansService_GetMealPlansByGuidAsync_Properties()
        {
            var result =
                await _mealPlansService.GetMealPlansByGuidAsync(mealPlansGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}