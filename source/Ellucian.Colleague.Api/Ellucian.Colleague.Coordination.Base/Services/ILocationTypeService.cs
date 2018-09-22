// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface ILocationTypeService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.LocationTypeItem>> GetLocationTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.LocationTypeItem> GetLocationTypeByGuidAsync(string guid);
    }
}
