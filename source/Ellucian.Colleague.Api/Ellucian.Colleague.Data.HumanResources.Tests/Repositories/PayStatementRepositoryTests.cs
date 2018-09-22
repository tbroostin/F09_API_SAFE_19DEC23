/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PayStatementRepositoryTests : BaseRepositorySetup
    {
        public TestPayStatementRepository testData;

        public PayStatementRepository repository;


        public void PayStatementRepositoryTestsInitialize()
        {
            MockInitialize();
            testData = new TestPayStatementRepository();
            repository = BuildRepository();
        }

        public PayStatementRepository BuildRepository()
        {
            #region web pay advice mock setup
            dataReaderMock.Setup(d => d.ReadRecordAsync<WebPayAdvices>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((id, b) =>
                {
                    var payStatementRecord = testData.payStatementRecords.FirstOrDefault(wpa => id == wpa.recordKey);
                    if (payStatementRecord == null)
                    {
                        return Task.FromResult<WebPayAdvices>(null);
                    }
                    else
                    {
                        return Task.FromResult(BuildWebPayAdviceRecord(payStatementRecord));
                    }
                });

            dataReaderMock.Setup(d => d.SelectAsync("WEB.PAY.ADVICES", It.IsAny<string>()))
                .Returns<string, string>((f, q) => Task.FromResult(testData.payStatementRecords == null ? null : testData.payStatementRecords.Select(r => r.recordKey).ToArray()));

            dataReaderMock.Setup(d => d.SelectAsync("WEB.PAY.ADVICES", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, q, v, p, b, i) => Task.FromResult(testData.payStatementRecords == null ? null : testData.payStatementRecords.Select(r => r.recordKey).ToArray()));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<WebPayAdvices>("WEB.PAY.ADVICES", It.IsAny<string[]>(), It.IsAny<bool>()))
                //.Callback<string, string[], bool>((f, ids, b) =>
                //{
                //    if (testData.payStatementRecords != null)
                //    {
                //        //copy records with each person id
                //        testData.payStatementRecords = ids.SelectMany(id =>
                //        {
                //            testData.payStatementRecords.ForEach(ps => ps.employeeId = id.Split('"')[1]);
                //            return testData.payStatementRecords.ToList();
                //        }).ToList();
                //    }
                //})
                .Returns<string, string[], bool>((f, ids, b) => Task.FromResult(
                      new Collection<WebPayAdvices>(
                          testData.payStatementRecords
                            .Where(wpa => ids.Any(key => key == wpa.recordKey))
                            .Select(r =>  BuildWebPayAdviceRecord(r))
                            .ToList())));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<WebPayAdvices>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, c) => Task.FromResult(testData.payStatementRecords == null ? null :
                      new Collection<WebPayAdvices>(
                          testData.payStatementRecords
                            .Where(wpa => ids.Any(key => key == wpa.recordKey))
                            .Select(r => BuildWebPayAdviceRecord(r))
                            .ToList())));

            #endregion

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            return new PayStatementRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        public WebPayAdvices BuildWebPayAdviceRecord(TestPayStatementRepository.PayStatementRecord record)
        {
            return new WebPayAdvices()
            {
                Recordkey = record.recordKey,
                WpaEmployeId = record.employeeId,
                WpaCheckDate = record.wpaDate,
                WpaAdviceNumber = record.adviceNumber,
                WpaCheckNumber = record.checkNumber,
                WpaDirDepositDesc = record.dirDepDesc,
                WpaWordPrintAmt = record.wordPrintAmount,
                WpaNonNetCheckAmt = record.nonNetCheckAmount,
                WpaNumericPrintAmt = record.numericPrintAmount,
                WpaEarnedVacation = record.earnedVacation,
                WpaGrossPay = record.grossPay,
                WpaNetPay = record.netPay,
                WpaOtherLeaveBalance = record.otherLeaveBalance,
                WpaOtherLeaveEarned = record.otherLeaveEarned,
                WpaOtherLeaveUsed = record.otherLeaveUsed,
                WpaPeriodDate = record.periodDate,
                WpaPrimaryDept = record.primaryDepartment,
                WpaSsn = record.SSN,
                WpaTotalBendeds = record.totalBendeds,
                WpaTotalTaxes = record.totalTaxes,
                WpaTotalYtdBendeds = record.ytdBenDeds,
                WpaTotalYtdGrossPay = record.ytdGrossPay,
                WpaTotalYtdNetPay = record.ytdNetPay,
                WpaTotalYtdTaxes = record.ytdTaxes,
                WpaUsedVacation = record.usedVacation,
                WpaVacationBalance = record.vacationBalance,
                WpaMailLabel = record.mailingLabel,
                DirDepInfoEntityAssociation = record.deposits.Select(d =>
                    new WebPayAdvicesDirDepInfo()
                    {
                        WpaDirDepositDescsAssocMember = d.bankName,
                        WpaDirDepositTypesAssocMember = d.accountType,
                        WpaDirDepAcctsLast4AssocMember = d.last4,
                        WpaDirDepositAmtsAssocMember = d.amount
                    }).ToList()
            };
        }

        [TestClass]
        public class GetSinglePayStatementSourceData : PayStatementRepositoryTests
        {


            [TestInitialize]
            public void Initialize()
            {
                PayStatementRepositoryTestsInitialize();
                testData.CreatePayStatementRecords("0003914");
            }

            [TestMethod]
            public async Task PayStatementSourceDataByIdIsReturnedAsExpected()
            {
                var id = testData.payStatementRecords.First().recordKey;
                var expected = await testData.GetPayStatementSourceDataAsync(id);
                var actual = await repository.GetPayStatementSourceDataAsync(id);
                Assert.AreEqual(expected, actual);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayStatementSourceDataByIdRequiresIdTest()
            {
                await repository.GetPayStatementSourceDataAsync((string)null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayStatementSourceDataByIdRequiresValidIdTest()
            {
                var actual = await repository.GetPayStatementSourceDataAsync("FOO");
            }

            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task ErrorWhenBuildingPayStatementSourceDataTest()
            {
                testData.payStatementRecords.ForEach(p => p.periodDate = null);
                var id = testData.payStatementRecords.First().recordKey;
                await repository.GetPayStatementSourceDataAsync(id);
                loggerMock.Verify(e => e.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task NoErrorBuildingPayStatementSourceBankDepositsByIdTest()
            {

                foreach (var testPayStatementRecord in testData.payStatementRecords)
                {
                    foreach (var deposit in testPayStatementRecord.deposits)
                    {
                        deposit.bankName = null;
                    }
                }

                //try
                //{
                var id = testData.payStatementRecords.First().recordKey;
                    await repository.GetPayStatementSourceDataAsync(id);
                //}
                //catch (Exception ex)
                //{
                //    Assert.IsInstanceOfType(ex.InnerException,typeof(ArgumentNullException));
                //}
            }


        }

        [TestClass]
        public class GetPayStatementSourceDataByPersonId : PayStatementRepositoryTests
        {

            public string inputPersonId;

            [TestInitialize]
            public void Initialize()
            {
                PayStatementRepositoryTestsInitialize();

                inputPersonId = "0003914";

                testData.CreatePayStatementRecords("0003914");
            }

            [TestMethod]
            public async Task PayStatementSourceDataByPersonIdIsReturnedAsExpected()
            {
                var personId = "12345";
                var expected = await testData.GetPayStatementSourceDataByPersonIdAsync(personId, null, null);
                var actual = await repository.GetPayStatementSourceDataByPersonIdAsync(personId, null, null);
                CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayStatementSourceDataByPersonIdRequiresPersonIdTest()
            {
                await repository.GetPayStatementSourceDataByPersonIdAsync((string)null, null, null);
            }

            [TestMethod]
            public async Task PayStatementSourceDataByPersonIdReturnsEmptyListTest()
            {
                testData.payStatementRecords = new List<TestPayStatementRepository.PayStatementRecord>();
                var expected = new List<PayStatementSourceData>();
                var actual = await repository.GetPayStatementSourceDataByPersonIdAsync("FOO", null, null);
                CollectionAssert.AreEqual(expected, actual.ToList());
            }

            [TestMethod]
            public async Task LogErrorBuildingPayStatementSourceBankDepositsByPersonIdTest()
            {
                foreach (var testPayStatementRecord in testData.payStatementRecords)
                {
                    testPayStatementRecord.recordKey = null;
                }
                await repository.GetPayStatementSourceDataByPersonIdAsync("54321", null, null);
                loggerMock.Verify(e => e.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }

        [TestClass]
        public class GetPayStatementSourceDataByPersonIds : PayStatementRepositoryTests
        {

            public List<string> inputPersonIds;

            public bool simulateCache;

            public async Task<IEnumerable<PayStatementSourceData>> getActual()
            {
                return await repository.GetPayStatementSourceDataByPersonIdAsync(inputPersonIds, null, null);
            }

            [TestInitialize]
            public void Initialize()
            {
                PayStatementRepositoryTestsInitialize();

                inputPersonIds = new List<string>() { "0003914", "0004451" };

                inputPersonIds.ForEach(personId => testData.CreatePayStatementRecords(personId));

 

                simulateCache = false;

                cacheProviderMock.Setup(c => c.Contains(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>((key, region) =>
                        simulateCache);

                cacheProviderMock.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>((key, region) =>
                    {
                        var personId = inputPersonIds.FirstOrDefault(id => key.IndexOf(id) >= 0);                    
                        return personId == null ? null : testData.GetPayStatementSourceDataByPersonId(personId, null, null);
                    });
            }

            [TestMethod]
            public async Task PayStatementsForMultipleIdsTest()
            {
                var actual = await getActual();

                Assert.IsTrue(actual.Any(a => a.EmployeeId == inputPersonIds[0]));
                Assert.IsTrue(actual.Any(a => a.EmployeeId == inputPersonIds[1]));
            }

            [TestMethod]
            public async Task RecordsRetreivedFromDatabaseTest()
            {
                simulateCache = false;
                var actual = await getActual();


                cacheProviderMock.Verify(c => c.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [TestMethod]
            public async Task RecordsFromDatabaseAddedToCacheTest()
            {
                simulateCache = false;
                var actual = await getActual();

                cacheProviderMock.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<IEnumerable<PayStatementSourceData>>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task RecordsRetrievedFromCacheTest()
            {
                simulateCache = true;
                var actual = await getActual();

                dataReaderMock.Verify(d => d.BulkReadRecordAsync<WebPayAdvices>(It.IsAny<string[]>(), It.IsAny<bool>()), Times.Never);
            }

            [TestMethod]
            public async Task NullRecordsInCacheTest()
            {
                simulateCache = true;
                testData.payStatementRecords = null;

                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task NullRecordsInDatabase()
            {
                simulateCache = false;
                testData.payStatementRecords = null;

                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonIdsRequiredTest()
            {
                inputPersonIds = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonIdsEmptyTest()
            {
                inputPersonIds = new List<string>();
                await getActual();
            }




        }

        [TestClass]
        public class GetMultiplePayStatementSourceData : PayStatementRepositoryTests
        {
            public List<string> inputIds;

            public async Task<IEnumerable<PayStatementSourceData>> getActual()
            {
                return await repository.GetPayStatementSourceDataAsync(inputIds);
            }

            [TestInitialize]
            public void Initialize()
            {
                PayStatementRepositoryTestsInitialize();
                testData.CreatePayStatementRecords("0003914");
                inputIds = testData.payStatementRecords.Select(ps => ps.recordKey).ToList();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InputIdsRequiredTest()
            {
                inputIds = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InputIdsRequiredItemsTest()
            {
                inputIds = new List<string>();
                await getActual();
            }

            [TestMethod]
            public async Task NullRecordsTest()
            {
                testData.payStatementRecords = null;
                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task EmptyRecordsTest()
            {
                testData.payStatementRecords = new List<TestPayStatementRepository.PayStatementRecord>();
                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task NumberOfObjectsMatchesNumberOfInputIdsTest()
            {
                var actual = await getActual();
                Assert.AreEqual(inputIds.Count(), actual.Count());
            }

            [TestMethod]
            public async Task InvalidRecordsAreHandledTest()
            {
                testData.payStatementRecords.ForEach(ps => ps.periodDate = null);
                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }
        }
    }
}
