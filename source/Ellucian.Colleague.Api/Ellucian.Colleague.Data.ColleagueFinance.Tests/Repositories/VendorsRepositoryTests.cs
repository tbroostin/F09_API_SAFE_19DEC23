/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class VendorsRepositoryTests : BaseRepositorySetup
    {
        private Mock<ICacheProvider> _iCacheProviderMock;
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;
        private ApiSettings apiSettings;

        private VendorsRepository vendorRepository;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors> vendors;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;
        private Ellucian.Colleague.Domain.ColleagueFinance.Entities.Vendors vendorEntity;
        private CreateUpdateVendorResponse response;

        private System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Corp> corp;
        string criteria = "";
        string[] ids = new string[] { "1", "2", "3", "4" };
        string expectedRecordKey = "1";
        string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";
        int offset = 0;
        int limit = 4;

        [TestInitialize]
        public void Initialize()
        {
            _iCacheProviderMock = new Mock<ICacheProvider>();
            _iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
            _iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
            _iLoggerMock = new Mock<ILogger>();
            _dataReaderMock = new Mock<IColleagueDataReader>();

            _iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
            _iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader())
                .Returns(_dataReaderMock.Object);
            _iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker())
                .Returns(_iColleagueTransactionInvokerMock.Object);

            apiSettings = new ApiSettings("TEST");

            BuildData();

            vendorRepository = new VendorsRepository(_iCacheProviderMock.Object, _iColleagueTransactionFactoryMock.Object, _iLoggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _iCacheProviderMock = null;
            _iColleagueTransactionFactoryMock = null;
            _iColleagueTransactionInvokerMock = null;
            _iLoggerMock = null;
            _dataReaderMock = null;
            apiSettings = null;
            vendorRepository = null;
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsAsync()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            var actuals = await vendorRepository.GetVendorsAsync(offset, limit);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsAsync_VendorsFilter()
        {
            string vendorID = "TestCriteria";
            criteria = "WITH VENDORS.ID = '"+ vendorID + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actuals = await vendorRepository.GetVendorsAsync(offset, limit, vendorID);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsAsync_ClassificationsFilter()
        {
            List<string> classificationsID = new List<string>() { "TestCriteria" } ;
            criteria = "WITH VEN.TYPES = '" + classificationsID + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actuals = await vendorRepository.GetVendorsAsync(offset, limit, "", classificationsID);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsAsync_StatusFilter_Active()
        {
            criteria = "WITH VEN.ACTIVE.FLAG = 'Y'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> statuses = new List<string>() { "active" };
            var actuals = await vendorRepository.GetVendorsAsync(offset, limit, "", null, statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsAsync_StatusFilter_holdPayment()
        {
            criteria = "WITH VEN.STOP.PAYMENT.FLAG = 'Y'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> statuses = new List<string>() { "holdPayment" };
            var actuals = await vendorRepository.GetVendorsAsync(offset, limit, "", null, statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsAsync_StatusFilter_Approved()
        {
            criteria = "WITH VEN.APPROVAL.FLAG = 'Y'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> statuses = new List<string>() { "approved" };
            var actuals = await vendorRepository.GetVendorsAsync(offset, limit, "", null, statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsAsync_AllFilters()
        {
            string vendorID = "TestCriteria";
            List<string> classifications =  new List<string>() {  "TestCriteria" };
            criteria = "WITH VENDORS.ID = 'TestCriteria' AND WITH VEN.TYPES = 'TestCriteria' AND VEN.APPROVAL.FLAG = 'Y'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", It.IsAny<string>())).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> statuses = new List<string>() { "approved" };
            var actuals = await vendorRepository.GetVendorsAsync(offset, limit, vendorID, classifications, statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsByGuidAsync()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            
            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(expectedPerson);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actual = await vendorRepository.GetVendorsByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task VendorsRepository_PUT()
        {
            var person = people.First();
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), true)).ReturnsAsync(person);
            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(It.IsAny<CreateUpdateVendorRequest>())).ReturnsAsync(response);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(expectedPerson);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actual = await vendorRepository.UpdateVendorsAsync(vendorEntity);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task VendorsRepository_POST()
        {
            var person = people.First();
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), true)).ReturnsAsync(person);
            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(It.IsAny<CreateUpdateVendorRequest>())).ReturnsAsync(response);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(expectedPerson);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actual = await vendorRepository.CreateVendorsAsync(vendorEntity);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_PUT_vendorEntity_Null_ArgumentNullException()
        {
            var actual = await vendorRepository.UpdateVendorsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_PUT_vendorEntityId_Null_ArgumentNullException()
        {
            vendorEntity = new Domain.ColleagueFinance.Entities.Vendors(" ");
            var actual = await vendorRepository.UpdateVendorsAsync(vendorEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_PUT_ExecuteAsync_Error_RepositoryException()
        {
            var person = people.First();
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);
            response.Errors = new List<Errors>() 
            {
                new Errors()
                {
                   ErrorCodes = "ABC",
                   ErrorMessages = "Execute error in CTX."
                }
            };

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), true)).ReturnsAsync(person);
            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(It.IsAny<CreateUpdateVendorRequest>())).ReturnsAsync(response);

            var actual = await vendorRepository.UpdateVendorsAsync(vendorEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_POST_Entity_Null_ArgumentNullException()
        {
            var actual = await vendorRepository.CreateVendorsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_POST_ExecuteAsync_Error_RepositoryException()
        {
            var person = people.First();
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);
            response.Errors = new List<Errors>() 
            {
                new Errors()
                {
                   ErrorCodes = "ABC",
                   ErrorMessages = "Execute error in CTX."
                }
            };

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), true)).ReturnsAsync(person);
            _iColleagueTransactionInvokerMock.Setup(repo => repo.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(It.IsAny<CreateUpdateVendorRequest>())).ReturnsAsync(response);

            var actual = await vendorRepository.CreateVendorsAsync(vendorEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsAsync_Vendors_Null()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);

            var actuals = await vendorRepository.GetVendorsAsync(offset, limit);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsAsync_People_Null()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(null);

            var actuals = await vendorRepository.GetVendorsAsync(offset, limit);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuidAsync_IdNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(expectedPerson);

            var actual = await vendorRepository.GetVendorsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuidAsync_GuidNull_KeyNotFoundException()
        {
            var actual = await vendorRepository.GetVendorsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuidAsync_DictionaryNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = null;

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var actual = await vendorRepository.GetVendorsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuidAsync_FoundEntryNull_KeyNotFoundException()
        {
            GuidLookupResult result = null;
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var actual = await vendorRepository.GetVendorsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuidAsync_WrongEntityName_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "WrongEntityName", PrimaryKey = null, SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var actual = await vendorRepository.GetVendorsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuidAsync_VendorNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(null);

            var actual = await vendorRepository.GetVendorsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuidAsync_PersonNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actual = await vendorRepository.GetVendorsByGuidAsync(guid);
        }

        private void BuildData()
        {
            vendors = new Collection<DataContracts.Vendors>() 
            {
                new DataContracts.Vendors()
                {
                    RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46",
                    Recordkey = "1",
                    VenActiveFlag = "Y",
                    VendorsAddDate = new DateTime(2015, 05, 01),
                    VenApTypes = new List<string>(){"AP2"},
                    VenApprovalFlag = "Y",
                    VenCurrencyCode = "CAN",
                    VenMisc = new List<string>(){"GC"},
                    VenStopPaymentFlag = "N",
                    VenTerms = new List<string>(){"30"},
                    VenTypes = new List<string>(){"AE", "IN"},
                    VenComments = "Comment 1",
                    VenCategories = new List<string>() {"travel" }
                },
                new DataContracts.Vendors()
                {
                    RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff",
                    Recordkey = "2",
                    VenActiveFlag = "Y",
                    VendorsAddDate = new DateTime(2015, 10, 01),
                    VenApTypes = new List<string>(),
                    VenApprovalFlag = "Y",
                    VenCurrencyCode = "US",
                    VenMisc = new List<string>(),
                    VenStopPaymentFlag = "N",
                    VenTerms = new List<string>(){"60"},
                    VenTypes = new List<string>(){"AE", "IN"},
                    VenComments = "Comment 2",
                    VenCategories = new List<string>() {"travel" }
                },
                new DataContracts.Vendors()
                {
                    RecordGuid = "41a341d6-ebc0-4ac7-a77f-262e5e7dfd62",
                    Recordkey = "3",
                    VenActiveFlag = "Y",
                    VendorsAddDate = new DateTime(2015, 01, 01),
                    VenApTypes = new List<string>(),
                    VenApprovalFlag = "Y",
                    VenCurrencyCode = "US",
                    VenMisc = new List<string>(),
                    VenStopPaymentFlag = "N",
                    VenTerms = new List<string>(){"30", "60"},
                    VenTypes = new List<string>(){"AE"},
                    VenComments = "Comment 3"
                },
                new DataContracts.Vendors()
                {
                    RecordGuid = "4d9962e8-195b-4442-93d7-197901cfb438",
                    Recordkey = "4",
                    VenActiveFlag = "Y",
                    VendorsAddDate = new DateTime(2015, 12, 01),
                    VenApTypes = new List<string>(),
                    VenApprovalFlag = "Y",
                    VenCurrencyCode = "CAN",
                    VenMisc = new List<string>(),
                    VenStopPaymentFlag = "N",
                    VenTerms = new List<string>(){"30", "60", "90"},
                    VenTypes = new List<string>(){"IN"},
                    VenComments = "Comment 4"
                }
            };

            people = new Collection<Base.DataContracts.Person>() 
            {
                new Base.DataContracts.Person(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "1", PersonCorpIndicator = "Y"},
                new Base.DataContracts.Person(){RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff", Recordkey = "2", PersonCorpIndicator = "Y"},
                new Base.DataContracts.Person(){RecordGuid = "41a341d6-ebc0-4ac7-a77f-262e5e7dfd62", Recordkey = "3", PersonCorpIndicator = "N"},
                new Base.DataContracts.Person(){RecordGuid = "4d9962e8-195b-4442-93d7-197901cfb438", Recordkey = "4", PersonCorpIndicator = "N"}
            };

            corp = new Collection<Corp>()
            {
                new Corp(){ Recordkey = "1", CorpParents = new List<string>() {"3" } },
                new Corp(){ Recordkey = "2", CorpParents = new List<string>() {"4" } },
                new Corp(){ Recordkey = "3", CorpParents = new List<string>() {"1" } },
                new Corp(){ Recordkey = "4" },

            };

            vendorEntity = new Domain.ColleagueFinance.Entities.Vendors(guid)
            {
                ActiveFlag = "Y",
                AddDate = DateTime.Today.AddDays(-5),
                ApprovalFlag = "Y",
                ApTypes = new List<string>() { "AP2" },
                Comments = "Comment 1",
                CorpParent = new List<string>() { "1" },
                CurrencyCode = "US",
                IntgHoldReasons = new List<string>() { "Active" },
                IsOrganization = false,
                Terms = new List<string>() { "30", "60", "90" },
                Types = new List<string>() { "IN" }
            };
            response = new CreateUpdateVendorResponse() {VendorGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", VendorId = "1" };
        }
    }
}
