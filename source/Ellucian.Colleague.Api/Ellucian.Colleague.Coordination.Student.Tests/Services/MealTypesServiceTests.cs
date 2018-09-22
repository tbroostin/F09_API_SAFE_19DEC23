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
    public class MealTypesServiceTests
    {
        private const string mealTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string mealTypesCode = "AT";
        private ICollection<MealType> _mealTypesCollection;
        private MealTypesService _mealTypesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IStaffRepository> _staffRepositoryMock;
        private Mock<IStudentRepository> _studentRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _staffRepositoryMock = new Mock<IStaffRepository>();
            _studentRepositoryMock = new Mock<IStudentRepository>();
            _loggerMock = new Mock<ILogger>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            _mealTypesCollection = new List<MealType>()
                {
                    new MealType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new MealType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new MealType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetMealTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_mealTypesCollection);

            _mealTypesService = new MealTypesService(_referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentFactoryMock.Object, _roleRepositoryMock.Object, _studentRepositoryMock.Object, _staffRepositoryMock.Object, _loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mealTypesService = null;
            _mealTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task MealTypesService_GetMealTypesAsync()
        {
            var results = await _mealTypesService.GetMealTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<MealTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task MealTypesService_GetMealTypesAsync_Count()
        {
            var results = await _mealTypesService.GetMealTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task MealTypesService_GetMealTypesAsync_Properties()
        {
            var result =
                (await _mealTypesService.GetMealTypesAsync(true)).FirstOrDefault(x => x.Code == mealTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task MealTypesService_GetMealTypesAsync_Expected()
        {
            var expectedResults = _mealTypesCollection.FirstOrDefault(c => c.Guid == mealTypesGuid);
            var actualResult =
                (await _mealTypesService.GetMealTypesAsync(true)).FirstOrDefault(x => x.Id == mealTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task MealTypesService_GetMealTypesByGuidAsync_Empty()
        {
            await _mealTypesService.GetMealTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task MealTypesService_GetMealTypesByGuidAsync_Null()
        {
            await _mealTypesService.GetMealTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task MealTypesService_GetMealTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetMealTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _mealTypesService.GetMealTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task MealTypesService_GetMealTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _mealTypesCollection.First(c => c.Guid == mealTypesGuid);
            var actualResult =
                await _mealTypesService.GetMealTypesByGuidAsync(mealTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task MealTypesService_GetMealTypesByGuidAsync_Properties()
        {
            var result =
                await _mealTypesService.GetMealTypesByGuidAsync(mealTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}