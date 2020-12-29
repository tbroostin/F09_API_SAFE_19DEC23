// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface ICampusOrganizationRepository
    {
        Task<IEnumerable<CampusOrganization>> GetCampusOrganizationsAsync(bool bypassCache);
        Task<Tuple<IEnumerable<CampusInvolvement>, int>> GetCampusInvolvementsAsync(int offset, int limit);
        Task<CampusInvolvement> GetGetCampusInvolvementByIdAsync(string id);
        Task<IEnumerable<CampusOrgAdvisorRole>> GetCampusOrgAdvisorsAsync(IEnumerable<string> hrpId);
        Task<IEnumerable<CampusOrgMemberRole>> GetCampusOrgMembersAsync(IEnumerable<string> hrpId);
        Task<IEnumerable<CampusOrganization2>> GetCampusOrganizations2Async(List<string> campusOrgsIds);
    }
}
