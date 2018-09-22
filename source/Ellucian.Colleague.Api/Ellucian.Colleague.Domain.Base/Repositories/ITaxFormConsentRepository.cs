// Copyright 2015 Ellucian Company L.P. and its affiliates.

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
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form consents</returns>
        Task<IEnumerable<TaxFormConsent>> GetAsync(string personId, TaxForms taxForm);

        /// <summary>
        /// Creates a tax form consent record.
        /// </summary>
        /// <param name="newTaxFormConsent">Tax form consent DTO</param>
        /// <returns>New tax form consent DTO</returns>
        Task<TaxFormConsent> PostAsync(TaxFormConsent newTaxFormConsent);
    }
}
