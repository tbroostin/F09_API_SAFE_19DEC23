/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Dmi.Runtime;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class AcademicProgressRepositoryTests : BaseRepositorySetup
    {
        public string studentId;

        public TestAcademicProgressRepository expectedRepository;
        public List<AcademicProgressEvaluationResult> expectedEvaluations
        {
            get { return expectedRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId).Result.ToList(); }
        }

        public AcademicProgressRepository actualRepository;
        public IEnumerable<AcademicProgressEvaluationResult> actualEvaluations;
        
        public void AcademicProgressRepositoryTestsInitialize()
        {
            MockInitialize();
            studentId = "0003914";
            expectedRepository = new TestAcademicProgressRepository();
            actualRepository = BuildActualRepository();
        }

        private AcademicProgressRepository BuildActualRepository()
        {
            dataReaderMock.Setup(d => d.ReadRecordAsync<FinAid>(studentId, true))
                .Returns<string, bool>((id, b) =>
                    Task.FromResult((expectedRepository.financialAidStudentData == null) ? null :
                    new FinAid()
                    {
                        Recordkey = studentId,
                        FaSapResultsId = expectedRepository.financialAidStudentData.sapResultsIds
                    }));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<SapResults>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) =>
                    Task.FromResult((expectedRepository.SapResultsData == null) ? null :
                    new Collection<SapResults>(
                    expectedRepository.SapResultsData.Where(record => ids.Contains(record.id)).Select(record =>
                        new SapResults()
                        {
                            Recordkey = record.id,
                            SapResultsAdddate = record.evaluationDate,
                            SaprCalcSapStatus = record.originalStatusCode,
                            SaprCumAttCred = record.cumulativeAttemptedCredits,
                            SaprCumCmplCred = record.cumulativeCompletedCredits,
                            SaprCumCmplGradePts = record.cumulativeCompletedGradePoints,
                            SaprCumAttCredRem = record.cumulativeAttemptedCreditsNoRemedial,
                            SaprCumCmplCredRem = record.cumulativeCompletedCreditsNoRemedial,
                            SaprEvalPdEndDate = record.evaluationPeriodEndDate,
                            SaprEvalPdEndTerm = record.evaluationPeriodEndTerm,
                            SaprEvalPdStartDate = record.evaluationPeriodStartDate,
                            SaprEvalPdStartTerm = record.evaluationPeriodStartTerm,
                            SaprOvrSapStatus = record.overrideStatusCode,
                            SaprStudentId = studentId,
                            SaprTrmAttCred = record.evaluationPeriodAttemptedCredits,
                            SaprTrmCmplCred = record.evaluationPeriodCompletedCredits,
                            SaprTrmCmplGradePts = record.evaluationPeriodCompletedGradePoints,                            
                            SaprAcadProgram = record.academicProgram,
                            SaprBatchId = record.batchId,                            
                        }).ToList())));

            dataReaderMock.Setup(r => r.ReadRecordAsync<AcadProgramReqmts>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns<string, string, bool>((tName, filekey, b) => 
                {
                    string[] key = filekey.Split('*');
                    var programReq = expectedRepository.programRequirementsData.FirstOrDefault(pr => pr.programCode == key[0] && pr.catalogCode == key[1]);
                    return Task.FromResult((programReq == null) ? null :
                        new AcadProgramReqmts()
                        {
                            Recordkey = programReq.programCode,
                            AcprMaxCred = programReq.maxCredits,
                            AcprCred = programReq.minCredits
                        });
                });

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            loggerMock.Setup(l => l.IsWarnEnabled).Returns(true);
            loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

            return new AcademicProgressRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestClass]
        public class GetStudentAcademicProgressEvaluationsTests : AcademicProgressRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressRepositoryTestsInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                CollectionAssert.AreEqual(expectedEvaluations, actualEvaluations.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = null;
                await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
            }

            [TestMethod]
            public async Task NullFinAidRecordLogsInfoReturnsEmptyListTest()
            {
                expectedRepository.financialAidStudentData = null;
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                Assert.AreEqual(0, actualEvaluations.Count());

                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no financial aid data", studentId)));
            }

            [TestMethod]
            public async Task NullSapResultsIdsLogsInfoReturnsEmptyListTest()
            {
                expectedRepository.financialAidStudentData.sapResultsIds = null;
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                Assert.AreEqual(0, actualEvaluations.Count());

                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no Academic Progress Evaluations", studentId)));
            }

            [TestMethod]
            public async Task EmptySapResultsIdsLogsInfoReturnsEmptyListTest()
            {
                expectedRepository.financialAidStudentData.sapResultsIds = new List<string>();
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                Assert.AreEqual(0, actualEvaluations.Count());

                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no Academic Progress Evaluations", studentId)));
            }

            [TestMethod]
            public async Task NullSapResultsRecordsLogsDataErrorReturnsEmptyListTest()
            {
                expectedRepository.SapResultsData = null;
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                Assert.AreEqual(0, actualEvaluations.Count());

                var message = string.Format("FIN.AID {0} has FA.SAP.RESULTS.ID entries which do not point to existing SAP.RESULTS records: {1}", studentId, String.Join(", ", expectedRepository.financialAidStudentData.sapResultsIds));

                //loggerMock.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<object[]>()));
                loggerMock.Verify(l => l.Error(message));
            }

            [TestMethod]
            public async Task EmptySapResultsRecordsLogsDataErrorReturnsEmptyListTest()
            {
                expectedRepository.SapResultsData = new List<TestAcademicProgressRepository.SapResultsRecord>();
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                Assert.AreEqual(0, actualEvaluations.Count());

                var message = string.Format("FIN.AID {0} has FA.SAP.RESULTS.ID entries which do not point to existing SAP.RESULTS records: {1}", studentId, String.Join(", ", expectedRepository.financialAidStudentData.sapResultsIds));

                //loggerMock.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<object[]>()));
                loggerMock.Verify(l => l.Error(message));
            }

            [TestMethod]
            public async Task TwoResultsWithSameJobIdHaveDifferentEvaluationDatesTest()
            {
                var laterId = expectedRepository.financialAidStudentData.sapResultsIds[0];
                var earlyId = expectedRepository.financialAidStudentData.sapResultsIds[1];

                DateTime? laterTime = new DateTime(DmiString.PickTimeToDateTime(55555).Ticks);
                DateTime? earlyTime = new DateTime(DmiString.PickTimeToDateTime(55554).Ticks);
                DateTimeOffset expectedLaterTime = laterTime.ToPointInTimeDateTimeOffset(DateTime.Today, apiSettings.ColleagueTimeZone).Value;
                DateTimeOffset expectedEarlyTime = earlyTime.ToPointInTimeDateTimeOffset(DateTime.Today, apiSettings.ColleagueTimeZone).Value;

                expectedRepository.SapResultsData[0].batchId = "SAPC_MCD_55555_17809";
                expectedRepository.SapResultsData[0].evaluationDate = DateTime.Today;
                expectedRepository.SapResultsData[1].batchId = "SAPC_MCD_55555_17809";
                expectedRepository.SapResultsData[1].evaluationDate = DateTime.Today;
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);

                var laterEvaluation = actualEvaluations.First(e => e.Id == laterId);
                var earlyEvaluation = actualEvaluations.First(e => e.Id == earlyId);

                Assert.AreEqual(expectedLaterTime, laterEvaluation.EvaluationDateTime);
                Assert.AreEqual(expectedEarlyTime, earlyEvaluation.EvaluationDateTime);

                Assert.IsTrue(laterEvaluation.EvaluationDateTime > earlyEvaluation.EvaluationDateTime);
            }

            [TestMethod]
            public async Task ErrorParsingTimeFromBatchId_LogsError_SetsEvaluationDateTimeToAddDateTest()
            {
                var expectedId = expectedRepository.SapResultsData[0].id;
                var expectedDateTimeOffset = expectedRepository.SapResultsData[0].evaluationDate.ToPointInTimeDateTimeOffset(DateTime.Today, apiSettings.ColleagueTimeZone).Value;

                expectedRepository.SapResultsData[0].batchId = "";
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedDateTimeOffset, actualEvaluation.EvaluationDateTime);

                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            }

            [TestMethod]
            public async Task UseOverrideStatusTest()
            {
                var expectedStatus = "foo";
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                    {
                        id = expectedId,
                        originalStatusCode = "bar",
                        overrideStatusCode = expectedStatus,
                        evaluationDate = DateTime.Today,
                        evaluationPeriodStartTerm = "2015/FA",
                        evaluationPeriodEndTerm = "2015/FA",
                        academicProgram = "MATH.BA"
                    });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedStatus, actualEvaluation.StatusCode);
            }

            [TestMethod]
            public async Task UseOriginalStatusTest()
            {
                var expectedStatus = "foo";
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = expectedStatus,
                    overrideStatusCode = string.Empty,
                    evaluationDate = DateTime.Today,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedStatus, actualEvaluation.StatusCode);
            }

            [TestMethod]
            public async Task CatchAndReportAcademicProgressEvaluationCreationErrorTest()
            {
                expectedRepository.SapResultsData.First().academicProgram = null;
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                CollectionAssert.AreEqual(expectedEvaluations, actualEvaluations.ToList());

                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            }

            [TestMethod]
            public async Task CumulativeAttemptedCreditsTest()
            {
                var expectedCredits = 10m;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",                    
                    evaluationDate = DateTime.Today,
                    cumulativeAttemptedCredits = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedCredits, actualEvaluation.CumulativeAttemptedCredits);
            }

            [TestMethod]
            public async Task NullCumulativeAttemptedCreditsTest()
            {
                decimal? expectedCredits = null;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    cumulativeAttemptedCredits = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(0, actualEvaluation.CumulativeAttemptedCredits);
            }

            [TestMethod]
            public async Task CumulativeCompletedCreditsTest()
            {
                var expectedCredits = 10m;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    cumulativeCompletedCredits = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedCredits, actualEvaluation.CumulativeCompletedCredits);
            }

            [TestMethod]
            public async Task NullCumulativeCompletedCreditsTest()
            {
                decimal? expectedCredits = null;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    cumulativeCompletedCredits = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(0, actualEvaluation.CumulativeCompletedCredits);
            }

            [TestMethod]
            public async Task CumulativeCompletedGradePointsTest()
            {
                var expectedCredits = 10m;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    cumulativeCompletedGradePoints = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedCredits, actualEvaluation.CumulativeCompletedGradePoints);
            }

            [TestMethod]
            public async Task NullCumulativeCompletedGradePointsTest()
            {
                decimal? expectedCredits = null;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    cumulativeCompletedGradePoints = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(0, actualEvaluation.CumulativeCompletedGradePoints);
            }

            [TestMethod]
            public async Task EvaluationPeriodAttemptedCreditsTest()
            {
                var expectedCredits = 10m;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    evaluationPeriodAttemptedCredits = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedCredits, actualEvaluation.EvaluationPeriodAttemptedCredits);
            }

            [TestMethod]
            public async Task NullEvaluationPeriodAttemptedCreditsTest()
            {
                decimal? expectedCredits = null;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    evaluationPeriodAttemptedCredits = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(0, actualEvaluation.EvaluationPeriodAttemptedCredits);
            }

            [TestMethod]
            public async Task EvaluationPeriodCompletedCreditsTest()
            {
                var expectedCredits = 10m;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    evaluationPeriodCompletedCredits = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedCredits, actualEvaluation.EvaluationPeriodCompletedCredits);
            }

            [TestMethod]
            public async Task NullEvaluationPeriodCompletedCreditsTest()
            {
                decimal? expectedCredits = null;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    evaluationPeriodCompletedCredits = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(0, actualEvaluation.EvaluationPeriodCompletedCredits);
            }

            [TestMethod]
            public async Task EvaluationPeriodCompletedGradePointsTest()
            {
                var expectedGradePoints = 10m;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = expectedId,
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    evaluationPeriodCompletedGradePoints = expectedGradePoints,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedGradePoints, actualEvaluation.EvaluationPeriodCompletedGradePoints);
            }

            [TestMethod]
            public async Task NullEvaluationPeriodCompletedGradePointsTest()
            {
                decimal? expectedGradePoints = null;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    evaluationPeriodCompletedGradePoints = expectedGradePoints,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(0, actualEvaluation.EvaluationPeriodCompletedGradePoints);
            }

            [TestMethod]
            public async Task CumulativeAttemptedCreditsExcludingRemedialTest()
            {
                var expectedCredits = 10m;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    cumulativeAttemptedCreditsNoRemedial = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedCredits, actualEvaluation.CumulativeAttemptedCreditsExcludingRemedial);
            }

            [TestMethod]
            public async Task NullCumulativeAttemptedCreditsExcludingRemedialTest()
            {
                decimal? expectedCredits = null;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    cumulativeAttemptedCreditsNoRemedial = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(0, actualEvaluation.CumulativeAttemptedCreditsExcludingRemedial);
            }

            [TestMethod]
            public async Task CumulativeCompletedCreditsExcludingRemedialTest()
            {
                var expectedCredits = 10m;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    cumulativeCompletedCreditsNoRemedial = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedCredits, actualEvaluation.CumulativeCompletedCreditsExcludingRemedial);
            }

            [TestMethod]
            public async Task NullCumulativeCompletedCreditsExcludingRemedialTest()
            {
                decimal? expectedCredits = null;
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,
                    cumulativeCompletedCreditsNoRemedial = expectedCredits,
                    evaluationPeriodStartTerm = "2015/FA",
                    evaluationPeriodEndTerm = "2015/FA",
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(0, actualEvaluation.CumulativeCompletedCreditsExcludingRemedial);
            }

            [TestMethod]
            public async Task SetEvaluationPeriodsTest()
            {
                var expectedStartTerm = "2015/FALL";
                var expectedEndTerm = "2015/SPRING";
                var expectedStartDate = DateTime.Today;
                var expectedEndDate = DateTime.Today.AddDays(1);
                var expectedId = "foobar";
                expectedRepository.financialAidStudentData.sapResultsIds.Add(expectedId);
                expectedRepository.SapResultsData.Add(new TestAcademicProgressRepository.SapResultsRecord()
                {
                    id = "foobar",
                    originalStatusCode = "S",
                    evaluationDate = DateTime.Today,  
                    evaluationPeriodStartTerm = expectedStartTerm,
                    evaluationPeriodEndTerm = expectedEndTerm,
                    evaluationPeriodStartDate = expectedStartDate,
                    evaluationPeriodEndDate = expectedEndDate,
                    academicProgram = "MATH.BA"
                });
                actualEvaluations = await actualRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
                var actualEvaluation = actualEvaluations.First(e => e.Id == expectedId);
                Assert.AreEqual(expectedStartTerm, actualEvaluation.EvaluationPeriodStartTerm);
                Assert.AreEqual(expectedEndTerm, actualEvaluation.EvaluationPeriodEndTerm);
            }
        }

        [TestClass]
        public class GetAcademicProgressProgramDetailTests : AcademicProgressRepositoryTests
        {
            private string programCode, catalog;
            private AcademicProgressProgramDetail expectedProgramDetail;

            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressRepositoryTestsInitialize();
                programCode = expectedRepository.programRequirementsData.First().programCode;
                catalog = expectedRepository.programRequirementsData.First().catalogCode;
                expectedProgramDetail = expectedRepository.GetStudentAcademicProgressProgramDetailAsync(programCode, catalog).Result;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullProgramCode_ThrowsArgumentNullExceptionTest()
            {
                await actualRepository.GetStudentAcademicProgressProgramDetailAsync(null, catalog);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullCatalogCode_ThrowsArgumentNullExceptionTest()
            {
                await actualRepository.GetStudentAcademicProgressProgramDetailAsync(programCode, null);
            }

            [TestMethod]
            public async Task GetStudentAcademicProgressProgramDetailAsync_ReturnsExpectedResultTest()
            {
                var actualProgramDetail = await actualRepository.GetStudentAcademicProgressProgramDetailAsync(programCode, catalog);
                Assert.AreEqual(expectedProgramDetail.ProgramCode, actualProgramDetail.ProgramCode);
                Assert.AreEqual(expectedProgramDetail.ProgramMaxCredits, actualProgramDetail.ProgramMaxCredits);
                Assert.AreEqual(expectedProgramDetail.ProgramMinCredits, actualProgramDetail.ProgramMinCredits);
            }

            [TestMethod]
            [ExpectedException (typeof(KeyNotFoundException))]
            public async Task NoAcademicProgramRequirementFound_MessageIsLoggedAndNullReturnedTest()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<AcadProgramReqmts>(It.IsAny<string>(), It.IsAny<string>(), true))
                .ReturnsAsync(() => null);
                try
                {
                    Assert.IsNull(await actualRepository.GetStudentAcademicProgressProgramDetailAsync(programCode, catalog));                    
                }
                catch
                {
                    loggerMock.Verify(l => l.Info("Program '" + programCode + "' for catalog '" + catalog + "'" + "is missing ACAD.PROGRAM.REQMTS record."));
                    throw;
                }
            }
        }

    }
}
