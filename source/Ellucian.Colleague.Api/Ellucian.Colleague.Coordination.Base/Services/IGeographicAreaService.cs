// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IGeographicAreaService :IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>, int>> GetGeographicAreasAsync(int offset, int limit, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.GeographicArea> GetGeographicAreaByGuidAsync(string guid);
    }
}
