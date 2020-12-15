// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Define the method signatures for a TaxFormStatementRepository
    /// </summary>
    public interface IColleagueFinanceTaxFormStatementRepository
    {
        /// <summary>
        /// Returns a set of tax form statements domain entities for the person for the type of tax form.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">The type of tax form: 1099-MISC, W-2, etc.</param>
        /// <returns>Set of tax form statements</returns>
        Task<IEnumerable<TaxFormStatement3>> Get2Async(string personId, string taxForm);

        /// <summary>
        /// Returns a set of tax form statements domain entities for the person for the type of tax form.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get2Async instead.")]
        Task<IEnumerable<TaxFormStatement2>> GetAsync(string personId, TaxForms taxForm);
    }
}
