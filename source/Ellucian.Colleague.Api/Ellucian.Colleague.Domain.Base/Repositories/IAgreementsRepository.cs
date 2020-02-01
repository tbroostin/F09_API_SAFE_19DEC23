// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IAgreementsRepository
    {
        /// <summary>
        /// Get all <see cref="AgreementPeriod"/>agreement periods</see>
        /// </summary>
        /// <param name="ignoreCache">Flag indicating whether or not to ignore cached data</param>
        /// <returns>All agreement periods</returns>
        Task<IEnumerable<AgreementPeriod>> GetAgreementPeriodsAsync(bool ignoreCache = false);

        /// <summary>
        /// Get <see cref="PersonAgreement"/>person agreements</see> by person ID
        /// </summary>
        /// <param name="id">Person identifier</param>
        /// <returns>Collection of person agreements for a given person</returns>
        Task<IEnumerable<PersonAgreement>> GetPersonAgreementsByPersonIdAsync(string id);

        /// <summary>
        /// Updates a <see cref="PersonAgreement">person agreement</see>
        /// </summary>
        /// <param name="agreement">The <see cref="PersonAgreement">person agreement</see> to update</param>
        /// <returns>An updated <see cref="PersonAgreement">person agreement</see></returns>
        Task<PersonAgreement> UpdatePersonAgreementAsync(PersonAgreement agreement);
    }
}
