// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Encryption key
    /// </summary>
    [Serializable]
    public class EncrKey
    {
        /// <summary>
        /// ID of the encryption key
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
                    throw new InvalidOperationException("Encryption key Id cannot be changed.");
                }
            }
        }

        /// <summary>
        /// The key's display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The key's description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The key
        /// </summary>
        private string _Key;
        public string Key
        {
            get { return _Key; }
            set
            {
                if (string.IsNullOrEmpty(_Key))
                {
                    _Key = value;
                }
                else
                {
                    throw new InvalidOperationException("Encryption key cannot be changed.");
                }
            }
        }

        /// <summary>
        /// The version of the key
        /// </summary>
        private int _Version;
        public int Version
        {
            get { return _Version; }
            set
            {
                if (_Version <= 0)
                {
                    _Version = value;
                }
                else
                {
                    throw new InvalidOperationException("The version of the key cannot be changed.");
                }
            }
        }

        /// <summary>
        /// The key's status
        /// </summary>
        public EncrKeyStatus Status { get; set; }

        /// <summary>
        /// Create an encryption key
        /// </summary>
        /// <param name="id">Encryption key ID</param>
        /// <param name="name">Key name</param>
        /// <param name="key">Key</param>
        /// <param name="version">Key version</param>
        /// <param name="status">Key status</param>
        public EncrKey(string id, string name, string key, int version, EncrKeyStatus status)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            if (version <= 0)
                throw new ArgumentException("version");

            Id = id;
            Name = name;
            Key = key;
            Version = version;
            Status = status;
        }
    }
}