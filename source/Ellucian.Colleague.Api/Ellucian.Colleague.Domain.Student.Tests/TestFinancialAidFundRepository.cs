//Copyright 2018 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestFinancialAidFundRepository : IFinancialAidFundRepository
    {
        public Task<FinancialAidFund> GetFinancialAidFundByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FinancialAidFundsFinancialProperty>> GetFinancialAidFundFinancialsAsync(string awardId, IEnumerable<string> fundYears, string hostCountry)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FinancialAidFundsFinancialProperty>>(new List<Student.Entities.FinancialAidFundsFinancialProperty>()
                {
                    new Student.Entities.FinancialAidFundsFinancialProperty("2008", "OFFICE1", (decimal) 10000, "AWARD1"),
                    new Student.Entities.FinancialAidFundsFinancialProperty("2009", "OFFICE2", (decimal) 20000, "AWARD2"),
                    new Student.Entities.FinancialAidFundsFinancialProperty("2010", "OFFICE3", (decimal) 30000, "AWARD3"),
                });
        }


        public Task<IEnumerable<FinancialAidFund>> GetFinancialAidFundsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FinancialAidFund>>(new List<Student.Entities.FinancialAidFund>()
                {
                    new Student.Entities.FinancialAidFund("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.FinancialAidFund("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.FinancialAidFund("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }

        public Task<Tuple<IEnumerable<FinancialAidFund>, int>> GetFinancialAidFundsAsync(int offset, int limit, bool bypassCache)
        {
            throw new NotImplementedException();
        }
    }
}