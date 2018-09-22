//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    public class MealPlanRatesServiceTests
    {
        private const string mealPlanRatesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string mealPlanRatesCode = "AT";
        private ICollection<Domain.Student.Entities.MealPlanRates> _mealPlanRatesCollection;
        private ICollection<Domain.Student.Entities.MealPlan> _mealPlanCollection;
        private MealPlanRatesService _mealPlanRatesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepoMock;
        private Mock<IConfigurationRepository> _configRepoMock;


        [TestInitialize]
        public  void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _roleRepoMock = new Mock<IRoleRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _configRepoMock = new Mock<IConfigurationRepository>();

            _mealPlanRatesCollection = new List<Domain.Student.Entities.MealPlanRates>()
                {
                    new Domain.Student.Entities.MealPlanRates("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic")
                    { MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now), MealRatePeriod = MealPlanRatePeriods.Day },
                    new Domain.Student.Entities.MealPlanRates("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic")
                       { MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now.AddDays(-1)),  MealRatePeriod = MealPlanRatePeriods.Meal },
                new Domain.Student.Entities.MealPlanRates("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                 { MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now.AddDays(1)), MealRatePeriod = MealPlanRatePeriods.Month }
            };

            _mealPlanCollection = new List<Domain.Student.Entities.MealPlan>()
            {
                    new Domain.Student.Entities.MealPlan("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.MealPlan("949e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.MealPlan("e2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
            };

            // Set up current user
            _currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            _referenceRepositoryMock.Setup(repo => repo.GetMealPlanRatesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_mealPlanRatesCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>()))
              .ReturnsAsync(_mealPlanCollection);
             
            // International Parameters Host Country
            _referenceRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

            _mealPlanRatesService = new MealPlanRatesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactory, _roleRepoMock.Object,
                _loggerMock.Object, _configRepoMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mealPlanRatesService = null;
            _mealPlanRatesCollection = null;
            _mealPlanCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _configRepoMock = null;
        }

        [TestMethod]
        public async Task MealPlanRatesService_GetMealPlanRatesAsync()
        {
            var results = await _mealPlanRatesService.GetMealPlanRatesAsync(true);
            Assert.IsTrue(results is IEnumerable<Dtos.MealPlanRates>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task MealPlanRatesService_GetMealPlanRatesAsync_Count()
        {
            var results = await _mealPlanRatesService.GetMealPlanRatesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task MealPlanRatesService_GetMealPlanRatesAsync_Properties()
        {
            var result =
                (await _mealPlanRatesService.GetMealPlanRatesAsync(true)).FirstOrDefault(x => x.Id == mealPlanRatesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
  
        }

        [TestMethod]
        public async Task MealPlanRatesService_GetMealPlanRatesAsync_Expected()
        {
            var expectedResults = _mealPlanRatesCollection.FirstOrDefault(c => c.Guid == mealPlanRatesGuid);
            var actualResult =
                (await _mealPlanRatesService.GetMealPlanRatesAsync(true)).FirstOrDefault(x => x.Id == mealPlanRatesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(string.Concat(expectedResults.Description, " ", Convert.ToDateTime(expectedResults.MealPlansMealPlanRates.EffectiveDates).ToShortDateString()), actualResult.Title);
            Assert.AreEqual(expectedResults.MealPlansMealPlanRates.MealRates, actualResult.Rate.Value);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MealPlanRatesService_GetMealPlanRatesByGuidAsync_Empty()
        {
            await _mealPlanRatesService.GetMealPlanRatesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MealPlanRatesService_GetMealPlanRatesByGuidAsync_Null()
        {
            await _mealPlanRatesService.GetMealPlanRatesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MealPlanRatesService_GetMealPlanRatesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetMealPlanRatesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _mealPlanRatesService.GetMealPlanRatesByGuidAsync("99");
        }

        [TestMethod]
        public async Task MealPlanRatesService_GetMealPlanRatesByGuidAsync_Expected()
        {
            var expectedResults =
                _mealPlanRatesCollection.First(c => c.Guid == mealPlanRatesGuid);
            var actualResult =
                await _mealPlanRatesService.GetMealPlanRatesByGuidAsync(mealPlanRatesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(string.Concat(expectedResults.Description, " ", Convert.ToDateTime(expectedResults.MealPlansMealPlanRates.EffectiveDates).ToShortDateString()), actualResult.Title);
            Assert.AreEqual(expectedResults.MealPlansMealPlanRates.MealRates, actualResult.Rate.Value);

        }

        [TestMethod]
        public async Task MealPlanRatesService_GetMealPlanRatesByGuidAsync_Properties()
        {
            var result =
                await _mealPlanRatesService.GetMealPlanRatesByGuidAsync(mealPlanRatesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);

        }
    }
}