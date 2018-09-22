// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Proxy permissions being granted or revoked for a user. 
    /// </summary>
    public class ProxyPermissionAssignment
    {
        /// <summary>
        /// ID of the user either granting or revoking proxy access
        /// </summary>
        public string ProxySubjectId { get; set; }
        /// <summary>
        /// Collection of permissions being changed for the proxy subject user
        /// </summary>
        public List<ProxyAccessPermission> Permissions { get; set; }
        /// <summary>
        /// Text of the approval document approved by the user when proxy access is granted initially
        /// </summary>
        public List<string> ProxySubjectApprovalDocumentText { get; set; }
        /// <summary>
        /// Flag indicating whether or not reauthorization is occurring
        /// </summary>
        public bool IsReauthorizing { get; set; }
        /// <summary>
        /// Proposed email address of the proxy if proxy does not have an existing email address. Permissions cannot
        /// be granted to a proxy that does not have a valid email address.
        /// </summary>
        public string ProxyEmailAddress { get; set; }
        /// <summary>
        /// Proposed email type for the specified email address. Can be empty and determined by the system at update.
        /// </summary>
        public string ProxyEmailType { get; set; }
    }
}
