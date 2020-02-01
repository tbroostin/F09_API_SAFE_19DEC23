// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Planning.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Planning.Repositories
{
    public interface IAdvisorRepository
    {
        /// <summary>
        /// Returns detailed advising information for an advisor: whether an active advisor, list of advisees
        /// </summary>
        Task<Entities.Advisor> GetAsync(string id, AdviseeInclusionType adviseeInclusionType = AdviseeInclusionType.AllAdvisees);
        /// <summary>
        /// Finds faculty advisors given a last, first, middle name.
        /// </summary>
        Task<IEnumerable<string>> SearchAdvisorByNameAsync(string lastName, string firstName = null, string middleName = null);

        Task<IEnumerable<string>> SearchAdvisorByNameForExactMatchAsync(string lastName, string firstName = null, string middleName = null);
        /// <summary>
        /// Returns basic entity for the given advisor IDs (name, email address).
        /// Must be found on either faculty or registered as a staff member to be included in their response.
        /// </summary>
        /// <param name="advisorIds">IDs of the advisors to retrieve</param>
        /// <param name="includeAdviseeType">Determines the type of advisee data that should be returned with the advisor entities.</param>
        /// <returns>List of Advisor entities (not fully built out)</returns>
        Task<IEnumerable<Advisor>> GetAdvisorsAsync(IEnumerable<string> advisorIds, AdviseeInclusionType adviseeInclusionType = AdviseeInclusionType.AllAdvisees);
    }
}
