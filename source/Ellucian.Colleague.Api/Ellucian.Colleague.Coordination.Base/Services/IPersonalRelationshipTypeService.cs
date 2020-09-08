// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonalRelationshipTypeService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.RelationType>> GetPersonalRelationTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.RelationType> GetPersonalRelationTypeByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationshipStatus>> GetPersonalRelationshipStatusesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.PersonalRelationshipStatus> GetPersonalRelationshipStatusByGuidAsync(string guid);
    }
}
