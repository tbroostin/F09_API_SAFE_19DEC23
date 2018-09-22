/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    [TestClass]
    public class LoanRequestEntityAdapterTests
    {
        private Dtos.FinancialAid.LoanRequest inputLoanRequest;

        private Domain.FinancialAid.Entities.LoanRequest expectedLoanRequest;
        private Domain.FinancialAid.Entities.LoanRequest actualLoanRequest;

        private LoanRequestDtoToEntityAdapter loanRequestEntityAdapter;

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            inputLoanRequest = new Dtos.FinancialAid.LoanRequest()
            {
                Id = "1231",
                StudentId = "12491414",
                AwardYear = "2014",
                RequestDate = DateTime.Today,
                TotalRequestAmount = 23232,
                LoanRequestPeriods = new List<LoanRequestPeriod>()
                {
                    new LoanRequestPeriod(){
                        Code = "13/FA",
                        LoanAmount = 2265
                    },

                    new LoanRequestPeriod(){
                        Code = "14/SP",
                        LoanAmount = 2265
                    }
                },
                AssignedToId = "23232",
                Status = Dtos.FinancialAid.LoanRequestStatus.Rejected,
                StatusDate = DateTime.Today,
                StudentComments = "Comment"
            };

            expectedLoanRequest = new Domain.FinancialAid.Entities.LoanRequest(
                inputLoanRequest.Id,
                inputLoanRequest.StudentId,
                inputLoanRequest.AwardYear,
                inputLoanRequest.RequestDate,
                inputLoanRequest.TotalRequestAmount,
                inputLoanRequest.AssignedToId,
                (Domain.FinancialAid.Entities.LoanRequestStatus)inputLoanRequest.Status,
                inputLoanRequest.StatusDate,
                string.Empty);
            expectedLoanRequest.StudentComments = inputLoanRequest.StudentComments;

            foreach(var period in inputLoanRequest.LoanRequestPeriods){
                expectedLoanRequest.AddLoanPeriod(period.Code, period.LoanAmount);
            }

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            loanRequestEntityAdapter = new LoanRequestDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public void ExpectedEqualsActualTest()
        {
            actualLoanRequest = loanRequestEntityAdapter.MapToType(inputLoanRequest);
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
        public void NullInputThrowsExceptionTest()
        {
            loanRequestEntityAdapter.MapToType(null);
        }

        [TestMethod]
        public void LoanRequestStatusMap_AccpetedTest()
        {
            inputLoanRequest.Status = Dtos.FinancialAid.LoanRequestStatus.Accepted;
            actualLoanRequest = loanRequestEntityAdapter.MapToType(inputLoanRequest);

            Assert.AreEqual(Domain.FinancialAid.Entities.LoanRequestStatus.Accepted, actualLoanRequest.Status);
        }

        [TestMethod]
        public void LoanRequestStatusMap_PendingTest()
        {
            inputLoanRequest.Status = Dtos.FinancialAid.LoanRequestStatus.Pending;
            actualLoanRequest = loanRequestEntityAdapter.MapToType(inputLoanRequest);

            Assert.AreEqual(Domain.FinancialAid.Entities.LoanRequestStatus.Pending, actualLoanRequest.Status);
        }

        [TestMethod]
        public void LoanRequestStatusMap_RejectedTest()
        {
            inputLoanRequest.Status = Dtos.FinancialAid.LoanRequestStatus.Rejected;
            actualLoanRequest = loanRequestEntityAdapter.MapToType(inputLoanRequest);

            Assert.AreEqual(Domain.FinancialAid.Entities.LoanRequestStatus.Rejected, actualLoanRequest.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyLoanRequestPeriodListThrowsExceptionTest()
        {
            inputLoanRequest.LoanRequestPeriods = new List<LoanRequestPeriod>();
            actualLoanRequest = loanRequestEntityAdapter.MapToType(inputLoanRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NullLoanRequestPeriodListThrowsExceptionTest()
        {
            inputLoanRequest.LoanRequestPeriods = null;
            actualLoanRequest = loanRequestEntityAdapter.MapToType(inputLoanRequest);
        }
    }
}
