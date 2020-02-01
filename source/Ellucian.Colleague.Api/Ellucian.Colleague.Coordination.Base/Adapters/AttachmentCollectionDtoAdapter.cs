// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates a <see cref="AttachmentCollection"/> from a <see cref="Dtos.Base.AttachmentCollection"/>
    /// </summary>
    public class AttachmentCollectionDtoAdapter : AutoMapperAdapter<Dtos.Base.AttachmentCollection, AttachmentCollection>
    {
        /// <summary>
        /// Instantiates a new <see cref="AttachmentCollectionDtoAdapter"/>
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public AttachmentCollectionDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.Base.AttachmentCollectionStatus, AttachmentCollectionStatus>();
            AddMappingDependency<Dtos.Base.AttachmentAction, AttachmentAction>();
            AddMappingDependency<Dtos.Base.AttachmentCollectionIdentity, AttachmentCollectionIdentity>();
            AddMappingDependency<Dtos.Base.AttachmentCollectionIdentityType, AttachmentCollectionIdentityType>();
            AddMappingDependency<Dtos.Base.AttachmentOwnerAction, AttachmentOwnerAction>();
        }

        /// <summary>
        /// Maps a <see cref="AttachmentCollection"/> from a <see cref="Dtos.Base.AttachmentCollection"/>
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Base.AttachmentCollection"/> to map</param>
        /// <returns>The mapped <see cref="AttachmentCollection"/></returns>
        public override AttachmentCollection MapToType(Dtos.Base.AttachmentCollection source)
        {
            return new AttachmentCollection(source.Id, source.Name, source.Owner)
            {
                AllowedContentTypes = source.AllowedContentTypes,
                AttachmentOwnerActions = ConvertAttachmentOwnerActions(source.AttachmentOwnerActions),
                Description = source.Description,
                EncryptionKeyId = source.EncryptionKeyId,
                MaxAttachmentSize = source.MaxAttachmentSize,
                RetentionDuration = source.RetentionDuration,
                Roles = ConvertIdentities(source.Roles, Dtos.Base.AttachmentCollectionIdentityType.Role),
                Status = ConvertStatus(source.Status),
                Users = ConvertIdentities(source.Users, Dtos.Base.AttachmentCollectionIdentityType.User)
            };
        }

        // Convert collection DTO status to entity
        private AttachmentCollectionStatus ConvertStatus(Dtos.Base.AttachmentCollectionStatus status)
        {
            switch (status)
            {
                case Dtos.Base.AttachmentCollectionStatus.Active:
                    return AttachmentCollectionStatus.Active;
                case Dtos.Base.AttachmentCollectionStatus.Inactive:
                    return AttachmentCollectionStatus.Inactive;
                default:
                    return AttachmentCollectionStatus.Active;
            }
        }

        // Convert a list of collection DTO owner actions to entity ones
        private IEnumerable<AttachmentOwnerAction> ConvertAttachmentOwnerActions(IEnumerable<Dtos.Base.AttachmentOwnerAction> ownerActions)
        {
            var ownerActionEntities = new List<AttachmentOwnerAction>();

            if (ownerActions != null && ownerActions.Any())
            {
                foreach (var action in ownerActions)
                {
                    ownerActionEntities.Add(ConvertAttachmentOwnerAction(action));
                }
            }

            return ownerActionEntities;
        }

        // Convert attachment DTO owner action to entity
        private AttachmentOwnerAction ConvertAttachmentOwnerAction(Dtos.Base.AttachmentOwnerAction ownerAction)
        {
            switch (ownerAction)
            {
                case Dtos.Base.AttachmentOwnerAction.Delete:
                    return AttachmentOwnerAction.Delete;
                case Dtos.Base.AttachmentOwnerAction.Update:
                    return AttachmentOwnerAction.Update;
                default:
                    throw new ArgumentException("Cannot convert collection owner action");
            }
        }

        // Convert a list of collection DTO actions to entity ones
        private IEnumerable<AttachmentAction> ConvertAttachmentActions(IEnumerable<Dtos.Base.AttachmentAction> actions)
        {
            var actionEntities = new List<AttachmentAction>();

            if (actions != null && actions.Any())
            {
                foreach (var action in actions)
                {
                    actionEntities.Add(ConvertAttachmentAction(action));
                }
            }

            return actionEntities;
        }

        // Convert attachment DTO action to entity
        private AttachmentAction ConvertAttachmentAction(Dtos.Base.AttachmentAction action)
        {
            switch (action)
            {
                case Dtos.Base.AttachmentAction.Create:
                    return AttachmentAction.Create;
                case Dtos.Base.AttachmentAction.Delete:
                    return AttachmentAction.Delete;
                case Dtos.Base.AttachmentAction.Update:
                    return AttachmentAction.Update;
                case Dtos.Base.AttachmentAction.View:
                    return AttachmentAction.View;
                default:
                    throw new ArgumentException("Cannot convert collection action");
            }
        }

        // Convert a list of identity DTOs to entities
        private IEnumerable<AttachmentCollectionIdentity> ConvertIdentities(IEnumerable<Dtos.Base.AttachmentCollectionIdentity> identities,
            Dtos.Base.AttachmentCollectionIdentityType type)
        {
            var identityEntities = new List<AttachmentCollectionIdentity>();

            if (identities != null && identities.Any())
            {
                foreach (var identity in identities)
                {
                    if (identity != null)                    
                        identityEntities.Add(new AttachmentCollectionIdentity(identity.Id, ConvertIdentityType(type), ConvertAttachmentActions(identity.Actions)));
                }
            }

            return identityEntities;
        }

        // Convert identity type DTO to entity
        private AttachmentCollectionIdentityType ConvertIdentityType(Dtos.Base.AttachmentCollectionIdentityType type)
        {
            switch (type)
            {
                case Dtos.Base.AttachmentCollectionIdentityType.Role:
                    return AttachmentCollectionIdentityType.Role;
                case Dtos.Base.AttachmentCollectionIdentityType.User:
                    return AttachmentCollectionIdentityType.User;
                default:
                    throw new ArgumentException("Cannot convert identity type");
            }
        }
    }
}