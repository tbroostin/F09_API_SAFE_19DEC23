using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class LoanRequestTests
    {
        [TestClass]
        public class LoanRequestConstructorTests
        {
            private string id;
            private string studentId;
            private string awardYear;
            private DateTime requestDate;
            private int totalRequestAmount;
            private List<LoanRequestPeriod> loanRequestPeriods;
            private string studentComments;
            private string assignedToId;
            private LoanRequestStatus status;
            private DateTime statusDate;
            private string modifierId;
            private string modifierComments;

            private LoanRequest loanRequest;

            [TestInitialize]
            public void Initialize()
            {
                id = "1";
                studentId = "0003914";
                awardYear = "2014";
                requestDate = DateTime.Today;
                totalRequestAmount = 56789;
                loanRequestPeriods = new List<LoanRequestPeriod>()
                {
                    new LoanRequestPeriod("13/FA", 2500),
                    new LoanRequestPeriod("14/SP", 3456),
                    new LoanRequestPeriod("14/SH", 567)
                };
                studentComments = "This is my comment";
                assignedToId = "0010743";
                status = LoanRequestStatus.Pending;
                statusDate = DateTime.Today;
                modifierId = "0010743";
                modifierComments = "This are the modifier comments";

                loanRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
                foreach (var period in loanRequestPeriods)
                {
                    loanRequest.AddLoanPeriod(period.Code, (int)period.LoanAmount);
                }

            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                Assert.AreEqual(id, loanRequest.Id);
                Assert.AreEqual(studentId, loanRequest.StudentId);
                Assert.AreEqual(awardYear, loanRequest.AwardYear);
                Assert.AreEqual(requestDate, loanRequest.RequestDate);
                Assert.AreEqual(totalRequestAmount, loanRequest.TotalRequestAmount);
                Assert.AreEqual(loanRequestPeriods.Count, loanRequest.LoanRequestPeriods.Count);
                Assert.AreEqual(assignedToId, loanRequest.AssignedToId);
                Assert.AreEqual(status, loanRequest.Status);
                Assert.AreEqual(statusDate, loanRequest.StatusDate);
                Assert.AreEqual(modifierId, loanRequest.ModifierId);

                //TODO: Olga - change the loop: look up by period code
                for (int i = 0; i < loanRequestPeriods.Count; i++)
                {
                    var expectedLoanRequestPeriod = loanRequestPeriods[i];
                    var actualLoanRequestPeriod = loanRequest.LoanRequestPeriods[i];
                    Assert.AreEqual(expectedLoanRequestPeriod.Code, actualLoanRequestPeriod.Code);
                    Assert.AreEqual(expectedLoanRequestPeriod.LoanAmount, actualLoanRequestPeriod.LoanAmount);
                }
            }

            [TestMethod]
            public void StudentCommentsGetSetTest()
            {
                loanRequest.StudentComments = studentComments;
                Assert.AreEqual(studentComments, loanRequest.StudentComments);
            }

            [TestMethod]
            public void ModifierCommentsGetSetTest()
            {
                loanRequest.ModifierComments = modifierComments;
                Assert.AreEqual(modifierComments, loanRequest.ModifierComments);
            }

            [TestMethod]
            public void AssignedToIdGetSetTest()
            {
                assignedToId = "foobar";
                loanRequest.AssignedToId = assignedToId;
                Assert.AreEqual(assignedToId, loanRequest.AssignedToId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmptyIdThrowsExceptionTest()
            {
                new LoanRequest(null, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmptyStudentIdThrowsExceptionTest()
            {
                new LoanRequest(id, null, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmptyAwardYearThrowsExceptionTest()
            {
                new LoanRequest(id, studentId, null, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void TotalRequestAmountZeroOrLessThrowsExceptionTest()
            {
                new LoanRequest(id, studentId, awardYear, requestDate, 0, assignedToId, status, statusDate, modifierId);
            }
        }

        [TestClass]
        public class UpdateStatusTests
        {
            private string id;
            private string studentId;
            private string awardYear;
            private DateTime requestDate;
            private int totalRequestAmount;
            private List<LoanRequestPeriod> loanRequestPeriods;
            private string assignedToId;
            private LoanRequestStatus status;
            private DateTime statusDate;
            private string modifierId;

            private LoanRequest loanRequest;

            [TestInitialize]
            public void Initialize()
            {
                id = "1";
                studentId = "0003914";
                awardYear = "2014";
                requestDate = new DateTime(2014, 01, 01);
                totalRequestAmount = 56789;
                loanRequestPeriods = new List<LoanRequestPeriod>();
                assignedToId = "0010743";
                status = LoanRequestStatus.Pending;
                statusDate = new DateTime(2014, 01, 01);
                modifierId = "0010743";

                loanRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullModifierIdThrowsExceptionTest()
            {
                loanRequest.UpdateStatus(LoanRequestStatus.Accepted, null);
            }

            [TestMethod]
            public void SameStatusDoesNotUpdate_ReturnFalseTest()
            {
                var result = loanRequest.UpdateStatus(status, "foo");
                Assert.IsFalse(result);
                Assert.AreEqual(status, loanRequest.Status);
                Assert.AreEqual(statusDate, loanRequest.StatusDate);
                Assert.AreEqual(modifierId, loanRequest.ModifierId);
            }

            [TestMethod]
            public void StatusUpdateTest()
            {
                var newStatus = LoanRequestStatus.Accepted;
                var modifier = "0010743";
                var result = loanRequest.UpdateStatus(newStatus, modifier);
                Assert.IsTrue(result);

                Assert.AreEqual(newStatus, loanRequest.Status);
                Assert.AreEqual(DateTime.Today, loanRequest.StatusDate);
                Assert.AreEqual(modifier, loanRequest.ModifierId);
            }
        }

        [TestClass]
        public class EqualsTests
        {
            private string id;
            private string studentId;
            private string awardYear;
            private DateTime requestDate;
            private int totalRequestAmount;
            private Dictionary<string, decimal> loanRequestPeriods;
            private string studentComments;
            private string assignedToId;
            private LoanRequestStatus status;
            private DateTime statusDate;
            private string modifierId;
            private string modifierComments;

            private LoanRequest loanRequest;

            [TestInitialize]
            public void Initialize()
            {
                id = "1";
                studentId = "0003914";
                awardYear = "2014";
                requestDate = DateTime.Today;
                totalRequestAmount = 56789;
                loanRequestPeriods = new Dictionary<string, decimal>();
                studentComments = "This is my comment";
                assignedToId = "0010743";
                status = LoanRequestStatus.Pending;
                statusDate = DateTime.Today;
                modifierId = "0010743";
                modifierComments = "This are the modifier comments";

                loanRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
                loanRequest.StudentComments = studentComments;
                loanRequest.ModifierComments = modifierComments;
            }

            [TestMethod]
            public void SameId_EqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffIdNotEqualTest()
            {
                var testRequest = new LoanRequest("foo", studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
                Assert.AreNotEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffStudentIdEqualTest()
            {
                var testRequest = new LoanRequest(id, "foobar", awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffAwardYearEqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, "foobar", requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffRequestDateEqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, awardYear, new DateTime(2014, 1, 1), totalRequestAmount, assignedToId, status, statusDate, modifierId);
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffTotalRequestAmountEqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, awardYear, requestDate, 1, assignedToId, status, statusDate, modifierId);
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffAssignedToIdEqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, "foobar", status, statusDate, modifierId);
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffStatusEqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, LoanRequestStatus.Rejected, statusDate, modifierId);
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffStatusDateEqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, new DateTime(2014, 1, 1), modifierId);
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffModifierIdEqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, "foobar");
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffStudentCommentsEqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
                testRequest.StudentComments = "foobar";
                Assert.AreEqual(testRequest, loanRequest);
            }

            [TestMethod]
            public void DiffModifierCommentsEqualTest()
            {
                var testRequest = new LoanRequest(id, studentId, awardYear, requestDate, totalRequestAmount, assignedToId, status, statusDate, modifierId);
                testRequest.ModifierComments = "foobar";
                Assert.AreEqual(testRequest, loanRequest);
            }

        }
    }
}
