// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentStandingRepositoryTests
    {
        protected List<string> studentIds;
        protected Dictionary<string, StudentStandings> studentStandingRecords;
        Collection<StudentStandings> studentStandingResponseData;
        StudentStandingRepository studentStandingRepo;

        #region Private data array setup

        private string[,] _studentStandingData = {
                                       {"001", "0000304", "GOOD", "2014-01-02", "UG", "BA-HIST", "2013/FA","ACPG", "PROB", "Department assigned override", "999"},
                                       {"002", "0000304", "GOOD", "2014-01-02", "UG", "BA-HIST", "", "ACLV", "", "", "1000"},
                                       {"003", "0000304", "GOOD", "2014-01-02", "", "BA-HIST", "2013/FA", "TERM", "", "", "1001"},
                                       {"004", "0000404", "DEAN", "2014-02-05", "", "", "2013/FA", "TERM", "", "", "1002"},
                                       {"005", "0000404", "GOOD", "2014-02-05", "", "", "2013/FA", "TERM", "", "", "1003"},
                                       {"006", "0000504", "PROB", "2014-05-02", "UG", "", "2013/FA", "ACLV", "", "", "1004"},
                                       
                                       {"007", "9999999", null, "2014-01/01", null, null, null, null, null, null, "1005"},
                                       {"008", "9999998", "", "2014-01-01", "", "", "", "", "", "", "1006"}
                                   };

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            studentStandingRecords = SetupStudentStandings(out studentIds);
            studentStandingRepo = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentStandingRepo = null;
        }

        [TestMethod]
        public async Task CheckSingleStudentStandingProperties_Valid()
        {
            List<string> studentId = new List<string>();
            studentId.Add(studentIds.ElementAt(0));
            IEnumerable<StudentStanding> studentStandings = await studentStandingRepo.GetAsync(studentId);
            StudentStanding studentStanding = studentStandings.ElementAt(0);
            Assert.AreEqual(studentIds.ElementAt(0), studentStanding.StudentId);
            Assert.AreEqual("GOOD", studentStanding.StandingCode);
            Assert.AreEqual(DateTime.Parse("2014-01-02"), studentStanding.StandingDate);
            Assert.AreEqual("UG", studentStanding.Level);
            Assert.AreEqual("BA-HIST", studentStanding.Program);
            Assert.AreEqual("2013/FA", studentStanding.Term);
            Assert.AreEqual(StudentStandingType.Program, studentStanding.Type);
            Assert.AreEqual("PROB", studentStanding.CalcStandingCode);
            Assert.AreEqual("Department assigned override", studentStanding.OverrideReason);
            Assert.AreEqual("999", studentStanding.Id);
        }
        [TestMethod]
        public async Task MultiStudentStandingCount_Valid()
        {
            IEnumerable<StudentStanding> studentStandings = await studentStandingRepo.GetAsync(studentIds);
            Assert.AreEqual(6, studentStandings.Count());
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewStudentStandings_NullId()
        {
            StudentStanding studentStanding = new StudentStanding(null, "0000304", "GOOD", DateTime.Now);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewStudentStandings_NullStudentId()
        {
            StudentStanding studentStanding = new StudentStanding("999", null, "GOOD", DateTime.Now);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewStudentStandings_NullStanding()
        {
            StudentStanding studentStanding = new StudentStanding("999", "0000304", null, DateTime.Now);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewStudentStandings_NullStandingDate()
        {
            StudentStanding studentStanding = new StudentStanding("999", "0000304", "GOOD", default(DateTime));
        }

        [TestMethod]
        public async Task GetSingleStudentGroupStandings()
        {
            List<string> studentId = new List<string>();
            studentId.Add(studentIds.ElementAt(0));            
            var result = await studentStandingRepo.GetGroupedAsync(studentId);
            Assert.IsTrue(result["0000304"].Count() == 3);
            Assert.IsTrue(result["0000304"] != null && result["0000304"].Where(s => s.Term == "2013/FA").Count() == 2);
            Assert.IsTrue(result["0000304"] != null && result["0000304"].Where(s => s.StandingCode == "GOOD").Count() == 3);
        }

        [TestMethod]
        public async Task GetMultipleStudentsGroupStandings()
        {
            var result = await studentStandingRepo.GetGroupedAsync(studentIds);
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result["0000404"] != null && result["0000404"].Where(s => s.Term == "2013/FA").Count() == 2);
            Assert.IsTrue(result["0000404"] != null && result["0000404"].Where(s => s.StandingCode == "DEAN").Count() == 1);
            Assert.IsTrue(result["0000404"] != null && result["0000404"].Where(s => s.StandingCode == "GOOD").Count() == 1);
            Assert.IsTrue(result["0000504"] != null && result["0000504"].Where(s => s.Term == "2013/FA").Count() == 1);
            Assert.IsTrue(result["0000504"] != null && result["0000504"].Where(s => s.StandingCode == "PROB").Count() == 1);
        }

        private StudentStandingRepository BuildValidRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            var localCacheMock = new Mock<ObjectCache>();
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up Student Standings Response
            studentStandingResponseData = BuildStudentStandingsResponse(studentStandingRecords);

            string[] studentStandingIds = { "001", "002", "003", "004", "005", "006", "007", "008" };
            dataAccessorMock.Setup<string[]>(a => a.Select("STUDENT.STANDINGS", " WITH STS.STUDENT = '?'", It.IsAny<string[]>(), "?", true, 425)).Returns(studentStandingIds);
            dataAccessorMock.Setup<Task<Collection<StudentStandings>>>(acc => acc.BulkReadRecordAsync<StudentStandings>("STUDENT.STANDINGS", It.IsAny<string[]>(), true)).ReturnsAsync(studentStandingResponseData);

            // Construct StudentStanding repository
            studentStandingRepo = new StudentStandingRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return studentStandingRepo;
        }

        private Dictionary<string, StudentStandings> SetupStudentStandings(out List<string> ids)
        {
            ids = new List<string>();
            string[,] recordData = _studentStandingData;

            int recordCount = recordData.Length / 11;
            Dictionary<string, StudentStandings> results = new Dictionary<string, StudentStandings>();
            for (int i = 0; i < recordCount; i++)
            {
                StudentStandings response = new StudentStandings();
                string key = recordData[i, 0].TrimEnd();
                string studentId = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string standing = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd();
                DateTime? standingDate = (recordData[i, 3].TrimEnd() == null) ? DateTime.Now : DateTime.Parse(recordData[i, 3].TrimEnd());
                string level = (recordData[i, 4] == null) ? null : recordData[i, 4].TrimEnd();
                string program = (recordData[i, 5] == null) ? null : recordData[i, 5].TrimEnd();
                string term = (recordData[i, 6] == null) ? null : recordData[i, 6].TrimEnd();
                string type = (recordData[i, 7] == null) ? "TERM" : recordData[i, 7].TrimEnd();
                string calcStanding = (recordData[i, 8] == null) ? null : recordData[i, 8].TrimEnd();
                string overrideReason = (recordData[i, 9] == null) ? null : recordData[i, 9].TrimEnd();
                string studentStandingId = (recordData[i, 10] == null) ? null : recordData[i, 10].TrimEnd();

                response.Recordkey = key;
                response.StsStudent = studentId;
                response.StsAcadStanding = standing;
                response.StsAcadStandingDate = standingDate;
                response.StsAcadLevel = level;
                response.StsAcadProgram = program;
                response.StsTerm = term;
                response.StsType = type;
                response.StsCalcAcadStanding = calcStanding;
                response.StsOverrideReason = overrideReason;
                response.Recordkey = studentStandingId;
                if (ids.Where(id => id.Equals(studentId)).Count() == 0)
                {
                    ids.Add(studentId);
                }
                results.Add(key, response);
            }
            return results;
        }

        private Collection<StudentStandings> BuildStudentStandingsResponse(Dictionary<string, StudentStandings> studentStandingRecords)
        {
            Collection<StudentStandings> studentStandingContracts = new Collection<StudentStandings>();
            foreach (var studentStandingItem in studentStandingRecords)
            {
                studentStandingContracts.Add(studentStandingItem.Value);
            }
            return studentStandingContracts;
        }
    }
}
