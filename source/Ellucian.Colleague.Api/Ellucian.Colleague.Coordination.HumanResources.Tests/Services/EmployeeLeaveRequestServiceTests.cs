// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class EmployeeLeaveRequestServiceTests : HumanResourcesServiceTestsSetup
    {
        #region Setup
        public Mock<ISupervisorsRepository> supervisorsRepositoryMock;
        public Mock<IPersonBaseRepository> personBaseRepositoryMock;
        public Mock<IEmployeeLeaveRequestRepository> employeeLeaveRequestRepositoryMock;
        public TestEmployeeLeaveRequestRepository testEmployeeLeaveRequestRepository;
        private EmployeeLeaveRequestService employeeLeaveRequestService;
        private ICurrentUserFactory employeeLeaveRequestUserFactory;
        private ICurrentUserFactory timeApproverUserFactory;
        private ICurrentUserFactory proxyTimeApproverUserFactory;
        public Domain.Entities.Role timeApprovalRole;
        public Domain.Entities.Permission timeEntryApprovalPermission;

        // Adapters
        public ITypeAdapter<Domain.HumanResources.Entities.LeaveRequest, Dtos.HumanResources.LeaveRequest> leaveRequestEntityToDtoAdapter;
        public ITypeAdapter<Dtos.HumanResources.LeaveRequest, Domain.HumanResources.Entities.LeaveRequest> leaveRequestDtoToEntityAdapter;

        public FunctionEqualityComparer<Dtos.HumanResources.LeaveRequest> leaveRequestDtoComparer;

        public void EmployeeLeaveRequestServiceTestsInitilize()
        {
            MockInitialize();

            supervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
            personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
            employeeLeaveRequestRepositoryMock = new Mock<IEmployeeLeaveRequestRepository>();
            testEmployeeLeaveRequestRepository = new TestEmployeeLeaveRequestRepository();
            employeeLeaveRequestUserFactory = new EmployeeLeaveRequestUserFactory();
            timeApproverUserFactory = new TimeApproverUserFactory();
            proxyTimeApproverUserFactory = new ProxyTimeApproverUserFactory();

            leaveRequestEntityToDtoAdapter = new LeaveRequestEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            leaveRequestDtoToEntityAdapter = new LeaveRequestDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            var leaveRequestStatusDtoToEntityAdapter = new AutoMapperAdapter<Dtos.HumanResources.LeaveRequestStatus, Domain.HumanResources.Entities.LeaveRequestStatus>(adapterRegistryMock.Object, loggerMock.Object);
            var leaveRequestStatusEntityToDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.LeaveRequestStatus, Dtos.HumanResources.LeaveRequestStatus>(adapterRegistryMock.Object, loggerMock.Object);

            leaveRequestStatusEntityToDtoAdapter.AddMappingDependency<Domain.Base.Entities.Timestamp, Dtos.Base.Timestamp>();

            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.HumanResources.Entities.LeaveRequest, Dtos.HumanResources.LeaveRequest>())
              .Returns(() => leaveRequestEntityToDtoAdapter);

            adapterRegistryMock.Setup(ar => ar.GetAdapter<Dtos.HumanResources.LeaveRequest, Domain.HumanResources.Entities.LeaveRequest>())
                .Returns(() => leaveRequestDtoToEntityAdapter);

            adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.HumanResources.LeaveRequestStatus, Domain.HumanResources.Entities.LeaveRequestStatus>())
                .Returns(leaveRequestStatusDtoToEntityAdapter);

            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.HumanResources.Entities.LeaveRequestStatus, Dtos.HumanResources.LeaveRequestStatus>())
             .Returns(leaveRequestStatusEntityToDtoAdapter);

            supervisorsRepositoryMock.Setup(r => r.GetSuperviseesBySupervisorAsync(It.IsAny<string>(), null)).ReturnsAsync(new string[] { "0011560" });

            employeeLeaveRequestService = new EmployeeLeaveRequestService(supervisorsRepositoryMock.Object,
                personBaseRepositoryMock.Object,
                employeeLeaveRequestRepositoryMock.Object,
                adapterRegistryMock.Object,
                employeeLeaveRequestUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object
                );

            // Mock the repo methods
            employeeLeaveRequestRepositoryMock.Setup(elr => elr.GetLeaveRequestsAsync(It.IsAny<List<string>>()))
               .Returns<List<string>>((ids) =>
               testEmployeeLeaveRequestRepository.GetLeaveRequestsAsync(ids));

            employeeLeaveRequestRepositoryMock.Setup(elr => elr.GetLeaveRequestInfoByLeaveRequestIdAsync(It.IsAny<string>()))
               .Returns<string>((id) =>
               testEmployeeLeaveRequestRepository.GetLeaveRequestInfoByLeaveRequestIdAsync(id));

            employeeLeaveRequestRepositoryMock.Setup(elr => elr.UpdateLeaveRequestAsync(It.IsAny<LeaveRequestHelper>()))
             .Returns<LeaveRequestHelper>((helper) =>
             testEmployeeLeaveRequestRepository.UpdateLeaveRequestAsync(helper));

            employeeLeaveRequestRepositoryMock.Setup(elr => elr.CreateLeaveRequestStatusAsync(It.IsAny<LeaveRequestStatus>()))
                .Returns<LeaveRequestStatus>((lrs) => testEmployeeLeaveRequestRepository.CreateLeaveRequestStatusAsync(lrs));

            employeeLeaveRequestRepositoryMock.Setup(elr => elr.CreateLeaveRequestAsync(It.IsAny<LeaveRequest>()))
                .Returns<LeaveRequest>((lr) => testEmployeeLeaveRequestRepository.CreateLeaveRequestAsync(lr));

            leaveRequestDtoComparer = this.LeaveRequestComparer();
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            employeeLeaveRequestUserFactory = null;
            employeeLeaveRequestRepositoryMock = null;
            personBaseRepositoryMock = null;
            supervisorsRepositoryMock = null;
        }
        #endregion

        #region Tests
        [TestClass]
        public class GetLeaveRequestsAsyncTests : EmployeeLeaveRequestServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestServiceTestsInitilize();
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RepositoryCalledWithNoAccessUserTest()
            {
                await employeeLeaveRequestService.GetLeaveRequestsAsync("0018998");
            }

            [TestMethod]
            public async Task RepositoryCalledWithCurrentUserTest()
            {
                await employeeLeaveRequestService.GetLeaveRequestsAsync();
                employeeLeaveRequestRepositoryMock.Verify(rm => rm.GetLeaveRequestsAsync(It.Is<List<string>>(ids =>
                    ids.Count() == 1 && ids.ElementAt(0) == employeeLeaveRequestUserFactory.CurrentUser.PersonId)));
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = (await testEmployeeLeaveRequestRepository.GetLeaveRequestsAsync(new List<string>() { employeeLeaveRequestUserFactory.CurrentUser.PersonId }))
                    .Select(lr => leaveRequestEntityToDtoAdapter.MapToType(lr));

                var actual = await employeeLeaveRequestService.GetLeaveRequestsAsync();

                CollectionAssert.AreEqual(expected.ToList(), actual.ToList(), leaveRequestDtoComparer);
            }
        }

        [TestClass]
        public class GetLeaveRequestInfoByLeaveRequestIdAsync : EmployeeLeaveRequestServiceTests
        {
            public string inputLeaveRequestId;

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestServiceTestsInitilize();
                inputLeaveRequestId = testEmployeeLeaveRequestRepository.leaveRequestRecords[0].Id;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RepositoryCalledWithNoAccessUserTest()
            {
                await employeeLeaveRequestService.GetLeaveRequestInfoByLeaveRequestIdAsync(inputLeaveRequestId, "0018998");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserCannotAccessOthersLeaveRequestTest()
            {
                testEmployeeLeaveRequestRepository.leaveRequestRecords[0].EmployeeId = "dummyUser";
                await employeeLeaveRequestService.GetLeaveRequestInfoByLeaveRequestIdAsync(inputLeaveRequestId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RepositoryCalledWithoutLeaveRequestIdTest()
            {
                await employeeLeaveRequestService.GetLeaveRequestInfoByLeaveRequestIdAsync(null);
            }

            [TestMethod]
            public async Task CorrectDtoReturnedTest()
            {
                var expected = leaveRequestEntityToDtoAdapter.MapToType(await testEmployeeLeaveRequestRepository.GetLeaveRequestInfoByLeaveRequestIdAsync(inputLeaveRequestId));
                var actual = await employeeLeaveRequestService.GetLeaveRequestInfoByLeaveRequestIdAsync(inputLeaveRequestId);
                Assert.IsTrue(leaveRequestDtoComparer.Equals(expected, actual));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RepositoryReturnedNullTest()
            {
                employeeLeaveRequestRepositoryMock.Setup(r => r.GetLeaveRequestInfoByLeaveRequestIdAsync(It.IsAny<string>()))
                    .Returns<string>((id) => Task.FromResult<Domain.HumanResources.Entities.LeaveRequest>(null));

                try
                {
                    await employeeLeaveRequestService.GetLeaveRequestInfoByLeaveRequestIdAsync(inputLeaveRequestId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }
        }

        [TestClass]
        public class CreateLeaveRequestAsync : EmployeeLeaveRequestServiceTests
        {
            public Dtos.HumanResources.LeaveRequest newLeaveRequest;

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestServiceTestsInitilize();
                newLeaveRequest = new Dtos.HumanResources.LeaveRequest()
                {
                    Id = null,
                    PerLeaveId = "325",
                    EmployeeId = "0011560",
                    StartDate = new DateTime(2019, 06, 06),
                    EndDate = new DateTime(2019, 06, 06),
                    ApproverId = "0010351",
                    ApproverName = "Hadrian O. Racz",
                    Status = Dtos.HumanResources.LeaveStatusAction.Submitted,
                    LeaveRequestDetails = new List<Dtos.HumanResources.LeaveRequestDetail>()
                    {
                    new Dtos.HumanResources.LeaveRequestDetail()
                    {
                        Id = null,
                        LeaveRequestId = null,
                        LeaveDate = DateTime.Today,
                        LeaveHours = 8.00m
                    }
                    },
                    LeaveRequestComments = new List<Dtos.HumanResources.LeaveRequestComment>()
                };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RepositoryCalledWithNullDtoTest()
            {
                await employeeLeaveRequestService.CreateLeaveRequestAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsNotAuthorizedTest()
            {
                Assert.AreNotEqual(employeeLeaveRequestUserFactory.CurrentUser.PersonId, "0010378"); // 0010378 is not the current user
                Assert.AreNotEqual(employeeLeaveRequestUserFactory.CurrentUser.ProxySubjects.FirstOrDefault(), "0010378"); // 0010378 is not a current user proxy subject
                await employeeLeaveRequestService.CreateLeaveRequestAsync(newLeaveRequest, "0010378");
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingResourceException))]
            public async Task LeaveRequestBeingCreatedAlreadyExistsTest()
            {
                // Pass an existing leave request 
                var leaveRequestDto = new Dtos.HumanResources.LeaveRequest()
                {
                    Id = null,
                    PerLeaveId = "805",
                    EmployeeId = "0011560",
                    StartDate = new DateTime(2019, 11, 05),
                    EndDate = new DateTime(2019, 11, 06),
                    ApproverId = "0010351",
                    ApproverName = "Hadrian O. Racz",
                    Status = Dtos.HumanResources.LeaveStatusAction.Draft,
                    LeaveRequestDetails = new List<Dtos.HumanResources.LeaveRequestDetail>()
                    {
                        new Dtos.HumanResources.LeaveRequestDetail()
                        {
                            Id = null,
                            LeaveRequestId = null,
                            LeaveDate = new DateTime(2019,11,05),
                            LeaveHours = 8.00m,
                            ProcessedInPayPeriod = false
                        }
                    },
                    LeaveRequestComments = new List<Dtos.HumanResources.LeaveRequestComment>()
                };
                await employeeLeaveRequestService.UpdateLeaveRequestAsync(leaveRequestDto);
            }

            [TestMethod]
            public async Task NewLeaveRequestReturnedTest()
            {
                var expected = newLeaveRequest;
                var actual = await employeeLeaveRequestService.CreateLeaveRequestAsync(newLeaveRequest, null);

                // We cannot use the Dto Comparer here as the input request object will not have Id for the new leave request being created.
                foreach (var expectedProp in expected.GetType().GetProperties())
                {
                    foreach (var actualProp in actual.GetType().GetProperties())
                    {
                        if (expectedProp.Name != "LeaveRequestDetails" &&
                            expectedProp.Name != "LeaveRequestComments" &&
                            expectedProp.Name != "Id"
                            && expectedProp == actualProp)
                        {
                            var name = expectedProp.Name;
                            Assert.AreEqual(expectedProp.GetValue(expected), actualProp.GetValue(actual));
                            break;
                        }
                    }
                }

                // Asserting the leave request details and comments
                Assert.AreEqual(expected.LeaveRequestDetails.Count, actual.LeaveRequestDetails.Count);
                for (int i = 0; i < expected.LeaveRequestDetails.Count; i++)
                {
                    Assert.AreEqual(expected.LeaveRequestDetails[i].LeaveDate, actual.LeaveRequestDetails[i].LeaveDate);
                    Assert.AreEqual(expected.LeaveRequestDetails[i].LeaveHours, actual.LeaveRequestDetails[i].LeaveHours);
                }
                Assert.AreEqual(expected.LeaveRequestComments.Count, actual.LeaveRequestComments.Count);
                for (int i = 0; i < expected.LeaveRequestComments.Count; i++)
                {
                    Assert.AreEqual(expected.LeaveRequestComments[i].CommentAuthorName, actual.LeaveRequestComments[i].CommentAuthorName);
                    Assert.AreEqual(expected.LeaveRequestComments[i].Comments, actual.LeaveRequestComments[i].Comments);
                    Assert.AreEqual(expected.LeaveRequestComments[i].EmployeeId, actual.LeaveRequestComments[i].EmployeeId);
                    Assert.AreEqual(expected.LeaveRequestComments[i].LeaveRequestId, actual.LeaveRequestComments[i].LeaveRequestId);
                }
            }
        }

        [TestClass]
        public class CreateLeaveRequestStatusAsync : EmployeeLeaveRequestServiceTests
        {
            public Dtos.HumanResources.LeaveRequestStatus newLeaveRequestStatus;

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestServiceTestsInitilize();
                newLeaveRequestStatus = new Dtos.HumanResources.LeaveRequestStatus()
                {
                    LeaveRequestId = testEmployeeLeaveRequestRepository.leaveRequestRecords[0].Id,
                    ActionType = Dtos.HumanResources.LeaveStatusAction.Rejected,
                    ActionerId = testEmployeeLeaveRequestRepository.leaveRequestRecords[0].ApproverId
                };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RepositoryCalledWithNullDtoTest()
            {
                await employeeLeaveRequestService.CreateLeaveRequestStatusAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task RepositoryCalledWithNullLeaveRequestIdTest()
            {
                newLeaveRequestStatus.LeaveRequestId = null;
                await employeeLeaveRequestService.CreateLeaveRequestStatusAsync(newLeaveRequestStatus);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsNotAuthorizedTest()
            {
                Assert.AreNotEqual(employeeLeaveRequestUserFactory.CurrentUser.PersonId, "0010378"); // 0010378 is not the current user
                Assert.AreNotEqual(employeeLeaveRequestUserFactory.CurrentUser.ProxySubjects.FirstOrDefault(), "0010378"); // 0010378 is not a current user proxy subject
                await employeeLeaveRequestService.CreateLeaveRequestStatusAsync(newLeaveRequestStatus, "0010378");
            }

            [TestMethod]
            public async Task NewLeaveRequestStatusReturnedTest()
            {
                var expected = newLeaveRequestStatus;
                var actual = await employeeLeaveRequestService.CreateLeaveRequestStatusAsync(newLeaveRequestStatus);
                // We cannot use the Dto Comparer here as the input request object will not have Id for the new leave request status being created.
                foreach (var expectedProp in expected.GetType().GetProperties())
                {
                    foreach (var actualProp in actual.GetType().GetProperties())
                    {
                        if (expectedProp.Name != "Timestamp"
                            && expectedProp.Name != "Id"
                            && expectedProp == actualProp)
                        {
                            var name = expectedProp.Name;
                            Assert.AreEqual(expectedProp.GetValue(expected), actualProp.GetValue(actual));
                            break;
                        }
                    }
                }
            }
        }

        [TestClass]
        public class UpdateLeaveRequestAsync : EmployeeLeaveRequestServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestServiceTestsInitilize();
            }

            public async Task<Ellucian.Colleague.Dtos.HumanResources.LeaveRequest> getLeaveRequestDto()
            {
                return leaveRequestEntityToDtoAdapter.MapToType(await testEmployeeLeaveRequestRepository.GetLeaveRequestInfoByLeaveRequestIdAsync(testEmployeeLeaveRequestRepository.leaveRequestRecords[0].Id));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsNotAuthorizedTest()
            {
                Assert.AreNotEqual(employeeLeaveRequestUserFactory.CurrentUser.PersonId, "0010378"); // 0010378 is not the current user
                Assert.AreNotEqual(employeeLeaveRequestUserFactory.CurrentUser.ProxySubjects.FirstOrDefault(), "0010378"); // 0010378 is not a current user proxy subject
                await employeeLeaveRequestService.UpdateLeaveRequestAsync(await getLeaveRequestDto(), "0010378");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RepositoryCalledWithNullDtoTest()
            {
                await employeeLeaveRequestService.UpdateLeaveRequestAsync(null);
            }

            [TestMethod]
            public async Task SuccessfulUpdateTest_AddNewLeaveRequestDetail()
            {
                var expected = await getLeaveRequestDto();

                // Adding a new LeaveRequestDetail before updating.
                expected.EndDate = expected.EndDate.Value.AddDays(1);
                expected.LeaveRequestDetails.Add(new Dtos.HumanResources.LeaveRequestDetail()
                {
                    Id = null,
                    LeaveRequestId = expected.Id,
                    LeaveDate = expected.EndDate.Value,
                    LeaveHours = 8.00m,
                    ProcessedInPayPeriod = false
                });

                var actual = await employeeLeaveRequestService.UpdateLeaveRequestAsync(expected);

                // We cannot use the Dto Comparer here as the input request object will not have Id for the new leave request detail being added.
                foreach (var expectedProp in expected.GetType().GetProperties())
                {
                    foreach (var actualProp in actual.GetType().GetProperties())
                    {
                        if (expectedProp.Name != "LeaveRequestDetails" // Ignore the leave request details and comments for now
                            && expectedProp.Name != "LeaveRequestComments"
                            && expectedProp.Name != "Id"
                            && expectedProp == actualProp)
                        {
                            var name = expectedProp.Name;
                            Assert.AreEqual(expectedProp.GetValue(expected), actualProp.GetValue(actual));
                            break;
                        }
                    }
                }

                // Asserting the leave request detail and leave request comment objects
                Assert.AreEqual(expected.LeaveRequestDetails.Count, actual.LeaveRequestDetails.Count);
                for (int i = 0; i < expected.LeaveRequestDetails.Count; i++)
                {
                    Assert.AreEqual(expected.LeaveRequestDetails[i].LeaveDate, actual.LeaveRequestDetails[i].LeaveDate);
                    Assert.AreEqual(expected.LeaveRequestDetails[i].LeaveRequestId, actual.LeaveRequestDetails[i].LeaveRequestId);
                    Assert.AreEqual(expected.LeaveRequestDetails[i].LeaveHours, actual.LeaveRequestDetails[i].LeaveHours);
                }

                Assert.AreEqual(expected.LeaveRequestComments.Count, actual.LeaveRequestComments.Count);
                for (int i = 0; i < expected.LeaveRequestComments.Count; i++)
                {
                    Assert.AreEqual(expected.LeaveRequestComments[i].CommentAuthorName, actual.LeaveRequestComments[i].CommentAuthorName);
                    Assert.AreEqual(expected.LeaveRequestComments[i].Comments, actual.LeaveRequestComments[i].Comments);
                    Assert.AreEqual(expected.LeaveRequestComments[i].EmployeeId, actual.LeaveRequestComments[i].EmployeeId);
                    Assert.AreEqual(expected.LeaveRequestComments[i].LeaveRequestId, actual.LeaveRequestComments[i].LeaveRequestId);
                }
            }

            [TestMethod]
            public async Task SuccessfulUpdateTest_UpdateALeaveRequestDetail()
            {
                var expected = await getLeaveRequestDto();

                // Update a LeaveRequestDetail before updating.               
                expected.LeaveRequestDetails[0].LeaveHours = 10.00m;

                var actual = await employeeLeaveRequestService.UpdateLeaveRequestAsync(expected);

                Assert.IsTrue(leaveRequestDtoComparer.Equals(expected, actual));
            }

            [TestMethod]
            public async Task SuccessfulUpdateTest_DeleteALeaveRequestDetail()
            {
                var expected = await getLeaveRequestDto();

                // Delete a LeaveRequestDetail before updating.               
                expected.LeaveRequestDetails.RemoveAt(1);

                var actual = await employeeLeaveRequestService.UpdateLeaveRequestAsync(expected);

                Assert.IsTrue(leaveRequestDtoComparer.Equals(expected, actual));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserCannotUpdateOthersLeaveRequestTest()
            {
                testEmployeeLeaveRequestRepository.leaveRequestRecords[0].EmployeeId = "dummyUser";
                var lrToUpdate = leaveRequestEntityToDtoAdapter.MapToType(await testEmployeeLeaveRequestRepository.GetLeaveRequestInfoByLeaveRequestIdAsync(testEmployeeLeaveRequestRepository.leaveRequestRecords[0].Id));

                await employeeLeaveRequestService.UpdateLeaveRequestAsync(lrToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoExistingTimecardThrowsErrorTest()
            {
                employeeLeaveRequestRepositoryMock.Setup(m => m.GetLeaveRequestInfoByLeaveRequestIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                var lrToUpdate = leaveRequestEntityToDtoAdapter.MapToType(await testEmployeeLeaveRequestRepository.GetLeaveRequestInfoByLeaveRequestIdAsync(testEmployeeLeaveRequestRepository.leaveRequestRecords[0].Id));
                await employeeLeaveRequestService.UpdateLeaveRequestAsync(lrToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InputLeaveRequestEmployeeIdDoesNotMatchDatabaseLeaveRequestTest()
            {
                var inputDto = await getLeaveRequestDto();
                inputDto.EmployeeId = "foobar";
                await employeeLeaveRequestService.UpdateLeaveRequestAsync(inputDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingResourceException))]
            public async Task LeaveRequestBeingUpdatedAlreadyExistsTest()
            {
                var inputDto = await getLeaveRequestDto();
                inputDto.Id = "2";
                await employeeLeaveRequestService.UpdateLeaveRequestAsync(inputDto);
            }
        }
        #endregion

        [TestClass]
        public class GetLeaveRequestsForTimeEntryAsync : EmployeeLeaveRequestServiceTests
        {
            private IEnumerable<LeaveRequest> leaveRequestEntities;

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestServiceTestsInitilize();

                //Set up Data
                SetupData();

                //Set up mock
                SetupMock();
            }

            private async void SetupData()
            {
                leaveRequestEntities = await testEmployeeLeaveRequestRepository.GetLeaveRequestsForTimeEntryAsync(DateTime.Today, DateTime.Today.AddDays(4), new List<string>() { employeeLeaveRequestUserFactory.CurrentUser.PersonId });
            }

            private void SetupMock()
            {
                employeeLeaveRequestRepositoryMock.Setup(elr => elr.GetLeaveRequestsForTimeEntryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<string>>())).
               ReturnsAsync(leaveRequestEntities);
            }

            [TestMethod, ExpectedException(typeof(PermissionsException))]
            public async Task GetLeaveRequestsForTimeEntryAsync_Throws_PermissionException()
            {
                await employeeLeaveRequestService.GetLeaveRequestsForTimeEntryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), "0001958");
            }

            [TestMethod]
            public async Task GetLeaveRequestsForTimeEntryAsync_With_Valid_Data_From_Repository()
            {
                employeeLeaveRequestService = new EmployeeLeaveRequestService(supervisorsRepositoryMock.Object,
              personBaseRepositoryMock.Object,
              testEmployeeLeaveRequestRepository,
              adapterRegistryMock.Object,
              employeeLeaveRequestUserFactory,
              roleRepositoryMock.Object,
              loggerMock.Object
              );

                var expected = (await testEmployeeLeaveRequestRepository.GetLeaveRequestsForTimeEntryAsync(DateTime.Today, DateTime.Today.AddDays(4), new List<string>() { employeeLeaveRequestUserFactory.CurrentUser.PersonId }))
                  .Select(lr => leaveRequestEntityToDtoAdapter.MapToType(lr));

                string persondId = employeeLeaveRequestUserFactory.CurrentUser.PersonId;

                var actual = await employeeLeaveRequestService.GetLeaveRequestsForTimeEntryAsync(DateTime.Today, DateTime.Today.AddDays(4), persondId);

                Assert.IsTrue(actual.Any());
                Assert.AreEqual(actual.FirstOrDefault().EmployeeId, persondId);

                CollectionAssert.AreEqual(expected.ToList(), actual.ToList(), leaveRequestDtoComparer);
            }

            [TestMethod]
            public async Task GetLeaveRequestsForTimeEntryAsync_SupervisorCanAccessSuperviseesData()
            {
                // permissions mock
                timeApprovalRole = new Domain.Entities.Role(76, "TIME MANAGEMENT SUPERVISOR");
                timeEntryApprovalPermission = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ApproveRejectEmployeeTimecard);
                timeApprovalRole.AddPermission(timeEntryApprovalPermission);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { timeApprovalRole });

                employeeLeaveRequestService = new EmployeeLeaveRequestService(supervisorsRepositoryMock.Object,
                personBaseRepositoryMock.Object,
                testEmployeeLeaveRequestRepository,
                adapterRegistryMock.Object,
                timeApproverUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object
                );

                string personId = employeeLeaveRequestUserFactory.CurrentUser.PersonId;

                var expected = (await testEmployeeLeaveRequestRepository.GetLeaveRequestsForTimeEntryAsync(DateTime.Today, DateTime.Today.AddDays(4), new List<string>() { personId }))
                  .Select(lr => leaveRequestEntityToDtoAdapter.MapToType(lr));
                               
                var actual = await employeeLeaveRequestService.GetLeaveRequestsForTimeEntryAsync(DateTime.Today, DateTime.Today.AddDays(4), personId);

                Assert.IsTrue(actual.Any());
                Assert.AreEqual(actual.FirstOrDefault().EmployeeId, personId);

                CollectionAssert.AreEqual(expected.ToList(), actual.ToList(), leaveRequestDtoComparer);
            }

            [TestMethod]
            public async Task GetLeaveRequestsForTimeEntryAsync_ProxySupervisorCanAccessSuperviseesData()
            {               
                employeeLeaveRequestService = new EmployeeLeaveRequestService(supervisorsRepositoryMock.Object,
                personBaseRepositoryMock.Object,
                testEmployeeLeaveRequestRepository,
                adapterRegistryMock.Object,
                proxyTimeApproverUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object
                );

                string personId = timeApproverUserFactory.CurrentUser.PersonId;

                var expected = (await testEmployeeLeaveRequestRepository.GetLeaveRequestsForTimeEntryAsync(DateTime.Today, DateTime.Today.AddDays(4), new List<string>() { personId }))
                 .Select(lr => leaveRequestEntityToDtoAdapter.MapToType(lr));               

                var actual = await employeeLeaveRequestService.GetLeaveRequestsForTimeEntryAsync(DateTime.Today, DateTime.Today.AddDays(4), personId);

                Assert.IsTrue(actual.Any());
                Assert.AreEqual(actual.FirstOrDefault().EmployeeId, employeeLeaveRequestUserFactory.CurrentUser.PersonId);

                CollectionAssert.AreEqual(expected.ToList(), actual.ToList(), leaveRequestDtoComparer);
            }
        }

        #region Helpers
        private FunctionEqualityComparer<Dtos.HumanResources.LeaveRequest> LeaveRequestComparer()
        {
            return new FunctionEqualityComparer<Dtos.HumanResources.LeaveRequest>(
                (lr1, lr2) =>
                    lr1.Id == lr2.Id &&
                    lr1.EmployeeId == lr2.EmployeeId &&
                    lr1.PerLeaveId == lr2.PerLeaveId &&
                    lr1.StartDate == lr2.StartDate &&
                    lr1.EndDate == lr2.EndDate &&
                    lr1.ApproverId == lr2.ApproverId &&
                    lr1.ApproverName == lr2.ApproverName &&
                    this.leaveRequestDetailEqual(lr1, lr2),
                    (lr) => lr.Id.GetHashCode()
                );
        }

        private FunctionEqualityComparer<Dtos.HumanResources.LeaveRequestDetail> LeaveRequestDetailsComparer()
        {
            return new FunctionEqualityComparer<Dtos.HumanResources.LeaveRequestDetail>(
                (lrd1, lrd2) =>
                    lrd1.Id == lrd2.Id &&
                    lrd1.LeaveRequestId == lrd2.LeaveRequestId &&
                    lrd1.LeaveDate == lrd2.LeaveDate &&
                    lrd1.LeaveHours == lrd2.LeaveHours &&
                    lrd1.ProcessedInPayPeriod == lrd2.ProcessedInPayPeriod,
                    (lrd) => lrd.Id.GetHashCode()
                );
        }

        private bool leaveRequestDetailEqual(Dtos.HumanResources.LeaveRequest a, Dtos.HumanResources.LeaveRequest b)
        {
            CollectionAssert.AreEqual(a.LeaveRequestDetails, b.LeaveRequestDetails, LeaveRequestDetailsComparer());
            return true;
        }

        public class EmployeeLeaveRequestUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Brown",
                        PersonId = "0011560",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "jenbrown",
                        Roles = new List<string>() { "EMPLOYEE" },
                        SessionFixationId = "abc123"

                    });
                }
            }
        }

        public class TimeApproverUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "001",
                        Name = "Time Approver",
                        PersonId = "0011111",
                        SecurityToken = "999",
                        SessionTimeout = 20,
                        UserName = "jerry",
                        Roles = new List<string>() { "TIME MANAGEMENT SUPERVISOR" },
                        SessionFixationId = "qwerty44",
                    });
                }
            }
        }

        public class ProxyTimeApproverUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "001",
                        Name = "Proxy Time Approver",
                        PersonId = "0011223",
                        SecurityToken = "999",
                        SessionTimeout = 20,
                        UserName = "spacex",
                        Roles = new List<string>() { "EMPLOYEE" },
                        SessionFixationId = "qwerty44",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0011111",
                            Permissions = new List<string> { "TMTA" }
                        }
                    });
                }
            }
        }
        #endregion
    }
}
