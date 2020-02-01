// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for EncrKey Repository
    /// </summary>
    public interface IEncryptionKeyRepository
    {
        /// <summary>
        /// Get encryption key
        /// </summary>
        /// <param name="id">ID of the key to retrieve</param>
        /// <returns>The <see cref="EncrKey">encryption key</see></returns>
        Task<EncrKey> GetKeyAsync(string id);
    }
}