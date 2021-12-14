// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
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
using Ellucian.Colleague.Domain.Base.Transactions;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class ExternalEmploymentRepositoryTests
    {
        [TestClass]
        public class ExternalEmploymentRepositoryGetMethods : BaseRepositorySetup
        {
            Mock<ICacheProvider> iCacheProviderMock;
            Mock<IColleagueTransactionFactory> iColleagueTransactionFactoryMock;
            Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            //Mock<IColleagueDataReader> dataReaderMock;
            //ApiSettings apiSettings;
            //Collection<DataContracts.Person> people;
            ExternalEmploymentsRepository externalEmploymentRepository;
            List<ExternalEmployments> externalEmploymentEntities;
            Collection<DataContracts.Employmt> records;
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
                externalEmploymentRepository = BuildExternalEmploymentRepositoryRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                iCacheProviderMock = null;
                iColleagueTransactionFactoryMock = null;
                iColleagueTransactionInvokerMock = null;
                //iLoggerMock = null;
                //dataReaderMock = null;
                externalEmploymentRepository = null;
                externalEmploymentRepository = null;
            }

            [TestMethod]
            public async Task ExternalEmploymentRepository_GetExternalEmploymentsAsync()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = externalEmploymentEntities.Where(e => e.Guid == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "EMPLOYMT", record.Guid }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                var results = await externalEmploymentRepository.GetExternalEmploymentsAsync(offset, limit);
                Assert.IsNotNull(results);
            }

            //[TestMethod]
            //public async Task ExternalEmploymentRepository_GetExternalEmploymentsAsync_NullResults()
            //{
            //    dataAccessorMock.Setup(repo => repo.SelectAsync("EMPLOYMT", string.Empty)).ReturnsAsync(() => null);
            //    var results = await externalEmploymentRepository.GetExternalEmploymentsAsync(offset, limit);
            //    Assert.IsNull(results.Item1);
            //}

            [TestMethod]
            public async Task ExternalEmploymentRepository_GetExternalEmploymentByIdAsync()
            {
                //dataAccessorMock.Setup(i => i.ReadRecordAsync<DataContracts.Employmt>("EMPLOYMT", It.IsAny<string>(), It.IsAny<bool>()))
                //    .ReturnsAsync(records.FirstOrDefault(i => i.Recordkey.Equals("1")));
                dataAccessorMock.Setup(i => i.ReadRecordAsync<DataContracts.Employmt>(It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(records.FirstOrDefault(i => i.Recordkey.Equals("1")));
                //dataAccessorMock.Setup(i => i.ReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string>(), It.IsAny<bool>()))
                //    .ReturnsAsync(people.FirstOrDefault(i => i.Recordkey.Equals("1")));


                var results = await externalEmploymentRepository.GetExternalEmploymentsByGuidAsync(guid);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ExternalEmploymentRepository_GetExternalEmploymentsAsync_Exception()
            {
                dataAccessorMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.Employmt>("EMPLOYMT", It.IsAny<string[]>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var results = await externalEmploymentRepository.GetExternalEmploymentsAsync(offset, limit);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ExternalEmploymentRepository_GetExternalEmploymentByIdAsync_EmploymtIdNull_KeyNotFoundException()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    return Task.FromResult(new Dictionary<string, GuidLookupResult>());
                });
                var results = await externalEmploymentRepository.GetExternalEmploymentsByGuidAsync(guid);
            }

            //[TestMethod]
            //[ExpectedException(typeof(Exception))]
            //public async Task ExternalEmploymentRepository_GetExternalEmploymentByIdAsync_EmploymtRecordNull_KeyNotFoundException()
            //{
            //    dataAccessorMock.Setup(i => i.ReadRecordAsync<DataContracts.Employmt>(It.IsAny<string>(), It.IsAny<bool>()))
            //        .ReturnsAsync(() => null);
            //    var results = await externalEmploymentRepository.GetExternalEmploymentsByGuidAsync(guid);
            //}

            private void BuildEntities()
            {
                externalEmploymentEntities = new List<ExternalEmployments>() 
                {
                    new ExternalEmployments("3af740fe-ef2b-49f1-9f66-e7d9491e2064", "1", "1", "DESC1", "active")
                    {
                        EndDate = DateTime.Today.AddDays(30),
                        //PersonGuid = "4efe4633-b817-4fac-aada-2ca7d28de833",
                        StartDate = DateTime.Today,
                        Status = "active",
                        OrganizationId = "ORG1",
                        Supervisors = new List<ExternalEmploymentSupervisors>()
                        {
                            new ExternalEmploymentSupervisors("first", "last", "phone", "email")
                        }
                    },
                    new ExternalEmployments("8e1d05b3-534c-4a92-9b24-adcc11afdb0a", "2", "2", "DESC2", "STAT2")
                    {
                        EndDate = DateTime.Today.AddDays(40),
                        //PersonGuid = "58e39925-e912-45df-8a6f-f2af5f5f76ac",
                        StartDate = DateTime.Today.AddDays(2),
                        Status = "inactive"
                    }
                };
            }

            public ExternalEmploymentsRepository BuildExternalEmploymentRepositoryRepository()
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

                dataAccessorMock.Setup(repo => repo.SelectAsync("EMPLOYMT", string.Empty)).ReturnsAsync(ids);
                dataAccessorMock.Setup(repo => repo.SelectAsync("EMPLOYMT", It.IsAny<string>())).ReturnsAsync(ids);

                //people = new Collection<DataContracts.Employmt>() 
                //{
                //    new DataContracts.Employmt(){ RecordGuid = "ef11bd15-ebeb-4e9e-9b8a-d14c5ff2b097", Recordkey = "1" },
                //    new DataContracts.Employmt(){ RecordGuid = "fd9b4a83-b13a-47a2-9a09-65e1b9149678", Recordkey = "2" },
                //};
                //dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Employmt>(ids, It.IsAny<bool>())).ReturnsAsync(people);

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

                records = new Collection<DataContracts.Employmt>();
                foreach (var item in externalEmploymentEntities)
                {
                    DataContracts.Employmt record = new DataContracts.Employmt();

                    record.RecordGuid = item.Guid;
                    //record.StaffStatus = "A";
                    record.Recordkey = item.Id;
                    record.EmpStatus = item.Status;
                    record.EmpEmployee = item.PersonId;
                    record.EmpTitle = item.JobTitle;
                    record.EmpSpvsrEntityAssociation = new List<EmploymtEmpSpvsr>()
                    {
                        new EmploymtEmpSpvsr() 
                        {
                            EmpSpvsrFirstNameAssocMember = "first",
                            EmpSpvsrLastNameAssocMember = "last",
                            EmpSpvsrPhoneAssocMember = "phone",
                            EmpSpvsrEmailAssocMember = "email"
                        }
                    };
                    //record.StaffAddDate = item.StartOn;
                    //record.StaffChangeDate = item.EndOn;
                    records.Add(record);
                }
                //records.ElementAt(1).StaffStatus = "";
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Employmt>(ids, It.IsAny<bool>())).ReturnsAsync(records);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Employmt>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(records);

                var invalidRecords = new Dictionary<string, string>();
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.Employmt>()
                {
                    BulkRecordsRead = new Collection<Employmt>() { records[0], records[1] },
                    InvalidRecords = invalidRecords,
                    InvalidKeys = new string[] { }
                };
                dataAccessorMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.Employmt > ("EMPLOYMT", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);
                
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        var externalEmployment = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                        result.Add(gl.Guid, externalEmployment == null ? null : new GuidLookupResult() { Entity = "EMPLOYMT", PrimaryKey = externalEmployment.Recordkey });
                    }
                    return Task.FromResult(result);
                });

                // Set up transaction manager for mocking 
                var transManagerMock = new Mock<IColleagueTransactionInvoker>();

                string[] requestedIds1 = { "1", "2" };

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 100,
                    CacheName = "AllExternalEmploymentsRecordKeys",
                    Entity = "EMPLOYMT",
                    Sublist = requestedIds1.ToList(),
                    TotalCount = 2,
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
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var transManager = transManagerMock.Object;
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);

                // Build  repository
                externalEmploymentRepository = new ExternalEmploymentsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return externalEmploymentRepository;
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
