//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentFinancialAidAwardRepository
    {
        public Task<IEnumerable<StudentAwardHistoryStatus>> GetStudentAwardHistoryStatusesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.StudentAwardHistoryStatus>>(new List<StudentAwardHistoryStatus>()
                {
                    new StudentAwardHistoryStatus()
                    {
                        Status = "A",
                        StatusDate = new DateTime(2016, 02, 01),
                        Amount = (decimal) 2000000,
                        XmitAmount = (decimal) 1000
                    },
                    new StudentAwardHistoryStatus()
                    {
                        Status = "A",
                        StatusDate = new DateTime(2016, 02, 01),
                        Amount = (decimal) 1000000,
                        XmitAmount = (decimal) 1000
                    },
                    new StudentAwardHistoryStatus()
                    {
                        Status = "A",
                        StatusDate = new DateTime(2016, 02, 01),
                        Amount = (decimal) 3000000,
                        XmitAmount = (decimal) 1000
                    },
                });
        }
        public Task<IEnumerable<StudentAwardHistoryByPeriod>> GetStudentAwardHistoryByPeriodsAsync()
        {
            return Task.FromResult<IEnumerable<StudentAwardHistoryByPeriod>>(new List<StudentAwardHistoryByPeriod>()
                {
                    new StudentAwardHistoryByPeriod()
                    {
                        AwardPeriod = "CODE1",
                        Status = "A",
                        StatusDate = new DateTime(2016, 02, 01),
                        Amount = (decimal) 2000000,
                        XmitAmount = (decimal) 1000
                    },
                    new StudentAwardHistoryByPeriod()
                    {
                        AwardPeriod = "CODE2",
                        Status = "A",
                        StatusDate = new DateTime(2016, 02, 01),
                        Amount = (decimal) 1000000,
                        XmitAmount = (decimal) 1000
                    },
                    new StudentAwardHistoryByPeriod()
                    {
                        AwardPeriod = "CODE3",
                        Status = "A",
                        StatusDate = new DateTime(2016, 02, 01),
                        Amount = (decimal) 3000000,
                        XmitAmount = (decimal) 1000
                    },
                });
        }

        public Task<IEnumerable<StudentFinancialAidAward>> GetStudentFinancialAidAwardsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<StudentFinancialAidAward>>(new List<StudentFinancialAidAward>()
                {
                    new StudentFinancialAidAward("bb66b971-3ee0-4477-9bb7-539721f93434", "STUDENT1", "FUND1", "YEAR1"),
                    new StudentFinancialAidAward("5aeebc5c-c973-4f83-be4b-f64c95002124", "STUDENT2", "FUND2", "YEAR2"),
                    new StudentFinancialAidAward("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "STUDENT3", "FUND3", "YEAR3"),
                    new StudentFinancialAidAward("71278bac-8e6a-e1d4-72ea-cef1a337b633", "STUDENT4", "FUND4", "YEAR4")
                });
        }
    }
}