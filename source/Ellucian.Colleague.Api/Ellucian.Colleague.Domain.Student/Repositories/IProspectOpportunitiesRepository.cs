// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IProspectOpportunitiesRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<ProspectOpportunity>, int>> GetProspectOpportunitiesAsync(int offset, int limit, ProspectOpportunity criteriaObj = null,
            string[] filterPersonIds = null, bool bypassCache = false);
        Task<ProspectOpportunity> GetProspectOpportunityByIdAsync(string id, bool bypassCache = false);
        
        Task<ProspectOpportunity> CreateProspectOpportunitiesSubmissionsAsync(AdmissionApplication request);

        Task<ProspectOpportunity> UpdateProspectOpportunitiesSubmissionsAsync(AdmissionApplication request);

        Task<string> GetProspectOpportunityIdFromGuidAsync(string guid);

        Task<AdmissionApplication> GetProspectOpportunitiesSubmissionsByGuidAsync(string guid, bool bypassCache = false);
    }
}