// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Coordination service for Attachments
    /// </summary>
    [RegisterType]
    public class AttachmentService : BaseCoordinationService, IAttachmentService
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IAttachmentCollectionRepository _attachmentCollectionRepository;
        private readonly IEncryptionKeyRepository _encrKeyRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for AttachmentService
        /// </summary>
        /// <param name="adapterRegistry">Adaper registry</param>
        /// <param name="attachmentRepository">Attachment repository</param>
        /// <param name="attachmentCollectionRepository">Attachment collection repository</param>
        /// <param name="currentUserFactory">The current user factory</param>
        /// <param name="encrKeyRepository">Encryption key repository</param>
        /// <param name="roleRepository">The role repository</param>
        /// <param name="logger">The logger</param>
        public AttachmentService(IAdapterRegistry adapterRegistry, IAttachmentRepository attachmentRepository, 
            IAttachmentCollectionRepository attachmentCollectionRepository, ICurrentUserFactory currentUserFactory,
            IEncryptionKeyRepository encrKeyRepository, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _attachmentRepository = attachmentRepository;
            _attachmentCollectionRepository = attachmentCollectionRepository;
            _encrKeyRepository = encrKeyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get the attachment temp file path on the server
        /// </summary>
        /// <returns>The attachment temp file path on the server</returns>
        public string GetAttachTempFilePath()
        {
            return _attachmentRepository.GetAttachTempFilePath();
        }

        /// <summary>
        /// Get attachments
        /// </summary>
        /// <param name="owner">Owner to get attachments for</param>
        /// <param name="collectionId">Collection Id to get attachments for</param>
        /// <param name="tagOne">TagOne value to get attachments for</param>
        /// <returns>List of <see cref="Attachment">Attachments</see></returns>
        public async Task<IEnumerable<Attachment>> GetAttachmentsAsync(string owner, string collectionId, string tagOne)
        {
            if (string.IsNullOrEmpty(owner) && string.IsNullOrEmpty(collectionId))
            {
                // default to getting attachments for this user
                owner = CurrentUser.PersonId;
            }                

            var attachmentDtos = new List<Attachment>();

            // get attachments
            var attachments = await _attachmentRepository.GetAttachmentsAsync(owner, collectionId, tagOne);
            if (attachments != null && attachments.Any())
            {
                // get attachments this user can view
                var viewableAttachments = await GetActionableAttachmentsAsync(attachments, Domain.Base.Entities.AttachmentAction.View);
                if (viewableAttachments != null && viewableAttachments.Any())
                {
                    // convert collection entities to DTOs
                    var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Attachment, Attachment>();
                    viewableAttachments.ToList().ForEach(a => attachmentDtos.Add(adapter.MapToType(a)));
                }
            }

            return attachmentDtos;
        }

        /// <summary>
        /// Get the attachment's content
        /// </summary>
        /// <param name="attachmentId">Id of the attachment's content to get</param>
        /// <returns>Tuple of the attachment, its temp file location, and encryption metadata</returns>
        public async Task<Tuple<Attachment, string, AttachmentEncryption>> GetAttachmentContentAsync(string attachmentId)
        {
            if (string.IsNullOrEmpty(attachmentId))
                throw new ArgumentNullException("attachmentId");

            // get the attachment and its encryption metadata
            var attachment = await _attachmentRepository.GetAttachmentByIdWithEncrMetadataAsync(attachmentId);
            if (attachment == null)
                throw new KeyNotFoundException("Attachment not found");
            if (attachment.Status != Domain.Base.Entities.AttachmentStatus.Active)
                throw new KeyNotFoundException("The attachment is not active");

            // verify the user can view the attachment
            var viewableAttachments = await GetActionableAttachmentsAsync(
                new List<Domain.Base.Entities.Attachment>() { attachment }, Domain.Base.Entities.AttachmentAction.View);
            if (viewableAttachments == null || !viewableAttachments.Any())
            {
                string error = "User does not have permissions to view the content of the attachment";
                _logger.Error(string.Format("{0}, user = {1}, attachment {2} ({3}) in collection {4}",
                    error, CurrentUser.PersonId, attachment.Id, attachment.Name, attachment.CollectionId));
                throw new PermissionsException(error);
            }

            // verify the encryption key
            if (!string.IsNullOrEmpty(attachment.EncrKeyId))
            {
                await GetEncryptionKeyAsync(attachment.EncrKeyId);
            }

            // get the content temp file path
            var tempAttachmentPath = await _attachmentRepository.GetAttachmentContentAsync(attachment);
            if (string.IsNullOrEmpty(tempAttachmentPath))
                throw new KeyNotFoundException("Attachment content not found");

            // convert attachment entity to DTO
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Attachment, Attachment>();
            var attachmentDto = adapter.MapToType(attachment);

            // return the attachment metadata and content
            return new Tuple<Attachment, string, AttachmentEncryption>(attachmentDto, tempAttachmentPath,
                GetAttachmentEncryptionMetadata(attachment));
        }

        /// <summary>
        /// Create the new attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to create</param>
        /// <param name="attachmentEncryption">The <see cref="AttachmentEncryption">AttachmentEncryption</see> metadata</param>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        public async Task<Attachment> PostAttachmentAsync(Attachment attachment, AttachmentEncryption attachmentEncryption)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");

            // convert the attachment DTO to entity and verify the user has create permissions
            var attachmentEntityIn = await ConvertAttachmentDtoToEntityCreateAsync(attachment, attachmentEncryption);

            // create the new attachment
            var attachmentEntityOut = await _attachmentRepository.PostAttachmentAsync(attachmentEntityIn);

            // get the adapter to convert attachment entity to DTO
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Attachment, Attachment>();

            return entityToDtoAdapter.MapToType(attachmentEntityOut);
        }

        /// <summary>
        /// Create the new attachment w/ content
        /// </summary>
        /// <param name="attachment">The attachment's metadata</param>
        /// <param name="attachmentEncryption">The <see cref="AttachmentEncryption">AttachmentEncryption</see> metadata</param>
        /// <param name="attachmentContentStream">Stream to the attachment content</param>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        public async Task<Attachment> PostAttachmentAndContentAsync(
            Attachment attachment, AttachmentEncryption attachmentEncryption, Stream attachmentContentStream)
        {            
            if (attachment == null)
                throw new ArgumentNullException("attachment");
            if (attachmentContentStream == null || attachmentContentStream.Length <= 0)
                throw new ArgumentNullException("attachmentContentStream");
            if (!attachment.Size.HasValue)
                throw new ArgumentException("Attachment size is required");

            // convert the attachment DTO to entity and verify the user has create permissions
            var attachmentEntityIn = await ConvertAttachmentDtoToEntityCreateAsync(attachment, attachmentEncryption);

            // create the new attachment w/ content
            var attachmentEntityOut = await _attachmentRepository.PostAttachmentAndContentAsync(
                attachmentEntityIn, attachmentContentStream);

            // get the adapter to convert attachment entity to DTO
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Attachment, Attachment>();

            return entityToDtoAdapter.MapToType(attachmentEntityOut);
        }

        /// <summary>
        /// Update the attachment
        /// </summary>
        /// <param name="attachmentId">ID of the attachment to update</param>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to update</param>
        /// <returns>The updated <see cref="Attachment">Attachment</see></returns>
        public async Task<Attachment> PutAttachmentAsync(string attachmentId, Attachment attachment)
        {
            if (string.IsNullOrEmpty(attachmentId))
                throw new ArgumentNullException("attachmentId");
            if (attachment == null)
                throw new ArgumentNullException("attachment");
            if (attachmentId != attachment.Id)
                throw new ArgumentException("The attachment ID from the URL does not match the one in the body");

            // convert attachment DTO to entity
            var dtoToEntityAdapter = _adapterRegistry.GetAdapter<Attachment, Domain.Base.Entities.Attachment>();
            var attachmentEntityIn = dtoToEntityAdapter.MapToType(attachment);

            // verify the attachment exists and get its existing metadata
            var existingAttachmentEntity = await _attachmentRepository.GetAttachmentByIdWithEncrMetadataAsync(attachmentEntityIn.Id);
            if (existingAttachmentEntity == null)
                throw new KeyNotFoundException("The attachment does not exist");

            // verify the user can update the attachment
            var updateableAttachments = await GetActionableAttachmentsAsync(new List<Domain.Base.Entities.Attachment>() { existingAttachmentEntity },
                Domain.Base.Entities.AttachmentAction.Update);
            if (updateableAttachments == null || !updateableAttachments.Any())
            {
                string error = "User does not have permissions to update the attachment";
                _logger.Error(string.Format("{0}, user = {1}, attachment {2} ({3}) in collection {4}",
                    error, CurrentUser.PersonId, existingAttachmentEntity.Id, existingAttachmentEntity.Name, existingAttachmentEntity.CollectionId));
                throw new PermissionsException(error);
            }

            // verify the encryption key
            if (!string.IsNullOrEmpty(existingAttachmentEntity.EncrKeyId))
            {
                await GetEncryptionKeyAsync(existingAttachmentEntity.EncrKeyId);
            }

            // only allowed to change the name of the attachment
            existingAttachmentEntity.Name = attachmentEntityIn.Name;

            // update the attachment
            var attachmentEntityOut = await _attachmentRepository.PutAttachmentAsync(existingAttachmentEntity);

            // get the adapter to convert attachment entity to DTO
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Attachment, Attachment>();

            return entityToDtoAdapter.MapToType(attachmentEntityOut);
        }

        /// <summary>
        /// Delete the attachment
        /// </summary>
        /// <param name="attachmentId">Id of the attachment to delete</param>
        public async Task DeleteAttachmentAsync(string attachmentId)
        {
            if (string.IsNullOrEmpty(attachmentId))
                throw new ArgumentNullException("attachmentId");

            // get the attachment
            var attachment = await _attachmentRepository.GetAttachmentByIdNoEncrMetadataAsync(attachmentId);
            if (attachment == null)
                throw new KeyNotFoundException("Attachment not found");

            // verify the user can delete the attachment
            var deletableAttachments = await GetActionableAttachmentsAsync(new List<Domain.Base.Entities.Attachment>() { attachment },
                Domain.Base.Entities.AttachmentAction.Delete);
            if (deletableAttachments == null || !deletableAttachments.Any())
            {
                string error = "User does not have permissions to delete attachment";
                _logger.Error(string.Format("{0}, user = {1}, attachment {2} ({3}) in collection {4}",
                    error, CurrentUser.PersonId, attachment.Id, attachment.Name, attachment.CollectionId));
                throw new PermissionsException(error);
            }

            // delete the attachment
            await _attachmentRepository.DeleteAttachmentAsync(attachment);
        }

        // create the attachment entity
        private async Task<Domain.Base.Entities.Attachment> ConvertAttachmentDtoToEntityCreateAsync(Attachment attachment, AttachmentEncryption attachmentEncryption)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");

            // generate an ID
            attachment.Id = Guid.NewGuid().ToString();

            // assign an owner.
            // Others are allowed to create an attachment with a different owner, if they pass the create permission check
            if (string.IsNullOrEmpty(attachment.Owner) || attachment.Owner == CurrentUser.PersonId)
            {
                attachment.Owner = CurrentUser.PersonId;
            }

            // set to active status
            attachment.Status = AttachmentStatus.Active;

            // convert attachment DTO to entity
            var dtoToEntityAdapter = _adapterRegistry.GetAdapter<Attachment, Domain.Base.Entities.Attachment>();
            var attachmentEntity = dtoToEntityAdapter.MapToType(attachment);
            // add the encryption metadata
            if (attachmentEncryption != null)
            {
                attachmentEntity.EncrContentKey = attachmentEncryption.EncrContentKey;
                attachmentEntity.EncrIV = attachmentEncryption.EncrIV;
                attachmentEntity.EncrKeyId = attachmentEncryption.EncrKeyId;
                attachmentEntity.EncrType = attachmentEncryption.EncrType;
            }

            // verify the user can create the attachment
            var creatableAttachments = await GetActionableAttachmentsAsync(new List<Domain.Base.Entities.Attachment>() { attachmentEntity },
                Domain.Base.Entities.AttachmentAction.Create);
            if (creatableAttachments == null || !creatableAttachments.Any())
            {
                string error = "User does not have permissions to create attachment";
                _logger.Error(string.Format("{0}, user = {1}, attachment {2} ({3}) in collection {4}",
                    error, CurrentUser.PersonId, attachment.Id, attachment.Name, attachment.CollectionId));
                throw new PermissionsException(error);
            }

            // verify the encryption key
            if (!string.IsNullOrEmpty(attachmentEntity.EncrKeyId))
            {
                await GetEncryptionKeyAsync(attachmentEntity.EncrKeyId);
            }

            return attachmentEntity;
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

        // determine which attachments the user has the permissions to take the action on
        private async Task<IEnumerable<Domain.Base.Entities.Attachment>> GetActionableAttachmentsAsync(IEnumerable<Domain.Base.Entities.Attachment> attachments,
            Domain.Base.Entities.AttachmentAction action)
        {
            var actionableAttachments = new List<Domain.Base.Entities.Attachment>();

            if (attachments != null && attachments.Any())
            {
                // get the associated collections to verify permissions
                var attachmentCollections = await _attachmentCollectionRepository.GetAttachmentCollectionsByIdAsync(
                    attachments.Select(a => a.CollectionId).ToList().Distinct());
                if (attachmentCollections != null && attachmentCollections.Any())
                {
                    // get the user's role IDs
                    var userRoles = await GetUserRoleIdsAsync();

                    // group the attachments by collection
                    var attachmentsByCollection = attachments.ToLookup(a => a.CollectionId);
                    foreach (var attachmentByCollection in attachmentsByCollection)
                    {
                        var collection = attachmentCollections.Where(c => c.Id == attachmentByCollection.Key).FirstOrDefault();
                        if (collection != null)
                        {
                            // loop through each attachment to verify the action
                            foreach (var attachment in attachmentByCollection)
                            {
                                if (collection.VerifyAttachmentAction(attachment, CurrentUser.PersonId, userRoles, action))
                                    actionableAttachments.Add(attachment);
                            }
                        }
                    }
                }
            }

            return actionableAttachments;
        }

        // return an attachment's encryption metadata
        private AttachmentEncryption GetAttachmentEncryptionMetadata(Domain.Base.Entities.Attachment attachment)
        {
            if (attachment == null || string.IsNullOrEmpty(attachment.EncrKeyId))
                return null;

            // encryption metadata
            return new AttachmentEncryption(attachment.EncrKeyId, attachment.EncrType, attachment.EncrContentKey,
                attachment.EncrIV);
        }
    }
}