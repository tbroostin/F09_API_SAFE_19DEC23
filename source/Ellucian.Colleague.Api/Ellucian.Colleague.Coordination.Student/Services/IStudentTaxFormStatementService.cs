// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Define the methods signatures for a TaxFormStatementService.
    /// </summary>
    public interface IStudentTaxFormStatementService
    {
        /// <summary>
        /// Returns a set of tax form statements Dtos.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form (W-2, 1095-C, etc.)</param>
        /// <returns>List of tax form statements DTOs associated with the person and type of tax form.</returns>
        Task<IEnumerable<Dtos.Base.TaxFormStatement3>> Get2Async(string personId, string taxForm);

        /// <summary>
        /// Returns a set of tax form statements Dtos.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form (W-2, 1095-C, etc.)</param>
        /// <returns>List of tax form statements DTOs associated with the person and type of tax form.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get2Async instead.")]
        Task<IEnumerable<Dtos.Base.TaxFormStatement2>> GetAsync(string personId, Dtos.Base.TaxForms taxForm);
    }
}
