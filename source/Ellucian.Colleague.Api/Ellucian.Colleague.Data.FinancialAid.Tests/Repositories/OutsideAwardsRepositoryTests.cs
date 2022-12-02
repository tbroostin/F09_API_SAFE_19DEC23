/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Moq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using System.Collections.ObjectModel;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class OutsideAwardsRepositoryTests : BaseRepositorySetup
    {
        public TestOutsideAwardsRepository testOutsideAwardsRepository;
        public OutsideAwardsRepository actualOutsideAwardsRepository;

        public OutsideAward inputOutsideAward;
        public OutsideAward actualOutsideAward;
        public OutsideAward updatedOutsideAward;

        public Collection<FaOutsideAwards> outsideAwardDataContracts;
        public FaOutsideAwards updatedOutsideAwardDataContract;

        public CreateOutsideAwardResponse createOutsideAwardResponse;
        public CreateOutsideAwardRequest createOutsideAwardRequest;

        public DeleteOutsideAwardRequest deleteOutsideAwardRequest;
        public DeleteOutsideAwardResponse deleteOutsideAwardResponse;

        public UpdateOutsideAwardRequest updateOutsideAwardRequest;
        public UpdateOutsideAwardResponse updateOutsideAwardResponse;

        public string studentId;
        public string awardYearCode;
        
        public void BaseInitialize()
        {
            MockInitialize();

            testOutsideAwardsRepository = new TestOutsideAwardsRepository();

            outsideAwardDataContracts = BuildOutsideAwardDataContracts(testOutsideAwardsRepository.outsideAwardRecords);
            updatedOutsideAwardDataContract = BuildUpdatedOutsideAwardDataContract(testOutsideAwardsRepository.updateOutsideAwardRecord);
            
            createOutsideAwardResponse = new CreateOutsideAwardResponse();
            deleteOutsideAwardResponse = new DeleteOutsideAwardResponse();
            updateOutsideAwardResponse = new UpdateOutsideAwardResponse();

            BuildActualRepository();
        }

        public void BaseCleanup()
        {
            testOutsideAwardsRepository = null;
            outsideAwardDataContracts = null;
            createOutsideAwardResponse = null;
            actualOutsideAwardsRepository = null;
            updatedOutsideAwardDataContract = null;
            transManagerMock = null;
            loggerMock = null;
            cacheProviderMock = null;
            deleteOutsideAwardRequest = null;
            deleteOutsideAwardResponse = null;
            updateOutsideAwardResponse = null;
            updateOutsideAwardRequest = null;
        }

        public Collection<FaOutsideAwards> BuildOutsideAwardDataContracts(List<TestOutsideAwardsRepository.OutsideAwardRecord> outsideAwardRecords)
        {
            Collection<FaOutsideAwards> dataContracts = new Collection<FaOutsideAwards>();
            foreach (var record in outsideAwardRecords)
            {
                dataContracts.Add(new FaOutsideAwards()
                {
                    Recordkey = record.recordId,
                    FoaStudentId = record.studentId,
                    FoaYear = record.awardYear,
                    FoaAwardName = record.awardName,
                    FoaAwardType = record.awardType,
                    FoaAmount = record.awardAmount,
                    FoaFundingSource = record.fundingSource
                });
            }
            return dataContracts;
        }

        public FaOutsideAwards BuildUpdatedOutsideAwardDataContract(TestOutsideAwardsRepository.OutsideAwardRecord updatedOutsideAwardRecord)
        {
            FaOutsideAwards dataContract = new FaOutsideAwards()
            {
                Recordkey = updatedOutsideAwardRecord.recordId,
                FoaStudentId = updatedOutsideAwardRecord.studentId,
                FoaYear = updatedOutsideAwardRecord.awardYear,
                FoaAwardName = updatedOutsideAwardRecord.awardName,
                FoaAwardType = updatedOutsideAwardRecord.awardType,
                FoaAmount = updatedOutsideAwardRecord.awardAmount,
                FoaFundingSource = updatedOutsideAwardRecord.fundingSource
            };
            return dataContract;
        }

        public void BuildActualRepository()
        {
            transManagerMock.Setup(tm => tm.ExecuteAsync<CreateOutsideAwardRequest, CreateOutsideAwardResponse>(It.IsAny<CreateOutsideAwardRequest>()))
                .Returns(Task.FromResult(createOutsideAwardResponse))
                .Callback<CreateOutsideAwardRequest>(r => createOutsideAwardRequest = r);

            transManagerMock.Setup(tm => tm.ExecuteAsync<DeleteOutsideAwardRequest, DeleteOutsideAwardResponse>(It.IsAny<DeleteOutsideAwardRequest>()))
                .ReturnsAsync(deleteOutsideAwardResponse)
                .Callback<DeleteOutsideAwardRequest>(r => deleteOutsideAwardRequest = r);

            transManagerMock.Setup(tm => tm.ExecuteAsync<UpdateOutsideAwardRequest, UpdateOutsideAwardResponse>(It.IsAny<UpdateOutsideAwardRequest>()))
                .Returns(Task.FromResult(updateOutsideAwardResponse))
                .Callback<UpdateOutsideAwardRequest>(r => updateOutsideAwardRequest = r);

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<FaOutsideAwards>(It.IsAny<string>(), true))
                .Returns<string, bool>((recordId, b) =>
                {
                    FaOutsideAwards dataContract = outsideAwardDataContracts.FirstOrDefault(dc => dc.Recordkey == recordId);
                    return Task.FromResult((dataContract == null) ? null : dataContract);
                });

            dataReaderMock.Setup<Task<Collection<FaOutsideAwards>>>(dr => dr.BulkReadRecordAsync<FaOutsideAwards>(It.IsAny<string>(), true))
                .Returns<string, bool>((criteria, b) =>
                {
                    var contractsForYear = outsideAwardDataContracts.Where(a => a.FoaStudentId == studentId && a.FoaYear == awardYearCode);
                    Collection<FaOutsideAwards> outsideAwards = new Collection<FaOutsideAwards>();
                    foreach (var contract in contractsForYear)
                    {
                        outsideAwards.Add(contract);
                    }
                    return Task.FromResult(outsideAwards);
                });

            actualOutsideAwardsRepository = new OutsideAwardsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class CreateOutsideAwardTests : OutsideAwardsRepositoryTests
        {
            private OutsideAward expectedOutsideAward;            

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                
                FaOutsideAwards outsideAwardRecord = outsideAwardDataContracts.First();
                inputOutsideAward = new OutsideAward(outsideAwardRecord.Recordkey, outsideAwardRecord.FoaStudentId, outsideAwardRecord.FoaYear, 
                    outsideAwardRecord.FoaAwardName, outsideAwardRecord.FoaAwardType, outsideAwardRecord.FoaAmount.Value, outsideAwardRecord.FoaFundingSource);

                createOutsideAwardResponse.OutOutsideAwardId = inputOutsideAward.Id;

                expectedOutsideAward = await testOutsideAwardsRepository.CreateOutsideAwardAsync(inputOutsideAward);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task OutsideAwardIsNotNullTest()
            {
                actualOutsideAward = await actualOutsideAwardsRepository.CreateOutsideAwardAsync(inputOutsideAward);
                Assert.IsNotNull(actualOutsideAward);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullInputOutsideAward_ThrowsArgumentNullExceptionTest()
            {
                await actualOutsideAwardsRepository.CreateOutsideAwardAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task NullReturnedRecordId_ThrowsExceptionTest()
            {
                createOutsideAwardResponse.OutOutsideAwardId = null;
                await actualOutsideAwardsRepository.CreateOutsideAwardAsync(inputOutsideAward);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoRecordWithSpecifiedId_ThrowsKeyNotFoundExceptionTest()
            {
                createOutsideAwardResponse.OutOutsideAwardId = "foo";
                await actualOutsideAwardsRepository.CreateOutsideAwardAsync(inputOutsideAward);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ErrorRecreatingRetrievedRecord_ThrowsApplicationExceptionTest(){
                var awardRecord = outsideAwardDataContracts.First(ar => ar.Recordkey == inputOutsideAward.Id);
                awardRecord.FoaAwardName = string.Empty;
                await actualOutsideAwardsRepository.CreateOutsideAwardAsync(inputOutsideAward);
            }

            [TestMethod]
            public async Task ActualCreatedOutsideAward_EqualsExpectedTest()
            {
                actualOutsideAward = await actualOutsideAwardsRepository.CreateOutsideAwardAsync(inputOutsideAward);
                Assert.AreEqual(expectedOutsideAward.Id, actualOutsideAward.Id);
                Assert.AreEqual(expectedOutsideAward.StudentId, actualOutsideAward.StudentId);
                Assert.AreEqual(expectedOutsideAward.AwardYearCode, actualOutsideAward.AwardYearCode);
                Assert.AreEqual(expectedOutsideAward.AwardName, actualOutsideAward.AwardName);
                Assert.AreEqual(expectedOutsideAward.AwardType, actualOutsideAward.AwardType);
                Assert.AreEqual(expectedOutsideAward.AwardAmount, actualOutsideAward.AwardAmount);
                Assert.AreEqual(expectedOutsideAward.AwardFundingSource, actualOutsideAward.AwardFundingSource);
            }
            
        }

        [TestClass]
        public class GetOutsideAwardsAsync : OutsideAwardsRepositoryTests
        {
            private List<OutsideAward> expectedOutsideAwards;
            private List<OutsideAward> actualOutsideAwards;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                studentId = "0003914";
                awardYearCode = outsideAwardDataContracts.First().FoaYear;
                expectedOutsideAwards = (await testOutsideAwardsRepository.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
                expectedOutsideAwards = null;
                actualOutsideAwards = null;
            }

            [TestMethod]
            public async Task ActualOutsideAwards_AreNotNullTest()
            {
                actualOutsideAwards = (await actualOutsideAwardsRepository.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                Assert.IsNotNull(actualOutsideAwards);
            }

            [TestMethod]
            public async Task ActualOutsideAwards_IsNotEmptyListTest()
            {
                actualOutsideAwards = (await actualOutsideAwardsRepository.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                Assert.IsTrue(actualOutsideAwards.Any());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ArgumentNullExceptionThrownTest()
            {
                await actualOutsideAwardsRepository.GetOutsideAwardsAsync(null, awardYearCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardYearCode_ArgumentNullExceptionThrownTest()
            {
                await actualOutsideAwardsRepository.GetOutsideAwardsAsync(studentId, null);
            }

            [TestMethod]
            public async Task NoOutsideAwardRecordsFound_EmptyEntityListReturnedTest()
            {
                dataReaderMock.Setup<Task<Collection<FaOutsideAwards>>>(dr => dr.BulkReadRecordAsync<FaOutsideAwards>(It.IsAny<string>(), true))
                    .ReturnsAsync(new Collection<FaOutsideAwards>());

                actualOutsideAwardsRepository = new OutsideAwardsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                actualOutsideAwards = (await actualOutsideAwardsRepository.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                Assert.IsFalse(actualOutsideAwards.Any());
            }

            [TestMethod]
            public async Task NullOutsideAwardRecordsFound_EmptyEntityListReturnedTest()
            {
                dataReaderMock.Setup<Task<Collection<FaOutsideAwards>>>(dr => dr.BulkReadRecordAsync<FaOutsideAwards>(It.IsAny<string>(), true))
                    .ReturnsAsync((Collection<FaOutsideAwards>)null);

                actualOutsideAwardsRepository = new OutsideAwardsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                actualOutsideAwards = (await actualOutsideAwardsRepository.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                Assert.IsFalse(actualOutsideAwards.Any());
            }

            [TestMethod]
            public async Task ActualOutsideAwardsCount_EqualsExpectedTest()
            {
                actualOutsideAwards = (await actualOutsideAwardsRepository.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                Assert.AreEqual(expectedOutsideAwards.Count, actualOutsideAwards.Count());
            }

            [TestMethod]
            public async Task ActualOutsideAwards_EqualExpectedOnesTest()
            {
                actualOutsideAwards = (await actualOutsideAwardsRepository.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                foreach (var expectedAward in expectedOutsideAwards)
                {
                    var actualAward = actualOutsideAwards.First(a => a.Id == expectedAward.Id);
                    Assert.AreEqual(expectedAward.StudentId, actualAward.StudentId);
                    Assert.AreEqual(expectedAward.AwardName, actualAward.AwardName);
                    Assert.AreEqual(expectedAward.AwardType, actualAward.AwardType);
                    Assert.AreEqual(expectedAward.AwardAmount, actualAward.AwardAmount);
                    Assert.AreEqual(expectedAward.AwardYearCode, actualAward.AwardYearCode);
                    Assert.AreEqual(expectedAward.AwardFundingSource, actualAward.AwardFundingSource);
                }
            }
        }

        [TestClass]
        public class DeleteOutsideAwardAsync : OutsideAwardsRepositoryTests
        {
            private string recordId;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                recordId = outsideAwardDataContracts.First().Recordkey;
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullRecordId_ArgumentNullExceptionThrownTest()
            {
                await actualOutsideAwardsRepository.DeleteOutsideAwardAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoRecordWithSpecifiedId_KeyNotFoundExceptionThrownTest()
            {
                deleteOutsideAwardResponse.ErrorCode = "OutsideAward.MissingRecord";
                await actualOutsideAwardsRepository.DeleteOutsideAwardAsync("foo");               
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task TransactionError_ApplicationExceptionThrownTest()
            {
                deleteOutsideAwardResponse.ErrorCode = "any code";
                await actualOutsideAwardsRepository.DeleteOutsideAwardAsync(recordId);
            }

            [TestMethod]
            public async Task NoTransactionErrors_NoExceptionsThrownTest()
            {
                bool exceptionThrown = false;
                try
                {
                    await actualOutsideAwardsRepository.DeleteOutsideAwardAsync(recordId);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }
        }
        
        [TestClass]
        public class UpdateOutsideAwardAsync : OutsideAwardsRepositoryTests
        {
            private string recordId;
            private OutsideAward expectedOutsideAward;


            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                recordId = updatedOutsideAwardDataContract.Recordkey;

                FaOutsideAwards outsideAwardRecord = updatedOutsideAwardDataContract;

                updatedOutsideAward = new OutsideAward(outsideAwardRecord.Recordkey, outsideAwardRecord.FoaStudentId, outsideAwardRecord.FoaYear,
                    outsideAwardRecord.FoaAwardName, outsideAwardRecord.FoaAwardType, outsideAwardRecord.FoaAmount.Value, outsideAwardRecord.FoaFundingSource);

                updateOutsideAwardResponse.OutsideAwardId = updatedOutsideAward.Id;           

                expectedOutsideAward = await testOutsideAwardsRepository.UpdateOutsideAwardAsync(updatedOutsideAward);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullUpdatedOutsideAward_ThrowsArgumentNullExceptionTest()
            {
                await actualOutsideAwardsRepository.UpdateOutsideAwardAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateNullReturnedRecordId_ThrowsExceptionTest()
            {
                updateOutsideAwardResponse.OutsideAwardId = null;
                await actualOutsideAwardsRepository.UpdateOutsideAwardAsync(updatedOutsideAward);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateNoRecordWithSpecifiedId_ThrowsKeyNotFoundExceptionTest()
            {
                updateOutsideAwardResponse.OutsideAwardId = "foo";
                await actualOutsideAwardsRepository.UpdateOutsideAwardAsync(updatedOutsideAward);
            }

            [TestMethod]
            public async Task ActualUpdateOutsideAward_EqualsExpectedTest()
            {
                actualOutsideAward = await actualOutsideAwardsRepository.UpdateOutsideAwardAsync(updatedOutsideAward);
                Assert.AreEqual(expectedOutsideAward.Id, actualOutsideAward.Id);
                Assert.AreEqual(expectedOutsideAward.StudentId, actualOutsideAward.StudentId);
                Assert.AreEqual(expectedOutsideAward.AwardYearCode, actualOutsideAward.AwardYearCode);
                Assert.AreEqual(expectedOutsideAward.AwardName, actualOutsideAward.AwardName);
                Assert.AreEqual(expectedOutsideAward.AwardType, actualOutsideAward.AwardType);
                Assert.AreEqual(expectedOutsideAward.AwardFundingSource, actualOutsideAward.AwardFundingSource);

                // This will have the actual set to the original amount of 2000 as defined in the test data
                // and the expected set to 1500 as defined in the test data. Looking for a difference here
                // so this should ASSERT.ARENOTEQUAL. This is because in the repository it goes back and re-reads
                // the original records to return.
               
                Assert.AreNotEqual(expectedOutsideAward.AwardAmount, actualOutsideAward.AwardAmount);
            }




        }
    }
}
