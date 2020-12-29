// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Attachment Collection services
    /// </summary>
    public interface IAttachmentCollectionService : IBaseService
    {
        /// <summary>
        /// Get the attachment collection by ID
        /// </summary>
        /// <param name="attachmentCollectionId">The attachment collection Id</param>
        /// <returns>The <see cref="AttachmentCollection">Attachment Collection</see></returns>
        Task<AttachmentCollection> GetAttachmentCollectionByIdAsync(string attachmentCollectionId);

        /// <summary>
        /// Get the attachment collections for the user
        /// </summary>
        /// <returns>Attachment collections associated with the user</returns>
        Task<IEnumerable<AttachmentCollection>> GetAttachmentCollectionsByUserAsync();

        /// <summary>
        /// Create the new attachment collection
        /// </summary>
        /// <param name="attachmentCollection">The <see cref="AttachmentCollection">Attachment Collection</see> to create</param>
        /// <returns>Newly created <see cref="AttachmentCollection">Attachment Collection</see></returns>
        Task<AttachmentCollection> PostAttachmentCollectionAsync(AttachmentCollection attachmentCollection);

        /// <summary>
        /// Update the attachment collection
        /// </summary>
        /// <param name="attachmentCollectionId">The ID of the attachment collection</param>
        /// <param name="attachmentCollection">The <see cref="AttachmentCollection">Attachment Collection</see> to update</param>
        /// <returns>The updated <see cref="AttachmentCollection">Attachment Collection</see></returns>
        Task<AttachmentCollection> PutAttachmentCollectionAsync(string attachmentCollectionId, AttachmentCollection attachmentCollection);

        /// <summary>
        /// Get this user's effective permissions for the attachment collection
        /// </summary>
        /// <param name="attachmentCollectionId">The ID of the attachment collection</param>
        /// <returns>This user's <see cref="AttachmentCollectionEffectivePermissions">Attachment Collection Effective Permissions</see></returns>
        Task<AttachmentCollectionEffectivePermissions> GetEffectivePermissionsAsync(string attachmentCollectionId);
    }
}