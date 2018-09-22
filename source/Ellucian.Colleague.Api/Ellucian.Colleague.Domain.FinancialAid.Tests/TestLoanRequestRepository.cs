/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestLoanRequestRepository : ILoanRequestRepository
    {
        public class NewLoanRequestData
        {
            public string id;
            public string studentId;
            public string awardYear;
            public int totalRequestAmount;
            public List<LoanRequestPeriod> loanRequestPeriods;
            public DateTime requestDate;
            public string studentComments;
            public string assignedToId;
            public string statusCode;
            public DateTime statusDate;
            public string modifierId;
            public string modifierComments;
        }

        public List<NewLoanRequestData> NewLoanRequestList = new List<NewLoanRequestData>()
        {
            new NewLoanRequestData()
            {
                id = "1",
                studentId = "0003914",
                awardYear = "2014",
                totalRequestAmount = 49837,
                loanRequestPeriods = new List<LoanRequestPeriod>(){
                    new LoanRequestPeriod("13/FA", 40000),
                    new LoanRequestPeriod("14/SP", 9837) 
                },
                requestDate = DateTime.Today,
                studentComments = "This is a comment",
                assignedToId = "0010374",
                statusCode = "P",
                statusDate = DateTime.Today,
                modifierId = "",
                modifierComments = ""
            },
            new NewLoanRequestData()
            {
                id = "2",
                studentId = "0003914",
                awardYear = "2013",
                totalRequestAmount = 2344,
                loanRequestPeriods = new List<LoanRequestPeriod>(){
                    new LoanRequestPeriod("13/SH", 1172),
                    new LoanRequestPeriod("13/SP", 1172) 
                },
                requestDate = new DateTime(2013, 07,15),
                studentComments = "This is a comment",
                assignedToId = "0010374",
                statusCode = "A",
                statusDate = DateTime.Today,
                modifierId = "",
                modifierComments = ""
            },
            new NewLoanRequestData()
            {
                id = "3",
                studentId = "0003915",
                awardYear = "2014",
                totalRequestAmount = 8494,
                loanRequestPeriods = new List<LoanRequestPeriod>(){
                    new LoanRequestPeriod("13/FA", 2831),
                    new LoanRequestPeriod("14/SP", 2831),
                    new LoanRequestPeriod("13/WI", 2832)
                },
                requestDate = DateTime.Today,
                studentComments = "This is a comment",
                assignedToId = "0010374",
                statusCode = "P",
                statusDate = DateTime.Today,
                modifierId = "",
                modifierComments = ""
            },
        };

        public Task<LoanRequest> GetLoanRequestAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var loanRequestRecord = NewLoanRequestList.FirstOrDefault(l => l.id == id);
            if (loanRequestRecord == null)
            {
                throw new KeyNotFoundException(string.Format("NewLoanRequest record {0} does not exist.", id));
            }

            LoanRequestStatus status;
            switch (loanRequestRecord.statusCode.ToUpper())
            {
                case "A":
                    status = LoanRequestStatus.Accepted;
                    break;
                case "P":
                    status = LoanRequestStatus.Pending;
                    break;
                case "R":
                    status = LoanRequestStatus.Rejected;
                    break;
                default:
                    status = LoanRequestStatus.Pending;
                    break;
            }

            var loanRequest = new LoanRequest(loanRequestRecord.id,
                loanRequestRecord.studentId,
                loanRequestRecord.awardYear,
                loanRequestRecord.requestDate,
                loanRequestRecord.totalRequestAmount,
                loanRequestRecord.assignedToId,
                status,
                loanRequestRecord.statusDate,
                loanRequestRecord.modifierId);

            loanRequest.StudentComments = loanRequestRecord.studentComments;
            loanRequest.ModifierComments = loanRequestRecord.modifierComments;

            foreach (var period in loanRequestRecord.loanRequestPeriods)
            {
                loanRequest.AddLoanPeriod(period.Code, period.LoanAmount);
            }

            return Task.FromResult(loanRequest);
        }

        public Task<LoanRequest> CreateLoanRequestAsync(LoanRequest loanRequest, StudentAwardYear studentAwardYear)
        {
            if (loanRequest == null)
            {
                throw new ArgumentNullException("loanRequest");
            }

            var duplicateLoanRequest = NewLoanRequestList.FirstOrDefault(l => l.studentId == loanRequest.StudentId && l.awardYear == loanRequest.AwardYear);
            if (duplicateLoanRequest != null)
            {
                throw new ExistingResourceException(string.Format("Loan Request exists for student {0} and award year {1}", loanRequest.StudentId, loanRequest.AwardYear), duplicateLoanRequest.id);
            }

            var newLoanRequestRecordId = int.Parse(NewLoanRequestList.Select(l => l.id).OrderBy(id => id).Last()) + 1;
            var newLoanRequestRecord = new NewLoanRequestData()
            {
                id = newLoanRequestRecordId.ToString(),
                studentId = loanRequest.StudentId,
                awardYear = loanRequest.AwardYear,
                requestDate = loanRequest.RequestDate,
                totalRequestAmount = loanRequest.TotalRequestAmount,
                loanRequestPeriods = new List<LoanRequestPeriod>(),
                assignedToId = loanRequest.AssignedToId,
                statusCode = loanRequest.Status.ToString()[0].ToString(),
                statusDate = loanRequest.StatusDate,
                studentComments = loanRequest.StudentComments,
            };

            newLoanRequestRecord.loanRequestPeriods.AddRange(loanRequest.LoanRequestPeriods);

            NewLoanRequestList.Add(newLoanRequestRecord);

            return GetLoanRequestAsync(newLoanRequestRecordId.ToString());
        }
    }
}
