// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IRestrictionTypeService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.RestrictionType>> GetRestrictionTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.RestrictionType> GetRestrictionTypeByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.RestrictionType2>> GetRestrictionTypes2Async(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.RestrictionType2> GetRestrictionTypeByGuid2Async(string id);
    }
}
