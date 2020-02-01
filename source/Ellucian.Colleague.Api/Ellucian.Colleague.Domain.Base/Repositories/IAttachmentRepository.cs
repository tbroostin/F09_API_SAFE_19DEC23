// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for Attachment Repository
    /// </summary>
    public interface IAttachmentRepository
    {
        /// <summary>
        /// Get the attachment temp file path on the server
        /// </summary>
        /// <returns>The attachment temp file path on the server</returns>
        string GetAttachTempFilePath();

        /// <summary>
        /// Get attachments
        /// </summary>
        /// <param name="owner">Owner to get attachments for</param>
        /// <param name="collectionId">Collection Id to get attachments for</param>
        /// <param name="tagOne">TagOne value to get attachments for</param>
        /// <returns>List of <see cref="Attachment">Attachments</see></returns>
        Task<IEnumerable<Attachment>> GetAttachmentsAsync(string owner, string collectionId, string tagOne);

        /// <summary>
        /// Get the attachment by ID
        /// </summary>
        /// <param name="attachmentId">ID of the attachment</param>
        /// <returns>The <see cref="Attachment">attachment</see></returns>
        Task<Attachment> GetAttachmentByIdWithEncrMetadataAsync(string attachmentId);

        /// <summary>
        /// Get the attachment by ID and do not include its encryption parameters
        /// </summary>
        /// <param name="attachmentId">ID of the attachment</param>
        /// <returns>The <see cref="Attachment">attachment</see></returns>
        Task<Attachment> GetAttachmentByIdNoEncrMetadataAsync(string attachmentId);

        /// <summary>
        /// Get the attachment's content
        /// </summary>
        /// <param name="attachment">The attachment</param>
        /// <returns>Path to the attachment's temp file location</returns>
        Task<string> GetAttachmentContentAsync(Attachment attachment);

        /// <summary>
        /// Create the new attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to create</param>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        Task<Attachment> PostAttachmentAsync(Attachment attachment);

        /// <summary>
        /// Create the new attachment w/ content
        /// </summary>
        /// <param name="attachment">The attachment's metadata</param>
        /// <param name="attachmentContentStream">Stream to the attachment content</param>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        Task<Attachment> PostAttachmentAndContentAsync(Attachment attachment, Stream attachmentContentStream);

        /// <summary>
        /// Update the attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to update</param>
        /// <returns>Updated <see cref="Attachment">Attachment</see></returns>
        Task<Attachment> PutAttachmentAsync(Attachment attachment);

        /// <summary>
        /// Delete the attachment
        /// </summary>
        /// <param name="attachment">The attachment to delete</param>
        Task DeleteAttachmentAsync(Attachment attachment);
    }
}