// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
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
using System.Runtime.Caching;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentAttendanceRepositoryTests
    {
        [TestClass]
        public class StudentAttendanceRepository_GetStudentAttendancesAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            List<string> queryIds;
            IEnumerable<string> scsIds;
            Collection<StudentCourseSec> studentCourseSecResponseData;
            StudentAttendanceRepository StudentAttendanceRepository;


            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                queryIds = new List<string>() { "section1", "section2", "section3" };
                // Collection of data accessor responses
                studentCourseSecResponseData = BuildStudentCourseSecResponse();
                scsIds = studentCourseSecResponseData.Select(ss => ss.Recordkey);
                StudentAttendanceRepository = BuildValidStudentAttendanceRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentCourseSecResponseData = null;
                StudentAttendanceRepository = null;
            }

            [TestMethod]
            public async Task EmptyRepositoryDataReturnsEmptyList()
            {
                // Call Section Repository with a non-existant section
                var nonExistingIds = new List<string>() { "missingId" };
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(new List<string>().ToArray()));
                var studentAttendances = await StudentAttendanceRepository.GetStudentAttendancesAsync(nonExistingIds, null);
                Assert.AreEqual(0, studentAttendances.Count());

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfQueryIdsArgumentNull()
            {
                var studentAttendances = await StudentAttendanceRepository.GetStudentAttendancesAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfQueryIdsArgumentEmpty()
            {
                var studentAttendances = await StudentAttendanceRepository.GetStudentAttendancesAsync(new List<string>(), null);
            }

            [TestMethod]
            public async Task GetsAllStudentAttendance_NullDate()
            {
                var studentAttendances = await StudentAttendanceRepository.GetStudentAttendancesAsync(queryIds, null);
                Assert.AreEqual(12, studentAttendances.Count());
            }

            [TestMethod]
            public async Task GetsAllStudentAttendance_SpecifiedDate()
            {
                var studentAttendances = await StudentAttendanceRepository.GetStudentAttendancesAsync(queryIds, DateTime.Today);
                Assert.AreEqual(4, studentAttendances.Count());
            }

            [TestMethod]
            public async Task GetsAllStudentAttendance_SpecifiedDate_Correct_MinutesAttended_Values()
            {
                var studentAttendances2 = await StudentAttendanceRepository.GetStudentAttendancesAsync(new List<string>() { "section4" }, DateTime.Today.AddDays(-1));
                Assert.AreEqual(1, studentAttendances2.Count());
                Assert.AreEqual(135, studentAttendances2.First().CumulativeMinutesAttended);
                Assert.AreEqual(135, studentAttendances2.First().MinutesAttendedToDate);

                var studentAttendances3 = await StudentAttendanceRepository.GetStudentAttendancesAsync(new List<string>() { "section4" }, DateTime.Today.AddDays(-6));
                Assert.AreEqual(2, studentAttendances3.Count());
                Assert.AreEqual(135, studentAttendances3.First().CumulativeMinutesAttended);
                Assert.AreEqual(135, studentAttendances3.First().MinutesAttendedToDate);
                Assert.AreEqual(135, studentAttendances3.ElementAt(1).CumulativeMinutesAttended);
                Assert.AreEqual(135, studentAttendances3.ElementAt(1).MinutesAttendedToDate);

                var studentAttendances4 = await StudentAttendanceRepository.GetStudentAttendancesAsync(new List<string>() { "section4" }, DateTime.Today.AddDays(-11));
                Assert.AreEqual(1, studentAttendances4.Count());
                Assert.AreEqual(135, studentAttendances4.First().CumulativeMinutesAttended);
                Assert.AreEqual(60, studentAttendances4.First().MinutesAttendedToDate);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ThrowsExceptionIfAccessReturnsException()
            {
                StudentAttendanceRepository StudentAttendanceRepository = BuildInvalidStudentAttendanceRepository();
                var StudentAttendances = await StudentAttendanceRepository.GetStudentAttendancesAsync(queryIds, DateTime.Today.AddDays(-11));
            }

            [TestMethod]
            public async Task GetsAllStudentAttendance_NullDate_NullAttendanceTypes_With_Comment_Are_Not_Selected()
            {
                studentCourseSecResponseData = BuildCulmulativeTotalsStudentCourseSecResponse();
                scsIds = new List<string>() { studentCourseSecResponseData.ElementAt(0).Recordkey };
                StudentAttendanceRepository = BuildCumulativeTotalsStudentAttendanceRepository();
                var studentAttendances = await StudentAttendanceRepository.GetStudentAttendancesAsync(new List<string>() { "section6" }, null);

                var expectedLastRecordedDate = studentCourseSecResponseData.ElementAt(0).ScsAttendanceEntityAssociation.ElementAt(0).ScsAbsentDatesAssocMember;
                Assert.AreEqual(expectedLastRecordedDate, studentAttendances.ElementAt(0).LastAttendanceRecorded);
            }

            [TestMethod]
            public async Task GetsAllStudentAttendance_NullDate_NullAttendanceTypes_Correct_Attendance_Counts()
            {
                studentCourseSecResponseData = BuildCulmulativeTotalsStudentCourseSecResponse();
                scsIds = new List<string>() { studentCourseSecResponseData.ElementAt(1).Recordkey };
                StudentAttendanceRepository = BuildCumulativeTotalsStudentAttendanceRepository();
                var studentAttendances = await StudentAttendanceRepository.GetStudentAttendancesAsync(new List<string>() { "section7" }, null);

                //cumulative totals 2 present, 2 late, 1 absent, 1 excused 
                Assert.AreEqual(2, studentAttendances.ElementAt(0).NumberOfDaysPresent);
                Assert.AreEqual(2, studentAttendances.ElementAt(0).NumberOfDaysLate);
                Assert.AreEqual(1, studentAttendances.ElementAt(0).NumberOfDaysAbsent);
                Assert.AreEqual(1, studentAttendances.ElementAt(0).NumberOfDaysExcused);
            }

            [TestMethod]
            public async Task GetsAllStudentAttendance_NullDate_NullAbsentDates()
            {
                studentCourseSecResponseData = BuildCulmulativeTotalsStudentCourseSecResponse();
                scsIds = new List<string>() { studentCourseSecResponseData.ElementAt(2).Recordkey };
                StudentAttendanceRepository = BuildCumulativeTotalsStudentAttendanceRepository();
                var studentAttendances = await StudentAttendanceRepository.GetStudentAttendancesAsync(new List<string>() { "section8" }, null);

                var expectedLastRecordedDate = studentCourseSecResponseData.ElementAt(2).ScsAttendanceEntityAssociation.ElementAt(0).ScsAbsentDatesAssocMember;
                Assert.AreEqual(expectedLastRecordedDate, studentAttendances.ElementAt(0).LastAttendanceRecorded);
            }

            private Collection<StudentCourseSec> BuildStudentCourseSecResponse()
            {
                Collection<StudentCourseSec> studentCourseSecs = new Collection<StudentCourseSec>();

                // First StudentCourseSec has 3 attendances - 1 for today and 2 others
                var scs1 = new StudentCourseSec()
                {
                    Recordkey = "scs1",
                    ScsCourseSection = "section1",
                    ScsStudent = "student1",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa1 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-10), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa1);
                var scsa2 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-5), ScsAttendanceTypesAssocMember = "l", ScsAttendanceReasonAssocMember = "Reason2", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa2);
                var scsa3 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today, ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "E", ScsAttendanceReasonAssocMember = "Reason3", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa3);
                studentCourseSecs.Add(scs1);

                // Second StudentCourseSec has 1 attendances - 0 for today and 1 other
                var scs2 = new StudentCourseSec()
                {
                    Recordkey = "scs2",
                    ScsCourseSection = "section1",
                    ScsStudent = "student2",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa4 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-10), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs2.ScsAttendanceEntityAssociation.Add(scsa4);
                studentCourseSecs.Add(scs2);

                // Third StudentCourseSec has 3 attendances - 1 for today and 1 other and 1 bad data that will not convert (no date)
                var scs3 = new StudentCourseSec()
                {
                    Recordkey = "scs3",
                    ScsCourseSection = "section1",
                    ScsStudent = "student3",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa5 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-10), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceReasonAssocMember = "Reason1" };
                scs3.ScsAttendanceEntityAssociation.Add(scsa5);
                var scsa6 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today, ScsAttendanceTypesAssocMember = "l", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs3.ScsAttendanceEntityAssociation.Add(scsa6);
                var scsaNoDate = new StudentCourseSecScsAttendance() { ScsAttendanceTypesAssocMember = "L", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs3.ScsAttendanceEntityAssociation.Add(scsaNoDate);
                studentCourseSecs.Add(scs3);

                // Fourth StudentCourseSec has 1 attendances - 1 for today - and 1 bad data (no type or comment)
                var scs4 = new StudentCourseSec()
                {
                    Recordkey = "scs4",
                    ScsCourseSection = "section1",
                    ScsStudent = "student4",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa7 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today, ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs4.ScsAttendanceEntityAssociation.Add(scsa7);
                var scsaNoType = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today, ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs4.ScsAttendanceEntityAssociation.Add(scsaNoType);
                studentCourseSecs.Add(scs4);

                // Fifth StudentCourseSec has 3 attendances - all using minutes, 1 for today, 2 in the past
                var scs5 = new StudentCourseSec()
                {
                    Recordkey = "scs5",
                    ScsCourseSection = "section4",
                    ScsStudent = "student5",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa8 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-11), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceMinutesAssocMember = 60, ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs5.ScsAttendanceEntityAssociation.Add(scsa8);
                var scsa9 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-6), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceMinutesAssocMember = 45, ScsAttendanceReasonAssocMember = "Reason2", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-3), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs5.ScsAttendanceEntityAssociation.Add(scsa9);
                var scsa10 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-6), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceMinutesAssocMember = 30, ScsAttendanceReasonAssocMember = "Reason3", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs5.ScsAttendanceEntityAssociation.Add(scsa10);
                var scsa11 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-1), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceMinutesAssocMember = null, ScsAttendanceReasonAssocMember = "Reason3", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs5.ScsAttendanceEntityAssociation.Add(scsa11);
                studentCourseSecs.Add(scs5);

                return studentCourseSecs;
            }

            private Collection<StudentCourseSec> BuildCulmulativeTotalsStudentCourseSecResponse()
            {
                Collection<StudentCourseSec> studentCourseSecs = new Collection<StudentCourseSec>();

                var studentCourseSec = new StudentCourseSec()
                {
                    Recordkey = "scs6",
                    ScsCourseSection = "section6",
                    ScsStudent = "student6",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };

                //add attendances record with date, comment and a status
                var studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-11),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = "P",
                    ScsAttendanceReasonAssocMember = "Comment with attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-11).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-11),

                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and empty status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-2),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = string.Empty,
                    ScsAttendanceReasonAssocMember = "Comment without attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-2).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-2),
                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and null status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-1),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = null,
                    ScsAttendanceReasonAssocMember = "Comment without attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-1).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-1),
                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);
                studentCourseSecs.Add(studentCourseSec);

                studentCourseSec = new StudentCourseSec()
                {
                    Recordkey = "scs7",
                    ScsCourseSection = "section7",
                    ScsStudent = "student7",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };

                //add attendances record with date, comment and a P status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-11),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = "P",
                    ScsAttendanceReasonAssocMember = "Present attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-11).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-11),
                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and a L status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-10),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = "L",
                    ScsAttendanceReasonAssocMember = "Late attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-10).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-10),
                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and a l status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-9),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = "l",
                    ScsAttendanceReasonAssocMember = "late attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-9).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-9),
                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and a E status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-8),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = "E",
                    ScsAttendanceReasonAssocMember = "Excused attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-8).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-8),
                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and a A status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-7),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = "A",
                    ScsAttendanceReasonAssocMember = "Absent attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-7).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-7),
                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and a p status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-6),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = "p",
                    ScsAttendanceReasonAssocMember = "present attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-6).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-6),
                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and a null status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-5),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = null,
                    ScsAttendanceReasonAssocMember = "null attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-5).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-5),

                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and a blank status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-4),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = string.Empty,
                    ScsAttendanceReasonAssocMember = "blank attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-4).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-4),

                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);
                studentCourseSecs.Add(studentCourseSec);

                studentCourseSec = new StudentCourseSec()
                {
                    Recordkey = "scs8",
                    ScsCourseSection = "section8",
                    ScsStudent = "student8",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };

                //add attendances record with null absent date, comment and a status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = null,
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = "P",
                    ScsAttendanceReasonAssocMember = "Comment with attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-11).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-11),

                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);

                //add attendances record with date, comment and empty status
                studentCourseSecAttendance = new StudentCourseSecScsAttendance()
                {
                    ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-2),
                    ScsAttendanceInstrMethodsAssocMember = "LEC",
                    ScsAttendanceTypesAssocMember = string.Empty,
                    ScsAttendanceReasonAssocMember = "Comment without attendance status",
                    ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-2).AddHours(-1),
                    ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-2),
                };
                studentCourseSec.ScsAttendanceEntityAssociation.Add(studentCourseSecAttendance);
                studentCourseSecs.Add(studentCourseSec);

                return studentCourseSecs;
            }

            private StudentAttendanceRepository BuildValidStudentAttendanceRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(scsIds.ToArray()));
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(Task.FromResult(studentCourseSecResponseData));

                StudentAttendanceRepository repository = new StudentAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentAttendanceRepository BuildCumulativeTotalsStudentAttendanceRepository()
            {
                var recordKey = scsIds.ElementAt(0);
                Collection<StudentCourseSec> responseData = new Collection<StudentCourseSec>(studentCourseSecResponseData.Where(key => key.Recordkey == recordKey).ToList());

                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(scsIds.ToArray()));
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(Task.FromResult(responseData));

                StudentAttendanceRepository repository = new StudentAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentAttendanceRepository BuildInvalidStudentAttendanceRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(scsIds.ToArray()));
                Exception expectedFailure = new Exception("fail");
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Throws(expectedFailure);

                StudentAttendanceRepository repository = new StudentAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return repository;
            }
        }

        [TestClass]
        public class StudentAttendanceRepository_UpdateStudentAttendanceAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> mockManager;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            UpdateStudentAttendanceRequest updatedTranscationRequest;
            List<string> queryIds;
            IEnumerable<string> scsIds;
            DateTime meetingDate;
            DateTime? startTime;
            DateTime? endTime;
            DateTimeOffset? startTimeOffset;
            DateTimeOffset? endTimeOffset;
            string sectionId;
            IEnumerable<SectionMeetingInstance> meetingInstances;
            StudentAttendance studentAttendanceToUpdate;
            Collection<StudentCourseSec> studentCourseSecResponseData;
            StudentAttendanceRepository StudentAttendanceRepository;


            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                meetingDate = new DateTime(2018, 1, 5);
                startTime = new DateTime(2018, 1, 5, 9, 0, 0);
                endTime = new DateTime(2018, 1, 5, 10, 0, 0);

                var colleagueTimeZone = TimeZoneInfo.Local.Id;
                startTimeOffset = startTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                endTimeOffset = endTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone);

                sectionId = "sectionId";
                studentAttendanceToUpdate = new StudentAttendance("studentId", sectionId, meetingDate, "P", null, "This is the comment")
                {
                    InstructionalMethod = "LEC",
                    StartTime = startTimeOffset,
                    EndTime = endTimeOffset,
                    StudentCourseSectionId = "studentCourseSec1"
                };

                meetingInstances = new List<SectionMeetingInstance>()
                {
                new SectionMeetingInstance("111", sectionId, new DateTime(2018, 1, 1), startTimeOffset, endTimeOffset) { InstructionalMethod = "LEC" },
                new SectionMeetingInstance("222", sectionId, new DateTime(2018, 1, 3), startTimeOffset, endTimeOffset),
                new SectionMeetingInstance("333", sectionId, meetingDate, startTimeOffset, endTimeOffset) { InstructionalMethod = "LEC" }
                };

                queryIds = new List<string>() { "section1" };
                // Collection of data accessor responses
                studentCourseSecResponseData = BuildStudentCourseSecResponse(studentAttendanceToUpdate);
                scsIds = studentCourseSecResponseData.Select(ss => ss.Recordkey);
                StudentAttendanceRepository = BuildValidStudentAttendanceRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
                studentCourseSecResponseData = null;
                StudentAttendanceRepository = null;
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfStudentAttendanceNull()
            {
                var studentAttendances = await StudentAttendanceRepository.UpdateStudentAttendanceAsync(null, meetingInstances);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionIfSectionMeetingInstancesArgumentNull()
            {
                var studentAttendances = await StudentAttendanceRepository.UpdateStudentAttendanceAsync(studentAttendanceToUpdate, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionIfSectionMeetingInstancesArgumentEmpty()
            {
                var studentAttendances = await StudentAttendanceRepository.UpdateStudentAttendanceAsync(studentAttendanceToUpdate, new List<SectionMeetingInstance>());
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionIfSectionMeetingInstanceNotFound()
            {
                // Date Times don't match any in meeting list
                StudentAttendance stTest = new Domain.Student.Entities.StudentAttendance("studentId", "section1", meetingDate, "P", null)
                {
                    InstructionalMethod = "LEC",
                };
                var studentAttendances = await StudentAttendanceRepository.UpdateStudentAttendanceAsync(stTest, meetingInstances);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateStudentAttendance_TransactionReturnsErrorFlag()
            {
                var response = new UpdateStudentAttendanceResponse()
                {
                    ErrorFlag = true
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<UpdateStudentAttendanceRequest>(req => updatedTranscationRequest = req);

                var sa = await StudentAttendanceRepository.UpdateStudentAttendanceAsync(studentAttendanceToUpdate, meetingInstances);
            }

            [TestMethod]
            [ExpectedException(typeof(RecordLockException))]
            public async Task UpdateStudentAttendance_TransactionReturnsLockedError()
            {
                var response = new UpdateStudentAttendanceResponse()
                {
                    ErroredStudentAttendances = new List<ErroredStudentAttendances>()
                    {
                        new ErroredStudentAttendances() { ErroredStudentCourseSecIds = "StudentCourseSec1", ErrorMessages = "Something is locked and cannot be updated" }
                    }
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<UpdateStudentAttendanceRequest>(req => updatedTranscationRequest = req);

                var sa = await StudentAttendanceRepository.UpdateStudentAttendanceAsync(studentAttendanceToUpdate, meetingInstances);
            }
            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateStudentAttendance_TransactionReturnsOtherIndividualError()
            {
                var response = new UpdateStudentAttendanceResponse()
                {
                    ErroredStudentAttendances = new List<ErroredStudentAttendances>()
                    {
                        new ErroredStudentAttendances() { ErroredStudentCourseSecIds = "StudentCourseSec1", ErrorMessages = "Something else" }
                    }
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<UpdateStudentAttendanceRequest>(req => updatedTranscationRequest = req);

                var sa = await StudentAttendanceRepository.UpdateStudentAttendanceAsync(studentAttendanceToUpdate, meetingInstances);
            }


            // Commented out the following because the timezone's do not match (for some reason) so after update it cannot find a match.... ????

            [TestMethod]
            public async Task UpdateStudentAttendance_SuccessfulUpdate()
            {
                var sa = await StudentAttendanceRepository.UpdateStudentAttendanceAsync(studentAttendanceToUpdate, meetingInstances);
                Assert.AreEqual(studentAttendanceToUpdate.SectionId, sa.SectionId);
            }


            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionIfTransactionReturnsException()
            {
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Throws(new Exception());
                var studentAttendances = await StudentAttendanceRepository.UpdateStudentAttendanceAsync(studentAttendanceToUpdate, meetingInstances);
            }


            private Collection<StudentCourseSec> BuildStudentCourseSecResponse(StudentAttendance sa)
            {
                Collection<StudentCourseSec> studentCourseSecs = new Collection<StudentCourseSec>();
                // First StudentCourseSec has 3 attendances - 1 for today and 2 others
                var scs1 = new StudentCourseSec()
                {
                    Recordkey = sa.StudentCourseSectionId,
                    ScsCourseSection = sa.SectionId,
                    ScsStudent = sa.StudentId,
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa1 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-10), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa1);
                var scsa2 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-5), ScsAttendanceTypesAssocMember = "l", ScsAttendanceReasonAssocMember = "Reason2", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa2);
                var scsa3 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = sa.MeetingDate, ScsAttendanceInstrMethodsAssocMember = sa.InstructionalMethod, ScsAttendanceTypesAssocMember = sa.AttendanceCategoryCode, ScsAttendanceReasonAssocMember = sa.Comment, ScsAttendanceStartTimesAssocMember = startTime, ScsAttendanceEndTimesAssocMember = endTime };
                scs1.ScsAttendanceEntityAssociation.Add(scsa3);
                studentCourseSecs.Add(scs1);

                return studentCourseSecs;
            }


            private StudentAttendanceRepository BuildValidStudentAttendanceRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                localCacheMock = new Mock<ObjectCache>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Mock selection of student course sec id - when one is not provided in the incoming StudentAttendance.
                string[] scsIds = new List<string>() { "studentCourseSec1" }.ToArray();
                dataAccessorMock.Setup(acc => acc.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string>())).Returns(Task.FromResult(scsIds.ToArray()));

                // Successful response for an update
                var response = new UpdateStudentAttendanceResponse()
                {
                    UpdatedStudentCourseSecId = new List<string>() { "studentCourseSec1" },
                    ErrorFlag = false

                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<UpdateStudentAttendanceRequest>(req => updatedTranscationRequest = req);

                // Mock responses needed to get the newly updated StudentAttendance item....
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(scsIds.ToArray()));
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(Task.FromResult(studentCourseSecResponseData));


                StudentAttendanceRepository repository = new StudentAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }


        }

        [TestClass]
        public class StudentAttendanceRepository_UpdateSectionAttendanceAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> mockManager;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            UpdateStudentAttendanceRequest updatedTranscationRequest;
            List<string> queryIds;
            IEnumerable<string> scsIds;
            List<string> crossListIds;
            DateTime meetingDate;
            DateTime? startTime;
            DateTime? endTime;
            DateTimeOffset? startTimeOffset;
            DateTimeOffset? endTimeOffset;
            string sectionId;
            SectionMeetingInstance meetingInstance;
            IEnumerable<SectionMeetingInstance> meetingInstances;
            SectionAttendance sectionAttendance;
            Collection<StudentCourseSec> studentCourseSecResponseData;
            StudentAttendanceRepository StudentAttendanceRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                meetingDate = new DateTime(2018, 1, 5);
                startTime = new DateTime(2018, 1, 5, 9, 0, 0);
                endTime = new DateTime(2018, 1, 5, 10, 0, 0);

                var colleagueTimeZone = TimeZoneInfo.Local.Id;
                startTimeOffset = startTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                endTimeOffset = endTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone);

                sectionId = "sectionId";
                crossListIds = new List<string>() { "section2", "section3" };
                meetingInstance = new SectionMeetingInstance("333", sectionId, meetingDate, startTimeOffset, endTimeOffset) { InstructionalMethod = "LEC" };
                sectionAttendance = new SectionAttendance(sectionId, meetingInstance);
                sectionAttendance.AddStudentSectionAttendance(new StudentSectionAttendance("studentCourseSec1", "A", null, "Reason for absense. "));
                sectionAttendance.AddStudentSectionAttendance(new StudentSectionAttendance("studentCourseSec2", null, null, "Reason for absense. "));
                sectionAttendance.AddStudentSectionAttendance(new StudentSectionAttendance("studentCourseSec3", "P", null, null));

                meetingInstances = new List<SectionMeetingInstance>()
                {
                new SectionMeetingInstance("111", sectionId, new DateTime(2018, 1, 1), startTimeOffset, endTimeOffset) { InstructionalMethod = "LEC" },
                new SectionMeetingInstance("222", sectionId, new DateTime(2018, 1, 3), startTimeOffset, endTimeOffset),
                meetingInstance
                };

                queryIds = new List<string>() { "section1" };
                // Collection of data accessor responses
                studentCourseSecResponseData = BuildStudentCourseSecResponse(sectionAttendance);
                scsIds = studentCourseSecResponseData.Select(ss => ss.Recordkey);
                StudentAttendanceRepository = BuildValidStudentAttendanceRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
                StudentAttendanceRepository = null;
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsException_SectionAttendanceNull()
            {
                var studentAttendances = await StudentAttendanceRepository.UpdateSectionAttendanceAsync(null, crossListIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsException_NoStudentSectionAttencances()
            {
                var invalidSectionAttendance = new SectionAttendance(sectionId, meetingInstance);
                var studentAttendances = await StudentAttendanceRepository.UpdateSectionAttendanceAsync(invalidSectionAttendance, crossListIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateSectionAttendanceAsync_TransactionReturnsErrorFlag()
            {
                var response = new UpdateStudentAttendanceResponse()
                {
                    ErrorFlag = true,
                    UpdatedStudentCourseSecId = new List<string>() { "studentCourseSec1" }
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<UpdateStudentAttendanceRequest>(req => updatedTranscationRequest = req);

                var sa = await StudentAttendanceRepository.UpdateSectionAttendanceAsync(sectionAttendance, crossListIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfTransactionReturnsException()
            {
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Throws(new Exception());
                var sa = await StudentAttendanceRepository.UpdateSectionAttendanceAsync(sectionAttendance, crossListIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateSectionAttendanceAsync_UpdatedStudentCourseSecIdEmpty()
            {
                var response = new UpdateStudentAttendanceResponse()
                {
                    ErrorFlag = false,
                    UpdatedStudentCourseSecId = new List<string>()
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<UpdateStudentAttendanceRequest>(req => updatedTranscationRequest = req);

                var sa = await StudentAttendanceRepository.UpdateSectionAttendanceAsync(sectionAttendance, crossListIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateSectionAttendanceAsync_UpdatedStudentCourseSecIdNull()
            {
                var response = new UpdateStudentAttendanceResponse()
                {
                    ErrorFlag = false
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<UpdateStudentAttendanceRequest>(req => updatedTranscationRequest = req);

                var sa = await StudentAttendanceRepository.UpdateSectionAttendanceAsync(sectionAttendance, crossListIds);
            }

            [TestMethod]
            public async Task UpdateSectionAttendance_ReturnedValidResponse()
            {
                var response = new UpdateStudentAttendanceResponse()
                {
                    ErrorFlag = false,
                    UpdatedStudentCourseSecId = new List<string>() { "studentCourseSec1", null, "notFoundId" },
                    ErroredStudentAttendances = new List<ErroredStudentAttendances>()
                     {
                         new ErroredStudentAttendances() { ErrorMessages = "Invalid Error - no corresponding student course sec Id" },
                         null,
                         new ErroredStudentAttendances() {  ErroredStudentCourseSecIds = "studentCourseSec2", ErrorMessages = "Some error." },
                     }
                };
                // Make sure the proper request is sent in.
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => r.SectionId == sectionId && r.StudentAttendances != null && r.CalendarSchedulesId == meetingInstance.Id))).ReturnsAsync(response).Callback<UpdateStudentAttendanceRequest>(req => updatedTranscationRequest = req);
                // This tests a valid response is built
                // Should have 1 successfully updated item that is Id "studentCourseSec1". The null should be ignored. The one that can't be found should be an error.
                // Should ignore the one error that has no student Course sec Id but include the Updated item that has an Id of notFound - hence 2 error.

                var sa = await StudentAttendanceRepository.UpdateSectionAttendanceAsync(sectionAttendance, crossListIds);
                Assert.IsInstanceOfType(sa, typeof(SectionAttendanceResponse));
                Assert.AreEqual(sectionId, sa.SectionId);
                Assert.AreEqual(1, sa.UpdatedStudentAttendances.Count());
                Assert.AreEqual(1, sa.StudentAttendanceErrors.Count());
                Assert.AreEqual(1, sa.StudentCourseSectionsWithDeletedAttendances.Count());
                // Verify the attendance category of the successful one to be sure it pulled the correct student attendance for the correct meeting date and time.
                var updatedSA = sa.UpdatedStudentAttendances.ElementAt(0);
                Assert.AreEqual("A", updatedSA.AttendanceCategoryCode);
            }


            private Collection<StudentCourseSec> BuildStudentCourseSecResponse(SectionAttendance sa)
            {
                Collection<StudentCourseSec> studentCourseSecs = new Collection<StudentCourseSec>();
                var updatedSSA = sa.StudentAttendances.Where(ssa => ssa.StudentCourseSectionId == "studentCourseSec1").FirstOrDefault();
                // studentCourseSec1 has 4 attendances - 2 for the meeting instance meeting date (one with correct time and one for an earlier time) , and 2 for other dates
                var scs1 = new StudentCourseSec()
                {
                    Recordkey = "studentCourseSec1",
                    ScsCourseSection = sa.SectionId,
                    ScsStudent = "studentId",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa1 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-10), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa1);
                var scsa2 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-5), ScsAttendanceTypesAssocMember = "l", ScsAttendanceReasonAssocMember = "Reason2", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa2);
                var scsa3 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = sa.MeetingInstance.MeetingDate, ScsAttendanceInstrMethodsAssocMember = sa.MeetingInstance.InstructionalMethod, ScsAttendanceTypesAssocMember = "L", ScsAttendanceReasonAssocMember = updatedSSA.Comment, ScsAttendanceStartTimesAssocMember = startTime.Value.AddHours(-3), ScsAttendanceEndTimesAssocMember = endTime };
                scs1.ScsAttendanceEntityAssociation.Add(scsa3);
                var scsa4 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = sa.MeetingInstance.MeetingDate, ScsAttendanceInstrMethodsAssocMember = sa.MeetingInstance.InstructionalMethod, ScsAttendanceTypesAssocMember = updatedSSA.AttendanceCategoryCode, ScsAttendanceReasonAssocMember = updatedSSA.Comment, ScsAttendanceStartTimesAssocMember = startTime, ScsAttendanceEndTimesAssocMember = endTime };
                scs1.ScsAttendanceEntityAssociation.Add(scsa4);
                studentCourseSecs.Add(scs1);
                var scs2 = new StudentCourseSec()
                {
                    Recordkey = "other",
                    ScsCourseSection = "section2",
                    ScsStudent = "studentId2",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa5 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = sa.MeetingInstance.MeetingDate, ScsAttendanceInstrMethodsAssocMember = sa.MeetingInstance.InstructionalMethod, ScsAttendanceTypesAssocMember = updatedSSA.AttendanceCategoryCode, ScsAttendanceReasonAssocMember = updatedSSA.Comment, ScsAttendanceStartTimesAssocMember = startTime, ScsAttendanceEndTimesAssocMember = endTime };
                scs2.ScsAttendanceEntityAssociation.Add(scsa5);
                studentCourseSecs.Add(scs2);
                return studentCourseSecs;
            }


            private StudentAttendanceRepository BuildValidStudentAttendanceRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                localCacheMock = new Mock<ObjectCache>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Mock selection of student course sec id - when one is not provided in the incoming StudentAttendance.
                string[] scsIds = new List<string>() { "studentCourseSec1" }.ToArray();
                dataAccessorMock.Setup(acc => acc.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string>())).Returns(Task.FromResult(scsIds.ToArray()));

                // Successful response for an update
                //var response = new UpdateStudentAttendanceResponse()
                //{
                //    UpdatedStudentCourseSecId = new List<string>() { "studentCourseSec1", "studentCourseSec3" },
                //    ErroredStudentAttendances = new List<string>()
                //    ErrorFlag = false

                //};
                //mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(It.Is<UpdateStudentAttendanceRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<UpdateStudentAttendanceRequest>(req => updatedTranscationRequest = req);

                // Mock responses needed to get the newly updated StudentAttendance item.
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(scsIds.ToArray()));
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(Task.FromResult(studentCourseSecResponseData));

                StudentAttendanceRepository repository = new StudentAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }


        }

        [TestClass]
        public class StudentAttendanceRepository_GetStudentSectionsAttendancesAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            List<string> queryIds;
            List<string> allScsIds;
            List<string> fewScsIds;
            Collection<StudentCourseSec> studentCourseSecResponseData;
            Collection<StudentCourseSec> fewStudentCourseSecResponseData = new Collection<StudentCourseSec>();
            StudentAttendanceRepository StudentAttendanceRepository;


            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                queryIds = new List<string>() { "section1", "section2", "section3" };
                // Collection of data accessor responses
                studentCourseSecResponseData = BuildStudentCourseSecResponse();
                //retrieve all course.sec records ids
                allScsIds = studentCourseSecResponseData.Select(ss => ss.Recordkey).ToList();
                //retrieve only selected course.sec.ids
                fewScsIds = studentCourseSecResponseData.Where(s => queryIds.Contains(s.ScsCourseSection)).Select(a => a.Recordkey).ToList();
                List<StudentCourseSec> selectedCourseSec = studentCourseSecResponseData.Where(s => queryIds.Contains(s.ScsCourseSection)).ToList();
                foreach (var courseSec in selectedCourseSec)
                {
                    fewStudentCourseSecResponseData.Add(courseSec);
                }


                StudentAttendanceRepository = BuildValidStudentAttendanceRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentCourseSecResponseData = null;
                StudentAttendanceRepository = null;
            }

            [TestMethod]
            public async Task SectionIds_NotInrepository_ReturnsEmptyList()
            {
                // Call Section Repository with a non-existant section
                var nonExistingIds = new List<string>() { "notInListId" };
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(new List<string>().ToArray()));
                var studentAttendances = await StudentAttendanceRepository.GetStudentSectionAttendancesAsync("student1", nonExistingIds);
                Assert.AreEqual("student1", studentAttendances.StudentId);
                Assert.AreEqual(0, studentAttendances.SectionWiseAttendances.Count);

            }

            [TestMethod]
            public async Task SectionIds_NotInrepository_ReturnsNull()
            {
                // Call Section Repository with a non-existant section
                var nonExistingIds = new List<string>() { "notInListId" };
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(() => null);
                var studentAttendances = await StudentAttendanceRepository.GetStudentSectionAttendancesAsync("student1", nonExistingIds);
                Assert.AreEqual("student1", studentAttendances.StudentId);
                Assert.AreEqual(0, studentAttendances.SectionWiseAttendances.Count);

            }
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ThrowsException_IfRepositoryReturnsException()
            {
                StudentAttendanceRepository StudentAttendanceRepository = BuildInvalidStudentAttendanceRepository();
                var StudentAttendances = await StudentAttendanceRepository.GetStudentSectionAttendancesAsync("student1", queryIds);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfStudentIdArgumentNull()
            {
                var studentAttendances = await StudentAttendanceRepository.GetStudentSectionAttendancesAsync(null, queryIds);
            }




            [TestMethod]
            public async Task RetrieveAllSectionsAttendanceForAStudent_SectionIdsAreNull()
            {
                var studentAttendances = await StudentAttendanceRepository.GetStudentSectionAttendancesAsync("student1", null);
                Assert.IsNotNull(studentAttendances);
                Assert.AreEqual("student1", studentAttendances.StudentId);
                //will not build sectiona ttendace when student attendance does not exist for student course sec record.
                Assert.AreEqual(allScsIds.Count() - 1, studentAttendances.SectionWiseAttendances.Count);
            }

            [TestMethod]
            public async Task RetrievOnlyFewSectionsAttendanceForAStudent_SectionIdsAsQueryPassed()
            {
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(Task.FromResult(fewStudentCourseSecResponseData));
                var studentAttendances = await StudentAttendanceRepository.GetStudentSectionAttendancesAsync("student1", queryIds);
                Assert.IsNotNull(studentAttendances);
                Assert.AreEqual("student1", studentAttendances.StudentId);
                Assert.AreEqual(fewScsIds.Count(), studentAttendances.SectionWiseAttendances.Count);
            }

            [TestMethod]
            public async Task Validate_Correct_DataInCollection()
            {
                var studentAttendances = await StudentAttendanceRepository.GetStudentSectionAttendancesAsync("student1", null);
                Assert.AreEqual("student1", studentAttendances.StudentId);
                //will not build sectiona ttendace when student attendance does not exist for student course sec record.
                Assert.AreEqual(allScsIds.Count() - 1, studentAttendances.SectionWiseAttendances.Count);
                //first section attendance
                Assert.AreEqual(3, studentAttendances.SectionWiseAttendances["section1"].Count);
                Assert.AreEqual("P", studentAttendances.SectionWiseAttendances["section1"][0].AttendanceCategoryCode);
                Assert.AreEqual("L", studentAttendances.SectionWiseAttendances["section1"][1].AttendanceCategoryCode);
                Assert.AreEqual("E", studentAttendances.SectionWiseAttendances["section1"][2].AttendanceCategoryCode);
                //2nd section attendance
                Assert.AreEqual(1, studentAttendances.SectionWiseAttendances["section2"].Count);
                Assert.AreEqual("P", studentAttendances.SectionWiseAttendances["section2"][0].AttendanceCategoryCode);
                //3rd section attendance
                Assert.AreEqual(2, studentAttendances.SectionWiseAttendances["section3"].Count);
                Assert.AreEqual("P", studentAttendances.SectionWiseAttendances["section3"][0].AttendanceCategoryCode);
                Assert.AreEqual("l", studentAttendances.SectionWiseAttendances["section3"][1].AttendanceCategoryCode);
                //4th section attendance
                Assert.AreEqual(2, studentAttendances.SectionWiseAttendances["section4"].Count);
                Assert.AreEqual("P", studentAttendances.SectionWiseAttendances["section4"][0].AttendanceCategoryCode);
                Assert.AreEqual(null, studentAttendances.SectionWiseAttendances["section4"][1].AttendanceCategoryCode);
                //5th section attendance
                Assert.AreEqual(4, studentAttendances.SectionWiseAttendances["section5"].Count);
                Assert.AreEqual(135, studentAttendances.SectionWiseAttendances["section5"][0].CumulativeMinutesAttended);
                Assert.AreEqual(135, studentAttendances.SectionWiseAttendances["section5"][1].CumulativeMinutesAttended);
                Assert.AreEqual(135, studentAttendances.SectionWiseAttendances["section5"][2].CumulativeMinutesAttended);
                Assert.AreEqual(135, studentAttendances.SectionWiseAttendances["section5"][2].CumulativeMinutesAttended);

            }



            private Collection<StudentCourseSec> BuildStudentCourseSecResponse()
            {
                Collection<StudentCourseSec> studentCourseSecs = new Collection<StudentCourseSec>();

                // First StudentCourseSec has 3 attendances - 1 for today and 2 others
                var scs1 = new StudentCourseSec()
                {
                    Recordkey = "scs1",
                    ScsCourseSection = "section1",
                    ScsStudent = "student1",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa1 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-10), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa1);
                var scsa2 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-5), ScsAttendanceTypesAssocMember = "L", ScsAttendanceReasonAssocMember = "Reason2", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa2);
                var scsa3 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today, ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "E", ScsAttendanceReasonAssocMember = "Reason3", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs1.ScsAttendanceEntityAssociation.Add(scsa3);
                studentCourseSecs.Add(scs1);

                // Second StudentCourseSec has 1 attendances - 0 for today and 1 other
                var scs2 = new StudentCourseSec()
                {
                    Recordkey = "scs2",
                    ScsCourseSection = "section2",
                    ScsStudent = "student1",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa4 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-10), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs2.ScsAttendanceEntityAssociation.Add(scsa4);
                studentCourseSecs.Add(scs2);

                // Third StudentCourseSec has 3 attendances - 1 for today and 1 other and 1 bad data that will not convert (no date)
                var scs3 = new StudentCourseSec()
                {
                    Recordkey = "scs3",
                    ScsCourseSection = "section3",
                    ScsStudent = "student1",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa5 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-10), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceReasonAssocMember = "Reason1" };
                scs3.ScsAttendanceEntityAssociation.Add(scsa5);
                var scsa6 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today, ScsAttendanceTypesAssocMember = "l", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs3.ScsAttendanceEntityAssociation.Add(scsa6);
                var scsaNoDate = new StudentCourseSecScsAttendance() { ScsAttendanceTypesAssocMember = "L", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs3.ScsAttendanceEntityAssociation.Add(scsaNoDate);
                studentCourseSecs.Add(scs3);

                // Fourth StudentCourseSec has 1 attendances - 1 for today - and 1 bad data (no type or comment)
                var scs4 = new StudentCourseSec()
                {
                    Recordkey = "scs4",
                    ScsCourseSection = "section4",
                    ScsStudent = "student1",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa7 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today, ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceTypesAssocMember = "P", ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs4.ScsAttendanceEntityAssociation.Add(scsa7);
                var scsaNoType = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today, ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs4.ScsAttendanceEntityAssociation.Add(scsaNoType);
                studentCourseSecs.Add(scs4);

                // Fifth StudentCourseSec has 3 attendances - all using minutes, 1 for today, 2 in the past
                var scs5 = new StudentCourseSec()
                {
                    Recordkey = "scs5",
                    ScsCourseSection = "section5",
                    ScsStudent = "student1",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };
                var scsa8 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-11), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceMinutesAssocMember = 60, ScsAttendanceReasonAssocMember = "Reason1", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs5.ScsAttendanceEntityAssociation.Add(scsa8);
                var scsa9 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-6), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceMinutesAssocMember = 45, ScsAttendanceReasonAssocMember = "Reason2", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-3), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs5.ScsAttendanceEntityAssociation.Add(scsa9);
                var scsa10 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-6), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceMinutesAssocMember = 30, ScsAttendanceReasonAssocMember = "Reason3", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs5.ScsAttendanceEntityAssociation.Add(scsa10);
                var scsa11 = new StudentCourseSecScsAttendance() { ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-1), ScsAttendanceInstrMethodsAssocMember = "LEC", ScsAttendanceMinutesAssocMember = null, ScsAttendanceReasonAssocMember = "Reason3", ScsAttendanceStartTimesAssocMember = DateTime.Now.AddHours(-1), ScsAttendanceEndTimesAssocMember = DateTime.Now };
                scs5.ScsAttendanceEntityAssociation.Add(scsa11);
                studentCourseSecs.Add(scs5);

                // Sixth StudentCourseSec that doesn't have any attendances
                var scs6 = new StudentCourseSec()
                {
                    Recordkey = "scs6",
                    ScsCourseSection = "section6",
                    ScsStudent = "student1",
                    ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                };

                studentCourseSecs.Add(scs6);



                return studentCourseSecs;
            }


            private StudentAttendanceRepository BuildValidStudentAttendanceRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                //data accessor when studentId and sectionIds are passsed -- this will return few
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(fewScsIds.ToArray()));

                //data accessor when only studentId is passed and list of section ids are null -- This will return all
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(allScsIds.ToArray()));

                //to retrueve all stucdent course sec records
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(Task.FromResult(studentCourseSecResponseData));

                StudentAttendanceRepository repository = new StudentAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentAttendanceRepository BuildInvalidStudentAttendanceRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                dataAccessorMock.Setup(sacc => sacc.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(allScsIds.ToArray()));
                Exception expectedFailure = new Exception("fail");
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Throws(expectedFailure);

                StudentAttendanceRepository repository = new StudentAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return repository;
            }


        }
    }
}


