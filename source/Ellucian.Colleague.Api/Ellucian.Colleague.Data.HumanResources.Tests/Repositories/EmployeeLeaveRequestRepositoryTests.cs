// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class EmployeeLeaveRequestRepositoryTests : BasePersonSetup
    {
        public string employeeId;
        public List<string> employeeIds;
        public string leaveRequestId;
        public string newlyCreatedLeaveRequestId;
        public TestEmployeeLeaveRequestRepository testData;
        public EmployeeLeaveRequestRepository repositoryUnderTest;

        public CreateLeaveRequestRequest actualCreateLeaveRequestRequest;
        public CreateLeaveRequestStatusRequest actualCreateLeaveRequestStatusRequest;
        public UpdateLeaveRequestRequest actualUpdateLeaveRequestRequest;

        public DateTime startDate;
        public DateTime endDate;

        public EmployeeLeaveRequestRepository BuildRepository()
        {
            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            #region GetLeaveRequestsAsync
            #region LEAVE.REQUEST
            dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST", "WITH LR.EMPLOYEE.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
              .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                      Task.FromResult((testData.leaveRequestRecords == null) ? null :
                          testData.leaveRequestRecords
                          .Select(rec => rec.Id).ToArray()
                      ));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequest>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testData.leaveRequestRecords == null ? null :
                    new Collection<DataContracts.LeaveRequest>(
                        testData.leaveRequestRecords
                        .Where(rec => ids.Contains(rec.Id))
                        .Select(rec => new DataContracts.LeaveRequest()
                        {
                            Recordkey = rec.Id,
                            LrPerleaveId = rec.PerLeaveId,
                            LrEmployeeId = rec.EmployeeId,
                            LrStartDate = rec.StartDate,
                            LrEndDate = rec.EndDate,
                            LrApproverId = rec.ApproverId,
                            LrApproverName = rec.ApproverName
                        }).ToList()
                    )));


            #endregion

            #region LEAVE.REQUEST.DETAIL
            dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", "WITH LRD.LEAVE.REQUEST.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
             .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                     Task.FromResult((TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords == null) ? null :
                         TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords
                         .Select(rec => rec.Id).ToArray()

      ));
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestDetail>(It.IsAny<string[]>(), It.IsAny<bool>()))
            .Returns<string[], bool>((ids, b) => Task.FromResult(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords == null ? null :
                new Collection<DataContracts.LeaveRequestDetail>(
                    TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords
                        .Where(rec => ids.Contains(rec.Id))
                        .Select(rec => new DataContracts.LeaveRequestDetail()
                        {
                            Recordkey = rec.Id,
                            LrdLeaveRequestId = rec.LeaveRequestId,
                            LrdLeaveDate = rec.LeaveDate,
                            LrdLeaveHours = rec.LeaveHours
                        }).ToList()
                    )));
            #endregion

            #region LEAVE.REQUEST.STATUS
            dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.STATUS", "WITH LRS.LEAVE.REQUEST.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
            .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                    Task.FromResult((testData.leaveRequestStatusRecords == null) ? null :
                        testData.leaveRequestStatusRecords
                        .Select(rec => rec.Id).ToArray()
                    ));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestStatus>(It.IsAny<string[]>(), It.IsAny<bool>()))
            .Returns<string[], bool>((ids, b) => Task.FromResult(testData.leaveRequestStatusRecords == null ? null :
                new Collection<DataContracts.LeaveRequestStatus>(
                    testData.leaveRequestStatusRecords
                        .Where(rec => ids.Contains(rec.Id))
                        .Select(rec => new DataContracts.LeaveRequestStatus()
                        {
                            Recordkey = rec.Id,
                            LrsLeaveRequestId = rec.LeaveRequestId,
                            LrsActionerId = rec.ActionerId,
                            LrsActionType = rec.ActionType.ToString(),
                            LeaveRequestStatusAdddate = rec.AddDate,
                            LeaveRequestStatusAddtime = rec.AddTime,
                            LeaveRequestStatusAddopr = rec.AddOpr,
                            LeaveRequestStatusChgdate = rec.AddDate,
                            LeaveRequestStatusChgtime = rec.AddTime,
                            LeaveRequestStatusChgopr = rec.AddOpr
                        }).ToList()
                    )));
            #endregion

            #region PERSON.BASE          
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Person>("PERSON", It.IsAny<string[]>(), true))
                   .Returns<string, string[], bool>((x, y, z) => Task.FromResult(new Collection<Person>()
               {
                    new Person(){
                        Recordkey = "0011560",
                        LastName = "Brown",
                        FirstName = "Jennifer",
                        MiddleName = ""
                    }
               }));
            #endregion
            // To Do : Incude LEAVE.REQUEST.COMMENTS
            #endregion

            #region GetLeaveRequestInfoByLeaveRequestIdAsync

            dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.LeaveRequest>(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns<string, bool>((id, b) =>
            Task.FromResult(testData.leaveRequestRecords
                .Where(lr => lr.Id == id)
                .Select(lr => new DataContracts.LeaveRequest()
                {
                    Recordkey = lr.Id,
                    LrPerleaveId = lr.PerLeaveId,
                    LrEmployeeId = lr.EmployeeId,
                    LrStartDate = lr.StartDate,
                    LrEndDate = lr.EndDate,
                    LrApproverId = lr.ApproverId,
                    LrApproverName = lr.ApproverName
                }).FirstOrDefault())
                );

            #endregion

            #region CreateLeaveRequestAsync           
            transManagerMock.Setup(t => t.ExecuteAsync<Transactions.CreateLeaveRequestRequest, Transactions.CreateLeaveRequestResponse>(It.IsAny<Transactions.CreateLeaveRequestRequest>()))
                .Callback<Transactions.CreateLeaveRequestRequest>((req) =>
                {
                    actualCreateLeaveRequestRequest = req;
                })
                .Returns<Transactions.CreateLeaveRequestRequest>((req) =>
                    Task.FromResult(new Transactions.CreateLeaveRequestResponse()
                    {
                        ErrorMessage = "",
                        OutLeaveRequestId = testData.CreateLeaveRequestHelper(req.LrPerleaveId, req.LrEmployeeId, req.LrStartDate, req.LrEndDate
                        , req.LrApproverId, req.LrApproverName, LeaveStatusAction.Draft, BuildLeaveRequestDetailsToBeCreated(req.CreateLeaveRequestDetails))
                    }));
            #endregion

            #region CreateLeaveRequestStatusAsync
            transManagerMock.Setup(t => t.ExecuteAsync<Transactions.CreateLeaveRequestStatusRequest, Transactions.CreateLeaveRequestStatusResponse>(It.IsAny<Transactions.CreateLeaveRequestStatusRequest>()))
             .Callback<Transactions.CreateLeaveRequestStatusRequest>((req) =>
             {
                 actualCreateLeaveRequestStatusRequest = req;
             })
             .Returns<Transactions.CreateLeaveRequestStatusRequest>((req) =>
                 Task.FromResult(new Transactions.CreateLeaveRequestStatusResponse()
                 {
                     ErrorMessage = "",
                     NewLeaveStatusKey = testData.CreateLeaveRequestStatusHelper(req.LrLeaveRequestId, (LeaveStatusAction)Enum.Parse(typeof(LeaveStatusAction), req.LrActionType), req.LrActionerId)
                 }));

            dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.LeaveRequestStatus>(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns<string, bool>((id, b) =>
            Task.FromResult(testData.leaveRequestStatusRecords
                .Where(lr => lr.Id == id)
                .Select(lr => new DataContracts.LeaveRequestStatus()
                {
                    Recordkey = lr.Id,
                    LrsLeaveRequestId = lr.LeaveRequestId,
                    LrsActionerId = lr.ActionerId,
                    LrsActionType = lr.ActionType.ToString(),
                    LeaveRequestStatusAdddate = lr.AddDate,
                    LeaveRequestStatusAddtime = lr.AddTime,
                    LeaveRequestStatusAddopr = lr.AddOpr,
                    LeaveRequestStatusChgdate = lr.ChangeDate,
                    LeaveRequestStatusChgtime = lr.ChangeTime,
                    LeaveRequestStatusChgopr = lr.ChangeOpr
                }).FirstOrDefault())
                );



            #endregion

            #region UpdateLeaveRequestAsync
            transManagerMock.Setup(t => t.ExecuteAsync<UpdateLeaveRequestRequest, UpdateLeaveRequestResponse>(It.IsAny<UpdateLeaveRequestRequest>()))
              .Callback<UpdateLeaveRequestRequest>((req) =>
              {
                  actualUpdateLeaveRequestRequest = req;
              })
              .Returns<UpdateLeaveRequestRequest>((req) => Task.FromResult(new UpdateLeaveRequestResponse() { ErrorMessage = "" }));
            #endregion

            return new EmployeeLeaveRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        public void EmployeeLeaveRequestRepositoryTestsInitialize()
        {
            MockInitialize();
            testData = new TestEmployeeLeaveRequestRepository();
            employeeId = testData.leaveRequestRecords[0].EmployeeId;
            employeeIds = new List<string>() { employeeId };
            leaveRequestId = testData.leaveRequestRecords[1].Id;
            repositoryUnderTest = BuildRepository();
            startDate = DateTime.Today;
            endDate = DateTime.Today.AddDays(4);
        }

        [TestClass]
        public class GetLeaveRequestsAsyncTests : EmployeeLeaveRequestRepositoryTests
        {
            public async Task<IEnumerable<LeaveRequest>> getExpected()
            {
                return await testData.GetLeaveRequestsAsync(employeeIds);
            }

            public async Task<IEnumerable<LeaveRequest>> getActual()
            {
                return await repositoryUnderTest.GetLeaveRequestsAsync(employeeIds);
            }

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestRepositoryTestsInitialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullEmployeeIdsTest()
            {
                employeeIds = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task EmptyEmployeeIdsTest()
            {
                employeeIds = new List<string>();
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task WhiteSpaceEmployeeIdsTest()
            {
                employeeIds = new List<string>() { "" };
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NoLeaveRequestKeysTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST", "WITH LR.EMPLOYEE.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
               .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                            Task.FromResult<string[]>(
                                null
                            ));

                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NoLeaveRequesDetailKeysTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", "WITH LRD.LEAVE.REQUEST.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                            Task.FromResult<string[]>(
                                null
                            ));

                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NoLeaveRequesStatusKeysTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.STATUS", "WITH LRS.LEAVE.REQUEST.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                            Task.FromResult<string[]>(
                                null
                            ));

                await getActual();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = await getExpected();
                var actual = await getActual();

                Assert.AreEqual(expected.Count(), actual.Count());
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.AreEqual(expected.ElementAt(i).Id, actual.ElementAt(i).Id);
                    Assert.AreEqual(expected.ElementAt(i).PerLeaveId, actual.ElementAt(i).PerLeaveId);
                    Assert.AreEqual(expected.ElementAt(i).EmployeeId, actual.ElementAt(i).EmployeeId);
                    Assert.AreEqual(expected.ElementAt(i).StartDate, actual.ElementAt(i).StartDate);
                    Assert.AreEqual(expected.ElementAt(i).EndDate, actual.ElementAt(i).EndDate);
                    Assert.AreEqual(expected.ElementAt(i).Status, actual.ElementAt(i).Status);
                    Assert.AreEqual(expected.ElementAt(i).ApproverId, actual.ElementAt(i).ApproverId);
                    Assert.IsTrue(string.IsNullOrWhiteSpace(actual.ElementAt(i).ApproverName));

                    CollectionAssert.AreEqual(expected.ElementAt(i).LeaveRequestDetails, actual.ElementAt(i).LeaveRequestDetails);
                }
            }

            [TestCleanup]
            public void CleanUp()
            {
                testData = null;
                repositoryUnderTest = null;
            }
        }

        [TestClass]
        public class GetLeaveRequestInfoByLeaveRequestIdAsyncTests : EmployeeLeaveRequestRepositoryTests
        {
            public async Task<LeaveRequest> getExpected()
            {
                return await testData.GetLeaveRequestInfoByLeaveRequestIdAsync(leaveRequestId);
            }

            public async Task<LeaveRequest> getActual()
            {
                return await repositoryUnderTest.GetLeaveRequestInfoByLeaveRequestIdAsync(leaveRequestId);
            }

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestRepositoryTestsInitialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullLeaveRequestIdTest()
            {
                leaveRequestId = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmptyLeaveRequestIdTest()
            {
                leaveRequestId = "";
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoMatchingLeaveRequestFoundTest()
            {
                leaveRequestId = "999";
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullLeaveRequesDetailKeysTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", string.Format("WITH LRD.LEAVE.REQUEST.ID EQ '{0}'", leaveRequestId)))
                .ReturnsAsync(() => null);

                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullLeaveRequesStatusKeysTest()
            {
                #region LEAVE.REQUEST.DETAIL
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", string.Format("WITH LRD.LEAVE.REQUEST.ID EQ '{0}'", leaveRequestId)))
                .ReturnsAsync(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == leaveRequestId)
                       .Select(rec => rec.Id).ToArray());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestDetail>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords == null ? null :
                    new Collection<DataContracts.LeaveRequestDetail>(
                        TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == leaveRequestId)
                           .Select(rec => new DataContracts.LeaveRequestDetail()
                           {
                               Recordkey = rec.Id,
                               LrdLeaveRequestId = rec.LeaveRequestId,
                               LrdLeaveDate = rec.LeaveDate,
                               LrdLeaveHours = rec.LeaveHours
                           }).ToList()
                        )));
                #endregion

                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.STATUS", string.Format("WITH LRS.LEAVE.REQUEST.ID EQ '{0}'", leaveRequestId)))
                .ReturnsAsync(() => null);

                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmptyLeaveRequesDetailKeysTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", string.Format("WITH LRD.LEAVE.REQUEST.ID EQ '{0}'", leaveRequestId)))
                 .ReturnsAsync(new string[] { });

                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmptyLeaveRequesStatustKeysTest()
            {
                #region LEAVE.REQUEST.DETAIL
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", string.Format("WITH LRD.LEAVE.REQUEST.ID EQ '{0}'", leaveRequestId)))
                .ReturnsAsync(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == leaveRequestId)
                       .Select(rec => rec.Id).ToArray());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestDetail>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords == null ? null :
                    new Collection<DataContracts.LeaveRequestDetail>(
                        TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == leaveRequestId)
                           .Select(rec => new DataContracts.LeaveRequestDetail()
                           {
                               Recordkey = rec.Id,
                               LrdLeaveRequestId = rec.LeaveRequestId,
                               LrdLeaveDate = rec.LeaveDate,
                               LrdLeaveHours = rec.LeaveHours
                           }).ToList()
                        )));
                #endregion

                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.STATUS", string.Format("WITH LRS.LEAVE.REQUEST.ID EQ '{0}'", leaveRequestId)))
                    .ReturnsAsync(new string[] { });

                await getActual();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                #region LEAVE.REQUEST.DETAIL
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", string.Format("WITH LRD.LEAVE.REQUEST.ID EQ '{0}'", leaveRequestId)))
                .ReturnsAsync(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == leaveRequestId)
                       .Select(rec => rec.Id).ToArray());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestDetail>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords == null ? null :
                    new Collection<DataContracts.LeaveRequestDetail>(
                        TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == leaveRequestId)
                           .Select(rec => new DataContracts.LeaveRequestDetail()
                           {
                               Recordkey = rec.Id,
                               LrdLeaveRequestId = rec.LeaveRequestId,
                               LrdLeaveDate = rec.LeaveDate,
                               LrdLeaveHours = rec.LeaveHours
                           }).ToList()
                        )));
                #endregion

                #region LEAVE.REQUEST.STATUS
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.STATUS", string.Format("WITH LRS.LEAVE.REQUEST.ID EQ '{0}'", leaveRequestId)))
                   .ReturnsAsync(testData.leaveRequestStatusRecords.Where(lrs => lrs.LeaveRequestId == leaveRequestId).Select(rec => rec.Id).ToArray());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestStatus>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testData.leaveRequestStatusRecords == null ? null :
                    new Collection<DataContracts.LeaveRequestStatus>(
                        testData.leaveRequestStatusRecords.Where(lrs => lrs.LeaveRequestId == leaveRequestId)
                            .Select(rec => new DataContracts.LeaveRequestStatus()
                            {
                                Recordkey = rec.Id,
                                LrsLeaveRequestId = rec.LeaveRequestId,
                                LrsActionerId = rec.ActionerId,
                                LrsActionType = rec.ActionType.ToString(),
                                LeaveRequestStatusAdddate = rec.AddDate,
                                LeaveRequestStatusAddtime = rec.AddTime,
                                LeaveRequestStatusAddopr = rec.AddOpr,
                                LeaveRequestStatusChgdate = rec.AddDate,
                                LeaveRequestStatusChgtime = rec.AddTime,
                                LeaveRequestStatusChgopr = rec.AddOpr
                            }).ToList()
                        )));

                // To Do: Comments
                #endregion

                var expected = await getExpected();
                var actual = await getActual();

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.PerLeaveId, actual.PerLeaveId);
                Assert.AreEqual(expected.EmployeeId, actual.EmployeeId);
                Assert.AreEqual(expected.StartDate, actual.StartDate);
                Assert.AreEqual(expected.EndDate, actual.EndDate);
                Assert.AreEqual(expected.ApproverId, actual.ApproverId);
                Assert.IsTrue(string.IsNullOrWhiteSpace(actual.ApproverName));
                Assert.AreEqual(expected.Status, actual.Status);

                CollectionAssert.AreEqual(expected.LeaveRequestDetails, actual.LeaveRequestDetails);
            }

            [TestCleanup]
            public void CleanUp()
            {
                testData = null;
                repositoryUnderTest = null;
            }
        }

        [TestClass]
        public class CreateLeaveRequestAsyncTests : EmployeeLeaveRequestRepositoryTests
        {
            public LeaveRequest inputLeaveRequest;

            public async Task<LeaveRequest> createActual()
            {
                return await repositoryUnderTest.CreateLeaveRequestAsync(inputLeaveRequest);
            }

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestRepositoryTestsInitialize();
                inputLeaveRequest = new LeaveRequest(null, "895", "0011560",
                    DateTime.Today, DateTime.Today, "0010351",
                    "", LeaveStatusAction.Draft,
                    new List<LeaveRequestDetail>()
                    {
                        new LeaveRequestDetail(null, null, DateTime.Today, 8.00m, false)
                    },
                    new List<LeaveRequestComment>() { });
            }

            [TestCleanup]
            public void CleanUp()
            {
                testData = null;
                repositoryUnderTest = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullLeaveRequestTest()
            {
                try
                {
                    inputLeaveRequest = null;
                    await createActual();

                }
                catch (ArgumentNullException ane)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ane;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CtxResponseIsNullTest()
            {
                try
                {
                    transManagerMock.Setup(t => t.ExecuteAsync<Transactions.CreateLeaveRequestRequest, Transactions.CreateLeaveRequestResponse>(It.IsAny<Transactions.CreateLeaveRequestRequest>()))
                        .Returns<Transactions.CreateLeaveRequestRequest>(req => Task.FromResult<Transactions.CreateLeaveRequestResponse>(null));

                    await createActual();
                }
                catch (ApplicationException ane)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ane;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CtxResponseContainsAnErrorMessageTest()
            {
                try
                {
                    transManagerMock.Setup(t => t.ExecuteAsync<Transactions.CreateLeaveRequestRequest, Transactions.CreateLeaveRequestResponse>(It.IsAny<Transactions.CreateLeaveRequestRequest>()))
                        .Returns<Transactions.CreateLeaveRequestRequest>(req => Task.FromResult(new Transactions.CreateLeaveRequestResponse() { ErrorMessage = "This is an error" }));

                    await createActual();
                }
                catch (ApplicationException ane)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ane;
                }
            }

            [TestMethod]
            public async Task CreateLeaveRequestSuccessfulTest()
            {
                newlyCreatedLeaveRequestId = TestEmployeeLeaveRequestRepository.randomLeaveRequestId;

                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.LeaveRequest>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((id, b) =>
                Task.FromResult(testData.leaveRequestRecords
                    .Where(lr => lr.Id == newlyCreatedLeaveRequestId)
                    .Select(lr => new DataContracts.LeaveRequest()
                    {
                        Recordkey = lr.Id,
                        LrPerleaveId = lr.PerLeaveId,
                        LrEmployeeId = lr.EmployeeId,
                        LrStartDate = lr.StartDate,
                        LrEndDate = lr.EndDate,
                        LrApproverId = lr.ApproverId,
                        LrApproverName = lr.ApproverName
                    }).FirstOrDefault())
                    );

                #region LEAVE.REQUEST.DETAIL
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", It.IsAny<string>()))
                .ReturnsAsync(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == newlyCreatedLeaveRequestId)
                       .Select(rec => rec.Id).ToArray());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestDetail>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords == null ? null :
                    new Collection<DataContracts.LeaveRequestDetail>(
                        TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == newlyCreatedLeaveRequestId)
                           .Select(rec => new DataContracts.LeaveRequestDetail()
                           {
                               Recordkey = rec.Id,
                               LrdLeaveRequestId = rec.LeaveRequestId,
                               LrdLeaveDate = rec.LeaveDate,
                               LrdLeaveHours = rec.LeaveHours
                           }).ToList()
                        )));
                #endregion

                #region LEAVE.REQUEST.STATUS
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.STATUS", string.Format("WITH LRS.LEAVE.REQUEST.ID EQ '{0}'", newlyCreatedLeaveRequestId)))
                   .ReturnsAsync(testData.leaveRequestStatusRecords.Where(lrs => lrs.LeaveRequestId == newlyCreatedLeaveRequestId).Select(rec => rec.Id).ToArray());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestStatus>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testData.leaveRequestStatusRecords == null ? null :
                    new Collection<DataContracts.LeaveRequestStatus>(
                        testData.leaveRequestStatusRecords.Where(lrs => lrs.LeaveRequestId == newlyCreatedLeaveRequestId)
                            .Select(rec => new DataContracts.LeaveRequestStatus()
                            {
                                Recordkey = rec.Id,
                                LrsLeaveRequestId = rec.LeaveRequestId,
                                LrsActionerId = rec.ActionerId,
                                LrsActionType = rec.ActionType.ToString(),
                                LeaveRequestStatusAdddate = rec.AddDate,
                                LeaveRequestStatusAddtime = rec.AddTime,
                                LeaveRequestStatusAddopr = rec.AddOpr,
                                LeaveRequestStatusChgdate = rec.AddDate,
                                LeaveRequestStatusChgtime = rec.AddTime,
                                LeaveRequestStatusChgopr = rec.AddOpr
                            }).ToList()
                        )));

                // To Do: Comments
                #endregion             

                var newlyCreatedLeaveRequest = await createActual();

                Assert.AreEqual(inputLeaveRequest.PerLeaveId, newlyCreatedLeaveRequest.PerLeaveId);
                Assert.AreEqual(inputLeaveRequest.EmployeeId, newlyCreatedLeaveRequest.EmployeeId);
                Assert.AreEqual(inputLeaveRequest.StartDate, newlyCreatedLeaveRequest.StartDate);
                Assert.AreEqual(inputLeaveRequest.EndDate, newlyCreatedLeaveRequest.EndDate);
                Assert.AreEqual(inputLeaveRequest.ApproverId, newlyCreatedLeaveRequest.ApproverId);
                Assert.AreEqual(inputLeaveRequest.ApproverName, newlyCreatedLeaveRequest.ApproverName);
                Assert.AreEqual(inputLeaveRequest.Status, newlyCreatedLeaveRequest.Status);
                Assert.AreEqual(inputLeaveRequest.LeaveRequestDetails.Count, newlyCreatedLeaveRequest.LeaveRequestDetails.Count);
                Assert.AreEqual(inputLeaveRequest.LeaveRequestComments.Count, newlyCreatedLeaveRequest.LeaveRequestComments.Count);

                for (int i = 0; i < newlyCreatedLeaveRequest.LeaveRequestDetails.Count; i++)
                {
                    Assert.AreEqual(inputLeaveRequest.LeaveRequestDetails[i].LeaveDate, newlyCreatedLeaveRequest.LeaveRequestDetails[i].LeaveDate);
                    Assert.AreEqual(inputLeaveRequest.LeaveRequestDetails[i].LeaveHours, newlyCreatedLeaveRequest.LeaveRequestDetails[i].LeaveHours);
                }
            }
        }

        [TestClass]
        public class CreateLeaveRequestStatusAsyncTests : EmployeeLeaveRequestRepositoryTests
        {
            public LeaveRequestStatus inputLeaveRequestStatus;

            public async Task<LeaveRequestStatus> createActual()
            {
                return await repositoryUnderTest.CreateLeaveRequestStatusAsync(inputLeaveRequestStatus);
            }

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestRepositoryTestsInitialize();
                inputLeaveRequestStatus = new LeaveRequestStatus(null, "10", LeaveStatusAction.Submitted, "0011560");
            }

            [TestCleanup]
            public void CleanUp()
            {
                testData = null;
                repositoryUnderTest = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullLeaveRequestStatusTest()
            {
                try
                {
                    inputLeaveRequestStatus = null;
                    await createActual();

                }
                catch (ArgumentNullException ane)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ane;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullLeaveRequestIdTest()
            {
                try
                {
                    inputLeaveRequestStatus = new LeaveRequestStatus(null, null, LeaveStatusAction.Submitted, "0011560");
                    await createActual();

                }
                catch (ArgumentNullException ane)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ane;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task EmptyLeaveRequestIdTest()
            {
                try
                {
                    inputLeaveRequestStatus = new LeaveRequestStatus(null, "", LeaveStatusAction.Submitted, "0011560");
                    await createActual();

                }
                catch (ArgumentNullException ane)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ane;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CtxResponseIsNullTest()
            {
                try
                {
                    transManagerMock.Setup(t => t.ExecuteAsync<Transactions.CreateLeaveRequestStatusRequest, Transactions.CreateLeaveRequestStatusResponse>(It.IsAny<Transactions.CreateLeaveRequestStatusRequest>()))
                        .Returns<Transactions.CreateLeaveRequestStatusRequest>(req => Task.FromResult<Transactions.CreateLeaveRequestStatusResponse>(null));

                    await createActual();
                }
                catch (ApplicationException ane)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ane;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CtxResponseContainsAnErrorMessageTest()
            {
                try
                {
                    transManagerMock.Setup(t => t.ExecuteAsync<Transactions.CreateLeaveRequestStatusRequest, Transactions.CreateLeaveRequestStatusResponse>(It.IsAny<Transactions.CreateLeaveRequestStatusRequest>()))
                        .Returns<Transactions.CreateLeaveRequestStatusRequest>(req => Task.FromResult(new Transactions.CreateLeaveRequestStatusResponse() { ErrorMessage = "This is an error" }));

                    await createActual();
                }
                catch (ApplicationException ane)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ane;
                }
            }

            [TestMethod]
            public async Task CreateLeaveRequestStatusSuccessfulTest()
            {
                var newlyCreatedLeaveRequest = await createActual();

                Assert.AreEqual(inputLeaveRequestStatus.LeaveRequestId, newlyCreatedLeaveRequest.LeaveRequestId);
                Assert.AreEqual(inputLeaveRequestStatus.ActionerId, newlyCreatedLeaveRequest.ActionerId);
                Assert.AreEqual(inputLeaveRequestStatus.ActionType, newlyCreatedLeaveRequest.ActionType);
            }
        }

        [TestClass]
        public class UpdateLeaveRequestAsyncTests
            : EmployeeLeaveRequestRepositoryTests
        {
            public LeaveRequestHelper inputLeaveRequestHelper;

            private bool CompareUpdateRequest(UpdateLeaveRequestRequest req, LeaveRequest lr)
            {
                if
                    (
                    req.LeaveRequestId == lr.Id &&
                    req.LrApproverId == lr.ApproverId &&
                    req.LrApproverName == lr.ApproverName &&
                    req.LrEmployeeId == lr.EmployeeId &&
                    req.LrPerleaveId == lr.PerLeaveId &&
                    req.LrStartDate == lr.StartDate &&
                    req.LrEndDate == lr.EndDate
                    )
                    return true;
                else
                    return false;
            }

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestRepositoryTestsInitialize();

                inputLeaveRequestHelper = new LeaveRequestHelper(new LeaveRequest("2", "697", "0011560", new DateTime(2019, 04, 05),
                    new DateTime(2019, 04, 05), "0010351", "", LeaveStatusAction.Submitted,
                    new List<LeaveRequestDetail>() { new LeaveRequestDetail("961", "2", new DateTime(2019, 04, 05), 8.00m, false) },
                    new List<LeaveRequestComment>() { }));

                var leaveRequestDetailsTOBeUpdated = new LeaveRequestDetail("961", "2", new DateTime(2019, 04, 05), 12.00m, false);
                inputLeaveRequestHelper.LeaveRequestDetailsToUpdate.Add(leaveRequestDetailsTOBeUpdated);
            }

            [TestCleanup]
            public void CleanUp()
            {
                base.testData = null;
                base.repositoryUnderTest = null;
                inputLeaveRequestHelper = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RepositoryCalledWithNullLeaveRequestHelperTest()
            {
                await repositoryUnderTest.UpdateLeaveRequestAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task LeaveRequestObjectOfLeaveRequestHelperIsNullTest()
            {
                inputLeaveRequestHelper.LeaveRequest = null;
                await repositoryUnderTest.UpdateLeaveRequestAsync(inputLeaveRequestHelper);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CtxReturnedNullTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<UpdateLeaveRequestRequest, UpdateLeaveRequestResponse>(It.IsAny<UpdateLeaveRequestRequest>()))
                      .Returns<UpdateLeaveRequestRequest>(req => Task.FromResult<UpdateLeaveRequestResponse>(null));

                await repositoryUnderTest.UpdateLeaveRequestAsync(inputLeaveRequestHelper);
            }

            [TestMethod]
            [ExpectedException(typeof(RecordLockException))]
            public async Task CtxThrowsConflictErrorMessageTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<UpdateLeaveRequestRequest, UpdateLeaveRequestResponse>(It.IsAny<UpdateLeaveRequestRequest>()))
                                          .Returns<UpdateLeaveRequestRequest>(req => Task.FromResult(new UpdateLeaveRequestResponse() { ErrorMessage = "CONFLICT: This is a conflict" }));

                await repositoryUnderTest.UpdateLeaveRequestAsync(inputLeaveRequestHelper);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CtxThrowsOtherErrorMessageTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<UpdateLeaveRequestRequest, UpdateLeaveRequestResponse>(It.IsAny<UpdateLeaveRequestRequest>()))
                                         .Returns<UpdateLeaveRequestRequest>(req => Task.FromResult(new UpdateLeaveRequestResponse() { ErrorMessage = "This is an error message" }));

                await repositoryUnderTest.UpdateLeaveRequestAsync(inputLeaveRequestHelper);
            }

            [TestMethod]
            public async Task SuccessfulUpdateTest()
            {

                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.LeaveRequest>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((id, b) =>
                Task.FromResult(testData.leaveRequestRecords
                    .Where(lr => lr.Id == inputLeaveRequestHelper.LeaveRequest.Id)
                    .Select(lr => new DataContracts.LeaveRequest()
                    {
                        Recordkey = lr.Id,
                        LrPerleaveId = lr.PerLeaveId,
                        LrEmployeeId = lr.EmployeeId,
                        LrStartDate = lr.StartDate,
                        LrEndDate = lr.EndDate,
                        LrApproverId = lr.ApproverId,
                        LrApproverName = lr.ApproverName
                    }).FirstOrDefault())
                    );

                #region LEAVE.REQUEST.DETAIL
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", It.IsAny<string>()))
                .ReturnsAsync(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == inputLeaveRequestHelper.LeaveRequest.Id)
                       .Select(rec => rec.Id).ToArray());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestDetail>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords == null ? null :
                    new Collection<DataContracts.LeaveRequestDetail>(
                        TestEmployeeLeaveRequestRepository.leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == inputLeaveRequestHelper.LeaveRequest.Id)
                           .Select(rec => new DataContracts.LeaveRequestDetail()
                           {
                               Recordkey = rec.Id,
                               LrdLeaveRequestId = rec.LeaveRequestId,
                               LrdLeaveDate = rec.LeaveDate,
                               LrdLeaveHours = rec.LeaveHours
                           }).ToList()
                        )));
                #endregion

                #region LEAVE.REQUEST.STATUS
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.STATUS", string.Format("WITH LRS.LEAVE.REQUEST.ID EQ '{0}'", inputLeaveRequestHelper.LeaveRequest.Id)))
                   .ReturnsAsync(testData.leaveRequestStatusRecords.Where(lrs => lrs.LeaveRequestId == inputLeaveRequestHelper.LeaveRequest.Id).Select(rec => rec.Id).ToArray());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequestStatus>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testData.leaveRequestStatusRecords == null ? null :
                    new Collection<DataContracts.LeaveRequestStatus>(
                        testData.leaveRequestStatusRecords.Where(lrs => lrs.LeaveRequestId == inputLeaveRequestHelper.LeaveRequest.Id)
                            .Select(rec => new DataContracts.LeaveRequestStatus()
                            {
                                Recordkey = rec.Id,
                                LrsLeaveRequestId = rec.LeaveRequestId,
                                LrsActionerId = rec.ActionerId,
                                LrsActionType = rec.ActionType.ToString(),
                                LeaveRequestStatusAdddate = rec.AddDate,
                                LeaveRequestStatusAddtime = rec.AddTime,
                                LeaveRequestStatusAddopr = rec.AddOpr,
                                LeaveRequestStatusChgdate = rec.AddDate,
                                LeaveRequestStatusChgtime = rec.AddTime,
                                LeaveRequestStatusChgopr = rec.AddOpr
                            }).ToList()
                        )));

                // To Do: Comments
                #endregion             

                await repositoryUnderTest.UpdateLeaveRequestAsync(inputLeaveRequestHelper);
                transManagerMock.Verify(t => t.ExecuteAsync<UpdateLeaveRequestRequest, UpdateLeaveRequestResponse>(It.Is<UpdateLeaveRequestRequest>(r => this.CompareUpdateRequest(r, inputLeaveRequestHelper.LeaveRequest))));
            }
        }


        [TestClass]
        public class GetLeaveRequestsForTimeEntryAsyncTests : EmployeeLeaveRequestRepositoryTests
        {
            public async Task<IEnumerable<LeaveRequest>> getExpected()
            {
                return await testData.GetLeaveRequestsForTimeEntryAsync(startDate, endDate, employeeIds);
            }

            public async Task<IEnumerable<LeaveRequest>> getActual()
            {
                return await repositoryUnderTest.GetLeaveRequestsForTimeEntryAsync(startDate, endDate, employeeIds);
            }

            [TestInitialize]
            public void Initialize()
            {
                EmployeeLeaveRequestRepositoryTestsInitialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetLeaveRequestsForTimeEntryAsync_NullEmployeeIdsTest()
            {
                employeeIds = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetLeaveRequestsForTimeEntryAsync_EmptyEmployeeIdsTest()
            {
                employeeIds = new List<string>();
                await getActual();
            }


            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetLeaveRequestsForTimeEntryAsync_NoLeaveRequestKeysTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST", "WITH LR.EMPLOYEE.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
               .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                            Task.FromResult<string[]>(
                                null
                            ));

                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetLeaveRequestsForTimeEntryAsync_NoLeaveRequestDetailKeysTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.DETAIL", "WITH LRD.LEAVE.REQUEST.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                            Task.FromResult<string[]>(
                                null
                            ));

                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetLeaveRequestsForTimeEntryAsync_NoLeaveRequestStatusKeysTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST.STATUS", "WITH LRS.LEAVE.REQUEST.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                            Task.FromResult<string[]>(
                                null
                            ));

                await getActual();
            }

            [TestMethod]
            public async Task GetLeaveRequestsForTimeEntryAsync_ExpectedEqualsActualTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LEAVE.REQUEST", "WITH LR.EMPLOYEE.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                Task.FromResult((testData.leaveRequestsForTimeEntry == null) ? null :
                    testData.leaveRequestsForTimeEntry
                    .Select(rec => rec.Id).ToArray()
            ));

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.LeaveRequest>(It.IsAny<string[]>(), It.IsAny<bool>()))
               .Returns<string[], bool>((ids, b) => Task.FromResult(testData.leaveRequestsForTimeEntry == null ? null :
                   new Collection<DataContracts.LeaveRequest>(
                       testData.leaveRequestsForTimeEntry
                       .Where(rec => ids.Contains(rec.Id))
                       .Select(rec => new DataContracts.LeaveRequest()
                       {
                           Recordkey = rec.Id,
                           LrPerleaveId = rec.PerLeaveId,
                           LrEmployeeId = rec.EmployeeId,
                           LrStartDate = rec.StartDate,
                           LrEndDate = rec.EndDate,
                           LrApproverId = rec.ApproverId,
                           LrApproverName = rec.ApproverName
                       }).ToList()
                   )));

                var expected = await getExpected();
                var actual = await getActual();

                Assert.AreEqual(expected.Count(), actual.Count());
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.AreEqual(expected.ElementAt(i).Id, actual.ElementAt(i).Id);
                    Assert.AreEqual(expected.ElementAt(i).PerLeaveId, actual.ElementAt(i).PerLeaveId);
                    Assert.AreEqual(expected.ElementAt(i).EmployeeId, actual.ElementAt(i).EmployeeId);
                    Assert.AreEqual(expected.ElementAt(i).StartDate, actual.ElementAt(i).StartDate);
                    Assert.AreEqual(expected.ElementAt(i).EndDate, actual.ElementAt(i).EndDate);
                    Assert.AreEqual(expected.ElementAt(i).Status, actual.ElementAt(i).Status);
                    Assert.AreEqual(expected.ElementAt(i).ApproverId, actual.ElementAt(i).ApproverId);
                    Assert.IsTrue(string.IsNullOrWhiteSpace(actual.ElementAt(i).ApproverName));

                    CollectionAssert.AreEqual(expected.ElementAt(i).LeaveRequestDetails, actual.ElementAt(i).LeaveRequestDetails);
                }
            }

            [TestCleanup]
            public void CleanUp()
            {
                testData = null;
                repositoryUnderTest = null;
            }
        }

        #region Helpers
        /// <summary>
        /// Builds LeaveRequestDetails using CreateLeaveRequestDetails
        /// </summary>
        /// <param name="createLeaveRequestDetails"></param>
        /// <returns></returns>
        private List<LeaveRequestDetail> BuildLeaveRequestDetailsToBeCreated(List<CreateLeaveRequestDetails> createLeaveRequestDetails)
        {
            List<LeaveRequestDetail> leaveRequestDetailsToBeCreated = new List<LeaveRequestDetail>();

            if (createLeaveRequestDetails != null && createLeaveRequestDetails.Any())
            {
                foreach (var lrd in createLeaveRequestDetails)
                {
                    leaveRequestDetailsToBeCreated.Add(new LeaveRequestDetail(null, null, lrd.LrdLeaveDate.Value, lrd.LrdHours, false));
                }
            }

            return leaveRequestDetailsToBeCreated;
        }
        #endregion
    }
}
