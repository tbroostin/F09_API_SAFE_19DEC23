// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Define the methods signatures for a TaxFormStatementService.
    /// </summary>
    public interface ITaxFormStatementService
    {
        /// <summary>
        /// Returns a set of tax form statements Dtos.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form (W-2, 1095-C, etc.)</param>
        /// <returns>List of tax form statements DTOs associated with the person and type of tax form.</returns>
        Task<IEnumerable<TaxFormStatement>> GetAsync(string personId, TaxForms taxForm);
    }
}
