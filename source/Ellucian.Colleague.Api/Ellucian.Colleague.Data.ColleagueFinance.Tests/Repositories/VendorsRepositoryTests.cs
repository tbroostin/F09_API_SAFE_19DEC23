/*Copyright 2016-2020 Ellucian Company L.P. and its affiliates.*/

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
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]

    public class VendorsRepositoryTestsv8 : BaseRepositorySetup
    {
        private Mock<ICacheProvider> _iCacheProviderMock;
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;

        private CreateUpdateVendorResponse response;
        private VoucherVendorSearchResults voucherVendors;
        private TxGetVoucherVendorResultsRequest voucherVendorRequest;
        private TxGetVoucherVendorResultsResponse voucherVendorResponse;

        private VendorsRepository vendorRepository;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors> vendors;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;
        private Ellucian.Colleague.Domain.ColleagueFinance.Entities.Vendors vendorEntity;

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

            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<TxGetVoucherVendorResultsRequest, TxGetVoucherVendorResultsResponse>(It.IsAny<TxGetVoucherVendorResultsRequest>())).ReturnsAsync(voucherVendorResponse);
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
            criteria = "WITH VENDORS.ID = '" + vendorID + "'";
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
        public async Task VendorsRepository_GetVendorsAsync_ClassificationsFilter()
        {
            List<string> classificationsID = new List<string>() { "TestCriteria" };
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
            List<string> classifications = new List<string>() { "TestCriteria" };
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
            response = new CreateUpdateVendorResponse() { VendorGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", VendorId = "1" };
        }
    }

    [TestClass]
    public class VendorsRepositoryTestsv11 : BaseRepositorySetup
    {
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;

        private VendorsRepository vendorRepository;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors> vendors;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;
        private Ellucian.Colleague.Domain.ColleagueFinance.Entities.Vendors vendorEntity;
        private CreateUpdateVendorResponse response;
        private VoucherVendorSearchResults voucherVendors;
        private TxGetVoucherVendorResultsRequest voucherVendorRequest;
        private TxGetVoucherVendorResultsResponse voucherVendorResponse;

        private VendorSearchResults vendorSearchResults;
        private GetActiveVendorResultsRequest vendorSearchResultRequest;
        private GetActiveVendorResultsResponse vendorSearchResultResponse;

        private VendorDefaultTaxFormInfo vendorDefaultTaxFormInfo;
        private GetVendDefaultTaxInfoRequest vendorDefaultTaxFormRequest;
        private GetVendDefaultTaxInfoResponse vendorDefaultTaxFormResponse;
        private string vendorId;
        private string apType;

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

            vendorRepository = new VendorsRepository(cacheProviderMock.Object, _iColleagueTransactionFactoryMock.Object, _iLoggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _iColleagueTransactionFactoryMock = null;
            _iColleagueTransactionInvokerMock = null;
            _iLoggerMock = null;
            _dataReaderMock = null;
            apiSettings = null;
            vendorRepository = null;
            transManager = null;
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            var actuals = await vendorRepository.GetVendors2Async(offset, limit);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendors2Async_person_not_found()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(new Collection<Person>() { new Person() { Recordkey = "1234" } });
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            var actuals = await vendorRepository.GetVendors2Async(offset, limit);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_VendorsFilter()
        {
            string vendorID = "TestCriteria";
            criteria = "WITH VENDORS.ID = '" + vendorID + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actuals = await vendorRepository.GetVendors2Async(offset, limit, vendorID);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_ClassificationsFilter()
        {
            List<string> classificationsID = new List<string>() { "TestCriteria" };
            criteria = "WITH VEN.TYPES = '" + classificationsID + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", classificationsID);
            Assert.IsNotNull(actuals);
        }


        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_TypesFilter_EP()
        {
            List<string> typesID = new List<string>() { "eprocurement", "travel", "procurement" };
            criteria = "WITH VEN.CATEGORIES = '" + "EP" + "' AND WITH VEN.CATEGORIES = '" + "TR" + "'" + "' AND WITH VEN.CATEGORIES = '" + "PR" + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, null, null, typesID);
            Assert.IsNotNull(actuals);
        }



        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_StatusFilter_Active()
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
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_StatusFilter_holdPayment()
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
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_StatusFilter_Approved()
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
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_TaxId_US()
        {
            var taxId = "123456789";
            var taxcriteria = string.Format("WITH CORP.TAX.ID EQ '{0}'", taxId);
            _dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new IntlParams() { HostCountry = "USA" });
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("CORP.FOUNDS", It.IsAny<string[]>(), taxcriteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, null, null, null, taxId);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_TaxId_NoCorp()
        {
            var taxId = "123456789";
            var taxcriteria = string.Format("WITH CORP.TAX.ID EQ '{0}'", taxId);
            _dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new IntlParams() { HostCountry = "USA" });
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("CORP.FOUNDS", It.IsAny<string[]>(), taxcriteria)).ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, null, null, null, taxId);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_TaxId_NoVendor()
        {
            var taxId = "123456789";
            var taxcriteria = string.Format("WITH CORP.TAX.ID EQ '{0}'", taxId);
            _dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new IntlParams() { HostCountry = "USA" });
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(new string[0]);
            _dataReaderMock.Setup(repo => repo.SelectAsync("CORP.FOUNDS", It.IsAny<string[]>(), taxcriteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, null, null, null, taxId);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_ParentVendor()
        {
            criteria = "";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR = 'Y' AND WITH PARENTS NE '' BY.EXP PARENTS SAVING PARENTS")).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR EQ 'Y' AND WITH PARENTS EQ '?'", ids, "?", true, 425)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> relatedReference = new List<string>() { "parentVendor" };
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, null, relatedReference);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_ParentVendor_noChildrenVendor()
        {
            criteria = "";
            string[] children = { "1", "2" };
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR = 'Y' AND WITH PARENTS NE '' BY.EXP PARENTS SAVING PARENTS")).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR EQ 'Y' AND WITH PARENTS EQ '?'", ids, "?", true, 425)).ReturnsAsync(children);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", children, criteria)).ReturnsAsync(new string[0]);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> relatedReference = new List<string>() { "parentVendor" };
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, null, relatedReference);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_ParentVendor_Parent_NoVendor()
        {
            criteria = "";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR = 'Y' AND WITH PARENTS NE '' BY.EXP PARENTS SAVING PARENTS")).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR EQ 'Y' AND WITH PARENTS EQ '?'", ids, "?", true, 425)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> relatedReference = new List<string>() { "parentVendor" };
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, null, relatedReference);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_ParentVendor_NOChildren()
        {
            criteria = "";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR = 'Y' AND WITH PARENTS NE '' BY.EXP PARENTS SAVING PARENTS")).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR EQ 'Y' AND WITH PARENTS EQ '?'", ids, "?", true, 425)).ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> relatedReference = new List<string>() { "parentVendor" };
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, null, relatedReference);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_ParentVendor_NoParents()
        {
            criteria = "";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR = 'Y' AND WITH PARENTS NE '' BY.EXP PARENTS SAVING PARENTS")).ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR EQ 'Y' AND WITH PARENTS EQ '?'", ids, "?", true, 425)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> relatedReference = new List<string>() { "parentVendor" };
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, "", null, null, relatedReference);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_AllFilters()
        {
            string vendorID = "TestCriteria";
            List<string> classifications = new List<string>() { "TestCriteria" };
            criteria = "WITH VENDORS.ID = 'TestCriteria' AND WITH VEN.TYPES = 'TestCriteria' AND VEN.APPROVAL.FLAG = 'Y'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", It.IsAny<string>())).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> statuses = new List<string>() { "approved" };
            List<string> types = new List<string>() { "eprocurement", "travel" };
            var actuals = await vendorRepository.GetVendors2Async(offset, limit, vendorID, classifications, statuses, types);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsByGuid2Async()
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

            var actual = await vendorRepository.GetVendorsByGuid2Async(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendorsByGuid2Async_NoPerson()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actual = await vendorRepository.GetVendorsByGuid2Async(guid);

        }

        [TestMethod]
        public async Task VendorsRepository_PUTv11()
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

            var actual = await vendorRepository.UpdateVendors2Async(vendorEntity);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task VendorsRepository_POST2()
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

            var actual = await vendorRepository.CreateVendors2Async(vendorEntity);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_PUT2_vendorEntity_Null_ArgumentNullException()
        {
            var actual = await vendorRepository.UpdateVendors2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_PUT2_vendorEntityId_Null_ArgumentNullException()
        {
            vendorEntity = new Domain.ColleagueFinance.Entities.Vendors(" ");
            var actual = await vendorRepository.UpdateVendors2Async(vendorEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_PUT2_ExecuteAsync_Error_RepositoryException()
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

            var actual = await vendorRepository.UpdateVendors2Async(vendorEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_POST_Entity_Null_ArgumentNullException()
        {
            var actual = await vendorRepository.CreateVendors2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_POST2_ExecuteAsync_Error_RepositoryException()
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

            var actual = await vendorRepository.CreateVendors2Async(vendorEntity);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_Vendors_Null()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);

            var actuals = await vendorRepository.GetVendors2Async(offset, limit);
        }


        [TestMethod]
        public async Task VendorsRepository_GetVendors2Async_No_Records_Selected()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _iColleagueTransactionInvokerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
       .ReturnsAsync(null);
            var actuals = await vendorRepository.GetVendors2Async(offset, limit);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendors2Async_People_Null()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(null);

            var actuals = await vendorRepository.GetVendors2Async(offset, limit);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuid2Async_IdNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(expectedPerson);

            var actual = await vendorRepository.GetVendorsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendorsByGuid2Async_GuidNull_KeyNotFoundException()
        {
            var actual = await vendorRepository.GetVendorsByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuid2Async_DictionaryNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = null;

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var actual = await vendorRepository.GetVendorsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuid2Async_FoundEntryNull_KeyNotFoundException()
        {
            GuidLookupResult result = null;
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var actual = await vendorRepository.GetVendorsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendorsByGuid2Async_WrongEntityName_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "WrongEntityName", PrimaryKey = null, SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var actual = await vendorRepository.GetVendorsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsByGuid2Async_VendorNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(null);

            var actual = await vendorRepository.GetVendorsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendorsByGuid2Async_PersonNull_KeyNotFoundException()
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

            var actual = await vendorRepository.GetVendorsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_VendorSearchForVoucherAsync_CriteriaEmpty_Exception()
        {
            var actual = await vendorRepository.VendorSearchForVoucherAsync("", "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_VendorSearchForVoucherAsync_ArgumentNullException()
        {
            var actual = await vendorRepository.VendorSearchForVoucherAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task VendorsRepository_VendorSearchForVoucherAsync_ResponseIsNull()
        {
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<TxGetVoucherVendorResultsRequest, TxGetVoucherVendorResultsResponse>(It.IsAny<TxGetVoucherVendorResultsRequest>())).ReturnsAsync(null);
            await vendorRepository.VendorSearchForVoucherAsync("Office", "AP");
        }

        [TestMethod]
        public async Task VendorsRepository_VendorSearchForVoucherAsync_ResponseNoSearchResultforVendors()
        {
            voucherVendorResponse.VoucherVendorSearchResults = null;
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<TxGetVoucherVendorResultsRequest, TxGetVoucherVendorResultsResponse>(It.IsAny<TxGetVoucherVendorResultsRequest>())).ReturnsAsync(voucherVendorResponse);
            var voucherVendorSearchResults = await vendorRepository.VendorSearchForVoucherAsync("Office", "AP");
            Assert.IsTrue(voucherVendorSearchResults.Count() == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task VendorsRepository_VendorSearchForVoucherAsync_ResponseHasErrorMessages()
        {
            voucherVendorResponse.AlErrorMessages = new List<string>() { "ErrorMessage1", "ErrorMessage2" };
            voucherVendorResponse.AError = true;
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<TxGetVoucherVendorResultsRequest, TxGetVoucherVendorResultsResponse>(It.IsAny<TxGetVoucherVendorResultsRequest>())).ReturnsAsync(voucherVendorResponse);
            await vendorRepository.VendorSearchForVoucherAsync("Office", "AP");
        }

        [TestMethod]
        public async Task VendorsRepository_VendorSearchForVoucherAsync_ReturnsSearchResultSuccess()
        {
            voucherVendorResponse.VoucherVendorSearchResults.Add(voucherVendors);
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<TxGetVoucherVendorResultsRequest, TxGetVoucherVendorResultsResponse>(It.IsAny<TxGetVoucherVendorResultsRequest>())).ReturnsAsync(voucherVendorResponse);
            var results = await vendorRepository.VendorSearchForVoucherAsync("Office", "AP");

            var result = results.Where(x => x.VendorId == x.VendorId).FirstOrDefault();
            var voucherVendorResult = voucherVendorResponse.VoucherVendorSearchResults.Where(x => x.AlVendorIds == x.AlVendorIds).FirstOrDefault();

            Assert.IsTrue(results.Count() == 2);
            Assert.AreEqual(result.VendorId, voucherVendorResult.AlVendorIds);
            Assert.AreEqual(result.VendorNameLines.FirstOrDefault(), voucherVendorResult.AlVendorNames);
            Assert.AreEqual(result.Country, voucherVendorResult.AlVendorCountry);
            Assert.AreEqual(result.FormattedAddress, voucherVendorResult.AlVendorFormattedAddresses);
            Assert.AreEqual(result.City, voucherVendorResult.AlVendorCity);
            Assert.AreEqual(result.TaxForm, voucherVendorResult.AlVendorTaxForm);
            Assert.AreEqual(result.TaxFormCode, voucherVendorResult.AlVendorTaxFormCode);
            Assert.AreEqual(result.TaxFormLocation, voucherVendorResult.AlVendorTaxFormLoc);

        }

        #region SearchByKeywordAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_SearchByKeywordAsync_ArgumentNullException()
        {
            var actual = await vendorRepository.SearchByKeywordAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task VendorsRepository_SearchByKeywordAsync_ResponseIsNull()
        {
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<GetActiveVendorResultsRequest, GetActiveVendorResultsResponse>(It.IsAny<GetActiveVendorResultsRequest>())).ReturnsAsync(null);
            await vendorRepository.SearchByKeywordAsync("Office", "AP");
        }

        [TestMethod]
        public async Task VendorsRepository_SearchByKeywordAsync_ResponseNoSearchResultforVendors()
        {
            vendorSearchResultResponse.VendorSearchResults = null;
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<GetActiveVendorResultsRequest, GetActiveVendorResultsResponse>(It.IsAny<GetActiveVendorResultsRequest>())).ReturnsAsync(vendorSearchResultResponse);
            var voucherVendorSearchResults = await vendorRepository.SearchByKeywordAsync("Office", "AP");
            Assert.IsTrue(voucherVendorSearchResults.Count() == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task VendorsRepository_SearchByKeywordAsync_ResponseHasErrorMessages()
        {
            vendorSearchResultResponse.AlErrorMessages = new List<string>() { "ErrorMessage1", "ErrorMessage2" };
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<GetActiveVendorResultsRequest, GetActiveVendorResultsResponse>(It.IsAny<GetActiveVendorResultsRequest>())).ReturnsAsync(vendorSearchResultResponse);
            await vendorRepository.SearchByKeywordAsync("Office", "AP");
        }

        [TestMethod]
        public async Task VendorsRepository_SearchByKeywordAsync_ReturnsSearchResultSuccess()
        {
            vendorSearchResultResponse.VendorSearchResults.Add(vendorSearchResults);
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<GetActiveVendorResultsRequest, GetActiveVendorResultsResponse>(It.IsAny<GetActiveVendorResultsRequest>())).ReturnsAsync(vendorSearchResultResponse);
            var results = await vendorRepository.SearchByKeywordAsync("Office", "AP");

            var result = results.Where(x => x.VendorId == x.VendorId).FirstOrDefault();
            var voucherVendorResult = vendorSearchResultResponse.VendorSearchResults.Where(x => x.AlVendorIds == x.AlVendorIds).FirstOrDefault();

            Assert.IsTrue(results.Count() == 2);
            Assert.AreEqual(result.VendorId, voucherVendorResult.AlVendorIds);
            Assert.AreEqual(result.VendorName, voucherVendorResult.AlVendorNames);
            Assert.AreEqual(result.VendorAddress, voucherVendorResult.AlVendorAddresses);
            Assert.AreEqual(result.TaxForm, voucherVendorResult.AlVendorTaxForm);
            Assert.AreEqual(result.TaxFormCode, voucherVendorResult.AlVendorTaxFormCode);
            Assert.AreEqual(result.TaxFormLocation, voucherVendorResult.AlVendorTaxFormLoc);

        }

        #endregion

        #region GetVendorDefaultTaxFormInfoAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_GetVendorDefaultTaxFormInfoAsync_ArgumentNullException()
        {
            var actual = await vendorRepository.GetVendorDefaultTaxFormInfoAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task VendorsRepository_GetVendorDefaultTaxFormInfoAsync_ResponseIsNull()
        {
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<GetVendDefaultTaxInfoRequest, GetVendDefaultTaxInfoResponse>(It.IsAny<GetVendDefaultTaxInfoRequest>())).ReturnsAsync(null);
            await vendorRepository.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorDefaultTaxFormInfoAsync_ReturnsSearchResultSuccess()
        {
            _iColleagueTransactionInvokerMock.Setup(t => t.ExecuteAsync<GetVendDefaultTaxInfoRequest, GetVendDefaultTaxInfoResponse>(It.IsAny<GetVendDefaultTaxInfoRequest>())).ReturnsAsync(vendorDefaultTaxFormResponse);
            var result = await vendorRepository.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);

            var voucherVendorResult = vendorSearchResultResponse.VendorSearchResults.Where(x => x.AlVendorIds == x.AlVendorIds).FirstOrDefault();
            Assert.IsNotNull(result);

            Assert.AreEqual(result.TaxForm, vendorDefaultTaxFormResponse.ATaxForm);
            Assert.AreEqual(result.TaxFormBoxCode, vendorDefaultTaxFormResponse.ATaxFormCode);
            Assert.AreEqual(result.TaxFormState, vendorDefaultTaxFormResponse.ATaxFormLoc);
        }

        #endregion
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

            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllVendorsFilter:",
                Entity = "VENDORS",
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
            response = new CreateUpdateVendorResponse() { VendorGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", VendorId = "1" };

            voucherVendors = new VoucherVendorSearchResults()
            {
                AlVendorIds = "123",
                AlVendorNames = "Blue Cross Blue Shield",
                AlVendorAddress = "PO Box 69845",
                AlVendorCity = "Minneapolis",
                AlVendorState = "MN",
                AlVendorZip = "55430",
                AlVendorCountry = "USA",
                AlVendorFormattedAddresses = "PO Box 69845 Minneapolis MN 55430 USA",
                AlVendorAddrIds = "143",
                AlVendorTaxForm = "1098",
                AlVendorTaxFormCode = "NEC",
                AlVendorTaxFormLoc = "FL"
            };

            voucherVendorRequest = new TxGetVoucherVendorResultsRequest()
            {
                ASearchCriteria = "Office",
                AApType = "AP"
            };

            voucherVendorResponse = new TxGetVoucherVendorResultsResponse()
            {
                ACorpSearchCriteria = "",
                AError = false,
                VoucherVendorSearchResults = new List<VoucherVendorSearchResults>()
                {
                    new VoucherVendorSearchResults()
                    {
                        AlVendorIds = "124",
                        AlVendorNames = "Blue Cross Office Shield",
                        AlVendorAddress = "PO Box 69845",
                        AlVendorCity = "Minneapolis",
                        AlVendorState = "MN",
                        AlVendorZip = "55430",
                        AlVendorCountry = "USA",
                        AlVendorFormattedAddresses = "PO Box 69845 Minneapolis MN 55430 USA",
                        AlVendorAddrIds = "143",
                        AlVendorTaxForm = "1098",
                        AlVendorTaxFormCode = "NEC",
                        AlVendorTaxFormLoc = "FL"
                    }
                }
            };

            vendorId = "0000190";
            apType = "AP";
            vendorSearchResults = new VendorSearchResults()
            {
                AlVendorIds = vendorId,
                AlVendorNames = "Office Supply",
                AlVendorAddresses = "Address 1",
                AlVendorTaxForm = "1098",
                AlVendorTaxFormCode = "NEC",
                AlVendorTaxFormLoc = "FL"
            };

            vendorSearchResultRequest = new GetActiveVendorResultsRequest()
            {
                ASearchCriteria = "Office",
                AApType = apType
            };

            vendorSearchResultResponse = new GetActiveVendorResultsResponse()
            {
                VendorSearchResults = new List<VendorSearchResults>()
                {
                    new VendorSearchResults()
                    {
                        AlVendorIds = vendorId,
                        AlVendorNames = "Office Supply",
                        AlVendorAddresses = "Address 1",
                        AlVendorTaxForm ="1098",
                        AlVendorTaxFormCode = "NEC",
                        AlVendorTaxFormLoc ="FL"
                    }
                }
            };

            vendorDefaultTaxFormRequest = new GetVendDefaultTaxInfoRequest()
            {
                AVendorId = vendorId,
                AApType = apType
            };

            vendorDefaultTaxFormResponse = new GetVendDefaultTaxInfoResponse()
            {
                ATaxForm = "1098",
                ATaxFormCode = "NEC",
                ATaxFormLoc = "FL"
            };

        }
    }

    [TestClass]
    public class VendorsRepository_VendorsMaximumTests : BaseRepositorySetup
    {
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;

        private VendorsRepository vendorRepository;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors> vendors;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Address> address;
      
        private Ellucian.Colleague.Domain.ColleagueFinance.Entities.Vendors vendorEntity;
        private CreateUpdateVendorResponse response;
        private VoucherVendorSearchResults voucherVendors;
        private TxGetVoucherVendorResultsRequest voucherVendorRequest;
        private TxGetVoucherVendorResultsResponse voucherVendorResponse;

        private VendorSearchResults vendorSearchResults;
        private GetActiveVendorResultsRequest vendorSearchResultRequest;
        private GetActiveVendorResultsResponse vendorSearchResultResponse;

        private VendorDefaultTaxFormInfo vendorDefaultTaxFormInfo;
        private GetVendDefaultTaxInfoRequest vendorDefaultTaxFormRequest;
        private GetVendDefaultTaxInfoResponse vendorDefaultTaxFormResponse;
        private string vendorId;
        private string apType;

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

            vendorRepository = new VendorsRepository(cacheProviderMock.Object, _iColleagueTransactionFactoryMock.Object, _iLoggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _iColleagueTransactionFactoryMock = null;
            _iColleagueTransactionInvokerMock = null;
            _iLoggerMock = null;
            _dataReaderMock = null;
            apiSettings = null;
            vendorRepository = null;
            transManager = null;
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", ids, true)).ReturnsAsync(address);

            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendorsMaximumAsync_person_not_found()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(new Collection<Person>() { new Person() { Recordkey = "1234" } });
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_VendorsFilter()
        {
            string vendorID = "TestCriteria";
            criteria = "WITH VENDORS.ID = '" + vendorID + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit, vendorID);
            Assert.IsNotNull(actuals);
        }


        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_TypesFilter_EP()
        {
            List<string> typesID = new List<string>() { "eprocurement", "travel", "procurement" };
            criteria = "WITH VEN.CATEGORIES = '" + "EP" + "' AND WITH VEN.CATEGORIES = '" + "TR" + "'" + "' AND WITH VEN.CATEGORIES = '" + "PR" + "'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit, "", null,  typesID);
            Assert.IsNotNull(actuals);
        }



        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_StatusFilter_Active()
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
            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit, "",  statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_StatusFilter_holdPayment()
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
            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit, "",  statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_StatusFilter_Approved()
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
            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit, "",  statuses);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_TaxId_US()
        {
            var taxId = "123456789";
            var taxcriteria = string.Format("WITH CORP.TAX.ID EQ '{0}'", taxId);
            _dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new IntlParams() { HostCountry = "USA" });
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("CORP.FOUNDS", It.IsAny<string[]>(), taxcriteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit, "", null, null, taxId);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_TaxId_NoCorp()
        {
            var taxId = "123456789";
            var taxcriteria = string.Format("WITH CORP.TAX.ID EQ '{0}'", taxId);
            _dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new IntlParams() { HostCountry = "USA" });
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("CORP.FOUNDS", It.IsAny<string[]>(), taxcriteria)).ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit, "", null, null,  taxId);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_TaxId_NoVendor()
        {
            var taxId = "123456789";
            var taxcriteria = string.Format("WITH CORP.TAX.ID EQ '{0}'", taxId);
            _dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new IntlParams() { HostCountry = "USA" });
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", ids, criteria)).ReturnsAsync(new string[0]);
            _dataReaderMock.Setup(repo => repo.SelectAsync("CORP.FOUNDS", It.IsAny<string[]>(), taxcriteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit, "", null, null, taxId);
            Assert.IsNotNull(actuals);
        }


        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_FilterCombo()
        {
            string vendorID = "TestCriteria";
            List<string> classifications = new List<string>() { "TestCriteria" };
            criteria = "WITH VENDORS.ID = 'TestCriteria' AND WITH VEN.TYPES = 'TestCriteria' AND VEN.APPROVAL.FLAG = 'Y'";
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", It.IsAny<string>())).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            List<string> statuses = new List<string>() { "approved" };
            List<string> types = new List<string>() { "eprocurement", "travel" };
            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit, vendorID, statuses, types);
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumByGuidAsync()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            //var expectedAddress = address.FirstOrDefault(i => i.Recordkey.Equals(expectedPerson.PersonAddresses.FirstOrDefault().ToString()))
   
            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(expectedPerson);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", ids, true)).ReturnsAsync(address);

            var actual = await vendorRepository.GetVendorsMaximumByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendorsMaximumByGuidAsync_NoPerson()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", ids, true)).ReturnsAsync(corp);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>(expectedRecordKey, true)).ReturnsAsync(null);

            await vendorRepository.GetVendorsMaximumByGuidAsync(guid);
        }


        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_Vendors_Null()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(null);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);

            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit);
        }


        [TestMethod]
        public async Task VendorsRepository_GetVendorsMaximumAsync_No_Records_Selected()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(people);
            _iColleagueTransactionInvokerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
       .ReturnsAsync(null);
            var actuals = await vendorRepository.GetVendorsMaximumAsync(offset, limit);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorsRepository_GetVendorsMaximumAsync_People_Null()
        {
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDORS", criteria)).ReturnsAsync(ids);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", ids, true))
                .ReturnsAsync(vendors);
            _dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", ids, true))
                .ReturnsAsync(null);

            await vendorRepository.GetVendorsMaximumAsync(offset, limit);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsMaximumByGuidAsync_IdNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(expectedVendor);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedRecordKey, true)).ReturnsAsync(expectedPerson);

            var actual = await vendorRepository.GetVendorsMaximumByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendorsMaximumByGuidAsync_GuidNull_KeyNotFoundException()
        {
            var actual = await vendorRepository.GetVendorsMaximumByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsMaximumByGuidAsync_DictionaryNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = null;

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var actual = await vendorRepository.GetVendorsMaximumByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsMaximumByGuidAsync_FoundEntryNull_KeyNotFoundException()
        {
            GuidLookupResult result = null;
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var actual = await vendorRepository.GetVendorsMaximumByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendorsMaximumByGuidAsync_WrongEntityName_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "WrongEntityName", PrimaryKey = null, SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var actual = await vendorRepository.GetVendorsMaximumByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorsRepository_GetVendorsMaximumByGuidAsync_VendorNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "VENDORS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            var expectedVendor = vendors.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));

            _dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(expectedRecordKey, true)).ReturnsAsync(null);

            var actual = await vendorRepository.GetVendorsMaximumByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorsRepository_GetVendorsMaximumByGuidAsync_PersonNull_KeyNotFoundException()
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

            var actual = await vendorRepository.GetVendorsMaximumByGuidAsync(guid);
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

            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllVendorsFilter:",
                Entity = "VENDORS",
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
            people = new Collection<Base.DataContracts.Person>()
            {
                new Base.DataContracts.Person(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "1", PersonCorpIndicator = "Y", PersonAddresses = new List<string> {"5"},
                        PerphoneEntityAssociation = new List<PersonPerphone>() { new PersonPerphone() { PersonalPhoneNumberAssocMember = "1234567890", PersonalPhoneTypeAssocMember = "HOME"} }},
                new Base.DataContracts.Person(){RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff", Recordkey = "2", PersonCorpIndicator = "Y", PersonAddresses = new List<string> {"6"} ,
                PerphoneEntityAssociation = new List<PersonPerphone>() { new PersonPerphone() { PersonalPhoneNumberAssocMember = "1234567890", PersonalPhoneTypeAssocMember = "HOME"} }},
                new Base.DataContracts.Person(){RecordGuid = "41a341d6-ebc0-4ac7-a77f-262e5e7dfd62", Recordkey = "3", PersonCorpIndicator = "N", PersonAddresses = new List<string> {"7"} ,
                PerphoneEntityAssociation = new List<PersonPerphone>() { new PersonPerphone() { PersonalPhoneNumberAssocMember = "1234567890", PersonalPhoneTypeAssocMember = "HOME" } }},
                new Base.DataContracts.Person(){RecordGuid = "4d9962e8-195b-4442-93d7-197901cfb438", Recordkey = "4", PersonCorpIndicator = "N", PersonAddresses = new List<string> {"8"} ,
                PerphoneEntityAssociation = new List<PersonPerphone>() { new PersonPerphone() { PersonalPhoneNumberAssocMember = "1234567890", PersonalPhoneTypeAssocMember = "HOME"} }}
            };

            address = new Collection<Address>()
            {
                new Address() {Recordkey = "5", AddressLines = new List<string> {"123 Main St"}, City = "Buffalo", State = "NY", Zip = "14225"},
                new Address() {Recordkey = "6", AddressLines = new List<string> {"123 Main St"}, City = "Buffalo", State = "NY", Zip = "14225"},
                new Address() {Recordkey = "7", AddressLines = new List<string> {"123 Main St"}, City = "Buffalo", State = "NY", Zip = "14225"},
                new Address() {Recordkey = "8", AddressLines = new List<string> {"123 Main St"}, City = "Buffalo", State = "NY", Zip = "14225"}

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
            response = new CreateUpdateVendorResponse() { VendorGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", VendorId = "1" };

            voucherVendors = new VoucherVendorSearchResults()
            {
                AlVendorIds = "123",
                AlVendorNames = "Blue Cross Blue Shield",
                AlVendorAddress = "PO Box 69845",
                AlVendorCity = "Minneapolis",
                AlVendorState = "MN",
                AlVendorZip = "55430",
                AlVendorCountry = "USA",
                AlVendorFormattedAddresses = "PO Box 69845 Minneapolis MN 55430 USA",
                AlVendorAddrIds = "143",
                AlVendorTaxForm = "1098",
                AlVendorTaxFormCode = "NEC",
                AlVendorTaxFormLoc = "FL"
            };

            voucherVendorRequest = new TxGetVoucherVendorResultsRequest()
            {
                ASearchCriteria = "Office",
                AApType = "AP"
            };

            voucherVendorResponse = new TxGetVoucherVendorResultsResponse()
            {
                ACorpSearchCriteria = "",
                AError = false,
                VoucherVendorSearchResults = new List<VoucherVendorSearchResults>()
                {
                    new VoucherVendorSearchResults()
                    {
                        AlVendorIds = "124",
                        AlVendorNames = "Blue Cross Office Shield",
                        AlVendorAddress = "PO Box 69845",
                        AlVendorCity = "Minneapolis",
                        AlVendorState = "MN",
                        AlVendorZip = "55430",
                        AlVendorCountry = "USA",
                        AlVendorFormattedAddresses = "PO Box 69845 Minneapolis MN 55430 USA",
                        AlVendorAddrIds = "143",
                        AlVendorTaxForm = "1098",
                        AlVendorTaxFormCode = "NEC",
                        AlVendorTaxFormLoc = "FL"
                    }
                }
            };

            vendorId = "0000190";
            apType = "AP";
            vendorSearchResults = new VendorSearchResults()
            {
                AlVendorIds = vendorId,
                AlVendorNames = "Office Supply",
                AlVendorAddresses = "Address 1",
                AlVendorTaxForm = "1098",
                AlVendorTaxFormCode = "NEC",
                AlVendorTaxFormLoc = "FL"
            };

            vendorSearchResultRequest = new GetActiveVendorResultsRequest()
            {
                ASearchCriteria = "Office",
                AApType = apType
            };

            vendorSearchResultResponse = new GetActiveVendorResultsResponse()
            {
                VendorSearchResults = new List<VendorSearchResults>()
                {
                    new VendorSearchResults()
                    {
                        AlVendorIds = vendorId,
                        AlVendorNames = "Office Supply",
                        AlVendorAddresses = "Address 1",
                        AlVendorTaxForm ="1098",
                        AlVendorTaxFormCode = "NEC",
                        AlVendorTaxFormLoc ="FL"
                    }
                }
            };

            vendorDefaultTaxFormRequest = new GetVendDefaultTaxInfoRequest()
            {
                AVendorId = vendorId,
                AApType = apType
            };

            vendorDefaultTaxFormResponse = new GetVendDefaultTaxInfoResponse()
            {
                ATaxForm = "1098",
                ATaxFormCode = "NEC",
                ATaxFormLoc = "FL"
            };

        }
    }
}
