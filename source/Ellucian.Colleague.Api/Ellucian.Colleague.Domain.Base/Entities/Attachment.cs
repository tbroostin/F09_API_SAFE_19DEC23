// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// File attachment
    /// </summary>
    [Serializable]
    public class Attachment
    {
        /// <summary>
        /// ID of the attachment
        /// </summary>
        private string _Id;
        public string Id
        {
            get { return _Id; }
            set
            {
                if (string.IsNullOrEmpty(_Id))
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Attachment Id cannot be changed.");
                }
            }
        }

        /// <summary>
        /// Attachment collection Id this attachment belongs to
        /// </summary>
        private string _CollectionId;
        public string CollectionId
        {
            get { return _CollectionId; }
            set
            {
                if (string.IsNullOrEmpty(_CollectionId))
                {
                    _CollectionId = value;
                }
                else
                {
                    throw new InvalidOperationException("Attachment's collection ID cannot be changed.");
                }
            }
        }

        /// <summary>
        /// Owner/Colleague person ID of the attachment
        /// </summary>
        private string _Owner;
        public string Owner
        {
            get { return _Owner; }
            set
            {
                if (string.IsNullOrEmpty(_Owner))
                {
                    _Owner = value;
                }
                else
                {
                    throw new InvalidOperationException("Attachment's owner cannot be changed.");
                }
            }
        }

        /// <summary>
        /// The attachment's display name (e.g. 2017 Tax Info.pdf)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Content type (e.g. application/pdf; utf-8)
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Size, in bytes, of the attachment
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// Status of the attachment (e.g. active, deleted)
        /// </summary>
        public AttachmentStatus Status { get; set; }

        /// <summary>
        /// Date when the attachment was created
        /// </summary>
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// Colleague person ID of who created the attachment
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Date when the attachment's properties or content was modified
        /// </summary>
        public DateTimeOffset? ModifiedAt { get; set; }

        /// <summary>
        /// Colleague person ID of who modified the attachment
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Date when the attachment was deleted
        /// </summary>
        public DateTimeOffset? DeletedAt { get; set; }

        /// <summary>
        /// Colleague person ID of who deleted the attachment
        /// </summary>
        public string DeletedBy { get; set; }

        /// <summary>
        /// Free form text tag
        /// </summary>
        public string TagOne { get; set; }

        /// <summary>
        /// ID of the encryption key used to encrypt the content key
        /// </summary>
        public string EncrKeyId { get; set; }

        /// <summary>
        /// Type of encryption used to encrypt the attachment content
        /// </summary>
        public string EncrType { get; set; }

        /// <summary>
        /// Key (encrypted) used to encrypt the attachment content
        /// </summary>
        public byte[] EncrContentKey { get; set; }

        /// <summary>
        /// IV used to encrypt the attachment content
        /// </summary>
        public byte[] EncrIV { get; set; }

        /// <summary>
        /// Create a file attachment
        /// </summary>
        /// <param name="id">Attachment ID</param>
        /// <param name="collectionId">Id of the collection the attachment belongs to</param>
        /// <param name="name">Attachment name</param>
        /// <param name="contentType">Attachment content type</param>
        /// <param name="size">Size of the attachment in bytes</param>
        /// <param name="owner">The attachment owner</param>
        public Attachment(string id, string collectionId, string name, string contentType, long? size, string owner)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(collectionId))
                throw new ArgumentNullException("collectionId");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException("contentType");
            if (size == null)
                throw new ArgumentNullException("size");
            if (string.IsNullOrEmpty(owner))
                throw new ArgumentNullException("owner");

            Id = id;
            CollectionId = collectionId;
            Name = name;
            ContentType = contentType;
            Size = size;
            Owner = owner;
        }        
    }
}