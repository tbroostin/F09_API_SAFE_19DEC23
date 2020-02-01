﻿// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Content Key
    /// </summary>
    public class ContentKey
    {
        /// <summary>
        /// The encryption key ID
        /// </summary>
        public string EncryptionKeyId { get; set; }

        /// <summary>
        /// The encrypted content key
        /// </summary>
        public byte[] EncryptedKey { get; set; }

        /// <summary>
        /// The plain-text content key
        /// </summary>
        public byte[] Key { get; set; }
    }
}