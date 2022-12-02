//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
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
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentFinancialAidOfficeRepositoryTests
    {
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<ILogger> loggerMock;

        List<FinancialAidOfficeItem> allFinancialAidOffices = new List<FinancialAidOfficeItem>();
        string valcodeName;
        ApiSettings apiSettings;

        StudentFinancialAidOfficeRepository studentFinancialAidOfficeRepository;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");

            allFinancialAidOffices.Add(new Domain.Student.Entities.FinancialAidOfficeItem("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2001", "CODE1", "NAME1") { AddressLines = new List<string>() { "line1" }, City = "city1", State = "state1", PostalCode = "post1", AidAdministrator = "admin1", PhoneNumber = "phone1", FaxNumber = "fax1", EmailAddress = "email1" });
            allFinancialAidOffices.Add(new Domain.Student.Entities.FinancialAidOfficeItem("73244057-D1EC-4094-A0B7-DE602533E3A6", "2002", "CODE2", "NAME2") { AddressLines = new List<string>() { "line2" }, City = "city2", State = "state2", PostalCode = "post2", AidAdministrator = "admin2", PhoneNumber = "phone2", FaxNumber = "fax2", EmailAddress = "email2" });
            allFinancialAidOffices.Add(new Domain.Student.Entities.FinancialAidOfficeItem("1df164eb-8178-4321-a9f7-24f12d3991d8", "2003", "CODE3", "NAME3") { AddressLines = new List<string>() { "line3" }, City = "city3", State = "state3", PostalCode = "post3", AidAdministrator = "admin3", PhoneNumber = "phone3", FaxNumber = "fax3", EmailAddress = "email3" });

            studentFinancialAidOfficeRepository = BuildValidReferenceDataRepository();
            valcodeName = studentFinancialAidOfficeRepository.BuildFullCacheKey("AllFinancialAidOffices");
        }

        [TestCleanup]
        public void Cleanup()
        {
            allFinancialAidOffices = null;
            valcodeName = string.Empty;
            apiSettings = null;
        }

        [TestMethod]
        public async Task StudentFinancialAidOfficeRepository_GetFinancialAidOfficesAsync_False()
        {
            var results = await studentFinancialAidOfficeRepository.GetFinancialAidOfficesAsync(false);
            Assert.AreEqual(allFinancialAidOffices.Count(), results.Count());

            foreach (var financialAidOffice in allFinancialAidOffices)
            {
                var result = results.FirstOrDefault(i => i.Guid == financialAidOffice.Guid);

                Assert.AreEqual(financialAidOffice.Code, result.Code);
                Assert.AreEqual(financialAidOffice.Code, result.Description);
                Assert.AreEqual(financialAidOffice.Guid, result.Guid);
            }

        }

        [TestMethod]
        public async Task StudentFinancialAidOfficeRepository_GetFinancialAidOfficesAsync_True()
        {
            var results = await studentFinancialAidOfficeRepository.GetFinancialAidOfficesAsync(true);
            Assert.AreEqual(allFinancialAidOffices.Count(), results.Count());

            foreach (var financialAidOffice in allFinancialAidOffices)
            {
                var result = results.FirstOrDefault(i => i.Guid == financialAidOffice.Guid);

                Assert.AreEqual(financialAidOffice.Code, result.Code);
                Assert.AreEqual(financialAidOffice.Code, result.Description);
                Assert.AreEqual(financialAidOffice.Guid, result.Guid);
            }

        }

        [TestMethod]
        public async Task StudentFinancialAidOfficeRepository_GetFinancialAidOfficesAsync_FaofcAddress_Null()
        {
            allFinancialAidOffices.First().AddressLines = null;
            allFinancialAidOffices.First().City = string.Empty;
            allFinancialAidOffices.First().State = string.Empty;
            allFinancialAidOffices.First().PostalCode = string.Empty;
            var results = await studentFinancialAidOfficeRepository.GetFinancialAidOfficesAsync(true);
            Assert.AreEqual(allFinancialAidOffices.Count(), results.Count());

            foreach (var financialAidOffice in allFinancialAidOffices)
            {
                var result = results.FirstOrDefault(i => i.Guid == financialAidOffice.Guid);

                Assert.AreEqual(financialAidOffice.Code, result.Code);
                Assert.AreEqual(financialAidOffice.Code, result.Description);
                Assert.AreEqual(financialAidOffice.Guid, result.Guid);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task StudentFinancialAidOfficeRepository_GetFinancialAidOfficesAsync_Exception()
        {
            dataAccessorMock.Setup(repo => repo.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", It.IsAny<bool>())).ThrowsAsync(new Exception());
            var results = await studentFinancialAidOfficeRepository.GetFinancialAidOfficesAsync(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinancialAidOfficeRepository_GetFinancialAidOfficesAsync_Entity_NameNUll()
        {
            FinancialAidOfficeItem item = new FinancialAidOfficeItem(Guid.NewGuid().ToString(), "123", "descr", "");
        }

        private StudentFinancialAidOfficeRepository BuildValidReferenceDataRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            // Set up data accessor for mocking 
            dataAccessorMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            var records = new Collection<DataContracts.FaOffices>();
            foreach (var item in allFinancialAidOffices)
            {
                DataContracts.FaOffices record = new DataContracts.FaOffices();
                record.RecordGuid = item.Guid;
                //record = item.Description;
                record.Recordkey = item.Code;
                record.FaofcName = item.Name;
                record.FaofcAddress = item.AddressLines;
                record.FaofcCity = item.City;
                record.FaofcState = item.State;
                record.FaofcZip = item.PostalCode;
                record.FaofcPellFaDirector = item.AidAdministrator;
                record.FaofcPellPhoneNumber = item.PhoneNumber;
                record.FaofcPellFaxNumber = item.FaxNumber;
                record.FaofcPellInternetAddress = item.EmailAddress;
                records.Add(record);
            }
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.FaOffices>("FA.OFFICES", "", true)).ReturnsAsync(records);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
            {
                var result = new Dictionary<string, RecordKeyLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var record = allFinancialAidOffices.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                    result.Add(string.Join("+", new string[] { "FA.OFFICES", record.Code }),
                        new RecordKeyLookupResult() { Guid = record.Guid });
                }
                return Task.FromResult(result);
            });

            // Construct repository
            studentFinancialAidOfficeRepository = new StudentFinancialAidOfficeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return studentFinancialAidOfficeRepository;
        }
    }
}