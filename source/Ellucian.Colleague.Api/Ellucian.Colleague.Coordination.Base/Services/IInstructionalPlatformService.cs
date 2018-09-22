// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IInstructionalPlatformService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.InstructionalPlatform>> GetInstructionalPlatformsAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.InstructionalPlatform> GetInstructionalPlatformByGuidAsync(string guid, bool bypassCache);
    }
}
