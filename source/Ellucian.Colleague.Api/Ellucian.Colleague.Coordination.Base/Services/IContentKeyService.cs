// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for ContentKey services
    /// </summary>
    public interface IContentKeyService
    {
        /// <summary>
        /// Get a Content Key
        /// </summary>
        /// <param name="encryptionKeyId">The encryption key ID to use to encrypt the content key</param>
        /// <returns>The <see cref="ContentKey">Content Key</see></returns>
        Task<ContentKey> GetContentKeyAsync(string encryptionId);

        /// <summary>
        /// Post an encrypted content key to have it decrypted
        /// </summary>
        /// <param name="contentKeyRequest">The <see cref="ContentKeyRequest">Content Key Request</see></param>
        /// <returns>The <see cref="ContentKey">Content Key</see></returns>
        Task<ContentKey> PostContentKeyAsync(ContentKeyRequest contentKeyRequest);
    }
}