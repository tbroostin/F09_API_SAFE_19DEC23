//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
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

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentAdvisorRelationshipsTests : BaseRepositorySetup
    {

        private Mock<ILogger> _loggerMock;
        private StudentAdvisorRelationshipsRepository studentAdvisorRelationshipsRepository;
        private Mock<ICacheProvider> _cacheProvider;
        private Mock<IColleagueTransactionFactory> _transactionFactory;
        private Mock<IColleagueDataReader> dataAccessorMock;

        private ICollection<StudentAdvisorRelationship> studentAdvisorRelationshipsCollection;
        private string[] studentAdvismentIds = { "1", "2", "3" };
        private Collection<StudentAdvisement> studentAdvisementDataContractList;
        private Collection<StudentAdvisement> studentAdvisementDataContractListofOne;
        string criteria = "WITH STAD.STUDENT NE '' AND WITH STAD.FACULTY NE '' AND STAD.START.DATE NE ''";


        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            _loggerMock = new Mock<ILogger>();
            _cacheProvider = new Mock<ICacheProvider>();
            _transactionFactory = new Mock<IColleagueTransactionFactory>();
            dataAccessorMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            _cacheProvider.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


            studentAdvisorRelationshipsCollection = new List<StudentAdvisorRelationship>()
                {
                    new StudentAdvisorRelationship() {
                        Id = "1",
                        Guid = "3632ece0-8b9e-495f-a697-b5c9e053aad5",
                        Advisor = "ad1",
                        AdvisorType = "Type1",
                        StartOn = new DateTime(2001, 10,15),
                        Program = "ProgCode1",
                        Student = "stu1"
                    },
                    new StudentAdvisorRelationship() {
                        Id = "2",
                        Guid = "176d35fb-5f7a-4c06-b3ae-65a7662c8b43",
                        Advisor = "ad2",
                        StartOn = new DateTime(2001, 09,01),
                        EndOn = new DateTime(2004, 05,15),
                        Student = "stu2"
                    },
                    new StudentAdvisorRelationship() {
                        Id = "3",
                        Guid = "635a3ad5-59ab-47ca-af87-8538c2ad727f",
                        Advisor = "ad3",
                        AdvisorType = "Type1",
                        StartOn = new DateTime(2009, 07,17),
                        Program = "ProgCode1",
                        Student = "stu3"
                    },
                };
            dataAccessorMock.Setup(repo => repo.SelectAsync("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2", "3" });
            GetCacheApiKeysRequest request = new GetCacheApiKeysRequest() 
            {
                Criteria = criteria,

            };
            Mock<IColleagueTransactionInvoker> mockManager = new Mock<IColleagueTransactionInvoker>();

            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 2,
                CacheName = "AllStudentAdvisors:",
                Entity = "STUDENT.ADVISEMENT",
                Sublist = new List<string>() { "1", "2", "3" },
                TotalCount = 7,
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
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                .ReturnsAsync(resp);

            studentAdvisementDataContractList = new Collection<StudentAdvisement>();            
            studentAdvisementDataContractListofOne = new Collection<StudentAdvisement>();
                        
            foreach (var sar in studentAdvisorRelationshipsCollection)
            {
                var sa = new StudentAdvisement()
                {
                    RecordGuid = sar.Guid,
                    Recordkey = sar.Id,
                    StadFaculty = sar.Advisor,
                    StadStudent = sar.Student,
                    StadType = sar.AdvisorType,
                    StadAcadProgram = sar.Program,
                    StadEndDate = sar.EndOn,
                    StadStartDate = sar.StartOn
                };
                studentAdvisementDataContractList.Add(sa);
                if (sar.Id == "1")
                {
                    studentAdvisementDataContractListofOne.Add(sa);
                }

                PersonSt personSt = new PersonSt() { Recordkey = sar.Student, PstAdvisement = new List<string>() { sar.Id } };
                dataAccessorMock.Setup(x => x.ReadRecordAsync<PersonSt>(sar.Student, true)).ReturnsAsync(personSt);
                DataContracts.Faculty faculty = new DataContracts.Faculty() { Recordkey = sar.Advisor, FacAdvisees = new List<string>() { sar.Id }, FacAdviseFlag = "Y" };
                dataAccessorMock.Setup(x => x.ReadRecordAsync<DataContracts.Faculty> (sar.Advisor, true)).ReturnsAsync(faculty);
            }

            

            string[] listOfOneKey = { "1" };
            dataAccessorMock.Setup(x => x.BulkReadRecordAsync<StudentAdvisement>(studentAdvismentIds, true)).ReturnsAsync(studentAdvisementDataContractList);
            dataAccessorMock.Setup(x => x.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", "", true)).ReturnsAsync(studentAdvisementDataContractList);
            dataAccessorMock.Setup(x => x.BulkReadRecordAsync<StudentAdvisement>(listOfOneKey, true)).ReturnsAsync(studentAdvisementDataContractListofOne);


            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            studentAdvisorRelationshipsRepository = new StudentAdvisorRelationshipsRepository(_cacheProvider.Object,
                transFactoryMock.Object, _loggerMock.Object);
            var cacheKey = studentAdvisorRelationshipsRepository.BuildFullCacheKey("AllStudentAdvisorRelationships");
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(studentAdvisementDataContractList).Verifiable();


        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerMock = null;
            _cacheProvider = null;
            _transactionFactory = null;
            studentAdvisorRelationshipsRepository = null;
            dataReaderMock = null;
        }

        [TestMethod]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsAsync()
        {
            dataAccessorMock.Setup(x => x.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentAdvisementDataContractList);

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsAsync(0, 100, true);

            Assert.IsNotNull(results);
            Assert.AreEqual(7, results.Item2);
            Assert.AreEqual(3, results.Item1.Count());

            foreach (var actual in results.Item1)
            {
                var expected = studentAdvisorRelationshipsCollection.FirstOrDefault(x => x.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Guid, actual.Guid);
                Assert.AreEqual(expected.Advisor, actual.Advisor);
                Assert.AreEqual(expected.Student, actual.Student);
                Assert.AreEqual(expected.AdvisorType, actual.AdvisorType);
                Assert.AreEqual(expected.Program, actual.Program);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
            }
        }

        [TestMethod]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsAsync_filters()
        {
            dataAccessorMock.Setup(x => x.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentAdvisementDataContractList);

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsAsync(0, 100, true,
                "stu1","ad1", "Type1");

            Assert.IsNotNull(results);
            Assert.AreEqual(7, results.Item2);
            Assert.AreEqual(3, results.Item1.Count());

        }

        [TestMethod]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsByGuidAsync()
        {
            var guid = new GuidLookupResult()
            {
                 Entity = "STUDENT.ADVISEMENT",
                  PrimaryKey = "1",
                   SecondaryKey = ""
            };
            Dictionary<string, GuidLookupResult> guidLookup = new Dictionary<string, GuidLookupResult>();
            guidLookup.Add("1", guid);

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                return Task.FromResult(guidLookup);
            });

            var saDataContract = studentAdvisementDataContractList.FirstOrDefault();
            
            dataAccessorMock.Setup(x => x.ReadRecordAsync<StudentAdvisement>("1",true)).ReturnsAsync(saDataContract);

            var actual = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync("1");
            
            var expected = studentAdvisorRelationshipsCollection.FirstOrDefault(x => x.Id == actual.Id);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Guid, actual.Guid);
            Assert.AreEqual(expected.Advisor, actual.Advisor);
            Assert.AreEqual(expected.Student, actual.Student);
            Assert.AreEqual(expected.AdvisorType, actual.AdvisorType);
            Assert.AreEqual(expected.Program, actual.Program);
            Assert.AreEqual(expected.StartOn, actual.StartOn);
            Assert.AreEqual(expected.EndOn, actual.EndOn);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsByGuidAsync_Null()
        {
            
            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync(null);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsByGuidAsync_NotFound()
        {
            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsByGuidAsync_ValueNotFound()
        {
            GuidLookupResult guid = null;
            Dictionary<string, GuidLookupResult> guidLookup = new Dictionary<string, GuidLookupResult>();
            guidLookup.Add("1", guid);

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                return Task.FromResult(guidLookup);
            });

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsByGuidAsync_NotRightEntity()
        {
            var guid = new GuidLookupResult()
            {
                Entity = "WrongEntity",
                PrimaryKey = "1",
                SecondaryKey = ""
            };
            Dictionary<string, GuidLookupResult> guidLookup = new Dictionary<string, GuidLookupResult>();
            guidLookup.Add("1", guid);

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                return Task.FromResult(guidLookup);
            });

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsByGuidAsync_MissingIDEntity()
        {
            var guid = new GuidLookupResult()
            {
                Entity = "STUDENT.ADVISEMENT",
                PrimaryKey = "",
                SecondaryKey = ""
            };
            Dictionary<string, GuidLookupResult> guidLookup = new Dictionary<string, GuidLookupResult>();
            guidLookup.Add("1", guid);

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                return Task.FromResult(guidLookup);
            });

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsByGuidAsync_ReadRecordFailing()
        {
            var guid = new GuidLookupResult()
            {
                Entity = "STUDENT.ADVISEMENT",
                PrimaryKey = "1",
                SecondaryKey = ""
            };
            Dictionary<string, GuidLookupResult> guidLookup = new Dictionary<string, GuidLookupResult>();
            guidLookup.Add("1", guid);

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                return Task.FromResult(guidLookup);
            });

            dataAccessorMock.Setup(x => x.ReadRecordAsync<StudentAdvisement>("1", true)).ReturnsAsync(() => null);

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync("1");
        }



    }
}
