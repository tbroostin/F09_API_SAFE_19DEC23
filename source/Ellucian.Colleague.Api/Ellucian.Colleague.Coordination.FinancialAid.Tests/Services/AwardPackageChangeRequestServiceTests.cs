/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class AwardPackageChangeRequestServiceTests : FinancialAidServiceTestsSetup
    {
        public string studentId;
        public string faCounselorId;

        public TestStudentAwardRepository studentAwardRepository;
        public TestFinancialAidReferenceDataRepository financialAidReferenceDataRepository;        
        public TestStudentAwardYearRepository studentAwardYearRepository;
        public TestFinancialAidOfficeRepository financialAidOfficeRepository;
        public TestAwardPackageChangeRequestRepository awardPackageChangeRequestRepository;
        public TestCommunicationRepository communicationRepository;

        public Mock<IStudentAwardRepository> studentAwardRepositoryMock;
        public Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
        public Mock<IStudentRepository> studentRepositoryMock;
        public Mock<IApplicantRepository> applicantRepositoryMock;
        public Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
        public Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;
        public Mock<IAwardPackageChangeRequestRepository> awardPackageChangeRequestRepositoryMock;
        public Mock<ICommunicationRepository> communicationRepositoryMock;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public ITypeAdapter<Domain.FinancialAid.Entities.AwardPackageChangeRequest, AwardPackageChangeRequest> awardPackageChangeRequestDtoAdapter;
        public ITypeAdapter<AwardPackageChangeRequest, Domain.FinancialAid.Entities.AwardPackageChangeRequest> awardPackageChangeRequestEntityAdapter;

        public FunctionEqualityComparer<AwardPackageChangeRequest> awardPackageChangeRequestDtoComparer;

        public AwardPackageChangeRequestService actualService;        

        public void AwardPackageChangeRequestServiceTestsInitialize()
        {
            BaseInitialize();

            studentAwardRepository = new TestStudentAwardRepository();
            financialAidReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            financialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            awardPackageChangeRequestRepository = new TestAwardPackageChangeRequestRepository();
            communicationRepository = new TestCommunicationRepository();

            studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
            financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
            studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
            financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            awardPackageChangeRequestRepositoryMock = new Mock<IAwardPackageChangeRequestRepository>();
            communicationRepositoryMock = new Mock<ICommunicationRepository>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            studentId = currentUserFactory.CurrentUser.PersonId;
            faCounselorId = new CurrentUserSetup.CounselorUserFactory().CurrentUser.PersonId;

            awardPackageChangeRequestDtoComparer = new FunctionEqualityComparer<AwardPackageChangeRequest>(
                (cr1, cr2) => cr1.StudentId == cr2.StudentId && cr1.AwardId == cr2.AwardId && cr1.AwardYearId == cr2.AwardYearId,
                (cr) => cr.StudentId.GetHashCode() ^ cr.AwardId.GetHashCode() ^ cr.AwardYearId.GetHashCode());

            awardPackageChangeRequestDtoAdapter = new AwardPackageChangeRequestEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            awardPackageChangeRequestEntityAdapter = new AwardPackageChangeRequestDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            awardPackageChangeRequestRepositoryMock.Setup(r => r.GetAwardPackageChangeRequestsAsync(It.IsAny<string>()))
                .Returns<string>((id) => awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(id));

            financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
                .ReturnsAsync(financialAidOfficeRepository.GetFinancialAidOffices());
                        
            financialAidReferenceDataRepositoryMock.Setup(r => r.Awards)
                .Returns(() => financialAidReferenceDataRepository.Awards);

            financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses)
                .Returns(() => financialAidReferenceDataRepository.AwardStatuses);
            
            awardPackageChangeRequestRepositoryMock.Setup(r => r.CreateAwardPackageChangeRequestAsync(It.IsAny<Domain.FinancialAid.Entities.AwardPackageChangeRequest>(), It.IsAny<Domain.FinancialAid.Entities.StudentAward>()))
                .Returns<Domain.FinancialAid.Entities.AwardPackageChangeRequest, Domain.FinancialAid.Entities.StudentAward>(
                    (changeRequest, studentAward) => awardPackageChangeRequestRepository.CreateAwardPackageChangeRequestAsync(changeRequest, studentAward));

            communicationRepositoryMock.Setup(r => r.SubmitCommunication(It.IsAny<Domain.Base.Entities.Communication>()))
                .Returns<Domain.Base.Entities.Communication>((comm) => communicationRepository.SubmitCommunication(comm));

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.FinancialAid.Entities.AwardPackageChangeRequest, AwardPackageChangeRequest>())
                .Returns(awardPackageChangeRequestDtoAdapter);

            adapterRegistryMock.Setup(r => r.GetAdapter<AwardPackageChangeRequest, Domain.FinancialAid.Entities.AwardPackageChangeRequest>())
                .Returns(awardPackageChangeRequestEntityAdapter);
            BuildAwardPackageChangeRequestService();
        }

        private void BuildAwardPackageChangeRequestService()
        {
            actualService = new AwardPackageChangeRequestService(
                    adapterRegistryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    awardPackageChangeRequestRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    communicationRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
        }

        [TestClass]
        public class GetStudentAwardPackageChangeRequestTests : AwardPackageChangeRequestServiceTests
        {
            public List<AwardPackageChangeRequest> expectedChangeRequests
            {
                get
                {
                    return awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result
                        .Select(cr => awardPackageChangeRequestDtoAdapter.MapToType(cr)).ToList();
                }
            }

            public IEnumerable<AwardPackageChangeRequest> actualChangeRequests;
            

            [TestInitialize]
            public void Initialize()
            {
                AwardPackageChangeRequestServiceTestsInitialize();
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualChangeRequests = await actualService.GetAwardPackageChangeRequestsAsync(studentId);
                Assert.IsNotNull(actualChangeRequests);
                Assert.IsTrue(actualChangeRequests.Count() > 0);
                CollectionAssert.AreEqual(expectedChangeRequests, actualChangeRequests.ToList(), awardPackageChangeRequestDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = string.Empty;
                await actualService.GetAwardPackageChangeRequestsAsync(studentId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsCounselorTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildAwardPackageChangeRequestService();

                actualChangeRequests = await actualService.GetAwardPackageChangeRequestsAsync(studentId);

                CollectionAssert.AreEqual(expectedChangeRequests, actualChangeRequests.ToList(), awardPackageChangeRequestDtoComparer);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsCounselorWithNoPermissionsTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                
                BuildAwardPackageChangeRequestService();

                await actualService.GetAwardPackageChangeRequestsAsync(studentId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxyTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                
                BuildAwardPackageChangeRequestService();

                actualChangeRequests = await actualService.GetAwardPackageChangeRequestsAsync(studentId);

                CollectionAssert.AreEqual(expectedChangeRequests, actualChangeRequests.ToList(), awardPackageChangeRequestDtoComparer);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyDifferentPersonTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                BuildAwardPackageChangeRequestService();

                await actualService.GetAwardPackageChangeRequestsAsync(studentId);
            }

            /// <summary>
            /// User is not self, nor admin, nor proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserNotRequestingSelfAndNotCounselorNorProxyTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));

                studentId = "foobar";
                try
                {
                    await actualService.GetAwardPackageChangeRequestsAsync(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to get awardPackageChangeRequest resources for student {1}", currentUserFactory.CurrentUser.PersonId, studentId)));
                    throw;
                }
            }

            [TestMethod]
            public async Task NullChangeRequestsReturnsEmptyListTest()
            {
                awardPackageChangeRequestRepository.DeclinedStatusChangeRequestData = new List<TestAwardPackageChangeRequestRepository.DeclinedStatusChangeRequestRecord>();
                awardPackageChangeRequestRepository.LoanAmountChangeRequestData = new List<TestAwardPackageChangeRequestRepository.LoanAmountChangeRequestRecord>();
                actualChangeRequests = await actualService.GetAwardPackageChangeRequestsAsync(studentId);

                Assert.AreEqual(0, actualChangeRequests.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no AwardPackageChangeRequests", studentId)));
            }
        }

        [TestClass]
        public class GetSingleAwardPackageChangeRequestTests : AwardPackageChangeRequestServiceTests
        {
            public string inputChangeRequestId;

            public AwardPackageChangeRequest expectedChangeRequest
            {
                get
                {
                    return awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result
                        .Select(cr => awardPackageChangeRequestDtoAdapter.MapToType(cr))
                        .FirstOrDefault(cr => cr.Id == inputChangeRequestId);
                }
            }

            public AwardPackageChangeRequest actualChangeRequest;
            
            [TestInitialize]
            public void Initialize()
            {
                AwardPackageChangeRequestServiceTestsInitialize();
                inputChangeRequestId = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result.First().Id;
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActual()
            {
                actualChangeRequest = await actualService.GetAwardPackageChangeRequestAsync(studentId, inputChangeRequestId);
                Assert.IsTrue(awardPackageChangeRequestDtoComparer.Equals(expectedChangeRequest, actualChangeRequest));
            }

            [TestMethod]
            public async Task UserIsCounselor_CanAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildAwardPackageChangeRequestService();

                actualChangeRequest = await actualService.GetAwardPackageChangeRequestAsync(studentId, inputChangeRequestId);
                Assert.IsTrue(awardPackageChangeRequestDtoComparer.Equals(expectedChangeRequest, actualChangeRequest));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsCounselorWithNoPermissions_CannotAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                
                BuildAwardPackageChangeRequestService();

                await actualService.GetAwardPackageChangeRequestAsync(studentId, inputChangeRequestId);
            }

            [TestMethod]
            public async Task UserIsProxy_CanAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                
                BuildAwardPackageChangeRequestService();

                actualChangeRequest = await actualService.GetAwardPackageChangeRequestAsync(studentId, inputChangeRequestId);
                Assert.IsTrue(awardPackageChangeRequestDtoComparer.Equals(expectedChangeRequest, actualChangeRequest));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsProxyDifferentPerson_CannotAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                BuildAwardPackageChangeRequestService();

                await actualService.GetAwardPackageChangeRequestAsync(studentId, inputChangeRequestId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ChangeRequestIdRequiredTest()
            {
                inputChangeRequestId = "";
                await actualService.GetAwardPackageChangeRequestAsync(studentId, inputChangeRequestId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoChangeRequestsExistForStudentTest()
            {
                awardPackageChangeRequestRepository.DeclinedStatusChangeRequestData = new List<TestAwardPackageChangeRequestRepository.DeclinedStatusChangeRequestRecord>();
                awardPackageChangeRequestRepository.LoanAmountChangeRequestData = new List<TestAwardPackageChangeRequestRepository.LoanAmountChangeRequestRecord>();

                try
                {
                    await actualService.GetAwardPackageChangeRequestAsync(studentId, inputChangeRequestId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no AwardPackageChangeRequests", studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InputChangeRequestIdDoesNotExistForStudentTest()
            {
                inputChangeRequestId = "foobar";

                try
                {
                    await actualService.GetAwardPackageChangeRequestAsync(studentId, inputChangeRequestId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("AwardPackageChangeRequest resource {0} does not exist for Student {1}", inputChangeRequestId, studentId)));
                    throw;
                }
            }

        }

        [TestClass]
        public class CreateAwardPackageChangeRequestTests : AwardPackageChangeRequestServiceTests
        {
            public AwardPackageChangeRequest inputChangeRequestDto;
            public string awardYearCode;
            public string awardId;
            public Domain.FinancialAid.Entities.StudentAwardYear studentAwardYearEntity;
            public Domain.FinancialAid.Entities.StudentAward studentAwardEntity;

            public AwardPackageChangeRequest expectedChangeRequest;
            public AwardPackageChangeRequest actualChangeRequest;

            public List<Domain.Base.Entities.Communication> submittedCommunications;
            private CurrentOfficeService currentOfficeService;            
            
            private IEnumerable<Domain.FinancialAid.Entities.Award> allAwards
            {
                get
                {
                    return financialAidReferenceDataRepository.Awards;
                }
            }
            private IEnumerable<Domain.FinancialAid.Entities.AwardStatus> allStatuses
            {
                get
                {
                    return financialAidReferenceDataRepository.AwardStatuses;
                }
            }

            public IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> financialAidOfficeEntities
            { get { return financialAidOfficeRepository.GetFinancialAidOfficesAsync().Result; } }
            public IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYearEntities
            { get { return studentAwardYearRepository.GetStudentAwardYearsAsync(studentId, new CurrentOfficeService(financialAidOfficeEntities)).Result; } }
            public IEnumerable<Domain.FinancialAid.Entities.StudentAward> studentAwardEntities
            { get { return studentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYearEntities, allAwards, allStatuses).Result; } }

            [TestInitialize]
            public async void Initialize()
            {
                AwardPackageChangeRequestServiceTestsInitialize();                
                financialAidOfficeRepository.officeParameterRecordData.ForEach(o => { o.ReviewLoanChanges = "Y"; o.ReviewDeclinedAwards = "Y"; });
                currentOfficeService = new CurrentOfficeService(financialAidOfficeEntities);

                studentAwardYearEntity = studentAwardYearEntities.First();
                studentAwardEntity = studentAwardEntities.First(a => a.StudentAwardYear.Code == studentAwardYearEntity.Code);

                awardYearCode = studentAwardYearEntity.Code;
                awardId = studentAwardEntity.Award.Code;

                inputChangeRequestDto = new AwardPackageChangeRequest()
                {
                    StudentId = studentId,
                    AwardYearId = awardYearCode,
                    AwardId = awardId,
                    AwardPeriodChangeRequests = studentAwardEntity.StudentAwardPeriods.Select(period =>
                        new AwardPeriodChangeRequest()
                        {
                            AwardPeriodId = period.AwardPeriodId,
                            NewAmount = null,
                            NewAwardStatusId = (period.AwardStatus.Code == "D") ? "R" : "D"
                        })
                };

                submittedCommunications = new List<Domain.Base.Entities.Communication>();

                communicationRepositoryMock.Setup(r => r.SubmitCommunication(It.IsAny<Domain.Base.Entities.Communication>()))
                    .Callback<Domain.Base.Entities.Communication>((comm) => submittedCommunications.Add(comm));

                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
                    .ReturnsAsync(financialAidOfficeEntities);
                
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurrentOfficeService>()))
                    .ReturnsAsync(studentAwardYearEntity);

                studentAwardRepositoryMock.Setup(r => r.GetStudentAwardAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>(), 
                    It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                    .ReturnsAsync(studentAwardEntity);

                expectedChangeRequest = new AwardPackageChangeRequest()
                {
                    AssignedToCounselorId = inputChangeRequestDto.AssignedToCounselorId,
                    AwardId = inputChangeRequestDto.AwardId,
                    AwardPeriodChangeRequests = inputChangeRequestDto.AwardPeriodChangeRequests,
                    AwardYearId = inputChangeRequestDto.AwardYearId,
                    CreateDateTime = inputChangeRequestDto.CreateDateTime,
                    Id =inputChangeRequestDto.Id,
                    StudentId = inputChangeRequestDto.StudentId
                };
                
                actualChangeRequest = await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto); 
            }
            
            /// <summary>
            /// User os self
            /// </summary>
            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                Assert.IsTrue(awardPackageChangeRequestDtoComparer.Equals(expectedChangeRequest, actualChangeRequest));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await actualService.CreateAwardPackageChangeRequestAsync("", inputChangeRequestDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InputChangeRequestRequiredTest()
            {
                await actualService.CreateAwardPackageChangeRequestAsync(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsCounselorCannotCreateRequestTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildAwardPackageChangeRequestService();
                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to create awardPackageChangeRequest resources for {1}", currentUserFactory.CurrentUser.PersonId, studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserNotRequestingSelfTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));

                studentId = "foobar";
                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to create awardPackageChangeRequest resources for {1}", currentUserFactory.CurrentUser.PersonId, studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentIdOfInputChangeRequestRequired()
            {
                inputChangeRequestDto.StudentId = "";
                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("StudentId attribute of newAwardPackageChangeRequest is required")));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AwardYearIdOfInputChangeRequestRequired()
            {
                inputChangeRequestDto.AwardYearId = "";
                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("AwardYearId attribute of newAwardPackageChangeRequest is required")));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AwardIdOfInputChangeRequestRequired()
            {
                inputChangeRequestDto.AwardId = "";
                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("AwardId attribute of newAwardPackageChangeRequest is required")));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentIdOfInputChangeRequestMustMatchInputStudentIdTest()
            {
                inputChangeRequestDto.StudentId = "foobar";
                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("studentId must match StudentId attribute of newAwardPackageChangeRequest")));
                    throw;
                }
            }
            
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentHasNoMatchingAwardYearTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurrentOfficeService>()))
                    .ReturnsAsync(() => null);
                BuildAwardPackageChangeRequestService();

                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Award Year {0} does not exist for studentId {1}", awardYearCode, studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task SpecifiedAwardYearDoesNotExistTest()
            {
                inputChangeRequestDto.AwardYearId = "foobar";
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurrentOfficeService>()))
                    .ReturnsAsync(studentAwardYearEntities.First(y => y.Code == inputChangeRequestDto.AwardYearId));
                BuildAwardPackageChangeRequestService();

                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Award Year {0} does not exist for studentId {1}", inputChangeRequestDto.AwardYearId, studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoAwardsExistTest()
            {
                financialAidReferenceDataRepository.awardRecordData = new List<TestFinancialAidReferenceDataRepository.AwardRecord>();

                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No awards exist")));
                    throw;
                }
            }            

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoAwardStatusesExistTest()
            {
                financialAidReferenceDataRepository.AwardActionData = new string[,] { };

                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No award statuses exist")));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentHasNoAwardsForYearTest()
            {
                studentAwardRepository.ClearAllAwardData();
                studentAwardRepositoryMock.Setup(r => r.GetStudentAwardAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>(),
                   It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                   .Returns(studentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYearEntities.First(y => y.Code == awardYearCode), awardId, allAwards, allStatuses));
                BuildAwardPackageChangeRequestService();

                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no awards in year {1}", studentId, inputChangeRequestDto.AwardYearId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task SpecifiedAwardIdDoesNotExistTest()
            {
                inputChangeRequestDto.AwardId = "foobar";
                studentAwardRepositoryMock.Setup(r => r.GetStudentAwardAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>(),
                   It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                   .Returns(studentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYearEntities.First(y => y.Code == awardYearCode), inputChangeRequestDto.AwardId, allAwards, allStatuses));
                BuildAwardPackageChangeRequestService();
                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Award {0} does not exist for Student {1} in year {2}", inputChangeRequestDto.AwardId, studentId, inputChangeRequestDto.AwardYearId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoMatchingAwardTest()
            {
                studentAwardRepositoryMock.Setup(r => r.GetStudentAwardAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>(),
                   It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                   .ReturnsAsync(() => null);
                BuildAwardPackageChangeRequestService();
                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Award {0} does not exist for Student {1} in year {2}", inputChangeRequestDto.AwardId, studentId, inputChangeRequestDto.AwardYearId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task NullEntityReturnedByRepositoryTest()
            {
                Domain.FinancialAid.Entities.AwardPackageChangeRequest nullRequest = null;
                awardPackageChangeRequestRepositoryMock.Setup(r => r.CreateAwardPackageChangeRequestAsync(It.IsAny<Domain.FinancialAid.Entities.AwardPackageChangeRequest>(), It.IsAny<Domain.FinancialAid.Entities.StudentAward>()))
                    .ReturnsAsync(nullRequest);

                try
                {
                    await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error("Unable to create award package change request resource"));
                    throw;
                }
            }

            [TestMethod]
            public async Task NoCommunicationsSubmittedTest()
            {
                financialAidOfficeRepository.officeParameterRecordData.ForEach(p =>
                    {
                        p.LoanChangeCommunicationCode = string.Empty;
                        p.RejectedAwardCommunicationCode = string.Empty;
                        p.ReviewLoanChanges = "Y";
                    });
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurrentOfficeService>()))
                    .ReturnsAsync(studentAwardYearEntities.First(y => y.Code == awardYearCode));

                studentAwardRepositoryMock.Setup(r => r.GetStudentAwardAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>(),
                    It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                    .Returns(studentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYearEntities.First(y => y.Code == awardYearCode), awardId, allAwards, allStatuses));

                BuildAwardPackageChangeRequestService();
                submittedCommunications = new List<Domain.Base.Entities.Communication>();
                await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);

                Assert.AreEqual(0, submittedCommunications.Count());
            }

            [TestMethod]
            public async Task SingleCommunicationSubmittedTest()
            {
                var loanChangeCode = "foo";
                financialAidOfficeRepository.officeParameterRecordData.ForEach(p =>
                {
                    p.LoanChangeCommunicationCode = loanChangeCode;
                    p.LoanChangeCommunicationStatus = "R";
                    p.RejectedAwardCommunicationCode = string.Empty;
                    p.ReviewLoanChanges = "Y";
                });

                awardId = studentAwardRepository.loanData.First(l => l.awardYear == awardYearCode).awardId;
                
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurrentOfficeService>()))
                    .ReturnsAsync(studentAwardYearEntities.First(y => y.Code == awardYearCode));

                studentAwardEntity = studentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYearEntities.First(y => y.Code == awardYearCode), awardId, allAwards, allStatuses).Result;
                studentAwardRepositoryMock.Setup(r => r.GetStudentAwardAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>(),
                    It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                    .ReturnsAsync(studentAwardEntity);

                BuildAwardPackageChangeRequestService();
                submittedCommunications = new List<Domain.Base.Entities.Communication>();

                inputChangeRequestDto = new AwardPackageChangeRequest()
                {
                    StudentId = studentId,
                    AwardYearId = studentAwardEntity.StudentAwardYear.Code,
                    AwardId = studentAwardEntity.Award.Code,
                    AwardPeriodChangeRequests = studentAwardEntity.StudentAwardPeriods.Select(period =>
                        new AwardPeriodChangeRequest()
                        {
                            AwardPeriodId = period.AwardPeriodId,
                            NewAmount = 500,
                            NewAwardStatusId = string.Empty
                        })
                };
                await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);

                Assert.AreEqual(1, submittedCommunications.Count());
                Assert.AreEqual(loanChangeCode, submittedCommunications[0].Code);
            }

            [TestMethod]
            public async Task ErrorSubmittingCommunicationTest()
            {
                var exception = new ApplicationException("ae");
                communicationRepositoryMock.Setup(r => r.SubmitCommunication(It.IsAny<Domain.Base.Entities.Communication>()))
                    .Throws(exception);

                var loanChangeCode = "foo";
                financialAidOfficeRepository.officeParameterRecordData.ForEach(p =>
                {
                    p.LoanChangeCommunicationCode = loanChangeCode;
                    p.LoanChangeCommunicationStatus = "R";
                    p.RejectedAwardCommunicationCode = string.Empty;
                    p.ReviewLoanChanges = "Y";
                });

                inputChangeRequestDto = new AwardPackageChangeRequest()
                {
                    StudentId = studentId,
                    AwardYearId = awardYearCode,
                    AwardId = awardId,
                    AwardPeriodChangeRequests = studentAwardEntity.StudentAwardPeriods.Select(period =>
                        new AwardPeriodChangeRequest()
                        {
                            AwardPeriodId = period.AwardPeriodId,
                            NewAwardStatusId = "D"
                        })
                };
                await actualService.CreateAwardPackageChangeRequestAsync(studentId, inputChangeRequestDto);

                loggerMock.Verify(l => l.Debug(It.Is<Exception>(e => e.GetType() == exception.GetType()), It.IsAny<string>()));
            }

        }
    }
}
