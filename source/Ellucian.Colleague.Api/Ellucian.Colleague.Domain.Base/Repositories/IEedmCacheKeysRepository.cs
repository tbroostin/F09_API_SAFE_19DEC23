// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for Country Repository
    /// </summary>
    public interface IEedmCacheKeysRepository
    {

        /// <summary>
        /// Clear EEDM Cache Keys
        /// </summary>
        void ClearEedmCacheKeys();
    }
}