// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Coordination service for Attachment Collections
    /// </summary>
    [RegisterType]
    public class AttachmentCollectionService : BaseCoordinationService, IAttachmentCollectionService
    {
        private readonly IAttachmentCollectionRepository _attachmentCollectionRepository;
        private readonly IEncryptionKeyRepository _encrKeyRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for AttachmentCollectionService
        /// </summary>
        /// <param name="adapterRegistry">Adaper registry</param>
        /// <param name="attachmentCollectionRepository">Attachment collection repository</param>
        /// <param name="encrKeyRepository">Encryption key repository</param>
        /// <param name="currentUserFactory">The current user factory</param>
        /// <param name="roleRepository">The role repository</param>
        /// <param name="logger">The logger</param>
        public AttachmentCollectionService(IAdapterRegistry adapterRegistry, IAttachmentCollectionRepository attachmentCollectionRepository,
            IEncryptionKeyRepository encrKeyRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _attachmentCollectionRepository = attachmentCollectionRepository;
            _encrKeyRepository = encrKeyRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get the attachment collection by ID
        /// </summary>
        /// <param name="attachmentCollectionId">The attachment collection Id</param>
        /// <returns>The <see cref="AttachmentCollection">Attachment Collection</see></returns>
        public async Task<AttachmentCollection> GetAttachmentCollectionByIdAsync(string attachmentCollectionId)
        {
            if (string.IsNullOrEmpty(attachmentCollectionId))
                throw new ArgumentNullException("attachmentCollectionId");

            // get the collection by ID
            var attachmentCollection = await _attachmentCollectionRepository.GetAttachmentCollectionByIdAsync(attachmentCollectionId);
            if (attachmentCollection == null)
                throw new KeyNotFoundException("Attachment collection not found");

            // get the user's role IDs
            var userRoles = await GetUserRoleIdsAsync();

            // verify the collection can be viewed
            if (!attachmentCollection.VerifyViewAttachmentCollection(CurrentUser.PersonId, userRoles))
            {
                string error = "User does not have permissions to view the collection";
                _logger.Error(string.Format("{0}, user = {1}, collection {2}", error, CurrentUser.PersonId, attachmentCollection.Id));
                throw new PermissionsException(error);
            }

            // convert collection entity to DTO
            var attachmentCollectionDtos = await ConvertAttachmentCollectionsToDtosAsync(
                new List<Domain.Base.Entities.AttachmentCollection>() { attachmentCollection });
            return attachmentCollectionDtos.FirstOrDefault();
        }

        /// <summary>
        /// Get the attachment collections for the user
        /// </summary>
        /// <returns>Attachment collections associated with the user</returns>
        public async Task<IEnumerable<AttachmentCollection>> GetAttachmentCollectionsByUserAsync()
        {
            // get the user's roles
            var userRoles = await GetUserRoleIdsAsync();

            // get the collections associated to this user
            var attachmentCollections = await _attachmentCollectionRepository.GetAttachmentCollectionsByUserAsync(CurrentUser.PersonId, userRoles);

            // convert collection entities to DTO
            return await ConvertAttachmentCollectionsToDtosAsync(attachmentCollections);
        }

        /// <summary>
        /// Create the new attachment collection
        /// </summary>
        /// <param name="attachmentCollection">The <see cref="AttachmentCollection">Attachment Collection</see> to create</param>
        /// <returns>Newly created <see cref="AttachmentCollection">Attachment Collection</see></returns>
        public async Task<AttachmentCollection> PostAttachmentCollectionAsync(AttachmentCollection attachmentCollection)
        {
            if (attachmentCollection == null)
                throw new ArgumentNullException("attachmentCollection");

            if (!HasPermission(BasePermissionCodes.CreateAttachmentCollection))
                throw new PermissionsException(string.Format("User {0} does not have permission to create an Attachment Collection", CurrentUser.UserId));

            // the owner is this user
            attachmentCollection.Owner = CurrentUser.PersonId;

            // convert collection DTO to entity
            var newAttachmentCollectionEntityIn = await ConvertAttachmentCollectionDtoToEntityAsync(attachmentCollection);

            // see if the collection exists
            var attachmentCollectionEntity = await _attachmentCollectionRepository.GetAttachmentCollectionByIdAsync(newAttachmentCollectionEntityIn.Id);
            if (attachmentCollectionEntity != null)
                throw new ArgumentException("Attachment collection already exists");

            // create the new collection
            var newAttachmentCollectionEntity = await _attachmentCollectionRepository.PostAttachmentCollectionAsync(newAttachmentCollectionEntityIn);

            // convert collection entity to DTO
            var attachmentCollectionDtos = await ConvertAttachmentCollectionsToDtosAsync(
                new List<Domain.Base.Entities.AttachmentCollection>() { newAttachmentCollectionEntity });
            return attachmentCollectionDtos.FirstOrDefault();
        }

        /// <summary>
        /// Update the attachment collection
        /// </summary>
        /// <param name="attachmentCollectionId">The ID of the attachment collection</param>
        /// <param name="attachmentCollection">The <see cref="AttachmentCollection">Attachment Collection</see> to update</param>
        /// <returns>The updated <see cref="AttachmentCollection">Attachment Collection</see></returns>
        public async Task<AttachmentCollection> PutAttachmentCollectionAsync(string attachmentCollectionId, AttachmentCollection attachmentCollection)
        {
            if (string.IsNullOrEmpty(attachmentCollectionId))
                throw new ArgumentNullException("attachmentCollectionId");
            if (attachmentCollection == null)
                throw new ArgumentNullException("attachmentCollection");
            if (attachmentCollectionId != attachmentCollection.Id)
                throw new ArgumentException("The attachment collection ID from the URL does not match the one in the body");

            // convert collection DTO to entity
            var updatedAttachmentCollectionEntityIn = await ConvertAttachmentCollectionDtoToEntityAsync(attachmentCollection);

            // verify the collection exists and check its permissions
            var attachmentCollectionEntity = await _attachmentCollectionRepository.GetAttachmentCollectionByIdAsync(updatedAttachmentCollectionEntityIn.Id);
            if (attachmentCollectionEntity == null)
                throw new KeyNotFoundException("The attachment collection does not exist");

            if (!attachmentCollectionEntity.VerifyUpdateAttachmentCollection(CurrentUser.PersonId))
            {
                string error = "The user does not have the permissions to update the collection";
                _logger.Error(string.Format("{0}, user = {1}, collection {2}", error, CurrentUser.PersonId, attachmentCollection.Id));
                throw new PermissionsException(error);
            }

            // update/create the collection
            var updatedAttachmentCollectionEntity = await _attachmentCollectionRepository.PutAttachmentCollectionAsync(updatedAttachmentCollectionEntityIn);

            // convert collection entity to DTO
            var attachmentCollectionDtos = await ConvertAttachmentCollectionsToDtosAsync(
                new List<Domain.Base.Entities.AttachmentCollection>() { updatedAttachmentCollectionEntity });
            return attachmentCollectionDtos.FirstOrDefault();
        }

        /// <summary>
        /// Get this user's effective permissions for the attachment collection
        /// </summary>
        /// <param name="attachmentCollectionId">The ID of the attachment collection</param>
        /// <returns>This user's <see cref="AttachmentCollectionEffectivePermissions">Attachment Collection Effective Permissions</see></returns>
        public async Task<AttachmentCollectionEffectivePermissions> GetEffectivePermissionsAsync(string attachmentCollectionId)
        {
            if (string.IsNullOrEmpty(attachmentCollectionId))
                throw new ArgumentNullException("attachmentCollectionId");
            
            // get the collection
            var attachmentCollection = await _attachmentCollectionRepository.GetAttachmentCollectionByIdAsync(attachmentCollectionId);
            if (attachmentCollection == null)
                throw new KeyNotFoundException("Attachment collection not found");

            // get the user's roles
            var userRoles = await GetUserRoleIdsAsync();

            // check permissions
            var effectivePermissions = new AttachmentCollectionEffectivePermissions
            {
                CanCreateAttachments = attachmentCollection.VerifyAttachmentAction(CurrentUser.PersonId, userRoles, Domain.Base.Entities.AttachmentAction.Create),
                CanDeleteAttachments = attachmentCollection.VerifyAttachmentAction(CurrentUser.PersonId, userRoles, Domain.Base.Entities.AttachmentAction.Delete),
                CanUpdateAttachments = attachmentCollection.VerifyAttachmentAction(CurrentUser.PersonId, userRoles, Domain.Base.Entities.AttachmentAction.Update),
                CanViewAttachments = attachmentCollection.VerifyAttachmentAction(CurrentUser.PersonId, userRoles, Domain.Base.Entities.AttachmentAction.View)
            };

            return effectivePermissions;
        }

        // Convert an attachment collection DTO to entity
        private async Task<Domain.Base.Entities.AttachmentCollection> ConvertAttachmentCollectionDtoToEntityAsync(
            AttachmentCollection attachmentCollectionDto)
        {
            // convert collection DTO to entity
            var dtoToEntityAdapter = _adapterRegistry.GetAdapter<AttachmentCollection, Domain.Base.Entities.AttachmentCollection>();
            var attachmentCollectionEntity = dtoToEntityAdapter.MapToType(attachmentCollectionDto);

            // verify the encryption key
            if (!string.IsNullOrEmpty(attachmentCollectionEntity.EncryptionKeyId))
            {
                await GetEncryptionKeyAsync(attachmentCollectionEntity.EncryptionKeyId);
            }

            if (attachmentCollectionEntity.Roles != null && attachmentCollectionEntity.Roles.Any())
            {
                // get roles
                var roleEntities = await _roleRepository.GetRolesAsync();
                if (roleEntities == null || !roleEntities.Any())
                    throw new Exception("No roles found for conversion of titles to IDs");
                
                foreach (var roleIdentity in attachmentCollectionEntity.Roles)
                {
                    // convert role title to its ID
                    var roleEntity = roleEntities.Where(r => r.Title == roleIdentity.Id).FirstOrDefault();
                    if (roleEntity == null)
                        throw new Exception(string.Format("Cannot convert role title {0} to its ID", roleIdentity.Id));

                    roleIdentity.Id = roleEntity.Id.ToString();
                }
            }

            return attachmentCollectionEntity;
        }

        // get the encryption key from the repository
        private async Task<Domain.Base.Entities.EncrKey> GetEncryptionKeyAsync(string encryptionKeyId)
        {
            // get the encryption key
            var encrKey = await _encrKeyRepository.GetKeyAsync(encryptionKeyId);
            if (encrKey == null)
                throw new KeyNotFoundException("Encryption key not found");
            // the key must be active
            if (encrKey.Status != Domain.Base.Entities.EncrKeyStatus.Active)
                throw new ArgumentException("The encryption key is not active");

            return encrKey;
        }

        // Convert role IDs in an attachment collection DTO to their associated titles
        private async Task<IEnumerable<AttachmentCollection>> ConvertAttachmentCollectionsToDtosAsync(
            IEnumerable<Domain.Base.Entities.AttachmentCollection> attachmentCollectionEntities)
        {
            var attachmentCollectionDtos = new List<AttachmentCollection>();

            if (attachmentCollectionEntities != null && attachmentCollectionEntities.Any())
            {
                // get roles
                var roleEntities = await _roleRepository.GetRolesAsync();
                if (roleEntities == null || !roleEntities.Any())
                    throw new Exception("No roles found for conversion of IDs to titles");

                var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.AttachmentCollection, AttachmentCollection>();
                
                foreach (var attachmentCollectionEntity in attachmentCollectionEntities)
                {
                    // convert collection entity to DTO
                    var attachmentCollectionDto = entityToDtoAdapter.MapToType(attachmentCollectionEntity);
                    if (attachmentCollectionDto != null && attachmentCollectionDto.Roles != null && attachmentCollectionDto.Roles.Any())
                    {
                        foreach (var roleIdentity in attachmentCollectionDto.Roles)
                        {
                            // convert the role ID to its title
                            var roleEntity = roleEntities.Where(r => r.Id.ToString() == roleIdentity.Id).FirstOrDefault();
                            if (roleEntity == null)
                                throw new Exception(string.Format("Cannot convert role ID {0} to its title", roleIdentity.Id));

                            roleIdentity.Id = roleEntity.Title;
                        }
                    }
                    attachmentCollectionDtos.Add(attachmentCollectionDto);
                }
            }

            return attachmentCollectionDtos;
        }
    }
}