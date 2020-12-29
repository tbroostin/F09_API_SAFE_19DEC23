// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestHumanResourcesTaxFormStatementRepository : IHumanResourcesTaxFormStatementRepository
    {
        public List<TaxFormStatement3> Statements3 { get; set; }

        public TestHumanResourcesTaxFormStatementRepository()
        {
            this.Statements3 = new List<TaxFormStatement3>();
            this.Statements = new List<TaxFormStatement2>();
        }

        public async Task<IEnumerable<TaxFormStatement3>> Get2Async(string personId, string taxForm)
        {
            return await Task.FromResult(this.Statements3.Where(x => x != null && x.TaxForm == taxForm).ToList());
        }

        public List<TaxFormStatement2> Statements { get; set; }

        public async Task<IEnumerable<TaxFormStatement2>> GetAsync(string personId, TaxForms taxForm)
        {
            return await Task.FromResult(this.Statements.Where(x => x != null && x.TaxForm == taxForm).ToList());
        }
    }
}