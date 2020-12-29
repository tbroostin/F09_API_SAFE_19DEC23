// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Attachment services
    /// </summary>
    public interface IAttachmentService : IBaseService
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
        /// Get the attachment's content
        /// </summary>
        /// <param name="attachmentId">Id of the attachment's content to get</param>
        /// <returns>Tuple of the attachment, its temp file location, and encryption metadata</returns>
        Task<Tuple<Attachment, string, AttachmentEncryption>> GetAttachmentContentAsync(string attachmentId);

        /// <summary>
        /// Query attachments
        /// </summary>
        /// <param name="criteria">Criteria to query attachments by</param>
        /// <returns>List of <see cref="Attachment">Attachments matching the query criteria</see></returns>
        Task<IEnumerable<Attachment>> QueryAttachmentsAsync(AttachmentSearchCriteria criteria);

        /// <summary>
        /// Create the new attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to create</param>
        /// <param name="attachmentEncryption">The <see cref="AttachmentEncryption">AttachmentEncryption</see> metadata</param>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        Task<Attachment> PostAttachmentAsync(Attachment attachment, AttachmentEncryption attachmentEncryption);

        /// <summary>
        /// Create the new attachment w/ content
        /// </summary>
        /// <param name="attachment">The attachment's metadata</param>
        /// <param name="attachmentEncryption">The <see cref="AttachmentEncryption">AttachmentEncryption</see> metadata</param>
        /// <param name="attachmentContentStream">Stream to the attachment content</param>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        Task<Attachment> PostAttachmentAndContentAsync(Attachment attachment, AttachmentEncryption attachmentEncryption, Stream attachmentContentStream);

        /// <summary>
        /// Update the attachment
        /// </summary>
        /// <param name="attachmentId">ID of the attachment to update</param>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to update</param>
        /// <returns>The updated <see cref="Attachment">Attachment</see></returns>
        Task<Attachment> PutAttachmentAsync(string attachmentId, Attachment attachment);

        /// <summary>
        /// Delete the attachment
        /// </summary>
        /// <param name="attachmentId">Id of the attachment to delete</param>
        Task DeleteAttachmentAsync(string attachmentId);
    }
}