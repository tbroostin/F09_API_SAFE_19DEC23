// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentRequestRepositoryTests
    {
       


        [TestClass]
        public class StudentTranscriptRequest_Create
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> mockManager;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            CreateStudentRequestRequest createdTranscationRequest;
            StudentRequestLogs requestResponseData;
            StudentRequestRepository requestRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                requestResponseData = BuildValidRequestLogsResponse();
                requestRepository=BuildValidRequestRepository();
                

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
            }

           
            private StudentRequestRepository BuildValidRequestRepository()
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
                CreateStudentRequestResponse createResponse = new CreateStudentRequestResponse();
                createResponse.ErrorOccurred = false;
                createResponse.ErrorMessage = null;
                createResponse.StudentRequestLogsId = "12";
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).ReturnsAsync(createResponse).Callback<CreateStudentRequestRequest>(req => createdTranscationRequest = req);

                // Set up repo response for request Get request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(requestResponseData);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                StudentRequestRepository repository = new StudentRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentRequestLogs BuildValidRequestLogsResponse()
            {
                StudentRequestLogs requestData= new StudentRequestLogs();
                requestData.Recordkey = "12";
                requestData.StrlRecipientName = "my name";
                requestData.StrlStudent = "11111";
                requestData.StrlTranscriptGroupings = new List<string>() { "UG"};
                requestData.StrlStuRequestLogHolds = "OTHER";
                requestData.StrlType = "T";
                return requestData;

            }

            [TestMethod]
            public async Task Valid_Transcript_Request_Data()
            {
                //create StudentTranscriptRequest domain object
                //compare createRequest have transcriptGrouping value same as provided through domain entity
                StudentTranscriptRequest newRequest = new StudentTranscriptRequest("11111", "my name","UG");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest= await requestRepository.CreateStudentRequestAsync(newRequest);

                //intermediate transaction request created
                Assert.AreEqual(createdTranscationRequest.RequestType, "T");
                Assert.AreEqual(createdTranscationRequest.TranscriptGrouping,"UG");
                Assert.AreEqual(createdTranscationRequest.RequestHoldCode, "OTHER");
                Assert.AreEqual(createdTranscationRequest.StudentId, "11111");
                Assert.AreEqual(createdTranscationRequest.RecipientName, "my name");

                //final domain value created which is returned
                Assert.AreEqual(createdDomainRequest.GetType(), typeof(StudentTranscriptRequest));
                Assert.AreEqual((createdDomainRequest as StudentTranscriptRequest).TranscriptGrouping, "UG");
                Assert.AreEqual(createdDomainRequest.HoldRequest, "OTHER");
                Assert.AreEqual(createdDomainRequest.Id, "12");
                Assert.AreEqual(createdDomainRequest.StudentId, "11111");
                Assert.AreEqual(createdDomainRequest.RecipientName, "my name");
          
            }

            [TestMethod]
            public async Task NumerOfCopies_Is_Null_Transcript_Request_Data()
            {
                //create StudentTranscriptRequest domain object
                //compare createRequest have transcriptGrouping value same as provided through domain entity
                StudentTranscriptRequest newRequest = new StudentTranscriptRequest("11111", "my name", "UG");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                newRequest.NumberOfCopies = null;
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);

                //intermediate transaction request created
                Assert.AreEqual(createdTranscationRequest.RequestType, "T");
                Assert.AreEqual(createdTranscationRequest.TranscriptGrouping, "UG");
                Assert.AreEqual(createdTranscationRequest.RequestHoldCode, "OTHER");
                Assert.AreEqual(createdTranscationRequest.StudentId, "11111");
                Assert.AreEqual(createdTranscationRequest.RecipientName, "my name");
                Assert.AreEqual(createdTranscationRequest.NumberOfCopies, 1);

                //final domain value created which is returned
                Assert.AreEqual(createdDomainRequest.GetType(), typeof(StudentTranscriptRequest));
                Assert.AreEqual((createdDomainRequest as StudentTranscriptRequest).TranscriptGrouping, "UG");
                Assert.AreEqual(createdDomainRequest.HoldRequest, "OTHER");
                Assert.AreEqual(createdDomainRequest.Id, "12");
                Assert.AreEqual(createdDomainRequest.StudentId, "11111");
                Assert.AreEqual(createdDomainRequest.RecipientName, "my name");

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Student_Transcript_Request_is_Null()
            {
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(null);
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Create_response_Transaction_throws_Exception()
            {
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).Throws(new Exception());
                StudentTranscriptRequest newRequest = new StudentTranscriptRequest("11111", "my name","UG");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);

            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task created_response_is_null()
            {
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).ReturnsAsync(null);
                StudentTranscriptRequest newRequest = new StudentTranscriptRequest("11111", "my name","UG");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);
            }

            //create response is not null but error  occured
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task created_response_is_errored()
            {
                CreateStudentRequestResponse createResponse = new CreateStudentRequestResponse();
                createResponse.ErrorOccurred = true;
                createResponse.ErrorMessage = "some error";
                createResponse.StudentRequestLogsId = "12";
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).ReturnsAsync(createResponse).Callback<CreateStudentRequestRequest>(req => createdTranscationRequest = req);

                StudentTranscriptRequest newRequest = new StudentTranscriptRequest("11111", "my name","UG");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);
            }

            //create response student logs id is null
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task created_response_request_log_id_is_null()
            {
                CreateStudentRequestResponse createResponse = new CreateStudentRequestResponse();
                createResponse.ErrorOccurred = false;
                createResponse.ErrorMessage = "";
                createResponse.StudentRequestLogsId = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).ReturnsAsync(createResponse).Callback<CreateStudentRequestRequest>(req => createdTranscationRequest = req);

                StudentTranscriptRequest newRequest = new StudentTranscriptRequest("11111", "my name","UG");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);
            }

            //comments tests
        }
        [TestClass]
        public class StudentEnrollmentRequest_Create
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> mockManager;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            CreateStudentRequestRequest createdTranscationRequest;
            StudentRequestLogs requestResponseData=new StudentRequestLogs();
            StudentRequestRepository requestRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                requestResponseData = BuildValidRequestLogsResponse();
                requestRepository = BuildValidRequestRepository();


            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
            }


            private StudentRequestRepository BuildValidRequestRepository()
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
                CreateStudentRequestResponse createResponse = new CreateStudentRequestResponse();
                createResponse.ErrorOccurred = false;
                createResponse.ErrorMessage = null;
                createResponse.StudentRequestLogsId = "12";
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).ReturnsAsync(createResponse).Callback<CreateStudentRequestRequest>(req => createdTranscationRequest = req);

                // Set up repo response for request Get request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(requestResponseData);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                StudentRequestRepository repository = new StudentRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentRequestLogs BuildValidRequestLogsResponse()
            {
                StudentRequestLogs requestData = new StudentRequestLogs();
                requestData.Recordkey = "12";
                requestData.StrlRecipientName = "my name";
                requestData.StrlStudent = "11111";
                requestData.StrlStuRequestLogHolds = "OTHER";
                requestData.StrlType = "E";
                return requestData;

            }

            [TestMethod]
            public async Task Valid_Enrollment_Request_Data()
            {
                //create StudentEnrollmentRequest domain object
                StudentEnrollmentRequest newRequest = new StudentEnrollmentRequest("11111", "my name");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);

                //intermediate transaction request created
                Assert.AreEqual(createdTranscationRequest.RequestType, "E");
                Assert.AreEqual(createdTranscationRequest.TranscriptGrouping, string.Empty);
                Assert.AreEqual(createdTranscationRequest.RequestHoldCode, "OTHER");
                Assert.AreEqual(createdTranscationRequest.StudentId, "11111");
                Assert.AreEqual(createdTranscationRequest.RecipientName, "my name");

                //final domain value created which is returned
                Assert.AreEqual(createdDomainRequest.GetType(), typeof(StudentEnrollmentRequest));
                Assert.AreEqual(createdDomainRequest.HoldRequest, "OTHER");
                Assert.AreEqual(createdDomainRequest.Id, "12");
                Assert.AreEqual(createdDomainRequest.StudentId, "11111");
                Assert.AreEqual(createdDomainRequest.RecipientName, "my name");

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Student_enrollment_Request_is_Null()
            {
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(null);
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Create_response_Transaction_throws_Exception()
            {
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).Throws(new Exception());
                StudentEnrollmentRequest newRequest = new StudentEnrollmentRequest("11111", "my name");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);

            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task created_response_is_null()
            {
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).ReturnsAsync(null);
                StudentEnrollmentRequest newRequest = new StudentEnrollmentRequest("11111", "my name");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);
            }

            //create response is not null but error  occured
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task created_response_is_errored()
            {
                CreateStudentRequestResponse createResponse = new CreateStudentRequestResponse();
                createResponse.ErrorOccurred = true;
                createResponse.ErrorMessage = "some error";
                createResponse.StudentRequestLogsId = "12";
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).ReturnsAsync(createResponse).Callback<CreateStudentRequestRequest>(req => createdTranscationRequest = req);

                StudentEnrollmentRequest newRequest = new StudentEnrollmentRequest("11111", "my name");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);
            }

            //create response student logs id is null
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task created_response_request_log_id_is_null()
            {
                CreateStudentRequestResponse createResponse = new CreateStudentRequestResponse();
                createResponse.ErrorOccurred = false;
                createResponse.ErrorMessage = "";
                createResponse.StudentRequestLogsId = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(It.Is<CreateStudentRequestRequest>(r => !string.IsNullOrEmpty(r.StudentId)))).ReturnsAsync(createResponse).Callback<CreateStudentRequestRequest>(req => createdTranscationRequest = req);

                StudentEnrollmentRequest newRequest = new StudentEnrollmentRequest("11111", "my name");
                newRequest.HoldRequest = "OTHER";
                newRequest.Id = "";
                StudentRequest createdDomainRequest = await requestRepository.CreateStudentRequestAsync(newRequest);
            }

        }
        [TestClass]
        public class StudentTranscriptRequest_Get
        {

            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            StudentRequestLogs requestResponseData;
            StudentRequestRepository requestRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                requestResponseData = BuildValidRequestLogsResponse();
                requestRepository = BuildValidRequestRepository();


            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
            }


            private StudentRequestRepository BuildValidRequestRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                localCacheMock = new Mock<ObjectCache>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
               
                // Set up repo response for request Get request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(requestResponseData);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                StudentRequestRepository repository = new StudentRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentRequestLogs BuildValidRequestLogsResponse()
            {
                StudentRequestLogs requestData = new StudentRequestLogs();
                requestData.Recordkey = "12";
                requestData.StrlRecipientName = "my name";
                requestData.StrlStudent = "11111";
                requestData.StrlTranscriptGroupings = new List<string>() { "UG" };
                requestData.StrlStuRequestLogHolds = "OTHER";
                requestData.StrlType = "T";
                return requestData;

            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Request_id_Is_Null()
            {
                var request= await requestRepository.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RequestLog_returned_is_null()
            {
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(null);
                var request = await requestRepository.GetAsync("12");
            }
            [TestMethod]
            public async Task Proper_domain_request_returned()
            {
                StudentRequest retrievedDomainRequest = await requestRepository.GetAsync("12");
                //final domain value created which is returned
                Assert.AreEqual(retrievedDomainRequest.GetType(), typeof(StudentTranscriptRequest));
                Assert.AreEqual((retrievedDomainRequest as StudentTranscriptRequest).TranscriptGrouping, "UG");
                Assert.AreEqual(retrievedDomainRequest.HoldRequest, "OTHER");
                Assert.AreEqual(retrievedDomainRequest.Id, "12");
                Assert.AreEqual(retrievedDomainRequest.StudentId, "11111");
                Assert.AreEqual(retrievedDomainRequest.RecipientName, "my name");
            }
           //transcript grouping tests
            //if transcript groupings collection is null
            [TestMethod]
            public async Task Transcript_grouping_is_null()
            {
                StudentRequestLogs requestData = new StudentRequestLogs();
                requestData.Recordkey = "12";
                requestData.StrlRecipientName = "my name";
                requestData.StrlStudent = "11111";
                requestData.StrlTranscriptGroupings = null;
                requestData.StrlStuRequestLogHolds = "OTHER";
                requestData.StrlType = "T";
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(requestData);
                StudentRequest retrievedDomainRequest = await requestRepository.GetAsync("12");
                //final domain value created which is returned
                Assert.AreEqual(retrievedDomainRequest.GetType(), typeof(StudentTranscriptRequest));
                Assert.AreEqual((retrievedDomainRequest as StudentTranscriptRequest).TranscriptGrouping, string.Empty);
                Assert.AreEqual(retrievedDomainRequest.HoldRequest, "OTHER");
                Assert.AreEqual(retrievedDomainRequest.Id, "12");
                Assert.AreEqual(retrievedDomainRequest.StudentId, "11111");
                Assert.AreEqual(retrievedDomainRequest.RecipientName, "my name");

            }
            //if transcript groupings collection have empty elements
            [TestMethod]
            public async Task Transcript_grouping_have_empty_elements()
            {
                StudentRequestLogs requestData = new StudentRequestLogs();
                requestData.Recordkey = "12";
                requestData.StrlRecipientName = "my name";
                requestData.StrlStudent = "11111";
                requestData.StrlTranscriptGroupings = new List<string>();
                requestData.StrlStuRequestLogHolds = "OTHER";
                requestData.StrlType = "T";
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(requestData);
                StudentRequest retrievedDomainRequest = await requestRepository.GetAsync("12");
                //final domain value created which is returned
                Assert.AreEqual(retrievedDomainRequest.GetType(), typeof(StudentTranscriptRequest));
                Assert.AreEqual((retrievedDomainRequest as StudentTranscriptRequest).TranscriptGrouping, string.Empty);
                Assert.AreEqual(retrievedDomainRequest.HoldRequest, "OTHER");
                Assert.AreEqual(retrievedDomainRequest.Id, "12");
                Assert.AreEqual(retrievedDomainRequest.StudentId, "11111");
                Assert.AreEqual(retrievedDomainRequest.RecipientName, "my name");

            }
            //if transcript groupings collection have only one element
            [TestMethod]
            public async Task Transcript_grouping_have_one_element()
            {
                StudentRequestLogs requestData = new StudentRequestLogs();
                requestData.Recordkey = "12";
                requestData.StrlRecipientName = "my name";
                requestData.StrlStudent = "11111";
                requestData.StrlTranscriptGroupings = new List<string>() {"UG"};
                requestData.StrlStuRequestLogHolds = "OTHER";
                requestData.StrlType = "T";
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(requestData);
                StudentRequest retrievedDomainRequest = await requestRepository.GetAsync("12");
                //final domain value created which is returned
                Assert.AreEqual(retrievedDomainRequest.GetType(), typeof(StudentTranscriptRequest));
                Assert.AreEqual((retrievedDomainRequest as StudentTranscriptRequest).TranscriptGrouping, "UG");
                Assert.AreEqual(retrievedDomainRequest.HoldRequest, "OTHER");
                Assert.AreEqual(retrievedDomainRequest.Id, "12");
                Assert.AreEqual(retrievedDomainRequest.StudentId, "11111");
                Assert.AreEqual(retrievedDomainRequest.RecipientName, "my name");

            }
            //if transcript groupings collection have multiple elements and only first one is picked
            [TestMethod]
            public async Task Transcript_grouping_first_element_picked()
            {
                StudentRequestLogs requestData = new StudentRequestLogs();
                requestData.Recordkey = "12";
                requestData.StrlRecipientName = "my name";
                requestData.StrlStudent = "11111";
                requestData.StrlTranscriptGroupings = new List<string>() { "UG" ,"RG","AG"};
                requestData.StrlStuRequestLogHolds = "OTHER";
                requestData.StrlType = "T";
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(requestData);
                StudentRequest retrievedDomainRequest = await requestRepository.GetAsync("12");
                //final domain value created which is returned
                Assert.AreEqual(retrievedDomainRequest.GetType(), typeof(StudentTranscriptRequest));
                Assert.AreEqual((retrievedDomainRequest as StudentTranscriptRequest).TranscriptGrouping, "UG");
                Assert.AreEqual(retrievedDomainRequest.HoldRequest, "OTHER");
                Assert.AreEqual(retrievedDomainRequest.Id, "12");
                Assert.AreEqual(retrievedDomainRequest.StudentId, "11111");
                Assert.AreEqual(retrievedDomainRequest.RecipientName, "my name");

            }
        }
        [TestClass]
        public class StudentEnrollmentRequest_Get
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            StudentRequestLogs requestResponseData;
            StudentRequestRepository requestRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                requestResponseData = BuildValidRequestLogsResponse();
                requestRepository = BuildValidRequestRepository();


            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
            }


            private StudentRequestRepository BuildValidRequestRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                localCacheMock = new Mock<ObjectCache>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for request Get request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(requestResponseData);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                StudentRequestRepository repository = new StudentRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentRequestLogs BuildValidRequestLogsResponse()
            {
                StudentRequestLogs requestData = new StudentRequestLogs();
                requestData.Recordkey = "12";
                requestData.StrlRecipientName = "my name";
                requestData.StrlStudent = "11111";
                requestData.StrlTranscriptGroupings = null;
                requestData.StrlStuRequestLogHolds = "OTHER";
                requestData.StrlType = "E";
                return requestData;

            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Request_id_Is_Null()
            {
                var request = await requestRepository.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RequestLog_returned_is_null()
            {
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(null);
                var request = await requestRepository.GetAsync("12");
            }
            [TestMethod]
            public async Task Proper_domain_request_returned()
            {
                StudentRequest retrievedDomainRequest = await requestRepository.GetAsync("12");
                //final domain value created which is returned
                Assert.AreEqual(retrievedDomainRequest.GetType(), typeof(StudentEnrollmentRequest));
                Assert.AreEqual(retrievedDomainRequest.HoldRequest, "OTHER");
                Assert.AreEqual(retrievedDomainRequest.Id, "12");
                Assert.AreEqual(retrievedDomainRequest.StudentId, "11111");
                Assert.AreEqual(retrievedDomainRequest.RecipientName, "my name");
            }
        }

        [TestClass]
        public class GetStudentRequestsAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            Collection<StudentRequestLogs> requestResponseData;
            StudentRequestRepository requestRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                requestResponseData = BuildValidRequestLogsResponse();
                requestRepository = BuildValidRequestRepository();


            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
                requestResponseData = null;
            }


            private StudentRequestRepository BuildValidRequestRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                localCacheMock = new Mock<ObjectCache>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for a student with requests. 
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(requestResponseData);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                StudentRequestRepository repository = new StudentRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private Collection<StudentRequestLogs> BuildValidRequestLogsResponse()
            {
                Collection<StudentRequestLogs> studentRequestLogs = new Collection<StudentRequestLogs>();

                StudentRequestLogs requestData1 = new StudentRequestLogs();
                requestData1.Recordkey = "12";
                requestData1.StrlRecipientName = "Enrollment recipient name";
                requestData1.StrlStudent = "11111";
                requestData1.StrlType = "E";
                requestData1.StrlAddress = new List<string>() { "Address Line 1", "Address Line 2" };
                requestData1.StrlCity = "Some City";
                requestData1.StrlState = "ST";
                requestData1.StrlZip = "ZIPCODE";
                requestData1.StrlComments = string.Empty;
                requestData1.StrlDate = new DateTime(2016, 1, 10);
                requestData1.StrlPrintDate = DateTime.Now;
                
                studentRequestLogs.Add(requestData1);

                StudentRequestLogs requestData2 = new StudentRequestLogs();
                requestData2.Recordkey = "14";
                requestData2.StrlRecipientName = "Transcript recipient name";
                requestData2.StrlStudent = "11111";
                requestData1.StrlAddress = new List<string>() { "Foriegn Address Line 1", "Foreign Address Line 2" };
                requestData2.StrlTranscriptGroupings = new List<string>() {"UG"};
                requestData2.StrlStuRequestLogHolds = "OTHER";
                requestData2.StrlType = "T";
                requestData2.StrlComments = string.Empty;
                requestData2.StrlDate = DateTime.MinValue;
                requestData2.StrlPrintDate = null;
                studentRequestLogs.Add(requestData2);

                StudentRequestLogs requestData3 = new StudentRequestLogs();
                requestData2.Recordkey = "16";
                requestData2.StrlRecipientName = "Another Transcript recipient name";
                requestData2.StrlStudent = "11111";
                requestData1.StrlAddress = new List<string>() { "Some Address Line" };
                requestData2.StrlTranscriptGroupings = new List<string>() { "UG" };
                requestData2.StrlType = "T";
                requestData2.StrlComments = "This is the comments on the request.";
                requestData2.StrlPrintDate = new DateTime(2015, 12, 10);
                requestData2.StrlDate = new DateTime(2014, 1, 5);
                studentRequestLogs.Add(requestData2);

                return studentRequestLogs;

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Student_id_Is_Null()
            {
                var requests = await requestRepository.GetStudentRequestsAsync(null);
            }

            [TestMethod]
            public async Task RequestLogs_ReturnsEmptyList_WhenDataReaderNull()
            {
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(null);
                var requests = await requestRepository.GetStudentRequestsAsync("11111");
                Assert.AreEqual(0, requests.Count());
            }

            [TestMethod]
             public async Task RequestLogs_ReturnsEmptyList_WhenDataReaderReturnsEmptyList()
            {
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentRequestLogs>(It.IsAny<string>(), true)).ReturnsAsync(new Collection<StudentRequestLogs>());
                var requests = await requestRepository.GetStudentRequestsAsync("11111");
                Assert.AreEqual(0, requests.Count());
            }
 
            [TestMethod]
            public async Task Proper_domain_requests_returned()
            {
                List<StudentRequest> retrievedRequests = await requestRepository.GetStudentRequestsAsync("11111");
                Assert.AreEqual(requestResponseData.Count(), retrievedRequests.Count());
                foreach (var requestContract in requestResponseData)
                {
                    StudentRequest requestEntity = retrievedRequests.Where(r => r.Id == requestContract.Recordkey).FirstOrDefault();
                    Assert.IsNotNull(requestEntity);
                    if (requestContract.StrlType == "E")
                    {
                        Assert.AreEqual(requestEntity.GetType(), typeof(StudentEnrollmentRequest));
                    }
                    if (requestContract.StrlType == "T")
                    {
                        Assert.AreEqual(requestEntity.GetType(), typeof(StudentTranscriptRequest));
                        StudentTranscriptRequest str = requestEntity as StudentTranscriptRequest;
                        if (requestContract.StrlTranscriptGroupings != null && requestContract.StrlTranscriptGroupings.Any())
                        {
                            Assert.AreEqual(requestContract.StrlTranscriptGroupings.ElementAt(0), str.TranscriptGrouping);
                        }  
                    }
                    Assert.AreEqual(requestContract.StrlRecipientName, requestEntity.RecipientName);
                    Assert.AreEqual(requestContract.StrlStuRequestLogHolds, requestEntity.HoldRequest);
                    Assert.AreEqual(requestContract.StrlCopies, requestEntity.NumberOfCopies);
                    Assert.AreEqual(requestContract.StrlCity, requestEntity.MailToCity);
                    Assert.AreEqual(requestContract.StrlState, requestEntity.MailToState);
                    Assert.AreEqual(requestContract.StrlZip, requestEntity.MailToPostalCode);
                    Assert.AreEqual(requestContract.StrlCountry, requestEntity.MailToCountry);
                    Assert.AreEqual(requestContract.StrlAddress, requestEntity.MailToAddressLines);
                    Assert.AreEqual(requestContract.StrlComments, requestEntity.Comments);
                    Assert.AreEqual(requestContract.StrlDate, requestEntity.RequestDate);
                    Assert.AreEqual(requestContract.StrlPrintDate, requestEntity.CompletedDate);
                    
                }

            }
        }

        [TestClass]
        public class GetStudentRequestFeeAsync
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<ILogger> loggerMock;
            private ApiSettings apiSettings;
            private Mock<IColleagueTransactionInvoker> mockManager;
            private IStudentRequestRepository studentRequestRepository;
            private GetStudentRequestFeeRequest getFeeRequest;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
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
                studentRequestRepository = new StudentRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_NullStudent()
            {
                var studentRequestEntity = await studentRequestRepository.GetStudentRequestFeeAsync(null, "12345");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_EmptyStudent()
            {
                var studentRequestEntity = await studentRequestRepository.GetStudentRequestFeeAsync(string.Empty, "12345");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_NullProgramCode()
            {
                var studentRequestEntity = await studentRequestRepository.GetStudentRequestFeeAsync("studentId", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_EmptyProgramCode()
            {
                var studentRequestEntity = await studentRequestRepository.GetStudentRequestFeeAsync("studentId", string.Empty);
            }

            [TestMethod]
            public async Task GetStudentRequestFeeAsync_ReturnsValidFee()
            {
                var validResponse = BuildValidGetstudentRequestFeeResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetStudentRequestFeeRequest, GetStudentRequestFeeResponse>(It.IsAny<GetStudentRequestFeeRequest>())).Returns(Task.FromResult(validResponse)).Callback<GetStudentRequestFeeRequest>(req => getFeeRequest = req);
                var studentRequestFee = await studentRequestRepository.GetStudentRequestFeeAsync("studentId", "12345");
                //compare transaction request 
                Assert.AreEqual("studentId", studentRequestFee.StudentId);
                Assert.AreEqual("12345", studentRequestFee.RequestId);
                Assert.AreEqual(validResponse.Fee, studentRequestFee.Amount);
                Assert.AreEqual(validResponse.DistributionCode, studentRequestFee.PaymentDistributionCode);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentRequestFeeAsync_ColleagueTXThrowsException()
            {
                var validResponse = BuildValidEmptystudentRequestFeeResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetStudentRequestFeeRequest, GetStudentRequestFeeResponse>(It.IsAny<GetStudentRequestFeeRequest>())).Throws(new Exception());
                var studentRequestFee = await studentRequestRepository.GetStudentRequestFeeAsync("studentId", "12345");
            }

            [TestMethod]
            public async Task GetStudentRequestFeeAsync_ReturnsValidEmptyFee()
            {
                var validResponse = BuildValidEmptystudentRequestFeeResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetStudentRequestFeeRequest, GetStudentRequestFeeResponse>(It.IsAny<GetStudentRequestFeeRequest>())).Returns(Task.FromResult(validResponse)).Callback<GetStudentRequestFeeRequest>(req => getFeeRequest = req);
                var studentRequestFee = await studentRequestRepository.GetStudentRequestFeeAsync("studentId", "12345");
                //compare transaction request 
                Assert.AreEqual("studentId", studentRequestFee.StudentId);
                Assert.AreEqual("12345", studentRequestFee.RequestId);
                Assert.IsNull(studentRequestFee.Amount);
                Assert.IsNull(studentRequestFee.PaymentDistributionCode);
            }

            private GetStudentRequestFeeResponse BuildValidGetstudentRequestFeeResponse()
            {
                GetStudentRequestFeeResponse response = new GetStudentRequestFeeResponse();
                response.Fee = 100m;
                response.DistributionCode = "DISTR";
                return response;
            }

            private GetStudentRequestFeeResponse BuildValidEmptystudentRequestFeeResponse()
            {
                GetStudentRequestFeeResponse response = new GetStudentRequestFeeResponse();
                response.Fee = null;
                response.DistributionCode = null;
                return response;
            }
        }
    }
}
