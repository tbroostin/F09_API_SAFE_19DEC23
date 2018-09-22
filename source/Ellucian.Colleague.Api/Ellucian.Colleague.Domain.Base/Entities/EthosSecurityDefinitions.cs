// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    ///  Ethos Data Privacy Settings
    /// </summary>
    [Serializable]
    public class EthosSecurityDefinitions
    {
        /// <summary>
        /// String in a specific format for identifying which property to protect
        /// </summary>
        public string PropertyInformation { get; set; }

        /// <summary>
        /// username this protection applies to
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// user role this protection applies to
        /// </summary>
        public string UserRole { get; set; }

        /// <summary>
        /// user permission this protection applies to
        /// </summary>
        public string UserPermission { get; set; }

        /// <summary>
        /// bool that determines if the username/roles/permissions are required to view the data
        /// </summary>
        public bool RequiredToViewData { get; set; }

        /// <summary>
        /// bool that determines if the username/roles/poermissions are not allowed to view the data
        /// </summary>
        public bool NotAllowedToViewData { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiName">Name of API</param>
        /// <param name="propertySetting">property setting informatino</param>
        /// <param name="userName">username to secure by</param>
        /// <param name="userRole">user role to secure by</param>
        /// <param name="userPermission">user permission to secure by</param>
        /// <param name="requiredToViewProperty">If username/role/permission is required to see the property, else any user with the username/role/property will not be allowed to see it</param>
        public EthosSecurityDefinitions(string propertySetting, string userName, string userRole, string userPermission, bool requiredToViewProperty)
        {
            PropertyInformation = propertySetting;
            UserName = userName;
            UserRole = userRole;
            UserPermission = userPermission;
            RequiredToViewData = requiredToViewProperty;
            NotAllowedToViewData = !requiredToViewProperty;
        }
    }
}
