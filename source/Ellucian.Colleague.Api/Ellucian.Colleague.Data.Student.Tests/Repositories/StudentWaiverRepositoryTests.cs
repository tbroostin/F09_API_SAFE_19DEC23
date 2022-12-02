// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentWaiverRepositoryTests
    {
        [TestClass]
        public class StudentWaiverRepository_GetSectionWaivers
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            Collection<StudentReqWaivers> waiversResponseData;
            ApiSettings apiSettings;
            StudentWaiverRepository waiverRepo;
            string multiLineComment;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Collection of data accessor responses
                waiversResponseData = BuildWaiversResponse();

                waiverRepo = BuildValidWaiverRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
                waiversResponseData = null;
                waiverRepo = null;
            }

            [TestMethod]
            public async Task GetsAllWaiversforSection()
            {
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");
                Assert.AreEqual(3, waivers.Count);
            }

            [TestMethod]
            public async Task StudentWaiver_Initialized()
            {
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");
                foreach (var response in waiversResponseData)
                {
                    var waiver = waivers.Where(w => w.Id == response.Recordkey).FirstOrDefault();
                    Assert.AreEqual(response.Recordkey, waiver.Id);
                    Assert.AreEqual(response.SrwvCourse, waiver.CourseId);
                    Assert.AreEqual(response.SrwvEndDate, waiver.EndDate);
                    Assert.AreEqual(response.SrwvReason, waiver.ReasonCode);
                    Assert.AreEqual(response.SrwvSection, waiver.SectionId);
                    Assert.AreEqual(response.SrwvStartDate, waiver.StartDate);
                    Assert.AreEqual(response.SrwvStudent, waiver.StudentId);
                    Assert.AreEqual(response.SrwvTerm, waiver.TermCode);
                    Assert.AreEqual(response.SrwvWaiverPersonId, waiver.AuthorizedBy);
                    Assert.AreEqual(response.StudentReqWaiversChgopr, waiver.ChangedBy);
                    Assert.AreEqual(response.StudentReqWaiversChgdate, new DateTime(waiver.DateTimeChanged.Year, waiver.DateTimeChanged.Month, waiver.DateTimeChanged.Day));
                    Assert.AreEqual(new TimeSpan(response.StudentReqWaiversChgtime.Value.Hour, response.StudentReqWaiversChgtime.Value.Minute, response.StudentReqWaiversChgtime.Value.Second),
                        new TimeSpan(waiver.DateTimeChanged.Hour, waiver.DateTimeChanged.Minute, waiver.DateTimeChanged.Second));
                }
            }

            [TestMethod]
            public async Task RequisiteWaivers_Initialized()
            {
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");
                foreach (var response in waiversResponseData)
                {
                    var waiver = waivers.Where(w => w.Id == response.Recordkey).FirstOrDefault();
                    foreach (var responseReqWaiver in response.SrwvReqCoursesEntityAssociation)
                    {
                        var reqWaiver = waiver.RequisiteWaivers.Where(rw => rw.RequisiteId == responseReqWaiver.SrwvAcadReqmtsAssocMember).FirstOrDefault();
                        Assert.AreEqual(responseReqWaiver.SrwvAcadReqmtsAssocMember, reqWaiver.RequisiteId);
                        switch (responseReqWaiver.SrwvWaiveReqmtFlagAssocMember.ToUpper())
                        {
                            case "":
                                Assert.AreEqual(WaiverStatus.NotSelected, reqWaiver.Status);
                                break;
                            case "Y":
                                Assert.AreEqual(WaiverStatus.Waived, reqWaiver.Status);
                                break;
                            case "N":
                                Assert.AreEqual(WaiverStatus.Denied, reqWaiver.Status);
                                break;
                            default:
                                Assert.AreEqual(WaiverStatus.NotSelected, reqWaiver.Status);
                                break;
                        }
                    }
                }
            }

            [TestMethod]
            public async Task ReplacesValueMarksWithLineBreaksInComment()
            {
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");

                var expectedCommentLines = multiLineComment.Replace(Convert.ToChar(DynamicArray.VM), '\n');
                Assert.AreEqual(expectedCommentLines, waivers.ElementAt(0).Comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionIfAccessReturnsException()
            {
                StudentWaiverRepository waiverRepo = BuildInvalidWaiverRepository();
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfSectionIdArgumentNull()
            {
                var waivers = await waiverRepo.GetSectionWaiversAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfSectionIdArgumentEmpty()
            {
                var waivers = await waiverRepo.GetSectionWaiversAsync(string.Empty);
            }

            [TestMethod]
            public async Task EmptyRepoDataReturnsEmptyList()
            {
                // Set up repo response for waivers request
                Collection<StudentReqWaivers> emptyResponse = new Collection<StudentReqWaivers>();
                dataAccessorMock.Setup<Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(emptyResponse);
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");
                Assert.AreEqual(0, waivers.Count());
            }

            [TestMethod]
            public async Task NullRepoDataReturnsEmptyList()
            {
                // Set up repo response for waivers request
                Collection<StudentReqWaivers> nullResponse = new Collection<StudentReqWaivers>();
                nullResponse = null;
                dataAccessorMock.Setup<Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(nullResponse);
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");
                Assert.AreEqual(0, waivers.Count());
            }

            [TestMethod]
            public async Task ReturnsOnlyValidItems()
            {
                // Add a bad waiver to the normal response
                var badWaiverData = new StudentReqWaivers();
                badWaiverData.Recordkey = "1";
                badWaiverData.SrwvStudent = "0000123";
                badWaiverData.SrwvSection = "SEC1";
                badWaiverData.SrwvCourse = "CRS1";
                badWaiverData.SrwvTerm = "2016/FA";
                badWaiverData.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();
                var mixedResponseData = waiversResponseData;
                mixedResponseData.Add(badWaiverData);
                dataAccessorMock.Setup<Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(waiversResponseData);

                // Verify that the original (two) waiver response items are returned even though an additional item has invalid data
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");
                Assert.AreEqual(3, waivers.Count());
            }

            [TestMethod]
            public async Task ReturnsRevokedWaiver()
            {
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");
                Assert.AreEqual(1, waivers.Where(w => w.IsRevoked == true).Count());
            }

            [TestMethod]
            public async Task ReturnsOnlyValidRequisiteWaiverItems()
            {
                waiversResponseData.ElementAt(1).SrwvReqCoursesEntityAssociation.ElementAt(0).SrwvAcadReqmtsAssocMember = null;
                dataAccessorMock.Setup < Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(waiversResponseData);

                // Verify that the original (two) waiver response items are returned even though one of the requisite items has invalid data
                var waivers = await waiverRepo.GetSectionWaiversAsync("SEC1");
                Assert.AreEqual(3, waivers.Count());
            }

            private StudentWaiverRepository BuildValidWaiverRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                localCacheMock = new Mock<ObjectCache>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for waivers request
                dataAccessorMock.Setup < Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(waiversResponseData);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                StudentWaiverRepository repository = new StudentWaiverRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentWaiverRepository BuildInvalidWaiverRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                Exception expectedFailure = new Exception("fail");

                dataAccessorMock.Setup < Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).Throws(expectedFailure);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                
                StudentWaiverRepository repository = new StudentWaiverRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return repository;
            }

            private Collection<StudentReqWaivers> BuildWaiversResponse()
            {
                Collection<StudentReqWaivers> repoWaivers = new Collection<StudentReqWaivers>();

                var waiverData = new StudentReqWaivers();

                // This student has no specific requisite waiver information
                waiverData.Recordkey = "1";
                waiverData.SrwvStudent = "0000123";
                waiverData.SrwvSection = "SEC1";
                waiverData.SrwvTerm = "2016/SP";
                waiverData.SrwvReason = "OTHER";
                multiLineComment = "Student 456 comment. Line1" + Convert.ToChar(DynamicArray.VM) + "comment line2" + Convert.ToChar(DynamicArray.VM) + "comment line3 the end";
                waiverData.SrwvComments = multiLineComment;
                waiverData.SrwvWaiverPersonId = "0000987";
                waiverData.StudentReqWaiversChgdate = new DateTime(2015, 01, 21);
                var chTime = DateTime.MinValue;
                chTime.AddHours(21);
                chTime.AddMinutes(01);
                chTime.AddSeconds(15);
                waiverData.StudentReqWaiversChgtime = chTime;
                waiverData.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();

                repoWaivers.Add(waiverData);

                // Three attached requisite waivers
                var waiverData1 = new StudentReqWaivers();
                waiverData1.Recordkey = "2";
                waiverData1.SrwvStudent = "0000456";
                waiverData1.SrwvSection = "SEC1";
                waiverData1.SrwvTerm = "2016/SP";
                waiverData1.SrwvReason = "LIFE";
                waiverData1.SrwvComments = "Student 456 comment line.";
                waiverData1.SrwvWaiverPersonId = "0000987";
                waiverData1.StudentReqWaiversChgdate = new DateTime(2015, 02, 14);
                var chTime1 = DateTime.MinValue;
                chTime1.AddHours(14);
                chTime1.AddMinutes(02);
                chTime1.AddSeconds(15);
                waiverData1.StudentReqWaiversChgtime = chTime1;
                waiverData1.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();
                // Waive requisite 1
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "REQ1",
                    SrwvWaiveReqmtFlagAssocMember = "y"
                });
                // Deny requisite 2
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "REQ2",
                    SrwvWaiveReqmtFlagAssocMember = "N"
                });
                // No action on requisite 3
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "REQ3",
                    SrwvWaiveReqmtFlagAssocMember = ""
                });


                repoWaivers.Add(waiverData1);

                // This student has a revoked waiver 
                var waiverData2 = new StudentReqWaivers();
                waiverData2.Recordkey = "3";
                waiverData2.SrwvStudent = "0000789";
                waiverData2.SrwvSection = "SEC1";
                waiverData2.SrwvTerm = "2016/SP";
                waiverData2.SrwvReason = "OTHER";
                waiverData2.SrwvWaiverPersonId = "0000987";
                waiverData2.StudentReqWaiversChgdate = new DateTime(2015, 01, 21);
                chTime.AddHours(21);
                chTime.AddMinutes(01);
                chTime.AddSeconds(15);
                waiverData2.StudentReqWaiversChgtime = chTime;
                waiverData2.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();
                waiverData2.SrwvRevokedFlag = "Y";
                repoWaivers.Add(waiverData2);

                return repoWaivers;
            }
        }

        [TestClass]
        public class StudentWaiverRepository_Get
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            StudentReqWaivers response;
            ApiSettings apiSettings;
            StudentWaiverRepository waiverRepo;
            string multiLineComment;
            string waiverId;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                waiverId = "2";

                // Collection of data accessor responses
                response = BuildWaiverResponse();

                waiverRepo = BuildValidWaiverRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
                response = null;
                waiverRepo = null;
            }

            [TestMethod]
            public async Task GetsWaiver()
            {
                var waiver = await waiverRepo.GetAsync(waiverId);
                Assert.AreEqual(waiverId, waiver.Id);
            }

            [TestMethod]
            public async Task StudentWaiver_Initialized()
            {
                var waiver = await waiverRepo.GetAsync(waiverId);
                Assert.AreEqual(response.Recordkey, waiver.Id);
                Assert.AreEqual(response.SrwvCourse, waiver.CourseId);
                Assert.AreEqual(response.SrwvEndDate, waiver.EndDate);
                Assert.AreEqual(response.SrwvReason, waiver.ReasonCode);
                Assert.AreEqual(response.SrwvSection, waiver.SectionId);
                Assert.AreEqual(response.SrwvStartDate, waiver.StartDate);
                Assert.AreEqual(response.SrwvStudent, waiver.StudentId);
                Assert.AreEqual(response.SrwvTerm, waiver.TermCode);
                Assert.AreEqual(response.SrwvWaiverPersonId, waiver.AuthorizedBy);
                Assert.AreEqual(response.StudentReqWaiversChgopr, waiver.ChangedBy);
                Assert.AreEqual(response.StudentReqWaiversChgdate, new DateTime(waiver.DateTimeChanged.Year, waiver.DateTimeChanged.Month, waiver.DateTimeChanged.Day));
                Assert.AreEqual(new TimeSpan(response.StudentReqWaiversChgtime.Value.Hour, response.StudentReqWaiversChgtime.Value.Minute, response.StudentReqWaiversChgtime.Value.Second),
                    new TimeSpan(waiver.DateTimeChanged.Hour, waiver.DateTimeChanged.Minute, waiver.DateTimeChanged.Second));
            }

            [TestMethod]
            public async Task RequisiteWaivers_Initialized()
            {
                var waiver = await waiverRepo.GetAsync(waiverId);
                    foreach (var responseReqWaiver in response.SrwvReqCoursesEntityAssociation)
                    {
                        var reqWaiver = waiver.RequisiteWaivers.Where(rw => rw.RequisiteId == responseReqWaiver.SrwvAcadReqmtsAssocMember).FirstOrDefault();
                        Assert.AreEqual(responseReqWaiver.SrwvAcadReqmtsAssocMember, reqWaiver.RequisiteId);
                        switch (responseReqWaiver.SrwvWaiveReqmtFlagAssocMember.ToUpper())
                        {
                            case "":
                                Assert.AreEqual(WaiverStatus.NotSelected, reqWaiver.Status);
                                break;
                            case "Y":
                                Assert.AreEqual(WaiverStatus.Waived, reqWaiver.Status);
                                break;
                            case "N":
                                Assert.AreEqual(WaiverStatus.Denied, reqWaiver.Status);
                                break;
                            default:
                                Assert.AreEqual(WaiverStatus.NotSelected, reqWaiver.Status);
                                break;
                        }
                    }
            }

            [TestMethod]
            public async Task ReplacesValueMarksWithLineBreaksInComment()
            {
                var waiver = await waiverRepo.GetAsync(waiverId);

                var expectedCommentLines = multiLineComment.Replace(Convert.ToChar(DynamicArray.VM), '\n');
                Assert.AreEqual(expectedCommentLines, waiver.Comment);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ThrowsExceptionIfAccessReturnsException()
            {
                StudentWaiverRepository waiverRepo = BuildInvalidWaiverRepository();
                var waiver = await waiverRepo.GetAsync(waiverId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfWaiverIdArgumentNull()
            {
                var waiver = await waiverRepo.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfWaiverIdArgumentEmpty()
            {
                var waiver = await waiverRepo.GetAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ThrowsExceptionIfRepoReponseNull()
            {
                // Set up repo response for waivers request
                StudentReqWaivers nullResponse = null;
                dataAccessorMock.Setup < Task<StudentReqWaivers>>(acc => acc.ReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(nullResponse);
                var waiver = await waiverRepo.GetAsync(waiverId);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ThrowsExceptionIfExceptionThrownByDataReader()
            {
                dataAccessorMock.Setup <Task<StudentReqWaivers>>(acc => acc.ReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).Throws(new Exception());
                var waiver = await waiverRepo.GetAsync(waiverId);
            }

             private StudentWaiverRepository BuildValidWaiverRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                localCacheMock = new Mock<ObjectCache>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for waivers request
                dataAccessorMock.Setup<Task<StudentReqWaivers>>(acc => acc.ReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(response);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                
                 StudentWaiverRepository repository = new StudentWaiverRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentWaiverRepository BuildInvalidWaiverRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                Exception expectedFailure = new Exception("fail");

                dataAccessorMock.Setup<Task<StudentReqWaivers>>(acc => acc.ReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).Throws(expectedFailure);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                
                StudentWaiverRepository repository = new StudentWaiverRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return repository;
            }

            private StudentReqWaivers BuildWaiverResponse()
            {
                // Three attached requisite waivers
                var waiverData1 = new StudentReqWaivers();
                waiverData1.Recordkey = "2";
                waiverData1.SrwvStudent = "0000456";
                waiverData1.SrwvSection = "SEC1";
                waiverData1.SrwvTerm = "2016/SP";
                waiverData1.SrwvReason = "LIFE";
                multiLineComment = "Student 456 comment. Line1" + Convert.ToChar(DynamicArray.VM) + "comment line2" + Convert.ToChar(DynamicArray.VM) + "comment line3 the end";
                waiverData1.SrwvComments = multiLineComment;
                waiverData1.SrwvWaiverPersonId = "0000987";
                waiverData1.StudentReqWaiversChgdate = new DateTime(2015, 02, 14);
                var chTime1 = DateTime.MinValue;
                chTime1.AddHours(14);
                chTime1.AddMinutes(02);
                chTime1.AddSeconds(15);
                waiverData1.StudentReqWaiversChgtime = chTime1;
                waiverData1.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();
                // Waive requisite 1
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "REQ1",
                    SrwvWaiveReqmtFlagAssocMember = "y"
                });
                // Deny requisite 2
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "REQ2",
                    SrwvWaiveReqmtFlagAssocMember = "N"
                });
                // No action on requisite 3
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "REQ3",
                    SrwvWaiveReqmtFlagAssocMember = ""
                });
                return waiverData1;
            }
        }

        [TestClass]
        public class StudentWaiverRepository_CreateSectionWaiver
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> mockManager;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            StudentReqWaivers waiverResponseData;
            ApiSettings apiSettings;
            StudentWaiverRepository waiverRepo;
            string multiLineComment;
            StudentWaiver waiverToAdd;
            CreateStudentReqWaiverRequest createRequest;
            string waiverId;
            string studentId;
            string sectionId;
            string reason;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                waiverId = "2";
                studentId = "0000123";
                sectionId = "SEC1";
                reason = "OTHER";
                multiLineComment = "Student 456 comment. Line1\ncomment line2\r\ncomment line3\rthe end";

                waiverResponseData = BuildWaiverResponse();
                waiverRepo = BuildValidWaiverRepository();

                waiverToAdd = new StudentWaiver(null, studentId, null, sectionId, reason, multiLineComment);

                waiverToAdd.AddRequisiteWaiver(new RequisiteWaiver("RW1", WaiverStatus.Denied));
                waiverToAdd.AddRequisiteWaiver(new RequisiteWaiver("RW2", WaiverStatus.NotSelected));
                waiverToAdd.AddRequisiteWaiver(new RequisiteWaiver("RW3", WaiverStatus.Waived));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
                waiverResponseData = null;
                waiverRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfIncomingWaiverIsNull()
            {
                StudentWaiver nullWaiver = null;
                await waiverRepo.CreateSectionWaiverAsync(nullWaiver);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_StudentId()
            {
                await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
                Assert.AreEqual(waiverToAdd.StudentId, createRequest.AStudentId);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_SectionId()
            {
                await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
                Assert.AreEqual(waiverToAdd.SectionId, createRequest.ASectionId);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_ReasonCode()
            {
                await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
                Assert.AreEqual(waiverToAdd.ReasonCode, createRequest.AReasonCode);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_RequisiteWaivers()
            {
                // Verifies that all requisite waivers in the original waiver are in the request and the status update correctly to Y/N/blank
                await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
                foreach (var item in waiverToAdd.RequisiteWaivers)
                {
                    var updateFlag = string.Empty;
                    if (item.Status == WaiverStatus.Waived) { updateFlag = "Y"; }
                    if (item.Status == WaiverStatus.Denied) { updateFlag = "N"; }
                    var requestWaiver = createRequest.RequirementGroup.Where(r => r.AlAcadReqmtIds == item.RequisiteId).FirstOrDefault();
                    Assert.IsNotNull(requestWaiver);
                    Assert.AreEqual(updateFlag, requestWaiver.AlWaiveReqmtFlag);
                }
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_Comment()
            {
                // Verifies that various types of carriage returns and line feeds are all properly converted to value marks
                await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
                char newLineCharacter = '\n';
                string alternateNewLineCharacter = "\r\n";
                string carriageReturnCharacter = "\r";
                string expectedCommentText = waiverToAdd.Comment.Replace(alternateNewLineCharacter, newLineCharacter.ToString());
                expectedCommentText = expectedCommentText.Replace(carriageReturnCharacter, newLineCharacter.ToString());
                var expectedCommentLines = expectedCommentText.Split(newLineCharacter);
                Assert.AreEqual(4, expectedCommentLines.Count());
                for (int i = 0; i < expectedCommentLines.Count(); i++)
                {
                    Assert.IsTrue(expectedCommentLines[i].Length > 0);
                    Assert.AreEqual(expectedCommentLines[i], createRequest.AlComment.ElementAt(i));
                }
            }

            [TestMethod]
            public async Task ReturnsWaiverWhenCreateSuccessful()
            {
                var newWaiver = await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
                Assert.AreEqual(waiverId, newWaiver.Id);
                Assert.AreEqual(waiverToAdd.StudentId, newWaiver.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenGetDoesNotReturnWaiverForGivenStudent()
            {
                waiverToAdd = new StudentWaiver(null, "9999123", null, sectionId, "OTHER", multiLineComment);
                var newWaiver = await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenGetDoesNotReturnWaiverForGivenSection()
            {
                waiverToAdd = new StudentWaiver(null, studentId, null, "SEC123", "OTHER", multiLineComment);
                var newWaiver = await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsKeyNotFoundExceptionWhenGetReadRecordReturnsNull()
            {
                // Set up repo response for null Get request
                waiverResponseData = null;
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(waiverResponseData);
                var newWaiver = await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingSectionWaiverException))]
            public async Task ThrowsSpecialExceptionWhenExistingSectionWaiverFound()
            {
                CreateStudentReqWaiverResponse createResponse = new CreateStudentReqWaiverResponse();
                createResponse.AErrorOccurred = "1";
                createResponse.AMsg = "Existing waiver found";
                createResponse.AExistingId = "123";
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentReqWaiverRequest, CreateStudentReqWaiverResponse>(It.Is<CreateStudentReqWaiverRequest>(r => !string.IsNullOrEmpty(r.ASectionId)))).ReturnsAsync(createResponse).Callback<CreateStudentReqWaiverRequest>(req => createRequest = req);
                var newWaiver = await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsSimpleExceptionForAllOtherErrors()
            {
                CreateStudentReqWaiverResponse createResponse = new CreateStudentReqWaiverResponse();
                createResponse.AErrorOccurred = "1";
                createResponse.AMsg = "Something bad happened";
                createResponse.AExistingId = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentReqWaiverRequest, CreateStudentReqWaiverResponse>(It.Is<CreateStudentReqWaiverRequest>(r => !string.IsNullOrEmpty(r.ASectionId)))).ReturnsAsync(createResponse).Callback<CreateStudentReqWaiverRequest>(req => createRequest = req);
                var newWaiver = await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenSuccessfulFlagWithNoIdReturned()
            {
                CreateStudentReqWaiverResponse createResponse = new CreateStudentReqWaiverResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentReqWaiverRequest, CreateStudentReqWaiverResponse>(It.Is<CreateStudentReqWaiverRequest>(r => !string.IsNullOrEmpty(r.ASectionId)))).ReturnsAsync(createResponse).Callback<CreateStudentReqWaiverRequest>(req => createRequest = req);
                var newWaiver = await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenTransactionRequestThrowsException()
            {
                CreateStudentReqWaiverResponse createResponse = new CreateStudentReqWaiverResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentReqWaiverRequest, CreateStudentReqWaiverResponse>(It.Is<CreateStudentReqWaiverRequest>(r => !string.IsNullOrEmpty(r.ASectionId)))).Throws(new Exception());
                var newWaiver = await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);                
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenTransactionResponseIsNull()
            {
                CreateStudentReqWaiverResponse createResponse = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentReqWaiverRequest, CreateStudentReqWaiverResponse>(It.Is<CreateStudentReqWaiverRequest>(r => !string.IsNullOrEmpty(r.ASectionId)))).ReturnsAsync(createResponse).Callback<CreateStudentReqWaiverRequest>(req => createRequest = req);
                var newWaiver = await waiverRepo.CreateSectionWaiverAsync(waiverToAdd);
            }

            private StudentWaiverRepository BuildValidWaiverRepository()
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
                CreateStudentReqWaiverResponse createResponse = new CreateStudentReqWaiverResponse();
                createResponse.AErrorOccurred = "";
                createResponse.AMsg = null;
                createResponse.AExistingId = "";
                createResponse.AStudentReqWaiversId = waiverId;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentReqWaiverRequest, CreateStudentReqWaiverResponse>(It.Is<CreateStudentReqWaiverRequest>(r => !string.IsNullOrEmpty(r.ASectionId)))).ReturnsAsync(createResponse).Callback<CreateStudentReqWaiverRequest>(req => createRequest = req);

                // Set up repo response for waiver Get request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(waiverResponseData);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                
                StudentWaiverRepository repository = new StudentWaiverRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentReqWaivers BuildWaiverResponse()
            {

                // Three attached requisite waivers
                var waiverData1 = new StudentReqWaivers();
                waiverData1.Recordkey = waiverId;
                waiverData1.SrwvStudent = studentId;
                waiverData1.SrwvSection = sectionId;
                waiverData1.SrwvTerm = "2016/SP";
                waiverData1.SrwvReason = reason;
                waiverData1.SrwvComments = multiLineComment;
                waiverData1.SrwvWaiverPersonId = "0000987";
                waiverData1.StudentReqWaiversChgdate = new DateTime(2015, 02, 14);
                var chTime1 = DateTime.MinValue;
                chTime1.AddHours(14);
                chTime1.AddMinutes(02);
                chTime1.AddSeconds(15);
                waiverData1.StudentReqWaiversChgtime = chTime1;
                waiverData1.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();
                // Waive requisite 1
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "RW1",
                    SrwvWaiveReqmtFlagAssocMember = "n"
                });
                // Deny requisite 2
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "RQ2",
                    SrwvWaiveReqmtFlagAssocMember = ""
                });
                // No action on requisite 3
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "RW3",
                    SrwvWaiveReqmtFlagAssocMember = "Y"
                });

                return waiverData1;
            }
        }

        [TestClass]
        public class StudentWaiverRepository_GetStudentWaivers
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Collection<StudentReqWaivers> waiversResponseData;
            ApiSettings apiSettings;
            StudentWaiverRepository waiverRepo;
            string multiLineComment;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Collection of data accessor responses
                waiversResponseData = BuildWaiversResponse();

                waiverRepo = BuildValidWaiverRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                waiversResponseData = null;
                waiverRepo = null;
            }

            //Test to check if studentId is null
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentId_Is_Null()
            {
                StudentWaiverRepository waiverRepo = BuildInvalidWaiverRepository();
                var waivers = await waiverRepo.GetStudentWaiversAsync(null);
            }

            [TestMethod]
            public async Task EmptyRepoDataReturnsEmptyList()
            {
                // Set up repo response for waivers request
                Collection<StudentReqWaivers> emptyResponse = new Collection<StudentReqWaivers>();
                dataAccessorMock.Setup<Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(emptyResponse);
                var waivers = await waiverRepo.GetStudentWaiversAsync("0000123");
                Assert.AreEqual(0, waivers.Count());
            }

            [TestMethod]
            public async Task NullRepoDataReturnsEmptyList()
            {
                // Set up repo response for waivers request
                Collection<StudentReqWaivers> nullResponse = new Collection<StudentReqWaivers>();
                nullResponse = null;
                dataAccessorMock.Setup<Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(nullResponse);
                var waivers = await waiverRepo.GetStudentWaiversAsync("0000123");
                Assert.AreEqual(0, waivers.Count());
            }

            [TestMethod]
            public async Task ReturnsOnlyValidItems()
            {
                // Add a bad waiver to the normal response
                var badWaiverData = new StudentReqWaivers();
                badWaiverData.Recordkey = "1";
                badWaiverData.SrwvStudent = "0000123";
                badWaiverData.SrwvSection = "SEC1";
                badWaiverData.SrwvCourse = "CRS1";
                badWaiverData.SrwvTerm = "2016/FA";
                badWaiverData.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();
                var mixedResponseData = waiversResponseData;
                mixedResponseData.Add(badWaiverData);
                dataAccessorMock.Setup<Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(waiversResponseData);

                // Verify that the original (two) waiver response items are returned even though an additional item has invalid data
                var waivers = await waiverRepo.GetStudentWaiversAsync("0000123");
                Assert.AreEqual(3, waivers.Count());
            }

            [TestMethod]
            public async Task ReturnsOnlyValidRequisiteWaiverItems()
            {
                waiversResponseData.ElementAt(1).SrwvReqCoursesEntityAssociation.ElementAt(0).SrwvAcadReqmtsAssocMember = null;
                dataAccessorMock.Setup<Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(waiversResponseData);

                // Verify that the original (two) waiver response items are returned even though one of the requisite items has invalid data
                var waivers = await waiverRepo.GetStudentWaiversAsync("0000123");
                Assert.AreEqual(3, waivers.Count());
            }


            private StudentWaiverRepository BuildValidWaiverRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for waivers request
                dataAccessorMock.Setup<Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).ReturnsAsync(waiversResponseData);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                StudentWaiverRepository repository = new StudentWaiverRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentWaiverRepository BuildInvalidWaiverRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                Exception expectedFailure = new Exception("fail");

                dataAccessorMock.Setup<Task<Collection<StudentReqWaivers>>>(acc => acc.BulkReadRecordAsync<StudentReqWaivers>(It.IsAny<string>(), true)).Throws(expectedFailure);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                StudentWaiverRepository repository = new StudentWaiverRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return repository;
            }

            private Collection<StudentReqWaivers> BuildWaiversResponse()
            {
                Collection<StudentReqWaivers> repoWaivers = new Collection<StudentReqWaivers>();

                var waiverData = new StudentReqWaivers();

                // This student has no specific requisite waiver information
                waiverData.Recordkey = "1";
                waiverData.SrwvStudent = "0000123";
                waiverData.SrwvSection = "SEC1";
                waiverData.SrwvTerm = "2016/SP";
                waiverData.SrwvReason = "OTHER";
                multiLineComment = "Student 456 comment. Line1" + Convert.ToChar(DynamicArray.VM) + "comment line2" + Convert.ToChar(DynamicArray.VM) + "comment line3 the end";
                waiverData.SrwvComments = multiLineComment;
                waiverData.SrwvWaiverPersonId = "0000987";
                waiverData.StudentReqWaiversChgdate = new DateTime(2015, 01, 21);
                var chTime = DateTime.MinValue;
                chTime.AddHours(21);
                chTime.AddMinutes(01);
                chTime.AddSeconds(15);
                waiverData.StudentReqWaiversChgtime = chTime;
                waiverData.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();

                repoWaivers.Add(waiverData);

                // Three attached requisite waivers
                var waiverData1 = new StudentReqWaivers();
                waiverData1.Recordkey = "2";
                waiverData1.SrwvStudent = "0000123";
                waiverData1.SrwvSection = "SEC2";
                waiverData1.SrwvTerm = "2016/SP";
                waiverData1.SrwvReason = "LIFE";
                waiverData1.SrwvComments = "Student 456 comment line.";
                waiverData1.SrwvWaiverPersonId = "0000987";
                waiverData1.StudentReqWaiversChgdate = new DateTime(2015, 02, 14);
                var chTime1 = DateTime.MinValue;
                chTime1.AddHours(14);
                chTime1.AddMinutes(02);
                chTime1.AddSeconds(15);
                waiverData1.StudentReqWaiversChgtime = chTime1;
                waiverData1.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();
                // Waive requisite 1
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "REQ1",
                    SrwvWaiveReqmtFlagAssocMember = "y"
                });
                // Deny requisite 2
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "REQ2",
                    SrwvWaiveReqmtFlagAssocMember = "N"
                });
                // No action on requisite 3
                waiverData1.SrwvReqCoursesEntityAssociation.Add(new StudentReqWaiversSrwvReqCourses()
                {
                    SrwvAcadReqmtsAssocMember = "REQ3",
                    SrwvWaiveReqmtFlagAssocMember = ""
                });


                repoWaivers.Add(waiverData1);

                // This student has a revoked waiver 
                var waiverData2 = new StudentReqWaivers();
                waiverData2.Recordkey = "3";
                waiverData2.SrwvStudent = "0000123";
                waiverData2.SrwvSection = "SEC3";
                waiverData2.SrwvTerm = "2016/SP";
                waiverData2.SrwvReason = "OTHER";
                waiverData2.SrwvWaiverPersonId = "0000987";
                waiverData2.StudentReqWaiversChgdate = new DateTime(2015, 01, 21);
                chTime.AddHours(21);
                chTime.AddMinutes(01);
                chTime.AddSeconds(15);
                waiverData2.StudentReqWaiversChgtime = chTime;
                waiverData2.SrwvReqCoursesEntityAssociation = new List<StudentReqWaiversSrwvReqCourses>();
                waiverData2.SrwvRevokedFlag = "Y";
                repoWaivers.Add(waiverData2);

                return repoWaivers;
            }
        }
    }
}
