// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
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
    public class StudentRecordsReleaseRepositoryTests: BaseRepositorySetup
    {
        [TestClass]
        public class StudentRecordsReleaseRepository_GetStudentRecordsReleaseInfoAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;

            Collection<StudentReleases> studentRecordsReleaseData;
            StudentRecordsReleaseRepository studentRecordsReleaseRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Collection of data accessor responses
                studentRecordsReleaseData = BuildStudentRecordsReleaseData();
                studentRecordsReleaseRepository = BuildValidStudentRecordsReleaseRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentRecordsReleaseData = null;
                studentRecordsReleaseRepository = null;
            }

            [TestMethod]
            public async Task GetsAllStudentRecordsReleaseInformation()
            {
                var studentRecordsReleaseInformation = await studentRecordsReleaseRepository.GetStudentRecordsReleaseInfoAsync("0000015");
                Assert.AreEqual(2, studentRecordsReleaseInformation.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionIfAccessReturnsException()
            {
                StudentRecordsReleaseRepository studentRecordsReleaseRepository = BuildInvalidStudentRecordsReleaseRepository();
                var studentRecordsReleaseInformation = await studentRecordsReleaseRepository.GetStudentRecordsReleaseInfoAsync("0000015");
            }

            [TestMethod]
            public async Task EmptyRepositoryDataReturnsEmptyLists()
            {
                var studentRecordsReleaseInformation = await studentRecordsReleaseRepository.GetStudentRecordsReleaseInfoAsync("0000999");
                Assert.AreEqual(0, studentRecordsReleaseInformation.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfStudentStringNull()
            {
                var studentRecordsReleaseInformation = await studentRecordsReleaseRepository.GetStudentRecordsReleaseInfoAsync(null);
            }

            [TestMethod]
            public async Task NullRepositoryDataReturnsEmptyLists()
            {
                var nullResponse = new Collection<StudentReleases>();
                nullResponse = null;
                dataAccessorMock.Setup<Task<Collection<StudentReleases>>>(acc => acc.BulkReadRecordAsync<StudentReleases>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentReleases>>(nullResponse));
                var studentRecordsReleaseInformation = await studentRecordsReleaseRepository.GetStudentRecordsReleaseInfoAsync("0000015");
                Assert.AreEqual(0, studentRecordsReleaseInformation.Count());
            }

            private Collection<StudentReleases> BuildStudentRecordsReleaseData()
            {
                Collection<StudentReleases> repoStudentRecordsRelease = new Collection<StudentReleases>();

                var studentRecordsReleaseData1 = new StudentReleases();
                studentRecordsReleaseData1.Recordkey = "1";
                studentRecordsReleaseData1.SrelStudent = "0000015";
                studentRecordsReleaseData1.SrelFirstName = "Brown";
                studentRecordsReleaseData1.SrelLastName = "Smith";
                studentRecordsReleaseData1.SrelAccessGiven = new List<string>() { "GRD", "PHONE" };
                studentRecordsReleaseData1.SrelRelationship = "Brother";
                studentRecordsReleaseData1.SrelPin = "9999";
                studentRecordsReleaseData1.SrelStartDate = new DateTime(2022, 05, 12);
                studentRecordsReleaseData1.SrelEndDate = new DateTime(2022, 05, 18);
                repoStudentRecordsRelease.Add(studentRecordsReleaseData1);

                var studentRecordsReleaseData2 = new StudentReleases();
                studentRecordsReleaseData2.Recordkey = "2";
                studentRecordsReleaseData2.SrelStudent = "0000015";
                studentRecordsReleaseData2.SrelFirstName = "Charles";
                studentRecordsReleaseData2.SrelLastName = "William";
                studentRecordsReleaseData2.SrelAccessGiven = new List<string>() { "ADR", "PHONE" };
                studentRecordsReleaseData2.SrelRelationship = "Mother";
                studentRecordsReleaseData2.SrelPin = "9988";
                studentRecordsReleaseData2.SrelStartDate = new DateTime(2022, 05, 11);
                studentRecordsReleaseData2.SrelEndDate = new DateTime(2022, 05, 21);
                repoStudentRecordsRelease.Add(studentRecordsReleaseData2);
                return repoStudentRecordsRelease;
            }

            private StudentRecordsReleaseRepository BuildValidStudentRecordsReleaseRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for student records release information request
                dataAccessorMock.Setup<Task<Collection<StudentReleases>>>(acc => acc.BulkReadRecordAsync<StudentReleases>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentReleases>>(studentRecordsReleaseData));

                StudentRecordsReleaseRepository repository = new StudentRecordsReleaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentRecordsReleaseRepository BuildInvalidStudentRecordsReleaseRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                Exception expectedFailure = new Exception("fail");
                dataAccessorMock.Setup<Task<Collection<StudentReleases>>>(acc => acc.BulkReadRecordAsync<StudentReleases>(It.IsAny<string>(), true)).Throws(expectedFailure);

                StudentRecordsReleaseRepository repository = new StudentRecordsReleaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }
        }
        [TestClass]
        public class StudentRecordsReleaseRepository_AddStudentRecordsRelease
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;          
            IColleagueDataReader dataReader;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IStudentRecordsReleaseRepository studentRecordsReleaseRepository;
            AddStudentReleaseRecordRequest createRequest;
            

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                dataReader = dataAccessorMock.Object;
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                studentRecordsReleaseRepository = new StudentRecordsReleaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
                
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AddStudentRecordsRelease_WithAllNull()
            {
                var studentReleaseRecordEntity = AddStudentRecordsRelease_WithNullValues();
                AddStudentReleaseRecordResponse createResponse = new AddStudentReleaseRecordResponse();
                createResponse.OutErrorMessages = new List<string> { "Student id is required","First Name is required", "Last name is required","PIN is required","Relation Type is required","Access given is required"};
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddStudentReleaseRecordRequest, AddStudentReleaseRecordResponse>(It.IsAny<AddStudentReleaseRecordRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddStudentReleaseRecordRequest>(req => createRequest = req);
                var studentRecordsRelease = await studentRecordsReleaseRepository.AddStudentRecordsReleaseInfoAsync(studentReleaseRecordEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AddStudentRecordsRelease_ResponseHasErrorMessage()
            {
                var studentReleaseRecordEntity = AddStudentRecordsRelease_WithNullValues();
                AddStudentReleaseRecordResponse createResponse = new AddStudentReleaseRecordResponse();
                createResponse.OutErrorMessages = new List<string> { "Error text" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddStudentReleaseRecordRequest, AddStudentReleaseRecordResponse>(It.IsAny<AddStudentReleaseRecordRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddStudentReleaseRecordRequest>(req => createRequest = req);
                var studentRecordsRelease = await studentRecordsReleaseRepository.AddStudentRecordsReleaseInfoAsync(studentReleaseRecordEntity);
            }

            [TestMethod]
            public async Task AddStudentRecordsRelease_Success()
            {
                var studentReleaseRecordEntity = AddStudentRecordsRelease_WithSuccess();
                AddStudentReleaseRecordResponse createResponse = new AddStudentReleaseRecordResponse()
                {   
                    OutError = "0",
                    OutErrorMessages = new List<string>(),
                    OutStudentRecordsReleaseId = "300"                 
                };
                StudentReleases stuRelease = new StudentReleases()
                {
                    Recordkey = "300",
                    SrelStudent = "0000111",
                    SrelFirstName = "first",
                    SrelLastName = "last",
                    SrelAccessGiven = new List<string>() { "Phone" },
                    SrelPin = "1111",
                    SrelRelationship = "Brother"
                };
                studentReleaseRecordEntity.Id = createResponse.OutStudentRecordsReleaseId;
                dataAccessorMock.Setup(s => s.ReadRecordAsync<StudentReleases>(It.IsAny<string>(), true)).ReturnsAsync(stuRelease);
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddStudentReleaseRecordRequest, AddStudentReleaseRecordResponse>(It.IsAny<AddStudentReleaseRecordRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddStudentReleaseRecordRequest>(req => createRequest = req);
                var studentReleaseRecord = await studentRecordsReleaseRepository.AddStudentRecordsReleaseInfoAsync(studentReleaseRecordEntity);

                Assert.AreEqual(studentReleaseRecord.Id, "300");
                Assert.AreEqual(studentReleaseRecord.FirstName, "first");
                Assert.AreEqual(studentReleaseRecord.LastName, "last");
                Assert.AreEqual(studentReleaseRecord.PIN, "1111");
                Assert.AreEqual(studentReleaseRecord.RelationType, "Brother");
                Assert.AreEqual(studentReleaseRecord.AccessAreas.Count, 1);
            }

            private StudentRecordsReleaseInfo AddStudentRecordsRelease_WithSuccess()
            {
                var studentRecordsReleaseEntity = new StudentRecordsReleaseInfo()
                {
                    
                    StudentId = "0000111",
                    FirstName = "first",
                    LastName = "last",
                    RelationType = "Brother",
                    PIN = "1111",
                    AccessAreas = new List<string>() {"Phone" },
                    StartDate = null,
                    EndDate = null,

                };
                return studentRecordsReleaseEntity;
            }

            private StudentRecordsReleaseInfo AddStudentRecordsRelease_WithNullValues()
            {
                var studentRecordsReleaseEntity = new StudentRecordsReleaseInfo()
                {
                    Id = "",
                    StudentId = null,
                    FirstName = "",
                    LastName = "",
                    RelationType = "",
                    PIN = "",
                    AccessAreas = new List<string>() { },
                    StartDate = null,
                    EndDate = null,

                };
                return studentRecordsReleaseEntity;
            }

        }

        [TestClass]
        public class StudentRecordsReleaseRepository_UpdateStudentRecordsRelease
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            IColleagueDataReader dataReader;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IStudentRecordsReleaseRepository studentRecordsReleaseRepository;
            UpdateStudentReleaseRecordsRequest updateRequest;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                dataReader = dataAccessorMock.Object;
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                studentRecordsReleaseRepository = new StudentRecordsReleaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateStudentRecordsRelease_WithAllNull()
            {
                var studentReleaseRecordEntity = UpdateStudentRecordsRelease_WithNullValues();
                UpdateStudentReleaseRecordsResponse updateResponse = new UpdateStudentReleaseRecordsResponse();
                updateResponse.OutErrorMessages = new List<string> { "Student id is required", "Student Release Id is required",  "PIN is required",  "Access given is required" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentReleaseRecordsRequest, UpdateStudentReleaseRecordsResponse>(It.IsAny<UpdateStudentReleaseRecordsRequest>())).Returns(Task.FromResult(updateResponse)).Callback<UpdateStudentReleaseRecordsRequest>(req => updateRequest = req);
                var studentRecordsRelease = await studentRecordsReleaseRepository.UpdateStudentRecordsReleaseInfoAsync(studentReleaseRecordEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateStudentRecordsRelease_ResponseHasErrorMessage()
            {
                var studentReleaseRecordEntity = UpdateStudentRecordsRelease_WithNullValues();
                UpdateStudentReleaseRecordsResponse updateResponse = new UpdateStudentReleaseRecordsResponse();
                updateResponse.OutErrorMessages = new List<string> { "Error text" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentReleaseRecordsRequest, UpdateStudentReleaseRecordsResponse>(It.IsAny<UpdateStudentReleaseRecordsRequest>())).Returns(Task.FromResult(updateResponse)).Callback<UpdateStudentReleaseRecordsRequest>(req => updateRequest = req);
                var studentRecordsRelease = await studentRecordsReleaseRepository.UpdateStudentRecordsReleaseInfoAsync(studentReleaseRecordEntity);
            }

            [TestMethod]
            public async Task UpdateStudentRecordsRelease_Success()
            {
                var studentReleaseRecordEntity = UpdateStudentRecordsRelease_WithSuccess();
                UpdateStudentReleaseRecordsResponse updateResponse = new UpdateStudentReleaseRecordsResponse()
                {
                    OutError = "0",
                    OutErrorMessages = new List<string>(),
                    OutStuRelId = "300"
                };

                StudentReleases stuRelease = new StudentReleases()
                {
                    Recordkey = "300",
                    SrelStudent = "0000111",
                    SrelFirstName = "first",
                    SrelLastName = "last",
                    SrelAccessGiven = new List<string>() { "Phone" },
                    SrelPin = "1111",
                    SrelRelationship = "Brother"
                };
                studentReleaseRecordEntity.Id = "300";
                dataAccessorMock.Setup(s => s.ReadRecordAsync<StudentReleases>(It.IsAny<string>(), true)).ReturnsAsync(stuRelease);
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateStudentReleaseRecordsRequest, UpdateStudentReleaseRecordsResponse>(It.IsAny<UpdateStudentReleaseRecordsRequest>())).Returns(Task.FromResult(updateResponse)).Callback<UpdateStudentReleaseRecordsRequest>(req => updateRequest = req);
                var studentReleaseRecord = await studentRecordsReleaseRepository.UpdateStudentRecordsReleaseInfoAsync(studentReleaseRecordEntity);
                Assert.AreEqual(studentReleaseRecord.Id, "300");
                Assert.AreEqual(studentReleaseRecord.FirstName, "first");
                Assert.AreEqual(studentReleaseRecord.LastName, "last");
                Assert.AreEqual(studentReleaseRecord.PIN, "1111");
                Assert.AreEqual(studentReleaseRecord.RelationType, "Brother");
                Assert.AreEqual(studentReleaseRecord.AccessAreas.Count, 1);            
            }

            private StudentRecordsReleaseInfo UpdateStudentRecordsRelease_WithSuccess()
            {
                var studentRecordsReleaseEntity = new StudentRecordsReleaseInfo()
                {
                    Id = "300",
                    StudentId = "0000111",
                    FirstName = "first",
                    LastName = "last",
                    RelationType = "Brother",
                    PIN = "1111",
                    AccessAreas = new List<string>() { "Phone" },
                    StartDate = null,
                    EndDate = null,

                };
                return studentRecordsReleaseEntity;
            }

            private StudentRecordsReleaseInfo UpdateStudentRecordsRelease_WithNullValues()
            {
                var studentRecordsReleaseEntity = new StudentRecordsReleaseInfo()
                {
                    Id = "",
                    StudentId = null,
                    FirstName = "",
                    LastName = "",
                    RelationType = "",
                    PIN = "",
                    AccessAreas = new List<string>() { },
                    StartDate = null,
                    EndDate = null,

                };
                return studentRecordsReleaseEntity;
            }

        }

        [TestClass]
        public class StudentRecordsReleaseRepository_EndStudentRecordsRelease
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IStudentRecordsReleaseRepository studentRecordsReleaseRepository;
            DeleteStudentReleaseRecordRequest deleteRequest;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                studentRecordsReleaseRepository = new StudentRecordsReleaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            public async Task EndStudentRecordsRelease_Success()
            {
                StudentReleases stuRelease = new StudentReleases()
                {
                    Recordkey = "300",
                    SrelStudent = "0000111",
                    SrelFirstName = "first",
                    SrelLastName = "last",
                    SrelAccessGiven = new List<string>() { "Phone" },
                    SrelPin = "1111",
                    SrelRelationship = "Brother",
                    SrelStartDate = DateTime.Now,
                    SrelEndDate = DateTime.Now
                };
                DeleteStudentReleaseRecordResponse deleteResponse = new DeleteStudentReleaseRecordResponse()
                {
                    OutError = "0",
                    OutErrorMessages = new List<string>()
                };
                dataAccessorMock.Setup(s => s.ReadRecordAsync<StudentReleases>(It.IsAny<string>(), true)).ReturnsAsync(stuRelease);
                mockManager.Setup(mgr => mgr.ExecuteAsync<DeleteStudentReleaseRecordRequest, DeleteStudentReleaseRecordResponse>(It.IsAny<DeleteStudentReleaseRecordRequest>())).Returns(Task.FromResult(deleteResponse)).Callback<DeleteStudentReleaseRecordRequest>(req => deleteRequest = req);
                var studentReleaseRecord = await studentRecordsReleaseRepository.DeleteStudentRecordsReleaseInfoAsync("0000111");

                Assert.AreEqual(studentReleaseRecord.Id, "300");
                Assert.AreEqual(studentReleaseRecord.FirstName, "first");
                Assert.AreEqual(studentReleaseRecord.LastName, "last");
                Assert.AreEqual(studentReleaseRecord.PIN, "1111");
                Assert.AreEqual(studentReleaseRecord.RelationType, "Brother");
                Assert.AreEqual(studentReleaseRecord.AccessAreas.Count, 1);
                Assert.AreEqual(studentReleaseRecord.StartDate.Value.Date, DateTime.Now.Date);
                Assert.AreEqual(studentReleaseRecord.EndDate.Value.Date, DateTime.Now.Date);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EndStudentRecordsRelease_NullParameter()
            {
                DeleteStudentReleaseRecordResponse deleteResponse = new DeleteStudentReleaseRecordResponse()
                {
                    OutError = "0",
                    OutErrorMessages = new List<string>()
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<DeleteStudentReleaseRecordRequest, DeleteStudentReleaseRecordResponse>(It.IsAny<DeleteStudentReleaseRecordRequest>())).Returns(Task.FromResult(deleteResponse)).Callback<DeleteStudentReleaseRecordRequest>(req => deleteRequest = req);
                var studentReleaseRecord = await studentRecordsReleaseRepository.DeleteStudentRecordsReleaseInfoAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task EndStudentRecordsRelease_Response_With_OutError()
            {
                DeleteStudentReleaseRecordResponse deleteResponse = new DeleteStudentReleaseRecordResponse()
                {
                    OutError = "0",
                    OutErrorMessages = new List<string> { "Error text" }
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<DeleteStudentReleaseRecordRequest, DeleteStudentReleaseRecordResponse>(It.IsAny<DeleteStudentReleaseRecordRequest>())).Returns(Task.FromResult(deleteResponse)).Callback<DeleteStudentReleaseRecordRequest>(req => deleteRequest = req);
                var studentReleaseRecord = await studentRecordsReleaseRepository.DeleteStudentRecordsReleaseInfoAsync("0000111");
            }
        }

        [TestClass]
        public class StudentRecordsReleaseRepository_GetStudentRecordsReleaseDenyAccessAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            StuRelDenyAccess studentRecordsReleasedenyAccessData;
            StudentRecordsReleaseRepository studentRecordsReleaseRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");               
                studentRecordsReleaseRepository = BuildValidStudentRecordsReleaseRepository();               
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentRecordsReleasedenyAccessData = null;
                studentRecordsReleaseRepository = null;
            }

            [TestMethod]
            public async Task GetStudentRecordsReleaseDenyAccessAsync_when_StuRelDenyAccessAll_is_Yes()
            {
                
                var studentRecordsReleaseDenyAccess = await studentRecordsReleaseRepository.GetStudentRecordsReleaseDenyAccessAsync("0000015");
                Assert.IsTrue(studentRecordsReleaseDenyAccess.DenyAccessToAll);
            }

            [TestMethod]
            public async Task GetStudentRecordsReleaseDenyAccessAsync_when_StuRelDenyAccessAll_is_No()
            {
                studentRecordsReleasedenyAccessData.StuRelDenyAccessAll = "N";
                var studentRecordsReleaseDenyAccess = await studentRecordsReleaseRepository.GetStudentRecordsReleaseDenyAccessAsync("0000015");
                Assert.IsFalse(studentRecordsReleaseDenyAccess.DenyAccessToAll);
            }
            [TestMethod]
            public async Task GetStudentRecordsReleaseDenyAccessAsync_when_StuRelDenyAccessAll_is_Empty()
            {
                studentRecordsReleasedenyAccessData.StuRelDenyAccessAll = "";
                var studentRecordsReleaseDenyAccess = await studentRecordsReleaseRepository.GetStudentRecordsReleaseDenyAccessAsync("0000015");
                Assert.IsFalse(studentRecordsReleaseDenyAccess.DenyAccessToAll);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionIfAccessReturnsException()
            {
                StudentRecordsReleaseRepository studentRecordsReleaseRepository = BuildInvalidStudentRecordsReleaseRepository();
                var studentRecordsReleaseDenyAccess = await studentRecordsReleaseRepository.GetStudentRecordsReleaseDenyAccessAsync("0000015");
            }

            [TestMethod]
            public async Task EmptyRepositoryDataReturnsFalse()
            {
                var studentRecordsReleaseDenyAccess = await studentRecordsReleaseRepository.GetStudentRecordsReleaseDenyAccessAsync("0000999");
                Assert.IsFalse(studentRecordsReleaseDenyAccess.DenyAccessToAll);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfStudentStringNull()
            {
                var studentRecordsReleaseDenyAccess = await studentRecordsReleaseRepository.GetStudentRecordsReleaseDenyAccessAsync(null);
            }

            [TestMethod]
            public async Task NullRepositoryDataReturnsFalse()
            {
                var nullResponse = new StuRelDenyAccess();
                nullResponse = null;
                dataAccessorMock.Setup<Task<StuRelDenyAccess>>(acc => acc.ReadRecordAsync<StuRelDenyAccess>("STU.REL.DENY.ACCESS", "0000015", It.IsAny<bool>())).Returns(Task.FromResult<StuRelDenyAccess>(nullResponse));
                var studentRecordsReleaseDenyAccess = await studentRecordsReleaseRepository.GetStudentRecordsReleaseDenyAccessAsync("0000015");
                Assert.IsFalse(studentRecordsReleaseDenyAccess.DenyAccessToAll);
            }

     

            private StudentRecordsReleaseRepository BuildValidStudentRecordsReleaseRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                studentRecordsReleasedenyAccessData = new StuRelDenyAccess()
                {
                    StuRelDenyAccessAll = "Y"
                };
                // Set up repo response for student records release deny access information request
                dataAccessorMock.Setup<Task<StuRelDenyAccess>>(acc => acc.ReadRecordAsync<StuRelDenyAccess>("STU.REL.DENY.ACCESS", "0000015", It.IsAny<bool>())).Returns(Task.FromResult<StuRelDenyAccess>(studentRecordsReleasedenyAccessData));

                StudentRecordsReleaseRepository repository = new StudentRecordsReleaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentRecordsReleaseRepository BuildInvalidStudentRecordsReleaseRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                ApplicationException expectedFailure = new ApplicationException("fail");
                dataAccessorMock.Setup<Task<StuRelDenyAccess>>(acc => acc.ReadRecordAsync<StuRelDenyAccess>("STU.REL.DENY.ACCESS", "0000015", It.IsAny<bool>())).Throws(expectedFailure);

                StudentRecordsReleaseRepository repository = new StudentRecordsReleaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }
        }

        [TestClass]
        public class StudentRecordsReleaseRepository_DenyStudentRecordsReleaseAccessAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            IColleagueDataReader dataReader;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IStudentRecordsReleaseRepository studentRecordsReleaseRepository;
            StudentRecordsReleaseDenyAccessAllRequest denyAccessAllRequest;


           [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                dataReader = dataAccessorMock.Object;
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                studentRecordsReleaseRepository = new StudentRecordsReleaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task DenyStudentRecordsReleaseAccessAsync_With_No_StudentId()
            {
                var denyStudentRecordsReleaseAccessInfoEntity = DenyStudentRecordsReleaseAccessAsync_Without_StudentId();
                StudentRecordsReleaseDenyAccessAllResponse denyAccessAllResponse = new StudentRecordsReleaseDenyAccessAllResponse();
                denyAccessAllResponse.OutErrorMessages = new List<string> { "Student id is required" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<StudentRecordsReleaseDenyAccessAllRequest, StudentRecordsReleaseDenyAccessAllResponse>(It.IsAny<StudentRecordsReleaseDenyAccessAllRequest>())).Returns(Task.FromResult(denyAccessAllResponse)).Callback<StudentRecordsReleaseDenyAccessAllRequest>(req => denyAccessAllRequest = req);
                var studentRecordsRelease = await studentRecordsReleaseRepository.DenyStudentRecordsReleaseAccessAsync(denyStudentRecordsReleaseAccessInfoEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task DenyStudentRecordsReleaseAccessAsync_ResponseHasErrorMessage()
            {
                var denyStudentRecordsReleaseAccessInfoEntity = DenyStudentRecordsReleaseAccessAsync_Without_StudentId();
                StudentRecordsReleaseDenyAccessAllResponse denyAccessAllResponse = new StudentRecordsReleaseDenyAccessAllResponse();
                denyAccessAllResponse.OutErrorMessages = new List<string> { "Error text" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<StudentRecordsReleaseDenyAccessAllRequest, StudentRecordsReleaseDenyAccessAllResponse>(It.IsAny<StudentRecordsReleaseDenyAccessAllRequest>())).Returns(Task.FromResult(denyAccessAllResponse)).Callback<StudentRecordsReleaseDenyAccessAllRequest>(req => denyAccessAllRequest = req);
                var studentRecordsRelease = await studentRecordsReleaseRepository.DenyStudentRecordsReleaseAccessAsync(denyStudentRecordsReleaseAccessInfoEntity);
            }

            [TestMethod]
            public async Task DenyStudentRecordsReleaseAccessAsync_Success()
            {
                var denyStudentRecordsReleaseAccessInfoEntity = DenyStudentRecordsReleaseAccessAsync_WithSuccess();
                StudentRecordsReleaseDenyAccessAllResponse denyAccessAllResponse = new StudentRecordsReleaseDenyAccessAllResponse()
                {
                    OutError = "0",
                    OutErrorMessages = new List<string>()
                };
                Collection<StudentReleases> stuRelease = new Collection<StudentReleases>();
                var studentRecordsReleaseData1 = new StudentReleases()
                {
                    Recordkey = "300",
                    SrelStudent = "0000111",
                    SrelFirstName = "first",
                    SrelLastName = "last",
                    SrelAccessGiven = new List<string>() { "Phone" },
                    SrelPin = "1111",
                    SrelRelationship = "Brother",
                    SrelStartDate = new DateTime(2022, 05, 10),
                    SrelEndDate = DateTime.Today
                };
                stuRelease.Add(studentRecordsReleaseData1);

                dataAccessorMock.Setup(s => s.BulkReadRecordAsync<StudentReleases>(It.IsAny<string>(), true)).ReturnsAsync(stuRelease);
                mockManager.Setup(mgr => mgr.ExecuteAsync<StudentRecordsReleaseDenyAccessAllRequest, StudentRecordsReleaseDenyAccessAllResponse>(It.IsAny<StudentRecordsReleaseDenyAccessAllRequest>())).Returns(Task.FromResult(denyAccessAllResponse)).Callback<StudentRecordsReleaseDenyAccessAllRequest>(req => denyAccessAllRequest = req);
                var studentReleaseRecord = await studentRecordsReleaseRepository.DenyStudentRecordsReleaseAccessAsync(denyStudentRecordsReleaseAccessInfoEntity);

                Assert.AreEqual(studentReleaseRecord.ElementAt(0).Id, stuRelease.ElementAt(0).Recordkey);
                Assert.AreEqual(studentReleaseRecord.ElementAt(0).StudentId, stuRelease.ElementAt(0).SrelStudent);
                Assert.AreEqual(studentReleaseRecord.ElementAt(0).FirstName, stuRelease.ElementAt(0).SrelFirstName);
                Assert.AreEqual(studentReleaseRecord.ElementAt(0).LastName, stuRelease.ElementAt(0).SrelLastName);
                CollectionAssert.AreEqual(studentReleaseRecord.ElementAt(0).AccessAreas, stuRelease.ElementAt(0).SrelAccessGiven);
                Assert.AreEqual(studentReleaseRecord.ElementAt(0).PIN, stuRelease.ElementAt(0).SrelPin);
                Assert.AreEqual(studentReleaseRecord.ElementAt(0).RelationType, stuRelease.ElementAt(0).SrelRelationship);
                Assert.AreEqual(studentReleaseRecord.ElementAt(0).StartDate, stuRelease.ElementAt(0).SrelStartDate);
                Assert.AreEqual(studentReleaseRecord.ElementAt(0).EndDate, stuRelease.ElementAt(0).SrelEndDate);
            }

            private DenyStudentRecordsReleaseAccessInformation DenyStudentRecordsReleaseAccessAsync_WithSuccess()
            {
                var denyStudentRecordsReleaseAccessInfoEntity = new DenyStudentRecordsReleaseAccessInformation()
                {
                    DenyAccessToAll = true,
                    StudentId = "0000111"
                };
                return denyStudentRecordsReleaseAccessInfoEntity;
            }

            private DenyStudentRecordsReleaseAccessInformation DenyStudentRecordsReleaseAccessAsync_Without_StudentId()
            {
                var denyStudentRecordsReleaseAccessInfoEntity = new DenyStudentRecordsReleaseAccessInformation()
                {
                    DenyAccessToAll = false,
                    StudentId = null
                };
                return denyStudentRecordsReleaseAccessInfoEntity;
            }


        }
    }
}
