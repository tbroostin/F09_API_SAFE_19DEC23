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
    public class AidApplicationsRepositoryTests : BaseRepositorySetup
    {
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;

        private UpdateAidApplResponse response;

        private AidApplicationsRepository aidApplicationsRepository;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo> faappDemoList;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps> faappAppsList;
        private Ellucian.Colleague.Domain.FinancialAid.Entities.AidApplications aidApplEntity;

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

            aidApplicationsRepository = new AidApplicationsRepository(cacheProviderMock.Object, _iColleagueTransactionFactoryMock.Object, _iLoggerMock.Object, apiSettings);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _iColleagueTransactionFactoryMock = null;
            _iColleagueTransactionInvokerMock = null;
            _iLoggerMock = null;
            _dataReaderMock = null;
            apiSettings = null;
            aidApplicationsRepository = null;
        }

        #region Get By Criteria
        [TestMethod]
        public async Task AidApplicationsRepository_GetByCriteria()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.APPS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(ids, true))
               .ReturnsAsync(faappAppsList);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationsRepository.GetAidApplicationsAsync(offset, limit);
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(ids.Count(), actuals.Item2);
            Assert.AreEqual(actuals.Item1.Count(), actuals.Item2);
        }

        [TestMethod]
        public async Task AidApplicationsRepository_GetByCriteria_AppDemoIdFilter()
        {
            criteria = "WITH FAAPP.APPS.ID = '" + appDemoId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.APPS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(ids, true))
               .ReturnsAsync(faappAppsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationsRepository.GetAidApplicationsAsync(offset, limit, appDemoId);

            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.APPS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationsRepository_GetByCriteria_PersonIdFilter()
        {
            criteria = "WITH FAAA.STUDENT.ID = '" + personId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.APPS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(ids, true))
               .ReturnsAsync(faappAppsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationsRepository.GetAidApplicationsAsync(offset, limit, "", personId);

            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.APPS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(ids.Count(), actuals.Item2);
            Assert.AreEqual(actuals.Item1.Count(), actuals.Item2);
        }

        [TestMethod]
        public async Task AidApplicationsRepository_GetByCriteria_AidApplicationTypeFilter()
        {
            criteria = "WITH FAAA.TYPE = '" + aidApplicationType + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.APPS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(ids, true))
               .ReturnsAsync(faappAppsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationsRepository.GetAidApplicationsAsync(offset, limit, "", "", aidApplicationType);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.APPS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationsRepository_GetByCriteria_PersonId_AidApplTypeFilter()
        {
            criteria = "WITH FAAA.STUDENT.ID = '" + personId + "'" + " AND WITH FAAA.TYPE = '" + aidApplicationType + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.APPS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(ids, true))
               .ReturnsAsync(faappAppsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationsRepository.GetAidApplicationsAsync(offset, limit, "", personId, aidApplicationType);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.APPS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationsRepository_GetByCriteria_AidYearFilter()
        {
            criteria = "WITH FAAA.YEAR = '" + aidYear + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.APPS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(ids, true))
               .ReturnsAsync(faappAppsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationsRepository.GetAidApplicationsAsync(offset, limit, "", "", "", aidYear);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.APPS", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationsRepository_GetByCriteria_AidApplicantAssignedIdFilter()
        {
            criteria = "WITH FAAD.ASSIGNED.ID = '" + applicantAssignedId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.APPS", "")).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(ids, true))
               .ReturnsAsync(faappAppsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationsRepository.GetAidApplicationsAsync(offset, limit, "", "", "", "", applicantAssignedId);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.APPS", ""), Times.Exactly(1));
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", ids, criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }


        [TestMethod]
        public async Task AidApplicationsRepository_GetByCriteria_AllFilters()
        {
            criteria = "WITH FAAPP.APPS.ID = '" + appDemoId + "'" + " AND WITH FAAA.STUDENT.ID = '" + personId + "'" + " AND WITH FAAA.TYPE = '" + aidApplicationType + "'" + " AND WITH FAAA.YEAR = '" + aidYear + "'";
            assignedCriteria = "WITH FAAD.ASSIGNED.ID = '" + applicantAssignedId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.APPS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", ids, assignedCriteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(ids, true))
               .ReturnsAsync(faappAppsList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationsRepository.GetAidApplicationsAsync(offset, limit, appDemoId, personId, aidApplicationType, aidYear, applicantAssignedId);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.APPS", criteria), Times.Exactly(1));
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", ids, assignedCriteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        #endregion

        #region Get By ID

        [TestMethod]
        public async Task AidApplicationsRepository_GetByID()
        {
            var result = await aidApplicationsRepository.GetAidApplicationsByIdAsync(id);
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRecordKey, result.Id);
            Assert.AreEqual(personId, result.PersonId);
            Assert.AreEqual(aidYear, result.AidYear);
            Assert.AreEqual(aidApplicationType, result.AidApplicationType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationsRepository_GetByID_IdEmpty_Exception()
        {
            var result = await aidApplicationsRepository.GetAidApplicationsByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationsRepository_GetByID_IdNull_Exception()
        {
            var result = await aidApplicationsRepository.GetAidApplicationsByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AidApplicationsRepository_GetByID_DataReader_Exception()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(expectedRecordKey, true)).ReturnsAsync(() => null);
            var result = await aidApplicationsRepository.GetAidApplicationsByIdAsync(id);
        }

        #endregion

        #region PUT
        [TestMethod]
        public async Task AidApplicationsRepository_PUT()
        {
            var actual = await aidApplicationsRepository.UpdateAidApplicationsAsync(aidApplEntity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedRecordKey, actual.Id);
            Assert.AreEqual(personId, actual.PersonId);
            Assert.AreEqual(aidYear, actual.AidYear);
            Assert.AreEqual(aidApplicationType, actual.AidApplicationType);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationsRepository_PUT_Entity_Null_ArgumentNullException()
        {
            var actual = await aidApplicationsRepository.UpdateAidApplicationsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationsRepository_PUT_EntityId_Null_ArgumentNullException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplications(" ", " ");
            var actual = await aidApplicationsRepository.UpdateAidApplicationsAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationsRepository_PUT_PersonId_Null_RepositoryException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplications(expectedRecordKey, expectedRecordKey);
            var actual = await aidApplicationsRepository.UpdateAidApplicationsAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationsRepository_PUT_Person_Invalid_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationsRepository.UpdateAidApplicationsAsync(aidApplEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationsRepository_PUT_ExecuteAsync_Error_RepositoryException()
        {
            response.ErrorFlag = true;
            response.ErrorCode = new List<string>() { "ABC" };
            response.ErrorMessage = new List<string>() { "Execute error in CTX." };

            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplRequest, UpdateAidApplResponse>(It.IsAny<UpdateAidApplRequest>())).ReturnsAsync(response);

            var actual = await aidApplicationsRepository.UpdateAidApplicationsAsync(aidApplEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationsRepository_PUT_GetByID_Returns_Null_Error_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationsRepository.UpdateAidApplicationsAsync(aidApplEntity);
        }


        #endregion

        #region POST
        [TestMethod]
        public async Task AidApplicationsRepository_POST()
        {
            aidApplEntity = new Domain.FinancialAid.Entities.AidApplications("1", "1");
            aidApplEntity.PersonId = personId;
            aidApplEntity.AidYear = aidYear;
            aidApplEntity.AidApplicationType = aidApplicationType;
            aidApplEntity.AssignedID = applicantAssignedId;
            var actual = await aidApplicationsRepository.CreateAidApplicationsAsync(aidApplEntity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedRecordKey, actual.Id);
            Assert.AreEqual(personId, actual.PersonId);
            Assert.AreEqual(aidYear, actual.AidYear);
            Assert.AreEqual(aidApplicationType, actual.AidApplicationType);
        }



        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationsRepository_POST_Entity_Null_ArgumentNullException()
        {
            var actual = await aidApplicationsRepository.CreateAidApplicationsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationsRepository_POST_ExecuteAsync_Error_RepositoryException()
        {
            response.ErrorFlag = true;
            response.ErrorCode = new List<string>() { "ABC" };
            response.ErrorMessage = new List<string>() { "Execute error in CTX." };

            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplRequest, UpdateAidApplResponse>(It.IsAny<UpdateAidApplRequest>())).ReturnsAsync(response);
            var actual = await aidApplicationsRepository.UpdateAidApplicationsAsync(aidApplEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationsRepository_POST_GetByID_Returns_Null_ArgumentNullException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationsRepository.CreateAidApplicationsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationsRepository_POST_PersonId_Returns_Null_ArgumentNullException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationsRepository.CreateAidApplicationsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationsRepository_POST_PersonId_Null_RepositoryException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplications(expectedRecordKey, expectedRecordKey);
            var actual = await aidApplicationsRepository.CreateAidApplicationsAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationsRepository_POST_Person_Invalid_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationsRepository.CreateAidApplicationsAsync(aidApplEntity); ;
        }

        #endregion

        private void BuildData()
        {
            faappAppsList = new Collection<DataContracts.FaappApps>()
            {
                new DataContracts.FaappApps()
                {
                    Recordkey = "1",
                    FaaaStudentId = personId,
                    FaaaYear = aidYear,
                    FaaaType = aidApplicationType,
                    FaaaBornB4Dt = "false",
                    FaaaMarried = "true",
                    FaaaGradProf = "false",
                    FaaaActiveDuty = "true",
                    FaaaVeteran = "false",
                    FaaaDependChildren = "true",
                    FaaaOtherDepend = "false",
                    FaaaOrphanWard = "true",
                    FaaaEmancipatedMinor = "false",
                    FaaaLegalGuardianship = "true",
                    FaaaHomelessBySchool = "true",
                    FaaaHomelessByHud = "false",
                    FaaaHomelessAtRisk = "true",
                    FaaaSTaxReturnFiled = "1",
                    FaaaSTaxFormType = "1",
                    FaaaSTaxFilingStatus = "1",
                    FaaaSSched1 = "Test1",
                    FaaaSAgi = "102938",
                    FaaaSUsTaxPd = 102937,
                    FaaaSStudentInc = "10247",
                    FaaaSpouseInc = "102847",
                    FaaaSCash = 462816,
                    FaaaSInvNetWorth = 293746,
                    FaaaSBusNetWorth = 483726,
                    FaaaSEduCredit = 492615,
                    FaaaSChildSupPaid = 492715,
                    FaaaSNeedBasedEmp = 301725,
                    FaaaSGrantScholAid = 301725,
                    FaaaSCombatPay = 301725,
                    FaaaSCoOpEarnings = 301725,
                    FaaaSPensionPayments = 301726,
                    FaaaSIraPayments = 301727,
                    FaaaSChildSupRecv = 301728,
                    FaaaSInterestIncome = 301729,
                    FaaaSUntxIraPen = 30173,
                    FaaaSVetNonEdBen = 301721,
                    FaaaSOtherUntaxedInc = 301735,
                    FaaaSOtherNonRepMoney = 3017245,
                    FaaaSMilitaryClergyAllow = 303725,
                    FaaaHousing1 = "B00000",
                    FaaaHousing1Plan = "1",
                    FaaaHousing2 = "G00000",
                    FaaaHousing2Plan = "1",
                    FaaaHousing3 = "B00000",
                    FaaaHousing3Plan = "2",
                    FaaaHousing4 = "E00000",
                    FaaaHousing4Plan = "3",
                    FaaaHousing5 = "B00000",
                    FaaaHousing5Plan = "1",
                    FaaaHousing6 = "B00000",
                    FaaaHousing6Plan = "2",
                    FaaaHousing7 = "G00000",
                    FaaaHousing7Plan = "3",
                    FaaaHousing8 = "E00000",
                    FaaaHousing8Plan = "1",
                    FaaaHousing9 = "G00000",
                    FaaaHousing9Plan = "2",
                    FaaaHousing10 = "E00000",
                    FaaaHousing10Plan = "1",
                    FaaaSMaritalStatus = "1",
                    FaaaSMaritalDate = 202210,
                    FaaaSLegalResSt = "CA",
                    FaaaSLegalResB4 = "false",

                    FaaaSLegalResDate = 200202,
                    FaaaP1GradeLvl = "1",
                    FaaaP2GradeLvl = "1",
                    FaaaHsGradType = "1",
                    FaaaHsName = "name",
                    FaaaHsCity = "city",
                    FaaaHsState = "VA",
                    FaaaHsCode = 192827,
                    FaaaDegreeBy = "true",
                    FaaaGradeLevel = "1",
                    FaaaDegOrCert = "1",
                    FaaaPMaritalDate = 202210,
                    FaaaP1Ssn = 102938456,
                    FaaaP2Ssn = 192834753,
                    FaaaP1LastName = "testLast",
                    FaaaP1FirstInit = "TestInit",
                    FaaaP1Dob = new DateTime(2000, 1, 25),
                    FaaaP2FirstInit = "Test2Init",
                    FaaaP2Dob = new DateTime(2000, 1, 25),
                    FaaaParentEmail = "Test@abc.com",
                    FaaaPLegalResSt = "VA",
                    FaaaPLegalResB4 = "false",
                    FaaaPLegalResDate = 200202,
                    FaaaPNbrFamily = 1,
                    FaaaPNbrCollege = 1,
                    FaaaPSsiBen = "true",
                    FaaaPFoodStamps = "false",
                    FaaaPLunchBen = "true",
                    FaaaPTanf = "false",
                    FaaaPWic = "true",
                    FaaaPTaxReturnFiled = "1",
                    FaaaPTaxFormType = "1",
                    FaaaPTaxFilingStatus = "1",
                    FaaaPSched1 = "1",
                    FaaaPDisWorker = "1",
                    FaaaPAgi = "10293473",
                    FaaaPUsTaxPaid = "102937",
                    FaaaP1Income = "1923746",
                    FaaaP2Income = "683623",
                    FaaaPCash = 39272,
                    FaaaPInvNetWorth = 20283,
                    FaaaPBusNetWorth = 38226,
                    FaaaPEduCredit = 68326,
                    FaaaPChildSupportPd = 302827,
                    FaaaPNeedBasedEmp = 82722,
                    FaaaPGrantScholAid = 3948567,
                    FaaaPCombatPay = 339838,
                    FaaaPCoOpEarnings = 28282882,
                    FaaaPPensionPymts = 505848,
                    FaaaPIraPymts = 595959,
                    FaaaPChildSupRcvd = 5959959,
                    FaaaPUntxIntInc = 3499383,
                    FaaaPUntxIraPen = 288383,
                    FaaaPMilClerAllow = 3883838,
                    FaaaPVetNonEdBen = 8383833,
                    FaaaPOtherUntxInc = 9292922,
                    FaaaSNbrFamily = 1,
                    FaaaSNbrCollege = 1,
                    FaaaSSsiBen = "true",
                    FaaaSFoodStamps = "false",
                    FaaaSLunchBen = "true",
                    FaaaSTanf = "false",
                    FaaaSWic = "true",
                    FaaaSDislWorker = "Test29",
                    FaaaDateCmpl = new DateTime(2000, 1, 25),
                    FaaaSignedFlag = "A",
                    FaaaPreparerSsn = 293485732,
                    FaaaPreparerEin = 203948576,
                    FaaaPreparerSigned = "1",
                    FaaaP2LastName = "TestLastName",
                    FaaaPMaritalStatus = "1"
                },
                new DataContracts.FaappApps()
                {
                    Recordkey = "2",
                    FaaaStudentId = personId,
                    FaaaYear = aidYear,
                    FaaaType = aidApplicationType,
                    FaaaBornB4Dt = "false",
                    FaaaMarried = "true",
                    FaaaGradProf = "false",
                    FaaaActiveDuty = "true",
                    FaaaVeteran = "false",
                    FaaaDependChildren = "true",
                    FaaaOtherDepend = "false",
                    FaaaOrphanWard = "true",
                    FaaaEmancipatedMinor = "false",
                    FaaaLegalGuardianship = "true",
                    FaaaHomelessBySchool = "true",
                    FaaaHomelessByHud = "false",
                    FaaaHomelessAtRisk = "true",
                    FaaaSTaxReturnFiled = "1",
                    FaaaSTaxFormType = "1",
                    FaaaSTaxFilingStatus = "1",
                    FaaaSSched1 = "Test1",
                    FaaaSAgi = "102938",
                    FaaaSUsTaxPd = 102937,
                    FaaaSStudentInc = "10247",
                    FaaaSpouseInc = "102847",
                    FaaaSCash = 462816,
                    FaaaSInvNetWorth = 293746,
                    FaaaSBusNetWorth = 483326,
                    FaaaSEduCredit = 492615,
                    FaaaSChildSupPaid = 492715,
                    FaaaSNeedBasedEmp = 301725,
                    FaaaSGrantScholAid = 301725,
                    FaaaSCombatPay = 301725,
                    FaaaSCoOpEarnings = 301725,
                    FaaaSPensionPayments = 301726,
                    FaaaSIraPayments = 301727,
                    FaaaSChildSupRecv = 301728,
                    FaaaSInterestIncome = 301729,
                    FaaaSUntxIraPen = 30173,
                    FaaaSVetNonEdBen = 301721,
                    FaaaSOtherUntaxedInc = 301735,
                    FaaaSOtherNonRepMoney = 3017245,
                    FaaaSMilitaryClergyAllow = 302725,
                    FaaaHousing1 = "B00000",
                    FaaaHousing1Plan = "1",
                    FaaaHousing2 = "G00000",
                    FaaaHousing2Plan = "1",
                    FaaaHousing3 = "B00000",
                    FaaaHousing3Plan = "2",
                    FaaaHousing4 = "E00000",
                    FaaaHousing4Plan = "3",
                    FaaaHousing5 = "B00000",
                    FaaaHousing5Plan = "1",
                    FaaaHousing6 = "B00000",
                    FaaaHousing6Plan = "2",
                    FaaaHousing7 = "G00000",
                    FaaaHousing7Plan = "3",
                    FaaaHousing8 = "E00000",
                    FaaaHousing8Plan = "1",
                    FaaaHousing9 = "G00000",
                    FaaaHousing9Plan = "2",
                    FaaaHousing10 = "E00000",
                    FaaaHousing10Plan = "1",
                    FaaaSMaritalStatus = "1",
                    FaaaSMaritalDate = 202210,
                    FaaaSLegalResSt = "CA",
                    FaaaSLegalResB4 = "false",

                    FaaaSLegalResDate = 200202,
                    FaaaP1GradeLvl = "1",
                    FaaaP2GradeLvl = "1",
                    FaaaHsGradType = "1",
                    FaaaHsName = "name",
                    FaaaHsCity = "city",
                    FaaaHsState = "VA",
                    FaaaHsCode = 192827,
                    FaaaDegreeBy = "true",
                    FaaaGradeLevel = "1",
                    FaaaDegOrCert = "1",
                    FaaaPMaritalDate = 202210,
                    FaaaP1Ssn = 1029456,
                    FaaaP2Ssn = 1928753,
                    FaaaP1LastName = "testLast",
                    FaaaP1FirstInit = "TestInit",
                    FaaaP1Dob = new DateTime(2000, 1, 25),
                    FaaaP2FirstInit = "Test2Init",
                    FaaaP2Dob = new DateTime(2000, 1, 25),
                    FaaaParentEmail = "Test@abc.com",
                    FaaaPLegalResSt = "VA",
                    FaaaPLegalResB4 = "false",
                    FaaaPLegalResDate = 200202,
                    FaaaPNbrFamily = 1,
                    FaaaPNbrCollege = 1,
                    FaaaPSsiBen = "true",
                    FaaaPFoodStamps = "false",
                    FaaaPLunchBen = "true",
                    FaaaPTanf = "false",
                    FaaaPWic = "true",
                    FaaaPTaxReturnFiled = "1",
                    FaaaPTaxFormType = "1",
                    FaaaPTaxFilingStatus = "1",
                    FaaaPSched1 = "1",
                    FaaaPDisWorker = "1",
                    FaaaPAgi = "10293473",
                    FaaaPUsTaxPaid = "102937",
                    FaaaP1Income = "1923746",
                    FaaaP2Income = "683623",
                    FaaaPCash = 39272,
                    FaaaPInvNetWorth = 20283,
                    FaaaPBusNetWorth = 38226,
                    FaaaPEduCredit = 68326,
                    FaaaPChildSupportPd = 302827,
                    FaaaPNeedBasedEmp = 82722,
                    FaaaPGrantScholAid = 3948567,
                    FaaaPCombatPay = 339838,
                    FaaaPCoOpEarnings = 28282882,
                    FaaaPPensionPymts = 505848,
                    FaaaPIraPymts = 595959,
                    FaaaPChildSupRcvd = 5959959,
                    FaaaPUntxIntInc = 3493383,
                    FaaaPUntxIraPen = 281383,
                    FaaaPMilClerAllow = 3283838,
                    FaaaPVetNonEdBen = 8383833,
                    FaaaPOtherUntxInc = 9292922,
                    FaaaSNbrFamily = 1,
                    FaaaSNbrCollege = 1,
                    FaaaSSsiBen = "true",
                    FaaaSFoodStamps = "false",
                    FaaaSLunchBen = "true",
                    FaaaSTanf = "false",
                    FaaaSWic = "true",
                    FaaaSDislWorker = "Test29",
                    FaaaDateCmpl = new DateTime(2000, 1, 25),
                    FaaaSignedFlag = "A",
                    FaaaPreparerSsn = 293485232,
                    FaaaPreparerEin = 203348576,
                    FaaaPreparerSigned = "1",
                    FaaaP2LastName = "TestLastName",
                    FaaaPMaritalStatus = "1"
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
                CacheName = "AllAidApplicationFilter:",
                Entity = "FAAPP.APPS",
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

            aidApplEntity = new Domain.FinancialAid.Entities.AidApplications("1", "1");
            
            aidApplEntity.PersonId = personId;
            aidApplEntity.AidYear = aidYear;
            aidApplEntity.AidApplicationType = aidApplicationType;
            aidApplEntity.AssignedID = applicantAssignedId;
            aidApplEntity.DegreeBy = true;
            aidApplEntity.GradeLevelInCollege = "1";
            aidApplEntity.DegreeOrCertificate = "1";
            aidApplEntity.BornBefore = false;
            aidApplEntity.Married = false;
            aidApplEntity.GradOrProfProgram = true;
            aidApplEntity.ActiveDuty = true;
            aidApplEntity.UsVeteran = false;
            aidApplEntity.DependentChildren = true;
            aidApplEntity.OtherDependents = true;
            aidApplEntity.OrphanWardFoster = false;
            aidApplEntity.EmancipatedMinor = true;
            aidApplEntity.LegalGuardianship = false;
            aidApplEntity.HomelessBySchool = true;
            aidApplEntity.HomelessByHud = false;
            aidApplEntity.HomelessAtRisk = true;
            aidApplEntity.StudentTaxReturnFiled = "1";
            aidApplEntity.StudentTaxFormType = "1";
            aidApplEntity.StudentTaxFilingStatus = "1";
            aidApplEntity.StudentSched1 = "TestUser14";
            aidApplEntity.StudentAgi = 1029311;
            aidApplEntity.StudentUsTaxPd = 1029312;
            aidApplEntity.SStudentInc = 1029314;
            aidApplEntity.SpouseInc = 1029315;
            aidApplEntity.StudentCash = 1029316;
            aidApplEntity.StudentInvNetWorth = 1029313;
            aidApplEntity.StudentBusNetWorth = 102931;
            aidApplEntity.StudentEduCredit = 102932;
            aidApplEntity.StudentChildSupPaid = 10292;
            aidApplEntity.StudentNeedBasedEmp = 10294;
            aidApplEntity.StudentGrantScholAid = 102933;
            aidApplEntity.StudentCombatPay = 102935;
            aidApplEntity.StudentCoOpEarnings = 10290;
            aidApplEntity.StudentPensionPayments = 102938;
            aidApplEntity.StudentIraPayments = 102936;
            aidApplEntity.StudentChildSupRecv = 102931;
            aidApplEntity.StudentInterestIncome = 1029;
            aidApplEntity.StudentUntxIraPen = 1029384;
            aidApplEntity.StudentVetNonEdBen = 1093454;
            aidApplEntity.StudentOtherUntaxedInc = 102938;
            aidApplEntity.StudentOtherNonRepMoney = 102932;
            aidApplEntity.StudentMilitaryClergyAllow = 10293;
            aidApplEntity.StudentLegalResSt = "CA";
            aidApplEntity.StudentLegalResB4 = false;
            aidApplEntity.StudentLegalResDate = "200202";
            aidApplEntity.P1GradeLvl = "1";
            aidApplEntity.P2GradeLvl = "1";
            aidApplEntity.HsGradType = "1";
            aidApplEntity.HsName = "name";
            aidApplEntity.HsCity = "abccity";
            aidApplEntity.HsState = "CA";
            aidApplEntity.HsCode = "120382";
            aidApplEntity.StudentNumberInFamily = 1;
            aidApplEntity.StudentNumberInCollege = 1;
            aidApplEntity.SSsiBen = true;
            aidApplEntity.SFoodStamps = false;
            aidApplEntity.SLunchBen = true;
            aidApplEntity.STanf = false;
            aidApplEntity.SWic = true;
            aidApplEntity.SDislWorker = "test";
            aidApplEntity.SchoolCode1 = "B00000";
            aidApplEntity.HousingPlan1 = "1";
            aidApplEntity.SchoolCode2 = "E00000";
            aidApplEntity.HousingPlan2 = "3";
            aidApplEntity.SchoolCode3 = "G00000";
            aidApplEntity.HousingPlan3 = "2";
            aidApplEntity.SchoolCode4 = "B00000";
            aidApplEntity.HousingPlan4 = "1";
            aidApplEntity.SchoolCode5 = "E00000";
            aidApplEntity.HousingPlan5 = "3";
            aidApplEntity.SchoolCode6 = "G00000";
            aidApplEntity.HousingPlan6 = "2";
            aidApplEntity.SchoolCode7 = "B00000";
            aidApplEntity.HousingPlan7 = "1";
            aidApplEntity.SchoolCode8 = "E00000";
            aidApplEntity.HousingPlan8 = "3";
            aidApplEntity.SchoolCode9 = "G00000";
            aidApplEntity.HousingPlan9 = "2";
            aidApplEntity.SchoolCode10 = "B00000";
            aidApplEntity.HousingPlan10 = "1";
            aidApplEntity.StudentMaritalStatus = "1";
            aidApplEntity.StudentMaritalDate = "202210";
            aidApplEntity.ApplicationCompleteDate = new DateTime(2000, 1, 25);
            aidApplEntity.SignedFlag = "A";
            aidApplEntity.PreparerSsn = 102938463;
            aidApplEntity.PreparerEin = 102938465;
            aidApplEntity.PreparerSigned = "1";
            aidApplEntity.PMaritalStatus = "1";
            aidApplEntity.PMaritalDate = "202210";
            aidApplEntity.P1Ssn = 102938466;
            aidApplEntity.P1LastName = "Testing1";
            aidApplEntity.P1FirstInit = "Testing";
            aidApplEntity.P1Dob = new DateTime(2000, 1, 25);
            aidApplEntity.P2Ssn = 102938469;
            aidApplEntity.P2LastName = "Test1";
            aidApplEntity.P2FirstInit = "Test";
            aidApplEntity.P2Dob = new DateTime(2000, 1, 25);
            aidApplEntity.ParentEmail = "abc@test.com";
            aidApplEntity.PLegalResSt = "CA";
            aidApplEntity.PLegalResB4 = true;
            aidApplEntity.PLegalResDate = "200202";
            aidApplEntity.PNbrFamily = 11;
            aidApplEntity.PNbrCollege = 1;
            aidApplEntity.PSsiBen = true;
            aidApplEntity.PFoodStamps = false;
            aidApplEntity.PLunchBen = true;
            aidApplEntity.PTanf = false;
            aidApplEntity.PWic = true;
            aidApplEntity.PTaxReturnFiled = "1";
            aidApplEntity.PTaxFormType = "1";
            aidApplEntity.PTaxFilingStatus = "1";
            aidApplEntity.PSched1 = "1";
            aidApplEntity.PDisWorker = "1";
            aidApplEntity.PAgi = 102932;
            aidApplEntity.PUsTaxPaid = 102931;
            aidApplEntity.P1Income = 102931;
            aidApplEntity.P2Income = 102938;
            aidApplEntity.PCash = 1029383;
            aidApplEntity.PInvNetWorth = 1029386;
            aidApplEntity.PBusNetWorth = 1029388;
            aidApplEntity.PEduCredit = 1029389;
            aidApplEntity.PChildSupportPd = 1029387;
            aidApplEntity.PNeedBasedEmp = 1029381;
            aidApplEntity.PGrantScholAid = 1029382;
            aidApplEntity.PCombatPay = 1029383;
            aidApplEntity.PCoOpEarnings = 1029384;
            aidApplEntity.PPensionPymts = 1029385;
            aidApplEntity.PIraPymts = 1029386;
            aidApplEntity.PChildSupRcvd = 1029382;
            aidApplEntity.PUntxIntInc = 1029384;
            aidApplEntity.PUntxIraPen = 1029321;
            aidApplEntity.PMilClerAllow = 1029381;
            aidApplEntity.PVetNonEdBen = 1029382;
            aidApplEntity.POtherUntxInc = 1029384;


            var expectedAppAddl = faappAppsList.FirstOrDefault(i => i.Recordkey.Equals(expectedRecordKey));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappApps>(expectedRecordKey, true)).ReturnsAsync(expectedAppAddl);
            var expectedAppDemo = faappDemoList.FirstOrDefault(i => i.Recordkey.Equals(expectedRecordKey));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(expectedRecordKey, true)).ReturnsAsync(expectedAppDemo);


            response = new UpdateAidApplResponse() { ApplId = "1", ErrorCode = new List<string>(), ErrorMessage = new List<string>(), ErrorFlag = false };
            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplRequest, UpdateAidApplResponse>(It.IsAny<UpdateAidApplRequest>())).ReturnsAsync(response);

            var personList = new Collection<Base.DataContracts.Person>()
            {
                new Base.DataContracts.Person(){Recordkey = personId}
            };
            var expectedPerson = personList.FirstOrDefault(i => i.Recordkey.Equals(personId));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(expectedPerson);

        }
    }
}
