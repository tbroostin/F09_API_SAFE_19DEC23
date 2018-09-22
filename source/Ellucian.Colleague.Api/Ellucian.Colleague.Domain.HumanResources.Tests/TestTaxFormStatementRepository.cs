// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestTaxFormStatementRepository : ITaxFormStatementRepository
    {
        public List<TaxFormStatement> Statements { get; set; }

        public TestTaxFormStatementRepository()
        {
            this.Statements = new List<TaxFormStatement>();
        }

        public async Task<IEnumerable<TaxFormStatement>> GetAsync(string personId, TaxForms taxForm)
        {
            return await Task.FromResult(this.Statements.Where(x => x != null && x.TaxForm == taxForm).ToList());
        }
    }
}