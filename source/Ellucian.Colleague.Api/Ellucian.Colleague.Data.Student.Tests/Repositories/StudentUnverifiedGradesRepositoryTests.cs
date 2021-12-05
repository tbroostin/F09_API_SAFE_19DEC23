//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
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

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentUnverifiedGradeRepositoryTests : BaseRepositorySetup
    {
        #region SETUP
        //Mock<IColleagueTransactionFactory> transFactoryMock;
        //Mock<IColleagueTransactionInvoker> transManagerMock;
        //Mock<ICacheProvider> cacheProviderMock;
        //Mock<IColleagueDataReader> dataReaderMock;
        //Mock<ILogger> loggerMock;
        Collection<StudentAcadCred> recordStudentAcadCreds;
        Collection<DataContracts.StudentCourseSec> recordStudentCourseSecs;

        string valcodeName;
        //ApiSettings apiSettings;
        public const string StudentUnverifiedGradesGuid = "bb66b971-3ee0-4477-9bb7-539721f93434";

        StudentUnverifiedGradesRepository studentUnverifiedGradesRepo;
        IEnumerable<StudentUnverifiedGrades> allStudentUnverifiedGrades;

        public TestStudentUnverifiedGradesRepository testDataRepository;

        public async Task<IEnumerable<StudentUnverifiedGrades>> getExpectedStudentUnverifiedGrades()
        {
            return await testDataRepository.GetStudentUnverifiedGradesAsync();
        }

        public async Task<IEnumerable<StudentUnverifiedGrades>> getActualStudentUnverifiedGrades()
        {
            Tuple<IEnumerable<StudentUnverifiedGrades>, int> tuple = await studentUnverifiedGradesRepo.GetStudentUnverifiedGradesAsync(0, 20, false);

            if(tuple != null)
            {
                return tuple.Item1;
            }
            else
            {
                return new List<StudentUnverifiedGrades>();
            }
        }

        [TestInitialize]
        public void MockInitialize()
        {
            loggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");

            allStudentUnverifiedGrades = new TestStudentUnverifiedGradesRepository().GetStudentUnverifiedGradesAsync().Result;
            
            testDataRepository = new TestStudentUnverifiedGradesRepository();

            studentUnverifiedGradesRepo = BuildValidStudentUnverifiedGradesRepository();
            valcodeName = studentUnverifiedGradesRepo.BuildFullCacheKey("AllStudentUnverifiedGrades");
        }

        [TestCleanup]
        public void Cleanup()
        {
            allStudentUnverifiedGrades = null;
            valcodeName = string.Empty;
            apiSettings = null;
        }
        #endregion

        #region MOCK EVENTS
        private Student.Repositories.StudentUnverifiedGradesRepository BuildValidStudentUnverifiedGradesRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            transManagerMock = new Mock<IColleagueTransactionInvoker>();

            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            // Set up data accessor for mocking 
            dataReaderMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

            recordStudentAcadCreds = new Collection<DataContracts.StudentAcadCred>();
            recordStudentCourseSecs = new Collection<DataContracts.StudentCourseSec>();

            foreach (var item in allStudentUnverifiedGrades)
            {
                DataContracts.StudentAcadCred studentAcadCred = new DataContracts.StudentAcadCred();
                StudentCourseSec studentCourseSecDataContract = new StudentCourseSec();

                studentAcadCred.RecordGuid = item.Guid;
                studentAcadCred.Recordkey = item.StudentAcadaCredId;
                studentAcadCred.StcGradeScheme = item.GradeScheme;
                studentAcadCred.StcGradeExpireDate = item.IncompleteGradeExtensionDate;
                studentAcadCred.StcFinalGrade = item.FinalGrade;
                studentAcadCred.StcVerifiedGradeDate = item.FinalGradeDate;

                studentCourseSecDataContract.RecordGuid = item.Guid;
                studentCourseSecDataContract.Recordkey = item.StudentCourseSecId;
                studentCourseSecDataContract.ScsStudent = item.StudentId;
                studentCourseSecDataContract.ScsStudentAcadCred = item.StudentAcadaCredId;
                studentCourseSecDataContract.ScsMidTermGrade1 = item.MidtermGrade1;
                studentCourseSecDataContract.ScsMidTermGrade2 = item.MidtermGrade2;
                studentCourseSecDataContract.ScsMidTermGrade3 = item.MidtermGrade3;
                studentCourseSecDataContract.ScsMidTermGrade4 = item.MidtermGrade4;
                studentCourseSecDataContract.ScsMidTermGrade5 = item.MidtermGrade5;
                studentCourseSecDataContract.ScsMidTermGrade6 = item.MidtermGrade6;
                studentCourseSecDataContract.ScsMidGradeDate1 = item.MidtermGradeDate1;
                studentCourseSecDataContract.ScsMidGradeDate2 = item.MidtermGradeDate2;
                studentCourseSecDataContract.ScsMidGradeDate3 = item.MidtermGradeDate3;
                studentCourseSecDataContract.ScsMidGradeDate4 = item.MidtermGradeDate4;
                studentCourseSecDataContract.ScsMidGradeDate5 = item.MidtermGradeDate5;
                studentCourseSecDataContract.ScsMidGradeDate6 = item.MidtermGradeDate6;
                studentCourseSecDataContract.ScsLastAttendDate = item.LastAttendDate;
                studentCourseSecDataContract.ScsNeverAttendedFlag = "Y";
                
                recordStudentAcadCreds.Add(studentAcadCred);
                recordStudentCourseSecs.Add(studentCourseSecDataContract);
            }
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(recordStudentAcadCreds);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(recordStudentCourseSecs);

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(recordStudentAcadCreds);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(recordStudentCourseSecs);

            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(recordStudentAcadCreds.FirstOrDefault());
            dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2", "3" });

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
            {
                var result = new Dictionary<string, RecordKeyLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var record = allStudentUnverifiedGrades.Where(e => e.StudentCourseSecId == recordKeyLookup.PrimaryKey).FirstOrDefault();
                    result.Add(string.Join("+", new string[] { "STUDENT.COURSE.SEC", record.StudentCourseSecId }),
                        new RecordKeyLookupResult() { Guid = record.Guid });
                }
                return Task.FromResult(result);
            });

            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.COURSE.SEC", LdmGuidPrimaryKey = StudentUnverifiedGradesGuid };
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            
            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllSectionRegistrationsKeys:",
                Entity = "STUDENT.COURSE.SEC",
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

            // Construct repository
            studentUnverifiedGradesRepo = new StudentUnverifiedGradesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return studentUnverifiedGradesRepo;
        }


        #endregion

        #region GetStudentUnverifiedGradesTests
        [TestClass]
        public class GetStudentUnverifiedGradesTests : StudentUnverifiedGradeRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
            }

            #region TESTS FOR FUNCTIONALITY
            [TestMethod]
            public async Task ExpectedEqualsActualStudentUnverifiedGradesTest()
            {
                var expected = await getExpectedStudentUnverifiedGrades();
                var actual = await getActualStudentUnverifiedGrades();
                Assert.AreEqual(expected.Count(), actual.Count());
            }

            [TestMethod]
            public async Task StudentUnverifiedGradeRepository_GetStudentUnverifiedGradesAsync_False()
            {
                var results = await studentUnverifiedGradesRepo.GetStudentUnverifiedGradesAsync(0, 20, false);
                Assert.AreEqual(allStudentUnverifiedGrades.Count(), results.Item2);

                foreach (var studentUnverifiedGrade in allStudentUnverifiedGrades)
                {
                    var result = results.Item1.FirstOrDefault(i => i.Guid == studentUnverifiedGrade.Guid);

                    Assert.AreEqual(studentUnverifiedGrade.StudentCourseSecId, result.StudentCourseSecId);
                    Assert.AreEqual(studentUnverifiedGrade.Guid, result.Guid);
                }

            }
            #endregion

            [TestMethod]
            public async Task StudentUnverifiedGradeRepository_GetGuidsCollectionAsync()
            {
                IEnumerable<string> sublist = new List<string>() { "1", "2" };
                Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                recordKeyLookupResults.Add("STUDENT.ACAD.CRED+1", new RecordKeyLookupResult() { Guid = "854da721-4191-4875-bf58-7d6c00ffea8f", ModelName = "STUDENT.ACAD.CRED" });
                recordKeyLookupResults.Add("STUDENT.ACAD.CRED+2", new RecordKeyLookupResult() { Guid = "71e1a806-24a8-4d93-91a2-02d86056b63c", ModelName = "STUDENT.ACAD.CRED" });
                List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

                dataReaderMock.Setup(i => i.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2", "3", "4" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

                var results = await studentUnverifiedGradesRepo.GetGuidsCollectionAsync(sublist, "STUDENT.ACAD.CRED");
                Assert.IsNotNull(results);
                Assert.AreEqual(2, results.Count());
                foreach (var result in results)
                {
                    RecordKeyLookupResult recordKeyLookupResult = null;
                    recordKeyLookupResults.TryGetValue(string.Concat("STUDENT.ACAD.CRED+", result.Key), out recordKeyLookupResult);

                    Assert.AreEqual(result.Value, recordKeyLookupResult.Guid);
                }
            }


            [TestMethod]
            public async Task StudentUnverifiedGradeRepository_UpdateStudentUnverifiedGradesSubmissionsAsync()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);

                Assert.AreEqual(studentUnverifiedGrade.Guid, result.Guid); //"bb66b971-3ee0-4477-9bb7-539721f93434"
                Assert.AreEqual(studentUnverifiedGrade.StudentAcadaCredId, result.StudentAcadaCredId); //studentAcadCredId1
                Assert.AreEqual(studentUnverifiedGrade.StudentId, result.StudentId); //"stud1"
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentUnverifiedGradeRepository_Update_FinalGradeNotFound()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                studentUnverifiedGrade.FinalGrade = "A";

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                studentAcadCred.StcFinalGrade = "A";
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentUnverifiedGradeRepository_Update_MidTerm1NotFound()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                studentUnverifiedGrade.MidtermGrade1 = "A";

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                studentCourseSec.ScsMidTermGrade1 = "A";
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentUnverifiedGradeRepository_Update_MidTerm2NotFound()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                studentUnverifiedGrade.MidtermGrade2 = "A";

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                studentCourseSec.ScsMidTermGrade2 = "A";
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentUnverifiedGradeRepository_Update_MidTerm3NotFound()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                studentUnverifiedGrade.MidtermGrade3 = "A";

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                studentCourseSec.ScsMidTermGrade3 = "A";
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentUnverifiedGradeRepository_Update_MidTerm4NotFound()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                studentUnverifiedGrade.MidtermGrade4 = "A";

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                studentCourseSec.ScsMidTermGrade4 = "A";
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentUnverifiedGradeRepository_Update_MidTerm5NotFound()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                studentUnverifiedGrade.MidtermGrade5 = "A";

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                studentCourseSec.ScsMidTermGrade5 = "A";
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentUnverifiedGradeRepository_Update_MidTerm6NotFound()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);
                studentUnverifiedGrade.MidtermGrade6 = "A";

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                studentCourseSec.ScsMidTermGrade6 = "A";
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentUnverifiedGradeRepository_UpdateStudentUnverifiedGradesSubmissionsAsync_CTXError()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                // StudentAcadCred studentAcadCredDataContract = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response()
                {
                    Guid = StudentUnverifiedGradesGuid,
                    GradeMessages2 =
                    new List<GradeMessages2>() { new GradeMessages2() { ErrorMessge = "ERROR", StatusCode = "FAILURE" } }
                };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentUnverifiedGradeRepository_UpdateStudentUnverifiedGradesSubmissionsAsync_Update_StudentAcadCredEmpty()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                studentCourseSec.ScsStudentAcadCred = string.Empty;
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                // StudentAcadCred studentAcadCredDataContract = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response()
                {
                    Guid = StudentUnverifiedGradesGuid,
                    GradeMessages2 =
                    new List<GradeMessages2>() { new GradeMessages2() { ErrorMessge = "ERROR", StatusCode = "FAILURE" } }
                };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentUnverifiedGradeRepository_Update_StudentCourseSecNotFound()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentUnverifiedGradeRepository_Update_StudentAcadCredNotFound()
            {
                var studentUnverifiedGrade = allStudentUnverifiedGrades.FirstOrDefault(x => x.Guid == StudentUnverifiedGradesGuid);

                var studentCourseSec = recordStudentCourseSecs.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentCourseSecId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);

                var studentAcadCred = recordStudentAcadCreds.FirstOrDefault(scs => scs.Recordkey == studentUnverifiedGrade.StudentAcadaCredId);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var updateResponse = new ImportGrades2Response() { Guid = StudentUnverifiedGradesGuid };
                transManagerMock.Setup(i => i.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(It.IsAny<ImportGrades2Request>())).ReturnsAsync(updateResponse);
                var result = await studentUnverifiedGradesRepo.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGrade);
            }
        }
        #endregion

    }
}
 