using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using System.Collections.ObjectModel;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Data.Base.DataContracts;
using System.Threading;

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
                        id = "1",
                        guid = "3632ece0-8b9e-495f-a697-b5c9e053aad5",
                        advisor = "ad1",
                        advisorType = "Type1",
                        startOn = new DateTime(2001, 10,15),
                        program = "ProgCode1",
                        student = "stu1"
                    },
                    new StudentAdvisorRelationship() {
                        id = "2",
                        guid = "176d35fb-5f7a-4c06-b3ae-65a7662c8b43",
                        advisor = "ad2",
                        startOn = new DateTime(2001, 09,01),
                        endOn = new DateTime(2004, 05,15),
                        student = "stu2"
                    },
                    new StudentAdvisorRelationship() {
                        id = "3",
                        guid = "635a3ad5-59ab-47ca-af87-8538c2ad727f",
                        advisor = "ad3",
                        advisorType = "Type1",
                        startOn = new DateTime(2009, 07,17),
                        program = "ProgCode1",
                        student = "stu3"
                    },
                };

            
            studentAdvisementDataContractList = new Collection<StudentAdvisement>();            
            studentAdvisementDataContractListofOne = new Collection<StudentAdvisement>();
                        
            foreach (var sar in studentAdvisorRelationshipsCollection)
            {
                var sa = new StudentAdvisement()
                {
                    RecordGuid = sar.guid,
                    Recordkey = sar.id,
                    StadFaculty = sar.advisor,
                    StadStudent = sar.student,
                    StadType = sar.advisorType,
                    StadAcadProgram = sar.program,
                    StadEndDate = sar.endOn,
                    StadStartDate = sar.startOn
                };
                studentAdvisementDataContractList.Add(sa);
                if (sar.id == "1")
                {
                    studentAdvisementDataContractListofOne.Add(sa);
                }

                PersonSt personSt = new PersonSt() { Recordkey = sar.student, PstAdvisement = new List<string>() { sar.id } };
                dataAccessorMock.Setup(x => x.ReadRecordAsync<PersonSt>(sar.student, true)).ReturnsAsync(personSt);
                DataContracts.Faculty faculty = new DataContracts.Faculty() { Recordkey = sar.advisor, FacAdvisees = new List<string>() { sar.id } };
                dataAccessorMock.Setup(x => x.ReadRecordAsync<DataContracts.Faculty> (sar.advisor, true)).ReturnsAsync(faculty);
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

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsAsync(0, 100, true);

            Assert.IsNotNull(results);
            Assert.AreEqual(3, results.Item2);
            Assert.AreEqual(3, results.Item1.Count());

            foreach(var actual in results.Item1)
            {
                var expected = studentAdvisorRelationshipsCollection.FirstOrDefault(x => x.id == actual.id);

                Assert.AreEqual(expected.id, actual.id);
                Assert.AreEqual(expected.guid, actual.guid);
                Assert.AreEqual(expected.advisor, actual.advisor);
                Assert.AreEqual(expected.student, actual.student);
                Assert.AreEqual(expected.advisorType, actual.advisorType);
                Assert.AreEqual(expected.program, actual.program);
                Assert.AreEqual(expected.startOn, actual.startOn);
                Assert.AreEqual(expected.endOn, actual.endOn);
            }
        }

        [TestMethod]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsAsync_filters()
        {

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsAsync(0, 100, true,
                "stu1","ad1", "Type1");

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Item2);
            Assert.AreEqual(1, results.Item1.Count());

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
            
            var expected = studentAdvisorRelationshipsCollection.FirstOrDefault(x => x.id == actual.id);

            Assert.AreEqual(expected.id, actual.id);
            Assert.AreEqual(expected.guid, actual.guid);
            Assert.AreEqual(expected.advisor, actual.advisor);
            Assert.AreEqual(expected.student, actual.student);
            Assert.AreEqual(expected.advisorType, actual.advisorType);
            Assert.AreEqual(expected.program, actual.program);
            Assert.AreEqual(expected.startOn, actual.startOn);
            Assert.AreEqual(expected.endOn, actual.endOn);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAdvisorRelationshipsRepo_GetStudentAdvisorRelationshipsAsync_NotFound()
        {
            string[] studentAdvismentIds = {  };
            studentAdvisementDataContractList = null;
            dataAccessorMock.Setup(x => x.SelectAsync("STUDENT.ADVISEMENT", 
                "WITH STAD.STUDENT NE '' AND STAD.FACULTY NE '' AND STAD.START.DATE NE ''")).ReturnsAsync(studentAdvismentIds);
            dataAccessorMock.Setup(x => x.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", studentAdvismentIds, true)).ReturnsAsync(studentAdvisementDataContractList);

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsAsync(0, 100, true);
            
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
        [ExpectedException(typeof(RepositoryException))]
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

            dataAccessorMock.Setup(x => x.ReadRecordAsync<StudentAdvisement>("1", true)).ReturnsAsync(null);

            var results = await studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync("1");
        }



    }
}
