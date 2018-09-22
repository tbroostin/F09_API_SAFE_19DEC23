using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class MealPlanAssignmentRepositoryTests : BaseRepositorySetup
    {
        private ICollection<MealPlanAssignment> _studentMealPlansCollection;
        private Collection<DataContracts.MealPlanAssignment> records;

        MealPlanAssignmentRepository _mealPlanAssignmentRepository;
        Mock<IColleagueDataReader> dataAccessorMock;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            BuildData();
            _mealPlanAssignmentRepository = BuildMealPlanAssignmentRepository();
        }

        private void BuildData()
        {
            _studentMealPlansCollection = new List<MealPlanAssignment>()
                {
                    new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "LNCH", DateTime.Today, 5, "A", DateTime.Today)
                    {
                       EndDate = DateTime.Today.AddMonths(2),
                       OverrideArCode = "111",                       
                       OverrideRate = 950.00m,
                       RateOverrideReason = "AT",
                       Term = "2016/Spr",
                       UsedRatePeriods = 1,
                       MealCard = "meal assignment 123",
                       MealComments = "Dietrich Dining Hall",
                       PercentUsed = 30
                    },
                    new MealPlanAssignment("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "2", "DIN", DateTime.Today, 4, "T", DateTime.Today)
                    {
                       EndDate = DateTime.Today.AddMonths(1),
                       OverrideArCode = "111",
                       OverrideRate = null,
                       RateOverrideReason = "AC",
                       Term = "2016/Fall",
                       UsedRatePeriods = 1,
                       MealCard = "meal assignment xyz",
                       MealComments = "Squires Dining Hall",
                       PercentUsed = 30
                    }
                };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentMealPlansCollection = null;
            records = null;
            _mealPlanAssignmentRepository = null;
            dataAccessorMock = null;
        }
        
        [TestMethod]
        public async Task MealPlanAssignmentRepository_GET()
        {
            dataAccessorMock.Setup(repo => repo.SelectAsync("MEAL.PLAN.ASSIGNMENT", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2" });
            var results = await _mealPlanAssignmentRepository.GetAsync(It.IsAny<int>(), It.IsAny<int>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MealPlanAssignmentRepository_GET_KeyNotFoundException()
        {
            dataAccessorMock.Setup(repo => repo.SelectAsync("MEAL.PLAN.ASSIGNMENT", It.IsAny<string>())).ReturnsAsync(new[] { "1", "200" });
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.MealPlanAssignment>("MEAL.PLAN.ASSIGNMENT", It.IsAny<string[]>(), true)).ReturnsAsync(null);

            var results = await _mealPlanAssignmentRepository.GetAsync(It.IsAny<int>(), It.IsAny<int>());
        }

        [TestMethod]
        public async Task MealPlanAssignmentRepository_GETById()
        {
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.MealPlanAssignment>(It.IsAny<string>(), true)).ReturnsAsync(records[0]);
            var results = await _mealPlanAssignmentRepository.GetByIdAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            Assert.IsNotNull(results);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MealPlanAssignmentRepository_GETById_ArgumentNullException()
        {
            var results = await _mealPlanAssignmentRepository.GetByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MealPlanAssignmentRepository_GETById_KeyNotFoundException()
        {
            var results = await _mealPlanAssignmentRepository.GetByIdAsync("BadKey");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MealPlanAssignmentRepository_GETById_MealPlanAssignments_Null_KeyNotFoundException()
        {
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.MealPlanAssignment>(It.IsAny<string>(), true)).ReturnsAsync(null);
            var results = await _mealPlanAssignmentRepository.GetByIdAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
        }

        private MealPlanAssignmentRepository BuildMealPlanAssignmentRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            // Set up data accessor for mocking 
            dataAccessorMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            records = new Collection<DataContracts.MealPlanAssignment>();
            foreach (var item in _studentMealPlansCollection)
            {
                DataContracts.MealPlanAssignment record = new DataContracts.MealPlanAssignment();
                record.RecordGuid = item.Guid;
                record.Recordkey = item.Id;
                record.MpasPersonId = item.PersonId;
                record.MpasEndDate = item.EndDate;
                record.MpasMealPlan = item.MealPlan;
                record.MpasNoRatePeriods = item.NoRatePeriods;
                record.MpasOverrideArCode = item.OverrideArCode;
                record.MpasOverrideRate = item.OverrideRate;
                record.MpasRateOverrideReason = item.RateOverrideReason;
                record.MpasStartDate = item.StartDate;
                record.MpasTerm = item.Term;
                record.MpasComments = item.MealComments;
                record.MpasMealCard = item.MealCard;
                record.MpasStatusesEntityAssociation = new List<DataContracts.MealPlanAssignmentMpasStatuses>()
                {
                    new DataContracts.MealPlanAssignmentMpasStatuses()
                    {
                        MpasStatusAssocMember = "Status1",
                        MpasStatusDateAssocMember = DateTime.Today.AddDays(2)
                    }
                };
                records.Add(record);
            }
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.MealPlanAssignment>("MEAL.PLAN.ASSIGNMENT", It.IsAny<string[]>(), true)).ReturnsAsync(records);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var stuprog = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, stuprog == null ? null : new GuidLookupResult() { Entity = "MEAL.PLAN.ASSIGNMENT", PrimaryKey = stuprog.Recordkey });
                }
                return Task.FromResult(result);
            });

            // Construct repository
            _mealPlanAssignmentRepository = new MealPlanAssignmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return _mealPlanAssignmentRepository;
        }
    }
}
