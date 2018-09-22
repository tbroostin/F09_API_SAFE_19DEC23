/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class LoanRequestServiceTests
    {
        [TestClass]
        public class GetLoanRequestTests : FinancialAidServiceTestsSetup
        {
            private string inputId;
            private string inputStudentId;

            private TestLoanRequestRepository testLoanRequestRepository;
            private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;
            private TestStudentAwardYearRepository testStudentAwardYearRepository;

            private Domain.FinancialAid.Entities.LoanRequest inputLoanRequestEntity;

            private Dtos.FinancialAid.LoanRequest expectedLoanRequest;
            private Dtos.FinancialAid.LoanRequest actualLoanRequest;

            private LoanRequestService loanRequestService;

            private Mock<ILoanRequestRepository> loanRequestRepositoryMock;
            private Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;
            private Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IApplicantRepository> applicantRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                testLoanRequestRepository = new TestLoanRequestRepository();
                testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();

                inputId = testLoanRequestRepository.NewLoanRequestList.First().id;
                inputStudentId = testLoanRequestRepository.NewLoanRequestList.First().studentId;

                inputLoanRequestEntity = testLoanRequestRepository.GetLoanRequestAsync(inputId).Result;

                loanRequestRepositoryMock = new Mock<ILoanRequestRepository>();
                loanRequestRepositoryMock.Setup(r => r.GetLoanRequestAsync(It.IsAny<string>())).Returns<string>(async(id) => await testLoanRequestRepository.GetLoanRequestAsync(id));

                financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                applicantRepositoryMock = new Mock<IApplicantRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                var loanRequestDtoAdapter = new LoanRequestEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedLoanRequest = loanRequestDtoAdapter.MapToType(inputLoanRequestEntity);

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.LoanRequest, Dtos.FinancialAid.LoanRequest>()).Returns(loanRequestDtoAdapter);

                loanRequestService = new LoanRequestService(
                    adapterRegistryMock.Object,
                    loanRequestRepositoryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActionTest()
            {
                actualLoanRequest = await loanRequestService.GetLoanRequestAsync(inputId);
                Assert.AreEqual(expectedLoanRequest.Id, actualLoanRequest.Id);
                Assert.AreEqual(expectedLoanRequest.StudentId, actualLoanRequest.StudentId);
                Assert.AreEqual(expectedLoanRequest.AwardYear, actualLoanRequest.AwardYear);
                Assert.AreEqual(expectedLoanRequest.RequestDate, actualLoanRequest.RequestDate);
                Assert.AreEqual(expectedLoanRequest.TotalRequestAmount, actualLoanRequest.TotalRequestAmount);
                Assert.AreEqual(expectedLoanRequest.LoanRequestPeriods.Count, actualLoanRequest.LoanRequestPeriods.Count);
                Assert.AreEqual(expectedLoanRequest.Status, actualLoanRequest.Status);
                Assert.AreEqual(expectedLoanRequest.StatusDate, actualLoanRequest.StatusDate);
                Assert.AreEqual(expectedLoanRequest.AssignedToId, actualLoanRequest.AssignedToId);
                Assert.AreEqual(expectedLoanRequest.StudentComments, actualLoanRequest.StudentComments);

                for (var i = 0; i < expectedLoanRequest.LoanRequestPeriods.Count; i++)
                {
                    var expectedLoanRequestPeriod = expectedLoanRequest.LoanRequestPeriods[i];
                    Assert.IsTrue(actualLoanRequest.LoanRequestPeriods.Select(lrp => lrp.Code).Contains(expectedLoanRequestPeriod.Code));
                    Assert.AreEqual(expectedLoanRequestPeriod.LoanAmount, actualLoanRequest.LoanRequestPeriods.First(lrp => lrp.Code == expectedLoanRequestPeriod.Code).LoanAmount);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullInputIdThrowsExceptionTest()
            {
                await loanRequestService.GetLoanRequestAsync(null);
            }

            [TestMethod]
            public async Task NullObjectFromRepoThrowsExceptionTest()
            {
                var exceptionCaught = false;
                try
                {
                    inputLoanRequestEntity = null;
                    loanRequestRepositoryMock.Setup(l => l.GetLoanRequestAsync(inputId)).ReturnsAsync(inputLoanRequestEntity);
                    await loanRequestService.GetLoanRequestAsync(inputId);
                }
                catch (KeyNotFoundException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(string.Format("Unable to find requested LoanRequest resource {0}", inputId)));

            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CounselorHasPermissionTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                loanRequestService = new LoanRequestService(
                    adapterRegistryMock.Object,
                    loanRequestRepositoryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                Assert.IsNotNull(await loanRequestService.GetLoanRequestAsync(inputId));
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorHasNoPermissionTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                

                loanRequestService = new LoanRequestService(
                    adapterRegistryMock.Object,
                    loanRequestRepositoryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await loanRequestService.GetLoanRequestAsync(inputId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task UserIsProxy_CanAccessTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                loanRequestService = new LoanRequestService(
                    adapterRegistryMock.Object,
                    loanRequestRepositoryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualLoanRequest = await loanRequestService.GetLoanRequestAsync(inputId);
                Assert.IsNotNull(actualLoanRequest);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsProxyForDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();


                loanRequestService = new LoanRequestService(
                    adapterRegistryMock.Object,
                    loanRequestRepositoryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await loanRequestService.GetLoanRequestAsync(inputId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserDoesnotHavePermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                loanRequestService = new LoanRequestService(
                    adapterRegistryMock.Object,
                    loanRequestRepositoryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await loanRequestService.GetLoanRequestAsync("3");
            }

            [TestMethod]
            public async Task PermissionExceptionLogsErrorTest()
            {
                var currentUserId = inputLoanRequestEntity.StudentId;
                inputLoanRequestEntity = new Domain.FinancialAid.Entities.LoanRequest(
                    inputLoanRequestEntity.Id,
                    "foobar",
                    inputLoanRequestEntity.AwardYear,
                    inputLoanRequestEntity.RequestDate,
                    inputLoanRequestEntity.TotalRequestAmount,
                    inputLoanRequestEntity.AssignedToId,
                    inputLoanRequestEntity.Status,
                    inputLoanRequestEntity.StatusDate,
                    inputLoanRequestEntity.ModifierId);
                inputId = inputLoanRequestEntity.Id;

                loanRequestRepositoryMock.Setup(r => r.GetLoanRequestAsync(inputId)).ReturnsAsync(inputLoanRequestEntity);

                var exceptionCaught = false;
                var message = string.Format("{0} does not have permission to get LoanRequest resource {1}", currentUserId, inputId);
                try
                {
                    await loanRequestService.GetLoanRequestAsync(inputId);
                }
                catch (PermissionsException)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(message));
            }
        }

        [TestClass]
        public class CreateLoanRequestTests : FinancialAidServiceTestsSetup
        {
            //Used for Fakes when possible
            //protected static IDisposable Context { get; set; }

            private Dtos.FinancialAid.LoanRequest inputLoanRequestDto;
            private string inputStudentId;
            private Domain.Student.Entities.Student inputStudentEntity;
            private Domain.Student.Entities.Applicant inputApplicantEntity;

            private TestLoanRequestRepository testLoanRequestRepository;
            private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;
            private TestStudentAwardYearRepository testStudentAwardYearRepository;

            private Domain.FinancialAid.Entities.LoanRequest newLoanRequestEntity;
            private LoanRequestDtoToEntityAdapter loanRequestEntityAdapter;

            private Dtos.FinancialAid.LoanRequest expectedLoanRequest;
            private Dtos.FinancialAid.LoanRequest actualLoanRequest;

            private LoanRequestService loanRequestService;

            private Mock<ILoanRequestRepository> loanRequestRepositoryMock;
            private Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;
            private Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IApplicantRepository> applicantRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                //Used for Fakes when possible
                //Context = ShimsContext.Create();

                testLoanRequestRepository = new TestLoanRequestRepository();
                testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();

                testStudentAwardYearRepository.FaStudentData.PendingLoanRequestIds = new List<string>();

                inputStudentId = testLoanRequestRepository.NewLoanRequestList.First().studentId;

                inputLoanRequestDto = new Dtos.FinancialAid.LoanRequest()
                {
                    Id = "-2",
                    StudentId = inputStudentId,
                    AwardYear = "2014",
                    TotalRequestAmount = 5555,
                    LoanRequestPeriods = new List<LoanRequestPeriod>()
                    {
                        new LoanRequestPeriod(){
                            Code = "13/FA",
                            LoanAmount = 2500
                        },

                        new LoanRequestPeriod(){
                            Code = "14/SP",
                            LoanAmount = 3055
                        }                    
                    },
                    StudentComments = "These are my comments"
                };

                inputStudentEntity = new Domain.Student.Entities.Student(inputStudentId, "foobar", null, new List<string>(), new List<string>())
                {
                    FinancialAidCounselorId = "1111111"
                };
                inputApplicantEntity = null;



                expectedLoanRequest = new Dtos.FinancialAid.LoanRequest()
                {
                    Id = "-1",
                    StudentId = inputLoanRequestDto.StudentId,
                    AwardYear = inputLoanRequestDto.AwardYear,
                    TotalRequestAmount = inputLoanRequestDto.TotalRequestAmount,
                    LoanRequestPeriods = inputLoanRequestDto.LoanRequestPeriods,
                    StudentComments = inputLoanRequestDto.StudentComments,
                    RequestDate = DateTime.Today,
                    AssignedToId = inputStudentEntity.FinancialAidCounselorId,
                    Status = Dtos.FinancialAid.LoanRequestStatus.Pending,
                    StatusDate = DateTime.Today
                };

                loanRequestRepositoryMock = new Mock<ILoanRequestRepository>();
                loanRequestRepositoryMock.Setup(r => r.CreateLoanRequestAsync(It.IsAny<Domain.FinancialAid.Entities.LoanRequest>(), It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>()))
                    .Returns(
                    (Domain.FinancialAid.Entities.LoanRequest lr, Domain.FinancialAid.Entities.StudentAwardYear y) =>
                    {
                        newLoanRequestEntity = new Domain.FinancialAid.Entities.LoanRequest(lr.Id, inputStudentId, lr.AwardYear, lr.RequestDate, lr.TotalRequestAmount, lr.AssignedToId, lr.Status, lr.StatusDate, lr.ModifierId)
                        {
                            StudentComments = lr.StudentComments,
                            ModifierComments = lr.ModifierComments
                        };
                        foreach (var period in lr.LoanRequestPeriods)
                        {
                            newLoanRequestEntity.AddLoanPeriod(period.Code, period.LoanAmount);
                        }
                        return Task.FromResult(newLoanRequestEntity);
                    });

                financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync()).ReturnsAsync(testFinancialAidOfficeRepository.GetFinancialAidOffices());

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(inputStudentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(inputStudentId,
                    new CurrentOfficeService(testFinancialAidOfficeRepository.GetFinancialAidOffices())));

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepositoryMock.Setup(r => r.GetAsync(inputStudentId)).ReturnsAsync(inputStudentEntity);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(inputStudentId)).ReturnsAsync(inputApplicantEntity);

                loanRequestEntityAdapter = new LoanRequestDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.FinancialAid.LoanRequest, Domain.FinancialAid.Entities.LoanRequest>()).Returns(loanRequestEntityAdapter);

                var loanRequestDtoAdapter = new LoanRequestEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.LoanRequest, Dtos.FinancialAid.LoanRequest>()).Returns(loanRequestDtoAdapter);

                loanRequestService = new LoanRequestService(
                    adapterRegistryMock.Object,
                    loanRequestRepositoryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

            }

            [TestCleanup]
            public void Cleanup()
            {
                // Context.Dispose();
                // Context = null;
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualLoanRequest = await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
                Assert.AreEqual(expectedLoanRequest.Id, actualLoanRequest.Id);
                Assert.AreEqual(expectedLoanRequest.StudentId, actualLoanRequest.StudentId);
                Assert.AreEqual(expectedLoanRequest.AwardYear, actualLoanRequest.AwardYear);
                Assert.AreEqual(expectedLoanRequest.RequestDate, actualLoanRequest.RequestDate);
                Assert.AreEqual(expectedLoanRequest.TotalRequestAmount, actualLoanRequest.TotalRequestAmount);
                Assert.AreEqual(expectedLoanRequest.LoanRequestPeriods.Count, actualLoanRequest.LoanRequestPeriods.Count);
                Assert.AreEqual(expectedLoanRequest.Status, actualLoanRequest.Status);
                Assert.AreEqual(expectedLoanRequest.StatusDate, actualLoanRequest.StatusDate);
                Assert.AreEqual(expectedLoanRequest.AssignedToId, actualLoanRequest.AssignedToId);
                Assert.AreEqual(expectedLoanRequest.StudentComments, actualLoanRequest.StudentComments);

                for (var i = 0; i < expectedLoanRequest.LoanRequestPeriods.Count; i++)
                {
                    var expectedLoanRequestPeriod = expectedLoanRequest.LoanRequestPeriods[i];
                    Assert.IsTrue(actualLoanRequest.LoanRequestPeriods.Select(lrp => lrp.Code).Contains(expectedLoanRequestPeriod.Code));
                    Assert.AreEqual(expectedLoanRequestPeriod.LoanAmount, actualLoanRequest.LoanRequestPeriods.First(lrp => lrp.Code == expectedLoanRequestPeriod.Code).LoanAmount);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullInputArgumentThrowsExceptionTest()
            {
                await loanRequestService.CreateLoanRequestAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentIdIsRequiredTest()
            {
                inputLoanRequestDto.StudentId = "";
                await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AwardYearIsRequiredTest()
            {
                inputLoanRequestDto.AwardYear = "";
                await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task TotalRequestAmountIsRequiredTest()
            {
                inputLoanRequestDto.TotalRequestAmount = 0;
                await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task LoanRequestPeriodListIsRequiredTest()
            {
                inputLoanRequestDto.LoanRequestPeriods = null;
                await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task LoanRequestPeriodListIsRequiredNotBeEmptyTest()
            {
                inputLoanRequestDto.LoanRequestPeriods = new List<LoanRequestPeriod>();
                await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
            }

            /// <summary>
            /// User is not self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorCannotCreateLoanRequestTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                loanRequestService = new LoanRequestService(
                    adapterRegistryMock.Object,
                    loanRequestRepositoryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
            }

            [TestMethod]
            public async Task CatchPermissionsExceptionLogErrorTest()
            {
                var currentUserId = inputLoanRequestDto.StudentId;
                inputLoanRequestDto.StudentId = "foobar";

                var exceptionCaught = false;
                var message = string.Format("{0} does not have permission to create LoanRequest resource for {1}", currentUserId, inputLoanRequestDto.StudentId);
                try
                {
                    await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
                }
                catch (PermissionsException)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(message));
            }

            [TestMethod]
            public async Task StudentIsStudent_AssignToCounselorTest()
            {
                actualLoanRequest = await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
                var expectedAssignedTo = inputStudentEntity.FinancialAidCounselorId;
                Assert.AreEqual(expectedAssignedTo, actualLoanRequest.AssignedToId);
            }

            [TestMethod]
            public async Task StudentRepositoryReturnsNull_StudentIsApplicantTest()
            {
                var expectedAssignedToId = "2222222";
                inputApplicantEntity = new Domain.Student.Entities.Applicant(inputStudentId, "foobar")
                {
                    FinancialAidCounselorId = expectedAssignedToId
                };
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(inputStudentId)).ReturnsAsync(inputApplicantEntity);

                inputStudentEntity = null;
                studentRepositoryMock.Setup(r => r.GetAsync(inputStudentId)).ReturnsAsync(inputStudentEntity);

                actualLoanRequest = await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
                Assert.AreEqual(expectedAssignedToId, actualLoanRequest.AssignedToId);
            }

            [TestMethod]
            public async Task StudentRepositoryThrowsException_StudentIsApplicantTest()
            {
                var expectedAssignedToId = "2222222";
                inputApplicantEntity = new Domain.Student.Entities.Applicant(inputStudentId, "foobar")
                {
                    FinancialAidCounselorId = expectedAssignedToId
                };

                studentRepositoryMock.Setup(r => r.GetAsync(inputStudentId)).Throws(new Exception("e"));
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(inputStudentId)).ReturnsAsync(inputApplicantEntity);

                actualLoanRequest = await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
                Assert.AreEqual(expectedAssignedToId, actualLoanRequest.AssignedToId);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentAndApplicantRepositoriesReturnNullTest()
            {
                inputStudentEntity = null;
                inputApplicantEntity = null;
                studentRepositoryMock.Setup(r => r.GetAsync(inputStudentId)).ReturnsAsync(inputStudentEntity);
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(inputStudentId)).ReturnsAsync(inputApplicantEntity);

                await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentAndApplicantRepositoriesThrowExceptionTest()
            {
                inputStudentEntity = null;
                inputApplicantEntity = null;
                studentRepositoryMock.Setup(r => r.GetAsync(inputStudentId)).Throws(new Exception("e"));
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(inputStudentId)).Throws(new Exception("e"));

                await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task ApplicantRepositoryThrowsExceptionTest()
            {

                inputStudentEntity = null;
                studentRepositoryMock.Setup(r => r.GetAsync(inputStudentId)).ReturnsAsync(inputStudentEntity);
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(inputStudentId)).Throws(new Exception("e"));

                await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
            }

            [TestMethod]
            public async Task LoanRequestRepositoryReturnsNull_ThrowExceptionLogsErrorTest()
            {
                newLoanRequestEntity = null;
                loanRequestRepositoryMock.Setup(r => r.CreateLoanRequestAsync(It.IsAny<Domain.FinancialAid.Entities.LoanRequest>(), It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>()))
                    .ReturnsAsync(newLoanRequestEntity);

                var exceptionCaught = false;
                try
                {
                    await loanRequestService.CreateLoanRequestAsync(inputLoanRequestDto);
                }
                catch (Exception)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(string.Format("Unknown resource id for new LoanRequest resource for student id {0} and awardYear {1}", inputLoanRequestDto.StudentId, inputLoanRequestDto.AwardYear)));
            }
        }
    }
}
