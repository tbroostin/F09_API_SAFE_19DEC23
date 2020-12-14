// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestColleagueFinanceTaxFormStatementRepository : IColleagueFinanceTaxFormStatementRepository
    {
        public List<TaxFormStatement3> Statements3 { get; set; }

        public TestColleagueFinanceTaxFormStatementRepository()
        {
            this.Statements3 = new List<TaxFormStatement3>();
        }

        public async Task<IEnumerable<TaxFormStatement3>> Get2Async(string personId, string taxForm)
        {
            return await Task.FromResult(this.Statements3.Where(x => x != null && x.TaxForm == taxForm).ToList());
        }

        public Task<IEnumerable<TaxFormStatement2>> GetAsync(string personId, TaxForms taxForm)
        {
            throw new System.NotImplementedException();
        }
    }
}