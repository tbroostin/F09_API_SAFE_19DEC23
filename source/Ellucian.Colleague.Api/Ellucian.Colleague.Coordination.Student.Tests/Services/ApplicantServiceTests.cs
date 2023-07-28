// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class ApplicantServiceTests
    {

        public class StudentUserFactory : ICurrentUserFactory
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
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                }
            }
        }

        public class CounselorUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "321",
                        Name = "Joe",
                        PersonId = "0718745",
                        SecurityToken = "9USSD9d9sdD.DS9983",
                        SessionTimeout = 30,
                        UserName = "JoeCounselor",
                        Roles = new List<string>() { "FINANCIAL AID COUNSELOR" },
                        SessionFixationId = "xyz987"
                    });
                }
            }
        }

        public class StudentUserFactoryWithProxy : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Charlie",
                        PersonId = "0006574",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0000001"
                        }
                    });
                }
            }
        }

        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory currentUserFactory;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public void BaseInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            currentUserFactory = new ApplicantServiceTests.StudentUserFactory();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
        }

        public void BaseCleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            roleRepositoryMock = null;
            currentUserFactory = null;
        }

        [TestClass]
        public class GetApplicantTests : ApplicantServiceTests
        {
            private string applicantId;

            private TestApplicantRepository testApplicantRepository;            
            private Domain.Student.Entities.Applicant inputApplicantEntity;
            private AutoMapperAdapter<Domain.Student.Entities.Applicant, Dtos.Student.Applicant> applicantDtoAdapter;

            private Dtos.Student.Applicant expectedApplicant;
            private Dtos.Student.Applicant actualApplicant;
            private List<Role> roles;

            private Mock<IApplicantRepository> applicantRepositoryMock;
            private Mock<IStudentRepository> studentRepoMock;
            private Mock<IPersonBaseRepository> personRepoMock;


            private ApplicantService applicantService;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                applicantId = currentUserFactory.CurrentUser.PersonId;

                testApplicantRepository = new TestApplicantRepository();

                testApplicantRepository.personData.Add(new TestApplicantRepository.Person()
                    {
                        Id = applicantId,
                        LastName = "Foobar"
                    });

                inputApplicantEntity = await testApplicantRepository.GetApplicantAsync(applicantId);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                applicantRepositoryMock.Setup(ar => ar.GetApplicantAsync(applicantId)).Returns(Task.FromResult(inputApplicantEntity));

                roles = new List<Role>()
                {
                    new Role(1, "FINANCIAL AID COUNSELOR")                  
                };
                roles[0].AddPermission(new Permission("VIEW.FINANCIAL.AID.INFORMATION"));

                roleRepositoryMock.Setup(rr => rr.Roles).Returns(roles);

                applicantDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Applicant, Dtos.Student.Applicant>(adapterRegistryMock.Object, loggerMock.Object);
                expectedApplicant = applicantDtoAdapter.MapToType(inputApplicantEntity);

                adapterRegistryMock.Setup<ITypeAdapter<Domain.Student.Entities.Applicant, Dtos.Student.Applicant>>(
                    a => a.GetAdapter<Domain.Student.Entities.Applicant, Dtos.Student.Applicant>()
                    ).Returns(applicantDtoAdapter);

                studentRepoMock = new Mock<IStudentRepository>();
                personRepoMock = new Mock<IPersonBaseRepository>();

                BuildApplicantService();

                actualApplicant =await applicantService.GetApplicantAsync(applicantId);
            }

            private void BuildApplicantService()
            {
                applicantService = new ApplicantService(adapterRegistryMock.Object,
                    applicantRepositoryMock.Object,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object,
                    studentRepoMock.Object, 
                    personRepoMock.Object,
                    baseConfigurationRepository);
            }
            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                applicantId = null;
                testApplicantRepository = null;
                inputApplicantEntity = null;
                applicantDtoAdapter = null;
                expectedApplicant = null;
                actualApplicant = null;
                applicantRepositoryMock = null;
                applicantService = null;
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedApplicant);
                Assert.IsNotNull(actualApplicant);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ApplicantIdRequiredTest()
            {
                await applicantService.GetApplicantAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsNotCounselor_ApplicantIdMustBeCurrentUserTest()
            {
                applicantId = "foobar";
                await applicantService.GetApplicantAsync(applicantId);
            }

            [TestMethod]
            public async Task CounselorCanAccessDataTest()
            {
                currentUserFactory = new CounselorUserFactory();
                BuildApplicantService();

                Assert.AreNotEqual(currentUserFactory.CurrentUser.PersonId, applicantId);

                actualApplicant = await applicantService.GetApplicantAsync(applicantId);
            }

            [TestMethod]
            public async Task PermissionsExceptionLogsErrorTest()
            {
                applicantId = "foobar";
                var exceptionCaught = false;

                try
                {
                   await applicantService.GetApplicantAsync(applicantId);
                }
                catch (PermissionsException)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task PersonProxyCanAccessDataTest()
            {
                bool exceptionCaught = false;
                currentUserFactory = new StudentUserFactoryWithProxy();
                BuildApplicantService();
                
                try
                {
                    await applicantService.GetApplicantAsync(applicantId);
                }
                catch { exceptionCaught = true; }

                Assert.IsFalse(exceptionCaught);
                Assert.AreNotEqual(currentUserFactory.CurrentUser.PersonId, applicantId);
            }

        }
    }
}
