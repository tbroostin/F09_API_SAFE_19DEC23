/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class AidApplicationResultsRepositoryTests : BaseRepositorySetup
    {
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;

        private UpdateAidApplResultResponse response;

        private AidApplicationResultsRepository aidApplicationResultsRepository;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo> faappDemoList;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults> faappResultsList;
        private Ellucian.Colleague.Domain.FinancialAid.Entities.AidApplicationResults aidApplResultsEntity;

        string criteria = "";
        string assignedCriteria = "";
        string[] ids = new string[] { "1", "2" };
        string expectedRecordKey = "1";
        string id = "1";
        int offset = 0;
        int limit = 4;
        string appDemoId = "1";
        string personId = "0000100";
        string aidYear = "2023";
        string aidApplicationType = "CALISIR";
        int? transactionNumber = 61;
        string applicantAssignedId = "987654321";

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            _iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
            _iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
            _iLoggerMock = new Mock<ILogger>();
            _dataReaderMock = new Mock<IColleagueDataReader>();
            _iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader())
                .Returns(_dataReaderMock.Object);
            _iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker())
                .Returns(transManager);

            apiSettings = new ApiSettings("TEST");

            BuildData();

            aidApplicationResultsRepository = new AidApplicationResultsRepository(cacheProviderMock.Object, _iColleagueTransactionFactoryMock.Object, _iLoggerMock.Object, apiSettings);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _iColleagueTransactionFactoryMock = null;
            _iColleagueTransactionInvokerMock = null;
            _iLoggerMock = null;
            _dataReaderMock = null;
            apiSettings = null;
            aidApplicationResultsRepository = null;
        }

        #region Get By Criteria
        [TestMethod]
        public async Task AidApplicationResultsRepository_GetByCriteria()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(ids, true))
               .ReturnsAsync(faappResultsList);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit);
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(ids.Count(), actuals.Item2);
            Assert.AreEqual(actuals.Item1.Count(), actuals.Item2);
        }

        [TestMethod]
        public async Task AidApplicationResultsRepository_GetByCriteria_AppDemoIdFilter()
        {
            criteria = "WITH FAAPP.CALC.RESULTS.ID = '" + appDemoId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(ids, true))
               .ReturnsAsync(faappResultsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit, appDemoId);

            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationResultsRepository_GetByCriteria_PersonIdFilter()
        {
            criteria = "WITH FAPR.STUDENT.ID = '" + personId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(ids, true))
               .ReturnsAsync(faappResultsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit, "", personId);

            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(ids.Count(), actuals.Item2);
            Assert.AreEqual(actuals.Item1.Count(), actuals.Item2);
        }

        [TestMethod]
        public async Task AidApplicationResultsRepository_GetByCriteria_AidApplicationTypeFilter()
        {
            criteria = "WITH FAPR.TYPE = '" + aidApplicationType + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(ids, true))
               .ReturnsAsync(faappResultsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit, "", "", aidApplicationType);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_PersonId_AidApplTypeFilter()
        {
            criteria = "WITH FAPR.STUDENT.ID = '" + personId + "'" + " AND WITH FAPR.TYPE = '" + aidApplicationType + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(ids, true))
               .ReturnsAsync(faappResultsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit, "", personId, aidApplicationType);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AidYearFilter()
        {
            criteria = "WITH FAPR.YEAR = '" + aidYear + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(ids, true))
               .ReturnsAsync(faappResultsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit, "", "", "", aidYear);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }
        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_TransactionNumberFilter()
        {
            criteria = "WITH FAPR.TRANS.NBR = '" + transactionNumber + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(ids, true))
               .ReturnsAsync(faappResultsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit, "", "", "", "", transactionNumber);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }
        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AidApplicantAssignedIdFilter()
        {
            criteria = "WITH FAAD.ASSIGNED.ID = '" + applicantAssignedId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", "")).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(ids, true))
               .ReturnsAsync(faappResultsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit, "", "", "", "", null,applicantAssignedId);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", ""), Times.Exactly(1));
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", ids, criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }


        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AllFilters()
        {
            criteria = "WITH FAAPP.CALC.RESULTS.ID = '" + appDemoId + "'" + " AND WITH FAPR.STUDENT.ID = '" + personId + "'" + " AND WITH FAPR.TYPE = '" + aidApplicationType + "'" + " AND WITH FAPR.YEAR = '" + aidYear + "'" + " AND WITH FAPR.TRANS.NBR = '" + transactionNumber + "'";
            assignedCriteria = "WITH FAAD.ASSIGNED.ID = '" + applicantAssignedId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", ids, assignedCriteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(ids, true))
               .ReturnsAsync(faappResultsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit, appDemoId, personId, aidApplicationType, aidYear, transactionNumber,applicantAssignedId);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.CALC.RESULTS", criteria), Times.Exactly(1));
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", ids, assignedCriteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        #endregion

        #region Get By ID

        [TestMethod]
        public async Task AidApplicationResultsRepository_GetByID()
        {
            var result = await aidApplicationResultsRepository.GetAidApplicationResultsByIdAsync(id);
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRecordKey, result.Id);
            Assert.AreEqual(personId, result.PersonId);
            Assert.AreEqual(aidYear, result.AidYear);
            Assert.AreEqual(aidApplicationType, result.AidApplicationType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationResultsRepository_GetByID_IdEmpty_Exception()
        {
            var result = await aidApplicationResultsRepository.GetAidApplicationResultsByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationResultsRepository_GetByID_IdNull_Exception()
        {
            var result = await aidApplicationResultsRepository.GetAidApplicationResultsByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AidApplicationResultsRepository_GetByID_DataReader_Exception()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(expectedRecordKey, true)).ReturnsAsync(() => null);
            var result = await aidApplicationResultsRepository.GetAidApplicationResultsByIdAsync(id);
        }

        #endregion

        #region PUT
        [TestMethod]
        public async Task AidApplicationResultsRepository_PUT()
        {
            var actual = await aidApplicationResultsRepository.UpdateAidApplicationResultsAsync(aidApplResultsEntity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedRecordKey, actual.Id);
            Assert.AreEqual(personId, actual.PersonId);
            Assert.AreEqual(aidYear, actual.AidYear);
            Assert.AreEqual(aidApplicationType, actual.AidApplicationType);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationResultsRepository_PUT_Entity_Null_ArgumentNullException()
        {
            var actual = await aidApplicationResultsRepository.UpdateAidApplicationResultsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationResultsRepository_PUT_EntityId_Null_ArgumentNullException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplicationResults(" ", " ");
            var actual = await aidApplicationResultsRepository.UpdateAidApplicationResultsAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationResultsRepository_PUT_PersonId_Null_RepositoryException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplicationResults(expectedRecordKey, expectedRecordKey);
            var actual = await aidApplicationResultsRepository.UpdateAidApplicationResultsAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationResultsRepository_PUT_Person_Invalid_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationResultsRepository.UpdateAidApplicationResultsAsync(aidApplResultsEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationResultsRepository_PUT_ExecuteAsync_Error_RepositoryException()
        {
            response.UpdateAidApplResultErrors = new List<UpdateAidApplResultErrors>()
            {
                new UpdateAidApplResultErrors()
                {
                   ErrorCode = "ABC",
                   ErrorMessage = "Execute error in CTX."
                }
            };

            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplResultRequest, UpdateAidApplResultResponse>(It.IsAny<UpdateAidApplResultRequest>())).ReturnsAsync(response);

            var actual = await aidApplicationResultsRepository.UpdateAidApplicationResultsAsync(aidApplResultsEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationResultsRepository_PUT_GetByID_Returns_Null_Error_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationResultsRepository.UpdateAidApplicationResultsAsync(aidApplResultsEntity);
        }


        #endregion

        #region POST
        [TestMethod]
        public async Task AidApplicationResultsRepository_POST()
        {
            aidApplResultsEntity = new Domain.FinancialAid.Entities.AidApplicationResults("1", "1");
            aidApplResultsEntity.PersonId = personId;
            aidApplResultsEntity.AidYear = aidYear;
            aidApplResultsEntity.AidApplicationType = aidApplicationType;
            aidApplResultsEntity.ApplicantAssignedId = applicantAssignedId;
            var actual = await aidApplicationResultsRepository.CreateAidApplicationResultsAsync(aidApplResultsEntity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedRecordKey, actual.Id);
            Assert.AreEqual(personId, actual.PersonId);
            Assert.AreEqual(aidYear, actual.AidYear);
            Assert.AreEqual(aidApplicationType, actual.AidApplicationType);
        }



        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationResultsRepository_POST_Entity_Null_ArgumentNullException()
        {
            var actual = await aidApplicationResultsRepository.CreateAidApplicationResultsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationResultsRepository_POST_ExecuteAsync_Error_RepositoryException()
        {
            response.UpdateAidApplResultErrors = new List<UpdateAidApplResultErrors>()
            {
                new UpdateAidApplResultErrors()
                {
                   ErrorCode = "ABC",
                   ErrorMessage = "Execute error in CTX."
                }
            };

            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplResultRequest, UpdateAidApplResultResponse>(It.IsAny<UpdateAidApplResultRequest>())).ReturnsAsync(response);

            var actual = await aidApplicationResultsRepository.UpdateAidApplicationResultsAsync(aidApplResultsEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationResultsRepository_POST_GetByID_Returns_Null_ArgumentNullException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationResultsRepository.CreateAidApplicationResultsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationResultsRepository_POST_PersonId_Returns_Null_ArgumentNullException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationResultsRepository.CreateAidApplicationResultsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationResultsRepository_POST_PersonId_Null_RepositoryException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplicationResults(expectedRecordKey, expectedRecordKey);
            var actual = await aidApplicationResultsRepository.CreateAidApplicationResultsAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationResultsRepository_POST_Person_Invalid_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationResultsRepository.CreateAidApplicationResultsAsync(aidApplResultsEntity); ;
        }

        #endregion

        private void BuildData()
        {
            faappResultsList = new Collection<DataContracts.FaappCalcResults>()
            {
                new DataContracts.FaappCalcResults()
                {
                    Recordkey = "1",
                    FaprStudentId = personId,
                    FaprYear = aidYear,
                    FaprType = aidApplicationType,
                    FaprDepStatus = "I",
                    FaprDepOver = "1",
                    FaprDepOverSchoolCode = "E20234",
                    FaprTransDataSourceType = "4B",
                    FaprTransRcptDate = new DateTime(2001, 1, 20),
                    FaprSpecialCircumstances = "2",
                    FaprPAssetTholdExc = "true",
                    FaprSAssetTholdExc = "true",
                    FaprEtiDestNbr = "TG87902",
                    FaprCurrPseudoId = "675023526",
                    FaprCorrAppliedAgainst = "15",
                    FaprProfJudgInd ="2",
                    FaprApplDataSourceType = "2B",
                    FaprApplRcptDate = new DateTime(2001, 1, 20),
                    FaprAddrOnlyChgFlag = "4",
                    FaprPushedIsirFlag = "True",
                    FaprEfcChgFlag = "2",
                    FaprSLastNameChgFlag = "0",
                    FaprRejChgFlag = "false",
                    FaprSarcChgFlag = "false",
                    FaprComputeNbr = "325",
                    FaprCorrSource = "S",
                    FaprDupPid = "true",
                    FaprGradFlag ="false",
                    FaprTransProcDate = new DateTime(2000, 1, 30),
                    FaprProcRecordType="H",
                    FaprRejectCodes=new List<string>(){ "r1" },
                    FaprAzeInd="false",
                    FaprSntInd="1",
                    FaprPCalcTaxStatus="3",
                    FaprSCalcTaxStatus="3",
                    FaprSAddlFinInfoTotal=165784,
                    FaprSOthUntxInc=267234,
                    FaprPAddlFinInfoTotal=797356,
                    FaprPOthUntxInc=128643,
                    FaprHsInvalidFlag="false",
                    FaprSecEfc=667253289,
                    FaprSignRejEfc=934567823,
                    FaprPriEfcType="6",
                    FaprPriAlt1mnthEfc=556789,
                    FaprPriAlt2mnthEfc=765432,
                    FaprPriAlt3mnthEfc=778905,
                    FaprPriAlt4mnthEfc=523456,
                    FaprPriAlt5mnthEfc=526789,
                    FaprPriAlt6mnthEfc=812345,
                    FaprPriAlt7mnthEfc=423456,
                    FaprPriAlt8mnthEfc=356789,
                    FaprPriAlt10mnthEfc=323412,
                    FaprPriAlt11mnthEfc=212234,
                    FaprPriAlt12mnthEfc=312345,
                    FaprPriTi=36533784,
                    FaprPriAti=4234567,
                    FaprPriStx=2234567,
                    FaprPriEa=28546482,
                    FaprPriIpa=233456,
                    FaprPriAi=2234561,
                    FaprPriCai=1123466,
                    FaprPriDnw=2468954,
                    FaprPriNw=2345623,
                    FaprPriApa=234568,
                    FaprPriPca=3234567,
                    FaprPriAai=2123986,
                    FaprPriTsc=3123456,
                    FaprPriPc=32398645,
                    FaprPriSti=66344757,
                    FaprPriSati=2896543,
                    FaprPriSic=32437865,
                    FaprPriSdnw=2345765,
                    FaprPriSca=2453638,
                    FaprPriFti=52754389,
                    FaprCorrectionFlags="Test",
                    FaprHighlightFlags="Test1",
                    FaprCommentCodes=new List<string>() { "c1" },
                    FaprElecSchoolInd="1",
                    FaprEtiFlag="Y",
                    FaprSelectedForVerif="*",
                    FaprAssumCitizenship="1",
                    FaprAssumSMarStat="2",
                    FaprAssumSAgi=3675432,
                    FaprAssumSTaxPd=29874258,
                    FaprAssumSIncWork=265123455,
                    FaprAssumSpIncWork=297435609,
                    FaprAssumSAddlFinAmt=241689446,
                    FaprAssumBirthDatePrior="2",
                    FaprAssumSMarried="2",
                    FaprAssumLegalDep="2",
                    FaprAssumChildren="2",
                    FaprAssumSNbrFamily=99,
                    FaprAssumSNbrCollege=1,
                    FaprAssumSAssetTholdExc="false",
                    FaprAssumPMarStat="1",
                    FaprAssumPar1Ssn="true",
                    FaprAssumPar2Ssn="false",
                    FaprAssumPNbrFamily=82,
                    FaprAssumPNbrCollege=7,
                    FaprAssumPAgi=25637384,
                    FaprAssumPTaxPd=3256389,
                    FaprAssumPar1Income=3425673,
                    FaprAssumPar2Income=36354278,
                    FaprAssumPAddlFinAmt=52536732,
                    FaprAssumPAssetTholdExc="true",
                    FaprTransNbr=1,
                    FaprPriEfc=5677842,
                },
                new DataContracts.FaappCalcResults()
                {
                     Recordkey = "2",
                     FaprStudentId = personId,
                    FaprYear = aidYear,
                    FaprType = aidApplicationType,
                    FaprDepStatus = "I",
                    FaprDepOver = "1",
                    FaprDepOverSchoolCode = "E20234",
                    FaprTransDataSourceType = "4B",
                    FaprTransRcptDate = new DateTime(2001, 1, 20),
                    FaprSpecialCircumstances = "2",
                    FaprPAssetTholdExc = "true",
                    FaprSAssetTholdExc = "true",
                    FaprEtiDestNbr = "TG87902",
                    FaprCurrPseudoId = "675023526",
                    FaprCorrAppliedAgainst = "15",
                    FaprProfJudgInd ="2",
                    FaprApplDataSourceType = "2B",
                    FaprApplRcptDate = new DateTime(2001, 1, 20),
                    FaprAddrOnlyChgFlag = "4",
                    FaprPushedIsirFlag = "True",
                    FaprEfcChgFlag = "2",
                    FaprSLastNameChgFlag = "0",
                    FaprRejChgFlag = "false",
                    FaprSarcChgFlag = "false",
                    FaprComputeNbr = "325",
                    FaprCorrSource = "S",
                    FaprDupPid = "true",
                    FaprGradFlag ="false",
                    FaprTransProcDate = new DateTime(2000, 1, 30),
                    FaprProcRecordType="H",
                    FaprRejectCodes=new List<string>(){ "r1" },
                    FaprAzeInd="false",
                    FaprSntInd="1",
                    FaprPCalcTaxStatus="3",
                    FaprSCalcTaxStatus="3",
                    FaprSAddlFinInfoTotal=165784,
                    FaprSOthUntxInc=267234,
                    FaprPAddlFinInfoTotal=797356,
                    FaprPOthUntxInc=128643,
                    FaprHsInvalidFlag="false",
                    FaprSecEfc=667253289,
                    FaprSignRejEfc=934567823,
                    FaprPriEfcType="6",
                    FaprPriAlt1mnthEfc=556789,
                    FaprPriAlt2mnthEfc=765432,
                    FaprPriAlt3mnthEfc=778905,
                    FaprPriAlt4mnthEfc=523456,
                    FaprPriAlt5mnthEfc=526789,
                    FaprPriAlt6mnthEfc=812345,
                    FaprPriAlt7mnthEfc=423456,
                    FaprPriAlt8mnthEfc=356789,
                    FaprPriAlt10mnthEfc=323412,
                    FaprPriAlt11mnthEfc=212234,
                    FaprPriAlt12mnthEfc=312345,
                    FaprPriTi=36533784,
                    FaprPriAti=4234567,
                    FaprPriStx=2234567,
                    FaprPriEa=28546482,
                    FaprPriIpa=233456,
                    FaprPriAi=2234561,
                    FaprPriCai=1123466,
                    FaprPriDnw=2468954,
                    FaprPriNw=2345623,
                    FaprPriApa=234568,
                    FaprPriPca=3234567,
                    FaprPriAai=2123986,
                    FaprPriTsc=3123456,
                    FaprPriPc=32398645,
                    FaprPriSti=66344757,
                    FaprPriSati=2896543,
                    FaprPriSic=32437865,
                    FaprPriSdnw=2345765,
                    FaprPriSca=2453638,
                    FaprPriFti=52754389,
                    FaprCorrectionFlags="Test",
                    FaprHighlightFlags="Test1",
                    FaprCommentCodes=new List<string>() { "c1" },
                    FaprElecSchoolInd="1",
                    FaprEtiFlag="Y",
                    FaprSelectedForVerif="*",
                    FaprAssumCitizenship="1",
                    FaprAssumSMarStat="2",
                    FaprAssumSAgi=3675432,
                    FaprAssumSTaxPd=29874258,
                    FaprAssumSIncWork=265123455,
                    FaprAssumSpIncWork=297435609,
                    FaprAssumSAddlFinAmt=241689446,
                    FaprAssumBirthDatePrior="2",
                    FaprAssumSMarried="2",
                    FaprAssumLegalDep="2",
                    FaprAssumChildren="2",
                    FaprAssumSNbrFamily=99,
                    FaprAssumSNbrCollege=1,
                    FaprAssumSAssetTholdExc="false",
                    FaprAssumPMarStat="1",
                    FaprAssumPar1Ssn="true",
                    FaprAssumPar2Ssn="false",
                    FaprAssumPNbrFamily=82,
                    FaprAssumPNbrCollege=7,
                    FaprAssumPAgi=25637384,
                    FaprAssumPTaxPd=3256389,
                    FaprAssumPar1Income=3425673,
                    FaprAssumPar2Income=36354278,
                    FaprAssumPAddlFinAmt=52536732,
                    FaprAssumPAssetTholdExc="true",
                    FaprTransNbr=1,
                    FaprPriEfc=5677842




                }
            };

            faappDemoList = new Collection<DataContracts.FaappDemo>()
            {
                new DataContracts.FaappDemo()
                {
                    Recordkey = "1",
                    FaadStudentId = personId,
                    FaadYear = aidYear,
                    FaadType = aidApplicationType,
                    FaadAssignedId = applicantAssignedId,
                    FaadNameFirst = "TestFirstName",
                    FaadNameLast = "TestLastName",
                    FaadNameMi = "TestInitials",
                    FaadOrigName = "LN",
                    FaadAddr = "Street1",
                    FaadCity = "Virginia",
                    FaadState = "PA",
                    FaadCountry = "US",
                    FaadZip = "10000",
                    FaadBirthdate = new DateTime(2000, 1, 30),
                    FaadPhone = "999-0000-000",
                    FaadStudentEmailAddr = "test@testing.com",
                    FaadCitizenship = "1",
                    FaadAlternateNumber = "111-0000-1000",
                    FaadItin = "1234567"
                },
                new DataContracts.FaappDemo()
                {
                    Recordkey = "2",
                    FaadStudentId = "0000200",
                    FaadYear = "2023",
                    FaadType = "CALISIR",
                    FaadAssignedId = "987654322",
                    FaadNameFirst = "TestFirstName",
                    FaadNameLast = "TestLastName",
                    FaadNameMi = "TestInitials",
                    FaadOrigName = "LN",
                    FaadAddr = "Street1",
                    FaadCity = "Virginia",
                    FaadState = "PA",
                    FaadCountry = "US",
                    FaadZip = "10000",
                    FaadBirthdate = new DateTime(2000, 1, 30),
                    FaadPhone = "999-0000-000",
                    FaadStudentEmailAddr = "test@testing.com",
                    FaadCitizenship = "2",
                    FaadAlternateNumber = "111-0000-1000",
                    FaadItin = "12345678"
                }
            };
            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllAidApplicationResultsFilter:",
                Entity = "FAAPP.CALC.RESULTS",
                Sublist = ids.ToList(),
                TotalCount = ids.ToList().Count,
                KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
            };
            _iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(_iColleagueTransactionInvokerMock.Object);
            _iColleagueTransactionInvokerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                   .ReturnsAsync(resp);

            aidApplResultsEntity = new Domain.FinancialAid.Entities.AidApplicationResults("1", "1");
            //change this

            aidApplResultsEntity.PersonId = "0000100";
            aidApplResultsEntity.AidYear = "2023";
            aidApplResultsEntity.AidApplicationType = "CALISIR";
            aidApplResultsEntity.ApplicantAssignedId = "987654321";
            aidApplResultsEntity.TransactionNumber = 1;
            aidApplResultsEntity.DependencyOverride = "1";
            aidApplResultsEntity.DependencyOverSchoolCode = "E20234";
            aidApplResultsEntity.DependencyStatus = "I";
            aidApplResultsEntity.TransactionSource = "4B";
            aidApplResultsEntity.TransactionReceiptDate = new DateTime(2001, 1, 20);
            aidApplResultsEntity.SpecialCircumstances = "2";
            aidApplResultsEntity.ParentAssetExceeded = true;
            aidApplResultsEntity.StudentAssetExceeded = true;
            aidApplResultsEntity.DestinationNumber = "TG87902";
            aidApplResultsEntity.StudentCurrentPseudoId = "675023526";
            aidApplResultsEntity.CorrectionAppliedAgainst = "15";
            aidApplResultsEntity.ProfJudgementIndicator = "1";
            aidApplResultsEntity.ApplicationDataSource = "2B";
            aidApplResultsEntity.ApplicationReceiptDate = new DateTime(2001, 1, 20);
            aidApplResultsEntity.AddressOnlyChangeFlag = "4";
            aidApplResultsEntity.PushedApplicationFlag = true;
            aidApplResultsEntity.EfcChangeFlag = "1";
            aidApplResultsEntity.LastNameChange = "N";
            aidApplResultsEntity.RejectStatusChange = false;
            aidApplResultsEntity.SarcChange = false;
            aidApplResultsEntity.ComputeNumber = "325";
            aidApplResultsEntity.CorrectionSource = "S";
            aidApplResultsEntity.DuplicateIdIndicator = true;
            aidApplResultsEntity.GraduateFlag = false;
            aidApplResultsEntity.TransactionProcessedDate = new DateTime(2003, 11, 22);
            aidApplResultsEntity.ProcessedRecordType = "H";
            aidApplResultsEntity.RejectReasonCodes = new List<string>() { "r1" };
            aidApplResultsEntity.AutomaticZeroIndicator = false;
            aidApplResultsEntity.SimplifiedNeedsTest = "N";
            aidApplResultsEntity.ParentCalculatedTaxStatus = "3";
            aidApplResultsEntity.StudentCalculatedTaxStatus = "3";
            aidApplResultsEntity.StudentAddlFinCalcTotal = 20602400;
            aidApplResultsEntity.studentOthUntaxIncomeCalcTotal = 20802300;
            aidApplResultsEntity.ParentAddlFinCalcTotal = 13000208;
            aidApplResultsEntity.ParentOtherUntaxIncomeCalcTotal = 40802400;
            aidApplResultsEntity.InvalidHighSchool = false;
            aidApplResultsEntity.AssumCitizenship = "1";
            aidApplResultsEntity.AssumSMarStat = "1";
            aidApplResultsEntity.AssumSAgi = 4182240;
            aidApplResultsEntity.AssumSTaxPd = 5130135;
            aidApplResultsEntity.AssumSIncWork = 1291250;
            aidApplResultsEntity.AssumSpIncWork = 7540278;
            aidApplResultsEntity.AssumSAddlFinAmt = 22703403;
            aidApplResultsEntity.AssumBirthDatePrior = "2";
            aidApplResultsEntity.AssumSMarried = "2";
            aidApplResultsEntity.AssumChildren = "2";
            aidApplResultsEntity.AssumLegalDep = "2";
            aidApplResultsEntity.AssumSNbrFamily = 99;
            aidApplResultsEntity.AssumSNbrCollege = 1;
            aidApplResultsEntity.AssumSAssetTholdExc = false;
            aidApplResultsEntity.AssumPMarStat = "1";
            aidApplResultsEntity.AssumPar1Ssn = true;
            aidApplResultsEntity.AssumPar2Ssn = false;
            aidApplResultsEntity.AssumPNbrFamily = 82;
            aidApplResultsEntity.AssumPNbrCollege = 7;
            aidApplResultsEntity.AssumPAgi = 3322502;
            aidApplResultsEntity.AssumPTaxPd = 4530234;
            aidApplResultsEntity.AssumPar1Income = 4260275;
            aidApplResultsEntity.AssumPar2Income = 2350357;
            aidApplResultsEntity.AssumPAddlFinAmt = 15213506;
            aidApplResultsEntity.AssumPAssetTholdExc = true;
            aidApplResultsEntity.PrimaryEfc = 351250;
            aidApplResultsEntity.SecondaryEfc = 572026;
            aidApplResultsEntity.SignatureRejectEfc = 662032;
            aidApplResultsEntity.PrimaryEfcType = "6";
            aidApplResultsEntity.PriAlt1mnthEfc = 345671;
            aidApplResultsEntity.PriAlt2mnthEfc = 345672;
            aidApplResultsEntity.PriAlt3mnthEfc = 345673;
            aidApplResultsEntity.PriAlt4mnthEfc = 345674;
            aidApplResultsEntity.PriAlt5mnthEfc = 345675;
            aidApplResultsEntity.PriAlt6mnthEfc = 345676;
            aidApplResultsEntity.PriAlt7mnthEfc = 345677;
            aidApplResultsEntity.PriAlt8mnthEfc = 345678;
            aidApplResultsEntity.PriAlt10mnthEfc = 345680;
            aidApplResultsEntity.PriAlt11mnthEfc = 345681;
            aidApplResultsEntity.PriAlt12mnthEfc = 345682;
            aidApplResultsEntity.TotalIncome = 72370346;
            aidApplResultsEntity.AllowancesAgainstTotalIncome = 3234512;
            aidApplResultsEntity.TaxAllowance = 8314355;
            aidApplResultsEntity.EmploymentAllowance = 2685413;
            aidApplResultsEntity.IncomeProtectionAllowance = 3459835;
            aidApplResultsEntity.AvailableIncome = 83383552;
            aidApplResultsEntity.AvailableIncomeContribution = 3349835;
            aidApplResultsEntity.DiscretionaryNetWorth = 432498350;
            aidApplResultsEntity.NetWorth = 598512160;
            aidApplResultsEntity.AssetProtectionAllowance = 426286233;
            aidApplResultsEntity.ParentContributionAssets = 962861;
            aidApplResultsEntity.AdjustedAvailableIncome = 49628617;
            aidApplResultsEntity.TotalPrimaryStudentContribution = 1985426;
            aidApplResultsEntity.TotalPrimaryParentContribution = 5459833;
            aidApplResultsEntity.ParentContribution = 3285134;
            aidApplResultsEntity.StudentTotalIncome = 79628618;
            aidApplResultsEntity.StudentAllowanceAgainstIncome = 5962823;
            aidApplResultsEntity.DependentStudentIncContrib = 4328261;
            aidApplResultsEntity.StudentDiscretionaryNetWorth = 373598352;
            aidApplResultsEntity.StudentAssetContribution = 6459839;
            aidApplResultsEntity.FisapTotalIncome = 49853222;
            aidApplResultsEntity.CorrectionFlags = "Test10";
            aidApplResultsEntity.HighlightFlags = "Test11";
            aidApplResultsEntity.CommentCodes = new List<string>() { "c1" };
            aidApplResultsEntity.ElectronicFedSchoolCodeInd = "1";
            aidApplResultsEntity.ElectronicTransactionIndicator = "Y";
            aidApplResultsEntity.VerificationSelected = "*";

            var expectedAppResults = faappResultsList.FirstOrDefault(i => i.Recordkey.Equals(expectedRecordKey));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappCalcResults>(expectedRecordKey, true)).ReturnsAsync(expectedAppResults);
            var expectedAppDemo = faappDemoList.FirstOrDefault(i => i.Recordkey.Equals(expectedRecordKey));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(expectedRecordKey, true)).ReturnsAsync(expectedAppDemo);


            response = new UpdateAidApplResultResponse() { ApplResultsId = "1", UpdateAidApplResultErrors = new List<UpdateAidApplResultErrors>(), ErrorFlag = false };
            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplResultRequest, UpdateAidApplResultResponse>(It.IsAny<UpdateAidApplResultRequest>())).ReturnsAsync(response);

            var personList = new Collection<Base.DataContracts.Person>()
            {
                new Base.DataContracts.Person(){Recordkey = personId}
            };
            var expectedPerson = personList.FirstOrDefault(i => i.Recordkey.Equals(personId));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(expectedPerson);

        }
    }
}
