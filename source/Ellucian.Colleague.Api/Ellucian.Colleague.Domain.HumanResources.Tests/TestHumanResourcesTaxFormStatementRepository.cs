// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestHumanResourcesTaxFormStatementRepository : IHumanResourcesTaxFormStatementRepository
    {
        public List<TaxFormStatement2> Statements { get; set; }

        public TestHumanResourcesTaxFormStatementRepository()
        {
            this.Statements = new List<TaxFormStatement2>();
        }

        public async Task<IEnumerable<TaxFormStatement2>> GetAsync(string personId, TaxForms taxForm)
        {
            return await Task.FromResult(this.Statements.Where(x => x != null && x.TaxForm == taxForm).ToList());
        }
    }
}