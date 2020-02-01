// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.BudgetManagement.DataContracts;
using Ellucian.Colleague.Data.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.BudgetManagement.Tests.Repositories
{
    [TestClass]
    public class BudgetDevelopmentConfigurationRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup

        private BudgetConfigurationRepository actualRepository;
        private TestBudgetDevelopmentConfigurationRepository testBuDevConfigRepository;
        private BudgetDevDefaults buDevConfigRecord;
        private Budget budgetRecord;
        private BudgetConfiguration buDevConfigEntity;
        private List<BudgetConfigurationComparable> comparableList;

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();

            actualRepository = new BudgetConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            testBuDevConfigRepository = new TestBudgetDevelopmentConfigurationRepository();
            buDevConfigRecord = testBuDevConfigRepository.BudgetDevDefaultsContract;
            budgetRecord = testBuDevConfigRepository.BudgetContract;
            comparableList = new List<BudgetConfigurationComparable>();

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudgetDevDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.testBuDevConfigRepository.BudgetDevDefaultsContract);
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<Budget>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.testBuDevConfigRepository.BudgetContract);
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
            testBuDevConfigRepository = null;
            buDevConfigRecord = null;
            budgetRecord = null;
            buDevConfigEntity = null;
            comparableList = null;
        }
        #endregion

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_Success()
        {
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();

            comparableList = GetListOfComparables(buDevConfigEntity);
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, budgetRecord.BuTitle);
            Assert.AreEqual(buDevConfigEntity.BudgetYear, budgetRecord.BuBaseYear);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Working);
            int seq = 0;
            for (int i = 0; i < 5; i++)
            {
                int? year = null;
                string header = string.Empty;
                switch (i)
                {
                    case 0:
                        year = budgetRecord.BuComp1Year;
                        header = budgetRecord.BuComp1Heading;
                        break;
                    case 1:
                        year = budgetRecord.BuComp2Year;
                        header = budgetRecord.BuComp2Heading;
                        break;
                    case 2:
                        year = budgetRecord.BuComp3Year;
                        header = budgetRecord.BuComp3Heading;
                        break;
                    case 3:
                        year = budgetRecord.BuComp4Year;
                        header = budgetRecord.BuComp4Heading;
                        break;
                    case 4:
                        year = budgetRecord.BuComp5Year;
                        header = budgetRecord.BuComp5Heading;
                        break;
                }

                if (!string.IsNullOrWhiteSpace(header))
                {
                    var comparable = comparableList.Find(x => x.ComparableYear == year.ToString() && x.ComparableHeader == header);
                    seq = seq + 1;
                    Assert.AreEqual(comparable.ComparableHeader, header);
                    Assert.AreEqual(comparable.ComparableId, "C" + seq);
                    Assert.AreEqual(comparable.SequenceNumber, seq);
                    if (year != null)
                    {
                        Assert.AreEqual(comparable.ComparableYear, year.ToString());
                    }
                    else if (year == null)
                    {
                        Assert.AreEqual(comparable.ComparableYear, string.Empty);
                    }
                }
            }
            Assert.AreEqual(comparableList.Count, seq);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NoHeadings()
        {
            budgetRecord.BuComp1Heading = null;
            budgetRecord.BuComp2Heading = null;
            budgetRecord.BuComp3Heading = null;
            budgetRecord.BuComp4Heading = null;
            budgetRecord.BuComp5Heading = null;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();

            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, budgetRecord.BuTitle);
            Assert.AreEqual(buDevConfigEntity.BudgetYear, budgetRecord.BuBaseYear);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Working);
            Assert.AreEqual(buDevConfigEntity.BudgetConfigurationComparables.Count, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NullC1Heading()
        {
            budgetRecord.BuComp1Heading = null;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();

            comparableList = GetListOfComparables(buDevConfigEntity);
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, budgetRecord.BuTitle);
            Assert.AreEqual(buDevConfigEntity.BudgetYear, budgetRecord.BuBaseYear);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Working);
            int seq = 0;
            for (int i = 0; i < 5; i++)
            {
                int? year = null;
                string header = string.Empty;
                switch (i)
                {
                    case 0:
                        year = budgetRecord.BuComp1Year;
                        header = budgetRecord.BuComp1Heading;
                        break;
                    case 1:
                        year = budgetRecord.BuComp2Year;
                        header = budgetRecord.BuComp2Heading;
                        break;
                    case 2:
                        year = budgetRecord.BuComp3Year;
                        header = budgetRecord.BuComp3Heading;
                        break;
                    case 3:
                        year = budgetRecord.BuComp4Year;
                        header = budgetRecord.BuComp4Heading;
                        break;
                    case 4:
                        year = budgetRecord.BuComp5Year;
                        header = budgetRecord.BuComp5Heading;
                        break;
                }

                if (!string.IsNullOrWhiteSpace(header))
                {
                    var comparable = comparableList.Find(x => x.ComparableYear == year.ToString() && x.ComparableHeader == header);
                    seq = seq + 1;
                    Assert.AreEqual(comparable.ComparableHeader, header);
                    Assert.AreEqual(comparable.ComparableId, "C" + seq);
                    Assert.AreEqual(comparable.SequenceNumber, seq);
                    if (year != null)
                    {
                        Assert.AreEqual(comparable.ComparableYear, year.ToString());
                    }
                    else if (year == null)
                    {
                        Assert.AreEqual(comparable.ComparableYear, string.Empty);
                    }
                }
            }
            Assert.AreEqual(comparableList.Count, seq);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NullC3HeadingC2Year()
        {
            budgetRecord.BuComp2Year = null;
            budgetRecord.BuComp3Heading = null;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();

            comparableList = GetListOfComparables(buDevConfigEntity);
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, budgetRecord.BuTitle);
            Assert.AreEqual(buDevConfigEntity.BudgetYear, budgetRecord.BuBaseYear);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Working);
            int seq = 0;
            for (int i = 0; i < 5; i++)
            {
                int? year = null;
                string header = string.Empty;
                switch (i)
                {
                    case 0:
                        year = budgetRecord.BuComp1Year;
                        header = budgetRecord.BuComp1Heading;
                        break;
                    case 1:
                        year = budgetRecord.BuComp2Year;
                        header = budgetRecord.BuComp2Heading;
                        break;
                    case 2:
                        year = budgetRecord.BuComp3Year;
                        header = budgetRecord.BuComp3Heading;
                        break;
                    case 3:
                        year = budgetRecord.BuComp4Year;
                        header = budgetRecord.BuComp4Heading;
                        break;
                    case 4:
                        year = budgetRecord.BuComp5Year;
                        header = budgetRecord.BuComp5Heading;
                        break;
                }

                if (!string.IsNullOrWhiteSpace(header))
                {
                    var comparable = comparableList.Find(x => x.ComparableYear == year.ToString() && x.ComparableHeader == header);
                    seq = seq + 1;
                    Assert.AreEqual(comparable.ComparableHeader, header);
                    Assert.AreEqual(comparable.ComparableId, "C" + seq);
                    Assert.AreEqual(comparable.SequenceNumber, seq);
                    if (year != null)
                    {
                        Assert.AreEqual(comparable.ComparableYear, year.ToString());
                    }
                    else if (year == null)
                    {
                        Assert.AreEqual(comparable.ComparableYear, string.Empty);
                    }
                }
            }
            Assert.AreEqual(comparableList.Count, seq);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NoConfigRecord()
        {
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudgetDevDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(null as BudgetDevDefaults);
            });

            var expectedMessage = "BUDGET.DEV.DEFAULTS record does not exist.";
            var actualMessage = "";
            try
            {
                buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            }
            catch (Domain.Base.Exceptions.ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetDevelopmentConfigurationAsync_NullBudgetIdInConfigRecord()
        {
            this.buDevConfigRecord.BudDevBudget = null;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetBudgetDevelopmentConfigurationAsync_NoBudgetRecord()
        {
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<Budget>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(null as Budget);
            });
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NullTitle()
        {
            this.budgetRecord.BuTitle = null;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, string.Empty);
            Assert.AreEqual(buDevConfigEntity.BudgetYear, budgetRecord.BuBaseYear);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Working);

        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_EmptyTitle()
        {
            this.budgetRecord.BuTitle = string.Empty;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, string.Empty);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Working);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NullYear()
        {
            this.budgetRecord.BuBaseYear = null;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, budgetRecord.BuTitle);
            Assert.AreEqual(buDevConfigEntity.BudgetYear, string.Empty);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Working);

        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_EmptyYear()
        {
            this.budgetRecord.BuBaseYear = string.Empty;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetYear, string.Empty);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Working);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NewStatus()
        {
            this.budgetRecord.BuStatus = "N";
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, budgetRecord.BuTitle);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.New);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NewStatusLowercase()
        {
            this.budgetRecord.BuStatus = "n";
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, budgetRecord.BuTitle);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.New);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_GeneratedStatus()
        {
            this.budgetRecord.BuStatus = "G";
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, budgetRecord.BuTitle);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Generated);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_PostedStatus()
        {
            this.budgetRecord.BuStatus = "P";
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigRecord.BudDevBudget);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, budgetRecord.BuTitle);
            Assert.AreEqual(buDevConfigEntity.BudgetStatus, BudgetStatus.Posted);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetDevelopmentConfigurationAsync_NullStatus()
        {
            this.budgetRecord.BuStatus = null;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetDevelopmentConfigurationAsync_EmptyStatus()
        {
            this.budgetRecord.BuStatus = string.Empty;
            buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_OddStatus()
        {
            this.budgetRecord.BuStatus = "xyz";

            var expectedMessage = "Invalid status for budget: " + budgetRecord.Recordkey;

            var actualMessage = "";
            try
            {
                buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_BlankStatus()
        {
            this.budgetRecord.BuStatus = "";

            var expectedMessage = "Missing status for budget: " + budgetRecord.Recordkey;
            var actualMessage = "";
            try
            {
                buDevConfigEntity = await this.actualRepository.GetBudgetDevelopmentConfigurationAsync();
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        #region Private methods

        public List<BudgetConfigurationComparable> GetListOfComparables(BudgetConfiguration configEntity)
        {
            var compList = new List<BudgetConfigurationComparable>();
            foreach (var comparable in configEntity.BudgetConfigurationComparables)
            {
                compList.Add(comparable);
            }
            return compList;
        }

        #endregion
    }
}