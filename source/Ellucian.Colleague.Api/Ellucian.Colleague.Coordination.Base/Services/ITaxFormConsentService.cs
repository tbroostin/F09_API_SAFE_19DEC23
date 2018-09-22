// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// This interface defines the methods for getting and posting tax form consents.
    /// </summary>
    public interface ITaxFormConsentService
    {
        /// <summary>
        /// Returns a set of tax form consent Dtos.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Tax form consent DTOs associated with the specified person.</returns>
        Task<IEnumerable<TaxFormConsent>> GetAsync(string personId, TaxForms taxForm);

        /// <summary>
        /// Creates a tax form consent record.
        /// </summary>
        /// <param name="newTaxFormConsent">Tax form consent DTO</param>
        /// <returns>New tax form consent DTO</returns>
        Task<TaxFormConsent> PostAsync(TaxFormConsent newTaxFormConsent);

        /// <summary>
        /// Determines if the current user can view employee / student tax forms as an administrator
        /// </summary>
        /// <param name="taxForm"></param>
        /// <returns>Task(boolean)</returns>
        Task<bool> CanViewTaxDataWithOrWithoutConsent(TaxForms taxForm);
    }
}
