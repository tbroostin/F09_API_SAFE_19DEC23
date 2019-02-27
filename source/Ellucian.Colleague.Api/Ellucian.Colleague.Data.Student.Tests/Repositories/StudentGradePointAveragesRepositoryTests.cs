//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
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
    public class StudentGradePointAveragesRepositoryTests
    {
        #region SETUP
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataReaderMock;
        Mock<ILogger> loggerMock;

        string valcodeName;
        ApiSettings apiSettings;

        StudentGradePointAveragesRepository studentGradePointAveragesRepo;
        IEnumerable<StudentAcademicCredit> allStudentAcademicCredits;

        public TestStudentGradePointAveragesRepository testDataRepository;

        public async Task<IEnumerable<StudentAcademicCredit>> getExpectedStudentGradePointAverages()
        {
            return await testDataRepository.GetStudentGradePointAveragesAsync();
        }

        public async Task<IEnumerable<StudentAcademicCredit>> getActualStudentGradePointAverages()
        {
            Tuple<IEnumerable<StudentAcademicCredit>, int> tuple = await studentGradePointAveragesRepo.GetStudentGpasAsync(0, 20, new TestStudentGradePointAveragesRepository().GetStudentGradePointAveragesAsync().Result.FirstOrDefault(), "gradeDate");

            if(tuple != null)
            {
                return tuple.Item1;
            }
            else
            {
                return new List<StudentAcademicCredit>();
            }
        }

        [TestInitialize]
        public void MockInitialize()
        {
            loggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");

            allStudentAcademicCredits = new TestStudentGradePointAveragesRepository().GetStudentGradePointAveragesAsync().Result;
            
            testDataRepository = new TestStudentGradePointAveragesRepository();

            studentGradePointAveragesRepo = BuildValidStudentGradePointAveragesRepository();
            valcodeName = studentGradePointAveragesRepo.BuildFullCacheKey("AllStudentGradePointAverages");
        }

        [TestCleanup]
        public void Cleanup()
        {
            allStudentAcademicCredits = null;
            valcodeName = string.Empty;
            apiSettings = null;
        }
        #endregion

        #region MOCK EVENTS
        private Student.Repositories.StudentGradePointAveragesRepository BuildValidStudentGradePointAveragesRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            // Set up data accessor for mocking 
            dataReaderMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

            var recordStudentAcadCreds = new Collection<DataContracts.StudentAcadCred>();
            var recordPersonSt = new Collection<Data.Base.DataContracts.PersonSt>();

            foreach (var item in allStudentAcademicCredits)
            {
                DataContracts.StudentAcadCred studentAcadCred = new DataContracts.StudentAcadCred();
                Data.Base.DataContracts.PersonSt personStDataContract = new Data.Base.DataContracts.PersonSt();

                studentAcadCred.RecordGuid = item.PersonSTGuid;
                studentAcadCred.Recordkey = item.StudentId;
                studentAcadCred.StcMarkAcadCredentials = new List<string>() { "1", "2" };

                personStDataContract.RecordGuid = item.PersonSTGuid;
                personStDataContract.Recordkey = item.StudentId;
                personStDataContract.PstStudentAcadCred = new List<string>() { "1", "2", "3" };


                recordStudentAcadCreds.Add(studentAcadCred);
                recordPersonSt.Add(personStDataContract);
            }
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(recordStudentAcadCreds);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.PersonSt>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(recordPersonSt);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.PersonSt>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(recordPersonSt);
            
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(recordStudentAcadCreds.FirstOrDefault());
            dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2", "3" });
            dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2", "3" });

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
            {
                var result = new Dictionary<string, RecordKeyLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var record = allStudentAcademicCredits.Where(e => e.StudentId == recordKeyLookup.PrimaryKey).FirstOrDefault();
                    result.Add(string.Join("+", new string[] { "STUDENT.ACAD.CRED", record.StudentId }),
                        new RecordKeyLookupResult() { Guid = record.PersonSTGuid });
                }
                return Task.FromResult(result);
            });

            // Construct repository
            studentGradePointAveragesRepo = new StudentGradePointAveragesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return studentGradePointAveragesRepo;
        }


        #endregion

        #region GetStudentGradePointAveragesTests
        [TestClass]
        public class GetStudentGradePointAveragesTests : StudentGradePointAveragesRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
            }

            //#region TESTS FOR FUNCTIONALITY
            [TestMethod]
            public async Task ExpectedEqualsActualStudentGradePointAveragesTest()
            {
                var expected = await getExpectedStudentGradePointAverages();
                var actual = await getActualStudentGradePointAverages();
                Assert.AreEqual(expected.Count(), actual.Count());
            }

            [TestMethod]
            public async Task StudentGradePointAverageRepository_GetStudentGradePointAveragesAsync_False()
            {
                var results = await studentGradePointAveragesRepo.GetStudentGpasAsync(0, 20, allStudentAcademicCredits.FirstOrDefault(), "gradeDate");
                Assert.AreEqual(allStudentAcademicCredits.Count(), results.Item2);

                foreach (var studentAcademicCredit in allStudentAcademicCredits)
                {
                    var result = results.Item1.FirstOrDefault(i => i.PersonSTGuid == studentAcademicCredit.PersonSTGuid);

                    Assert.AreEqual(studentAcademicCredit.StudentId, result.StudentId);
                    Assert.AreEqual(studentAcademicCredit.PersonSTGuid, result.PersonSTGuid);
                }

            }
            //#endregion
        }
        #endregion

    }
}