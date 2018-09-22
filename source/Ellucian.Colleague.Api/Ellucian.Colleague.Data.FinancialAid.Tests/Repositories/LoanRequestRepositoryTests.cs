/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class LoanRequestRepositoryTests : BaseRepositorySetup
    {
        public List<NewLoanRequest> newLoanRequestDataContracts;
        public NewLoanRequest inputLoanRequestDataContract;

        public LoanRequest expectedLoanRequest;
        public LoanRequest actualLoanRequest;

        public TestStudentAwardYearRepository studentAwardYearRepository;
        public TestFinancialAidOfficeRepository financialAidOfficeRepository;

        public TestLoanRequestRepository expectedRepository;
        public LoanRequestRepository actualRepository;

        public CreateLoanRequestResponse createLoanRequestResponseTransaction;
        public CreateLoanRequestRequest actualCreateLoanRequestRequestTransaction;

        public string inputId;

        public void InitializeBase()
        {
            MockInitialize();

            expectedRepository = new TestLoanRequestRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            financialAidOfficeRepository = new TestFinancialAidOfficeRepository();

            newLoanRequestDataContracts = BuildNewLoanRequestDataContracts(expectedRepository.NewLoanRequestList);
            inputId = newLoanRequestDataContracts.First().Recordkey;

            expectedLoanRequest = expectedRepository.GetLoanRequestAsync(inputId).Result;

            createLoanRequestResponseTransaction = new CreateLoanRequestResponse();

            actualRepository = BuildRepository();
        }

        private LoanRequestRepository BuildRepository()
        {
            inputLoanRequestDataContract = newLoanRequestDataContracts.FirstOrDefault(l => l.Recordkey == inputId);
            dataReaderMock.Setup(d => d.ReadRecordAsync<NewLoanRequest>(inputId, true)).ReturnsAsync(inputLoanRequestDataContract);

            transManagerMock.Setup(t => t.ExecuteAsync<CreateLoanRequestRequest, CreateLoanRequestResponse>(It.IsAny<CreateLoanRequestRequest>()))
                .ReturnsAsync(createLoanRequestResponseTransaction)
                .Callback<CreateLoanRequestRequest>(r => actualCreateLoanRequestRequestTransaction = r);

            return new LoanRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        private List<NewLoanRequest> BuildNewLoanRequestDataContracts(List<TestLoanRequestRepository.NewLoanRequestData> newLoanRequestDataList)
        {
            var dataContractList = new List<NewLoanRequest>();
            foreach (var newLoanRequestDataItem in newLoanRequestDataList)
            {
                var dataContract = new NewLoanRequest()
                {
                    Recordkey = newLoanRequestDataItem.id,
                    NlrStudentId = newLoanRequestDataItem.studentId,
                    NlrAwardYear = newLoanRequestDataItem.awardYear,
                    NewLoanRequestAdddate = newLoanRequestDataItem.requestDate,
                    NlrTotalRequestAmount = newLoanRequestDataItem.totalRequestAmount,
                    NlrAwardPeriods = newLoanRequestDataItem.loanRequestPeriods.Select(lrp => lrp.Code).ToList(),
                    NlrAwardPeriodAmounts = newLoanRequestDataItem.loanRequestPeriods.Select(lrp => (int?)lrp.LoanAmount).ToList(),
                    NlrAssignedToId = newLoanRequestDataItem.assignedToId,
                    NlrCurrentStatus = newLoanRequestDataItem.statusCode,
                    NlrCurrentStatusDate = newLoanRequestDataItem.statusDate,
                    NlrModifierId = newLoanRequestDataItem.modifierId,
                    NlrStudentComments = newLoanRequestDataItem.studentComments,
                    NlrModifierComments = newLoanRequestDataItem.modifierComments
                };
                dataContract.buildAssociations();
                dataContractList.Add(dataContract);
            }
            return dataContractList;
        }

        [TestClass]
        public class GetLoanRequestTests : LoanRequestRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                InitializeBase();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualLoanRequest = await actualRepository.GetLoanRequestAsync(inputId);
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
                Assert.AreEqual(expectedLoanRequest.ModifierId, actualLoanRequest.ModifierId);
                Assert.AreEqual(expectedLoanRequest.ModifierComments, actualLoanRequest.ModifierComments);

                for (var i = 0; i < expectedLoanRequest.LoanRequestPeriods.Count; i++)
                {
                    var expectedLoanRequestPeriod = expectedLoanRequest.LoanRequestPeriods[i];
                    Assert.IsTrue(actualLoanRequest.LoanRequestPeriods.Select(lrp => lrp.Code).Contains(expectedLoanRequestPeriod.Code));
                    Assert.AreEqual(expectedLoanRequestPeriod.LoanAmount, actualLoanRequest.LoanRequestPeriods.First(lrp => lrp.Code == expectedLoanRequestPeriod.Code).LoanAmount);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InputIdIsNullTest()
            {
                await actualRepository.GetLoanRequestAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoDbRecordForInputIdTest()
            {
                await actualRepository.GetLoanRequestAsync("foobar");
            }

            [TestMethod]
            public async Task AcceptedStatusTest()
            {
                inputLoanRequestDataContract.NlrCurrentStatus = "A";
                actualLoanRequest = await actualRepository.GetLoanRequestAsync(inputId);
                Assert.AreEqual(LoanRequestStatus.Accepted, actualLoanRequest.Status);
            }

            [TestMethod]
            public async Task PendingStatusTest()
            {
                inputLoanRequestDataContract.NlrCurrentStatus = "P";
                actualLoanRequest = await actualRepository.GetLoanRequestAsync(inputId);
                Assert.AreEqual(LoanRequestStatus.Pending, actualLoanRequest.Status);
            }

            [TestMethod]
            public async Task RejectedStatusTest()
            {
                inputLoanRequestDataContract.NlrCurrentStatus = "R";
                actualLoanRequest = await actualRepository.GetLoanRequestAsync(inputId);
                Assert.AreEqual(LoanRequestStatus.Rejected, actualLoanRequest.Status);
            }

            [TestMethod]
            public async Task UnknownStatusTest()
            {
                var unknownStatus = "foobar";
                inputLoanRequestDataContract.NlrCurrentStatus = unknownStatus;
                actualLoanRequest = await actualRepository.GetLoanRequestAsync(inputId);
                Assert.AreEqual(LoanRequestStatus.Pending, actualLoanRequest.Status);

                loggerMock.Verify(l => l.Info(string.Format("LoanRequestStatus does not exist for NewLoanRequest record id {0}, status {1}. Setting to Pending.", inputId, unknownStatus)));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ErrorCreatingObjectThrowsExceptionTest()
            {
                inputLoanRequestDataContract.NlrStudentId = string.Empty;
                await actualRepository.GetLoanRequestAsync(inputId);
            }
        }

        [TestClass]
        public class CreateLoanRequestTests : LoanRequestRepositoryTests
        {
            private LoanRequest inputLoanRequest;
            private string studentId;
            private StudentAwardYear inputStudentAwardYear;

            [TestInitialize]
            public void Initialize()
            {
                InitializeBase();
                studentId = "1234567";
                inputLoanRequest = new LoanRequest("-1", studentId, "2014", DateTime.Today, 12345, "1111111", LoanRequestStatus.Pending, DateTime.Today, string.Empty);
                inputLoanRequest.AddLoanPeriod("14/SP", 6172);
                inputLoanRequest.AddLoanPeriod("13/FA", 6173);

                var currentOfficeService = new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices());
                var studentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                inputStudentAwardYear = studentAwardYears.First(y => y.Code == inputLoanRequest.AwardYear);

                expectedLoanRequest = expectedRepository.CreateLoanRequestAsync(inputLoanRequest, inputStudentAwardYear).Result;

                newLoanRequestDataContracts = BuildNewLoanRequestDataContracts(expectedRepository.NewLoanRequestList);

                createLoanRequestResponseTransaction.OutLoanRequestId = expectedLoanRequest.Id;

                inputId = expectedLoanRequest.Id;

                actualRepository = BuildRepository();

            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualLoanRequest = await actualRepository.CreateLoanRequestAsync(inputLoanRequest, inputStudentAwardYear);
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
                Assert.AreEqual(expectedLoanRequest.ModifierId, actualLoanRequest.ModifierId);
                Assert.AreEqual(expectedLoanRequest.ModifierComments, actualLoanRequest.ModifierComments);

                for (var i = 0; i < expectedLoanRequest.LoanRequestPeriods.Count; i++)
                {
                    var expectedLoanRequestPeriod = expectedLoanRequest.LoanRequestPeriods[i];
                    Assert.IsTrue(actualLoanRequest.LoanRequestPeriods.Select(lrp => lrp.Code).Contains(expectedLoanRequestPeriod.Code));
                    Assert.AreEqual(expectedLoanRequestPeriod.LoanAmount, actualLoanRequest.LoanRequestPeriods.First(lrp => lrp.Code == expectedLoanRequestPeriod.Code).LoanAmount);
                }

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullArgumentThrowsExceptionTest()
            {
                await actualRepository.CreateLoanRequestAsync(null, inputStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentAwardYearThrowsExceptionTest()
            {
                await actualRepository.CreateLoanRequestAsync(inputLoanRequest, null);
            }

            [TestMethod]
            public async Task TransactionRequestTest()
            {
                actualLoanRequest = await actualRepository.CreateLoanRequestAsync(inputLoanRequest, inputStudentAwardYear);
                Assert.AreEqual(expectedLoanRequest.StudentId, actualCreateLoanRequestRequestTransaction.StudentId);
                Assert.AreEqual(expectedLoanRequest.AwardYear, actualCreateLoanRequestRequestTransaction.AwardYear);
                Assert.AreEqual(expectedLoanRequest.RequestDate, actualCreateLoanRequestRequestTransaction.RequestDate);
                Assert.AreEqual(expectedLoanRequest.TotalRequestAmount, actualCreateLoanRequestRequestTransaction.TotalRequestAmount);
                //Assert.AreEqual(expectedLoanRequest.LoanRequestPeriods.Count, actualCreateLoanRequestRequestTransaction.LoanRequestPeriods.Count);
                Assert.AreEqual(expectedLoanRequest.LoanRequestPeriods.Count, actualCreateLoanRequestRequestTransaction.LoanAwardPeriods.Count);
                Assert.AreEqual(expectedLoanRequest.AssignedToId, actualCreateLoanRequestRequestTransaction.AssignedToId);
                Assert.AreEqual(expectedLoanRequest.Status.ToString()[0].ToString(), actualCreateLoanRequestRequestTransaction.Status);
                Assert.AreEqual(expectedLoanRequest.StatusDate, actualCreateLoanRequestRequestTransaction.StatusDate);
                Assert.AreEqual(expectedLoanRequest.StudentComments, actualCreateLoanRequestRequestTransaction.StudentComments);
                Assert.AreEqual(inputStudentAwardYear.CurrentConfiguration.NewLoanCommunicationCode, actualCreateLoanRequestRequestTransaction.CmCode);
                Assert.AreEqual(inputStudentAwardYear.CurrentConfiguration.NewLoanCommunicationStatus, actualCreateLoanRequestRequestTransaction.CmStatus);

                for (var i = 0; i < expectedLoanRequest.LoanRequestPeriods.Count; i++)
                {
                    var expectedLoanRequestPeriod = expectedLoanRequest.LoanRequestPeriods[i];
                    var actualLoanRequestPeriod = actualCreateLoanRequestRequestTransaction.LoanAwardPeriods[i];
                    Assert.AreEqual(expectedLoanRequestPeriod.Code, actualLoanRequestPeriod.LoanPeriodIds);
                    Assert.AreEqual(expectedLoanRequestPeriod.LoanAmount, actualLoanRequestPeriod.LoanPeriodAmounts);
                }
            }

            [TestMethod]
            public async Task CommunicationCodeAndStatusAreEmptyIfCurrentConfigurationIsNullTest()
            {
                inputStudentAwardYear = new StudentAwardYear(studentId, inputLoanRequest.AwardYear, new FinancialAidOffice("office"));
                actualLoanRequest = await actualRepository.CreateLoanRequestAsync(inputLoanRequest, inputStudentAwardYear);

                Assert.AreEqual(string.Empty, actualCreateLoanRequestRequestTransaction.CmCode);
                Assert.AreEqual(string.Empty, actualCreateLoanRequestRequestTransaction.CmStatus);
            }

            [TestMethod]
            public async Task ErrorMessageIsLoggedTest()
            {
                var errorMessage = "This is an error";
                createLoanRequestResponseTransaction.ErrorMessage = errorMessage;
                bool exceptionCaught = false;
                try
                {
                    await actualRepository.CreateLoanRequestAsync(inputLoanRequest, inputStudentAwardYear);
                }
                catch (Exception)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(errorMessage));
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingResourceException))]
            public async Task ExistingResourceExceptionTest()
            {
                var errorMessage = "EXISTINGRESOURCE: A resource already exists on the server";
                createLoanRequestResponseTransaction.ErrorMessage = errorMessage;

                await actualRepository.CreateLoanRequestAsync(inputLoanRequest, inputStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(OperationCanceledException))]
            public async Task OperationCanceledExceptionTest()
            {
                var errorMessage = "CONFLICT: Record lock!";
                createLoanRequestResponseTransaction.ErrorMessage = errorMessage;

                await actualRepository.CreateLoanRequestAsync(inputLoanRequest, inputStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GenericExceptionTest()
            {
                var errorMessage = "Some unknown error occurred";
                createLoanRequestResponseTransaction.ErrorMessage = errorMessage;

                await actualRepository.CreateLoanRequestAsync(inputLoanRequest, inputStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task LoanRequestPeriodListNullOrEmptyExceptionTest()
            {
                inputLoanRequest.RemoveLoanPeriod("14/SP");
                inputLoanRequest.RemoveLoanPeriod("13/FA");
                actualLoanRequest = await actualRepository.CreateLoanRequestAsync(inputLoanRequest, inputStudentAwardYear);
            }
        }

    }
}
