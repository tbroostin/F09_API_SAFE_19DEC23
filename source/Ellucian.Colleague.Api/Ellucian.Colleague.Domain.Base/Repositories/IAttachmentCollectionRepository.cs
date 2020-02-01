// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for Attachment Collection Repository
    /// </summary>
    public interface IAttachmentCollectionRepository
    {
        /// <summary>
        /// Get the attachment collection by ID
        /// </summary>
        /// <param name="attachmentCollectionId">The attachment collection Id</param>
        /// <returns>The <see cref="AttachmentCollection">Attachment Collection</see></returns>
        Task<AttachmentCollection> GetAttachmentCollectionByIdAsync(string attachmentCollectionId);

        /// <summary>
        /// Get the attachment collections by list of IDs
        /// </summary>
        /// <param name="attachmentCollectionIds">The attachment collection Id</param>
        /// <returns>List of <see cref="AttachmentCollection">Attachment Collections</see></returns>
        Task<IEnumerable<AttachmentCollection>> GetAttachmentCollectionsByIdAsync(IEnumerable<string> attachmentCollectionIds);

        /// <summary>
        /// Get the attachment collections for the user
        /// </summary>
        /// <param name="personId">Person ID of the user</param>
        /// <param name="roleIds">User's role Ids</param>
        /// <returns>List of <see cref="AttachmentCollection">Attachment Collections</see></returns>
        Task<IEnumerable<AttachmentCollection>> GetAttachmentCollectionsByUserAsync(string personId, IEnumerable<string> roleIds);

        /// <summary>
        /// Create the new attachment collection
        /// </summary>
        /// <param name="attachmentCollection">The <see cref="AttachmentCollection">Attachment Collection</see> to create</param>
        /// <returns>Newly created <see cref="AttachmentCollection">Attachment Collection</see></returns>
        Task<AttachmentCollection> PostAttachmentCollectionAsync(AttachmentCollection attachmentCollection);

        /// <summary>
        /// Update the attachment collection
        /// </summary>
        /// <param name="attachmentCollection">The <see cref="AttachmentCollection">Attachment Collection</see> to update</param>
        /// <returns>The updated <see cref="AttachmentCollection">Attachment Collection</see></returns>
        Task<AttachmentCollection> PutAttachmentCollectionAsync(AttachmentCollection attachmentCollection);
    }
}