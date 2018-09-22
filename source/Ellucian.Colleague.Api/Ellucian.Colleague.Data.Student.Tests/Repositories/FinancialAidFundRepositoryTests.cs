//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class FinancialAidFundRepositoryTests
    {
        #region SETUP
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataReaderMock;
        Mock<ILogger> loggerMock;

        string valcodeName;
        ApiSettings apiSettings;

        FinancialAidFundRepository fundRepo;
        IEnumerable<FinancialAidFund> allFinancialAidFunds;
        IEnumerable<FinancialAidFundsFinancialProperty> allFinancialAidFundFinancials;

        public string awardId;
        public IEnumerable<string> fundYears;
        public TestFinancialAidFundRepository testDataRepository;

        public async Task<IEnumerable<FinancialAidFundsFinancialProperty>> getExpectedFinancialAidFundsFinancials()
        {
            return await testDataRepository.GetFinancialAidFundFinancialsAsync("AWARD1", new List<string>(), "USA");
        }

        public async Task<IEnumerable<FinancialAidFundsFinancialProperty>> getActualFinancialAidFundsFinancials()
        {
            return await fundRepo.GetFinancialAidFundFinancialsAsync(awardId, fundYears, "USA");
        }

        public async Task<IEnumerable<FinancialAidFundsFinancialProperty>> getActualFinancialAidFundsFinancialsNoYears()
        {
            return await fundRepo.GetFinancialAidFundFinancialsAsync(awardId, new List<string>(), "USA");
        }

        [TestInitialize]
        public void MockInitialize()
        {
            loggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");

            allFinancialAidFunds = new TestFinancialAidFundRepository().GetFinancialAidFundsAsync(false).Result;
            allFinancialAidFundFinancials = new TestFinancialAidFundRepository().GetFinancialAidFundFinancialsAsync(awardId, fundYears, "USA").Result;

            awardId = "0003914";

            fundYears = new List<string>() { "2008" };
            testDataRepository = new TestFinancialAidFundRepository();

            fundRepo = BuildValidFundRepository();
            valcodeName = fundRepo.BuildFullCacheKey("AllFinancialAidFunds");
        }

        [TestCleanup]
        public void Cleanup()
        {
            allFinancialAidFunds = null;
            valcodeName = string.Empty;
            apiSettings = null;
        }
        #endregion

        #region MOCK EVENTS
        private Student.Repositories.FinancialAidFundRepository BuildValidFundRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            // Set up data accessor for mocking 
            dataReaderMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

            var records = new Collection<DataContracts.Awards>();
            foreach (var item in allFinancialAidFunds)
            {
                DataContracts.Awards record = new DataContracts.Awards();
                record.RecordGuid = item.Guid;
                record.AwDescription = item.Description;
                record.Recordkey = item.Code;
                record.AwExplanationText = item.Description2;
                record.AwType = item.Source;
                record.AwCategory = item.CategoryCode;
                record.AwReportingFundingType = item.FundingType;
                records.Add(record);
            }
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Awards>("AWARDS", "", true)).ReturnsAsync(records);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
            {
                var result = new Dictionary<string, RecordKeyLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var record = allFinancialAidFunds.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                    result.Add(string.Join("+", new string[] { "AWARDS", record.Code }),
                        new RecordKeyLookupResult() { Guid = record.Guid });
                }
                return Task.FromResult(result);
            });

            var acyrRecords = new Collection<DataContracts.FundOfficeAcyr>();
            foreach (var item in allFinancialAidFundFinancials)
            {
                DataContracts.FundOfficeAcyr record = new DataContracts.FundOfficeAcyr();
                record.Recordkey = item.AwardCode + "*" + item.Office;
                record.FundofcBudgetAmt = (long)item.BudgetedAmount;
                //record.FundofcOverAmt = (long) item.MaximumOfferedBudgetAmount;
                //record.FundofcOverPct = null;
                acyrRecords.Add(record);
            }
            //dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.FundOfficeAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(acyrRecords);

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<FundOfficeAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((x, y, z) =>
                    Task.FromResult(new Collection<FundOfficeAcyr>(allFinancialAidFundFinancials.Select(a =>
                        new FundOfficeAcyr()
                        {
                            Recordkey = a.AwardCode + "*" + a.Office,
                            FundofcBudgetAmt = (long)a.BudgetedAmount,
                            //FundofcOverAmt = (long) item.MaximumOfferedBudgetAmount;
                            //FundofcOverPct = null;
                        }).ToList())));

            // Construct repository
            fundRepo = new FinancialAidFundRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return fundRepo;
        }


        #endregion

        #region GetFinancialAidFundsTests
        [TestClass]
        public class GetFinancialAidFundsTests : FinancialAidFundRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
            }

            #region TESTS FOR FUNCTIONALITY
            [TestMethod]
            public async Task ExpectedEqualsActualFinancialAidFundsTest()
            {
                var expected = await getExpectedFinancialAidFundsFinancials();
                var actual = await getActualFinancialAidFundsFinancials();
                Assert.AreEqual(expected.Count(), actual.Count());
            }

            [TestMethod]
            public async Task ActualFinancialAidFundsNoFundYearsTest()
            {
                var actual = await getActualFinancialAidFundsFinancialsNoYears();
                Assert.AreEqual(0, actual.Count());
            }

            [TestMethod]
            public async Task FinancialAidFundRepository_GetFinancialAidFundsAsync_False()
            {
                var results = await fundRepo.GetFinancialAidFundsAsync(false);
                Assert.AreEqual(allFinancialAidFunds.Count(), results.Count());

                foreach (var financialAidFund in allFinancialAidFunds)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidFund.Guid);

                    Assert.AreEqual(financialAidFund.Code, result.Code);
                    Assert.AreEqual(financialAidFund.Description, result.Description);
                    Assert.AreEqual(financialAidFund.Guid, result.Guid);
                }

            }
            #endregion
        }
        #endregion

    }
}