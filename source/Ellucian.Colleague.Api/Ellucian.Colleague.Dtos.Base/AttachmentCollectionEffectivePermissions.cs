// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Attachment collection effective permissions
    /// </summary>
    public class AttachmentCollectionEffectivePermissions
    {
        /// <summary>
        /// True if the the user can view attachments in this collection
        /// </summary>
        public bool CanViewAttachments { get; set; }

        /// <summary>
        /// True if the user can create attachments in this collection
        /// </summary>
        public bool CanCreateAttachments { get; set; }

        /// <summary>
        /// True if the user can update attachments in this collection
        /// </summary>
        public bool CanUpdateAttachments { get; set; }

        /// <summary>
        /// True if the user can delete attachments in this collection
        /// </summary>
        public bool CanDeleteAttachments { get; set; }
    }
}