// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Proxy permissions being granted or revoked for a user
    /// </summary>
    [Serializable]
    public class ProxyPermissionAssignment
    {
        private string _proxySubjectId;
        /// <summary>
        /// ID of the user either granting or revoking proxy access
        /// </summary>
        public string ProxySubjectId { get { return _proxySubjectId; } }

        private readonly List<ProxyAccessPermission> _permissions = new List<ProxyAccessPermission>();
        /// <summary>
        /// Collection of permissions being changed for the proxied user
        /// </summary>
        public ReadOnlyCollection<ProxyAccessPermission> Permissions { get; private set; }

        /// <summary>
        /// Text of the approval document approved by the user when proxy access is granted initially
        /// </summary>
        public List<string> ProxySubjectApprovalDocumentText { get; set; }

        private bool _isReauthorizing;
        /// <summary>
        /// Flag indicating whether or not reauthorization is occurring
        /// </summary>
        public bool IsReauthorizing { get { return _isReauthorizing; } }

        /// <summary>
        /// Proposed email address of the proxy. 
        /// </summary>
        public string ProxyEmailAddress { get; set; }

        /// <summary>
        /// Proposed email type for the specified email address. Can be empty and determined by the system at update.
        /// </summary>
        public string ProxyEmailType { get; set; }

        /// <summary>
        /// Constructor for Proxy Permission Assignment
        /// </summary>
        /// <param name="proxySubjectId">ID of the person making the change in access</param>
        /// <param name="permissions">A collection of one or more permission changes to make</param>
        /// <param name="isReauthorizing">Flag indicating whether or not reauthorization is occurring</param>
        public ProxyPermissionAssignment(string proxySubjectId, IEnumerable<ProxyAccessPermission> permissions, bool isReauthorizing = false)
        {
            if (string.IsNullOrEmpty(proxySubjectId))
            {
                throw new ArgumentNullException("proxySubjectId", "Proxy Subject user ID must have a value.");
            }
            if (permissions == null || permissions.Count() == 0)
            {
                throw new ArgumentNullException("permissions", "Must provide at least one permission");
            }
            var idList = permissions.Select(p => p.ProxySubjectId).Distinct().ToList();
            if (idList.Count != 1 || idList[0] != proxySubjectId)
            {
                throw new ArgumentException("All permissions must be for same proxy subject user as on the assignment.", "permissions");
            }

            _proxySubjectId = proxySubjectId;
            _permissions.AddRange(permissions);
            _isReauthorizing = isReauthorizing;
            Permissions = _permissions.AsReadOnly();
            ProxySubjectApprovalDocumentText = new List<string>();
        }
    }
}
