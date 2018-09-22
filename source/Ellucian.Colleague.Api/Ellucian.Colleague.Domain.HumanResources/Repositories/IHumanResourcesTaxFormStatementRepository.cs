﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    /// <summary>
    /// Define the method signatures for a TaxFormStatementRepository
    /// </summary>
    public interface IHumanResourcesTaxFormStatementRepository
    {
        /// <summary>
        /// Returns a set of tax form statements domain entities for the person for the type of tax form.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        Task<IEnumerable<TaxFormStatement2>> GetAsync(string personId, TaxForms taxForm);
    }
}
