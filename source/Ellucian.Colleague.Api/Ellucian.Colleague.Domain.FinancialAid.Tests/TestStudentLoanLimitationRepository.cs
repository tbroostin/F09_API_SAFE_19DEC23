/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestStudentLoanLimitationRepository : IStudentLoanLimitationRepository
    {
        public string studentId = "0003914";

        public class TestLoanLimitation
        {
            public string AwardYear;
            public int SubMaxAmount;
            public int UnsubMaxAmount;
            public int GradPlusMaxAmount;
        }

        public List<TestLoanLimitation> testLoanLimitsData = new List<TestLoanLimitation>()
        {
            new TestLoanLimitation()
            {
                AwardYear = "2012",
                SubMaxAmount = 99999,
                UnsubMaxAmount = 99999,
                GradPlusMaxAmount = 99999,
            },

            new TestLoanLimitation()
            {
                AwardYear = "2013",
                SubMaxAmount = 99999,
                UnsubMaxAmount = 99999,
                GradPlusMaxAmount = 99999,
            },

            new TestLoanLimitation()
            {
                AwardYear = "2014",
                SubMaxAmount = 99999,
                UnsubMaxAmount = 99999,
                GradPlusMaxAmount = 99999,
            },
            new TestLoanLimitation()
            {
                AwardYear = "2015",
                SubMaxAmount = 12000,
                UnsubMaxAmount = 18000,
                GradPlusMaxAmount = 34999,
            },
            new TestLoanLimitation()
            {
                AwardYear = "2017",
                SubMaxAmount = 12000,
                UnsubMaxAmount = 18000,
                GradPlusMaxAmount = 34999,
            }
        };

        public Task<IEnumerable<StudentLoanLimitation>> GetStudentLoanLimitationsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            var loanLimitList = new List<StudentLoanLimitation>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var limitData = testLoanLimitsData.FirstOrDefault(t => t.AwardYear == studentAwardYear.Code);
                if (limitData != null)
                {
                    var limit = new StudentLoanLimitation(studentAwardYear.Code, studentId)
                    {
                        GradPlusMaximumAmount = limitData.GradPlusMaxAmount,
                        SubsidizedMaximumAmount = limitData.SubMaxAmount,
                        UnsubsidizedMaximumAmount = limitData.UnsubMaxAmount
                    };

                    loanLimitList.Add(limit);
                }
            }
            return Task.FromResult(loanLimitList.AsEnumerable());
        }
    }
}
