// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using System.Threading;
using Ellucian.Colleague.Domain.Base.Transactions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
            public class SectionRegistrationRepositoryTests : BaseRepositorySetup
    {
        protected void MainInitialize()
        {
            base.MockInitialize();

        }

        [TestClass]
        public class GetSectionRegistrationTests
        {
            SectionRegistrationRepository sectionRegistrationRepo;
            List<AcademicCredit> academicCreditData;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;

            [TestInitialize]
            public void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationRepo = null;
                academicCreditData = null;
                transFactoryMock = null;
                loggerMock = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
            }

            [TestMethod]
            public async Task RegistrationResponse_PropertiesValid()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.GetAsync(guid);
                Assert.AreEqual(studentAcadCred.RecordGuid, registrationResponse.Guid);
                Assert.AreEqual(studentAcadCred.StcPersonId, registrationResponse.StudentId);
                Assert.AreEqual(studentCourseSec.ScsCourseSection, registrationResponse.SectionId);
                Assert.AreEqual(studentCourseSec.ScsPassAudit, registrationResponse.PassAudit);
            }

            [TestMethod]
            public async Task SectionRegistrationRespository_GetSectionRegistrationsAsync()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                var studentAcadCredIdsList = new string[] { studentAcadCred.Recordkey };
                dataAccessorMock.Setup(acc => acc.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string>())).ReturnsAsync(studentAcadCredIdsList);
                
                dataAccessorMock.Setup(
                    acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<StudentAcadCred>() {studentAcadCred});

                dataAccessorMock.Setup(
                   acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), true))
                   .ReturnsAsync(new Collection<StudentCourseSec>() { studentCourseSec });

                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponses = await sectionRegistrationRepo.GetSectionRegistrationsAsync(0, 10, "", "");
                var registrationResponse = registrationResponses.Item1.FirstOrDefault();
                Assert.IsNotNull(registrationResponse);
                Assert.AreEqual(studentAcadCred.RecordGuid, registrationResponse.Guid);
                Assert.AreEqual(studentAcadCred.StcPersonId, registrationResponse.StudentId);
                Assert.AreEqual(studentCourseSec.ScsCourseSection, registrationResponse.SectionId);
                Assert.AreEqual(studentCourseSec.ScsPassAudit, registrationResponse.PassAudit);

            }

            [TestMethod]
            public async Task RegistrationAudit_PropertiesValid()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.Where(a => a.GradingType == GradingType.Audit).FirstOrDefault();
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.GetAsync(guid);
                Assert.AreEqual(studentAcadCred.RecordGuid, registrationResponse.Guid);
                Assert.AreEqual(studentAcadCred.StcPersonId, registrationResponse.StudentId);
                Assert.AreEqual(studentCourseSec.ScsCourseSection, registrationResponse.SectionId);
                Assert.AreEqual(studentCourseSec.ScsPassAudit, registrationResponse.PassAudit);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegistrationNoStc_ReturnsException()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.FirstOrDefault();
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(null);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.GetAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegistrationNoScs_ReturnsException()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.FirstOrDefault();
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(null);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.GetAsync(guid);
            }

            private SectionRegistrationRepository BuildValidSectionRegistrationRepository()
            {
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                sectionRegistrationRepo = new SectionRegistrationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return sectionRegistrationRepo;

            }

            private StudentAcadCred BuildValidStcResponse(AcademicCredit ac)
            {
                StudentAcadCred stc = new StudentAcadCred();

                stc.RecordGuid = Guid.NewGuid().ToString();
                stc.Recordkey = ac.Id;
                stc.StcPersonId = ac.Id;
                stc.StcAcadLevel = ac.AcademicLevelCode;
                stc.StcAllowReplFlag = ac.CanBeReplaced ? "Y" : "";
                stc.StcAltcumContribCmplCred = ac.AdjustedCredit;
                stc.StcAltcumContribGpaCred = ac.AdjustedGpaCredit;
                stc.StcAltcumContribGradePts = ac.AdjustedGradePoints;
                stc.StcCeus = ac.ContinuingEducationUnits;
                if (ac.Course != null)
                {
                    stc.StcCourse = ac.Course.Id;
                    stc.StcCourseLevel = ac.Course.CourseLevelCodes.First();
                }
                stc.StcCourseName = ac.CourseName;
                stc.StcCred = ac.Credit;
                string typecode;
                switch (ac.Type)
                {
                    case CreditType.ContinuingEducation:
                        typecode = "CE";
                        break;
                    case CreditType.Institutional:
                        typecode = "IN";
                        break;
                    case CreditType.Transfer:
                        typecode = "TRN";
                        break;
                    default:
                        typecode = "OTH";
                        break;
                }
                stc.StcCredType = typecode;
                // For one academic credit leave the departments blank and be sure they get defaulted from the course
                if (ac.Id == "1")
                {
                    stc.StcDepts = new List<string>();
                }
                else
                {
                    stc.StcDepts = ac.DepartmentCodes.ToList();
                }
                stc.StcStartDate = ac.StartDate;
                stc.StcEndDate = ac.EndDate;
                stc.StcGpaCred = ac.GpaCredit;
                stc.StcGradePts = ac.GradePoints;
                stc.StcGradeScheme = ac.GradeSchemeCode;
                stc.StcCmplCred = ac.CompletedCredit;
                stc.StcAttCred = ac.AttemptedCredit;
                stc.StcSectionNo = ac.SectionNumber;
                stc.StcStudentEquivEval = ac.CourseName;
                //stc.StcMark 
                stc.StcReplCode = ac.ReplacedStatus == ReplacedStatus.Replaced ? "R" : null;
                stc.StcRepeatedAcadCred = ac.RepeatAcademicCreditIds;

                // Status 
                string stat = "";
                switch (ac.Status)
                {
                    case CreditStatus.Add: { stat = "A"; break; }
                    case CreditStatus.Cancelled: { stat = "C"; break; }
                    case CreditStatus.Deleted: { stat = "X"; break; }
                    case CreditStatus.Dropped: { stat = "D"; break; }
                    case CreditStatus.New: { stat = "N"; break; }
                    case CreditStatus.Preliminary: { stat = "PR"; break; }
                    case CreditStatus.TransferOrNonCourse: { stat = "TR"; break; }
                    case CreditStatus.Withdrawn: { stat = "W"; break; }
                    default: { stat = ""; break; }
                }

                stc.StcStatus = new List<string>() { stat };

                stc.StcStatusesEntityAssociation = new List<StudentAcadCredStcStatuses>();
                StudentAcadCredStcStatuses statusitem = new StudentAcadCredStcStatuses(
                                                                DateTime.Now,
                                                                ac.Status.ToString()[0].ToString(),
                                                                DateTime.Now,
                                                                "");
                stc.StcStatusesEntityAssociation.Add(statusitem);
                stc.StcStudentCourseSec = ac.Id; // Not real life example here.  The SCS record would have a diff-
                // erent ID in real life, but we don't keep it here, so for mocking
                // we will pretend it is the same as the STC record ID.
                stc.StcSubject = ac.SubjectCode;
                stc.StcTerm = ac.TermCode;
                //stc.StcTitle I don't think we use this
                if (ac.VerifiedGrade != null)
                {
                    stc.StcVerifiedGrade = ac.VerifiedGrade.Id;
                    // added for mobile
                    stc.StcVerifiedGradeDate = new DateTime(ac.VerifiedGradeTimestamp.Value.Year, ac.VerifiedGradeTimestamp.Value.Month, ac.VerifiedGradeTimestamp.Value.Day, 0, 0, 0);
                    // end added for mobile
                }
                return stc;
            }

            private StudentCourseSec BuildValidScsResponse(AcademicCredit ac)
            {
                StudentCourseSec scs = new StudentCourseSec();
                scs.Recordkey = ac.Id;
                scs.ScsCourseSection = ac.SectionId;
                switch (ac.GradingType)
                {
                    case GradingType.Graded:
                        scs.ScsPassAudit = "";
                        break;
                    case GradingType.PassFail:
                        scs.ScsPassAudit = "P";
                        break;
                    case GradingType.Audit:
                        scs.ScsPassAudit = "A";
                        break;
                    default:
                        break;
                }
                // added for mobile
                // for each midtermgrade in the list, populate the correct field 
                // Not necessarily sequential starting from 1!!! Mock the time as
                // midnight, which we won't see if we get a time from the SCSCC
                foreach (MidTermGrade mtg in ac.MidTermGrades)
                {
                    switch (mtg.Position)
                    {
                        case 1:
                            scs.ScsMidTermGrade1 = mtg.GradeId;
                            scs.ScsMidGradeDate1 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 2:
                            scs.ScsMidTermGrade2 = mtg.GradeId;
                            scs.ScsMidGradeDate2 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 3:
                            scs.ScsMidTermGrade3 = mtg.GradeId;
                            scs.ScsMidGradeDate3 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 4:
                            scs.ScsMidTermGrade4 = mtg.GradeId;
                            scs.ScsMidGradeDate4 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 5:
                            scs.ScsMidTermGrade5 = mtg.GradeId;
                            scs.ScsMidGradeDate5 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 6:
                            scs.ScsMidTermGrade6 = mtg.GradeId;
                            scs.ScsMidGradeDate6 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                    }
                }
                // end added for mobile
                return scs;
            }
        }

        [TestClass]
        public class GetSectionRegistrationV7Tests
        {
            SectionRegistrationRepository sectionRegistrationRepo;
            List<AcademicCredit> academicCreditData;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;

            [TestInitialize]
            public void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationRepo = null;
                academicCreditData = null;
                transFactoryMock = null;
                loggerMock = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
            }

            [TestMethod]
            public async Task RegistrationResponseV7_PropertiesValid()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.GetAsync(guid);
                Assert.AreEqual(studentAcadCred.RecordGuid, registrationResponse.Guid);
                Assert.AreEqual(studentAcadCred.StcPersonId, registrationResponse.StudentId);
                Assert.AreEqual(studentCourseSec.ScsCourseSection, registrationResponse.SectionId);
                Assert.AreEqual(studentCourseSec.ScsPassAudit, registrationResponse.PassAudit);
                Assert.AreEqual(studentAcadCred.StcCredType, registrationResponse.CreditType);
                Assert.AreEqual(studentAcadCred.StcAcadLevel, registrationResponse.AcademicLevel);
                Assert.AreEqual(studentAcadCred.StcCeus, registrationResponse.Ceus);
                Assert.AreEqual(studentAcadCred.StcAttCred, registrationResponse.Credit);
                Assert.AreEqual(studentAcadCred.StcGradePts, registrationResponse.GradePoint);
                Assert.AreEqual(studentAcadCred.StcReplCode, registrationResponse.ReplCode);
                Assert.AreEqual(studentAcadCred.StcRepeatedAcadCred, registrationResponse.RepeatedAcadCreds);
                Assert.AreEqual(studentAcadCred.StcAltcumContribCmplCred, registrationResponse.AltcumContribCmplCred);
                Assert.AreEqual(studentAcadCred.StcAltcumContribGpaCred, registrationResponse.AltcumContribGpaCred);                
            }

            [TestMethod]
            public async Task SectionRegistrationRespositoryV7_GetSectionRegistrationsAsync()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                var studentAcadCredIdsList = new string[] { studentAcadCred.Recordkey };
                dataAccessorMock.Setup(acc => acc.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string>())).ReturnsAsync(studentAcadCredIdsList);

                dataAccessorMock.Setup(
                    acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<StudentAcadCred>() { studentAcadCred });

                dataAccessorMock.Setup(
                   acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), true))
                   .ReturnsAsync(new Collection<StudentCourseSec>() { studentCourseSec });

                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponses = await sectionRegistrationRepo.GetSectionRegistrationsAsync(0, 10, "", "");
                var registrationResponse = registrationResponses.Item1.FirstOrDefault();
                Assert.IsNotNull(registrationResponse);
                Assert.AreEqual(studentAcadCred.RecordGuid, registrationResponse.Guid);
                Assert.AreEqual(studentAcadCred.StcPersonId, registrationResponse.StudentId);
                Assert.AreEqual(studentCourseSec.ScsCourseSection, registrationResponse.SectionId);
                Assert.AreEqual(studentCourseSec.ScsPassAudit, registrationResponse.PassAudit);
                Assert.AreEqual(studentAcadCred.StcCredType, registrationResponse.CreditType);
                Assert.AreEqual(studentAcadCred.StcAcadLevel, registrationResponse.AcademicLevel);
                Assert.AreEqual(studentAcadCred.StcCeus, registrationResponse.Ceus);
                Assert.AreEqual(studentAcadCred.StcAttCred, registrationResponse.Credit);
                Assert.AreEqual(studentAcadCred.StcGradePts, registrationResponse.GradePoint);
                Assert.AreEqual(studentAcadCred.StcReplCode, registrationResponse.ReplCode);
                Assert.AreEqual(studentAcadCred.StcRepeatedAcadCred, registrationResponse.RepeatedAcadCreds);
                Assert.AreEqual(studentAcadCred.StcAltcumContribCmplCred, registrationResponse.AltcumContribCmplCred);
                Assert.AreEqual(studentAcadCred.StcAltcumContribGpaCred, registrationResponse.AltcumContribGpaCred);
            }

            [TestMethod]
            public async Task RegistrationAuditV7_PropertiesValid()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.Where(a => a.GradingType == GradingType.Audit).FirstOrDefault();
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.GetAsync(guid);
                Assert.AreEqual(studentAcadCred.RecordGuid, registrationResponse.Guid);
                Assert.AreEqual(studentAcadCred.StcPersonId, registrationResponse.StudentId);
                Assert.AreEqual(studentCourseSec.ScsCourseSection, registrationResponse.SectionId);
                Assert.AreEqual(studentCourseSec.ScsPassAudit, registrationResponse.PassAudit);
                Assert.AreEqual(studentAcadCred.StcCredType, registrationResponse.CreditType);
                Assert.AreEqual(studentAcadCred.StcAcadLevel, registrationResponse.AcademicLevel);
                Assert.AreEqual(studentAcadCred.StcCeus, registrationResponse.Ceus);
                Assert.AreEqual(studentAcadCred.StcAttCred, registrationResponse.Credit);
                Assert.AreEqual(studentAcadCred.StcGradePts, registrationResponse.GradePoint);
                Assert.AreEqual(studentAcadCred.StcReplCode, registrationResponse.ReplCode);
                Assert.AreEqual(studentAcadCred.StcRepeatedAcadCred, registrationResponse.RepeatedAcadCreds);
                Assert.AreEqual(studentAcadCred.StcAltcumContribCmplCred, registrationResponse.AltcumContribCmplCred);
                Assert.AreEqual(studentAcadCred.StcAltcumContribGpaCred, registrationResponse.AltcumContribGpaCred);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegistrationNoStcV7_ReturnsException()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.FirstOrDefault();
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(null);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.GetAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegistrationNoScsV7_ReturnsException()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.FirstOrDefault();
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(null);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.GetAsync(guid);
            }

            private SectionRegistrationRepository BuildValidSectionRegistrationRepository()
            {
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                sectionRegistrationRepo = new SectionRegistrationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return sectionRegistrationRepo;

            }

            private StudentAcadCred BuildValidStcResponse(AcademicCredit ac)
            {
                StudentAcadCred stc = new StudentAcadCred();

                stc.RecordGuid = Guid.NewGuid().ToString();
                stc.Recordkey = ac.Id;
                stc.StcPersonId = ac.Id;
                stc.StcAcadLevel = ac.AcademicLevelCode;
                stc.StcAllowReplFlag = ac.CanBeReplaced ? "Y" : "";
                stc.StcAltcumContribCmplCred = ac.AdjustedCredit;
                stc.StcAltcumContribGpaCred = ac.AdjustedGpaCredit;
                stc.StcAltcumContribGradePts = ac.AdjustedGradePoints;
                stc.StcCeus = ac.ContinuingEducationUnits;
                if (ac.Course != null)
                {
                    stc.StcCourse = ac.Course.Id;
                    stc.StcCourseLevel = ac.Course.CourseLevelCodes.First();
                }
                stc.StcCourseName = ac.CourseName;
                stc.StcCred = ac.Credit;
                string typecode;
                switch (ac.Type)
                {
                    case CreditType.ContinuingEducation:
                        typecode = "CE";
                        break;
                    case CreditType.Institutional:
                        typecode = "IN";
                        break;
                    case CreditType.Transfer:
                        typecode = "TRN";
                        break;
                    default:
                        typecode = "OTH";
                        break;
                }
                stc.StcCredType = typecode;
                // For one academic credit leave the departments blank and be sure they get defaulted from the course
                if (ac.Id == "1")
                {
                    stc.StcDepts = new List<string>();
                }
                else
                {
                    stc.StcDepts = ac.DepartmentCodes.ToList();
                }
                stc.StcStartDate = ac.StartDate;
                stc.StcEndDate = ac.EndDate;
                stc.StcGpaCred = ac.GpaCredit;
                stc.StcGradePts = ac.GradePoints;
                stc.StcGradeScheme = ac.GradeSchemeCode;
                stc.StcCmplCred = ac.CompletedCredit;
                stc.StcAttCred = ac.AttemptedCredit;
                stc.StcAttCeus = stc.StcCeus;
                stc.StcSectionNo = ac.SectionNumber;
                stc.StcStudentEquivEval = ac.CourseName;
                //stc.StcMark 
                stc.StcReplCode = ac.ReplacedStatus == ReplacedStatus.Replaced ? "R" : null;
                stc.StcRepeatedAcadCred = ac.RepeatAcademicCreditIds;

                // Status 
                string stat = "";
                switch (ac.Status)
                {
                    case CreditStatus.Add: { stat = "A"; break; }
                    case CreditStatus.Cancelled: { stat = "C"; break; }
                    case CreditStatus.Deleted: { stat = "X"; break; }
                    case CreditStatus.Dropped: { stat = "D"; break; }
                    case CreditStatus.New: { stat = "N"; break; }
                    case CreditStatus.Preliminary: { stat = "PR"; break; }
                    case CreditStatus.TransferOrNonCourse: { stat = "TR"; break; }
                    case CreditStatus.Withdrawn: { stat = "W"; break; }
                    default: { stat = ""; break; }
                }

                stc.StcStatus = new List<string>() { stat };

                stc.StcStatusesEntityAssociation = new List<StudentAcadCredStcStatuses>();
                StudentAcadCredStcStatuses statusitem = new StudentAcadCredStcStatuses(
                                                                DateTime.Now,
                                                                ac.Status.ToString()[0].ToString(),
                                                                DateTime.Now,
                                                                "");
                stc.StcStatusesEntityAssociation.Add(statusitem);
                stc.StcStudentCourseSec = ac.Id; // Not real life example here.  The SCS record would have a diff-
                // erent ID in real life, but we don't keep it here, so for mocking
                // we will pretend it is the same as the STC record ID.
                stc.StcSubject = ac.SubjectCode;
                stc.StcTerm = ac.TermCode;
                //stc.StcTitle I don't think we use this
                if (ac.VerifiedGrade != null)
                {
                    stc.StcVerifiedGrade = ac.VerifiedGrade.Id;
                    // added for mobile
                    stc.StcVerifiedGradeDate = new DateTime(ac.VerifiedGradeTimestamp.Value.Year, ac.VerifiedGradeTimestamp.Value.Month, ac.VerifiedGradeTimestamp.Value.Day, 0, 0, 0);
                    // end added for mobile
                }
                return stc;
            }

            private StudentCourseSec BuildValidScsResponse(AcademicCredit ac)
            {
                StudentCourseSec scs = new StudentCourseSec();
                scs.Recordkey = ac.Id;
                scs.ScsCourseSection = ac.SectionId;
                switch (ac.GradingType)
                {
                    case GradingType.Graded:
                        scs.ScsPassAudit = "";
                        break;
                    case GradingType.PassFail:
                        scs.ScsPassAudit = "P";
                        break;
                    case GradingType.Audit:
                        scs.ScsPassAudit = "A";
                        break;
                    default:
                        break;
                }
                // added for mobile
                // for each midtermgrade in the list, populate the correct field 
                // Not necessarily sequential starting from 1!!! Mock the time as
                // midnight, which we won't see if we get a time from the SCSCC
                foreach (MidTermGrade mtg in ac.MidTermGrades)
                {
                    switch (mtg.Position)
                    {
                        case 1:
                            scs.ScsMidTermGrade1 = mtg.GradeId;
                            scs.ScsMidGradeDate1 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 2:
                            scs.ScsMidTermGrade2 = mtg.GradeId;
                            scs.ScsMidGradeDate2 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 3:
                            scs.ScsMidTermGrade3 = mtg.GradeId;
                            scs.ScsMidGradeDate3 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 4:
                            scs.ScsMidTermGrade4 = mtg.GradeId;
                            scs.ScsMidGradeDate4 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 5:
                            scs.ScsMidTermGrade5 = mtg.GradeId;
                            scs.ScsMidGradeDate5 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 6:
                            scs.ScsMidTermGrade6 = mtg.GradeId;
                            scs.ScsMidGradeDate6 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                    }
                }
                // end added for mobile
                return scs;
            }
        }

        [TestClass]
        public class GetSectionRegistrationV1600Tests
        {
            SectionRegistrationRepository sectionRegistrationRepo;
            SectionRegistrationResponse sectReg;
            List<AcademicCredit> academicCreditData;
            Collection<CourseSections> courseSections;
            StudentAcadCred studentAcadCred;
            StudentCourseSec studentCourseSec;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> transManager;

            [TestInitialize]
            public void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transManager = new Mock<IColleagueTransactionInvoker>();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                academicCreditData = (new TestAcademicCreditRepository().GetAsync().Result).ToList();
                var academicCredit = academicCreditData.ElementAt(0);
                courseSections = BuildCourseSections();
                studentAcadCred = BuildValidStcResponse(academicCredit);
                studentCourseSec = BuildValidScsResponse(academicCredit);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationRepo = null;
                academicCreditData = null;
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async_StudentId_EmptyResults()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSectionRegistrationsKeys:",
                    Entity = "STUDENT.ACAD.CRED",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager.Object);
                transManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                sectReg = new SectionRegistrationResponse(It.IsAny<string>(), "1", It.IsAny<string>(), It.IsAny<string>(), new List<RegistrationMessage>());

                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), sectReg, It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async_AcadPeriod_EmptyResults()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSectionRegistrationsKeys:",
                    Entity = "STUDENT.ACAD.CRED",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager.Object);
                transManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny< SectionRegistrationResponse>(), "BadId", It.IsAny<string>());
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async_SectionId_EmptyResults()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSectionRegistrationsKeys:",
                    Entity = "STUDENT.ACAD.CRED",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager.Object);
                transManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                sectReg = new SectionRegistrationResponse(It.IsAny<string>(), It.IsAny<string>(), "1", It.IsAny<string>(), new List<RegistrationMessage>());

                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), sectReg, It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async_SectionId_WithStudentId_EmptyResults()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSectionRegistrationsKeys:",
                    Entity = "STUDENT.ACAD.CRED",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager.Object);
                transManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                sectReg = new SectionRegistrationResponse(It.IsAny<string>(), "1", "1", It.IsAny<string>(), new List<RegistrationMessage>());
                dataAccessorMock.SetupSequence(dr => dr.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new[] { "1" }))
                    .Returns(Task.FromResult(new[] { "1" }))
                    .Returns(Task.FromResult(new string[] { }));

                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), sectReg, It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async_SectionInstructor_EmptyResults()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSectionRegistrationsKeys:",
                    Entity = "STUDENT.ACAD.CRED",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager.Object);
                transManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                sectReg = new SectionRegistrationResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), new List<RegistrationMessage>());

                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(), It.IsAny<string>(), "1");
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async_SectionInstructor_CourseSecIdsNull_EmptyResults()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSectionRegistrationsKeys:",
                    Entity = "STUDENT.ACAD.CRED",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager.Object);
                transManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                sectReg = new SectionRegistrationResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), new List<RegistrationMessage>());
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1" });
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(null);

                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(), It.IsAny<string>(), "1");
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async_SectionInstructor_AcadCredLimitingKeysNull_EmptyResults()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSectionRegistrationsKeys:",
                    Entity = "STUDENT.ACAD.CRED",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager.Object);
                transManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1" });
                dataAccessorMock.SetupSequence(dr => dr.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new[] { "1" }));


                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(), It.IsAny<string>(), "1");
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async_EmptyResults()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSectionRegistrationsKeys:",
                    Entity = "STUDENT.ACAD.CRED",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager.Object);
                transManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(), It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async()
            {
                dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1" });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentAcadCred>() { studentAcadCred });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentCourseSec>() { studentCourseSec });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<CourseSections>(It.IsAny<string[]>(), true)).ReturnsAsync(courseSections);

                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(0, 1, It.IsAny<SectionRegistrationResponse>(), It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(response);
                Assert.AreEqual(1, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrations3Async_WithOverride()
            {
                dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1" });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentAcadCred>() { studentAcadCred });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentCourseSec>() { studentCourseSec });
                courseSections.ElementAt(0).SecTerm = "DiffTerm";
                courseSections.ElementAt(0).SecLocation = "DiffLocation";
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<CourseSections>(It.IsAny<string[]>(), true)).ReturnsAsync(courseSections);

                var response = await sectionRegistrationRepo.GetSectionRegistrations3Async(0, 1, It.IsAny<SectionRegistrationResponse>(), It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(response);
                Assert.AreEqual(1, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrationByIdAsync()
            {
                dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);
                courseSections.ElementAt(0).SecTerm = "DiffTerm";
                courseSections.ElementAt(0).SecLocation = "DiffLocation";
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<CourseSections>(It.IsAny<string[]>(), true)).ReturnsAsync(courseSections);

                var response = await sectionRegistrationRepo.GetSectionRegistrationByIdAsync("1");
                Assert.IsNotNull(response);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegistration_GetSectionRegistrationByIdAsync_WrongId_KeyNotFoundException()
            {
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(null);
                var response = await sectionRegistrationRepo.GetSectionRegistrationByIdAsync("BadId");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegistration_GetSectionRegistrationByIdAsync_StcStudentCourseSecNull_KeyNotFoundException()
            {
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);
                studentAcadCred.StcStudentCourseSec = "";
                var response = await sectionRegistrationRepo.GetSectionRegistrationByIdAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegistration_GetSectionRegistrationByIdAsync_StudentCourseSecNull_KeyNotFoundException()
            {
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                studentAcadCred.StcStudentCourseSec = "Bad Key";
                var response = await sectionRegistrationRepo.GetSectionRegistrationByIdAsync("1");
            }

            private SectionRegistrationRepository BuildValidSectionRegistrationRepository()
            {
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSectionRegistrationsKeys:",
                    Entity = "STUDENT.ACAD.CRED",
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
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager.Object);
                transManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                sectionRegistrationRepo = new SectionRegistrationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return sectionRegistrationRepo;

            }

            private StudentAcadCred BuildValidStcResponse(AcademicCredit ac)
            {
                StudentAcadCred stc = new StudentAcadCred();

                stc.RecordGuid = Guid.NewGuid().ToString();
                stc.Recordkey = ac.Id;
                stc.StcPersonId = ac.Id;
                stc.StcAcadLevel = ac.AcademicLevelCode;
                stc.StcAllowReplFlag = ac.CanBeReplaced ? "Y" : "";
                stc.StcAltcumContribCmplCred = ac.AdjustedCredit;
                stc.StcAltcumContribGpaCred = ac.AdjustedGpaCredit;
                stc.StcAltcumContribGradePts = ac.AdjustedGradePoints;
                stc.StcCeus = ac.ContinuingEducationUnits;
                if (ac.Course != null)
                {
                    stc.StcCourse = ac.Course.Id;
                    stc.StcCourseLevel = ac.Course.CourseLevelCodes.First();
                }
                stc.StcCourseName = ac.CourseName;
                stc.StcCred = ac.Credit;
                string typecode;
                switch (ac.Type)
                {
                    case CreditType.ContinuingEducation:
                        typecode = "CE";
                        break;
                    case CreditType.Institutional:
                        typecode = "IN";
                        break;
                    case CreditType.Transfer:
                        typecode = "TRN";
                        break;
                    default:
                        typecode = "OTH";
                        break;
                }
                stc.StcCredType = typecode;
                // For one academic credit leave the departments blank and be sure they get defaulted from the course
                if (ac.Id == "1")
                {
                    stc.StcDepts = new List<string>();
                }
                else
                {
                    stc.StcDepts = ac.DepartmentCodes.ToList();
                }
                stc.StcStartDate = ac.StartDate;
                stc.StcEndDate = ac.EndDate;
                stc.StcGpaCred = ac.GpaCredit;
                stc.StcGradePts = ac.GradePoints;
                stc.StcGradeScheme = ac.GradeSchemeCode;
                stc.StcCmplCred = ac.CompletedCredit;
                stc.StcAttCred = ac.AttemptedCredit;
                stc.StcAttCeus = stc.StcCeus;
                stc.StcSectionNo = ac.SectionNumber;
                stc.StcStudentEquivEval = ac.CourseName;
                //stc.StcMark 
                stc.StcReplCode = ac.ReplacedStatus == ReplacedStatus.Replaced ? "R" : null;
                stc.StcRepeatedAcadCred = ac.RepeatAcademicCreditIds;

                // Status 
                string stat = "";
                switch (ac.Status)
                {
                    case CreditStatus.Add: { stat = "A"; break; }
                    case CreditStatus.Cancelled: { stat = "C"; break; }
                    case CreditStatus.Deleted: { stat = "X"; break; }
                    case CreditStatus.Dropped: { stat = "D"; break; }
                    case CreditStatus.New: { stat = "N"; break; }
                    case CreditStatus.Preliminary: { stat = "PR"; break; }
                    case CreditStatus.TransferOrNonCourse: { stat = "TR"; break; }
                    case CreditStatus.Withdrawn: { stat = "W"; break; }
                    default: { stat = ""; break; }
                }

                stc.StcStatus = new List<string>() { stat };
                stc.StcStatusDate = new List<DateTime?>() { DateTime.Today.Date};

                stc.StcStatusesEntityAssociation = new List<StudentAcadCredStcStatuses>();
                StudentAcadCredStcStatuses statusitem = new StudentAcadCredStcStatuses(
                                                                DateTime.Now,
                                                                ac.Status.ToString()[0].ToString(),
                                                                DateTime.Now,
                                                                "");
                stc.StcStatusesEntityAssociation.Add(statusitem);
                stc.StcStudentCourseSec = ac.Id; // Not real life example here.  The SCS record would have a diff-
                // erent ID in real life, but we don't keep it here, so for mocking
                // we will pretend it is the same as the STC record ID.
                stc.StcSubject = ac.SubjectCode;
                stc.StcTerm = ac.TermCode;
                //stc.StcTitle I don't think we use this
                if (ac.VerifiedGrade != null)
                {
                    stc.StcVerifiedGrade = ac.VerifiedGrade.Id;
                    // added for mobile
                    stc.StcVerifiedGradeDate = new DateTime(ac.VerifiedGradeTimestamp.Value.Year, ac.VerifiedGradeTimestamp.Value.Month, ac.VerifiedGradeTimestamp.Value.Day, 0, 0, 0);
                    // end added for mobile
                }
                stc.StcTerm = "2009/SP";
                return stc;
            }

            private StudentCourseSec BuildValidScsResponse(AcademicCredit ac)
            {
                StudentCourseSec scs = new StudentCourseSec();
                scs.Recordkey = ac.Id;
                scs.ScsCourseSection = ac.SectionId;
                switch (ac.GradingType)
                {
                    case GradingType.Graded:
                        scs.ScsPassAudit = "";
                        break;
                    case GradingType.PassFail:
                        scs.ScsPassAudit = "P";
                        break;
                    case GradingType.Audit:
                        scs.ScsPassAudit = "A";
                        break;
                    default:
                        break;
                }
                // added for mobile
                // for each midtermgrade in the list, populate the correct field 
                // Not necessarily sequential starting from 1!!! Mock the time as
                // midnight, which we won't see if we get a time from the SCSCC
                foreach (MidTermGrade mtg in ac.MidTermGrades)
                {
                    switch (mtg.Position)
                    {
                        case 1:
                            scs.ScsMidTermGrade1 = mtg.GradeId;
                            scs.ScsMidGradeDate1 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 2:
                            scs.ScsMidTermGrade2 = mtg.GradeId;
                            scs.ScsMidGradeDate2 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 3:
                            scs.ScsMidTermGrade3 = mtg.GradeId;
                            scs.ScsMidGradeDate3 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 4:
                            scs.ScsMidTermGrade4 = mtg.GradeId;
                            scs.ScsMidGradeDate4 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 5:
                            scs.ScsMidTermGrade5 = mtg.GradeId;
                            scs.ScsMidGradeDate5 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 6:
                            scs.ScsMidTermGrade6 = mtg.GradeId;
                            scs.ScsMidGradeDate6 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                    }
                }
                scs.ScsLocation = "Loc 1";
                // end added for mobile
                return scs;
            }
            private Collection<CourseSections> BuildCourseSections()
            {
                Collection<CourseSections> courseSect = new Collection<CourseSections>()
                {
                    new CourseSections
                    {
                        RecordGuid = Guid.NewGuid().ToString(),
                        Recordkey = "8001",
                        SecTerm = "2009/SP",
                        SecLocation = "Loc 1"
                    }
                };
                return courseSect;
            }
        }

        [TestClass]
        public class GetSectionRegistrationGradeOptionsV1_0_0Tests
        {
            SectionRegistrationRepository sectionRegistrationRepo;
            Collection<CourseSections> courseSections;
            StudentAcadCred studentAcadCred;
            StudentCourseSec studentCourseSec;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;

            [TestInitialize]
            public void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();

                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                courseSections = BuildCourseSections();
                studentAcadCred = BuildValidStcResponse();
                studentCourseSec = BuildValidScsResponse();
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationRepo = null;
            }
                                   
            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrationGradeOptionsAsync()
            {
                dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1" });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentAcadCred>() { studentAcadCred });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentCourseSec>() { studentCourseSec });
                
                var response = await sectionRegistrationRepo.GetSectionRegistrationGradeOptionsAsync(0, 1, It.IsAny<StudentAcadCredCourseSecInfo>());
                Assert.IsNotNull(response);
                Assert.AreEqual(1, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrationGradeOptionsAsync_EmptyResults()
            {
                dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);
                var response = await sectionRegistrationRepo.GetSectionRegistrationGradeOptionsAsync(0, 1, It.IsAny<StudentAcadCredCourseSecInfo>());
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrationGradeOptionsAsync_SectionFilter()
            {
                var request = new StudentAcadCredCourseSecInfo("8003");
                dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1" });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentAcadCred>() { studentAcadCred });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentCourseSec>() { studentCourseSec });
                var response = await sectionRegistrationRepo.GetSectionRegistrationGradeOptionsAsync(0, 1, request);
                Assert.IsNotNull(response);
                Assert.AreEqual(1, response.Item2);
            }

            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrationGradeOptionsAsync_SectionFilter_EmptyResults()
            {
                var request = new StudentAcadCredCourseSecInfo("8003");
                dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentAcadCred>() { studentAcadCred });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentCourseSec>() { studentCourseSec });
                var response = await sectionRegistrationRepo.GetSectionRegistrationGradeOptionsAsync(0, 1, request);
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Item2);
            }
            
            [TestMethod]
            public async Task SectionRegistration_GetSectionRegistrationGradeOptionsByIdAsync()
            {
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSec);
                var response = await sectionRegistrationRepo.GetSectionRegistrationGradeOptionsByIdAsync("1");
                Assert.IsNotNull(response);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegistration_GetSectionRegistrationGradeOptionsByIdAsync_KeyNotFound()
            {
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                var response = await sectionRegistrationRepo.GetSectionRegistrationGradeOptionsByIdAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegistration_GetSectionRegistrationGradeOptionsByIdAsync_NoScsKey()
            {
                StudentAcadCred stc = new StudentAcadCred();
                stc.RecordGuid = Guid.NewGuid().ToString();
                stc.Recordkey = "3";
                stc.StcPersonId = "0000123";
                stc.StcGradeScheme = "UG";
                stc.StcTerm = "2018/FA";
                stc.StcStartDate = new DateTime(2018, 8, 25);
                stc.StcEndDate = new DateTime(2018, 12, 19);
                stc.StcFinalGrade = "1";
                stc.StcVerifiedGrade = "2";
                stc.StcStudentCourseSec = "";
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(stc);
                var response = await sectionRegistrationRepo.GetSectionRegistrationGradeOptionsByIdAsync("3");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegistration_GetSectionRegistrationGradeOptionsByIdAsync_NoScsRecord()
            {
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentAcadCred>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentCourseSec>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                var response = await sectionRegistrationRepo.GetSectionRegistrationGradeOptionsByIdAsync("1");
            }
            
            private SectionRegistrationRepository BuildValidSectionRegistrationRepository()
            {
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                sectionRegistrationRepo = new SectionRegistrationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return sectionRegistrationRepo;

            }

            private StudentAcadCred BuildValidStcResponse()
            {
                StudentAcadCred stc = new StudentAcadCred();
                stc.RecordGuid = Guid.NewGuid().ToString();
                stc.Recordkey = "1";
                stc.StcPersonId = "0000123";               
                stc.StcGradeScheme = "UG";
                stc.StcTerm = "2018/FA";
                stc.StcStartDate = new DateTime(2018, 8, 25);
                stc.StcEndDate = new DateTime(2018, 12, 19);
                stc.StcFinalGrade = "1";
                stc.StcVerifiedGrade = "2";
                stc.StcStudentCourseSec = "1";
                return stc;
            }

            private StudentCourseSec BuildValidScsResponse()
            {
                StudentCourseSec scs = new StudentCourseSec();
                scs.Recordkey = "1";
                scs.ScsCourseSection = "8001";
                scs.ScsStudentAcadCred = "1";
                return scs;
            }
            private Collection<CourseSections> BuildCourseSections()
            {
                Collection<CourseSections> courseSect = new Collection<CourseSections>()
                {
                    new CourseSections
                    {
                        RecordGuid = Guid.NewGuid().ToString(),
                        Recordkey = "8001",
                        SecTerm = "2018/FA"
                    }
                };
                return courseSect;
            }
        }

        [TestClass]
        public class UpdateSectionRegistrationTests
        {
            SectionRegistrationRepository sectionRegistrationRepo;
            List<AcademicCredit> academicCreditData;
            SectionRegistrationRequest request;
            UpdateSectionRegistrationResponse response;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            IColleagueTransactionInvoker transManager;
            IColleagueDataReader dataAccessor;

            [TestInitialize]
            public void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                dataAccessor = dataAccessorMock.Object;
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                transManager = transManagerMock.Object;

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessor);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationRepo = null;
            }

            [TestMethod]
            public async Task RegistrationRequest_ReturnsValidResponse()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);
                var studentId = studentAcadCred.StcPersonId;
                var sectionId = studentCourseSec.ScsCourseSection;
                var statusCode = academicCredit.Status.ToString();
                var sectionRegistration = new SectionRegistration()
                {
                    SectionId = sectionId,
                    Action = RegistrationAction.Add,
                };

                request = new SectionRegistrationRequest(studentId, studentAcadCred.RecordGuid, sectionRegistration);
                response = new UpdateSectionRegistrationResponse();
                response.SecRegGuid = studentAcadCred.RecordGuid;

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(It.IsAny<UpdateSectionRegistrationRequest>())).ReturnsAsync(response);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.UpdateAsync(request, guid, studentId, sectionId, statusCode);
                Assert.AreEqual(studentAcadCred.RecordGuid, registrationResponse.Guid);
                Assert.AreEqual(studentAcadCred.StcPersonId, registrationResponse.StudentId);
                Assert.AreEqual(studentCourseSec.ScsCourseSection, registrationResponse.SectionId);
                Assert.AreEqual(studentCourseSec.ScsPassAudit, registrationResponse.PassAudit);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RegistrationRequest_ReturnsErrorException()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);
                var studentId = studentAcadCred.StcPersonId;
                var sectionId = studentCourseSec.ScsCourseSection;
                var statusCode = academicCredit.Status.ToString();
                var sectionRegistration = new SectionRegistration()
                {
                    SectionId = sectionId,
                    Action = RegistrationAction.Add,
                };

                request = new SectionRegistrationRequest(studentId, studentAcadCred.RecordGuid, sectionRegistration);
                response = new UpdateSectionRegistrationResponse()
                {
                    ErrorOccurred = true,
                    ErrorMessage = "Error",
                    RegMessages = new List<RegMessages>()
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(It.IsAny<UpdateSectionRegistrationRequest>())).ReturnsAsync(response);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.UpdateAsync(request, guid, studentId, sectionId, statusCode);
            }

            private SectionRegistrationRepository BuildValidSectionRegistrationRepository()
            {
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                sectionRegistrationRepo = new SectionRegistrationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return sectionRegistrationRepo;

            }

            private StudentAcadCred BuildValidStcResponse(AcademicCredit ac)
            {
                StudentAcadCred stc = new StudentAcadCred();

                stc.RecordGuid = Guid.NewGuid().ToString();
                stc.Recordkey = ac.Id;
                stc.StcPersonId = ac.Id;
                stc.StcAcadLevel = ac.AcademicLevelCode;
                stc.StcAllowReplFlag = ac.CanBeReplaced ? "Y" : "";
                stc.StcAltcumContribCmplCred = ac.AdjustedCredit;
                stc.StcAltcumContribGpaCred = ac.AdjustedGpaCredit;
                stc.StcAltcumContribGradePts = ac.AdjustedGradePoints;
                stc.StcCeus = ac.ContinuingEducationUnits;
                if (ac.Course != null)
                {
                    stc.StcCourse = ac.Course.Id;
                    stc.StcCourseLevel = ac.Course.CourseLevelCodes.First();
                }
                stc.StcCourseName = ac.CourseName;
                stc.StcCred = ac.Credit;
                string typecode;
                switch (ac.Type)
                {
                    case CreditType.ContinuingEducation:
                        typecode = "CE";
                        break;
                    case CreditType.Institutional:
                        typecode = "IN";
                        break;
                    case CreditType.Transfer:
                        typecode = "TRN";
                        break;
                    default:
                        typecode = "OTH";
                        break;
                }
                stc.StcCredType = typecode;
                // For one academic credit leave the departments blank and be sure they get defaulted from the course
                if (ac.Id == "1")
                {
                    stc.StcDepts = new List<string>();
                }
                else
                {
                    stc.StcDepts = ac.DepartmentCodes.ToList();
                }
                stc.StcStartDate = ac.StartDate;
                stc.StcEndDate = ac.EndDate;
                stc.StcGpaCred = ac.GpaCredit;
                stc.StcGradePts = ac.GradePoints;
                stc.StcGradeScheme = ac.GradeSchemeCode;
                stc.StcCmplCred = ac.CompletedCredit;
                stc.StcAttCred = ac.AttemptedCredit;
                stc.StcSectionNo = ac.SectionNumber;
                stc.StcStudentEquivEval = ac.CourseName;
                //stc.StcMark 
                stc.StcReplCode = ac.ReplacedStatus == ReplacedStatus.Replaced ? "R" : null;
                stc.StcRepeatedAcadCred = ac.RepeatAcademicCreditIds;

                // Status 
                string stat = "";
                switch (ac.Status)
                {
                    case CreditStatus.Add: { stat = "A"; break; }
                    case CreditStatus.Cancelled: { stat = "C"; break; }
                    case CreditStatus.Deleted: { stat = "X"; break; }
                    case CreditStatus.Dropped: { stat = "D"; break; }
                    case CreditStatus.New: { stat = "N"; break; }
                    case CreditStatus.Preliminary: { stat = "PR"; break; }
                    case CreditStatus.TransferOrNonCourse: { stat = "TR"; break; }
                    case CreditStatus.Withdrawn: { stat = "W"; break; }
                    default: { stat = ""; break; }
                }

                stc.StcStatus = new List<string>() { stat };

                stc.StcStatusesEntityAssociation = new List<StudentAcadCredStcStatuses>();
                StudentAcadCredStcStatuses statusitem = new StudentAcadCredStcStatuses(
                                                                DateTime.Now,
                                                                ac.Status.ToString()[0].ToString(),
                                                                DateTime.Now,
                                                                "");
                stc.StcStatusesEntityAssociation.Add(statusitem);
                stc.StcStudentCourseSec = ac.Id; // Not real life example here.  The SCS record would have a diff-
                // erent ID in real life, but we don't keep it here, so for mocking
                // we will pretend it is the same as the STC record ID.
                stc.StcSubject = ac.SubjectCode;
                stc.StcTerm = ac.TermCode;
                //stc.StcTitle I don't think we use this
                if (ac.VerifiedGrade != null)
                {
                    stc.StcVerifiedGrade = ac.VerifiedGrade.Id;
                    // added for mobile
                    stc.StcVerifiedGradeDate = new DateTime(ac.VerifiedGradeTimestamp.Value.Year, ac.VerifiedGradeTimestamp.Value.Month, ac.VerifiedGradeTimestamp.Value.Day, 0, 0, 0);
                    // end added for mobile
                }
                return stc;
            }

            private StudentCourseSec BuildValidScsResponse(AcademicCredit ac)
            {
                StudentCourseSec scs = new StudentCourseSec();
                scs.Recordkey = ac.Id;
                scs.ScsCourseSection = ac.SectionId;
                switch (ac.GradingType)
                {
                    case GradingType.Graded:
                        scs.ScsPassAudit = "";
                        break;
                    case GradingType.PassFail:
                        scs.ScsPassAudit = "P";
                        break;
                    case GradingType.Audit:
                        scs.ScsPassAudit = "A";
                        break;
                    default:
                        break;
                }
                // added for mobile
                // for each midtermgrade in the list, populate the correct field 
                // Not necessarily sequential starting from 1!!! Mock the time as
                // midnight, which we won't see if we get a time from the SCSCC
                foreach (MidTermGrade mtg in ac.MidTermGrades)
                {
                    switch (mtg.Position)
                    {
                        case 1:
                            scs.ScsMidTermGrade1 = mtg.GradeId;
                            scs.ScsMidGradeDate1 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 2:
                            scs.ScsMidTermGrade2 = mtg.GradeId;
                            scs.ScsMidGradeDate2 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 3:
                            scs.ScsMidTermGrade3 = mtg.GradeId;
                            scs.ScsMidGradeDate3 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 4:
                            scs.ScsMidTermGrade4 = mtg.GradeId;
                            scs.ScsMidGradeDate4 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 5:
                            scs.ScsMidTermGrade5 = mtg.GradeId;
                            scs.ScsMidGradeDate5 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 6:
                            scs.ScsMidTermGrade6 = mtg.GradeId;
                            scs.ScsMidGradeDate6 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                    }
                }
                // end added for mobile
                return scs;
            }
        }

        [TestClass]
        public class UpdateSectionRegistrationV7Tests
        {
            SectionRegistrationRepository sectionRegistrationRepo;
            List<AcademicCredit> academicCreditData;
            SectionRegistrationRequest request;
            UpdateSectionRegistrationResponse response;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            IColleagueTransactionInvoker transManager;
            IColleagueDataReader dataAccessor;

            [TestInitialize]
            public void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                dataAccessor = dataAccessorMock.Object;
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                transManager = transManagerMock.Object;

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessor);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationRepo = null;
            }

            [TestMethod]
            public async Task RegistrationRequestV7_ReturnsValidResponse()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);
                var studentId = studentAcadCred.StcPersonId;
                var sectionId = studentCourseSec.ScsCourseSection;
                var statusCode = academicCredit.Status.ToString();
                var sectionRegistration = new SectionRegistration()
                {
                    SectionId = sectionId,
                    Action = RegistrationAction.Add,
                };

                request = new SectionRegistrationRequest(studentId, studentAcadCred.RecordGuid, sectionRegistration);
                response = new UpdateSectionRegistrationResponse();
                response.SecRegGuid = studentAcadCred.RecordGuid;

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(It.IsAny<UpdateSectionRegistrationRequest>())).ReturnsAsync(response);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.Update2Async(request, guid, studentId, sectionId, statusCode);
                Assert.AreEqual(studentAcadCred.RecordGuid, registrationResponse.Guid);
                Assert.AreEqual(studentAcadCred.StcPersonId, registrationResponse.StudentId);
                Assert.AreEqual(studentCourseSec.ScsCourseSection, registrationResponse.SectionId);
                Assert.AreEqual(studentCourseSec.ScsPassAudit, registrationResponse.PassAudit);
                Assert.AreEqual(studentAcadCred.StcCredType, registrationResponse.CreditType);
                Assert.AreEqual(studentAcadCred.StcAcadLevel, registrationResponse.AcademicLevel);
                Assert.AreEqual(studentAcadCred.StcCeus, registrationResponse.Ceus);
                Assert.AreEqual(studentAcadCred.StcAttCred, registrationResponse.Credit);
                Assert.AreEqual(studentAcadCred.StcGradePts, registrationResponse.GradePoint);
                Assert.AreEqual(studentAcadCred.StcReplCode, registrationResponse.ReplCode);
                Assert.AreEqual(studentAcadCred.StcRepeatedAcadCred, registrationResponse.RepeatedAcadCreds);
                Assert.AreEqual(studentAcadCred.StcAltcumContribCmplCred, registrationResponse.AltcumContribCmplCred);
                Assert.AreEqual(studentAcadCred.StcAltcumContribGpaCred, registrationResponse.AltcumContribGpaCred);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task RegistrationRequest_ReturnsErrorException()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);

                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);
                var studentId = studentAcadCred.StcPersonId;
                var sectionId = studentCourseSec.ScsCourseSection;
                var statusCode = academicCredit.Status.ToString();
                var sectionRegistration = new SectionRegistration()
                {
                    SectionId = sectionId,
                    Action = RegistrationAction.Add,
                };

                request = new SectionRegistrationRequest(studentId, studentAcadCred.RecordGuid, sectionRegistration);
                response = new UpdateSectionRegistrationResponse()
                {
                    ErrorOccurred = true,
                    ErrorMessage = "Error",
                    RegMessages = new List<RegMessages>()
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(It.IsAny<UpdateSectionRegistrationRequest>())).ReturnsAsync(response);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.Update2Async(request, guid, studentId, sectionId, statusCode);
            }

            private SectionRegistrationRepository BuildValidSectionRegistrationRepository()
            {
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                sectionRegistrationRepo = new SectionRegistrationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return sectionRegistrationRepo;

            }

            private StudentAcadCred BuildValidStcResponse(AcademicCredit ac)
            {
                StudentAcadCred stc = new StudentAcadCred();
                stc.RecordGuid = Guid.NewGuid().ToString();
                stc.Recordkey = ac.Id;
                stc.StcPersonId = ac.Id;
                stc.StcAcadLevel = ac.AcademicLevelCode;
                stc.StcAllowReplFlag = ac.CanBeReplaced ? "Y" : "";
                stc.StcAltcumContribCmplCred = ac.AdjustedCredit;
                stc.StcAltcumContribGpaCred = ac.AdjustedGpaCredit;
                stc.StcAltcumContribGradePts = ac.AdjustedGradePoints;
                stc.StcCeus = ac.ContinuingEducationUnits;                
                if (ac.Course != null)
                {
                    stc.StcCourse = ac.Course.Id;
                    stc.StcCourseLevel = ac.Course.CourseLevelCodes.First();
                }
                stc.StcCourseName = ac.CourseName;
                stc.StcCred = ac.Credit;
                string typecode;
                switch (ac.Type)
                {
                    case CreditType.ContinuingEducation:
                        typecode = "CE";
                        break;
                    case CreditType.Institutional:
                        typecode = "IN";
                        break;
                    case CreditType.Transfer:
                        typecode = "TRN";
                        break;
                    default:
                        typecode = "OTH";
                        break;
                }
                stc.StcCredType = typecode;
                // For one academic credit leave the departments blank and be sure they get defaulted from the course
                if (ac.Id == "1")
                {
                    stc.StcDepts = new List<string>();
                }
                else
                {
                    stc.StcDepts = ac.DepartmentCodes.ToList();
                }
                stc.StcStartDate = ac.StartDate;
                stc.StcEndDate = ac.EndDate;
                stc.StcGpaCred = ac.GpaCredit;
                stc.StcGradePts = ac.GradePoints;
                stc.StcGradeScheme = ac.GradeSchemeCode;
                stc.StcCmplCred = ac.CompletedCredit;
                stc.StcAttCred = ac.AttemptedCredit;
                stc.StcAttCeus = stc.StcCeus;
                stc.StcSectionNo = ac.SectionNumber;
                stc.StcStudentEquivEval = ac.CourseName;
                //stc.StcMark 
                stc.StcReplCode = ac.ReplacedStatus == ReplacedStatus.Replaced ? "R" : null;
                stc.StcRepeatedAcadCred = ac.RepeatAcademicCreditIds;

                // Status 
                string stat = "";
                switch (ac.Status)
                {
                    case CreditStatus.Add: { stat = "A"; break; }
                    case CreditStatus.Cancelled: { stat = "C"; break; }
                    case CreditStatus.Deleted: { stat = "X"; break; }
                    case CreditStatus.Dropped: { stat = "D"; break; }
                    case CreditStatus.New: { stat = "N"; break; }
                    case CreditStatus.Preliminary: { stat = "PR"; break; }
                    case CreditStatus.TransferOrNonCourse: { stat = "TR"; break; }
                    case CreditStatus.Withdrawn: { stat = "W"; break; }
                    default: { stat = ""; break; }
                }

                stc.StcStatus = new List<string>() { stat };

                stc.StcStatusesEntityAssociation = new List<StudentAcadCredStcStatuses>();
                StudentAcadCredStcStatuses statusitem = new StudentAcadCredStcStatuses(
                                                                DateTime.Now,
                                                                ac.Status.ToString()[0].ToString(),
                                                                DateTime.Now,
                                                                "");
                stc.StcStatusesEntityAssociation.Add(statusitem);
                stc.StcStudentCourseSec = ac.Id; // Not real life example here.  The SCS record would have a diff-
                // erent ID in real life, but we don't keep it here, so for mocking
                // we will pretend it is the same as the STC record ID.
                stc.StcSubject = ac.SubjectCode;
                stc.StcTerm = ac.TermCode;
                //stc.StcTitle I don't think we use this
                if (ac.VerifiedGrade != null)
                {
                    stc.StcVerifiedGrade = ac.VerifiedGrade.Id;
                    // added for mobile
                    stc.StcVerifiedGradeDate = new DateTime(ac.VerifiedGradeTimestamp.Value.Year, ac.VerifiedGradeTimestamp.Value.Month, ac.VerifiedGradeTimestamp.Value.Day, 0, 0, 0);
                    // end added for mobile
                }
                return stc;
            }

            private StudentCourseSec BuildValidScsResponse(AcademicCredit ac)
            {
                StudentCourseSec scs = new StudentCourseSec();
                scs.Recordkey = ac.Id;
                scs.ScsCourseSection = ac.SectionId;
                switch (ac.GradingType)
                {
                    case GradingType.Graded:
                        scs.ScsPassAudit = "";
                        break;
                    case GradingType.PassFail:
                        scs.ScsPassAudit = "P";
                        break;
                    case GradingType.Audit:
                        scs.ScsPassAudit = "A";
                        break;
                    default:
                        break;
                }
                // added for mobile
                // for each midtermgrade in the list, populate the correct field 
                // Not necessarily sequential starting from 1!!! Mock the time as
                // midnight, which we won't see if we get a time from the SCSCC
                foreach (MidTermGrade mtg in ac.MidTermGrades)
                {
                    switch (mtg.Position)
                    {
                        case 1:
                            scs.ScsMidTermGrade1 = mtg.GradeId;
                            scs.ScsMidGradeDate1 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 2:
                            scs.ScsMidTermGrade2 = mtg.GradeId;
                            scs.ScsMidGradeDate2 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 3:
                            scs.ScsMidTermGrade3 = mtg.GradeId;
                            scs.ScsMidGradeDate3 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 4:
                            scs.ScsMidTermGrade4 = mtg.GradeId;
                            scs.ScsMidGradeDate4 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 5:
                            scs.ScsMidTermGrade5 = mtg.GradeId;
                            scs.ScsMidGradeDate5 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 6:
                            scs.ScsMidTermGrade6 = mtg.GradeId;
                            scs.ScsMidGradeDate6 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                    }
                }
                // end added for mobile
                return scs;
            }
        }

        [TestClass]
        public class UpdateSectionRegistrationV1600Tests
        {
            SectionRegistrationRepository sectionRegistrationRepo;
            List<AcademicCredit> academicCreditData;
            SectionRegistrationRequest request;
            UpdateSectionRegistrationResponse response;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            IColleagueTransactionInvoker transManager;
            IColleagueDataReader dataAccessor;

            [TestInitialize]
            public void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                dataAccessor = dataAccessorMock.Object;
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                transManager = transManagerMock.Object;

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessor);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationRepo = null;
            }

            [TestMethod]
            public async Task RegistrationRequestV1600_ReturnsValidResponse()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);
                var studentId = studentAcadCred.StcPersonId;
                var sectionId = studentCourseSec.ScsCourseSection;
                var statusCode = academicCredit.Status.ToString();
                var sectionRegistration = new SectionRegistration()
                {
                    SectionId = sectionId,
                    Action = RegistrationAction.Add,
                };

                request = new SectionRegistrationRequest(studentId, studentAcadCred.RecordGuid, sectionRegistration);
                request.InvolvementStartOn = academicCredit.StartDate;
                request.InvolvementEndOn = academicCredit.EndDate;
                
                response = new UpdateSectionRegistrationResponse();
                response.SecRegGuid = studentAcadCred.RecordGuid;

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(It.IsAny<UpdateSectionRegistrationRequest>())).ReturnsAsync(response);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.Update2Async(request, guid, studentId, sectionId, statusCode);
                Assert.AreEqual(studentAcadCred.RecordGuid, registrationResponse.Guid);
                Assert.AreEqual(studentAcadCred.StcPersonId, registrationResponse.StudentId);
                Assert.AreEqual(studentCourseSec.ScsCourseSection, registrationResponse.SectionId);
                Assert.AreEqual(studentCourseSec.ScsPassAudit, registrationResponse.PassAudit);
                Assert.AreEqual(studentAcadCred.StcCredType, registrationResponse.CreditType);
                Assert.AreEqual(studentAcadCred.StcAcadLevel, registrationResponse.AcademicLevel);
                Assert.AreEqual(studentAcadCred.StcCeus, registrationResponse.Ceus);
                Assert.AreEqual(studentAcadCred.StcCred, registrationResponse.Credit);
                Assert.AreEqual(studentAcadCred.StcStartDate, registrationResponse.InvolvementStartOn);
                Assert.AreEqual(studentAcadCred.StcEndDate, registrationResponse.InvolvementEndOn);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task RegistrationRequest_ReturnsErrorException()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);

                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);
                var studentId = studentAcadCred.StcPersonId;
                var sectionId = studentCourseSec.ScsCourseSection;
                var statusCode = academicCredit.Status.ToString();
                var sectionRegistration = new SectionRegistration()
                {
                    SectionId = sectionId,
                    Action = RegistrationAction.Add,
                };

                request = new SectionRegistrationRequest(studentId, studentAcadCred.RecordGuid, sectionRegistration);
                response = new UpdateSectionRegistrationResponse()
                {
                    ErrorOccurred = true,
                    ErrorMessage = "Error",
                    RegMessages = new List<RegMessages>()
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(It.IsAny<UpdateSectionRegistrationRequest>())).ReturnsAsync(response);

                // Now mock that response
                dataAccessorMock.Setup<Task<StudentAcadCred>>(acc => acc.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string>(), true)).ReturnsAsync(studentAcadCred);
                dataAccessorMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string>(), true)).ReturnsAsync(studentCourseSec);
                var guid = studentAcadCred.RecordGuid;
                var id = studentAcadCred.Recordkey;
                var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.ACAD.CRED", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var registrationResponse = await sectionRegistrationRepo.Update2Async(request, guid, studentId, sectionId, statusCode);
            }

            private SectionRegistrationRepository BuildValidSectionRegistrationRepository()
            {
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                sectionRegistrationRepo = new SectionRegistrationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return sectionRegistrationRepo;

            }

            private StudentAcadCred BuildValidStcResponse(AcademicCredit ac)
            {
                StudentAcadCred stc = new StudentAcadCred();
                stc.RecordGuid = Guid.NewGuid().ToString();
                stc.Recordkey = ac.Id;
                stc.StcPersonId = ac.Id;
                stc.StcAcadLevel = ac.AcademicLevelCode;
                stc.StcCeus = ac.ContinuingEducationUnits;
                if (ac.Course != null)
                {
                    stc.StcCourse = ac.Course.Id;
                    stc.StcCourseLevel = ac.Course.CourseLevelCodes.First();
                }
                stc.StcCourseName = ac.CourseName;
                stc.StcCred = ac.Credit;
                string typecode;
                switch (ac.Type)
                {
                    case CreditType.ContinuingEducation:
                        typecode = "CE";
                        break;
                    case CreditType.Institutional:
                        typecode = "IN";
                        break;
                    case CreditType.Transfer:
                        typecode = "TRN";
                        break;
                    default:
                        typecode = "OTH";
                        break;
                }
                stc.StcCredType = typecode;
                // For one academic credit leave the departments blank and be sure they get defaulted from the course
                if (ac.Id == "1")
                {
                    stc.StcDepts = new List<string>();
                }
                else
                {
                    stc.StcDepts = ac.DepartmentCodes.ToList();
                }
                stc.StcStartDate = ac.StartDate;
                stc.StcEndDate = ac.EndDate;
                stc.StcGradeScheme = ac.GradeSchemeCode;
                stc.StcCred = ac.Credit;
                stc.StcCeus = stc.StcCeus;
                stc.StcSectionNo = ac.SectionNumber;
                stc.StcStudentEquivEval = ac.CourseName;
                stc.StcStartDate = ac.StartDate;
                stc.StcEndDate = ac.EndDate;

                // Status 
                string stat = "";
                switch (ac.Status)
                {
                    case CreditStatus.Add: { stat = "A"; break; }
                    case CreditStatus.Cancelled: { stat = "C"; break; }
                    case CreditStatus.Deleted: { stat = "X"; break; }
                    case CreditStatus.Dropped: { stat = "D"; break; }
                    case CreditStatus.New: { stat = "N"; break; }
                    case CreditStatus.Preliminary: { stat = "PR"; break; }
                    case CreditStatus.TransferOrNonCourse: { stat = "TR"; break; }
                    case CreditStatus.Withdrawn: { stat = "W"; break; }
                    default: { stat = ""; break; }
                }

                stc.StcStatus = new List<string>() { stat };

                stc.StcStatusesEntityAssociation = new List<StudentAcadCredStcStatuses>();
                StudentAcadCredStcStatuses statusitem = new StudentAcadCredStcStatuses(
                                                                DateTime.Now,
                                                                ac.Status.ToString()[0].ToString(),
                                                                DateTime.Now,
                                                                "");
                stc.StcStatusesEntityAssociation.Add(statusitem);
                stc.StcStudentCourseSec = ac.Id; // Not real life example here.  The SCS record would have a diff-
                // erent ID in real life, but we don't keep it here, so for mocking
                // we will pretend it is the same as the STC record ID.
                stc.StcSubject = ac.SubjectCode;
                stc.StcTerm = ac.TermCode;
                return stc;
            }

            private StudentCourseSec BuildValidScsResponse(AcademicCredit ac)
            {
                StudentCourseSec scs = new StudentCourseSec();
                scs.Recordkey = ac.Id;
                scs.ScsCourseSection = ac.SectionId;
                switch (ac.GradingType)
                {
                    case GradingType.Graded:
                        scs.ScsPassAudit = "";
                        break;
                    case GradingType.PassFail:
                        scs.ScsPassAudit = "P";
                        break;
                    case GradingType.Audit:
                        scs.ScsPassAudit = "A";
                        break;
                    default:
                        break;
                }
                return scs;
            }
        }

        [TestClass]
        public class UpdateSectionRegistrationGradesTests
        {
            SectionRegistrationRepository sectionRegistrationRepo;
            List<AcademicCredit> academicCreditData;
            ImportGradesRequest importGradeRequest;
            ImportGradesResponse importGradeResponse;
            SectionRegistrationRequest request;
            SectionRegistrationResponse sectionRegResponse;
            UpdateSectionRegistrationResponse updateSectionRegresponse;

            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            IColleagueTransactionInvoker transManager;
            IColleagueDataReader dataAccessor;

            [TestInitialize]
            public void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                dataAccessor = dataAccessorMock.Object;
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                transManager = transManagerMock.Object;

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessor);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);


            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationRepo = null;
                importGradeRequest = null;
                importGradeResponse = null;
            }

            [TestMethod]
            public async Task SectionRegistrationRepo_UpdateGrades_Test()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);
                var studentId = studentAcadCred.StcPersonId;
                var sectionId = studentCourseSec.ScsCourseSection;
                var statusCode = academicCredit.Status.ToString();
                var sectionRegistration = new SectionRegistration()
                {
                    SectionId = sectionId,
                    Action = RegistrationAction.Add,
                };

                request = new SectionRegistrationRequest(studentId, studentAcadCred.RecordGuid, sectionRegistration);
                BuildSectRegGradesReq(request);

                sectionRegResponse = new SectionRegistrationResponse(studentAcadCred.RecordGuid, studentId, sectionId, statusCode, studentAcadCred.StcGradeScheme, studentCourseSec.ScsPassAudit, new List<RegistrationMessage>());

                updateSectionRegresponse = new UpdateSectionRegistrationResponse();
                updateSectionRegresponse.SecRegGuid = studentAcadCred.RecordGuid;

                importGradeResponse = new ImportGradesResponse();
                importGradeResponse.GradeMessages = new List<GradeMessages>() 
                { 
                    new GradeMessages()
                    {
                        StatusCode = "SUCCESS",
                        ErrorMessge = "Error occured",
                        InfoMessage = "Info Message"
                    }
                };

                importGradeResponse.Guid = "0335b9e8-ba5a-4099-8633-bcb15b5ca9cb";
                importGradeResponse.SectionRegId = "19359";
                importGradeResponse.Grades = BuildGrades();

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<ImportGradesRequest, ImportGradesResponse>(It.IsAny <ImportGradesRequest>())).ReturnsAsync(importGradeResponse);

                var results = await sectionRegistrationRepo.UpdateGradesAsync(sectionRegResponse, request);

                Assert.AreEqual(request.FinalTermGrade.GradeKey, results.FinalTermGrade.GradeId);
                Assert.AreEqual("FINAL", results.FinalTermGrade.GradeTypeCode);
                Assert.AreEqual(request.FinalTermGrade.SubmittedBy, results.FinalTermGrade.SubmittedBy);
                Assert.AreEqual(request.FinalTermGrade.SubmittedOn, results.FinalTermGrade.SubmittedOn);
                Assert.AreEqual(request.FinalTermGrade.GradeChangeReason, results.FinalTermGrade.GradeChangeReason);
                Assert.AreEqual(request.FinalTermGrade.GradeTypeCode, results.FinalTermGrade.GradeTypeCode);
                Assert.AreEqual(request.VerifiedTermGrade.GradeKey, results.VerifiedTermGrade.GradeId);
                
                Assert.AreEqual("VERIFIED", results.VerifiedTermGrade.GradeTypeCode);
                Assert.AreEqual(request.VerifiedTermGrade.SubmittedBy, results.VerifiedTermGrade.SubmittedBy);
                Assert.AreEqual(request.VerifiedTermGrade.SubmittedOn, results.VerifiedTermGrade.SubmittedOn);
                Assert.AreEqual(request.VerifiedTermGrade.GradeChangeReason, results.VerifiedTermGrade.GradeChangeReason);
                Assert.AreEqual(request.VerifiedTermGrade.GradeTypeCode, results.VerifiedTermGrade.GradeTypeCode);

                Assert.AreEqual(6, results.MidTermGrades.Count());

                for (int i = 1; i <= 6; i++)
                {
                    Assert.AreEqual(request.MidTermGrades[i - 1].GradeKey, results.MidTermGrades[i - 1].GradeId);
                    Assert.AreEqual(i, results.MidTermGrades[i - 1].Position);
                    Assert.AreEqual(request.MidTermGrades[i - 1].SubmittedBy, results.MidTermGrades[i -1].SubmittedBy);
                    Assert.AreEqual(request.MidTermGrades[i -1].GradeTimestamp, results.MidTermGrades[i -1].GradeTimestamp);
                    Assert.AreEqual(request.MidTermGrades[i - 1].GradeChangeReason, results.MidTermGrades[i - 1].GradeChangeReason);
                    Assert.AreEqual(request.MidTermGrades[i - 1].GradeTypeCode, results.MidTermGrades[i - 1].GradeTypeCode);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task SectionRegistrationRepo_UpdateGrades_InvalidOperationException()
            {
                academicCreditData = (await new TestAcademicCreditRepository().GetAsync()).ToList();
                sectionRegistrationRepo = BuildValidSectionRegistrationRepository();
                var academicCredit = academicCreditData.ElementAt(0);
                StudentAcadCred studentAcadCred = BuildValidStcResponse(academicCredit);
                StudentCourseSec studentCourseSec = BuildValidScsResponse(academicCredit);
                var studentId = studentAcadCred.StcPersonId;
                var sectionId = studentCourseSec.ScsCourseSection;
                var statusCode = academicCredit.Status.ToString();
                var sectionRegistration = new SectionRegistration()
                {
                    SectionId = sectionId,
                    Action = RegistrationAction.Add,
                };

                request = new SectionRegistrationRequest(studentId, studentAcadCred.RecordGuid, sectionRegistration);
                BuildSectRegGradesReq(request);

                sectionRegResponse = new SectionRegistrationResponse(studentAcadCred.RecordGuid, studentId, sectionId, statusCode, studentAcadCred.StcGradeScheme, studentCourseSec.ScsPassAudit, 
                    new List<RegistrationMessage>());

                updateSectionRegresponse = new UpdateSectionRegistrationResponse();
                updateSectionRegresponse.SecRegGuid = studentAcadCred.RecordGuid;

                importGradeResponse = new ImportGradesResponse();
                importGradeResponse.GradeMessages = new List<GradeMessages>() 
                { 
                    new GradeMessages()
                    {
                        StatusCode = "FAILURE",
                        ErrorMessge = "Error occured"
                    }
                };
                importGradeResponse.Grades = BuildGrades();

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<ImportGradesRequest, ImportGradesResponse>(It.IsAny<ImportGradesRequest>())).ReturnsAsync(importGradeResponse);

                var results = await sectionRegistrationRepo.UpdateGradesAsync(sectionRegResponse, request);                
            }

            private void BuildSectRegGradesReq(SectionRegistrationRequest request)
            {
                request.FinalTermGrade = new TermGrade("0335b9e8-ba5a-4099-8633-bcb15b5ca9cb", new DateTimeOffset(new DateTime(2016, 01, 01, 0,0,0)), "0012297", "1");
                request.FinalTermGrade.GradeKey = "14";
                request.FinalTermGrade.GradeTypeCode = "FINAL";
                
                request.VerifiedTermGrade = new VerifiedTermGrade("0335b9e8-ba5a-4099-8633-bcb15b5ca9cb", new DateTimeOffset(new DateTime(2016, 01, 01, 0, 0, 0)), "0012297", "1");
                request.VerifiedTermGrade.GradeKey = "14";
                request.VerifiedTermGrade.GradeTypeCode = "VERIFIED";

                request.MidTermGrades = new List<MidTermGrade>();
                for (int i = 1; i <= 6; i++)
                {
                    request.MidTermGrades.Add(new MidTermGrade(i, "0335b9e8-ba5a-4099-8633-bcb15b5ca9cb", new DateTimeOffset(new DateTime(2016, 01, 01, 0, 0, 0))) { GradeKey = "14" , GradeTypeCode = string.Concat("MID", i.ToString())});
                }
            }

            private List<Transactions.Grades> BuildGrades()
            {
                List<Transactions.Grades> grades = new List<Transactions.Grades>();
                
                //Build Final Grades
                Transactions.Grades finalGrade = new Transactions.Grades
                {
                    Grade = "14",
                    GradeExpiry = string.Empty,
                    GradeKey = "14",
                    GradeSubmitBy = "0012297",
                    GradeSubmitDate = "2016/01/01",
                    GradeType = "FINAL",
                    InvEndOn = new DateTime(2016, 04, 01),
                    InvStartOn = new DateTime(2016, 01, 01),
                    LastDayAttendDate = string.Empty,
                    NeverAttend = "N",
                    GradeChangeReason = "OE"
                };
                grades.Add(finalGrade);

                //Build Verified Grades
                Transactions.Grades verifiedGrade = new Transactions.Grades
                {
                    Grade = "14",
                    GradeExpiry = string.Empty,
                    GradeKey = "14",
                    GradeSubmitBy = "0012297",
                    GradeSubmitDate = "2016/01/01",
                    GradeType = "VERIFIED",
                    InvEndOn = new DateTime(2016, 04, 01),
                    InvStartOn = new DateTime(2016, 01, 01),
                    LastDayAttendDate = string.Empty,
                    NeverAttend = "N",
                    GradeChangeReason = "EE"
                };
                grades.Add(verifiedGrade);

                //Build Midterm Grades

                for (var i = 1; i <= 6; i++)
                {
                    Transactions.Grades midTermGrade = new Transactions.Grades
                    {
                        Grade = "14",
                        GradeExpiry = string.Empty,
                        GradeKey = "14",
                        GradeSubmitBy = "0012297",
                        GradeSubmitDate = "2016/01/01",
                        GradeType = string.Concat("MID", i.ToString()),
                        InvEndOn = new DateTime(2016, 04, 01),
                        InvStartOn = new DateTime(2016, 01, 01),
                        LastDayAttendDate = string.Empty,
                        NeverAttend = "N",
                        GradeChangeReason = "OE", 
                        
                    };

                    grades.Add(midTermGrade);
                }
                return grades;
            }

            private SectionRegistrationRepository BuildValidSectionRegistrationRepository()
            {
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                sectionRegistrationRepo = new SectionRegistrationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return sectionRegistrationRepo;

            }

            private StudentAcadCred BuildValidStcResponse(AcademicCredit ac)
            {
                StudentAcadCred stc = new StudentAcadCred();

                stc.RecordGuid = Guid.NewGuid().ToString();
                stc.Recordkey = ac.Id;
                stc.StcPersonId = ac.Id;
                stc.StcAcadLevel = ac.AcademicLevelCode;
                stc.StcAllowReplFlag = ac.CanBeReplaced ? "Y" : "";
                stc.StcAltcumContribCmplCred = ac.AdjustedCredit;
                stc.StcAltcumContribGpaCred = ac.AdjustedGpaCredit;
                stc.StcAltcumContribGradePts = ac.AdjustedGradePoints;
                stc.StcCeus = ac.ContinuingEducationUnits;
                if (ac.Course != null)
                {
                    stc.StcCourse = ac.Course.Id;
                    stc.StcCourseLevel = ac.Course.CourseLevelCodes.First();
                }
                stc.StcCourseName = ac.CourseName;
                stc.StcCred = ac.Credit;
                string typecode;
                switch (ac.Type)
                {
                    case CreditType.ContinuingEducation:
                        typecode = "CE";
                        break;
                    case CreditType.Institutional:
                        typecode = "IN";
                        break;
                    case CreditType.Transfer:
                        typecode = "TRN";
                        break;
                    default:
                        typecode = "OTH";
                        break;
                }
                stc.StcCredType = typecode;
                // For one academic credit leave the departments blank and be sure they get defaulted from the course
                if (ac.Id == "1")
                {
                    stc.StcDepts = new List<string>();
                }
                else
                {
                    stc.StcDepts = ac.DepartmentCodes.ToList();
                }
                stc.StcStartDate = ac.StartDate;
                stc.StcEndDate = ac.EndDate;
                stc.StcGpaCred = ac.GpaCredit;
                stc.StcGradePts = ac.GradePoints;
                stc.StcGradeScheme = ac.GradeSchemeCode;
                stc.StcCmplCred = ac.CompletedCredit;
                stc.StcAttCred = ac.AttemptedCredit;
                stc.StcSectionNo = ac.SectionNumber;
                stc.StcStudentEquivEval = ac.CourseName;
                //stc.StcMark 
                stc.StcReplCode = ac.ReplacedStatus == ReplacedStatus.Replaced ? "R" : null;
                stc.StcRepeatedAcadCred = ac.RepeatAcademicCreditIds;

                // Status 
                string stat = "";
                switch (ac.Status)
                {
                    case CreditStatus.Add: { stat = "A"; break; }
                    case CreditStatus.Cancelled: { stat = "C"; break; }
                    case CreditStatus.Deleted: { stat = "X"; break; }
                    case CreditStatus.Dropped: { stat = "D"; break; }
                    case CreditStatus.New: { stat = "N"; break; }
                    case CreditStatus.Preliminary: { stat = "PR"; break; }
                    case CreditStatus.TransferOrNonCourse: { stat = "TR"; break; }
                    case CreditStatus.Withdrawn: { stat = "W"; break; }
                    default: { stat = ""; break; }
                }

                stc.StcStatus = new List<string>() { stat };

                stc.StcStatusesEntityAssociation = new List<StudentAcadCredStcStatuses>();
                StudentAcadCredStcStatuses statusitem = new StudentAcadCredStcStatuses(
                                                                DateTime.Now,
                                                                ac.Status.ToString()[0].ToString(),
                                                                DateTime.Now,
                                                                "");
                stc.StcStatusesEntityAssociation.Add(statusitem);
                stc.StcStudentCourseSec = ac.Id; // Not real life example here.  The SCS record would have a diff-
                // erent ID in real life, but we don't keep it here, so for mocking
                // we will pretend it is the same as the STC record ID.
                stc.StcSubject = ac.SubjectCode;
                stc.StcTerm = ac.TermCode;
                //stc.StcTitle I don't think we use this
                if (ac.VerifiedGrade != null)
                {
                    stc.StcVerifiedGrade = ac.VerifiedGrade.Id;
                    // added for mobile
                    stc.StcVerifiedGradeDate = new DateTime(ac.VerifiedGradeTimestamp.Value.Year, ac.VerifiedGradeTimestamp.Value.Month, ac.VerifiedGradeTimestamp.Value.Day, 0, 0, 0);
                    // end added for mobile
                }
                return stc;
            }

            private StudentCourseSec BuildValidScsResponse(AcademicCredit ac)
            {
                StudentCourseSec scs = new StudentCourseSec();
                scs.Recordkey = ac.Id;
                scs.ScsCourseSection = ac.SectionId;
                switch (ac.GradingType)
                {
                    case GradingType.Graded:
                        scs.ScsPassAudit = "";
                        break;
                    case GradingType.PassFail:
                        scs.ScsPassAudit = "P";
                        break;
                    case GradingType.Audit:
                        scs.ScsPassAudit = "A";
                        break;
                    default:
                        break;
                }
                // added for mobile
                // for each midtermgrade in the list, populate the correct field 
                // Not necessarily sequential starting from 1!!! Mock the time as
                // midnight, which we won't see if we get a time from the SCSCC
                foreach (MidTermGrade mtg in ac.MidTermGrades)
                {
                    switch (mtg.Position)
                    {
                        case 1:
                            scs.ScsMidTermGrade1 = mtg.GradeId;
                            scs.ScsMidGradeDate1 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 2:
                            scs.ScsMidTermGrade2 = mtg.GradeId;
                            scs.ScsMidGradeDate2 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 3:
                            scs.ScsMidTermGrade3 = mtg.GradeId;
                            scs.ScsMidGradeDate3 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 4:
                            scs.ScsMidTermGrade4 = mtg.GradeId;
                            scs.ScsMidGradeDate4 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 5:
                            scs.ScsMidTermGrade5 = mtg.GradeId;
                            scs.ScsMidGradeDate5 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                        case 6:
                            scs.ScsMidTermGrade6 = mtg.GradeId;
                            scs.ScsMidGradeDate6 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                            break;
                    }
                }
                // end added for mobile
                return scs;
            }
        }
    }
}

