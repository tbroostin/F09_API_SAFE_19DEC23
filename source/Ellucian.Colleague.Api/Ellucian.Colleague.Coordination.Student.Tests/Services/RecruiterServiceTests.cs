// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class RecruiterServiceTests
    {
        public class RecruiterUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Matt",
                        PersonId = "0000001",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "RecruiterSvc" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class NonRecruiterUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Matt",
                        PersonId = "0000001",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "Student" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        private Mock<IRecruiterRepository> recruiterRepositoryMock;
        private IRecruiterRepository recruiterRepository;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;
        private ICurrentUserFactory currentUserFactory;
        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;
        private Mock<ILogger> loggerMock;
        private ILogger logger;

        private RecruiterService service;
        private List<Domain.Entities.Role> allRoles;

        [TestInitialize]
        public void RecruiterServiceTests_Initialize()
        {
            recruiterRepositoryMock = new Mock<IRecruiterRepository>();
            recruiterRepository = recruiterRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            currentUserFactoryMock.Setup(uf => uf.CurrentUser).Returns(new RecruiterUserFactory().CurrentUser);
            currentUserFactory = currentUserFactoryMock.Object;

            roleRepositoryMock = new Mock<IRoleRepository>();
            // Set up list of all roles, including role from current user, with Recruiter permission
            allRoles = new List<Domain.Entities.Role>()
            {
                new Domain.Entities.Role(1, new RecruiterUserFactory().CurrentUser.Roles.ElementAt(0))
            };
            allRoles[0].AddPermission(new Domain.Entities.Permission(BasePermissionCodes.PerformRecruiterOperations));
            roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(allRoles);
            roleRepository = roleRepositoryMock.Object;

            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;

            service = new RecruiterService(recruiterRepository, adapterRegistry, currentUserFactory, roleRepository, logger);
        }

        [TestClass]
        public class RecruiterService_ImportApplicationAsync_Tests : RecruiterServiceTests
        {
            private Application applicationDto;

            [TestInitialize]
            public void RecruiterService_ImportApplicationAsync_Initialize()
            {
                base.RecruiterServiceTests_Initialize();
                applicationDto = new Application()
                {
                    AcademicProgram = "PROG1"
                };

                // Setup for Application DTO --> Entity conversion
                var applicationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.Application, Ellucian.Colleague.Domain.Student.Entities.Application>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.Application, Ellucian.Colleague.Domain.Student.Entities.Application>()).Returns(applicationDtoAdapter);

                // Setup for repository call
                recruiterRepositoryMock.Setup(repo => repo.ImportApplicationAsync(It.IsAny<Domain.Student.Entities.Application>())).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RecruiterService_ImportApplicationAsync_throws_PermissionsException_for_unauthorized_user()
            {
                currentUserFactoryMock.Setup(uf => uf.CurrentUser).Returns(new NonRecruiterUserFactory().CurrentUser);
                await service.ImportApplicationAsync(applicationDto);
            }

            [TestMethod]
            public async Task RecruiterService_ImportApplicationAsync_calls_repository_for_authorized_user()
            {
                await service.ImportApplicationAsync(applicationDto);
                recruiterRepositoryMock.Verify(repo => repo.ImportApplicationAsync(It.IsAny<Domain.Student.Entities.Application>()));
            }
        }

        [TestClass]
        public class RecruiterService_UpdateApplicationStatusAsync_Tests : RecruiterServiceTests
        {
            private Application applicationDto;

            [TestInitialize]
            public void RecruiterService_UpdateApplicationStatusAsync_Initialize()
            {
                base.RecruiterServiceTests_Initialize();
                applicationDto = new Application()
                {
                    AcademicProgram = "PROG1"
                };

                // Setup for Application DTO --> Entity conversion
                var applicationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.Application, Ellucian.Colleague.Domain.Student.Entities.Application>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.Application, Ellucian.Colleague.Domain.Student.Entities.Application>()).Returns(applicationDtoAdapter);

                // Setup for repository call
                recruiterRepositoryMock.Setup(repo => repo.UpdateApplicationAsync(It.IsAny<Domain.Student.Entities.Application>())).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RecruiterService_UpdateApplicationStatusAsync_throws_PermissionsException_for_unauthorized_user()
            {
                currentUserFactoryMock.Setup(uf => uf.CurrentUser).Returns(new NonRecruiterUserFactory().CurrentUser);
                await service.UpdateApplicationStatusAsync(applicationDto);
            }

            [TestMethod]
            public async Task RecruiterService_UpdateApplicationStatusAsync_calls_repository_for_authorized_user()
            {
                await service.UpdateApplicationStatusAsync(applicationDto);
                recruiterRepositoryMock.Verify(repo => repo.UpdateApplicationAsync(It.IsAny<Domain.Student.Entities.Application>()));
            }
        }

        [TestClass]
        public class RecruiterService_ImportTestScoresAsync_Tests : RecruiterServiceTests
        {
            private TestScore testScoreDto;

            [TestInitialize]
            public void RecruiterService_ImportTestScoresAsync_Initialize()
            {
                base.RecruiterServiceTests_Initialize();
                testScoreDto = new TestScore()
                {
                    Score = "SCORE1"
                };

                // Setup for TestScore DTO --> Entity conversion
                var testScoreDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.TestScore, Ellucian.Colleague.Domain.Student.Entities.TestScore>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.TestScore, Ellucian.Colleague.Domain.Student.Entities.TestScore>()).Returns(testScoreDtoAdapter);

                // Setup for repository call
                recruiterRepositoryMock.Setup(repo => repo.ImportTestScoresAsync(It.IsAny<Domain.Student.Entities.TestScore>())).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RecruiterService_ImportTestScoresAsync_throws_PermissionsException_for_unauthorized_user()
            {
                currentUserFactoryMock.Setup(uf => uf.CurrentUser).Returns(new NonRecruiterUserFactory().CurrentUser);
                await service.ImportTestScoresAsync(testScoreDto);
            }

            [TestMethod]
            public async Task RecruiterService_ImportTestScoresAsync_calls_repository_for_authorized_user()
            {
                await service.ImportTestScoresAsync(testScoreDto);
                recruiterRepositoryMock.Verify(repo => repo.ImportTestScoresAsync(It.IsAny<Domain.Student.Entities.TestScore>()));
            }
        }

        [TestClass]
        public class RecruiterService_ImportTranscriptCoursesAsync_Tests : RecruiterServiceTests
        {
            private TranscriptCourse transcriptCourseDto;

            [TestInitialize]
            public void RecruiterService_ImportTranscriptCoursesAsync_Initialize()
            {
                base.RecruiterServiceTests_Initialize();
                transcriptCourseDto = new TranscriptCourse()
                {
                    Course = "COURSE1"
                };

                // Setup for TranscriptCourse DTO --> Entity conversion
                var transcriptCourseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.TranscriptCourse, Ellucian.Colleague.Domain.Student.Entities.TranscriptCourse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.TranscriptCourse, Ellucian.Colleague.Domain.Student.Entities.TranscriptCourse>()).Returns(transcriptCourseDtoAdapter);

                // Setup for repository call
                recruiterRepositoryMock.Setup(repo => repo.ImportTranscriptCoursesAsync(It.IsAny<Domain.Student.Entities.TranscriptCourse>())).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RecruiterService_ImportTranscriptCoursesAsync_throws_PermissionsException_for_unauthorized_user()
            {
                currentUserFactoryMock.Setup(uf => uf.CurrentUser).Returns(new NonRecruiterUserFactory().CurrentUser);
                await service.ImportTranscriptCoursesAsync(transcriptCourseDto);
            }

            [TestMethod]
            public async Task RecruiterService_ImportTranscriptCoursesAsync_calls_repository_for_authorized_user()
            {
                await service.ImportTranscriptCoursesAsync(transcriptCourseDto);
                recruiterRepositoryMock.Verify(repo => repo.ImportTranscriptCoursesAsync(It.IsAny<Domain.Student.Entities.TranscriptCourse>()));
            }
        }

        [TestClass]
        public class RecruiterService_ImportCommunicationHistoryAsync_Tests : RecruiterServiceTests
        {
            private CommunicationHistory communicationHistoryDto;

            [TestInitialize]
            public void RecruiterService_ImportCommunicationHistoryAsync_Initialize()
            {
                base.RecruiterServiceTests_Initialize();
                communicationHistoryDto = new CommunicationHistory()
                {
                    CommunicationCode = "CODE1"
                };

                // Setup for Application DTO --> Entity conversion
                var applicationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Base.CommunicationHistory, Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Base.CommunicationHistory, Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory>()).Returns(applicationDtoAdapter);

                // Setup for repository call
                recruiterRepositoryMock.Setup(repo => repo.ImportCommunicationHistoryAsync(It.IsAny<Domain.Base.Entities.CommunicationHistory>())).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RecruiterService_ImportCommunicationHistoryAsync_throws_PermissionsException_for_unauthorized_user()
            {
                currentUserFactoryMock.Setup(uf => uf.CurrentUser).Returns(new NonRecruiterUserFactory().CurrentUser);
                await service.ImportCommunicationHistoryAsync(communicationHistoryDto);
            }

            [TestMethod]
            public async Task RecruiterService_ImportCommunicationHistoryAsync_calls_repository_for_authorized_user()
            {
                await service.ImportCommunicationHistoryAsync(communicationHistoryDto);
                recruiterRepositoryMock.Verify(repo => repo.ImportCommunicationHistoryAsync(It.IsAny<Domain.Base.Entities.CommunicationHistory>()));
            }
        }

        [TestClass]
        public class RecruiterService_RequestCommunicationHistoryAsync_Tests : RecruiterServiceTests
        {
            private CommunicationHistory communicationHistoryDto;

            [TestInitialize]
            public void RecruiterService_RequestCommunicationHistoryAsync_Initialize()
            {
                base.RecruiterServiceTests_Initialize();
                communicationHistoryDto = new CommunicationHistory()
                {
                    CommunicationCode = "CODE1"
                };

                // Setup for Application DTO --> Entity conversion
                var applicationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Base.CommunicationHistory, Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Base.CommunicationHistory, Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory>()).Returns(applicationDtoAdapter);

                // Setup for repository call
                recruiterRepositoryMock.Setup(repo => repo.RequestCommunicationHistoryAsync(It.IsAny<Domain.Base.Entities.CommunicationHistory>())).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RecruiterService_RequestCommunicationHistoryAsync_throws_PermissionsException_for_unauthorized_user()
            {
                currentUserFactoryMock.Setup(uf => uf.CurrentUser).Returns(new NonRecruiterUserFactory().CurrentUser);
                await service.RequestCommunicationHistoryAsync(communicationHistoryDto);
            }

            [TestMethod]
            public async Task RecruiterService_RequestCommunicationHistoryAsync_calls_repository_for_authorized_user()
            {
                await service.RequestCommunicationHistoryAsync(communicationHistoryDto);
                recruiterRepositoryMock.Verify(repo => repo.RequestCommunicationHistoryAsync(It.IsAny<Domain.Base.Entities.CommunicationHistory>()));
            }
        }

        [TestClass]
        public class RecruiterService_PostConnectionStatusAsync_Tests : RecruiterServiceTests
        {
            private ConnectionStatus connectionStatusDto;
            private Domain.Student.Entities.ConnectionStatus connectionStatusEntity;

            [TestInitialize]
            public void RecruiterService_PostConnectionStatusAsync_Initialize()
            {
                base.RecruiterServiceTests_Initialize();
                connectionStatusDto = new ConnectionStatus()
                {
                    Message = "Message"
                };
                connectionStatusEntity = new Domain.Student.Entities.ConnectionStatus()
                {
                    Message = "Message"
                };

                // Setup for ConnectionStatus DTO --> Entity conversion
                var connectionStatusDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.ConnectionStatus, Ellucian.Colleague.Domain.Student.Entities.ConnectionStatus>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.ConnectionStatus, Ellucian.Colleague.Domain.Student.Entities.ConnectionStatus>()).Returns(connectionStatusDtoAdapter);

                // Setup for ConnectionStatus Entity --> DTO conversion
                var connectionStatusEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ConnectionStatus, Ellucian.Colleague.Dtos.Student.ConnectionStatus>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ConnectionStatus, Ellucian.Colleague.Dtos.Student.ConnectionStatus>()).Returns(connectionStatusEntityAdapter);


                // Setup for repository call
                recruiterRepositoryMock.Setup(repo => repo.PostConnectionStatusAsync(It.IsAny<Domain.Student.Entities.ConnectionStatus>())).ReturnsAsync(connectionStatusEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RecruiterService_PostConnectionStatusAsync_throws_PermissionsException_for_unauthorized_user()
            {
                currentUserFactoryMock.Setup(uf => uf.CurrentUser).Returns(new NonRecruiterUserFactory().CurrentUser);
                await service.PostConnectionStatusAsync(connectionStatusDto);
            }

            [TestMethod]
            public async Task RecruiterService_PostConnectionStatusAsync_calls_repository_for_authorized_user()
            {
                var status = await service.PostConnectionStatusAsync(connectionStatusDto);
                Assert.AreEqual(status.Message, connectionStatusDto.Message);
            }
        }
    }
}
