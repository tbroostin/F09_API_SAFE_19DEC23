//Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentTranscriptGradesRepositoryTests : BaseRepositorySetup
    {
        private ICollection<StudentTranscriptGrades> _studentTranscriptGradesCollection;
        private ICollection<StudentTranscriptGradesAdjustments> _studentTranscriptGradesAdjustmentsCollection;
        private Collection<DataContracts.StudentAcadCred> records;

        StudentTranscriptGradesRepository _StudentTranscriptGradesRepository;
        StudentTranscriptGradesRepository _StudentTranscriptGradesRepositoryWithBadHistory;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<IColleagueTransactionInvoker> transactionInvokerMock;

        const string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        DateTime currentDate = DateTime.Now;
        IntlParams internationalParameters = new IntlParams()
        {
            HostShortDateFormat = "MDY",
            HostDateDelimiter = "/"
        };

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            BuildData();
            _StudentTranscriptGradesRepository = BuildStudentTranscriptGradesRepository();
            _StudentTranscriptGradesRepositoryWithBadHistory = BuildStudentTranscriptGradesRepository(true);
        }

        private void BuildData()
        {
            _studentTranscriptGradesCollection = new List<StudentTranscriptGrades>()
                {
                    new StudentTranscriptGrades("1", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                    {
                       AltcumContribCmplCredits = 1.0m,
                       AltcumContribGpaCredits = 2.0m,
                       AltcumContribGradePts = 3.0m,
                       AttemptedCeus = 1.0m,
                       AttemptedCredit = 1.0m,
                       CompletedCeus = 1.0m,
                       CompletedCredit = 1.0m,
                       ContribCmplCredits = 1.0m,
                       ContribGpaCredits = 1.0m,
                       ContribGradePoints = 1.0m,
                       Course = "MATH-101",
                       CourseName = "MATH-101",
                       CourseSection = "1",
                       CreditType = "TR",
                       FinalGradeExpirationDate = DateTime.Now, 
                       GradePoints = 2.0m,
                       GradeSchemeCode = "A",
                       RepeatAcademicCreditIds = new List<string> { "1", "2"},
                       ReplCode = "Y",
                       StudentCourseSectionId = "0000005",
                       
                       StudentId = "0000111",
                       StwebTranAltcumFlag = false,
                       Title = "title",
                       VerifiedGrade = "2",
                       VerifiedGradeDate = DateTime.Now
                    },
                    new StudentTranscriptGrades("2", "8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                    {
                       AltcumContribCmplCredits = 1.0m,
                       AltcumContribGpaCredits = 2.0m,
                       AltcumContribGradePts = 3.0m,
                       AttemptedCeus = 1.0m,
                       AttemptedCredit = 1.0m,
                       CompletedCeus = 1.0m,
                       CompletedCredit = 1.0m,
                       ContribCmplCredits = 1.0m,
                       ContribGpaCredits = 1.0m,
                       ContribGradePoints = 1.0m,
                       Course = "MATH-101",
                       CourseName = "MATH-101",
                       CourseSection = "100",
                       CreditType = "TR",
                       FinalGradeExpirationDate = DateTime.Now

                    },
                };

            _studentTranscriptGradesAdjustmentsCollection = new List<StudentTranscriptGradesAdjustments>()
            {
                new StudentTranscriptGradesAdjustments("1", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                {
                    ChangeReason = "CI",
                    ExtensionDate = null,
                    IncompleteGrade = "",
                    VerifiedGrade = "A"
                },
                new StudentTranscriptGradesAdjustments("2", "8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                {
                    ChangeReason = "CI",
                    ExtensionDate = new DateTime(2020, 10, 31),
                    IncompleteGrade = "F",
                    VerifiedGrade = "I"
                }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentTranscriptGradesCollection = null;
            records = null;
            _StudentTranscriptGradesRepository = null;
            _StudentTranscriptGradesRepositoryWithBadHistory = null;
            dataAccessorMock = null;
        }
        
        [TestMethod]
        public async Task StudentTranscriptGradesRepository_GET()
        {
            dataAccessorMock.Setup(repo => repo.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string>())).ReturnsAsync(new[] { "1" , "2"});
            var scsCourseSection = "0000001";
            var studentAcadCredRecord = records.FirstOrDefault(x => x.RecordGuid == guid);

            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.ACAD.CRED", LdmGuidSecondaryFld = "STC.INTG.KEY.IDX", LdmGuidPrimaryKey = "1" };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentAcadCredRecord);

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add("STUDENT.ACAD.CRED+" + studentAcadCredRecord.Recordkey + "+" + studentAcadCredRecord.StcIntgKeyIdx,
                new RecordKeyLookupResult() { Guid = guid });

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

            dataAccessorMock.Setup(dr => dr.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), ""))
                .ReturnsAsync(new string[] { studentAcadCredRecord.StcStudentCourseSec });
            dataAccessorMock.Setup(dr => dr.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<StudentCourseSec>() {
                    new StudentCourseSec() { Recordkey = studentAcadCredRecord.StcStudentCourseSec, ScsCourseSection = scsCourseSection } });

            dataAccessorMock.Setup(repo => repo.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(records);


            var results = await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task StudentTranscriptGradesRepository_GET_EmptySet()
        {
            dataAccessorMock.Setup(repo => repo.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string>())).ReturnsAsync(new[] { "1", "200" });
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), true)).ReturnsAsync(() => null);

            var results = await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>());
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Item2, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTranscriptGradesRepository_GETById_SecondaryFld_Invalid()
        {
            var studentAcadCredRecord = records.FirstOrDefault(x => x.RecordGuid == guid);

            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.ACAD.CRED", LdmGuidSecondaryFld = "INVALID", LdmGuidPrimaryKey = "1" };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentAcadCredRecord);

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add("STUDENT.ACAD.CRED+" + studentAcadCredRecord.Recordkey + "+" + studentAcadCredRecord.StcIntgKeyIdx,
                new RecordKeyLookupResult() { Guid = guid });

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

            await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTranscriptGradesRepository_GETById_RecordNotFound()
        {
            var studentAcadCredRecord = records.FirstOrDefault(x => x.RecordGuid == guid);
            studentAcadCredRecord.Recordkey = null;
            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.ACAD.CRED", LdmGuidSecondaryFld = "STC.INTG.KEY.IDX", LdmGuidPrimaryKey = "1" };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(() => null);

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add("STUDENT.ACAD.CRED+" + studentAcadCredRecord.Recordkey + "+" + studentAcadCredRecord.StcIntgKeyIdx,
                new RecordKeyLookupResult() { Guid = guid });

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

            await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTranscriptGradesRepository_GETById_GuidNotFound()
        {
            var studentAcadCredRecord = records.FirstOrDefault(x => x.RecordGuid == guid);
            studentAcadCredRecord.Recordkey = null;
            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.ACAD.CRED", LdmGuidSecondaryFld = "STC.INTG.KEY.IDX", LdmGuidPrimaryKey = "1" };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(() => null);

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add("STUDENT.ACAD.CRED+" + studentAcadCredRecord.Recordkey + "+" + studentAcadCredRecord.StcIntgKeyIdx,
                new RecordKeyLookupResult() { Guid = guid });

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

            await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentTranscriptGradesRepository_GETById_StwebDefaults_Null()
        {
           
            var studentAcadCredRecord = records.FirstOrDefault(x => x.RecordGuid == guid);

            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.ACAD.CRED", LdmGuidSecondaryFld = "STC.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1" };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentAcadCredRecord);

            dataReaderMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", It.IsAny<bool>())).ReturnsAsync(() => null);

            await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentTranscriptGradesRepository_GETById_RecordKeyLookup_null()
        {

            var scsCourseSection = "0000001";
            var studentAcadCredRecord = records.FirstOrDefault(x => x.RecordGuid == guid);

            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.ACAD.CRED", LdmGuidSecondaryFld = "STC.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1" };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentAcadCredRecord);

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add("STUDENT.ACAD.CRED+" + studentAcadCredRecord.Recordkey + "+" + studentAcadCredRecord.StcIntgKeyIdx,
                new RecordKeyLookupResult() { Guid = guid });

            //setting the recordKeyLookup to null generates the error
            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(() => null);

            dataAccessorMock.Setup(dr => dr.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), ""))
                .ReturnsAsync(new string[] { studentAcadCredRecord.StcStudentCourseSec });
            dataAccessorMock.Setup(dr => dr.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<StudentCourseSec>() {
                    new StudentCourseSec() { Recordkey = studentAcadCredRecord.StcStudentCourseSec, ScsCourseSection = scsCourseSection } });


            await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync(guid);

        }

        [TestMethod]
        public async Task StudentTranscriptGradesRepository_GETById()
        {
            var scsCourseSection = "0000001";
            var studentAcadCredRecord = records.FirstOrDefault(x => x.RecordGuid == guid);

            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.ACAD.CRED", LdmGuidSecondaryFld = "STC.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1" };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid) ;

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentAcadCredRecord);
           
            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
           
            recordLookupDict.Add("STUDENT.ACAD.CRED+" + studentAcadCredRecord.Recordkey + "+" + studentAcadCredRecord.StcIntgKeyIdx, 
                new RecordKeyLookupResult() { Guid = guid });
            
            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

            dataAccessorMock.Setup(dr => dr.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), ""))
                .ReturnsAsync(new string[] { studentAcadCredRecord.StcStudentCourseSec });
            dataAccessorMock.Setup(dr => dr.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync( new Collection<StudentCourseSec>() {
                    new StudentCourseSec() { Recordkey = studentAcadCredRecord.StcStudentCourseSec, ScsCourseSection = scsCourseSection } } );
            dataAccessorMock.Setup(dr => dr.ReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
               .ReturnsAsync(new StudentCourseSec() { Recordkey = studentAcadCredRecord.StcStudentCourseSec, ScsCourseSection = scsCourseSection });

            var results = await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync(guid);
            Assert.IsNotNull(results);
            Assert.AreEqual(results.AltcumContribCmplCredits, studentAcadCredRecord.StcAltcumContribCmplCred, "StcAltcumContribCmplCred");
            Assert.AreEqual(results.AltcumContribGpaCredits, studentAcadCredRecord.StcAltcumContribGpaCred, "StcAltcumContribGpaCred");
            Assert.AreEqual(results.AltcumContribGradePts, studentAcadCredRecord.StcAltcumContribGradePts, "StcAltcumContribGradePts");
            Assert.AreEqual(results.AttemptedCeus, studentAcadCredRecord.StcAttCeus, "StcAttCeus");
            Assert.AreEqual(results.AttemptedCredit, studentAcadCredRecord.StcAttCred, "StcAttCred");
            Assert.AreEqual(results.CompletedCeus, studentAcadCredRecord.StcCmplCeus, "StcCmplCeus");
            Assert.AreEqual(results.CompletedCredit, studentAcadCredRecord.StcCmplCred, "StcCmplCred");
            Assert.AreEqual(results.ContribCmplCredits, studentAcadCredRecord.StcCumContribCmplCred, "StcCumContribCmplCred");
            Assert.AreEqual(results.ContribGpaCredits, studentAcadCredRecord.StcCumContribGpaCred, "StcCumContribGpaCred");
            Assert.AreEqual(results.ContribGradePoints, studentAcadCredRecord.StcCumContribGradePts, "StcCumContribGradePts");
            Assert.AreEqual(results.Course, studentAcadCredRecord.StcCourse, "StcCourse");
            Assert.AreEqual(results.CourseName, studentAcadCredRecord.StcCourseName, "StcCourseName");
            Assert.AreEqual(results.CourseSection, scsCourseSection, "CourseSection");
            Assert.AreEqual(results.CreditType, studentAcadCredRecord.StcCredType, "StcCredType");

            Assert.AreEqual(results.GradePoints, studentAcadCredRecord.StcGradePts, "StcGradePts");
            Assert.AreEqual(results.GradeSchemeCode, studentAcadCredRecord.StcGradeScheme, "StcGradeScheme");
            Assert.AreEqual(results.Guid, studentAcadCredRecord.RecordGuid, "RecordGuid");
            Assert.AreEqual(results.Id, studentAcadCredRecord.Recordkey, "Recordkey");
            Assert.AreEqual(results.RepeatAcademicCreditIds, studentAcadCredRecord.StcRepeatedAcadCred, "StcRepeatedAcadCred");
            Assert.AreEqual(results.ReplCode, studentAcadCredRecord.StcReplCode, "StcReplCode");
            Assert.AreEqual(results.StudentCourseSectionId, studentAcadCredRecord.StcStudentCourseSec, "StcStudentCourseSec");
            Assert.AreEqual(results.StudentId, studentAcadCredRecord.StcPersonId, "StcPersonId");

            Assert.AreEqual(results.Title, studentAcadCredRecord.StcTitle, "StcTitle");
            Assert.AreEqual(results.VerifiedGrade, studentAcadCredRecord.StcVerifiedGrade, "StcVerifiedGrade");
            Assert.AreEqual(results.VerifiedGradeDate, studentAcadCredRecord.StcVerifiedGradeDate, "StcVerifiedGradeDate");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentTranscriptGradesRepository_GETById_ArgumentNullException()
        {
            var results = await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTranscriptGradesRepository_GETById_KeyNotFoundException()
        {
            var results = await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync("BadKey");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentTranscriptGradesRepository_GETById_StudentTranscriptGradess_Null_KeyNotFoundException()
        {
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), true)).ReturnsAsync(() => null);
            var results = await _StudentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
        }

        [TestMethod]
        public async Task GetGuidsCollectionAsync()
        {
            IEnumerable<string> sublist = new List<string>() { "1", "2" };
            Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
            recordKeyLookupResults.Add("STUDENT.ACAD.CRED+1", new RecordKeyLookupResult() { Guid = "854da721-4191-4875-bf58-7d6c00ffea8f", ModelName = "STUDENT.ACAD.CRED" });
            recordKeyLookupResults.Add("STUDENT.ACAD.CRED+2", new RecordKeyLookupResult() { Guid = "71e1a806-24a8-4d93-91a2-02d86056b63c", ModelName = "STUDENT.ACAD.CRED" });
            List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

            dataAccessorMock.Setup(i => i.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2", "3", "4" });
            dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

            var results = await _StudentTranscriptGradesRepository.GetGuidsCollectionAsync(sublist, "STUDENT.ACAD.CRED");
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
        public async Task StudentTranscriptGradesRepository_GET_withBadHistory()
        {
            dataAccessorMock.Setup(repo => repo.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2" });
            var scsCourseSection = "0000001";
            var studentAcadCredRecord = records.FirstOrDefault(x => x.RecordGuid == guid);

            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.ACAD.CRED", LdmGuidSecondaryFld = "STC.INTG.KEY.IDX", LdmGuidPrimaryKey = "1" };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentAcadCredRecord);

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add("STUDENT.ACAD.CRED+" + studentAcadCredRecord.Recordkey + "+" + studentAcadCredRecord.StcIntgKeyIdx,
                new RecordKeyLookupResult() { Guid = guid });

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

            dataAccessorMock.Setup(dr => dr.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), ""))
                .ReturnsAsync(new string[] { studentAcadCredRecord.StcStudentCourseSec });
            dataAccessorMock.Setup(dr => dr.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<StudentCourseSec>() {
                    new StudentCourseSec() { Recordkey = studentAcadCredRecord.StcStudentCourseSec, ScsCourseSection = scsCourseSection } });

            dataAccessorMock.Setup(repo => repo.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(records);


            var results = await _StudentTranscriptGradesRepositoryWithBadHistory.GetStudentTranscriptGradesAsync(It.IsAny<int>(), It.IsAny<int>());
            Assert.IsNotNull(results);
        }

        #region student-transcript-grades-adjustments

        [TestMethod]
        public async Task StudentTranscriptGradesRepository_UpdateStudentTranscriptGradesAdjustments_ValidResponse()
        {
            var scsCourseSection = "0000001";
            var studentAcadCredRecord = records.FirstOrDefault(x => x.RecordGuid == guid);

            var ldmGuid = new LdmGuid() { LdmGuidEntity = "STUDENT.ACAD.CRED", LdmGuidSecondaryFld = "STC.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1" };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentAcadCred>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentAcadCredRecord);

            var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            recordLookupDict.Add("STUDENT.ACAD.CRED+" + studentAcadCredRecord.Recordkey + "+" + studentAcadCredRecord.StcIntgKeyIdx,
                new RecordKeyLookupResult() { Guid = guid });

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

            dataAccessorMock.Setup(dr => dr.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), ""))
                .ReturnsAsync(new string[] { studentAcadCredRecord.StcStudentCourseSec });
            dataAccessorMock.Setup(dr => dr.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<StudentCourseSec>() {
                    new StudentCourseSec() { Recordkey = studentAcadCredRecord.StcStudentCourseSec, ScsCourseSection = scsCourseSection } });
            dataAccessorMock.Setup(dr => dr.ReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new StudentCourseSec() { Recordkey = studentAcadCredRecord.StcStudentCourseSec, ScsCourseSection = scsCourseSection });

            var results = await _StudentTranscriptGradesRepository.UpdateStudentTranscriptGradesAdjustmentsAsync(_studentTranscriptGradesAdjustmentsCollection.FirstOrDefault());

            Assert.IsNotNull(results);
            Assert.AreEqual(results.AltcumContribCmplCredits, studentAcadCredRecord.StcAltcumContribCmplCred, "StcAltcumContribCmplCred");
            Assert.AreEqual(results.AltcumContribGpaCredits, studentAcadCredRecord.StcAltcumContribGpaCred, "StcAltcumContribGpaCred");
            Assert.AreEqual(results.AltcumContribGradePts, studentAcadCredRecord.StcAltcumContribGradePts, "StcAltcumContribGradePts");
            Assert.AreEqual(results.AttemptedCeus, studentAcadCredRecord.StcAttCeus, "StcAttCeus");
            Assert.AreEqual(results.AttemptedCredit, studentAcadCredRecord.StcAttCred, "StcAttCred");
            Assert.AreEqual(results.CompletedCeus, studentAcadCredRecord.StcCmplCeus, "StcCmplCeus");
            Assert.AreEqual(results.CompletedCredit, studentAcadCredRecord.StcCmplCred, "StcCmplCred");
            Assert.AreEqual(results.ContribCmplCredits, studentAcadCredRecord.StcCumContribCmplCred, "StcCumContribCmplCred");
            Assert.AreEqual(results.ContribGpaCredits, studentAcadCredRecord.StcCumContribGpaCred, "StcCumContribGpaCred");
            Assert.AreEqual(results.ContribGradePoints, studentAcadCredRecord.StcCumContribGradePts, "StcCumContribGradePts");
            Assert.AreEqual(results.Course, studentAcadCredRecord.StcCourse, "StcCourse");
            Assert.AreEqual(results.CourseName, studentAcadCredRecord.StcCourseName, "StcCourseName");
            Assert.AreEqual(results.CourseSection, scsCourseSection, "CourseSection");
            Assert.AreEqual(results.CreditType, studentAcadCredRecord.StcCredType, "StcCredType");

            Assert.AreEqual(results.GradePoints, studentAcadCredRecord.StcGradePts, "StcGradePts");
            Assert.AreEqual(results.GradeSchemeCode, studentAcadCredRecord.StcGradeScheme, "StcGradeScheme");
            Assert.AreEqual(results.Guid, studentAcadCredRecord.RecordGuid, "RecordGuid");
            Assert.AreEqual(results.Id, studentAcadCredRecord.Recordkey, "Recordkey");
            Assert.AreEqual(results.RepeatAcademicCreditIds, studentAcadCredRecord.StcRepeatedAcadCred, "StcRepeatedAcadCred");
            Assert.AreEqual(results.ReplCode, studentAcadCredRecord.StcReplCode, "StcReplCode");
            Assert.AreEqual(results.StudentCourseSectionId, studentAcadCredRecord.StcStudentCourseSec, "StcStudentCourseSec");
            Assert.AreEqual(results.StudentId, studentAcadCredRecord.StcPersonId, "StcPersonId");

            Assert.AreEqual(results.Title, studentAcadCredRecord.StcTitle, "StcTitle");
            Assert.AreEqual(results.VerifiedGrade, studentAcadCredRecord.StcVerifiedGrade, "StcVerifiedGrade");
            Assert.AreEqual(results.VerifiedGradeDate, studentAcadCredRecord.StcVerifiedGradeDate, "StcVerifiedGradeDate");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentTranscriptGradesRepository_UpdateStudentTranscriptGradesAdjustments_RepositoryError()
        {
            var updateResponse = new Ellucian.Colleague.Data.Student.Transactions.UpdateTranscriptGradeResponse()
            {
                Error = true,
                UpdateTranscriptGradeErrors = new List<Transactions.UpdateTranscriptGradeErrors>()
                {
                    new Transactions.UpdateTranscriptGradeErrors()
                    {
                        ErrorCodes = "studentTranscriptGradesAdjustments",
                        ErrorMessages = "Error has occurred in Mock CTX call."
                    }
                }
            };
            transactionInvokerMock.Setup(acc => acc.ExecuteAsync<Transactions.UpdateTranscriptGradeRequest, Transactions.UpdateTranscriptGradeResponse>(It.IsAny<Transactions.UpdateTranscriptGradeRequest>())).ReturnsAsync(updateResponse);

            var results = await _StudentTranscriptGradesRepository.UpdateStudentTranscriptGradesAdjustmentsAsync(_studentTranscriptGradesAdjustmentsCollection.FirstOrDefault());
        }

        #endregion

        private StudentTranscriptGradesRepository BuildStudentTranscriptGradesRepository(bool badHistory = false)
        {
            // New optional param badHistory to mock DataReader behavior when
            // The history table is malformed

            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            transactionInvokerMock = new Mock<IColleagueTransactionInvoker>();

            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            // Set up data accessor for mocking 
            dataAccessorMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvokerMock.Object);

            var stWebDefaults = new StwebDefaults()
            {
                StwebDefaultCtk = "DEFAULT",
                StwebShowAdviseComplete = "Y",
                StwebCatalogYearPolicy = "1",
                StwebTranAltcumFlag = "Y"
            };

            dataReaderMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", It.IsAny<bool>())).Returns(Task.FromResult<StwebDefaults>(stWebDefaults));

            var updateResponse = new Ellucian.Colleague.Data.Student.Transactions.UpdateTranscriptGradeResponse()
            {
                Error = false,
                UpdateTranscriptGradeErrors = new List<Transactions.UpdateTranscriptGradeErrors>()
            };
            transactionInvokerMock.Setup(acc => acc.ExecuteAsync<Transactions.UpdateTranscriptGradeRequest, Transactions.UpdateTranscriptGradeResponse>(It.IsAny<Transactions.UpdateTranscriptGradeRequest>())).ReturnsAsync(updateResponse);

            records = new Collection<DataContracts.StudentAcadCred>();
            foreach (var item in _studentTranscriptGradesCollection)
            {
                DataContracts.StudentAcadCred record = new DataContracts.StudentAcadCred();
                record.RecordGuid = item.Guid;
                record.Recordkey = item.Id;
                record.StcIntgKeyIdx = item.Id;
                record.StcAltcumContribCmplCred = item.AltcumContribCmplCredits;

                record.StcAltcumContribGpaCred = item.AltcumContribGpaCredits;
                record.StcAltcumContribGradePts = item.AltcumContribGradePts;
                record.StcAttCeus = item.AttemptedCeus;
                record.StcAttCred = item.AttemptedCredit;
                record.StcCmplCeus = item.CompletedCeus;
                record.StcCmplCred = item.CompletedCredit;
                record.StcCourse = item.Course;
                record.StcCourseName = item.CourseName;
                
                record.StcCredType = item.CreditType;
                record.StcGradePts = item.GradePoints;
                record.StcGradeScheme = item.GradeSchemeCode;
                record.StcRepeatedAcadCred = item.RepeatAcademicCreditIds;
                record.StcReplCode = item.ReplCode;
                record.StcStudentCourseSec = item.StudentCourseSectionId;
                record.StcPersonId = item.StudentId;
                record.StcTitle = item.Title;
                record.StcVerifiedGradeDate = item.VerifiedGradeDate;
                record.StcVerifiedGrade = item.VerifiedGrade;

                records.Add(record);
            }
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), true)).ReturnsAsync(records);

            // var criteria = "WITH HL.RECORD.ID EQ '" + (string.Join(" ", studentAcadCredIds.ToArray())).Replace(" ", "' '") + "'";
            //await DataReader.BulkReadRecordAsync<StcHistLog>("STUDENT.ACAD.CRED.HIST.LOG", criteria);
           

            var histLogs = new Collection<DataContracts.StcHistLog>();
            foreach (var item in _studentTranscriptGradesCollection)
            {
                var key = string.Concat(UniDataFormatter.UnidataFormatDate(currentDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter), "*12345*CRS*", item.Id);
                DataContracts.StcHistLog histLog = new DataContracts.StcHistLog();
                histLog.StchlRecordId = item.Id;
                histLog.StchlDate = currentDate.Date;
                histLog.StchlTime = currentDate;
                histLog.Recordkey = key;
                histLog.StchlHist = new List<string>() { key, "*1" };
                histLogs.Add(histLog);
            }
            if (badHistory)
            {
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StcHistLog>("STUDENT.ACAD.CRED.HIST.LOG", It.IsAny<string>(), true)).ThrowsAsync(new NullReferenceException());
            }
            else
            {
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StcHistLog>("STUDENT.ACAD.CRED.HIST.LOG", It.IsAny<string>(), true)).ReturnsAsync(histLogs);
            }
            // var histIdCollection = studentAcadCredHistLogRecords.SelectMany(hl => hl.StchlHist).ToArray();
            //var histCriteria = "WITH HIST.ID EQ '" + (string.Join(" ", histIdCollection.ToArray())).Replace(" ", "' '") + "' AND WITH HIST.FIELD.NAME EQ 'STC.VERIFIED.GRADE'";
            //studentAcadCredHistRecords = await DataReader.BulkReadRecordAsync<Hist>("STUDENT.ACAD.CRED.HIST", histCriteria);
            var hists = new Collection<Base.DataContracts.Hist>();
            foreach (var item in histLogs)
            {
                var key = item.StchlHist.ElementAt(0);

                Base.DataContracts.Hist hist = new Base.DataContracts.Hist();
                hist.Recordkey = key;
                hist.HistFieldName = "STC.VERIFIED.GRADE";
                hist.HistOldValues = "1";
                hist.HistNewValues = "2";
                hist.HistLog = key.Remove(key.Length - 2);
                hists.Add(hist);

            }

            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Base.DataContracts.Hist>("STUDENT.ACAD.CRED.HIST", It.IsAny<string>(), true)).ReturnsAsync(hists);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            _StudentTranscriptGradesRepository = new StudentTranscriptGradesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new ApiSettings());

            return _StudentTranscriptGradesRepository;
        }
    }
}
