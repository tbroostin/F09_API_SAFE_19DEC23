// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Coordination service for Content Keys
    /// </summary>
    [RegisterType]
    public class ContentKeyService : BaseCoordinationService, IContentKeyService
    {
        private readonly IEncryptionKeyRepository _encrKeyRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry">Adaper registry</param>
        /// <param name="encrKeyRepository">Encryption key repository</param>
        /// <param name="currentUserFactory">The current user factory</param>
        /// <param name="roleRepository">The role repository</param>
        /// <param name="logger">The logger</param>
        public ContentKeyService(IAdapterRegistry adapterRegistry, IEncryptionKeyRepository encrKeyRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _encrKeyRepository = encrKeyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get a Content Key
        /// </summary>
        /// <param name="encryptionKeyId">The encryption key ID to use to encrypt the content key</param>
        /// <returns>The <see cref="ContentKey">Content Key</see></returns>
        public async Task<ContentKey> GetContentKeyAsync(string encryptionKeyId)
        {
            if (string.IsNullOrEmpty(encryptionKeyId))
                throw new ArgumentNullException("encryptionKeyId");

            // get the encryption key
            var encrKey = await GetEncryptionKeyAsync(encryptionKeyId);

            try
            {
                // create a content key
                var symAlgorithm = Aes.Create();
                symAlgorithm.KeySize = 256;

                // return the content key
                return new ContentKey()
                {
                    EncryptionKeyId = encryptionKeyId,
                    EncryptedKey = EncryptContentKey(encrKey.Key, symAlgorithm),
                    Key = symAlgorithm.Key
                };
            }
            catch (CryptographicException ce)
            {
                string error = "Error occurred encrypting the content key";
                logger.Error(ce, error);
                throw new Exception(error);
            }
        }

        /// <summary>
        /// Post an encrypted content key to have it decrypted
        /// </summary>
        /// <param name="contentKeyRequest">The <see cref="ContentKeyRequest">Content Key Request</see></param>
        /// <returns>The <see cref="ContentKey">Content Key</see></returns>
        public async Task<ContentKey> PostContentKeyAsync(ContentKeyRequest contentKeyRequest)
        {
            if (contentKeyRequest == null)
                throw new ArgumentNullException("contentKeyRequest");
            if (string.IsNullOrEmpty(contentKeyRequest.EncryptionKeyId))
                throw new ArgumentException("The encrypted key ID is required");
            if (contentKeyRequest.EncryptedKey == null || contentKeyRequest.EncryptedKey.Length <= 0)
                throw new ArgumentException("The encrypted content key is required");

            // get the encryption key
            var encrKey = await GetEncryptionKeyAsync(contentKeyRequest.EncryptionKeyId);

            try
            {
                // return the content key
                return new ContentKey()
                {
                    EncryptionKeyId = contentKeyRequest.EncryptionKeyId,
                    EncryptedKey = contentKeyRequest.EncryptedKey,
                    Key = DecryptContentKey(encrKey.Key, contentKeyRequest.EncryptedKey)
                };
            }
            catch (CryptographicException ce)
            {
                string error = "Error occurred decrypting the content key";
                logger.Error(ce, error);
                throw new Exception(error);
            }
        }

        // get the encryption key from the repository
        private async Task<EncrKey> GetEncryptionKeyAsync(string encryptionKeyId)
        {
            // get the encryption key
            var encrKey = await _encrKeyRepository.GetKeyAsync(encryptionKeyId);
            if (encrKey == null)
                throw new KeyNotFoundException("Encryption key not found");
            // the key must be active
            if (encrKey.Status != EncrKeyStatus.Active)
                throw new ArgumentException("The encryption key is not active");

            return encrKey;
        }

        // encrypt the content key using an encryption key
        private byte[] EncryptContentKey(string encryptionKey, SymmetricAlgorithm symAlgorithm)
        {
            var cryptoServiceProvider = new RSACryptoServiceProvider();
            cryptoServiceProvider.ImportParameters(GetRSAParams(encryptionKey));
            return cryptoServiceProvider.Encrypt(symAlgorithm.Key, false);
        }

        // decrypt the encrypted content key using an encryption key
        private byte[] DecryptContentKey(string encryptionKey, byte[] encryptedContentKey)
        {
            var cryptoServiceProvider = new RSACryptoServiceProvider();
            cryptoServiceProvider.ImportParameters(GetRSAParams(encryptionKey));
            return cryptoServiceProvider.Decrypt(encryptedContentKey, false);
        }

        // create the RSAParameters object based on the encryption key
        private RSAParameters GetRSAParams(string encryptionKey)
        {
            try
            {
                Object keyPair = null;

                // use the encryption key to encrypt the content key
                using (var keyReader = new StringReader(encryptionKey))
                {
                    // the encryption key must be in a PEM format
                    keyPair = new PemReader(keyReader).ReadObject();
                }

                if (keyPair is AsymmetricCipherKeyPair) // pkcs #1 format
                    keyPair = ((AsymmetricCipherKeyPair)keyPair).Private;
                var privateKey = (RsaPrivateCrtKeyParameters)keyPair;                    

                return new RSAParameters
                {
                    Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
                    P = privateKey.P.ToByteArrayUnsigned(),
                    Q = privateKey.Q.ToByteArrayUnsigned(),
                    DP = privateKey.DP.ToByteArrayUnsigned(),
                    DQ = privateKey.DQ.ToByteArrayUnsigned(),
                    InverseQ = privateKey.QInv.ToByteArrayUnsigned(),
                    D = privateKey.Exponent.ToByteArrayUnsigned(),
                    Exponent = privateKey.PublicExponent.ToByteArrayUnsigned()
                };
            }
            catch (InvalidCipherTextException icte)
            {
                string error = "Error occurred reading the encryption key";
                logger.Error(icte, error);
                throw new Exception(error);
            }
            catch (PemException pe)
            {
                string error = "Error occurred reading the encryption key";
                logger.Error(pe, error);
                throw new Exception(error);
            }
        }
    }
}