//Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    /// <summary>
    /// Test setup for AwardLetterHistoryRepopsitory
    /// </summary>
    [TestClass]    
    public class AwardLetterHistoryRepositoryTests : BaseRepositorySetup
    {
        //Data contacts and transactions
        public Collection<AwardLetterHistory> awardLetterHistoryData;
        public List<TestAwardLetterHistoryRepository.AwardLetterParamsTransaction> paramsTransactionData;
        public Collection<AltrParameters> altrParameters;

        //Responses
        public UpdateAwardLetterSignedDateResponse updateAwardLetterSignedDateResponseData;
        public CreateAwardLetterHistoryResponse createAwardLetterResponseData;

        //Requests
        public Transactions.UpdateAwardLetterSignedDateRequest actualUpdateRequest;
        public Transactions.CreateAwardLetterHistoryRequest actualCreateRequest;
        public Transactions.EvalAwardLetterParamsRuleTableRequest actualEvalRequest;
        
        //Student Id used throughout
        public string studentId;
        public string awardYear;
        private IEnumerable<StudentAwardYear> studentAwardYears;
        private IEnumerable<Award> allAwards;        
          
        //Test repositories
        public TestAwardLetterHistoryRepository expectedRepository;
        public TestFinancialAidOfficeRepository officeRepository;
        public TestStudentAwardYearRepository studentAwardYearRepository;
        public TestFinancialAidReferenceDataRepository referenceRepository;
        
        
        public AwardLetterHistoryRepository actualRepository;        
        public CurrentOfficeService currentOfficeService;


        public void BaseInitialize()
        {
            MockInitialize();

            studentId = "0003914";
            expectedRepository = new TestAwardLetterHistoryRepository();
            officeRepository = new TestFinancialAidOfficeRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
            referenceRepository = new TestFinancialAidReferenceDataRepository();

            studentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
            allAwards = referenceRepository.Awards;

            actualRepository = BuildAwardLetterHistoryRepository();
        }

        public AwardLetterHistoryRepository BuildAwardLetterHistoryRepository()
        {
            expectedRepository = new TestAwardLetterHistoryRepository();
            paramsTransactionData = expectedRepository.paramsTransactionData;
            BuildAwardLetterHistoryResponseData(expectedRepository.awardLetterHistoryData);
            altrParameters = BuildAltrParametersDataContracts(expectedRepository.awardLetterParameterData);

            updateAwardLetterSignedDateResponseData = new UpdateAwardLetterSignedDateResponse() { ErrorMessage = ""};
            createAwardLetterResponseData = new CreateAwardLetterHistoryResponse() { };
            
            dataReaderMock.Setup<Task<Collection<AwardLetterHistory>>>(alh => alh.BulkReadRecordAsync<AwardLetterHistory>(It.IsAny<string>(), true)).Returns(Task.FromResult(awardLetterHistoryData));
            
            dataReaderMock.Setup<Task<Collection<AltrParameters>>>(r => r.BulkReadRecordAsync<AltrParameters>(It.IsAny<string>(), false)).Returns(Task.FromResult(altrParameters));

            //returns an empty response since the repository doesn't do anything with it.
            //also capturing the submitted request for testing
            transManagerMock.Setup(t => t.ExecuteAsync<UpdateAwardLetterSignedDateRequest, UpdateAwardLetterSignedDateResponse>(It.IsAny<UpdateAwardLetterSignedDateRequest>())
                ).Returns(Task.FromResult(updateAwardLetterSignedDateResponseData)
                ).Callback<UpdateAwardLetterSignedDateRequest>(
                    req =>
                    {
                        actualUpdateRequest = req;
                    });
            transManagerMock.Setup(t => t.ExecuteAsync<CreateAwardLetterHistoryRequest, CreateAwardLetterHistoryResponse>(It.IsAny<CreateAwardLetterHistoryRequest>())
                ).Returns(Task.FromResult(createAwardLetterResponseData))
                .Callback<CreateAwardLetterHistoryRequest>(
                   req =>
                   {
                       actualCreateRequest = req;
                   }
                );

            transManagerMock.Setup(t => t.ExecuteAsync<EvalAwardLetterParamsRuleTableRequest, EvalAwardLetterParamsRuleTableResponse>(It.IsAny<EvalAwardLetterParamsRuleTableRequest>()))
                .Returns <EvalAwardLetterParamsRuleTableRequest>(
                (request) => 
                    Task.FromResult(new EvalAwardLetterParamsRuleTableResponse() { Result = paramsTransactionData.First(p => p.Year == request.Year).Result }))
                .Callback<EvalAwardLetterParamsRuleTableRequest>(
                req =>
                {
                    actualEvalRequest = req;
                });

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            //Return the mocked repository
            return new AwardLetterHistoryRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        private Collection<AltrParameters> BuildAltrParametersDataContracts(List<TestAwardLetterHistoryRepository.AwardLetterParameter> awardLetterParameters)
        {
            Collection<AltrParameters> altrParametersDataContracts = new Collection<AltrParameters>();
            foreach (var paramRecord in awardLetterParameters)
            {
                altrParametersDataContracts.Add(
                    new AltrParameters()
                    {
                        Recordkey = paramRecord.Id,
                        AltrClosingText = paramRecord.ClosingParagraph,
                        AltrIntroText = paramRecord.OpeningParagraph,
                        AltrCategoryGroup1 = paramRecord.AwardCategoriesGroup1,
                        AltrCategoryGroup2 = paramRecord.AwardCategoriesGroup2,
                        AltrTitleGroup1 = paramRecord.AwardCategoryGroup1Title,
                        AltrTitleGroup2 = paramRecord.AwardCategoryGroup2Title,
                        AltrTitleGroup3 = paramRecord.AwardCategoryGroup3Title,
                        AltrTitleAwdName = paramRecord.AwardColumnTitle,
                        AltrTitleAwdTotal = paramRecord.TotalColumnTitle,
                        AltrNeedBlock = paramRecord.IsNeedBlockActive ? "Y" : "N",
                        AltrHousingCode = paramRecord.IsHousingCodeActive ? "Y" : "N",
                        AltrOfficeBlock = paramRecord.IsOfficeBlockActive ? "Y" : "N"
                    });
            }
            return altrParametersDataContracts;
        }

        public void BuildAwardLetterHistoryResponseData(List<TestAwardLetterHistoryRepository.StudentAwardLetterHistory> alhTestData)
        {
            awardLetterHistoryData = new Collection<AwardLetterHistory>();
            foreach (var testDataObj in alhTestData)
            {
                var alhTest = new AwardLetterHistory()
                {
                    Recordkey = testDataObj.Id,
                    AlhAcceptedDate = testDataObj.AcceptedDate,
                    AlhStudentId = testDataObj.StudentId,
                    AlhAwardLetterParamsId = testDataObj.AwardLetterParametersId,
                    AlhAwardLetterDate = testDataObj.CreatedDate,
                    AlhEfc = testDataObj.EFC,
                    AlhCost = testDataObj.Cost,
                    AlhAwardYear = testDataObj.AwardYear,
                    AlhOfficeId = testDataObj.OfficeId,
                    AlhClosingParagraph = testDataObj.ClosingParagraph,
                    AlhOpeningParagraph = testDataObj.OpeningParagraph,
                    AlhAnnualAwardTableEntityAssociation = new List<AwardLetterHistoryAlhAnnualAwardTable>(),
                    AlhAwardPeriodTableEntityAssociation = new List<AwardLetterHistoryAlhAwardPeriodTable>(),
                    AlhGroupsEntityAssociation = new List<AwardLetterHistoryAlhGroups>(),
                    AlhPrefName = testDataObj.PreferredName,
                    AlhStudentName = testDataObj.StudentName,
                    AlhPrefAddrLine1 = testDataObj.StudentAddressLine1,
                    AlhPrefAddrLine2 = testDataObj.StudentAddressLine2,
                    AlhPrefAddrLine3 = testDataObj.StudentAddressLine3,
                    AlhPrefAddrLine4 = testDataObj.StudentAddressLine4
                    
                };
                dataReaderMock.Setup(r => r.ReadRecordAsync<AwardLetterHistory>(alhTest.Recordkey, true)).Returns(Task.FromResult(alhTest));
                awardLetterHistoryData.Add(alhTest);
            }
            
        }

        #region Obsolete methods tests      

        [TestClass]
        public class AwardLetterHistoryRepository_GetSingleAwardLetterAsyncTests : AwardLetterHistoryRepositoryTests
        {
            #region Declare Initialize and Cleanup

            //Test data
            private AwardLetter2 expectedAwardLetter;
            private AwardLetter2 actualAwardLetter;           

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                awardYear = "2014";                
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == awardYear);
                bool createALHRecord = true;

                expectedAwardLetter = await expectedRepository.GetAwardLetterAsync(studentId, studentAwardYear, allAwards, createALHRecord);               

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, allAwards, createALHRecord);
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                dataReaderMock = null;
                localCacheMock = null;
                loggerMock = null;
                transFactoryMock = null;
                transManagerMock = null;

                studentAwardYears = null;
                expectedRepository = null;
                actualRepository = null;
            }

            #endregion

            #region Test Cases

            [TestMethod]
            public void ExpectedAwardLetterEqualsActual()
            {
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == awardYear);
                allAwards = referenceRepository.Awards;
                bool createALHRecord = true;

                await actualRepository.GetAwardLetterAsync(null, studentAwardYear, allAwards,createALHRecord);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentYearRequiredTest()
            {
                studentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == awardYear);
                allAwards = referenceRepository.Awards;
                bool createALHRecord = true;

                await actualRepository.GetAwardLetterAsync(studentId, null, allAwards,createALHRecord);
            }

            [TestMethod]
            public async Task CreateALHRecordFalseTest()
            {
                studentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == awardYear);
                allAwards = referenceRepository.Awards;
                bool createALHRecord = false;

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, allAwards, createALHRecord);
                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            public async Task DoubleParagraphSpacing_IsAdheredToTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph \r\n\r\n with a new line";

                var paramRecord = altrParameters.First(r => r.Recordkey == mostRecentAwardLetter.AlhAwardLetterParamsId);
                paramRecord.AltrParaSpacing = "2"; 
                
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph \r\n\r\n\r\n\r\n with a new line";
                string expectedClosingParagraph = "This is the Closing Paragraph";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
                Assert.AreEqual(expectedClosingParagraph, actualAwardLetter.ClosingParagraph);
            }

            [TestMethod]
            public async Task SingleParagraphSpacing_IsAdheredToTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph \r\n\r\n with a new line";

                var paramRecord = altrParameters.First(r => r.Recordkey == mostRecentAwardLetter.AlhAwardLetterParamsId);
                paramRecord.AltrParaSpacing = "1";
                
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph \r\n\r\n with a new line";
                string expectedClosingParagraph = "This is the Closing Paragraph";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
                Assert.AreEqual(expectedClosingParagraph, actualAwardLetter.ClosingParagraph);
            }

            [TestMethod]
            public async Task UrlWithinParagraphDoubleQuotes_FormattedAsExpectedTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph with a link: <a href=\"http://www.go ogle.com\">click here</a>";
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph with a link: <a href=\"http://www.google.com\">click here</a>";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
            }

            [TestMethod]
            public async Task UrlWithinParagraphSingleQuotes_FormattedAsExpectedTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph with a link: <a href='http://www.go ogle.com'>click here</a>";
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph with a link: <a href='http://www.google.com'>click here</a>";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
            }

            [TestMethod]
            public async Task MultipleUrlsWithinParagraph_FormattedAsExpectedTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph with links: <a href=\"http://www.go ogle.com\">click here</a>\r\n\r\n<a href=\"http://coldevwcol01.data telsdd.com:7778/dvcoll_wsts t01_ui45/sl/ind\r\n\r\nex.htm\">click here</a>";
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph with links: <a href=\"http://www.google.com\">click here</a>\r\n\r\n<a href=\"http://coldevwcol01.datatelsdd.com:7778/dvcoll_wstst01_ui45/sl/index.htm\">click here</a>";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
            }

            [TestMethod]
            public void StudentName_EqualsExpectedValueTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();                
                Assert.AreEqual(expectedRecord.AlhStudentName, actualAwardLetter.StudentName);
            }

            [TestMethod]
            public void StudentAddress_EqualsExpectedValueTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                Assert.AreEqual(expectedRecord.AlhPrefName, actualAwardLetter.StudentAddress[0]);
                Assert.AreEqual(expectedRecord.AlhPrefAddrLine1, actualAwardLetter.StudentAddress[1]);
                Assert.AreEqual(expectedRecord.AlhPrefAddrLine2, actualAwardLetter.StudentAddress[2]);
                Assert.AreEqual(expectedRecord.AlhPrefAddrLine3, actualAwardLetter.StudentAddress[3]);
                Assert.AreEqual(expectedRecord.AlhPrefAddrLine4, actualAwardLetter.StudentAddress[4]);
            }

            [TestMethod]
            public async Task NullPreferredName_NumberOfStudentAddressLinesMatchesExpectedTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                expectedRecord.AlhPrefName = null;
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == expectedRecord.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, referenceRepository.Awards, false);
                //Expected number - 4 - the number of preferred address lines
                Assert.IsTrue(actualAwardLetter.StudentAddress.Count == 4);
            }

            [TestMethod]
            public async Task NullAddressLines_NumberOfStudentAddressLinesMatchesExpectedTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                expectedRecord.AlhPrefAddrLine1 = null;
                expectedRecord.AlhPrefAddrLine2 = null;
                expectedRecord.AlhPrefAddrLine3 = null;
                expectedRecord.AlhPrefAddrLine4 = null;
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == expectedRecord.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, referenceRepository.Awards, false);
                //Expected number - 1 - just the preferred name
                Assert.IsTrue(actualAwardLetter.StudentAddress.Count == 1);
            }

            [TestMethod]
            public async Task NullPreferredNameAndAddressLines_StudentAddressIsEmptyTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                expectedRecord.AlhPrefName = null;
                expectedRecord.AlhPrefAddrLine1 = null;
                expectedRecord.AlhPrefAddrLine2 = null;
                expectedRecord.AlhPrefAddrLine3 = null;
                expectedRecord.AlhPrefAddrLine4 = null;
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == expectedRecord.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetterAsync(studentId, studentAwardYear, referenceRepository.Awards, false);
                Assert.IsFalse(actualAwardLetter.StudentAddress.Any());
            }

            #endregion

        }

        [TestClass]
        public class AwardLetterHistoryRepository_GetAwardLettersAsyncTests : AwardLetterHistoryRepositoryTests
        {
            private IEnumerable<AwardLetter2> expectedAwardLetters;
            private IEnumerable<AwardLetter2> actualAwardLetters;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                expectedAwardLetters = await expectedRepository.GetAwardLettersAsync(studentId, studentAwardYears, allAwards);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentId = null;
                expectedRepository = null;
                officeRepository = null;
                studentAwardYearRepository = null;
                currentOfficeService = null;
                referenceRepository = null;
                allAwards = null;
                actualRepository = null;
                studentAwardYears = null;
                expectedAwardLetters = null;
                actualAwardLetters = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await actualRepository.GetAwardLettersAsync(null, studentAwardYears, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearsArgIsRequiredTest()
            {
                await actualRepository.GetAwardLettersAsync(studentId, null, allAwards);
            }

            [TestMethod]
            public async Task EmptyStudentAwardYearsList_LogsMessageTest()
            {
                await actualRepository.GetAwardLettersAsync(studentId, new List<StudentAwardYear>(), allAwards);
                loggerMock.Verify(l => l.Info(string.Format("Student {0} has a Financial Aid record, but no award year data", studentId)));
            }

            [TestMethod]
            public async Task EmptyStudentAwardYearsList_ReturnsEmptyAwardLetterListTest()
            {
                actualAwardLetters = await actualRepository.GetAwardLettersAsync(studentId, new List<StudentAwardYear>(), allAwards);
                Assert.IsFalse(actualAwardLetters.Any());
            }

            [TestMethod]
            public async Task ActualAwardLettersCount_EqualsExpectedCountTest()
            {
                actualAwardLetters = await actualRepository.GetAwardLettersAsync(studentId, studentAwardYears, allAwards);
                Assert.AreEqual(expectedAwardLetters.Count(), actualAwardLetters.Count());
            }

            [TestMethod]
            public async Task ActualAwardLetters_EqualsExpectedTest()
            {
                actualAwardLetters = await actualRepository.GetAwardLettersAsync(studentId, studentAwardYears, allAwards);
                foreach (var expectedLetter in expectedAwardLetters)
                {
                    var actualAwardLetter = actualAwardLetters.FirstOrDefault(al => al.Id == expectedLetter.Id);
                    Assert.IsNotNull(actualAwardLetter);
                    Assert.AreEqual(expectedLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
                    Assert.AreEqual(expectedLetter.AwardLetterYear, actualAwardLetter.AwardLetterYear);
                }
            }
        }

        [TestClass]
        public class AwardLetterHistoryRepository_GetAwardLetterByIdAsyncTests : AwardLetterHistoryRepositoryTests
        {
            private string recordId;
            private AwardLetter2 expectedAwardLetter;
            private AwardLetter2 actualAwardLetter;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                recordId = "67";

                expectedAwardLetter = await expectedRepository.GetAwardLetterByIdAsync(recordId, studentAwardYears, allAwards);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentId = null;
                expectedRepository = null;
                officeRepository = null;
                studentAwardYearRepository = null;
                currentOfficeService = null;
                referenceRepository = null;
                allAwards = null;
                actualRepository = null;
                studentAwardYears = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RecordIdIsRequiredTest()
            {
                await actualRepository.GetAwardLetterByIdAsync(null, studentAwardYears, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RecordIdIsRequiredTest2()
            {
                await actualRepository.GetAwardLetterByIdAsync(string.Empty, studentAwardYears, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearsArgIsRequiredTest()
            {
                await actualRepository.GetAwardLetterByIdAsync(recordId, null, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearsArgIsRequiredTest2()
            {
                await actualRepository.GetAwardLetterByIdAsync(recordId, new List<StudentAwardYear>(), allAwards);
            }

            [TestMethod]
            public async Task ActualAwardLetter_EqualsExpectedTest()
            {
                actualAwardLetter = await actualRepository.GetAwardLetterByIdAsync(recordId, studentAwardYears, allAwards);
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
                Assert.AreEqual(expectedAwardLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
                Assert.AreEqual(expectedAwardLetter.AwardLetterParameterId, actualAwardLetter.AwardLetterParameterId);
                Assert.AreEqual(expectedAwardLetter.AwardLetterYear, actualAwardLetter.AwardLetterYear);
            }

            [TestMethod]
            public async Task NoAwardLetterRecordForId_MessageLoggedTest()
            {
                await actualRepository.GetAwardLetterByIdAsync("foo", studentAwardYears, allAwards);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task NoAwardLetterRecordForId_ReturnsEmptyAwardLetterObjectTest()
            {
                actualAwardLetter = await actualRepository.GetAwardLetterByIdAsync("foo", studentAwardYears, allAwards);
                Assert.IsNotNull(actualAwardLetter);
                Assert.IsNull(actualAwardLetter.Id);
                Assert.IsNull(actualAwardLetter.CreatedDate);
                Assert.IsNull(actualAwardLetter.AwardLetterYear);
                Assert.IsNull(actualAwardLetter.StudentId);
            }
        }

        [TestClass]
        public class AwardLetterHistoryRepository_UpdateAwardLetterAsyncTests : AwardLetterHistoryRepositoryTests
        {
            private StudentAwardYear studentAwardYear;
            private AwardLetter2 expectedAwardLetter;
            private AwardLetter2 actualAwardLetter;
            private AwardLetter2 awardLetterToUpdate;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                studentAwardYear = studentAwardYears.First();
                var studentAwardLetterData = expectedRepository.awardLetterHistoryData.First(al => al.AwardYear == studentAwardYear.Code);
                
                awardLetterToUpdate = new AwardLetter2(studentId, studentAwardYear) { Id = studentAwardLetterData.Id };

                expectedAwardLetter = await expectedRepository.UpdateAwardLetterAsync(studentId, awardLetterToUpdate, studentAwardYear, allAwards);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentId = null;
                expectedRepository = null;
                officeRepository = null;
                studentAwardYearRepository = null;
                currentOfficeService = null;
                referenceRepository = null;
                allAwards = null;
                actualRepository = null;
                studentAwardYears = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardLetterIsRequiredTest()
            {
                await actualRepository.UpdateAwardLetterAsync(studentId, null, studentAwardYear, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardLetterAwardYearIsRequiredTest()
            {
                await actualRepository.UpdateAwardLetterAsync(studentId, new AwardLetter2(), studentAwardYear, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AwardLetterStudentIdsMustMatchTest()
            {
                await actualRepository.UpdateAwardLetterAsync(studentId, awardLetterToUpdate, new StudentAwardYear("foo", awardLetterToUpdate.AwardLetterYear), allAwards);
            }

            [TestMethod]
            public async Task ActualUpdatedAwardLetter_EqualsExpectedTest()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<AwardLetterHistory>(awardLetterToUpdate.Id, true))
                    .Returns(Task.FromResult(new AwardLetterHistory() {
                        Recordkey = awardLetterToUpdate.Id,
                        AlhAcceptedDate = DateTime.Today,
                        AlhStudentId = awardLetterToUpdate.StudentId,
                        AlhAwardLetterParamsId = awardLetterToUpdate.AwardLetterParameterId,
                        AlhAwardLetterDate = awardLetterToUpdate.CreatedDate,
                        AlhEfc = awardLetterToUpdate.EstimatedFamilyContributionAmount,
                        AlhCost = awardLetterToUpdate.BudgetAmount,
                        AlhAwardYear = awardLetterToUpdate.AwardLetterYear,
                        AlhOfficeId = awardLetterToUpdate.StudentOfficeCode,
                        AlhClosingParagraph = awardLetterToUpdate.ClosingParagraph,
                        AlhOpeningParagraph = awardLetterToUpdate.OpeningParagraph,
                        AlhAnnualAwardTableEntityAssociation = new List<AwardLetterHistoryAlhAnnualAwardTable>(),
                        AlhAwardPeriodTableEntityAssociation = new List<AwardLetterHistoryAlhAwardPeriodTable>(),
                        AlhGroupsEntityAssociation = new List<AwardLetterHistoryAlhGroups>()
                        
                    }));
                actualAwardLetter = await actualRepository.UpdateAwardLetterAsync(studentId, awardLetterToUpdate, studentAwardYear, allAwards);
                Assert.AreEqual(actualAwardLetter, expectedAwardLetter);
                Assert.AreEqual(expectedAwardLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
                Assert.AreEqual(expectedAwardLetter.StudentId, actualAwardLetter.StudentId);
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
            }

            [TestMethod]
            [ExpectedException (typeof(OperationCanceledException))]
            public async Task AcceptedDateNotNull_ThrowsOperationCancelledExceptionTest()
            {
                updateAwardLetterSignedDateResponseData.ErrorMessage = "This award letter has already been signed. No update required.";
                await actualRepository.UpdateAwardLetterAsync(studentId, awardLetterToUpdate, studentAwardYear, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(OperationCanceledException))]
            public async Task RecordLocked_ThrowsOperationCancelledExceptionTest()
            {
                updateAwardLetterSignedDateResponseData.ErrorMessage = "Random message";
                await actualRepository.UpdateAwardLetterAsync(studentId, awardLetterToUpdate, studentAwardYear, allAwards);
            }
            
        }

        #endregion

        [TestClass]
        public class AwardLetterHistoryRepository_GetAwardLetter2AsyncTests : AwardLetterHistoryRepositoryTests
        {
            #region Declare Initialize and Cleanup

            //Test data
            private AwardLetter3 expectedAwardLetter;
            private AwardLetter3 actualAwardLetter;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                awardYear = "2014";
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == awardYear);
                bool createALHRecord = true;

                expectedAwardLetter = await expectedRepository.GetAwardLetter2Async(studentId, studentAwardYear, allAwards, createALHRecord);

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, allAwards, createALHRecord);
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                dataReaderMock = null;
                localCacheMock = null;
                loggerMock = null;
                transFactoryMock = null;
                transManagerMock = null;

                studentAwardYears = null;
                expectedRepository = null;
                actualRepository = null;
            }

            #endregion

            #region Test Cases

            [TestMethod]
            public void ExpectedAwardLetterEqualsActual()
            {
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == awardYear);
                allAwards = referenceRepository.Awards;
                bool createALHRecord = true;

                await actualRepository.GetAwardLetter2Async(null, studentAwardYear, allAwards, createALHRecord);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentYearRequiredTest()
            {
                studentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == awardYear);
                allAwards = referenceRepository.Awards;
                bool createALHRecord = true;

                await actualRepository.GetAwardLetter2Async(studentId, null, allAwards, createALHRecord);
            }

            [TestMethod]
            public async Task CreateALHRecordFalseTest()
            {
                studentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == awardYear);
                allAwards = referenceRepository.Awards;
                bool createALHRecord = false;

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, allAwards, createALHRecord);
                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            public async Task DoubleParagraphSpacing_IsAdheredToTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph \r\n\r\n with a new line";

                var paramRecord = altrParameters.First(r => r.Recordkey == mostRecentAwardLetter.AlhAwardLetterParamsId);
                paramRecord.AltrParaSpacing = "2";

                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph \r\n\r\n\r\n\r\n with a new line";
                string expectedClosingParagraph = "This is the Closing Paragraph";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
                Assert.AreEqual(expectedClosingParagraph, actualAwardLetter.ClosingParagraph);
            }

            [TestMethod]
            public async Task SingleParagraphSpacing_IsAdheredToTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph \r\n\r\n with a new line";

                var paramRecord = altrParameters.First(r => r.Recordkey == mostRecentAwardLetter.AlhAwardLetterParamsId);
                paramRecord.AltrParaSpacing = "1";

                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph \r\n\r\n with a new line";
                string expectedClosingParagraph = "This is the Closing Paragraph";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
                Assert.AreEqual(expectedClosingParagraph, actualAwardLetter.ClosingParagraph);
            }

            [TestMethod]
            public async Task UrlWithinParagraphDoubleQuotes_FormattedAsExpectedTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph with a link: <a href=\"http://www.go ogle.com\">click here</a>";
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph with a link: <a href=\"http://www.google.com\">click here</a>";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
            }

            [TestMethod]
            public async Task UrlWithinParagraphSingleQuotes_FormattedAsExpectedTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph with a link: <a href='http://www.go ogle.com'>click here</a>";
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph with a link: <a href='http://www.google.com'>click here</a>";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
            }

            [TestMethod]
            public async Task MultipleUrlsWithinParagraph_FormattedAsExpectedTest()
            {
                var mostRecentAwardLetter = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                mostRecentAwardLetter.AlhOpeningParagraph = "This is a paragraph with links: <a href=\"http://www.go ogle.com\">click here</a>\r\n\r\n<a href=\"http://coldevwcol01.data telsdd.com:7778/dvcoll_wsts t01_ui45/sl/ind\r\n\r\nex.htm\">click here</a>";
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == mostRecentAwardLetter.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, referenceRepository.Awards, false);
                string expectedOpeningParagraph = "This is a paragraph with links: <a href=\"http://www.google.com\">click here</a>\r\n\r\n<a href=\"http://coldevwcol01.datatelsdd.com:7778/dvcoll_wstst01_ui45/sl/index.htm\">click here</a>";
                Assert.AreEqual(expectedOpeningParagraph, actualAwardLetter.OpeningParagraph);
            }

            [TestMethod]
            public void StudentName_EqualsExpectedValueTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                Assert.AreEqual(expectedRecord.AlhStudentName, actualAwardLetter.StudentName);
            }

            [TestMethod]
            public void StudentAddress_EqualsExpectedValueTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                Assert.AreEqual(expectedRecord.AlhPrefName, actualAwardLetter.StudentAddress[0]);
                Assert.AreEqual(expectedRecord.AlhPrefAddrLine1, actualAwardLetter.StudentAddress[1]);
                Assert.AreEqual(expectedRecord.AlhPrefAddrLine2, actualAwardLetter.StudentAddress[2]);
                Assert.AreEqual(expectedRecord.AlhPrefAddrLine3, actualAwardLetter.StudentAddress[3]);
                Assert.AreEqual(expectedRecord.AlhPrefAddrLine4, actualAwardLetter.StudentAddress[4]);
            }

            [TestMethod]
            public async Task NullPreferredName_NumberOfStudentAddressLinesMatchesExpectedTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                expectedRecord.AlhPrefName = null;
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == expectedRecord.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, referenceRepository.Awards, false);
                //Expected number - 4 - the number of preferred address lines
                Assert.IsTrue(actualAwardLetter.StudentAddress.Count == 4);
            }

            [TestMethod]
            public async Task NullAddressLines_NumberOfStudentAddressLinesMatchesExpectedTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                expectedRecord.AlhPrefAddrLine1 = null;
                expectedRecord.AlhPrefAddrLine2 = null;
                expectedRecord.AlhPrefAddrLine3 = null;
                expectedRecord.AlhPrefAddrLine4 = null;
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == expectedRecord.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, referenceRepository.Awards, false);
                //Expected number - 1 - just the preferred name
                Assert.IsTrue(actualAwardLetter.StudentAddress.Count == 1);
            }

            [TestMethod]
            public async Task NullPreferredNameAndAddressLines_StudentAddressIsEmptyTest()
            {
                var expectedRecord = awardLetterHistoryData.OrderByDescending(alh => alh.AlhAwardLetterDate).First();
                expectedRecord.AlhPrefName = null;
                expectedRecord.AlhPrefAddrLine1 = null;
                expectedRecord.AlhPrefAddrLine2 = null;
                expectedRecord.AlhPrefAddrLine3 = null;
                expectedRecord.AlhPrefAddrLine4 = null;
                var studentAwardYear = studentAwardYears.FirstOrDefault(say => say.Code == expectedRecord.AlhAwardYear);

                actualAwardLetter = await actualRepository.GetAwardLetter2Async(studentId, studentAwardYear, referenceRepository.Awards, false);
                Assert.IsFalse(actualAwardLetter.StudentAddress.Any());
            }

            #endregion

        }

        [TestClass]
        public class AwardLetterHistoryRepository_GetAwardLetterById2AsyncTests : AwardLetterHistoryRepositoryTests
        {
            private string recordId;
            private AwardLetter3 expectedAwardLetter;
            private AwardLetter3 actualAwardLetter;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                recordId = "67";

                expectedAwardLetter = await expectedRepository.GetAwardLetterById2Async(recordId, studentAwardYears, allAwards);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentId = null;
                expectedRepository = null;
                officeRepository = null;
                studentAwardYearRepository = null;
                currentOfficeService = null;
                referenceRepository = null;
                allAwards = null;
                actualRepository = null;
                studentAwardYears = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RecordIdIsRequiredTest()
            {
                await actualRepository.GetAwardLetterById2Async(null, studentAwardYears, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RecordIdIsRequiredTest2()
            {
                await actualRepository.GetAwardLetterById2Async(string.Empty, studentAwardYears, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearsArgIsRequiredTest()
            {
                await actualRepository.GetAwardLetterById2Async(recordId, null, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearsArgIsRequiredTest2()
            {
                await actualRepository.GetAwardLetterById2Async(recordId, new List<StudentAwardYear>(), allAwards);
            }

            [TestMethod]
            public async Task ActualAwardLetter_EqualsExpectedTest()
            {
                actualAwardLetter = await actualRepository.GetAwardLetterById2Async(recordId, studentAwardYears, allAwards);
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
                Assert.AreEqual(expectedAwardLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
                Assert.AreEqual(expectedAwardLetter.AwardLetterParameterId, actualAwardLetter.AwardLetterParameterId);
                Assert.AreEqual(expectedAwardLetter.AwardLetterYear, actualAwardLetter.AwardLetterYear);
            }

            [TestMethod]
            public async Task NoAwardLetterRecordForId_MessageLoggedTest()
            {
                await actualRepository.GetAwardLetterById2Async("foo", studentAwardYears, allAwards);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task NoAwardLetterRecordForId_ReturnsEmptyAwardLetterObjectTest()
            {
                actualAwardLetter = await actualRepository.GetAwardLetterById2Async("foo", studentAwardYears, allAwards);
                Assert.IsNotNull(actualAwardLetter);
                Assert.IsNull(actualAwardLetter.Id);
                Assert.IsNull(actualAwardLetter.CreatedDate);
                Assert.IsNull(actualAwardLetter.AwardLetterYear);
                Assert.IsNull(actualAwardLetter.StudentId);
            }
        }

        [TestClass]
        public class AwardLetterHistoryRepository_UpdateAwardLetter2AsyncTests : AwardLetterHistoryRepositoryTests
        {
            private StudentAwardYear studentAwardYear;
            private AwardLetter3 expectedAwardLetter;
            private AwardLetter3 actualAwardLetter;
            private AwardLetter3 awardLetterToUpdate;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                studentAwardYear = studentAwardYears.First();
                var studentAwardLetterData = expectedRepository.awardLetterHistoryData.First(al => al.AwardYear == studentAwardYear.Code);

                awardLetterToUpdate = new AwardLetter3(studentId, studentAwardYear) { Id = studentAwardLetterData.Id };

                expectedAwardLetter = await expectedRepository.UpdateAwardLetter2Async(studentId, awardLetterToUpdate, studentAwardYear, allAwards);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentId = null;
                expectedRepository = null;
                officeRepository = null;
                studentAwardYearRepository = null;
                currentOfficeService = null;
                referenceRepository = null;
                allAwards = null;
                actualRepository = null;
                studentAwardYears = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardLetterIsRequiredTest()
            {
                await actualRepository.UpdateAwardLetter2Async(studentId, null, studentAwardYear, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardLetterAwardYearIsRequiredTest()
            {
                await actualRepository.UpdateAwardLetter2Async(studentId, new AwardLetter3(), studentAwardYear, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AwardLetterStudentIdsMustMatchTest()
            {
                await actualRepository.UpdateAwardLetter2Async(studentId, awardLetterToUpdate, new StudentAwardYear("foo", awardLetterToUpdate.AwardLetterYear), allAwards);
            }

            [TestMethod]
            public async Task ActualUpdatedAwardLetter_EqualsExpectedTest()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<AwardLetterHistory>(awardLetterToUpdate.Id, true))
                    .Returns(Task.FromResult(new AwardLetterHistory()
                    {
                        Recordkey = awardLetterToUpdate.Id,
                        AlhAcceptedDate = DateTime.Today,
                        AlhStudentId = awardLetterToUpdate.StudentId,
                        AlhAwardLetterParamsId = awardLetterToUpdate.AwardLetterParameterId,
                        AlhAwardLetterDate = awardLetterToUpdate.CreatedDate,
                        AlhEfc = awardLetterToUpdate.EstimatedFamilyContributionAmount,
                        AlhCost = awardLetterToUpdate.BudgetAmount,
                        AlhAwardYear = awardLetterToUpdate.AwardLetterYear,
                        AlhOfficeId = awardLetterToUpdate.StudentOfficeCode,
                        AlhClosingParagraph = awardLetterToUpdate.ClosingParagraph,
                        AlhOpeningParagraph = awardLetterToUpdate.OpeningParagraph,
                        AlhAnnualAwardTableEntityAssociation = new List<AwardLetterHistoryAlhAnnualAwardTable>(),
                        AlhAwardPeriodTableEntityAssociation = new List<AwardLetterHistoryAlhAwardPeriodTable>(),
                        AlhGroupsEntityAssociation = new List<AwardLetterHistoryAlhGroups>()

                    }));
                actualAwardLetter = await actualRepository.UpdateAwardLetter2Async(studentId, awardLetterToUpdate, studentAwardYear, allAwards);
                Assert.AreEqual(actualAwardLetter, expectedAwardLetter);
                Assert.AreEqual(expectedAwardLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
                Assert.AreEqual(expectedAwardLetter.StudentId, actualAwardLetter.StudentId);
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(OperationCanceledException))]
            public async Task AcceptedDateNotNull_ThrowsOperationCancelledExceptionTest()
            {
                updateAwardLetterSignedDateResponseData.ErrorMessage = "This award letter has already been signed. No update required.";
                await actualRepository.UpdateAwardLetter2Async(studentId, awardLetterToUpdate, studentAwardYear, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(OperationCanceledException))]
            public async Task RecordLocked_ThrowsOperationCancelledExceptionTest()
            {
                updateAwardLetterSignedDateResponseData.ErrorMessage = "Random message";
                await actualRepository.UpdateAwardLetter2Async(studentId, awardLetterToUpdate, studentAwardYear, allAwards);
            }

        }

        [TestClass]
        public class AwardLetterHistoryRepository_GetAwardLetters2AsyncTests : AwardLetterHistoryRepositoryTests
        {
            private IEnumerable<AwardLetter3> expectedAwardLetters;
            private IEnumerable<AwardLetter3> actualAwardLetters;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                expectedAwardLetters = await expectedRepository.GetAwardLetters2Async(studentId, studentAwardYears, allAwards);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentId = null;
                expectedRepository = null;
                officeRepository = null;
                studentAwardYearRepository = null;
                currentOfficeService = null;
                referenceRepository = null;
                allAwards = null;
                actualRepository = null;
                studentAwardYears = null;
                expectedAwardLetters = null;
                actualAwardLetters = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await actualRepository.GetAwardLetters2Async(null, studentAwardYears, allAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearsArgIsRequiredTest()
            {
                await actualRepository.GetAwardLetters2Async(studentId, null, allAwards);
            }

            [TestMethod]
            public async Task EmptyStudentAwardYearsList_LogsMessageTest()
            {
                await actualRepository.GetAwardLetters2Async(studentId, new List<StudentAwardYear>(), allAwards);
                loggerMock.Verify(l => l.Info(string.Format("Student {0} has a Financial Aid record, but no award year data", studentId)));
            }

            [TestMethod]
            public async Task EmptyStudentAwardYearsList_ReturnsEmptyAwardLetterListTest()
            {
                actualAwardLetters = await actualRepository.GetAwardLetters2Async(studentId, new List<StudentAwardYear>(), allAwards);
                Assert.IsFalse(actualAwardLetters.Any());
            }

            [TestMethod]
            public async Task ActualAwardLettersCount_EqualsExpectedCountTest()
            {
                actualAwardLetters = await actualRepository.GetAwardLetters2Async(studentId, studentAwardYears, allAwards);
                Assert.AreEqual(expectedAwardLetters.Count(), actualAwardLetters.Count());
            }

            [TestMethod]
            public async Task ActualAwardLetters_EqualsExpectedTest()
            {
                actualAwardLetters = await actualRepository.GetAwardLetters2Async(studentId, studentAwardYears, allAwards);
                foreach (var expectedLetter in expectedAwardLetters)
                {
                    var actualAwardLetter = actualAwardLetters.FirstOrDefault(al => al.Id == expectedLetter.Id);
                    Assert.IsNotNull(actualAwardLetter);
                    Assert.AreEqual(expectedLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
                    Assert.AreEqual(expectedLetter.AwardLetterYear, actualAwardLetter.AwardLetterYear);
                }
            }
        }

    }
}
