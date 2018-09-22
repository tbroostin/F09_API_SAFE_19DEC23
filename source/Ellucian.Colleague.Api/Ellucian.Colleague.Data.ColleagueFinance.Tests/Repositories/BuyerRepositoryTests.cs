// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
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
using Ellucian.Colleague.Data.Base.Tests.Repositories;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class BuyerRepositoryTests
    {
        [TestClass]
        public class BuyerRepositoryGetMethods : BaseRepositorySetup
        {
            Mock<ICacheProvider> iCacheProviderMock;
            Mock<IColleagueTransactionFactory> iColleagueTransactionFactoryMock;
            Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
            Mock<IColleagueDataReader> dataAccessorMock;    
            //Mock<IColleagueDataReader> dataReaderMock;
            //ApiSettings apiSettings;
            Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;
            BuyerRepository buyerRepository;
            List<Buyer> buyerEntities;
            Collection<Ellucian.Colleague.Data.Base.DataContracts.Staff> records;
            string guid = "3af740fe-ef2b-49f1-9f66-e7d9491e2064";
            int offset = 0;
            int limit = 2;

            //int readSize;


            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                iCacheProviderMock = new Mock<ICacheProvider>();
                iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                //dataAccessorMock = new Mock<IColleagueDataReader>();
                //dataReaderMock = new Mock<IColleagueDataReader>();
                iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);

                apiSettings = new ApiSettings("TEST");
                BuildEntities();
                buyerRepository = BuildBuyerRepositoryRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                iCacheProviderMock = null;
                iColleagueTransactionFactoryMock = null;
                iColleagueTransactionInvokerMock = null;
                //iLoggerMock = null;
                //dataReaderMock = null;
                buyerRepository = null;
                buyerRepository = null;
            }

            [TestMethod]
            public async Task BuyerRepository_GetBuyersAsync()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = buyerEntities.Where(e => e.RecordKey == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "PERSON", record.RecordKey}),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                var results = await buyerRepository.GetBuyersAsync(offset, limit, It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task BuyerRepository_GetBuyersAsync_NullResults()
            {
                dataAccessorMock.Setup(repo => repo.SelectAsync("STAFF", string.Empty)).ReturnsAsync(null);
                var results = await buyerRepository.GetBuyersAsync(offset, limit, It.IsAny<bool>());
                Assert.IsNull(results.Item1);
            }

            [TestMethod]
            public async Task BuyerRepository_GetBuyerByIdAsync()
            {
                dataAccessorMock.Setup(i => i.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Staff>("STAFF", It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(records.FirstOrDefault(i => i.Recordkey.Equals("1")));

                dataAccessorMock.Setup(i => i.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(people.FirstOrDefault(i => i.Recordkey.Equals("1")));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = buyerEntities.Where(e => e.RecordKey == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "PERSON", record.RecordKey }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                var results = await buyerRepository.GetBuyerAsync(guid);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task BuyerRepository_GetBuyersAsync_Exception()
            {
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(It.IsAny<string[]>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var results = await buyerRepository.GetBuyersAsync(offset, limit, It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BuyerRepository_GetBuyerByIdAsync_StaffIdNull_KeyNotFoundException()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    return Task.FromResult(new Dictionary<string, GuidLookupResult>());
                });
                var results = await buyerRepository.GetBuyerAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BuyerRepository_GetBuyerByIdAsync_StaffRecordNull_KeyNotFoundException()
            {
                dataAccessorMock.Setup(i => i.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Staff>("STAFF", It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(null);
                var results = await buyerRepository.GetBuyerAsync(guid);
            }

            private void BuildEntities()
            {
                buyerEntities = new List<Buyer>() 
                {
                    new Buyer()
                    {
                        EndOn = DateTime.Today.AddDays(30),
                        Guid = "3af740fe-ef2b-49f1-9f66-e7d9491e2064",
                        Name = "First1, Last1",
                        PersonGuid = "4efe4633-b817-4fac-aada-2ca7d28de833",
                        RecordKey = "1",
                        StartOn = DateTime.Today,
                        Status = "active"
                    },
                    new Buyer()
                    {
                        EndOn = DateTime.Today.AddDays(40),
                        Guid = "8e1d05b3-534c-4a92-9b24-adcc11afdb0a",
                        Name = "First2, Last2",
                        PersonGuid = "58e39925-e912-45df-8a6f-f2af5f5f76ac",
                        RecordKey = "2",
                        StartOn = DateTime.Today.AddDays(2),
                        Status = "inactive"
                    }
                };
            }

            public BuyerRepository BuildBuyerRepositoryRepository()
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
                string[] ids = new[] { "1", "2" };

                dataAccessorMock.Setup(repo => repo.SelectAsync("STAFF", string.Empty)).ReturnsAsync(ids);

                people = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>() 
                {
                    new Ellucian.Colleague.Data.Base.DataContracts.Person(){ RecordGuid = "3af740fe-ef2b-49f1-9f66-e7d9491e2064", Recordkey = "1", FirstName = "FirstName1", LastName = "LastName1"  },
                    new Ellucian.Colleague.Data.Base.DataContracts.Person(){ RecordGuid = "8e1d05b3-534c-4a92-9b24-adcc11afdb0a", Recordkey = "2", FirstName = "FirstName2", LastName = "LastName2"  },
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(ids, It.IsAny<bool>())).ReturnsAsync(people);

                ApplValcodes applValCodes = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "A"
                            //ValActionCode1AssocMember = "A"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = ""
                            //ValActionCode1AssocMember = ""
                        }
                    }
                };
                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES", It.IsAny<bool>())).ReturnsAsync(applValCodes);

                records = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Staff>();
                foreach (var item in buyerEntities)
                {
                    Ellucian.Colleague.Data.Base.DataContracts.Staff record = new Ellucian.Colleague.Data.Base.DataContracts.Staff();

                    record.RecordGuid = item.Guid;
                    record.StaffStatus = "A";
                    record.Recordkey = item.RecordKey;
                    record.StaffAddDate = item.StartOn;
                    record.StaffChangeDate = item.EndOn;
                    records.Add(record);
                }
                records.ElementAt(1).StaffStatus = "";
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Staff>(ids, It.IsAny<bool>())).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        var buyer = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                        result.Add(gl.Guid, buyer == null ? null : new GuidLookupResult() { Entity = "STAFF", PrimaryKey = buyer.Recordkey });
                    }
                    return Task.FromResult(result);
                });

                // Build  repository
                buyerRepository = new BuyerRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return buyerRepository;
            }

            //[TestMethod]
            //public async Task PersonHoldsRepo_GetPersonHoldByIdAsync()
            //{
            //    StudentRestrictions studentRestrictions = new StudentRestrictions() { Recordkey = "1", StrStudent = "1", StrComments = "Comment 1", StrEndDate = DateTime.MaxValue, StrRestriction = "Academic", StrStartDate = DateTime.MinValue };
            //    GuidLookupResult res = new GuidLookupResult(){ Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "1", SecondaryKey = ""};
            //    Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
            //    dict.Add("1", res);

            //    dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
            //        {
            //            return Task.FromResult(dict);
            //        });
            //    dataReaderMock.Setup(i => i.ReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", It.IsAny<string>(), true)).ReturnsAsync(studentRestrictions);

                
            //    var result = await personHoldRepository.GetPersonHoldByIdAsync("1");

            //    Assert.AreEqual(studentRestrictions.Recordkey, result.Id);
            //    Assert.AreEqual(studentRestrictions.StrStudent, result.StudentId);
            //    Assert.AreEqual(studentRestrictions.StrComments, result.Comment);
            //    Assert.AreEqual(studentRestrictions.StrEndDate, result.EndDate);
            //    Assert.AreEqual(studentRestrictions.StrStartDate, result.StartDate);
            //    Assert.AreEqual(studentRestrictions.StrRestriction, result.RestrictionId);
            //}

            //[TestMethod]
            //public async Task PersonHoldsRepo_GetPersonHoldByPersonIdAsync()
            //{
            //    Collection<StudentRestrictions> studentHolds = new Collection<StudentRestrictions>() 
            //    {
            //        new StudentRestrictions()
            //            { Recordkey = "1", StrStudent = "1", StrComments = "Comment 1", StrEndDate = DateTime.MaxValue, StrRestriction = "Academic", StrStartDate = DateTime.MinValue },
            //        new StudentRestrictions()
            //            { Recordkey = "2", StrStudent = "1", StrComments = "Comment 2", StrEndDate = DateTime.MaxValue, StrRestriction = "Health", StrStartDate = DateTime.MinValue },
            //    };

            //    GuidLookupResult res = new GuidLookupResult() { Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "1", SecondaryKey = "" };
            //    Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
            //    dict.Add("1", res);
            //    dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
            //    {
            //        return Task.FromResult(dict);
            //    });

            //    dataReaderMock.Setup<Task<Collection<StudentRestrictions>>>(i => i.BulkReadRecordAsync<StudentRestrictions>(It.IsAny<string>(), true)).ReturnsAsync(studentHolds);

            //    var results = await personHoldRepository.GetPersonHoldsByPersonIdAsync("1");

            //    for (int i = 0; i < 2; i++)
            //    {
            //        StudentRestrictions restriction = studentHolds[i];
            //        PersonRestriction personRestriction = results.FirstOrDefault(key => key.Id == restriction.Recordkey);

            //        Assert.AreEqual(restriction.Recordkey, personRestriction.Id);
            //        Assert.AreEqual(restriction.StrStudent, personRestriction.StudentId);
            //        Assert.AreEqual(restriction.StrComments, personRestriction.Comment);
            //        Assert.AreEqual(restriction.StrEndDate, personRestriction.EndDate);
            //        Assert.AreEqual(restriction.StrStartDate, personRestriction.StartDate);
            //        Assert.AreEqual(restriction.StrRestriction, personRestriction.RestrictionId);
            //    }
            //}
            //[TestMethod]
            //public async Task PersonHoldRepo_GetStudentHoldIdFromGuidAsync()
            //{
            //    GuidLookupResult res = new GuidLookupResult() { Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "1", SecondaryKey = "" };
            //    Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
            //    dict.Add("1", res);
            //    dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
            //    {
            //        return Task.FromResult(dict);
            //    });

            //    var result = await personHoldRepository.GetStudentHoldIdFromGuidAsync("1");
            //    var value = dict.FirstOrDefault(i => i.Key == "1");
            //    Assert.AreEqual(res.PrimaryKey, value.Value.PrimaryKey);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task PersonHoldsRepo_GetPersonHoldByPersonIdAsync_ArgumentNullException()
            //{
            //    var results = await personHoldRepository.GetPersonHoldsByPersonIdAsync("");               
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task PersonHoldsRepo_GetPersonHoldByPersonIdAsync_PersonKeyNull_ArgumentNullException()
            //{
            //    Collection<StudentRestrictions> studentHolds = new Collection<StudentRestrictions>() 
            //    {
            //        new StudentRestrictions()
            //            { Recordkey = "1", StrStudent = "1", StrComments = "Comment 1", StrEndDate = DateTime.MaxValue, StrRestriction = "Academic", StrStartDate = DateTime.MinValue },
            //        new StudentRestrictions()
            //            { Recordkey = "2", StrStudent = "1", StrComments = "Comment 2", StrEndDate = DateTime.MaxValue, StrRestriction = "Health", StrStartDate = DateTime.MinValue },
            //    };
            //    GuidLookupResult res = new GuidLookupResult() { Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "", SecondaryKey = "" };
            //    Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
            //    dict.Add("5", res);
            //    dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
            //    {
            //        return Task.FromResult(dict);
            //    });

            //    var results = await personHoldRepository.GetPersonHoldsByPersonIdAsync("1");
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task PersonHoldRepo_GetStudentHoldGuidFromIdAsync_ArgumentNullException()
            //{
            //    var result = await personHoldRepository.GetStudentHoldGuidFromIdAsync("");
            //}

            //[TestMethod]
            //[ExpectedException(typeof(RepositoryException))]
            //public async Task PersonHoldRepo_GetStudentHoldGuidFromIdAsync_RepositoryException()
            //{
            //    var result = await personHoldRepository.GetStudentHoldGuidFromIdAsync("1");
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task PersonHoldRepo_GetStudentHoldIdFromGuidAsync_ArgumentNullException()
            //{
            //    var result = await personHoldRepository.GetStudentHoldIdFromGuidAsync("");
            //}

            //[TestMethod]
            //[ExpectedException(typeof(RepositoryException))]
            //public async Task PersonHoldRepo_GetStudentHoldIdFromGuidAsync_RepositoryException()
            //{
            //    dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ThrowsAsync(new RepositoryException());
            //    var result = await personHoldRepository.GetStudentHoldIdFromGuidAsync("abc");
            //}
        }
    }
}
