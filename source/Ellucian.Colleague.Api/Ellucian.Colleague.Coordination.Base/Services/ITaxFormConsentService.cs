// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Dtos.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// <param name="personId">Person ID.</param>
        /// <param name="taxForm">The type of tax form: 1099-MISC, W-2, etc.</param>
        /// <returns>Tax form consent DTOs associated with the specified person.</returns>
        Task<IEnumerable<TaxFormConsent2>> Get2Async(string personId, string taxForm);

        /// <summary>
        /// Creates a tax form consent record.
        /// </summary>
        /// <param name="newTaxFormConsent">Tax form consent DTO</param>
        /// <returns>New tax form consent DTO</returns>
        Task<TaxFormConsent2> Post2Async(TaxFormConsent2 newTaxFormConsent2);

        /// <summary>
        /// Determines if the current user can view employee / student tax forms as an administrator
        /// </summary>
        /// <param name="taxForm">The type of tax form: 1099-MISC, W-2, etc.</param>
        /// <returns>Task(boolean)</returns>
        Task<bool> CanViewTaxDataWithOrWithoutConsent2Async(string taxForm);


        #region OBSOLETE METHODS

        /// <summary>
        /// Returns a set of tax form consent Dtos.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Tax form consent DTOs associated with the specified person.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get2Async instead.")]
        Task<IEnumerable<TaxFormConsent>> GetAsync(string personId, TaxForms taxForm);

        /// <summary>
        /// Creates a tax form consent record.
        /// </summary>
        /// <param name="newTaxFormConsent">Tax form consent DTO</param>
        /// <returns>New tax form consent DTO</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Post2Async instead.")]
        Task<TaxFormConsent> PostAsync(TaxFormConsent newTaxFormConsent);

        /// <summary>
        /// Determines if the current user can view employee / student tax forms as an administrator
        /// </summary>
        /// <param name="taxForm"></param>
        /// <returns>Task(boolean)</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use CanViewTaxDataWithOrWithoutConsent2Async instead.")]
        Task<bool> CanViewTaxDataWithOrWithoutConsent(TaxForms taxForm);

        #endregion
    }
}
