// Copyright 2019 Ellucian Company L.P.and its affiliates.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// File attachment collection
    /// </summary>
    [Serializable]
    public class AttachmentCollection
    {
        /// <summary>
        /// ID of the attachment collection
        /// </summary>
        private string _Id;
        public string Id
        {
            get { return _Id; }
            set
            {
                if (string.IsNullOrEmpty(_Id))
                {
                    // enforce naming rules (ALPHA  DIGIT  "-" / "_")
                    if (!Regex.IsMatch(value, "^[a-zA-Z0-9-_]+$"))
                        throw new ArgumentException("Invalid attachment collection ID");

                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Attachment Collection Id cannot be changed.");
                }
            }
        }

        /// <summary>
        /// The attachment collection's display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of this attachment collection's purpose
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Colleague person Id who created/owns this collection - the owner is the collection admin
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Collections's status (e.g. active, inactive)
        /// </summary>
        public AttachmentCollectionStatus Status { get; set; }

        /// <summary>
        /// List of actions the attachment owner can take upon attachments they own in this collection
        /// </summary>
        public IEnumerable<AttachmentOwnerAction> AttachmentOwnerActions { get; set; }

        /// <summary>
        /// List of allowed content types in this collection
        /// </summary>
        public IEnumerable<string> AllowedContentTypes { get; set; }

        /// <summary>
        /// Max size, in bytes, of attachments that can be created in this collection
        /// </summary>
        public long? MaxAttachmentSize { get; set; }

        /// <summary>
        /// List of individual users and actions they can take upon attachments in this collection
        /// </summary>
        public IEnumerable<AttachmentCollectionIdentity> Users { get; set; }

        /// <summary>
        /// List of individual roles and actions they can take upon attachments in this collection
        /// </summary>
        public IEnumerable<AttachmentCollectionIdentity> Roles { get; set; }

        /// <summary>
        /// ISO-8601 duration, example: 5 years would be 'P5Y'
        /// </summary>
        private string _RetentionDuration;
        public string RetentionDuration
        {
            get { return _RetentionDuration; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // verify the retention duration format
                    XmlConvert.ToTimeSpan(value);
                    _RetentionDuration = value;
                }
                else
                {
                    _RetentionDuration = value;
                }
            }
        }

        /// <summary>
        /// ID of the encryption key used to encrypt the attachment content keys in this collection
        /// </summary>
        public string EncryptionKeyId { get; set;  }

        /// <summary>
        /// Create a file attachment collection
        /// </summary>
        /// <param name="id">Attachment collection ID</param>
        /// <param name="name">Attachment collection name</param>
        /// <param name="owner">Attachment collection owner</param>
        public AttachmentCollection(string id, string name, string owner)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(owner))
                throw new ArgumentNullException("owner");

            Id = id;
            Name = name;
            Owner = owner;
        }

        /// <summary>
        /// Verify if the user can perform the action against the given attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment"></param>
        /// <param name="user">The current user</param>
        /// <param name="userRoles">The current user's roles</param>
        /// <param name="action">The attachment action to verify</param>
        /// <returns>True if the user can perform the action</returns>
        public bool VerifyAttachmentAction(Attachment attachment, string user, IEnumerable<string> userRoles, AttachmentAction action)
        {
            switch (action)
            {
                case AttachmentAction.Create:
                    return (this.VerifyCreateAttachment(attachment, user, userRoles));
                case AttachmentAction.Delete:
                    return (this.VerifyDeleteAttachment(attachment, user, userRoles));
                case AttachmentAction.Update:
                    return (this.VerifyUpdateAttachment(attachment, user, userRoles));
                case AttachmentAction.View:
                    return (this.VerifyViewAttachment(attachment, user, userRoles));
                default:
                    throw new ArgumentException("Unknown attachment action");
            }
        }

        /// <summary>
        /// Verify if the user can perform the action against attachments, in general, based on the collection permissions
        /// </summary>
        /// <param name="user">The current user</param>
        /// <param name="userRoles">The current user's roles</param>
        /// <param name="action">The attachment action to verify</param>
        /// <returns>True if the user can perform the action</returns>
        public bool VerifyAttachmentAction(string user, IEnumerable<string> userRoles, AttachmentAction action)
        {
            if (string.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");

            return VerifyAttachmentActionPermissions(user, userRoles, action);
        }

        /// <summary>
        /// Verify if the user can create the given attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment"></param>
        /// <param name="user">The current user</param>
        /// <param name="userRoles">The current user's roles</param>
        /// <returns>True if the user can create attachments in this collection</returns>
        private bool VerifyCreateAttachment(Attachment attachment, string user, IEnumerable<string> userRoles)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");
            if (string.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");
            if (Status != AttachmentCollectionStatus.Active)
                throw new ArgumentException("Attachment collection is not active");
            if (attachment.Size > MaxAttachmentSize)
                throw new ArgumentException("Attachment size exceeds the collection's max attachment size");
            if (AllowedContentTypes != null &&
                !AllowedContentTypes.Contains(attachment.ContentType, StringComparer.Create(CultureInfo.CurrentCulture, true)))
                throw new ArgumentException("Attachment content type not allowed by collection");

            return VerifyAttachmentActionPermissions(user, userRoles, AttachmentAction.Create);
        }

        /// <summary>
        /// Verify if the user can update the given attachment
        /// </summary>        
        /// <param name="attachment">The <see cref="Attachment"></param>
        /// <param name="user">The current user</param>
        /// <param name="userRoles">The current user's roles</param>
        /// <returns>True if the attachment can be updated by the user</returns>
        private bool VerifyUpdateAttachment(Attachment attachment, string user, IEnumerable<string> userRoles)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");
            if (string.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");
            if (Status != AttachmentCollectionStatus.Active)
                throw new ArgumentException("Attachment collection is not active");
            if (attachment.Size > MaxAttachmentSize)
                throw new ArgumentException("Attachment size exceeds the collection's max attachment size");
            if (AllowedContentTypes != null &&
                !AllowedContentTypes.Contains(attachment.ContentType, StringComparer.Create(CultureInfo.CurrentCulture, true)))
                throw new ArgumentException("Attachment content type not allowed by collection");

            // verify action
            return VerifyAttachmentActionPermissions(attachment, user, userRoles, AttachmentAction.Update);
        }

        /// <summary>
        /// Verify if the user can delete the given attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment"></param>
        /// <param name="user">The current user</param>
        /// <param name="userRoles">The current user's roles</param>
        /// <returns>True if the user can delete the attachment</returns>
        private bool VerifyDeleteAttachment(Attachment attachment, string user, IEnumerable<string> userRoles)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");
            if (string.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");
            if (Status != AttachmentCollectionStatus.Active)
                throw new ArgumentException("Attachment collection is not active");
            if (attachment.Status == AttachmentStatus.Deleted)
                throw new ArgumentException("Attachment already has a status of deleted");

            // verify action
            return VerifyAttachmentActionPermissions(attachment, user, userRoles, AttachmentAction.Delete);
        }

        /// <summary>
        /// Verify if the user can view the given attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment"></param>
        /// <param name="user">The current user</param>
        /// <param name="userRoles">The current user's roles</param>
        /// <returns>True if the user can view the attachment</returns>
        private bool VerifyViewAttachment(Attachment attachment, string user, IEnumerable<string> userRoles)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");
            if (string.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");

            // verify action
            return VerifyAttachmentActionPermissions(attachment, user, userRoles, AttachmentAction.View);
        }

        // Verify if the user can perform the action against the given attachment
        private bool VerifyAttachmentActionPermissions(Attachment attachment, string user, IEnumerable<string> userRoles, AttachmentAction action)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");
            if (string.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");

            bool allowAction = false;

            // the collection must be active
            if (Status == AttachmentCollectionStatus.Active)
            {
                // check if the user is the attachment owner and what owner actions are allowed
                if (user == attachment.Owner )
                {
                    if (action == AttachmentAction.View)
                        allowAction = true;  // owner's view permission is implied

                    if (!allowAction && AttachmentOwnerActions != null && AttachmentOwnerActions.Any())
                        allowAction = AttachmentOwnerActions.Where(a => a.ToString() == action.ToString()).Any();
                }

                if (!allowAction)
                    allowAction = VerifyAttachmentActionPermissions(user, userRoles, action);
            }

            return allowAction;
        }

        // Verify if the user can perform the action against attachments, in general, based on the collection permissions
        private bool VerifyAttachmentActionPermissions(string user, IEnumerable<string> userRoles, AttachmentAction action)
        {
            if (string.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");

            bool allowAction = false;

            // the collection must be active
            if (Status == AttachmentCollectionStatus.Active)
            {
                bool continueCheck = false;

                if (action == AttachmentAction.Create)
                {
                    // the create action does not need the view action too.  Users can create attachments in a collection
                    // without being able to view others
                    continueCheck = true;
                }
                else
                {
                    // check for the view action first.  If the user cannot view, then no other actions are allowed

                    // check if the user is in the collection's users list with the view action
                    if (!continueCheck && Users != null && Users.Any())
                        continueCheck = Users.Where(u => u.Id == user && u.Actions.Contains(AttachmentAction.View)).Any();

                    // check if the user has a role in the collection's role list with the view action
                    if (!continueCheck && userRoles != null && userRoles.Any() && Roles != null && Roles.Any())
                        continueCheck = Roles.Where(r => userRoles.Contains(r.Id) && r.Actions.Contains(AttachmentAction.View)).Any();
                }

                if (continueCheck)
                {
                    if (action == AttachmentAction.View)
                        allowAction = true;  // already checked the view action

                    // check if the user is in the collection's users list with the action
                    if (!allowAction && Users != null && Users.Any())
                        allowAction = Users.Where(u => u.Id == user && u.Actions.Contains(action)).Any();

                    // check if the user has a role in the collection's role list with the action
                    if (!allowAction && userRoles != null && userRoles.Any() && Roles != null && Roles.Any())
                        allowAction = Roles.Where(r => userRoles.Contains(r.Id) && r.Actions.Contains(action)).Any();
                }
            }

            return allowAction;
        }

        /// <summary>
        /// Verify if the user can create/update the attachment collection
        /// </summary>
        /// <param name="user">The current user</param>
        /// <returns>True if the attachment collection can be created/updated by the user</returns>
        public bool VerifyUpdateAttachmentCollection(string user)
        {
            if (string.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");

            return (Owner == user);
        }

        /// <summary>
        /// Verify if the user can view the attachment collection
        /// </summary>
        /// <param name="user">The current user</param>
        /// <param name="userRoles">The current user's roles</param>
        /// <returns>True if the attachment collection can be viewed by the user</returns>
        public bool VerifyViewAttachmentCollection(string user, IEnumerable<string> userRoles)
        {
            if (string.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");

            bool allowView = false;

            // check if the user is the attachment collection owner
            if (Owner == user)
                allowView = true;

            // check if the user is in the collection's users list
            if (!allowView && Users != null && Users.Any())
                allowView = Users.Where(u => u.Id == user && u.Actions.Contains(AttachmentAction.View)).Any();

            // check if the user has a role in the collection's role list
            if (!allowView && userRoles != null && userRoles.Any() && Roles != null && Roles.Any())
                allowView = Roles.Where(r => userRoles.Contains(r.Id) && r.Actions.Contains(AttachmentAction.View)).Any();

            return allowView;
        }        
    }
}