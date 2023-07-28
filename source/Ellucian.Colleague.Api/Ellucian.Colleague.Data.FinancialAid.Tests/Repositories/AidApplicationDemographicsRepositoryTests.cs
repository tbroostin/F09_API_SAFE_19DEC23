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
    public class AidApplicationDemographicsRepositoryTests : BaseRepositorySetup
    {
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;

        private UpdateAidApplDemoResponse response;

        private AidApplicationDemographicsRepository aidApplicationDemographicsRepository;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo> faappDemoList;
        private Ellucian.Colleague.Domain.FinancialAid.Entities.AidApplicationDemographics aidApplDemoEntity;

        string criteria = "";
        string[] ids = new string[] { "1", "2" };
        string expectedRecordKey = "1";
        string id = "1";
        int offset = 0;
        int limit = 4;
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

            aidApplicationDemographicsRepository = new AidApplicationDemographicsRepository(cacheProviderMock.Object, _iColleagueTransactionFactoryMock.Object, _iLoggerMock.Object, apiSettings);
                        
        }

        [TestCleanup]
        public void Cleanup()
        {
            _iColleagueTransactionFactoryMock = null;
            _iColleagueTransactionInvokerMock = null;
            _iLoggerMock = null;
            _dataReaderMock = null;
            apiSettings = null;
            aidApplicationDemographicsRepository = null;
        }

        #region Get By Criteria
        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);
            var actuals = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsAsync(offset, limit);
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(ids.Count(), actuals.Item2);
            Assert.AreEqual(actuals.Item1.Count(), actuals.Item2);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_PersonIdFilter()
        {
            criteria = "WITH FAAD.STUDENT.ID = '" + personId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);

            var actuals = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsAsync(offset, limit, personId);

            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals);
            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(ids.Count(), actuals.Item2);
            Assert.AreEqual(actuals.Item1.Count(), actuals.Item2);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AidApplicationTypeFilter()
        {
            criteria = "WITH FAAD.TYPE = '" + aidApplicationType + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);

            var actuals = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsAsync(offset, limit, "", aidApplicationType);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_PersonId_AidApplTypeFilter()
        {
            criteria = "WITH FAAD.STUDENT.ID = '" + personId + "'" + " AND WITH FAAD.TYPE = '" + aidApplicationType + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);

            var actuals = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsAsync(offset, limit, personId, aidApplicationType);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AidYearFilter()
        {
            criteria = "WITH FAAD.YEAR = '" + aidYear + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);

            var actuals = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsAsync(offset, limit, null, null, aidYear);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AidApplicantAssignedIdFilter()
        {
            criteria = "WITH FAAD.ASSIGNED.ID = '" + applicantAssignedId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);

            var actuals = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsAsync(offset, limit, null, null, null, applicantAssignedId);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }


        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByCriteria_AllFilters()
        {
            criteria = "WITH FAAD.STUDENT.ID = '" + personId + "'" + " AND WITH FAAD.TYPE = '" + aidApplicationType + "'" + " AND WITH FAAD.YEAR = '" + aidYear + "'" + " AND WITH FAAD.ASSIGNED.ID = '" + applicantAssignedId + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("FAAPP.DEMO", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(ids, true))
               .ReturnsAsync(faappDemoList);

            var actuals = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsAsync(offset, limit, personId, aidApplicationType, aidYear, applicantAssignedId);
            _dataReaderMock.Verify(repo => repo.SelectAsync("FAAPP.DEMO", criteria), Times.Exactly(1));
            Assert.IsNotNull(actuals);
        }

        #endregion

        #region Get By ID

        [TestMethod]
        public async Task AidApplicationDemographicsRepository_GetByID()
        {
            var result = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsByIdAsync(id);
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRecordKey, result.Id);
            Assert.AreEqual(personId, result.PersonId);
            Assert.AreEqual(aidYear, result.AidYear);
            Assert.AreEqual(aidApplicationType, result.ApplicationType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationDemographicsRepository_GetByID_IdEmpty_Exception()
        {
            var result = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationDemographicsRepository_GetByID_IdNull_Exception()
        {
            var result = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AidApplicationDemographicsRepository_GetByID_DataReader_Exception()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(expectedRecordKey, true)).ReturnsAsync(() => null);
            var result = await aidApplicationDemographicsRepository.GetAidApplicationDemographicsByIdAsync(id);
        }

        #endregion

        #region PUT
        [TestMethod]
        public async Task AidApplicationDemographicsRepository_PUT()
        {
            var actual = await aidApplicationDemographicsRepository.UpdateAidApplicationDemographicsAsync(aidApplDemoEntity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedRecordKey, actual.Id);
            Assert.AreEqual(personId, actual.PersonId);
            Assert.AreEqual(aidYear, actual.AidYear);
            Assert.AreEqual(aidApplicationType, actual.ApplicationType);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationDemographicsRepository_PUT_Entity_Null_ArgumentNullException()
        {
            var actual = await aidApplicationDemographicsRepository.UpdateAidApplicationDemographicsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationDemographicsRepository_PUT_EntityId_Null_ArgumentNullException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics(" ", personId, aidYear, aidApplicationType);
            var actual = await aidApplicationDemographicsRepository.UpdateAidApplicationDemographicsAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationDemographicsRepository_PUT_PersonId_Null_RepositoryException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics(expectedRecordKey, " ", aidYear, aidApplicationType);
            var actual = await aidApplicationDemographicsRepository.UpdateAidApplicationDemographicsAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationDemographicsRepository_PUT_Person_Invalid_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationDemographicsRepository.UpdateAidApplicationDemographicsAsync(aidApplDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationDemographicsRepository_PUT_ExecuteAsync_Error_RepositoryException()
        {
            response.UpdateFaappDemoErrors = new List<UpdateFaappDemoErrors>()
            {                
                new UpdateFaappDemoErrors()
                {
                   ErrorCode = "ABC",
                   ErrorMessage = "Execute error in CTX."
                }
            };

            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplDemoRequest, UpdateAidApplDemoResponse>(It.IsAny<UpdateAidApplDemoRequest>())).ReturnsAsync(response);

            var actual = await aidApplicationDemographicsRepository.UpdateAidApplicationDemographicsAsync(aidApplDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationDemographicsRepository_PUT_GetByID_Returns_Null_Error_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationDemographicsRepository.UpdateAidApplicationDemographicsAsync(aidApplDemoEntity);
        }


        #endregion

        #region POST
        [TestMethod]
        public async Task AidApplicationDemographicsRepository_POST()
        {
            aidApplDemoEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics("New", personId, aidYear, aidApplicationType);
            var actual = await aidApplicationDemographicsRepository.CreateAidApplicationDemographicsAsync(aidApplDemoEntity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedRecordKey, actual.Id);
            Assert.AreEqual(personId, actual.PersonId);
            Assert.AreEqual(aidYear, actual.AidYear);
            Assert.AreEqual(aidApplicationType, actual.ApplicationType);
        }

        
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationDemographicsRepository_POST_Entity_Null_ArgumentNullException()
        {
            var actual = await aidApplicationDemographicsRepository.CreateAidApplicationDemographicsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationDemographicsRepository_POST_ExecuteAsync_Error_RepositoryException()
        {
            response.UpdateFaappDemoErrors = new List<UpdateFaappDemoErrors>()
            {
                new UpdateFaappDemoErrors()
                {
                   ErrorCode = "ABC",
                   ErrorMessage = "Execute error in CTX."
                }
            };

            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplDemoRequest, UpdateAidApplDemoResponse>(It.IsAny<UpdateAidApplDemoRequest>())).ReturnsAsync(response);

            var actual = await aidApplicationDemographicsRepository.CreateAidApplicationDemographicsAsync(aidApplDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationDemographicsRepository_POST_GetByID_Returns_Null_ArgumentNullException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationDemographicsRepository.CreateAidApplicationDemographicsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AidApplicationDemographicsRepository_POST_PersonId_Returns_Null_ArgumentNullException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(expectedRecordKey, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationDemographicsRepository.CreateAidApplicationDemographicsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationDemographicsRepository_POST_PersonId_Null_RepositoryException()
        {
            var appDemoEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics(expectedRecordKey, " ", aidYear, aidApplicationType);
            var actual = await aidApplicationDemographicsRepository.CreateAidApplicationDemographicsAsync(appDemoEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AidApplicationDemographicsRepository_POST_Person_Invalid_RepositoryException()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(() => null);

            var actual = await aidApplicationDemographicsRepository.CreateAidApplicationDemographicsAsync(aidApplDemoEntity);
        }

        #endregion

        private void BuildData()
        {
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
                CacheName = "AllAidApplicationDemoFilter:",
                Entity = "FAAPP.DEMO",
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

            aidApplDemoEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics("1", personId, aidYear, aidApplicationType);

            aidApplDemoEntity.ApplicantAssignedId = applicantAssignedId;
            aidApplDemoEntity.LastName = "TestLastName";
            aidApplDemoEntity.OrigName = "La";
            aidApplDemoEntity.FirstName = "TestFirstName";
            aidApplDemoEntity.MiddleInitial = "MI";

            aidApplDemoEntity.Address = new Domain.FinancialAid.Entities.Address();
            aidApplDemoEntity.Address.AddressLine = "street 123";
            aidApplDemoEntity.Address.City = "TestCity";
            aidApplDemoEntity.Address.State = "PA";
            aidApplDemoEntity.Address.Country = "US";
            aidApplDemoEntity.Address.ZipCode = "99999";


            aidApplDemoEntity.BirthDate = new DateTime(2000, 1, 30);
            aidApplDemoEntity.PhoneNumber = "999-0000-000";
            aidApplDemoEntity.EmailAddress = "test@testing.com";

            aidApplDemoEntity.CitizenshipStatusType = Domain.FinancialAid.Entities.AidApplicationCitizenshipStatus.Citizen;

            aidApplDemoEntity.AlternatePhoneNumber = "111-0000-1000";
            aidApplDemoEntity.StudentTaxIdNumber = "1234567";

            var expectedAppDemo = faappDemoList.FirstOrDefault(i => i.Recordkey.Equals(expectedRecordKey));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.FinancialAid.DataContracts.FaappDemo>(expectedRecordKey, true)).ReturnsAsync(expectedAppDemo);

            response = new UpdateAidApplDemoResponse() { DemoRecordKey = "1", UpdateFaappDemoErrors = new List<UpdateFaappDemoErrors>(), ErrorFlag = false };
            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<UpdateAidApplDemoRequest, UpdateAidApplDemoResponse>(It.IsAny<UpdateAidApplDemoRequest>())).ReturnsAsync(response);

            var personList = new Collection<Base.DataContracts.Person>()
            {
                new Base.DataContracts.Person(){Recordkey = personId}
            };
            var expectedPerson = personList.FirstOrDefault(i => i.Recordkey.Equals(personId));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personId, true)).ReturnsAsync(expectedPerson);

        }
    }
}
