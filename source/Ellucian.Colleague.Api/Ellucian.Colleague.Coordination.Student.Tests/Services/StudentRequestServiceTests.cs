// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentRequestServiceTests
    {
        // Sets up a Current user that is a faculty
        public abstract class CurrentUserSetup
        {
            protected Role studentRole = new Role(105, "Student");

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George Smith",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "GSmith",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }


        [TestClass]
        public class PostStudentTranscriptRequest : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentRequestRepository> requestRepoMock;
            private IStudentRequestRepository requestRepository;
            private IStudentRequestService requestService;
            private Domain.Student.Entities.StudentRequest requestResponse;
            private StudentRequest createdRepoRequest;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                requestRepoMock = new Mock<IStudentRequestRepository>();
                requestRepository = requestRepoMock.Object;
                // Mock Adapters from Dtos to Entitiies
                var requestDomainAdapter = new StudentRequestDtoToEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentRequest, Ellucian.Colleague.Domain.Student.Entities.StudentRequest>()).Returns(requestDomainAdapter);
                var transcriptRequestDomainAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.StudentTranscriptRequest, Ellucian.Colleague.Domain.Student.Entities.StudentTranscriptRequest>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentTranscriptRequest, Ellucian.Colleague.Domain.Student.Entities.StudentTranscriptRequest>()).Returns(transcriptRequestDomainAdapter);
                //Mock adapters from domain to dtos
                var requestDtoAdapter = new StudentRequestEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentRequest, Dtos.Student.StudentRequest>()).Returns(requestDtoAdapter);
                var transcriptRequestDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentTranscriptRequest, Dtos.Student.StudentTranscriptRequest>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentTranscriptRequest, Dtos.Student.StudentTranscriptRequest>()).Returns(transcriptRequestDtoAdapter);



                // Mock Waivers response
                requestResponse = BuildTranscriptRequestRepoResponse();
                requestRepoMock.Setup(repo => repo.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Returns(Task.FromResult(requestResponse)).Callback<StudentRequest>(req => createdRepoRequest = req);


                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                requestService = new StudentRequestService(adapterRegistry, requestRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                requestService = null;
                requestRepository = null;
            }



            private StudentRequest BuildTranscriptRequestRepoResponse()
            {
                StudentRequest str = new StudentTranscriptRequest("0000011", "my name", "UG");
                str.HoldRequest = "GRADE";
                str.Id = "1";
                str.NumberOfCopies = 3;
                return str;
            }

            private Dtos.Student.StudentRequest BuildValidTranscriptRequestDto()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentTranscriptRequest("0000011", "my name", new List<string>() { "address line 1" }) { TranscriptGrouping = "UG" };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                return str;

            }

            [TestMethod]
            public async Task Valid_Transcript_Request_Dto()
            {

                Dtos.Student.StudentRequest str = BuildValidTranscriptRequestDto();
                Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
                //will compare the intermediate domain object sent to repository - this will assert adaptor mapping from dto to domain
                Assert.AreEqual(createdRepoRequest.GetType(), typeof(StudentTranscriptRequest));
                Assert.AreEqual(createdRepoRequest.Id, null);
                Assert.AreEqual(createdRepoRequest.HoldRequest, "GRADE");
                Assert.AreEqual(createdRepoRequest.NumberOfCopies, 3);
                Assert.AreEqual(createdRepoRequest.StudentId, "0000011");
                Assert.AreEqual(createdRepoRequest.RecipientName, "my name");
                Assert.AreEqual((createdRepoRequest as StudentTranscriptRequest).TranscriptGrouping, "UG");

                //this is newly created dto that is returned by service
                Assert.AreEqual(createdStudentRequest.GetType(), typeof(Dtos.Student.StudentTranscriptRequest));
                Assert.AreEqual(createdStudentRequest.Id, "1");
                Assert.AreEqual(createdStudentRequest.HoldRequest, "GRADE");
                Assert.AreEqual(createdStudentRequest.NumberOfCopies, 3);
                Assert.AreEqual(createdStudentRequest.StudentId, "0000011");
                Assert.AreEqual(createdStudentRequest.RecipientName, "my name");
                Assert.AreEqual((createdStudentRequest as Dtos.Student.StudentTranscriptRequest).TranscriptGrouping, "UG");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentId_Transcript_Request_Is_Empty()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentTranscriptRequest("", "my name", new List<string>() { "address line 1" }) { TranscriptGrouping = "UG" };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ReciepientName_Transcript_Request_Is_Empty()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentTranscriptRequest("11111", "", new List<string>() { "address line 1" }) { TranscriptGrouping = "UG" };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "address line 1" };

                Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task TranscriptGrouping_Is_Empty_And_user_not_same_as_personId()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentTranscriptRequest("11111", "my name", new List<string>() { "address line 1" }) { TranscriptGrouping = null };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;

                Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentId_RecipientName_Transcript_Request_Is_Empty()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentTranscriptRequest(null, null, new List<string>() { "address line 1" }) { TranscriptGrouping = "UG" };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "address line 1" };
                try
                {
                    Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
                }
                catch (ArgumentException ex)
                {
                    Assert.IsTrue(ex.Message.Contains("StudentId"));
                    Assert.IsTrue(ex.Message.Contains("RecipientName"));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task MailAddressLines_Transcript_Request_Is_Empty()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentTranscriptRequest("1111", "my name", null) { TranscriptGrouping = "UG" };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
            }

        }

        [TestClass]
        public class PostStudentEnrollmentRequest : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentRequestRepository> requestRepoMock;
            private IStudentRequestRepository requestRepository;
            private IStudentRequestService requestService;
            private Domain.Student.Entities.StudentRequest requestResponse;
            private StudentRequest createdRepoRequest;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                requestRepoMock = new Mock<IStudentRequestRepository>();
                requestRepository = requestRepoMock.Object;
                // Mock Adapters from Dtos to Entitiies
                var requestDomainAdapter = new StudentRequestDtoToEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentRequest, Ellucian.Colleague.Domain.Student.Entities.StudentRequest>()).Returns(requestDomainAdapter);
                var enrollmentRequestDomainAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.StudentEnrollmentRequest, Ellucian.Colleague.Domain.Student.Entities.StudentEnrollmentRequest>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentEnrollmentRequest, Ellucian.Colleague.Domain.Student.Entities.StudentEnrollmentRequest>()).Returns(enrollmentRequestDomainAdapter);
                //Mock adapters from domain to dtos
                var requestDtoAdapter = new StudentRequestEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentRequest, Dtos.Student.StudentRequest>()).Returns(requestDtoAdapter);
                var enrollmentRequestDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentEnrollmentRequest, Dtos.Student.StudentEnrollmentRequest>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentEnrollmentRequest, Dtos.Student.StudentEnrollmentRequest>()).Returns(enrollmentRequestDtoAdapter);



                // Mock Waivers response
                requestResponse = BuildEnrollmentRequestRepoResponse();
                requestRepoMock.Setup(repo => repo.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Returns(Task.FromResult(requestResponse)).Callback<StudentRequest>(req => createdRepoRequest = req);


                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                requestService = new StudentRequestService(adapterRegistry, requestRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                requestService = null;
                requestRepository = null;
            }



            private StudentRequest BuildEnrollmentRequestRepoResponse()
            {
                StudentRequest str = new StudentEnrollmentRequest("0000011", "my name");
                str.HoldRequest = "GRADE";
                str.Id = "1";
                str.NumberOfCopies = 3;
                return str;
            }

            private Dtos.Student.StudentRequest BuildValidEnrollmentRequestDto()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentEnrollmentRequest("0000011", "my name", new List<string>() { "address line 1" });
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                return str;

            }

            [TestMethod]
            public async Task Valid_Enrollment_Request_Dto()
            {

                Dtos.Student.StudentRequest str = BuildValidEnrollmentRequestDto();
                Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
                //will compare the intermediate domain object sent to repository - this will assert adaptor mapping from dto to domain
                Assert.AreEqual(createdRepoRequest.GetType(), typeof(StudentEnrollmentRequest));
                Assert.AreEqual(createdRepoRequest.Id, null);
                Assert.AreEqual(createdRepoRequest.HoldRequest, "GRADE");
                Assert.AreEqual(createdRepoRequest.NumberOfCopies, 3);
                Assert.AreEqual(createdRepoRequest.StudentId, "0000011");
                Assert.AreEqual(createdRepoRequest.RecipientName, "my name");

                //this is newly created dto that is returned by service
                Assert.AreEqual(createdStudentRequest.GetType(), typeof(Dtos.Student.StudentEnrollmentRequest));
                Assert.AreEqual(createdStudentRequest.Id, "1");
                Assert.AreEqual(createdStudentRequest.HoldRequest, "GRADE");
                Assert.AreEqual(createdStudentRequest.NumberOfCopies, 3);
                Assert.AreEqual(createdStudentRequest.StudentId, "0000011");
                Assert.AreEqual(createdStudentRequest.RecipientName, "my name");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentId_Enrollment_Request_Is_Empty()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentEnrollmentRequest("", "my name", new List<string>() { "address line 1" });
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ReciepientName_Enrollment_Request_Is_Empty()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentEnrollmentRequest("11111", "", new List<string>() { "address line 1" });
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "address line 1" };

                Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentId_RecipientName_Enrollment_Request_Is_Empty()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentEnrollmentRequest(null, null, new List<string>() { "address line 1" });
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "address line 1" };
                try
                {
                    Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
                }
                catch (ArgumentException ex)
                {
                    Assert.IsTrue(ex.Message.Contains("StudentId"));
                    Assert.IsTrue(ex.Message.Contains("RecipientName"));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task MailAddressLines_Enrollment_Request_Is_Empty()
            {
                Ellucian.Colleague.Dtos.Student.StudentRequest str = new Ellucian.Colleague.Dtos.Student.StudentEnrollmentRequest("1111", "my name", null);
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                Dtos.Student.StudentRequest createdStudentRequest = await requestService.CreateStudentRequestAsync(str);
            }

        }
        [TestClass]
        public class GetStudentTranscriptRequest : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentRequestRepository> requestRepoMock;
            private IStudentRequestRepository requestRepository;
            private IStudentRequestService requestService;
            private Domain.Student.Entities.StudentRequest requestResponse;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                requestRepoMock = new Mock<IStudentRequestRepository>();
                requestRepository = requestRepoMock.Object;
                //Mock adapters from domain to dtos
                var requestDtoAdapter = new StudentRequestEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentRequest, Dtos.Student.StudentRequest>()).Returns(requestDtoAdapter);
                var transcriptRequestDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentTranscriptRequest, Dtos.Student.StudentTranscriptRequest>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentTranscriptRequest, Dtos.Student.StudentTranscriptRequest>()).Returns(transcriptRequestDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                requestService = new StudentRequestService(adapterRegistry, requestRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                requestService = null;
                requestRepository = null;
            }

            private StudentRequest BuildInvalidTranscriptRequestRepoResponse()
            {
                //required field mailtoaddress lines is missing
                StudentRequest str = new StudentTranscriptRequest("0000011", "my name", "UG");
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.Id = "1";
                return str;
            }
            private StudentRequest BuildTranscriptRequestRepoResponse()
            {
                StudentRequest str = new StudentTranscriptRequest("0000011", "my name", "UG");
                str.HoldRequest = "GRADE";
                str.Id = "1";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "line 1" };
                return str;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Request_Id_is_Null_Transcript_Request()
            {

                var str = await requestService.GetStudentRequestAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Request_Id_not_valid_Transcript_Request()
            {

                requestRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                var str = await requestService.GetStudentRequestAsync("2");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Exception_in_Repository_Transcript_Request()
            {
                requestRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Throws(new Exception());
                var str = await requestService.GetStudentRequestAsync("2");
            }
            //proper domain data retuned, adaptor converted to dto, dto is correct
            [TestMethod]
            public async Task Proper_dto_returned_transcript_request()
            {
                requestResponse = BuildTranscriptRequestRepoResponse();
                requestRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(requestResponse));

                Dtos.Student.StudentRequest retrievedStudentRequest = await requestService.GetStudentRequestAsync("1");

                //this is newly created dto that is returned by service
                Assert.AreEqual(retrievedStudentRequest.GetType(), typeof(Dtos.Student.StudentTranscriptRequest));
                Assert.AreEqual(retrievedStudentRequest.Id, "1");
                Assert.AreEqual(retrievedStudentRequest.HoldRequest, "GRADE");
                Assert.AreEqual(retrievedStudentRequest.NumberOfCopies, 3);
                Assert.AreEqual(retrievedStudentRequest.StudentId, "0000011");
                Assert.AreEqual(retrievedStudentRequest.RecipientName, "my name");
                Assert.AreEqual((retrievedStudentRequest as Dtos.Student.StudentTranscriptRequest).TranscriptGrouping, "UG");

            }

            //proper domain data retuned, adaptor converted to dto, dto is correct
            [TestMethod]
            public async Task dto_returned_but_has_required_addresslines_missing_transcript_request()
            {
                requestResponse = BuildInvalidTranscriptRequestRepoResponse();
                requestRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(requestResponse));

                Dtos.Student.StudentRequest retrievedStudentRequest = await requestService.GetStudentRequestAsync("1");

                //this is newly created dto that is returned by service
                Assert.AreEqual(retrievedStudentRequest.GetType(), typeof(Dtos.Student.StudentTranscriptRequest));
                Assert.AreEqual(retrievedStudentRequest.Id, "1");
                Assert.AreEqual(retrievedStudentRequest.HoldRequest, "GRADE");
                Assert.AreEqual(retrievedStudentRequest.NumberOfCopies, 3);
                Assert.AreEqual(retrievedStudentRequest.StudentId, "0000011");
                Assert.AreEqual(retrievedStudentRequest.RecipientName, "my name");
                Assert.AreEqual((retrievedStudentRequest as Dtos.Student.StudentTranscriptRequest).TranscriptGrouping, "UG");

            }
        }

        [TestClass]
        public class GetStudentEnrollmentRequest : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentRequestRepository> requestRepoMock;
            private IStudentRequestRepository requestRepository;
            private IStudentRequestService requestService;
            private Domain.Student.Entities.StudentRequest requestResponse;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                requestRepoMock = new Mock<IStudentRequestRepository>();
                requestRepository = requestRepoMock.Object;
                //Mock adapters from domain to dtos
                var requestDtoAdapter = new StudentRequestEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentRequest, Dtos.Student.StudentRequest>()).Returns(requestDtoAdapter);
                var enrollmentRequestDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentEnrollmentRequest, Dtos.Student.StudentEnrollmentRequest>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentEnrollmentRequest, Dtos.Student.StudentEnrollmentRequest>()).Returns(enrollmentRequestDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                requestService = new StudentRequestService(adapterRegistry, requestRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                requestService = null;
                requestRepository = null;
            }

            private StudentRequest BuildInvalidEnrollmentRequestRepoResponse()
            {
                //required field mailtoaddress lines is missing
                StudentRequest str = new StudentEnrollmentRequest("0000011", "my name");
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.Id = "1";
                return str;
            }
            private StudentRequest BuildEnrollmentRequestRepoResponse()
            {
                StudentRequest str = new StudentEnrollmentRequest("0000011", "my name");
                str.HoldRequest = "GRADE";
                str.Id = "1";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "line 1" };
                return str;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Request_Id_is_Null_Enrollment_Request()
            {

                var str = await requestService.GetStudentRequestAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Request_Id_not_valid_Enrollment_Request()
            {

                requestRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                var str = await requestService.GetStudentRequestAsync("2");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Exception_in_Repository_Enrollment_Request()
            {
                requestRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Throws(new Exception());
                var str = await requestService.GetStudentRequestAsync("2");
            }
            //proper domain data retuned, adaptor converted to dto, dto is correct
            [TestMethod]
            public async Task Proper_dto_returned_Enrollment_request()
            {
                requestResponse = BuildEnrollmentRequestRepoResponse();
                requestRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(requestResponse));

                Dtos.Student.StudentRequest retrievedStudentRequest = await requestService.GetStudentRequestAsync("1");

                //this is newly created dto that is returned by service
                Assert.AreEqual(retrievedStudentRequest.GetType(), typeof(Dtos.Student.StudentEnrollmentRequest));
                Assert.AreEqual(retrievedStudentRequest.Id, "1");
                Assert.AreEqual(retrievedStudentRequest.HoldRequest, "GRADE");
                Assert.AreEqual(retrievedStudentRequest.NumberOfCopies, 3);
                Assert.AreEqual(retrievedStudentRequest.StudentId, "0000011");
                Assert.AreEqual(retrievedStudentRequest.RecipientName, "my name");

            }

            //proper domain data retuned, adaptor converted to dto, dto is correct
            [TestMethod]
            public async Task dto_returned_but_has_required_addresslines_missing_Enrollment_request()
            {
                requestResponse = BuildInvalidEnrollmentRequestRepoResponse();
                requestRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(requestResponse));

                Dtos.Student.StudentRequest retrievedStudentRequest = await requestService.GetStudentRequestAsync("1");

                //this is newly created dto that is returned by service
                Assert.AreEqual(retrievedStudentRequest.GetType(), typeof(Dtos.Student.StudentEnrollmentRequest));
                Assert.AreEqual(retrievedStudentRequest.Id, "1");
                Assert.AreEqual(retrievedStudentRequest.HoldRequest, "GRADE");
                Assert.AreEqual(retrievedStudentRequest.NumberOfCopies, 3);
                Assert.AreEqual(retrievedStudentRequest.StudentId, "0000011");
                Assert.AreEqual(retrievedStudentRequest.RecipientName, "my name");

            }
        }

        [TestClass]
        public class GetStudentRequestsAsync : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentRequestRepository> requestRepoMock;
            private IStudentRequestRepository requestRepository;
            private IStudentRequestService requestService;
            private List<Domain.Student.Entities.StudentRequest> requestsResponse;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                requestRepoMock = new Mock<IStudentRequestRepository>();
                requestRepository = requestRepoMock.Object;

                //Mock adapters from domain to dtos
                var requestDtoAdapter = new StudentRequestEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentRequest, Dtos.Student.StudentRequest>()).Returns(requestDtoAdapter);

                var enrollmentRequestDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentEnrollmentRequest, Dtos.Student.StudentEnrollmentRequest>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentEnrollmentRequest, Dtos.Student.StudentEnrollmentRequest>()).Returns(enrollmentRequestDtoAdapter);

                var transcriptRequestDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentTranscriptRequest, Dtos.Student.StudentTranscriptRequest>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentTranscriptRequest, Dtos.Student.StudentTranscriptRequest>()).Returns(transcriptRequestDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                requestService = new StudentRequestService(adapterRegistry, requestRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                requestService = null;
                requestRepository = null;
            }

            private List<StudentRequest> BuildInvalidStudentRequestRepoResponse()
            {
                List<StudentRequest> invalidStuRequests = new List<StudentRequest>();

                //required field mailtoaddress lines is missing
                StudentRequest str = new StudentEnrollmentRequest("0000011", "my name");
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.Id = "1";
                invalidStuRequests.Add(str);

                return invalidStuRequests;
            }
            private List<StudentRequest> BuildStudentRequestsRepoResponse()
            {
                List<StudentRequest> stuRequests = new List<StudentRequest>();

                StudentRequest str1 = new StudentEnrollmentRequest("0000011", "enrollment recipient name") { Id = "1", NumberOfCopies = 3, MailToAddressLines = new List<string>() { "Line 1" }, RequestDate = DateTime.Now };
                stuRequests.Add(str1);

                StudentRequest str2 = new StudentTranscriptRequest("0000011", "transcript recipient name", "UG") { Comments = "Something here", HoldRequest = "GRADE", Id = "2", NumberOfCopies = 2, MailToAddressLines = new List<string>() { "String 1", "String 2" }, MailToCity = "The City", MailToCountry = "", MailToPostalCode = "Post it", MailToState = "STATE" };
                stuRequests.Add(str2);

                StudentRequest str3 = new StudentEnrollmentRequest("0000011", "enrollment recipient another") { Id = "3", NumberOfCopies = 1, MailToAddressLines = new List<string>() { "Another one" }, MailToCity = "Another City", MailToCountry = "USA", MailToPostalCode = "Postal 1", MailToState = "ST", Comments = "Comments/nComments", RequestDate = new DateTime(2016, 1, 10), CompletedDate = new DateTime(2016, 2, 20) };
                stuRequests.Add(str3);

                StudentRequest str4 = new StudentTranscriptRequest("0000011", "transcript recipient two", null) { Comments = "Something else here", HoldRequest = "", Id = "4", NumberOfCopies = 9, MailToAddressLines = new List<string>() { "String 3", "String 4" }, MailToCity = "Black City", MailToCountry = "FR", MailToPostalCode = "", MailToState = "" };
                stuRequests.Add(str4);

                StudentRequest str5 = new StudentEnrollmentRequest("0000011", "enrollment") { Id = "5", NumberOfCopies = 1, MailToAddressLines = new List<string>() { "Another one" } };
                stuRequests.Add(str5);

                return stuRequests;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Student_Id_is_Null_GetStudentRequests()
            {

                var str = await requestService.GetStudentRequestsAsync(null, "Transcript");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequestType_is_Null_GetStudentRequests()
            {

                var str = await requestService.GetStudentRequestsAsync("0000011", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequestType_is_Empty_GetStudentRequests()
            {

                var str = await requestService.GetStudentRequestsAsync("0000011", string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequestType_is_Invalid_GetStudentRequests()
            {

                var str = await requestService.GetStudentRequestsAsync("0000011", "Other");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentRequests_PermissionFailure()
            {

                var str = await requestService.GetStudentRequestsAsync("0000022", "Transcript");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentRequests_RepoThrowsOtherException()
            {
                requestRepoMock.Setup(repo => repo.GetStudentRequestsAsync(It.IsAny<string>())).Throws(new Exception());
                var str = await requestService.GetStudentRequestsAsync("0000011", "Transcript");
            }
            //proper domain data retuned, adaptor converted to dto, dto is correct
            [TestMethod]
            public async Task Proper_dtos_returned_GetStudentRequests_Enrollment()
            {
                requestsResponse = BuildStudentRequestsRepoResponse();
                requestRepoMock.Setup(repo => repo.GetStudentRequestsAsync(It.IsAny<string>())).Returns(Task.FromResult(requestsResponse));

                List<Dtos.Student.StudentRequest> retrievedEnrollmentRequests = await requestService.GetStudentRequestsAsync("0000011", "Enrollment");
                Assert.AreEqual(3, retrievedEnrollmentRequests.Count());

                foreach (var req in requestsResponse)
                {
                    if (req.GetType() == typeof(Dtos.Student.StudentEnrollmentRequest))
                    {
                        var retrievedReq = retrievedEnrollmentRequests.Where(e => e.Id == req.Id).FirstOrDefault();
                        Assert.AreEqual(retrievedReq.GetType(), typeof(Dtos.Student.StudentEnrollmentRequest));
                        Assert.AreEqual(req.HoldRequest, retrievedReq.HoldRequest);
                        Assert.AreEqual(req.MailToPostalCode, retrievedReq.MailToPostalCode);
                        Assert.AreEqual(req.MailToAddressLines, retrievedReq.MailToAddressLines);
                        Assert.AreEqual(req.MailToCity, retrievedReq.MailToCity);
                        Assert.AreEqual(req.MailToState, retrievedReq.MailToState);
                        Assert.AreEqual(req.NumberOfCopies, retrievedReq.NumberOfCopies);
                        Assert.AreEqual(req.RecipientName, retrievedReq.RecipientName);
                        Assert.AreEqual(req.StudentId, retrievedReq.StudentId);
                        Assert.AreEqual(req.Comments, retrievedReq.Comments);
                        Assert.AreEqual(req.CompletedDate, retrievedReq.CompletedDate);
                        Assert.AreEqual(req.RequestDate, retrievedReq.RequestDate);
                    }


                }
            }

            //proper domain data retuned, adaptor converted to dto, dto is correct
            [TestMethod]
            public async Task Proper_dtos_returned_GetStudentRequests_Transcript()
            {
                requestsResponse = BuildStudentRequestsRepoResponse();
                requestRepoMock.Setup(repo => repo.GetStudentRequestsAsync(It.IsAny<string>())).Returns(Task.FromResult(requestsResponse));

                List<Dtos.Student.StudentRequest> retrievedTranscriptRequests = await requestService.GetStudentRequestsAsync("0000011", "Transcript");
                Assert.AreEqual(2, retrievedTranscriptRequests.Count());

                foreach (var req in requestsResponse)
                {
                    if (req.GetType() == typeof(Dtos.Student.StudentTranscriptRequest))
                    {
                        var retrievedReq = retrievedTranscriptRequests.Where(e => e.Id == req.Id).FirstOrDefault();
                        Assert.AreEqual(retrievedReq.GetType(), typeof(Dtos.Student.StudentTranscriptRequest));
                        Assert.AreEqual(req.HoldRequest, retrievedReq.HoldRequest);
                        Assert.AreEqual(req.MailToPostalCode, retrievedReq.MailToPostalCode);
                        Assert.AreEqual(req.MailToAddressLines, retrievedReq.MailToAddressLines);
                        Assert.AreEqual(req.MailToCity, retrievedReq.MailToCity);
                        Assert.AreEqual(req.MailToState, retrievedReq.MailToState);
                        Assert.AreEqual(req.NumberOfCopies, retrievedReq.NumberOfCopies);
                        Assert.AreEqual(req.RecipientName, retrievedReq.RecipientName);
                        Assert.AreEqual(req.StudentId, retrievedReq.StudentId);
                        Assert.AreEqual(req.Comments, retrievedReq.Comments);
                        Assert.AreEqual(req.CompletedDate, retrievedReq.CompletedDate);
                        Assert.AreEqual(req.RequestDate, retrievedReq.RequestDate);
                    }


                }
            }


            [TestMethod]
            public async Task EmptyListWhenNone_GetStudentRequests_Transcript()
            {
                requestRepoMock.Setup(repo => repo.GetStudentRequestsAsync(It.IsAny<string>())).Returns(Task.FromResult(new List<StudentRequest>()));

                List<Dtos.Student.StudentRequest> retrievedTranscriptRequests = await requestService.GetStudentRequestsAsync("0000011", "Transcript");
                Assert.AreEqual(0, retrievedTranscriptRequests.Count());

            }


            [TestMethod]
            public async Task EmptyListWhenNoneOfRightType_GetStudentRequests_Transcript()
            {
                requestsResponse = BuildStudentRequestsRepoResponse();
                List<Domain.Student.Entities.StudentRequest> limitedResponse = requestsResponse.Where(rr => rr.GetType() == typeof(StudentEnrollmentRequest)).ToList();
                requestRepoMock.Setup(repo => repo.GetStudentRequestsAsync(It.IsAny<string>())).Returns(Task.FromResult(limitedResponse));

                List<Dtos.Student.StudentRequest> retrievedTranscriptRequests = await requestService.GetStudentRequestsAsync("0000011", "Transcript");
                Assert.AreEqual(0, retrievedTranscriptRequests.Count());

            }
        }

        [TestClass]
        public class GetStudentRequestFees:CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;
            private Mock<IStudentRequestRepository> studentRequestRepositoryMock;
            private IStudentRequestRepository studentRequestRepository;
            private IStudentRequestService studentRequestService;
            private StudentRequestFee studentRequestFeeEntityData;
            private StudentRequest studentRequestEntity;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;
                studentRequestRepositoryMock = new Mock<IStudentRequestRepository>();
                studentRequestRepository = studentRequestRepositoryMock.Object;
                studentRequestEntity = new StudentTranscriptRequest("0000011", "my name", "UG") { Id = "12345" };
                studentRequestRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(studentRequestEntity));
                logger = new Mock<ILogger>().Object;
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                var studentRequestDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRequestFee, Ellucian.Colleague.Dtos.Student.StudentRequestFee>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRequestFee, Ellucian.Colleague.Dtos.Student.StudentRequestFee>()).Returns(studentRequestDtoAdapter);
                studentRequestService = new StudentRequestService(adapterRegistry, studentRequestRepository, currentUserFactory, roleRepository, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                studentRequestRepositoryMock = null;
            }
            [TestMethod]
            public async Task GetStudentRequestFeeAsync_GetsStudentRequestFeeDto()
            {
                var studentId = "0000011";
                var requestId = "12345";
                var amount = 50m;
                var distribution = "DIST";
                studentRequestFeeEntityData = new StudentRequestFee(studentId, requestId, amount, distribution);
                studentRequestRepositoryMock.Setup(x => x.GetStudentRequestFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(studentRequestFeeEntityData));

                var studentRequestFeeDto = await studentRequestService.GetStudentRequestFeeAsync(studentId, requestId);
                Assert.IsTrue(studentRequestFeeDto is Dtos.Student.StudentRequestFee);
                Assert.AreEqual(studentId, studentRequestFeeDto.StudentId);
                Assert.AreEqual(requestId, studentRequestFeeDto.RequestId);
                Assert.AreEqual(amount, studentRequestFeeDto.Amount);
                Assert.AreEqual(distribution, studentRequestFeeDto.PaymentDistributionCode);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_BothParametersEmpty_ArgumentNullException()
            {
                var studentRequestDto = await studentRequestService.GetStudentRequestFeeAsync("", "");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_BothParametersNull_ArgumentNullException()
            {
                var studentRequestDto = await studentRequestService.GetStudentRequestFeeAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_RequestIdEmpty_ArgumentNullException()
            {
                var studentRequestDto = await studentRequestService.GetStudentRequestFeeAsync("0000011", "");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_RequestIdNull_ArgumentNullException()
            {
                var studentRequestDto = await studentRequestService.GetStudentRequestFeeAsync("0000011", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_StudentIdNull_ArgumentNullException()
            {
                var studentRequestDto = await studentRequestService.GetStudentRequestFeeAsync(null, "12345");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_StudentIdEmpty_ArgumentNullException()
            {
                var studentRequestDto = await studentRequestService.GetStudentRequestFeeAsync("", "12345");
            }
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentRequestFeeAsync_ExceptionFromRepository()
            {
                var studentId = "0000011";
                var programCode = "12345";
                var amount = 50m;
                var distribution = "DIST";
                GraduationApplicationFee studentRequestEntityFeeData = new GraduationApplicationFee(studentId, programCode, amount, distribution);
                studentRequestRepositoryMock.Setup(x => x.GetStudentRequestFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
                var studentRequestDto = await studentRequestService.GetStudentRequestFeeAsync("0000011", "12345");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentRequestFeeAsync_Student_Not_Same_As_CurrentUser()
            {
                var studentId = "0000012";
                var requestId = "12345";
                var amount = 50m;
                var distribution = "DIST";
                studentRequestFeeEntityData = new StudentRequestFee(studentId, requestId, amount, distribution);
                studentRequestRepositoryMock.Setup(x => x.GetStudentRequestFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(studentRequestFeeEntityData));
                var studentRequestFeeDto = await studentRequestService.GetStudentRequestFeeAsync(studentId, requestId);
             }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentRequestFeeAsync_Student_Is_Same_As_CurrentUser_But_RequestId_NotBelongToStudent()
            {
                var studentId = "0000011";
                var requestId = "12345";
                var amount = 50m;
                var distribution = "DIST";
                StudentRequest modStudentRequestEntity = new StudentTranscriptRequest("0000013", "my name", "UG") { Id = "12345" };
                studentRequestRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(modStudentRequestEntity));
                studentRequestFeeEntityData = new StudentRequestFee(studentId, requestId, amount, distribution);
                studentRequestRepositoryMock.Setup(x => x.GetStudentRequestFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(studentRequestFeeEntityData));
                var studentRequestFeeDto = await studentRequestService.GetStudentRequestFeeAsync("0000011", requestId);
            }
        }
    }
}