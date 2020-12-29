// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for encryption keys.
    /// </summary>
    [RegisterType]
    public class EncryptionKeyRepository : BaseColleagueRepository, IEncryptionKeyRepository
    {
        private const string activeStatus = "A";
        private const string inactiveStatus = "I";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public EncryptionKeyRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Get the encryption key
        /// </summary>
        /// <param name="id">ID of the key to retrieve</param>
        /// <returns>The <see cref="EncrKey">encryption key</see></returns>
        public async Task<EncrKey> GetKeyAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            // get the key
            var encrKeyRecord = await DataReader.ReadRecordAsync<EncrKeys>(id);
            if (encrKeyRecord == null)
                throw new RepositoryException("Encryption key not found");
            if (encrKeyRecord.EncrkVersion == null || encrKeyRecord.EncrkVersion <= 0)
                throw new RepositoryException("Invalid encryption key version");

            int version = (int)encrKeyRecord.EncrkVersion;

            return new EncrKey(encrKeyRecord.Recordkey, encrKeyRecord.EncrkName, encrKeyRecord.EncrkKey, version,
                ConvertStatus(encrKeyRecord.EncrkStatus))
            {
                Description = encrKeyRecord.EncrkDesc
            };
        }

        // convert an encryption key status to its enum
        private EncrKeyStatus ConvertStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentNullException("status");

            switch (status)
            {
                case activeStatus:
                    return EncrKeyStatus.Active;
                case inactiveStatus:
                    return EncrKeyStatus.Inactive;
                default:
                    throw new ArgumentException("Unknown status");
            }
        }
    }
}