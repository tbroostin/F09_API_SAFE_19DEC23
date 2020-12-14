// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestApproverRepository : IApproverRepository
    {
        // Create a list of approvals datal contracts.
        public List<Approvals> approvalRecords = new List<Approvals>()
        {
            new Approvals()
            {
                Recordkey = "BOB1",
                ApprvBeginDate = DateTime.Now.AddDays(-100),
                ApprvEndDate = DateTime.Now.AddDays(100),
                ApprvBeMaxAmt = 999999,
                ApprvClasses = new List<string> { "CLS1", "GL1", "EXPRD" },
                ApprvClassesBeginDate = new List<DateTime?> { DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-20), DateTime.Now.AddDays(-20) },
                ApprvClassesEndDate = new List<DateTime?> { DateTime.Now.AddDays(10), DateTime.Now.AddDays(20), DateTime.Now.AddDays(-5) }
            },
            new Approvals()
            {
                Recordkey = "BOB2",
                ApprvBeginDate = DateTime.Now.AddDays(-100),
                ApprvEndDate = DateTime.Now.AddDays(100),
                ApprvBeMaxAmt = 999999,
                ApprvClasses = new List<string> { "CLS1" },
                ApprvClassesBeginDate = new List<DateTime?> { },
                ApprvClassesEndDate = new List<DateTime?> { }
            }
        };

        public List<NextApprover> nextApprovers = new List<NextApprover>() {
             new NextApprover("JHN") { NextApproverPersonId="123" }
        };

        // Create a list of next approver validation response domain entities.
        public List<ApproverValidationResponse> approverValidationResponses = new List<ApproverValidationResponse>()
        {
            new ApproverValidationResponse("BOB1")
            {
                ApproverName = "First Bob",
                IsValid = true,
                ErrorMessage = null
            },
            new ApproverValidationResponse("BOB2")
            {
                ApproverName = "Second Bob",
                IsValid = true,
                ErrorMessage = null
            }
        };
        
        public async Task<ApproverValidationResponse> ValidateApproverAsync(string nextApproverId)
        {
            return await Task.Run(() =>
            {
                return approverValidationResponses.Where(x => x.Id == nextApproverId).FirstOrDefault();
            });
        }

        public async Task<string> GetApproverNameForIdAsync(string approverId)
        {
            return await Task.Run(() =>
            {
                return approverValidationResponses.Where(x => x.Id == approverId).FirstOrDefault().ApproverName;
            });
        }

        public async Task<IEnumerable<NextApprover>> QueryNextApproverByKeywordAsync(string searchCriteria)
        {
            return await Task.Run(() =>
            {
                return nextApprovers;
            });
            
        }
    }
}
