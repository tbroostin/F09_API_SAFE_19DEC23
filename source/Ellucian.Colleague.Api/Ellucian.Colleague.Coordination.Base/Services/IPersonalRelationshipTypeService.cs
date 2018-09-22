// Copyright 2015-17 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
