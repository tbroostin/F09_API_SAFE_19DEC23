// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    /// <summary>
    /// Define the method signatures for a TaxFormStatementRepository
    /// </summary>
    public interface ITaxFormStatementRepository
    {
        /// <summary>
        /// Returns a set of tax form statements domain entities for the person for the type of tax form.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        Task<IEnumerable<TaxFormStatement>> GetAsync(string personId, TaxForms taxForm);
    }
}
