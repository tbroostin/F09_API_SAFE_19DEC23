// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentFinancialAidAcademicProgressStatusesRepositoryTests
    {
        [TestClass]
        public class StudentFinancialAidAcademicProgressStatusesRepositoryTests_V15
        {
            [TestClass]
            public class StudentFinancialAidAcademicProgressStatusesRepositoryTests_GETALL_GETBYID : BaseRepositorySetup
            {
                #region DECLARATIONS

                Collection<SapResults> sapResults;

                private FinAid finAid;

                private StudentFinancialAidAcademicProgressStatusesRepository studentFAAcademicProgressStatusesRepository;

                private Dictionary<string, GuidLookupResult> lookUpResult;

                private string guid = "adcbf49c-f129-470c-aa31-272493846751";

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                    MockInitialize();

                    InitializeTestData();

                    InitializeTestMock();

                    studentFAAcademicProgressStatusesRepository = new StudentFinancialAidAcademicProgressStatusesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                }

                [TestCleanup]
                public void Cleanup()
                {
                    MockCleanup();

                    studentFAAcademicProgressStatusesRepository = null;
                }

                private void InitializeTestData()
                {
                    finAid = new FinAid() { FaCodPersonId = "1", FaCounselor = new List<string>() { "Counselor1", "Counselor2" }, FaCounselorEndDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, FaCounselorsEntityAssociation = new List<FinAidFaCounselors>() { new FinAidFaCounselors() { FaCounselorAssocMember = "FACounselor", FaCounselorEndDateAssocMember = DateTime.Now, FaCounselorStartDateAssocMember = DateTime.Now } }, FaCounselorStartDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, FaCsYears = new List<string>() { "2016", "2017" }, FaInterviews = new List<string>() { "Interviews1", "Interviews2" }, FaIsirNsldsIds = new List<string>() { "nslds1", "nslds2" }, FaPaperCopyOptInFlag = "true", FaPendingLoanRequestIds = new List<string> { "1", "2" }, FaResolvedLoanRequestIds = new List<string>() { "1", "2" }, FaSapResultsId = new List<string>() { "1", "2" }, FaSaYears = new List<string>() { "2016", "2017" }, FaYsYears = new List<string>()  { "2016", "2017" }, Recordkey = "1" };

                    sapResults = new Collection<SapResults>() { new SapResults() { RecordGuid = "adcbf49c-f129-470c-aa31-272493846751", Recordkey = "1", RecordModelName = "", SaprAcadProgram = "Program1", SaprAppealsId = new List<string>(){ "Appeal1", "Appeal2" }, SaprBatchId = "Batch1", SaprCalcSapStatus = "status1", SaprCalcThruDate = DateTime.Now, SaprCalcThruTerm="term1", SaprCumAttCred=10, SaprCumAttCredRem=4, SaprCumCmplCred=3, SaprCumCmplCredRem=4, SaprCumCmplGradePts=10, SaprCumGpaCred=7, SaprCumGpaGradePts=2, SapResultsAdddate = DateTime.Now, SaprEvalPdEndDate = DateTime.Now, SaprEvalPdEndTerm="term1", SaprEvalPdStartDate = DateTime.Now, SaprEvalPdStartTerm="term2", SaprOvrSapDate= DateTime.Now, SaprOvrSapStatus="status1", SaprSapTypeId="type1", SaprStudentId="1", SaprTrmAttCred=10, SaprTrmCmplCred=10, SaprTrmCmplGradePts=10, SaprTrmGpaCred=10, SaprTrmGpaGradePts=10 },
                                                                new SapResults() { RecordGuid = "adcbf49c-f129-470c-aa31-272493846752", Recordkey = "2", RecordModelName = "", SaprAcadProgram = "Program1", SaprAppealsId = new List<string>(){ "Appeal1", "Appeal2" }, SaprBatchId = "Batch1", SaprCalcSapStatus = "status1", SaprCalcThruDate = DateTime.Now, SaprCalcThruTerm="term1", SaprCumAttCred=10, SaprCumAttCredRem=4, SaprCumCmplCred=3, SaprCumCmplCredRem=4, SaprCumCmplGradePts=10, SaprCumGpaCred=7, SaprCumGpaGradePts=2, SapResultsAdddate = DateTime.Now, SaprEvalPdEndDate = DateTime.Now, SaprEvalPdEndTerm="term1", SaprEvalPdStartDate = DateTime.Now, SaprEvalPdStartTerm="term2", SaprSapTypeId="type1", SaprStudentId="1", SaprTrmAttCred=10, SaprTrmCmplCred=10, SaprTrmCmplGradePts=10, SaprTrmGpaCred=10, SaprTrmGpaGradePts=10 },
                                                                new SapResults() { RecordGuid = "adcbf49c-f129-470c-aa31-272493846753", Recordkey = "3", RecordModelName = "", SaprAcadProgram = "Program1", SaprAppealsId = new List<string>(){ "Appeal1", "Appeal2" }, SaprBatchId = "Batch1", SaprCalcSapStatus = "status1", SaprCalcThruTerm="term1", SaprCumAttCred=10, SaprCumAttCredRem=4, SaprCumCmplCred=3, SaprCumCmplCredRem=4, SaprCumCmplGradePts=10, SaprCumGpaCred=7, SaprCumGpaGradePts=2, SaprEvalPdEndTerm="term1", SaprEvalPdStartDate = DateTime.Now, SaprEvalPdStartTerm="term2", SaprOvrSapStatus="status1", SaprStudentId="1", SaprTrmAttCred=10, SaprTrmCmplCred=10, SaprTrmCmplGradePts=10, SaprTrmGpaCred=10, SaprTrmGpaGradePts=10 }
                    };

                    lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "SAP.RESULTS", PrimaryKey = "1", SecondaryKey = "1" } } };
                }

                private void InitializeTestMock()
                {

                    dataReaderMock.Setup(x => x.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(finAid);

                    dataReaderMock.Setup(d => d.SelectAsync("SAP.RESULTS", It.IsAny<string[]>() ,It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "2", "3" }.ToArray<string>());

                    dataReaderMock.Setup(d => d.ReadRecordAsync<SapResults>("SAP.RESULTS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(sapResults.FirstOrDefault());
                    //DataReader.ReadRecordAsync<SapResults>("SAP.RESULTS", recordInfo.PrimaryKey)

                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<SapResults>("SAP.RESULTS", It.IsAny<string[]>(), true)).ReturnsAsync(sapResults);

                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);


                }

                #endregion

                #region GETALL

                [TestMethod]
                public async Task StudentFAAPSRepository_GetSapResultsAsync_PersonId()
                {
                    var result = await studentFAAcademicProgressStatusesRepository.GetSapResultsAsync(0, 10, "1", "","", false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 3);
                }

                [TestMethod]
                public async Task StudentFAAPSRepository_GetSapResultsAsync_StatusId()
                {
                    var result = await studentFAAcademicProgressStatusesRepository.GetSapResultsAsync(0, 10, "", "1", "", false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 3);
                }

                [TestMethod]
                public async Task StudentFAAPSRepository_GetSapResultsAsync_TypeId()
                {
                    var result = await studentFAAcademicProgressStatusesRepository.GetSapResultsAsync(0, 10, "", "", "1", false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 3);
                }

                [TestMethod]
                public async Task StudentFAAPSRepository_GetSapResultsAsync_PersonIdAndStatusId()
                {
                    var result = await studentFAAcademicProgressStatusesRepository.GetSapResultsAsync(0, 10, "1", "1", "", false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 3);
                }

                [TestMethod]
                public async Task StudentFAAPSRepository_GetSapResultsAsync_StatusIdAndTypeId()
                {
                    var result = await studentFAAcademicProgressStatusesRepository.GetSapResultsAsync(0, 10, "", "1", "1", false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 3);
                }

                [TestMethod]
                public async Task StudentFAAPSRepository_GetSapResultsAsync_PersonId_Null()
                {
                    dataReaderMock.Setup(x => x.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                    var result = await studentFAAcademicProgressStatusesRepository.GetSapResultsAsync(0, 10, "1", "", "", false);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task StudentFAAPSRepository_GetSapResultsAsync_Result_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("SAP.RESULTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(() => null);
                    var result = await studentFAAcademicProgressStatusesRepository.GetSapResultsAsync(0, 10, "", "", "", false);
                    Assert.AreEqual(result.Item2, 0);
                }

                #endregion

                #region GETBYID

                [TestMethod]
                public async Task StudentFAAPSRepository_GetSapResultByGuidAsync()
                {
                    var result = await studentFAAcademicProgressStatusesRepository.GetSapResultByGuidAsync(guid);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.RecordGuid, guid);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task StudentFAAPSRepository_GetSapResultByGuidAsync_IdAsNull()
                {
                    await studentFAAcademicProgressStatusesRepository.GetSapResultByGuidAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSRepository_GetSapResultByGuidAsync_PrimaryKeyAsNull()
                {
                    lookUpResult.FirstOrDefault().Value.PrimaryKey = null;
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                    await studentFAAcademicProgressStatusesRepository.GetSapResultByGuidAsync(guid);
                }


                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSRepository_GetSapResultByGuidAsync_EntityAsNull()
                {
                    lookUpResult.FirstOrDefault().Value.Entity = null;
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                    await studentFAAcademicProgressStatusesRepository.GetSapResultByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSRepository_GetSapResultByGuidAsync_InvalidEntity()
                {
                    lookUpResult.FirstOrDefault().Value.Entity = "INVALID";
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                    await studentFAAcademicProgressStatusesRepository.GetSapResultByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSRepository_GetSapResultByGuidAsync_DataContractAsNull()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<SapResults>("SAP.RESULTS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                    await studentFAAcademicProgressStatusesRepository.GetSapResultByGuidAsync(guid);
                }

                #endregion
            }
        }
    }
}
