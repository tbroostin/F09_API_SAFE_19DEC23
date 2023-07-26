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
    public class AidApplicationAdditionalInfoRepositoryTests : BaseRepositorySetup
    {
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;

        private UpdateAidApplAdditionalResponse response;

        private AidApplicationAdditionalInfoRepository aidApplicationAdditionalInfoRepository;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo> faappDemoList;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl> faappAddlList;
        private Ellucian.Colleague.Domain.FinancialAid.Entities.AidApplicationAdditionalInfo aidApplAdditionalEntity;

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

            aidApplicationAdditionalInfoRepository = new AidApplicationAdditionalInfoRepository(cacheProviderMock.Object, _iColleagueTransactionFactoryMock.Object, _iLoggerMock.Object, apiSettings);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _iColleagueTransactionFactoryMock = null;
            _iColleagueTransactionInvokerMock = null;
            _iLoggerMock = null;
            _dataReaderMock = null;
            apiSettings = null;
            aidApplicationAdditionalInfoRepository = null;
        }

        #region Get By Criteria
        [TestMethod]
        public async Task AidApplicationAdditionalInfoRepository_GetByCriteria()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.ADDL", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(ids, true))
               .ReturnsAsync(faappAddlList);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoAsync(offset, limit);
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(ids.Count(), actuals.Item2);
            Assert.AreEqual(actuals.Item1.Count(), actuals.Item2);
        }

        [TestMethod]
        public async Task AidApplicationAdditionalInfoRepository_GetByCriteria_AppDemoIdFilter()
        {
            criteria = "WITH FAAPP.ADDL.ID = '" + appDemoId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.ADDL", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(ids, true))
               .ReturnsAsync(faappAddlList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoAsync(offset, limit, appDemoId);

            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.ADDL", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationAdditionalInfoRepository_GetByCriteria_PersonIdFilter()
        {
            criteria = "WITH FADL.STUDENT.ID = '" + personId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.ADDL", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(ids, true))
               .ReturnsAsync(faappAddlList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoAsync(offset, limit, "", personId);

            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.ADDL", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(ids.Count(), actuals.Item2);
            Assert.AreEqual(actuals.Item1.Count(), actuals.Item2);
        }

        [TestMethod]
        public async Task AidApplicationAdditionalInfoRepository_GetByCriteria_AidApplicationTypeFilter()
        {
            criteria = "WITH FADL.TYPE = '" + aidApplicationType + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.ADDL", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(ids, true))
               .ReturnsAsync(faappAddlList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoAsync(offset, limit, "", "", aidApplicationType);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.ADDL", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_PersonId_AidApplTypeFilter()
        {
            criteria = "WITH FADL.STUDENT.ID = '" + personId + "'" + " AND WITH FADL.TYPE = '" + aidApplicationType + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.ADDL", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(ids, true))
               .ReturnsAsync(faappAddlList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoAsync(offset, limit, "", personId, aidApplicationType);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.ADDL", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AidYearFilter()
        {
            criteria = "WITH FADL.YEAR = '" + aidYear + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.ADDL", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(ids, true))
               .ReturnsAsync(faappAddlList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoAsync(offset, limit, "", "", "", aidYear);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.ADDL", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AidApplicantAssignedIdFilter()
        {
            criteria = "WITH FAAD.ASSIGNED.ID = '" + applicantAssignedId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.ADDL", "")).ReturnsAsync(ids);            
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(ids, true))
               .ReturnsAsync(faappAddlList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoAsync(offset, limit, "", "", "", "", applicantAssignedId);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.ADDL", ""), Times.Exactly(1));
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", ids, criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }


        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AllFilters()
        {
            criteria = "WITH FAAPP.ADDL.ID = '" + appDemoId + "'" + " AND WITH FADL.STUDENT.ID = '" + personId + "'" + " AND WITH FADL.TYPE = '" + aidApplicationType + "'" + " AND WITH FADL.YEAR = '" + aidYear + "'";
            assignedCriteria = "WITH FAAD.ASSIGNED.ID = '" + applicantAssignedId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.ADDL", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", ids, assignedCriteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(ids, true))
               .ReturnsAsync(faappAddlList);

            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoAsync(offset, limit, appDemoId, personId, aidApplicationType, aidYear, applicantAssignedId);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.ADDL", criteria), Times.Exactly(1));
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", ids, assignedCriteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        #endregion

        #region Get By ID

        [TestMethod]
        public async Task AidApplicationAdditionalInfoRepository_GetByID()
        {
            var result = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoByIdAsync(id);
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRecordKey, result.Id);
            Assert.AreEqual(personId, result.PersonId);
            Assert.AreEqual(aidYear, result.AidYear);
            Assert.AreEqual(aidApplicationType, result.ApplicationType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationAdditionalInfoRepository_GetByID_IdEmpty_Exception()
        {
            var result = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationAdditionalInfoRepository_GetByID_IdNull_Exception()
        {
            var result = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AidApplicationAdditionalInfoRepository_GetByID_DataReader_Exception()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(expectedRecordKey, true)).ReturnsAsync(() => null);
            var result = await aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoByIdAsync(id);
        }

        #endregion

        #region PUT
        [TestMethod]
        public async Task AidApplicationAdditionalInfoRepository_PUT()
        {
            var actual = await aidApplicationAdditionalInfoRepository.UpdateAidApplicationAdditionalInfoAsync(aidApplAdditionalEntity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedRecordKey, actual.Id);
            Assert.AreEqual(personId, actual.PersonId);
            Assert.AreEqual(aidYear, actual.AidYear);
            Assert.AreEqual(aidApplicationType, actual.ApplicationType);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationAdditionalInfoRepository_PUT_Entity_Null_ArgumentNullException()
        {
            var actual = await aidApplicationAdditionalInfoRepository.UpdateAidApplicationAdditionalInfoAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationAdditionalInfoRepository_PUT_EntityId_Null_ArgumentNullException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo(" ", " ");
            var actual = await aidApplicationAdditionalInfoRepository.UpdateAidApplicationAdditionalInfoAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationAdditionalInfoRepository_PUT_PersonId_Null_RepositoryException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo(expectedRecordKey, expectedRecordKey);
            var actual = await aidApplicationAdditionalInfoRepository.UpdateAidApplicationAdditionalInfoAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationAdditionalInfoRepository_PUT_Person_Invalid_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationAdditionalInfoRepository.UpdateAidApplicationAdditionalInfoAsync(aidApplAdditionalEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationAdditionalInfoRepository_PUT_ExecuteAsync_Error_RepositoryException()
        {
            response.UpdateStudentInformationErrors = new List<UpdateStudentInformationErrors>()
            {
                new UpdateStudentInformationErrors()
                {
                   ErrorCode = "ABC",
                   ErrorMessage = "Execute error in CTX."
                }
            };

            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplAdditionalRequest, UpdateAidApplAdditionalResponse>(It.IsAny<UpdateAidApplAdditionalRequest>())).ReturnsAsync(response);

            var actual = await aidApplicationAdditionalInfoRepository.UpdateAidApplicationAdditionalInfoAsync(aidApplAdditionalEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationAdditionalInfoRepository_PUT_GetByID_Returns_Null_Error_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationAdditionalInfoRepository.UpdateAidApplicationAdditionalInfoAsync(aidApplAdditionalEntity);
        }


        #endregion

        #region POST
        [TestMethod]
        public async Task AidApplicationAdditionalInfoRepository_POST()
        {
            aidApplAdditionalEntity = new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo("1", "1");
            aidApplAdditionalEntity.PersonId = personId;
            aidApplAdditionalEntity.AidYear = aidYear;
            aidApplAdditionalEntity.ApplicationType = aidApplicationType;
            aidApplAdditionalEntity.ApplicantAssignedId = applicantAssignedId;
            var actual = await aidApplicationAdditionalInfoRepository.CreateAidApplicationAdditionalInfoAsync(aidApplAdditionalEntity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedRecordKey, actual.Id);
            Assert.AreEqual(personId, actual.PersonId);
            Assert.AreEqual(aidYear, actual.AidYear);
            Assert.AreEqual(aidApplicationType, actual.ApplicationType);
        }



        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationAdditionalInfoRepository_POST_Entity_Null_ArgumentNullException()
        {
            var actual = await aidApplicationAdditionalInfoRepository.CreateAidApplicationAdditionalInfoAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationAdditionalInfoRepository_POST_ExecuteAsync_Error_RepositoryException()
        {
            response.UpdateStudentInformationErrors = new List<UpdateStudentInformationErrors>()
            {
                new UpdateStudentInformationErrors()
                {
                   ErrorCode = "ABC",
                   ErrorMessage = "Execute error in CTX."
                }
            };

            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplAdditionalRequest, UpdateAidApplAdditionalResponse>(It.IsAny<UpdateAidApplAdditionalRequest>())).ReturnsAsync(response);

            var actual = await aidApplicationAdditionalInfoRepository.UpdateAidApplicationAdditionalInfoAsync(aidApplAdditionalEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationAdditionalInfoRepository_POST_GetByID_Returns_Null_ArgumentNullException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationAdditionalInfoRepository.CreateAidApplicationAdditionalInfoAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationAdditionalInfoRepository_POST_PersonId_Returns_Null_ArgumentNullException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationAdditionalInfoRepository.CreateAidApplicationAdditionalInfoAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationAdditionalInfoRepository_POST_PersonId_Null_RepositoryException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo(expectedRecordKey, expectedRecordKey);
            var actual = await aidApplicationAdditionalInfoRepository.CreateAidApplicationAdditionalInfoAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationAdditionalInfoRepository_POST_Person_Invalid_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationAdditionalInfoRepository.CreateAidApplicationAdditionalInfoAsync(aidApplAdditionalEntity); ;
        }

        #endregion
        
        private void BuildData()
        {
            faappAddlList = new Collection<DataContracts.FaappAddl>()
            {
                new DataContracts.FaappAddl()
                {
                    Recordkey = "1",
                    FadlStudentId = personId,
                    FadlYear = aidYear,
                    FadlType = aidApplicationType,
                    FadlSsid = "TestFirstName",
                    FadlFosterCare = "TestLastName",
                    FadlCounty = "TestInitials",
                    FadlWardshipState = "LN",
                    FadlChafeeConsider = "Street1",
                    FadlUser1 = "TestUser1",
                    FadlUser2 = "TestUser2",
                    FadlUser3 = "TestUser3",
                    FadlUser4 = "TestUser4",
                    FadlUser5 = "TestUser5",
                    FadlUser6 = "TestUser6",
                    FadlUser7 = "TestUser7",
                    FadlUser8 = "TestUser8",
                    FadlUser9 = "TestUser9",
                    FadlUser10 = "TestUser10",
                    FadlUser11 = "TestUser11",
                    FadlUser12 = "TestUser12",
                    FadlUser13 = "TestUser13",
                    FadlUser14 = "TestUser14",
                    FadlUser15 = new DateTime(2000, 1, 25),
                    FadlUser16 = new DateTime(2000, 1, 26),
                    FadlUser17 = new DateTime(2000, 1, 27),
                    FadlUser18 = new DateTime(2000, 1, 28),
                    FadlUser19 = new DateTime(2000, 1, 29),
                    FadlUser21 = new DateTime(2000, 1, 30),
                },
                new DataContracts.FaappAddl()
                {
                    Recordkey = "2",
                    FadlStudentId = personId,
                    FadlYear = aidYear,
                    FadlType = aidApplicationType,
                    FadlSsid = "530194758",
                    FadlFosterCare = "true",
                    FadlCounty = "TestCounty",
                    FadlWardshipState = "VA",
                    FadlChafeeConsider = "true",
                    FadlCcpgActive = "true",
                    FadlUser1 = "TestUser1",
                    FadlUser2 = "TestUser2",
                    FadlUser3 = "TestUser3",
                    FadlUser4 = "TestUser4",
                    FadlUser5 = "TestUser5",
                    FadlUser6 = "TestUser6",
                    FadlUser7 = "TestUser7",
                    FadlUser8 = "TestUser8",
                    FadlUser9 = "TestUser9",
                    FadlUser10 = "TestUser10",
                    FadlUser11 = "TestUser11",
                    FadlUser12 = "TestUser12",
                    FadlUser13 = "TestUser13",
                    FadlUser14 = "TestUser14",
                    FadlUser15 = new DateTime(2000, 1, 25),
                    FadlUser16 = new DateTime(2000, 1, 26),
                    FadlUser17 = new DateTime(2000, 1, 27),
                    FadlUser18 = new DateTime(2000, 1, 28),
                    FadlUser19 = new DateTime(2000, 1, 29),
                    FadlUser21 = new DateTime(2000, 1, 30),
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
                CacheName = "AllAidApplicationInfoFilter:",
                Entity = "FAAPP.ADDL",
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

            aidApplAdditionalEntity = new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo("1", "1");
            //change this
            aidApplAdditionalEntity.PersonId = personId;
            aidApplAdditionalEntity.AidYear = aidYear;
            aidApplAdditionalEntity.ApplicationType = aidApplicationType;
            aidApplAdditionalEntity.ApplicantAssignedId = applicantAssignedId;
            aidApplAdditionalEntity.StudentStateId = "530194758";
            aidApplAdditionalEntity.FosterCare = false;
            aidApplAdditionalEntity.ApplicationCounty = "TestCounty";
            aidApplAdditionalEntity.WardshipState = "VA";
            aidApplAdditionalEntity.ChafeeConsideration = false;
            aidApplAdditionalEntity.CreateCcpgRecord = true;
            aidApplAdditionalEntity.User1 = "TestUser1";
            aidApplAdditionalEntity.User2 = "TestUser2";
            aidApplAdditionalEntity.User3 = "TestUser3";
            aidApplAdditionalEntity.User4 = "TestUser4";
            aidApplAdditionalEntity.User5 = "TestUser5";
            aidApplAdditionalEntity.User6 = "TestUser6";
            aidApplAdditionalEntity.User7 = "TestUser7";
            aidApplAdditionalEntity.User8 = "TestUser8";
            aidApplAdditionalEntity.User9 = "TestUser9";
            aidApplAdditionalEntity.User10 = "TestUser10";
            aidApplAdditionalEntity.User11 = "TestUser11";
            aidApplAdditionalEntity.User12 = "TestUser12";
            aidApplAdditionalEntity.User13 = "TestUser13";
            aidApplAdditionalEntity.User14 = "TestUser14";
            aidApplAdditionalEntity.User15 = new DateTime(2000, 1, 25);
            aidApplAdditionalEntity.User16 = new DateTime(2000, 1, 26);
            aidApplAdditionalEntity.User17 = new DateTime(2000, 1, 27);
            aidApplAdditionalEntity.User18 = new DateTime(2000, 1, 28);
            aidApplAdditionalEntity.User19 = new DateTime(2000, 1, 29);
            aidApplAdditionalEntity.User21 = new DateTime(2000, 1, 30);

            var expectedAppAddl = faappAddlList.FirstOrDefault(i => i.Recordkey.Equals(expectedRecordKey));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappAddl>(expectedRecordKey, true)).ReturnsAsync(expectedAppAddl);
            var expectedAppDemo = faappDemoList.FirstOrDefault(i => i.Recordkey.Equals(expectedRecordKey));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(expectedRecordKey, true)).ReturnsAsync(expectedAppDemo);


            response = new UpdateAidApplAdditionalResponse() { AidAddId = "1", UpdateStudentInformationErrors = new List<UpdateStudentInformationErrors>(), ErrorFlag = false };
            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplAdditionalRequest, UpdateAidApplAdditionalResponse>(It.IsAny<UpdateAidApplAdditionalRequest>())).ReturnsAsync(response);

            var personList = new Collection<Base.DataContracts.Person>()
            {
                new Base.DataContracts.Person(){Recordkey = personId}
            };
            var expectedPerson = personList.FirstOrDefault(i => i.Recordkey.Equals(personId));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(expectedPerson);

        }
    }
}
