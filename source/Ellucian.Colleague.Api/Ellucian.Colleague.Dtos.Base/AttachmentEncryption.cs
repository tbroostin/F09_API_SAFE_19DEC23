// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// File attachment encryption metadata
    /// </summary>
    public class AttachmentEncryption
    {
        /// <summary>
        /// ID of the encryption key used to encrypt the content key
        /// </summary>
        public string EncrKeyId { get; private set; }

        /// <summary>
        /// Type of encryption used to encrypt the attachment content
        /// </summary>
        public string EncrType { get; private set; }

        /// <summary>
        /// Key (encrypted) used to encrypt the attachment content
        /// </summary>
        public byte[] EncrContentKey { get; private set; }

        /// <summary>
        /// IV used to encrypt the attachment content
        /// </summary>
        public byte[] EncrIV { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="encrKeyId">ID of the encryption key used to encrypt the content key</param>
        /// <param name="encrType">Type of encryption used to encrypt the attachment content</param>
        /// <param name="encrContentKey">Key (encrypted) used to encrypt the attachment content</param>
        /// <param name="encrIV">IV used to encrypt the attachment content</param>
        public AttachmentEncryption(string encrKeyId, string encrType, byte[] encrContentKey, byte[] encrIV)
        {
            if (string.IsNullOrEmpty(encrKeyId))
                throw new ArgumentNullException("encrKeyId");
            if (string.IsNullOrEmpty(encrType))
                throw new ArgumentNullException("encrType");
            if (encrContentKey == null || encrContentKey.Length <= 0)
                throw new ArgumentException("Content key is required");
            if (encrIV == null || encrIV.Length <= 0)
                throw new ArgumentException("IV is required");

            EncrKeyId = encrKeyId;
            EncrType = encrType;
            EncrContentKey = encrContentKey;
            EncrIV = encrIV;
        }
    }
}