// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Campus Organization services
    /// </summary>
    public interface ICampusOrganizationService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.CampusInvolvementRole>> GetCampusInvolvementRolesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.CampusInvolvementRole> GetCampusInvolvementRoleByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.CampusOrganizationType>> GetCampusOrganizationTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.CampusOrganizationType> GetCampusOrganizationTypeByGuidAsync(string guid);


        Task<IEnumerable<Dtos.CampusOrganization>> GetCampusOrganizationsAsync(bool bypassCache);
        Task<Dtos.CampusOrganization> GetCampusOrganizationByGuidAsync(string id);

        Task<Tuple<IEnumerable<Dtos.CampusInvolvement>, int>> GetCampusInvolvementsAsync(int offset, int limit, bool bypassCache);
        Task<Dtos.CampusInvolvement> GetCampusInvolvementByGuidAsync(string id);
    }
}
