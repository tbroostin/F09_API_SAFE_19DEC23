// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface ITaxFormConsentRepository
    {
        /// <summary>
        /// Return a set of tax form consent domain entities.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form:1099-MISC, W-2, etc.</param>
        /// <returns>Set of tax form consents</returns>
        Task<IEnumerable<TaxFormConsent2>> Get2Async(string personId, string taxForm);

        /// <summary>
        /// Creates a tax form consent record.
        /// </summary>
        /// <param name="newTaxFormConsent">Tax form consent DTO</param>
        /// <returns>New tax form consent DTO</returns>
        Task<TaxFormConsent2> Post2Async(TaxFormConsent2 newTaxFormConsent);

        #region OBSOLETE METHODS

        /// <summary>
        /// Return a set of tax form consent domain entities.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form consents</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get2Async instead.")]
        Task<IEnumerable<TaxFormConsent>> GetAsync(string personId, TaxForms taxForm);

        /// <summary>
        /// Creates a tax form consent record.
        /// </summary>
        /// <param name="newTaxFormConsent">Tax form consent DTO</param>
        /// <returns>New tax form consent DTO</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Post2Async instead.")]
        Task<TaxFormConsent> PostAsync(TaxFormConsent newTaxFormConsent);

        #endregion
    }
}
