// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IOtherHonorService: IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.OtherHonor>> GetOtherHonorsAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.OtherHonor> GetOtherHonorByGuidAsync(string guid);
    }
}
