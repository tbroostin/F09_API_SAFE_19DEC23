// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Comments services
    /// </summary>
    public interface IEedmCacheKeysService : IBaseService
    {
        void ClearEedmCacheKeys();
    }
}
