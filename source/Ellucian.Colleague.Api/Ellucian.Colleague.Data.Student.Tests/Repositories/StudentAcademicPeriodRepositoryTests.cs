// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Moq;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Domain.Base.Transactions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentAcademicPeriodRepositoryTests : BasePersonSetup
    {
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepo;
        private IStudentAcademicPeriodRepository studentAcademicPeriodRepo;
       
        private string knownStudentId1;
        private string knownStudentId2;
        private string knownStudentId3;
        private IEnumerable<string> ids;

        private string knownStudentTermsId1;
        private string knownStudentTermsId2;
        private string knownStudentTermsId3;
        private string knownStudentTermsId4;
        private string knownStudentTermsId5;
        private IEnumerable<string> studentTermIds;

        private Collection<DataContracts.Students> students;
        private Collection<DataContracts.StudentTerms> studentTerms;
        private List<StudentTermsSttrStatuses> statuses;
        private Dictionary<string, string> studentAcadPeriods;


        [TestInitialize]
        public async void Initialize()
        {
            // Setup mock repositories
            base.MockInitialize();
            studentRepoMock = new Mock<IStudentRepository>();
            studentRepo = studentRepoMock.Object;
            studentAcademicPeriodRepo = BuildValidStudentAcademicPeriodsRepository();
            apiSettings = new ApiSettings("TEST");
            // Mock student data
            students = new Collection<DataContracts.Students>();
            knownStudentId1 = "0000001";
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId1, StuAcadLevels = new List<string> { "UG" }, StuTerms = new List<string> { "2015/FA" } });
            knownStudentId2 = "0000002";
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId2, StuAcadLevels = new List<string> { "UG" }, StuTerms = new List<string> { "2015/SP", "2015/FA" } });
            knownStudentId3 = "0000003";
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId3, StuAcadLevels = new List<string> { "UG", "GR" }, StuTerms = new List<string> { "2015/FA", "2016/SP" } });

            statuses = new List<StudentTermsSttrStatuses>()
                {
                    new StudentTermsSttrStatuses("A", new DateTime(2016, 05, 01)),
                    new StudentTermsSttrStatuses("I", new DateTime(2016, 06, 01)),
                    new StudentTermsSttrStatuses("F", new DateTime(2016, 07, 01))
                };

            //Mock student terms data
            studentTerms = new Collection<DataContracts.StudentTerms>();
            knownStudentTermsId1 = "0000001*2015/FA*UG";
            studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId1, SttrStudentLoad = "F", RecordGuid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156", SttrStatusesEntityAssociation = statuses });
            knownStudentTermsId2 = "0000002*2015/SP*UG";
            studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId2, SttrStudentLoad = "F", RecordGuid = "0279fd3d-8fbf-4d2b-92b5-1e06ea112dc4", SttrStatusesEntityAssociation = statuses });
            knownStudentTermsId3 = "0000002*2015/FA*UG";
            studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId3, SttrStudentLoad = "P", RecordGuid = "6e2e3834-1c3f-415b-9554-2722ff4dc84a", SttrStatusesEntityAssociation = statuses });
            knownStudentTermsId4 = "0000003*2015/FA*UG";
            studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId4, SttrStudentLoad = "L", RecordGuid = "2051e603-0bd1-4e54-9848-8386130eb4ba", SttrStatusesEntityAssociation = statuses });
            knownStudentTermsId5 = "0000003*2015/FA*GR";
            studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId5, SttrStudentLoad = "P", RecordGuid = "ecdba581-534b-4ca5-8d33-1ea629e56ad8", SttrStatusesEntityAssociation = statuses });

            // mock data accessor STUDENTS response 
            ids = new List<string>() { knownStudentId1, knownStudentId2, knownStudentId3 };
            dataReaderMock.Setup(a => a.BulkReadRecordAsync<Students>(It.IsAny<string[]>(), true)).ReturnsAsync(students);
            dataReaderMock.Setup(a => a.BulkReadRecordAsync<Students>(ids.ToArray(), true)).ReturnsAsync(students);

            // mock data accessor STUDENT.TERMS response
            studentTermIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3, knownStudentTermsId4, knownStudentTermsId5 };

            // Read for all student terms
            dataReaderMock.Setup(a => a.BulkReadRecordAsync<StudentTerms>(studentTermIds.ToArray(), true)).ReturnsAsync(studentTerms);
       
            dataReaderMock.Setup(a => a.SelectAsync("STUDENT.TERMS", It.IsAny<string[]>(), It.IsAny<string>()))
                .ReturnsAsync(studentTerms.Select(x => x.Recordkey).ToArray());

            dataReaderMock.Setup(a => a.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(studentTerms); ;

            IEnumerable<string> sublist = new List<string>() { "1", "2" };
            var recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();

            studentAcadPeriods = new Dictionary<string, string>();
            studentAcadPeriods.Add("STUDENTS+0000001+2015/FA", Guid.NewGuid().ToString());
            studentAcadPeriods.Add("STUDENTS+0000002+2015/SP", Guid.NewGuid().ToString());
            studentAcadPeriods.Add("STUDENTS+0000002+2015/FA", Guid.NewGuid().ToString());
            studentAcadPeriods.Add("STUDENTS+0000003+2015/FA", Guid.NewGuid().ToString());

            foreach (var entry in studentAcadPeriods)
            {
                recordKeyLookupResults.Add(entry.Key, new RecordKeyLookupResult() { Guid = entry.Value, ModelName = "STUDENTS" });
            }

            dataReaderMock.Setup(i => i.SelectAsync("STUDENTS", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2", "3", "4" });
            dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

            foreach (var student in students)
            {
                dataReaderMock.Setup(i => i.ReadRecordAsync<Students>("STUDENTS", student.Recordkey, It.IsAny<bool>())).ReturnsAsync(student);
            }
        }

        private IStudentAcademicPeriodRepository BuildValidStudentAcademicPeriodsRepository()
        {
            // Set up data accessor for mocking 
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

            // Construct repository
            return new StudentAcademicPeriodsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRepo = null;
            studentAcademicPeriodRepo = null;
            apiSettings = null;
            studentRepoMock = null;

            students = null;
            studentTerms = null;
            statuses = null;
            studentAcadPeriods = null;
        }

        [TestMethod]
        public async Task StudentAcademicPeriodsRepository_GetStudentTermsAsync()
        {
            IEnumerable<string> studentIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3 };
            var studentTermsFiltered = studentTerms.Where(i => studentIds.Contains(i.Recordkey));

            var actuals = await studentAcademicPeriodRepo.GetStudentAcademicPeriodsAsync(0, 3, false);
            Assert.IsNotNull(actuals);

            foreach (var actual in actuals.Item1)
            {
                var studentAcadPeriodGuid = string.Empty;
                studentAcadPeriods.TryGetValue(string.Concat("STUDENTS+", actual.StudentId, "+", actual.Term), out studentAcadPeriodGuid);
                Assert.IsNotNull(studentAcadPeriodGuid);
                Assert.AreEqual(studentAcadPeriodGuid, actual.Guid);
                var expectedStudentTerms = studentTermsFiltered.Where(i => i.Recordkey.Split('*')[0].Equals(actual.StudentId)
                         && i.Recordkey.Split('*')[1].Equals(actual.Term));
                foreach (var studentTerm in actual.StudentTerms)
                {
                    var found = expectedStudentTerms.FirstOrDefault(x => x.Recordkey.Split('*')[2].Equals(studentTerm.AcademicLevel));
                    Assert.IsNotNull(found);
                }
            }
        }

        [TestMethod]
        public async Task StudentAcademicPeriodsRepository_GetStudentTermByIdAsync()
        {
            IEnumerable<string> studentIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3 };
            var studentTermsFiltered = studentTerms.Where(i => studentIds.Contains(i.Recordkey));

            var studentAcadPeriodDict = studentAcadPeriods.FirstOrDefault();
            string guid = studentAcadPeriodDict.Value;

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add(studentAcadPeriodDict.Key,
                new RecordKeyLookupResult() { Guid = guid });

            var guidLookupDict = new Dictionary<string, GuidLookupResult>();
            var splitKey = studentAcadPeriodDict.Key.Split('+');
            guidLookupDict.Add(studentAcadPeriodDict.Key,
                new GuidLookupResult() { Entity = "STUDENTS", PrimaryKey = splitKey[1], SecondaryKey = splitKey[2] });

            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);


            dataReaderMock.Setup(a => a.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<StudentTerms>(studentTermsFiltered.ToList()));

            var actual = await studentAcademicPeriodRepo.GetStudentAcademicPeriodByGuidAsync(guid);
            Assert.IsNotNull(actual);

            var studentAcadPeriodGuid = string.Empty;
            studentAcadPeriods.TryGetValue(string.Concat("STUDENTS+", actual.StudentId, "+", actual.Term), out studentAcadPeriodGuid);
            Assert.IsNotNull(studentAcadPeriodGuid);
            Assert.AreEqual(studentAcadPeriodGuid, actual.Guid);

            var expectedStudentTerms = studentTermsFiltered.Where(i => i.Recordkey.Split('*')[0].Equals(actual.StudentId)
                     && i.Recordkey.Split('*')[1].Equals(actual.Term));
            foreach (var studentTerm in actual.StudentTerms)
            {
                var found = expectedStudentTerms.FirstOrDefault(x => x.Recordkey.Split('*')[2].Equals(studentTerm.AcademicLevel));
                Assert.IsNotNull(found);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentAcademicPeriodsRepository_GetCacheApiKeysRequest_StudentTermsKeysNotFound()
        {
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                .Throws(new RepositoryException());

            IEnumerable<string> studentIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3 };
            var studentTermsFiltered = studentTerms.Where(i => studentIds.Contains(i.Recordkey));
            await studentAcademicPeriodRepo.GetStudentAcademicPeriodsAsync(0, 3, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentAcademicPeriodsRepository_GetStudentTermsAsync_StudentTermsNotFound()
        {
            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllStudentAcademicPeriods",
                Entity = "STUDENT.TERMS",
                Sublist = new List<string>() { "1" },
                TotalCount = 1,
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

            dataReaderMock.Setup(a => a.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .Throws(new RepositoryException());
          
            IEnumerable<string> studentIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3 };
            var studentTermsFiltered = studentTerms.Where(i => studentIds.Contains(i.Recordkey));
            await studentAcademicPeriodRepo.GetStudentAcademicPeriodsAsync(0, 3, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentAcademicPeriodsRepository_GetStudentTermByIdAsync_StudentTermsNotFound()
        {
            
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .Throws(new KeyNotFoundException());

            IEnumerable<string> studentIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3 };
            var studentTermsFiltered = studentTerms.Where(i => studentIds.Contains(i.Recordkey));

            var studentAcadPeriodDict = studentAcadPeriods.FirstOrDefault();
            string guid = studentAcadPeriodDict.Value;

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add(studentAcadPeriodDict.Key,
                new RecordKeyLookupResult() { Guid = guid });

            var guidLookupDict = new Dictionary<string, GuidLookupResult>();
            var splitKey = studentAcadPeriodDict.Key.Split('+');
            guidLookupDict.Add(studentAcadPeriodDict.Key,
                new GuidLookupResult() { Entity = "STUDENTS", PrimaryKey = splitKey[1], SecondaryKey = splitKey[2] });

            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);

            await studentAcademicPeriodRepo.GetStudentAcademicPeriodByGuidAsync(guid);
        }


        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentAcademicPeriodsRepository_GetStudentTermByIdAsync_StudentKeysNotFound()
        {
            dataReaderMock.Setup(i => i.ReadRecordAsync<Students>("STUDENTS", It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new KeyNotFoundException());

            IEnumerable<string> studentIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3 };
            var studentTermsFiltered = studentTerms.Where(i => studentIds.Contains(i.Recordkey));

            var studentAcadPeriodDict = studentAcadPeriods.FirstOrDefault();
            string guid = studentAcadPeriodDict.Value;

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add(studentAcadPeriodDict.Key,
                new RecordKeyLookupResult() { Guid = guid });

            var guidLookupDict = new Dictionary<string, GuidLookupResult>();
            var splitKey = studentAcadPeriodDict.Key.Split('+');
            guidLookupDict.Add(studentAcadPeriodDict.Key,
                new GuidLookupResult() { Entity = "STUDENTS", PrimaryKey = splitKey[1], SecondaryKey = splitKey[2] });

            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);


            dataReaderMock.Setup(a => a.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<StudentTerms>(studentTermsFiltered.ToList()));

            await studentAcademicPeriodRepo.GetStudentAcademicPeriodByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAcademicPeriodsRepository_GetStudentTermByIdAsync_FailedGuidLookup()
        {
            IEnumerable<string> studentIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3 };
            var studentTermsFiltered = studentTerms.Where(i => studentIds.Contains(i.Recordkey));

            var studentAcadPeriodDict = studentAcadPeriods.FirstOrDefault();
            string guid = studentAcadPeriodDict.Value;

            dataReaderMock.Setup(a => a.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<StudentTerms>(studentTermsFiltered.ToList()));

            await studentAcademicPeriodRepo.GetStudentAcademicPeriodByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentAcademicPeriodsRepository_GetStudentTermByIdAsync_InvalidEntityLookup()
        {
            IEnumerable<string> studentIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3 };
            var studentTermsFiltered = studentTerms.Where(i => studentIds.Contains(i.Recordkey));

            var studentAcadPeriodDict = studentAcadPeriods.FirstOrDefault();
            string guid = studentAcadPeriodDict.Value;

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add(studentAcadPeriodDict.Key,
                new RecordKeyLookupResult() { Guid = guid });

            var guidLookupDict = new Dictionary<string, GuidLookupResult>();
            var splitKey = studentAcadPeriodDict.Key.Split('+');
            guidLookupDict.Add(studentAcadPeriodDict.Key,
                new GuidLookupResult() { Entity = "STUDENT.TERMS", PrimaryKey = ""});

            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);

            await studentAcademicPeriodRepo.GetStudentAcademicPeriodByGuidAsync(guid);
        }

    }
}