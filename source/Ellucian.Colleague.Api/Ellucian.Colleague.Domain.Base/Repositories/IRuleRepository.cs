// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for rule repositories
    /// </summary>
    public interface IRuleRepository
    {
        /// <summary>
        /// Retrieves rules by ID.
        /// </summary>
        /// <param name="ruleIds"></param>
        /// <returns></returns>
        Task<IEnumerable<Rule>> GetManyAsync(IEnumerable<string> ruleIds);

        /// <summary>
        /// Retrieves a single rule
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        Task<Rule> GetAsync(string ruleId);

        /// <summary>
        /// Executes one or more rules against one or more records in Colleague.
        /// All requests must be against the same context type (primary view).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleRequests"></param>
        /// <returns></returns>
        Task<IEnumerable<RuleResult>> ExecuteAsync<T>(IEnumerable<RuleRequest<T>> ruleRequests);

        /// <summary>
        /// Executes one or more rules against one or more records in Colleague.
        /// All requests must be against the same context type (primary view).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleRequests"></param>
        /// <returns></returns>
        [Obsolete("Replaced by ExecuteAsync<T>")]
        IEnumerable<RuleResult> Execute<T>(IEnumerable<RuleRequest<T>> ruleRequests);
    }
}
