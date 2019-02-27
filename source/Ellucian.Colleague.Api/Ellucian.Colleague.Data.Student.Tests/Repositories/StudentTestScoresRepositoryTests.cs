// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
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
    public class StudentTestScoresRepositoryTests : BaseRepositorySetup
    {
        private Collection<StudentNonCourses> records;
        private List<StudentTestScores> _studentTestScoresEntities;
        private StudentTestScoresRepository _studentAptitudeAssessmentsRepository;
        private Collection<NonCourses> _nonCourseDataContracts;
        private ApplValcodes _nonCourseCats;
        private ApplValcodes _nonCourseStats;
        private ApplValcodes _nonAplicationTestSources;

        private UpdateStudentAptitudeAssessmentRequest request;
        private UpdateStudentAptitudeAssessmentResponse response;
        private Dictionary<string, GuidLookupResult> dicResult;
        private Base.DataContracts.Person person;
        private Base.DataContracts.IntlParams intlParams;

        private string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        //private Mock<IColleagueDataReader> dataReaderMock;

        int offset = 0;
        int limit = 200;

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();
            BuildData();
            _studentAptitudeAssessmentsRepository = BuildStudentTestScoresRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }


        [TestMethod]
        public async Task GetAsync_True()
        {
            var pageOfItems = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresAsync("", offset, limit, true);
            Assert.IsNotNull(pageOfItems);

            var ctr = 0;
            foreach (var result in pageOfItems.Item1)
            {
                var expected = _studentTestScoresEntities.ElementAt(ctr);
                Assert.AreEqual(result.Code, expected.Code);
                Assert.AreEqual(result.DateTaken, expected.DateTaken);
                Assert.AreEqual(result.Description, expected.Description);
                Assert.AreEqual(result.FormName, expected.FormName);
                Assert.AreEqual(result.FormNo, expected.FormNo);
                Assert.AreEqual(result.Guid, expected.Guid);
                Assert.AreEqual(result.Percentile1, expected.Percentile1);
                Assert.AreEqual(result.Percentile2, expected.Percentile2);
                Assert.AreEqual(result.Score, expected.Score);
                Assert.AreEqual(result.Source, expected.Source);
                Assert.AreEqual(result.SpecialFactors, expected.SpecialFactors);
                Assert.AreEqual(result.StatusCode, expected.StatusCode);
                Assert.AreEqual(result.StatusCodeSpProcessing, expected.StatusCodeSpProcessing);
                Assert.AreEqual(result.StatusDate, expected.StatusDate);
                Assert.AreEqual(result.StudentId, expected.StudentId);

                ctr += 1;
            }
        }

        [TestMethod]
        public async Task GetAsync_False()
        {
            var pageOfItems = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresAsync("", offset, limit, false);
            Assert.IsNotNull(pageOfItems);

            var ctr = 0;
            foreach (var result in pageOfItems.Item1)
            {
                var expected = _studentTestScoresEntities.ElementAt(ctr);
                Assert.AreEqual(result.Code, expected.Code);
                Assert.AreEqual(result.DateTaken, expected.DateTaken);
                Assert.AreEqual(result.Description, expected.Description);
                Assert.AreEqual(result.FormName, expected.FormName);
                Assert.AreEqual(result.FormNo, expected.FormNo);
                Assert.AreEqual(result.Guid, expected.Guid);
                Assert.AreEqual(result.Percentile1, expected.Percentile1);
                Assert.AreEqual(result.Percentile2, expected.Percentile2);
                Assert.AreEqual(result.Score, expected.Score);
                Assert.AreEqual(result.Source, expected.Source);
                Assert.AreEqual(result.SpecialFactors, expected.SpecialFactors);
                Assert.AreEqual(result.StatusCode, expected.StatusCode);
                Assert.AreEqual(result.StatusCodeSpProcessing, expected.StatusCodeSpProcessing);
                Assert.AreEqual(result.StatusDate, expected.StatusDate);
                Assert.AreEqual(result.StudentId, expected.StudentId);

                ctr += 1;
            }
        }

        [TestMethod]
        public async Task GetByIdAsync()
        {
            var id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            var expected = _studentTestScoresEntities.FirstOrDefault();
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentNonCourses>(It.IsAny<string>(), true)).ReturnsAsync(records[0]);
            var result = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(id);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Code, expected.Code);
            Assert.AreEqual(result.DateTaken, expected.DateTaken);
            Assert.AreEqual(result.Description, expected.Description);
            Assert.AreEqual(result.FormName, expected.FormName);
            Assert.AreEqual(result.FormNo, expected.FormNo);
            Assert.AreEqual(result.Guid, expected.Guid);
            Assert.AreEqual(result.Percentile1, expected.Percentile1);
            Assert.AreEqual(result.Percentile2, expected.Percentile2);
            Assert.AreEqual(result.Score, expected.Score);
            Assert.AreEqual(result.Source, expected.Source);
            Assert.AreEqual(result.SpecialFactors, expected.SpecialFactors);
            Assert.AreEqual(result.StatusCode, expected.StatusCode);
            Assert.AreEqual(result.StatusCodeSpProcessing, expected.StatusCodeSpProcessing);
            Assert.AreEqual(result.StatusDate, expected.StatusDate);
            Assert.AreEqual(result.StudentId, expected.StudentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetByIdAsync_Null_Argument()
        {
            await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetByIdAsync_KeyNotFoundException()
        {
            var id = "3d390690-7b66-4b66-820e-7610c96c5974";
            await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetByIdAsync_DataContract_NotFound_KeyNotFoundException()
        {
            var id = "3d390690-7b66-4b66-820e-7610c96c5973";
            var key = "1";
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentNonCourses>(key, true)).ReturnsAsync(null);
            await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetByIdAsync_NonCourse_NotFound_ArgumentException()
        {
            var record = records.FirstOrDefault();
            record.StncNonCourse = "X";
            var id = record.RecordGuid;
            var key = record.Recordkey;

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var LkupResult = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var stuNonCourse = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    LkupResult.Add(gl.Guid, stuNonCourse == null ? null : new GuidLookupResult() { Entity = "STUDENT.NON.COURSES", PrimaryKey = key });
                }
                return Task.FromResult(LkupResult);
            });
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<NonCourses>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);

            dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentNonCourses>(key, true)).ReturnsAsync(record);
           await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetByIdAsync_NoCourseCategory_NotFound_ArgumentException()
        {
            var record = records.FirstOrDefault();
            record.StncNonCourse = "4";
            var id = record.RecordGuid;
            var key = record.Recordkey;

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var LkupResult = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var stuNonCourse = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    LkupResult.Add(gl.Guid, stuNonCourse == null ? null : new GuidLookupResult() { Entity = "STUDENT.NON.COURSES", PrimaryKey = key });
                }
                return Task.FromResult(LkupResult);
            });
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<NonCourses>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_nonCourseDataContracts[3]);
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentNonCourses>(key, true)).ReturnsAsync(record);
            await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(id);
        }

        private void BuildData()
        {
            intlParams = new Base.DataContracts.IntlParams()
            {
                HostCountry = "USA",
                HostDateDelimiter = "/",
                HostShortDateFormat = "MDY"
            };

            person = new Base.DataContracts.Person()
            {
                FirstName = "first",
                LastName = "last"
            };

            _studentTestScoresEntities = new List<StudentTestScores>()
            {
                new StudentTestScores("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "0003784", "1", "ACT Test", new DateTime(2017, 12, 11))
                {
                    FormName = "ACT",
                    FormNo = "1",
                    Percentile1 = 79,
                    Percentile2 = 33,
                    Score = 200,
                    Source = "ACT",
                    SpecialFactors = new List<string>() { "A", "D" },
                    StatusCode = "AC",
                    StatusCodeSpProcessing = "2",
                    StatusDate = new DateTime(2017, 12, 11)
                },
                new StudentTestScores("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "0003784", "2", "SAT Test", new DateTime(2017, 12, 11))
                {
                    FormName = "SAT",
                    FormNo = "494",
                    Percentile1 = 79,
                    Score = 1200,
                    Source = "SAT",
                    SpecialFactors = new List<string>() { "A", "D" },
                    StatusCode = "AC",
                    StatusCodeSpProcessing = "2",
                    StatusDate = new DateTime(2017, 12, 11)
                },
                new StudentTestScores("d2253ac7-9931-4560-b42f-1fccd43c952e", "0003784", "3", "ACT Math", new DateTime(2017, 12, 11))
                {
                    FormName = "ACT.M",
                    FormNo = "700",
                    Percentile1 = 79,
                    Score = 1200,
                    Source = "SAT",
                    SpecialFactors = new List<string>() { "A", "D" },
                    StatusCode = "AC",
                    StatusCodeSpProcessing = "2",
                    StatusDate = new DateTime(2017, 12, 11)
                }
            };

            _nonCourseDataContracts = new Collection<NonCourses>()
                {
                   new NonCourses() { RecordGuid = "b9691210-8516-45ca-9cd1-7e5aa1777234", Recordkey = "1", NcrsCategory = "AD" },
                   new NonCourses() { RecordGuid = "7f3aac22-e0b5-4159-b4e2-da158362c41b", Recordkey = "2", NcrsCategory = "PL" },
                   new NonCourses() { RecordGuid = "8f3aac22-e0b5-4159-b4e2-da158362c41b", Recordkey = "3", NcrsCategory = "OT" },
                   new NonCourses() { RecordGuid = "9f3aab77-e0b5-4159-b4e2-cb958362c41b", Recordkey = "4", NcrsCategory = "G" }
                };

            _nonCourseCats = new ApplValcodes()
            {
                Recordkey = "NON.COURSE.CATEGORIES",
                ValsEntityAssociation = new List<ApplValcodesVals>()
                {
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "AD",
                        ValExternalRepresentationAssocMember = "AD",
                        ValActionCode1AssocMember = "A"
                    },
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "PL",
                        ValExternalRepresentationAssocMember = "PL",
                        ValActionCode1AssocMember = "P"
                    },
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "OT",
                        ValExternalRepresentationAssocMember = "OT",
                        ValActionCode1AssocMember = "T"
                    },
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "G",
                        ValExternalRepresentationAssocMember = "G",
                        ValActionCode1AssocMember = ""
                    }
                },
            };

            _nonCourseStats = new ApplValcodes()
            {
                Recordkey = "STUDENT.NON.COURSE.STATUSES",
                ValsEntityAssociation = new List<ApplValcodesVals>()
                {
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "NC",
                        ValExternalRepresentationAssocMember = "NC",
                        ValActionCode1AssocMember = ""
                    },
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "AC",
                        ValExternalRepresentationAssocMember = "AC",
                        ValActionCode1AssocMember = "2"
                    },
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "NT",
                        ValExternalRepresentationAssocMember = "NT",
                        ValActionCode1AssocMember = "3"
                    },
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "WD",
                        ValExternalRepresentationAssocMember = "WD",
                        ValActionCode1AssocMember = "1"
                    }
                },
            };

            _nonAplicationTestSources = new ApplValcodes()
            {
                Recordkey = "APPL.TEST.SOURCES",
                ValsEntityAssociation = new List<ApplValcodesVals>()
                {
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "SAT",
                        ValExternalRepresentationAssocMember = "HS",
                        ValActionCode1AssocMember = ""
                    },
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "ACT",
                        ValExternalRepresentationAssocMember = "SF",
                        ValActionCode1AssocMember = "1"
                    },
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "TP",
                        ValExternalRepresentationAssocMember = "TP",
                        ValActionCode1AssocMember = "TP"
                    }
                },
            };
        }

        private StudentTestScoresRepository BuildStudentTestScoresRepository()
        {
            apiSettings = new ApiSettings("TEST");

            records = new Collection<DataContracts.StudentNonCourses>();
            foreach (var item in _studentTestScoresEntities)
            {
                DataContracts.StudentNonCourses record = new DataContracts.StudentNonCourses();
                record.Recordkey = item.Code;
                record.RecordGuid = item.Guid;
                record.StncNonCourse = item.Code;
                record.StncPersonId = item.StudentId;
                record.StncPct = item.Percentile1;
                record.StncPct2 = item.Percentile2;
                record.StncScoreDec = item.Score;
                record.StncSource = item.Source;
                record.StncSpecialFactors = item.SpecialFactors;
                record.StncStartDate = item.DateTaken;
                record.StncStatus = item.StatusCode;
                record.StncTestFormName = item.FormName;
                record.StncTestFormNo = item.FormNo;
                record.StncTitle = item.Description;
                record.StncStatus = item.StatusCode;
                record.StncStatusDate = item.StatusDate;
                records.Add(record);
            }
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentNonCourses>(It.IsAny<string>(), true)).ReturnsAsync(records);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentNonCourses>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(records);
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentNonCourses>(It.IsAny<string>(), true)).ReturnsAsync(records[0]);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<NonCourses>("", true)).ReturnsAsync(_nonCourseDataContracts);
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<NonCourses>(It.IsAny<string>(),It.IsAny<bool>())).ReturnsAsync(_nonCourseDataContracts.FirstOrDefault());
            dataReaderMock.Setup(acc => acc.ReadRecord<ApplValcodes>("ST.VALCODES", "NON.COURSE.CATEGORIES", true)).Returns(_nonCourseCats);
            dataReaderMock.Setup(acc => acc.ReadRecord<ApplValcodes>("ST.VALCODES", "STUDENT.NON.COURSE.STATUSES", true)).Returns(_nonCourseStats);
            dataReaderMock.Setup(acc => acc.ReadRecord<ApplValcodes>("ST.VALCODES", "APPL.TEST.SOURCES", true)).Returns(_nonAplicationTestSources);
            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2", "3", "4" });

            dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "STNC.NON.COURSE", PrimaryKey = "1" } }
                };

            request = new UpdateStudentAptitudeAssessmentRequest()
            {
                StncGuid = guid,
                StncPersonId = "0003784",
                StncNonCourse = "1",
                StncStartDate = new DateTime(2017, 12, 11),
                StncTestFormName = "ACT",
                StncTestFormNo = "1",
                StncPct = 79,
                StncPct2 = 33,
                StncScoreDec = 200,
                StncSource = "ACT",
                StncSpecialFactors = new List<string>() { "A", "D" },
                StncDerivedStatus = "AC"

            };

            response = new UpdateStudentAptitudeAssessmentResponse()
            {
                StncGuid = guid,
                StudentNonCoursesId = "1"
            };
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
            dataReaderMock.Setup(r => r.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(person);
            dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", It.IsAny<bool>())).ReturnsAsync(intlParams);

            transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAptitudeAssessmentRequest, UpdateStudentAptitudeAssessmentResponse>(It.IsAny<UpdateStudentAptitudeAssessmentRequest>())).ReturnsAsync(response);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var stuNonCourse = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, stuNonCourse == null ? null : new GuidLookupResult() { Entity = "STUDENT.NON.COURSES", PrimaryKey = stuNonCourse.Recordkey });
                }
                return Task.FromResult(result);
            });

            // Construct repository
            _studentAptitudeAssessmentsRepository = new StudentTestScoresRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return _studentAptitudeAssessmentsRepository;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTestScoresService_CreateStudentTestScoresAsync_ArgumentNullException_studentTestScore_Null()
        {
            await _studentAptitudeAssessmentsRepository.CreateStudentTestScoresAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentTestScoresService_CreateStudentTestScoresAsync_RepositoryException()
        {
            var studentTestScore = _studentTestScoresEntities.FirstOrDefault(x => x.Guid == guid);
            response.UpdateStudentAptitudeAsessmentErrors = new List<UpdateStudentAptitudeAsessmentErrors>()
            {  new UpdateStudentAptitudeAsessmentErrors() { ErrorMessages = "Error" }};

            await _studentAptitudeAssessmentsRepository.CreateStudentTestScoresAsync(studentTestScore);
        }


        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTestScoresService_CreateStudentTestScoresAsync_Get_BuildstudentTestScore_ArgumentNullException_studentTestScore_Null()
        {
            var studentTestScore = _studentTestScoresEntities.FirstOrDefault(x => x.Guid == guid);

            dataReaderMock.Setup(r => r.ReadRecordAsync<DataContracts.StudentNonCourses>(It.IsAny<string>(), true)).ReturnsAsync(null);
            await _studentAptitudeAssessmentsRepository.CreateStudentTestScoresAsync(studentTestScore);
        }


        [TestMethod]
        public async Task StudentTestScoresService_CreateStudentTestScoresAsync()
        {
            var studentTestScore = _studentTestScoresEntities.FirstOrDefault(x => x.Guid == guid);
            var result = await _studentAptitudeAssessmentsRepository.CreateStudentTestScoresAsync(studentTestScore);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.ApplicationTestSource, "unofficial", "ApplicationTestSource");
            Assert.AreEqual(result.Code, request.StncNonCourse, "Code");
            Assert.AreEqual(result.DateTaken, request.StncStartDate, "DateTaken");
            Assert.AreEqual(result.Description, "ACT Test", "Description");
            Assert.AreEqual(result.FormName, request.StncTestFormName, "FormName");
            Assert.AreEqual(result.FormNo, request.StncTestFormNo, "FormNo");
            Assert.AreEqual(result.Guid, request.StncGuid, "Guid");
            Assert.AreEqual(result.Percentile1, request.StncPct, "Percentile1");
            Assert.AreEqual(result.Percentile2, request.StncPct2, "Percentile2");
            Assert.AreEqual(result.Score, request.StncScoreDec, "Score");
            Assert.AreEqual(result.Source, request.StncSource, "Source");
            Assert.AreEqual(result.StatusCode, request.StncDerivedStatus, "StatusCode");
            Assert.AreEqual(result.StudentId, request.StncPersonId, "Student");
            Assert.AreEqual(result.StatusDate, request.StncStartDate, "StatusDate");
            Assert.AreEqual(result.StatusCodeSpProcessing, "2", "StatusCodeSpProcessing");

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentTestScoresService_DeleteStudentTestScoresAsync()
        {
            transManagerMock.Setup(i => i.ExecuteAsync<DeleteStudentAptitudeAssessmentRequest, DeleteStudentAptitudeAssessmentResponse>(It.IsAny<DeleteStudentAptitudeAssessmentRequest>()))
                .ReturnsAsync(new DeleteStudentAptitudeAssessmentResponse()
                {
                    DeleteStudentAptitudeAssessmentErrors = new List<DeleteStudentAptitudeAssessmentErrors>()
                    {
                        new DeleteStudentAptitudeAssessmentErrors()
                        {
                            ErrorCodes = "1",
                            ErrorMessages = "ErrorMessages1"
                        }
                    }
                });
            await _studentAptitudeAssessmentsRepository.DeleteAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTestScoresService_UpdateStudentTestScoresAsync_ArgumentNullException_studentTestScore_Null()
        {
            await _studentAptitudeAssessmentsRepository.UpdateStudentTestScoresAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentTestScoresService_UpdateStudentTestScoresAsync_RepositoryException()
        {
            var studentTestScore = _studentTestScoresEntities.FirstOrDefault(x => x.Guid == guid);
            response.UpdateStudentAptitudeAsessmentErrors = new List<UpdateStudentAptitudeAsessmentErrors>()
            {  new UpdateStudentAptitudeAsessmentErrors() { ErrorMessages = "Error", ErrorCodes = "StudentAptitudeAssessments" }};
            await _studentAptitudeAssessmentsRepository.UpdateStudentTestScoresAsync(studentTestScore);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTestScoresService_UpdateStudentTestScoresAsync_Get_BuildstudentTestScore_ArgumentNullException_studentTestScore_Null()
        {
            var studentTestScore = _studentTestScoresEntities.FirstOrDefault(x => x.Guid == guid);
            dataReaderMock.Setup(r => r.ReadRecordAsync<DataContracts.StudentNonCourses>(It.IsAny<string>(), true)).ReturnsAsync(null);
            await _studentAptitudeAssessmentsRepository.UpdateStudentTestScoresAsync(studentTestScore);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTestScoresService_DeleteStudentTestScoresAsync_ArgumentNullException()
        {
            await _studentAptitudeAssessmentsRepository.DeleteAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTestScoresService_DeleteStudentTestScoresAsync_KeyNotFoundException()
        {
            await _studentAptitudeAssessmentsRepository.DeleteAsync("1234");
        }



        [TestMethod]
        public async Task StudentTestScoresService_UpdateStudentTestScoresAsync()
        {
            var studentTestScore = _studentTestScoresEntities.FirstOrDefault(x => x.Guid == guid);
            var result = await _studentAptitudeAssessmentsRepository.UpdateStudentTestScoresAsync(studentTestScore);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.ApplicationTestSource, "unofficial", "ApplicationTestSource");
            Assert.AreEqual(result.Code, request.StncNonCourse, "Code");
            Assert.AreEqual(result.DateTaken, request.StncStartDate, "DateTaken");
            Assert.AreEqual(result.Description, "ACT Test", "Description");
            Assert.AreEqual(result.FormName, request.StncTestFormName, "FormName");
            Assert.AreEqual(result.FormNo, request.StncTestFormNo, "FormNo");
            Assert.AreEqual(result.Guid, request.StncGuid, "Guid");
            Assert.AreEqual(result.Percentile1, request.StncPct, "Percentile1");
            Assert.AreEqual(result.Percentile2, request.StncPct2, "Percentile2");
            Assert.AreEqual(result.Score, request.StncScoreDec, "Score");
            Assert.AreEqual(result.Source, request.StncSource, "Source");
            Assert.AreEqual(result.StatusCode, request.StncDerivedStatus, "StatusCode");
            Assert.AreEqual(result.StudentId, request.StncPersonId, "Student");
            Assert.AreEqual(result.StatusDate, request.StncStartDate, "StatusDate");
            Assert.AreEqual(result.StatusCodeSpProcessing, "2", "StatusCodeSpProcessing");
           
        }
    }
}