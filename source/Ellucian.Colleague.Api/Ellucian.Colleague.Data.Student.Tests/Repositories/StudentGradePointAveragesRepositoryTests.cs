//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
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
        Mock<IColleagueTransactionInvoker> transManagerMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataReaderMock;
        Mock<ILogger> loggerMock;

        string valcodeName;
        ApiSettings apiSettings;

        StudentGradePointAveragesRepository studentGradePointAveragesRepo;
        IEnumerable<StudentAcademicCredit> allStudentAcademicCredits;
        Collection<StudentPrograms> studentPrograms;
        Collection<AcadCredentials> acadCredentials;

        public TestStudentGradePointAveragesRepository testDataRepository;

        public async Task<IEnumerable<StudentAcademicCredit>> GetExpectedStudentGradePointAverages()
        {
            return await testDataRepository.GetStudentGradePointAveragesAsync();
        }

        public async Task<IEnumerable<StudentAcademicCredit>> GetActualStudentGradePointAverages()
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
            transManagerMock = new Mock<IColleagueTransactionInvoker>();
            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            // Set up data accessor for mocking 
            dataReaderMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

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
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<PersonSt>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(recordPersonSt.FirstOrDefault());
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

            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllStudentGPAs:",
                Entity = "PERSON.ST",
                Sublist = new List<string>() { "1", "2", "3" },
                TotalCount = 3,
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

            //Acad credentials
            BulkReadOutput<AcadCredentials> bulkReadOutPut = new BulkReadOutput<AcadCredentials>();
            acadCredentials = new Collection<AcadCredentials>()
            {
                new AcadCredentials()
                {
                    AcadPersonId = "1",
                    AcadAcadProgram = "abc",
                    AcadCcdDate = new List<DateTime?>()
                    {
                        DateTime.Now.AddDays(-5),
                        DateTime.Now.AddDays(-10)
                    },
                    AcadDegreeDate = DateTime.Now.AddDays(-1),
                    Recordkey = "1",
                    RecordGuid = "186048e0-8b83-422b-a3f6-b3cbd2066ce4"
                },
                new AcadCredentials()
                {
                    AcadPersonId = "2",
                    AcadAcadProgram = "abc",
                    AcadCcdDate = new List<DateTime?>()
                    {
                        DateTime.Now.AddDays(-5),
                        DateTime.Now.AddDays(-10)
                    },
                    AcadDegreeDate = DateTime.Now.AddDays(-1),
                    Recordkey = "2",
                    RecordGuid = "286048e0-8b83-422b-a3f6-b3cbd2066ce4"
                }
            };
            bulkReadOutPut.BulkRecordsRead = acadCredentials;
            dataReaderMock.Setup( repo => repo.BulkReadRecordWithInvalidKeysAsync<AcadCredentials>( It.IsAny<string[]>(), It.IsAny<bool>() ) ).ReturnsAsync( bulkReadOutPut );

            //Student Programs
            studentPrograms = new Collection<StudentPrograms>()
            {
                new StudentPrograms()
                {
                    Recordkey = "1*abc",
                    RecordGuid = "e425346b-f52c-446c-9ad2-451fe3c6612d"
                },
                new StudentPrograms()
                {
                    Recordkey = "2*abc",
                    RecordGuid = "67dadd7b-36c3-41a4-935a-ae7277b08fcb"
                }
            };
            dataReaderMock.Setup( repo => repo.BulkReadRecordAsync<StudentPrograms>( It.IsAny<string[]>(), It.IsAny<bool>() ) ).ReturnsAsync( studentPrograms );


            // Construct repository
            studentGradePointAveragesRepo = new StudentGradePointAveragesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return studentGradePointAveragesRepo;
        }


        #endregion

        #region GetStudentGradePointAveragesTests
        [TestClass]
        public class GetStudentGradePointAveragesTests : StudentGradePointAveragesRepositoryTests
        {
            //#region TESTS FOR FUNCTIONALITY
            [TestMethod]
            public async Task GetStudentCredProgramInfoAsync_Null_Input_Params()
            {
                var result = await studentGradePointAveragesRepo.GetStudentCredProgramInfoAsync( new List<string>() { } );
                Assert.IsNull( result );
            }

            [TestMethod]
            public async Task GetStudentCredProgramInfoAsync()
            {
                var result = await studentGradePointAveragesRepo.GetStudentCredProgramInfoAsync( new List<string>() { "1", "2" } );
                Assert.IsNotNull( result );
            }

            [TestMethod]
            public async Task GetStudentCredProgramInfoAsync_Null_StProgId_NoMatch()
            {
                studentPrograms = new Collection<StudentPrograms>()
                {
                    new StudentPrograms()
                    {
                        Recordkey = "1*abc",
                        RecordGuid = "e425346b-f52c-446c-9ad2-451fe3c6612d"
                    },
                    new StudentPrograms()
                    {
                        Recordkey = "3*abc",
                        RecordGuid = "67dadd7b-36c3-41a4-935a-ae7277b08fcb"
                    }
                };
                dataReaderMock.Setup( repo => repo.BulkReadRecordAsync<StudentPrograms>( It.IsAny<string[]>(), It.IsAny<bool>() ) ).ReturnsAsync( studentPrograms );
                var result = await studentGradePointAveragesRepo.GetStudentCredProgramInfoAsync( new List<string>() { "1", "2" } );
                Assert.AreEqual( 1, result.Count() );
            }

            [TestMethod]
            public async Task GetStudentCredProgramInfoAsync_Null_AcadAcadProg()
            {
                acadCredentials = new Collection<AcadCredentials>()
                {
                    new AcadCredentials()
                    {
                        AcadPersonId = "1",
                        AcadAcadProgram = "abc",
                        AcadCcdDate = new List<DateTime?>()
                        {
                            DateTime.Now.AddDays(-5),
                            DateTime.Now.AddDays(-10)
                        },
                        AcadDegreeDate = DateTime.Now.AddDays(-1),
                        Recordkey = "1",
                        RecordGuid = "186048e0-8b83-422b-a3f6-b3cbd2066ce4"
                    },
                    new AcadCredentials()
                    {
                        AcadPersonId = "2",
                        AcadAcadProgram = "",
                        AcadCcdDate = new List<DateTime?>()
                        {
                            DateTime.Now.AddDays(-5),
                            DateTime.Now.AddDays(-10)
                        },
                        AcadDegreeDate = DateTime.Now.AddDays(-1),
                        Recordkey = "2",
                        RecordGuid = "286048e0-8b83-422b-a3f6-b3cbd2066ce4"
                    }
                };
                dataReaderMock.Setup( repo => repo.BulkReadRecordAsync<AcadCredentials>( It.IsAny<string[]>(), It.IsAny<bool>() ) ).ReturnsAsync( acadCredentials );
                var result = await studentGradePointAveragesRepo.GetStudentCredProgramInfoAsync( new List<string>() { "1", "2" } );
                Assert.AreEqual( 2, result.Count() );
            }

            [ TestMethod]
            public async Task ExpectedEqualsActualStudentGradePointAveragesTest()
            {
                var expected = await GetExpectedStudentGradePointAverages();
                var actual = await GetActualStudentGradePointAverages();
                Assert.AreEqual(expected.Count(), actual.Count());
            }

            [TestMethod]
            public async Task StudentGradePointAverageRepository_GetStudentGradePointAveragesAsync_False()
            {
                var results = await studentGradePointAveragesRepo.GetStudentGpasAsync(0, 20, allStudentAcademicCredits.FirstOrDefault());
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