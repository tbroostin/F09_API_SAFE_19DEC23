/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPayClassificationsRepository
    {
        /// <summary>
        /// Get PayClassifications objects for all PayClassifications bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="bypassCache">Bypass the cache if set to true.</param>
        /// <returns>Tuple of PayClassifications Entity objects <see cref="PayClassifications"/> and a count for paging.</returns>
        Task<IEnumerable<PayClassification>> GetPayClassificationsAsync(bool bypassCache = false);

        /// <summary>
        /// Get PayClassifications objects for a specific Id.
        /// </summary>   
        /// <param name="id">guid of the PayClassifications record.</param>
        /// <returns>PayClassifications Entity <see cref="PayClassifications"./></returns>
        Task<PayClassification> GetPayClassificationsByIdAsync(string id);
    }
}
