// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for attachment data.
    /// </summary>
    [RegisterType]
    public class AttachmentCollectionRepository : BaseColleagueRepository, IAttachmentCollectionRepository
    {
        private const string collectionActiveStatus = "A";
        private const string collectionInactiveStatus = "I";

        private const string collectionCreateAction = "C";
        private const string collectionDeleteAction = "D";
        private const string collectionUpdateAction = "U";
        private const string collectionViewAction = "V";

        /// <summary>
        /// Constructor for AttachmentRepository
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public AttachmentCollectionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
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
            var collectionRecord = await DataReader.ReadRecordAsync<AttachmentCollections>(attachmentCollectionId);

            return BuildCollections(new List<AttachmentCollections>() { collectionRecord }).FirstOrDefault();
        }

        /// <summary>
        /// Get the attachment collections by list of IDs
        /// </summary>
        /// <param name="attachmentCollectionIds">The attachment collection Id</param>
        /// <returns>List of <see cref="AttachmentCollection">Attachment Collection</see></returns>
        public async Task<IEnumerable<AttachmentCollection>> GetAttachmentCollectionsByIdAsync(IEnumerable<string> attachmentCollectionIds)
        {
            if (attachmentCollectionIds == null || !attachmentCollectionIds.Any())
                throw new ArgumentNullException("attachmentCollectionIds");

            // get the collections by ID
            var collectionRecords = await DataReader.BulkReadRecordAsync<AttachmentCollections>(attachmentCollectionIds.ToArray());

            return BuildCollections(collectionRecords);
        }

        /// <summary>
        /// Get the attachment collections for the user
        /// </summary>
        /// <param name="personId">Person ID of the user</param>
        /// <param name="roleIds">User's role Ids</param>
        /// <returns>List of <see cref="AttachmentCollection">Attachment Collections</see></returns>
        public async Task<IEnumerable<AttachmentCollection>> GetAttachmentCollectionsByUserAsync(string personId, IEnumerable<string> roleIds)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId");

            // get if owner, user, or role
            var criteria = new StringBuilder();
            criteria.Append(string.Format("WITH ATCOL.OWNER EQ '{0}' OR ( ATCOL.USERS.ID EQ '{1}' )", personId, personId));
            if (roleIds != null && roleIds.Any())
            {
                var rolesArray = roleIds.ToArray();
                criteria.Append(" OR (");
                for (int roleIdx = 0; roleIdx < roleIds.Count(); roleIdx++)
                {
                    if (roleIdx == 0)
                        criteria.Append(string.Format(" ATCOL.ROLES.ID EQ '{0}'", rolesArray[roleIdx]));
                    else
                        criteria.Append(string.Format(" OR ATCOL.ROLES.ID EQ '{0}'", rolesArray[roleIdx]));
                }
                criteria.Append(" )");
            }
            criteria.Append(string.Format(" AND ATCOL.STATUS EQ '{0}'", collectionActiveStatus));
            var collectionRecords = await DataReader.BulkReadRecordAsync<AttachmentCollections>(criteria.ToString());

            return BuildCollections(collectionRecords);
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

            try
            {
                // call the CTX to update the collection
                var createResponse = await transactionInvoker.ExecuteAsync<UpdateAttachmentCollectionsRequest,
                    UpdateAttachmentCollectionsResponse>(BuildUpdateRequest(attachmentCollection));
                if (createResponse == null)
                {
                    string error = "No response received creating attachment collection";
                    logger.Error(string.Format("{0}, collection {1}", error, attachmentCollection.Id));
                    throw new RepositoryException(error);
                }
                if (!string.IsNullOrEmpty(createResponse.ErrorMsg))
                {
                    string error = "Error occurred creating attachment collection";
                    logger.Error(string.Format("{0}, collection {1} : error = {2}", error, attachmentCollection.Id, createResponse.ErrorMsg));
                    throw new RepositoryException(error);
                }
            }
            catch (ColleagueTransactionException cte)
            {
                string error = "Exception occurred creating attachment collection";
                logger.Error(cte, string.Format("{0}, collection {1} : error = {2}", error, attachmentCollection.Id, cte.Message));
                throw new RepositoryException(error);
            }

            // return the newly created attachment collection
            return await GetAttachmentCollectionByIdAsync(attachmentCollection.Id);
        }

        /// <summary>
        /// Update the attachment collection
        /// </summary>
        /// <param name="attachmentCollection">The <see cref="AttachmentCollection">Attachment Collection</see> to update</param>
        /// <returns>The updated <see cref="AttachmentCollection">Attachment Collection</see></returns>
        public async Task<AttachmentCollection> PutAttachmentCollectionAsync(AttachmentCollection attachmentCollection)
        {
            if (attachmentCollection == null)
                throw new ArgumentNullException("attachmentCollection");
            
            try
            {
                // call the CTX to update the collection
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAttachmentCollectionsRequest,
                    UpdateAttachmentCollectionsResponse>(BuildUpdateRequest(attachmentCollection));
                if (updateResponse == null)
                {
                    string error = "No response received updating attachment collection";
                    logger.Error(string.Format("{0}, collection {1}", error, attachmentCollection.Id));
                    throw new RepositoryException(error);
                }
                if (!string.IsNullOrEmpty(updateResponse.ErrorMsg))
                {
                    string error = "Error occurred updating attachment collection";
                    logger.Error(string.Format("{0}, collection {1} : error = {2}", error, attachmentCollection.Id, updateResponse.ErrorMsg));
                    throw new RepositoryException(error);
                }
            }
            catch (ColleagueTransactionException cte)
            {
                string error = "Exception occurred updating attachment collection";
                logger.Error(cte, string.Format("{0}, collection {1} : error = {2}", error, attachmentCollection.Id, cte.Message));
                throw new RepositoryException(error);
            }

            // return the updated attachment collection
            return await GetAttachmentCollectionByIdAsync(attachmentCollection.Id);
        }

        // Create collection entities from the associated data contracts
        private IEnumerable<AttachmentCollection> BuildCollections(IEnumerable<AttachmentCollections> collectionRecords)
        {
            var collectionEntities = new List<AttachmentCollection>();

            if (collectionRecords != null && collectionRecords.Any())
            {
                foreach (var record in collectionRecords)
                {
                    if (record != null)
                    {
                        // create the entity
                        var collectionEntity = new AttachmentCollection(record.Recordkey, record.AtcolName, record.AtcolOwner)
                        {                            
                            AllowedContentTypes = ConvertCollectionContentTypesToEntities(record.AtcolAllowedContentTypes),
                            AttachmentOwnerActions = ConvertCollectionOwnerActionsToEntities(record.AtcolOwnerActions),
                            Description = record.AtcolDescription,
                            EncryptionKeyId = record.AtcolEncrKeyId,
                            MaxAttachmentSize = record.AtcolMaxAttachmentSize,
                            RetentionDuration = record.AtcolRetainDuration,
                            Roles = ConvertRolesToEntities(record.AtcolRolesEntityAssociation),
                            Status = ConvertCollectionStatusToEntity(record.AtcolStatus),
                            Users = ConvertUsersToEntities(record.AtcolUsersEntityAssociation)
                        };

                        // add to collection
                        collectionEntities.Add(collectionEntity);
                    }
                }
            }

            return collectionEntities;
        }

        // Convert an collection data contract status to entity
        private AttachmentCollectionStatus ConvertCollectionStatusToEntity(string status)
        {
            switch (status)
            {
                case collectionActiveStatus:
                    return AttachmentCollectionStatus.Active;
                case collectionInactiveStatus:
                    return AttachmentCollectionStatus.Inactive;
                default:
                    return AttachmentCollectionStatus.Active;
            }
        }

        // Convert an collection entity status to internal
        private string ConvertCollectionStatusToInternal(AttachmentCollectionStatus status)
        {
            switch (status)
            {
                case AttachmentCollectionStatus.Active:
                    return collectionActiveStatus;
                case AttachmentCollectionStatus.Inactive:
                    return collectionInactiveStatus;
                default:
                    return collectionActiveStatus;
            }
        }

        // Convert list of collection owner action strings to entities
        private IEnumerable<AttachmentOwnerAction> ConvertCollectionOwnerActionsToEntities(IEnumerable<string> ownerActions)
        {
            var ownerActionEntities = new List<AttachmentOwnerAction>();

            if (ownerActions != null && ownerActions.Any())
            {
                foreach (var action in ownerActions)
                {
                    if (!string.IsNullOrEmpty(action))
                        ownerActionEntities.Add(ConvertCollectionOwnerActionToEntity(action));
                }
            }

            return ownerActionEntities;
        }

        // Convert collection owner action string to entity
        private AttachmentOwnerAction ConvertCollectionOwnerActionToEntity(string ownerAction)
        {
            switch (ownerAction)
            {
                case collectionDeleteAction:
                    return AttachmentOwnerAction.Delete;
                case collectionUpdateAction:
                    return AttachmentOwnerAction.Update;
                default:
                    throw new ArgumentException("Cannot convert attachment owner action");
            }
        }

        // Convert collection owner action entity to string
        private string ConvertCollectionOwnerActionToInternal(AttachmentOwnerAction ownerAction)
        {
            switch (ownerAction)
            {
                case AttachmentOwnerAction.Delete:
                    return collectionDeleteAction;
                case AttachmentOwnerAction.Update:
                    return collectionUpdateAction;
                default:
                    throw new ArgumentException("Cannot convert attachment owner action to internal");
            }
        }

        // Convert list of collection action strings to entities
        private IEnumerable<AttachmentAction> ConvertCollectionActionsToEntities(IEnumerable<string> actions)
        {
            var actionEntities = new List<AttachmentAction>();

            if (actions != null && actions.Any())
            {
                foreach (var action in actions)
                {
                    if (!string.IsNullOrEmpty(action))
                        actionEntities.Add(ConvertCollectionActionToEntity(action));
                }
            }

            return actionEntities;
        }

        // Convert collection action string to entity
        private AttachmentAction ConvertCollectionActionToEntity(string action)
        {
            switch (action)
            {
                case collectionCreateAction:
                    return AttachmentAction.Create;
                case collectionDeleteAction:
                    return AttachmentAction.Delete;
                case collectionUpdateAction:
                    return AttachmentAction.Update;
                case collectionViewAction:
                    return AttachmentAction.View;
                default:
                    throw new ArgumentException("Cannot convert attachment action");
            }
        }

        // Convert collection action entity to internal
        private string ConvertCollectionActionToInternal(AttachmentAction action)
        {
            switch (action)
            {
                case AttachmentAction.Create:
                    return collectionCreateAction;
                case AttachmentAction.Delete:
                    return collectionDeleteAction;
                case AttachmentAction.Update:
                    return collectionUpdateAction;
                case AttachmentAction.View:
                    return collectionViewAction;
                default:
                    throw new ArgumentException("Cannot convert attachment action to internal");
            }
        }

        // Convert role data contract to entity
        private IEnumerable<AttachmentCollectionIdentity> ConvertRolesToEntities(IEnumerable<AttachmentCollectionsAtcolRoles> roles)
        {
            IEnumerable<AttachmentCollectionIdentity> roleEntities = new List<AttachmentCollectionIdentity>();

            if (roles != null && roles.Any())
            {
                // group the actions by role ID and get the list of actions
                roleEntities = roles.GroupBy(
                    r => r.AtcolRolesIdAssocMember,
                    r => r.AtcolRolesActionsAssocMember,
                    (key, actions) => new AttachmentCollectionIdentity(key, AttachmentCollectionIdentityType.Role,
                        ConvertCollectionActionsToEntities(actions.ToList())));
            }

            return roleEntities;
        }

        // Convert user data contract to entity
        private IEnumerable<AttachmentCollectionIdentity> ConvertUsersToEntities(IEnumerable<AttachmentCollectionsAtcolUsers> users)
        {
            IEnumerable<AttachmentCollectionIdentity> userEntities = new List<AttachmentCollectionIdentity>();

            if (users != null && users.Any())
            {
                // group the actions by user ID and get the list of actions
                userEntities = users.GroupBy(
                    r => r.AtcolUsersIdAssocMember,
                    r => r.AtcolUsersActionsAssocMember,
                    (key, actions) => new AttachmentCollectionIdentity(key, AttachmentCollectionIdentityType.User,
                        ConvertCollectionActionsToEntities(actions.ToList())));
            }

            return userEntities;
        }

        // Convert list of content type internal strings to entities
        private IEnumerable<string> ConvertCollectionContentTypesToEntities(IEnumerable<string> contentTypes)
        {
            var contentTypeEntities = new List<string>();

            if (contentTypes != null && contentTypes.Any())
            {
                foreach (var contentType in contentTypes)
                {
                    if (!string.IsNullOrEmpty(contentType))
                        contentTypeEntities.Add(ConvertCollectionContentTypeToEntity(contentType));
                }
            }

            return contentTypeEntities;
        }

        // Convert collection content type to entity
        private string ConvertCollectionContentTypeToEntity(string contentType)
        {
            switch (contentType)
            {
                case "PDF":
                    return "application/pdf";
                case "JPEG":
                    return "image/jpeg";
                case "PNG":
                    return "image/png";
                case "GIF":
                    return "image/gif";
                case "BITMAP":
                    return "image/bmp";
                case "TIFF":
                    return "image/tiff";
                default:
                    throw new ArgumentException("Cannot convert attachment content type");
            }
        }

        // Convert collection content type entity to internal
        private string ConvertCollectionContentTypeToInternal(string contentType)
        {
            switch (contentType.ToLower())
            {
                case "application/pdf":
                    return "PDF";
                case "image/jpeg":
                    return "JPEG";
                case "image/png":
                    return "PNG";
                case "image/gif":
                    return "GIF";
                case "image/bmp":
                    return "BITMAP";
                case "image/tiff":
                    return "TIFF";
                default:
                    throw new ArgumentException("Cannot convert attachment content type to internal");
            }
        }

        // Create an update request from a collection entity
        private UpdateAttachmentCollectionsRequest BuildUpdateRequest(AttachmentCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            var request = new UpdateAttachmentCollectionsRequest()
            {
                Id = collection.Id,
                Description = collection.Description,
                EncrKeyId = collection.EncryptionKeyId,
                MaxAttachmentSize = collection.MaxAttachmentSize.ToString(),
                Name = collection.Name,
                Owner = collection.Owner,
                RetainDuration = collection.RetentionDuration,
                Status = ConvertCollectionStatusToInternal(collection.Status)
            };

            if (collection.AttachmentOwnerActions != null && collection.AttachmentOwnerActions.Any())
            {
                request.AttachmentOwnerActions = collection.AttachmentOwnerActions.ToList()
                    .ConvertAll(oa => ConvertCollectionOwnerActionToInternal(oa));
            }

            if (collection.AllowedContentTypes != null && collection.AllowedContentTypes.Any())
            {
                request.AllowedContentTypes = collection.AllowedContentTypes.ToList()
                    .ConvertAll(ct => ConvertCollectionContentTypeToInternal(ct));
            }

            // convert roles
            if (collection.Roles != null && collection.Roles.Any())
            {
                foreach (var role in collection.Roles)
                {
                    if (role.Actions != null && role.Actions.Any())
                    {
                        // roles are stored with the role name repeated.  Create a separate one per action
                        foreach (var action in role.Actions)
                        {
                            request.Roles.Add(new Roles() { RolesId = role.Id, RolesActions = ConvertCollectionActionToInternal(action) });
                        }
                    }
                }
            }

            // convert users
            if (collection.Users != null && collection.Users.Any())
            {
                foreach (var user in collection.Users)
                {
                    if (user.Actions != null && user.Actions.Any())
                    {
                        // users are stored with the user name repeated.  Create a separate one per action
                        foreach (var action in user.Actions)
                        {
                            request.Users.Add(new Users() { UsersId = user.Id, UsersActions = ConvertCollectionActionToInternal(action) });
                        }
                    }
                }
            }

            return request;
        }
    }
}