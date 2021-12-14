// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Represents proxy configuration data
    /// </summary>
    public class ProxyConfiguration
    {
        /// <summary>
        /// Flag indicating whether or not proxy functionality is allowed
        /// </summary>
        public bool ProxyIsEnabled { get; set; }

        /// <summary>
        /// Code for the disclosure release approval document
        /// </summary>
        public string DisclosureReleaseDocumentId { get; set; }

        /// <summary>
        /// Disclosure release text
        /// </summary>
        public string DisclosureReleaseText { get; set; }

        /// <summary>
        /// Code for the proxy email document
        /// </summary>
        public string ProxyEmailDocumentId { get; set; }

        /// <summary>
        /// Flag indicating whether or not users may add non-existent/unrelated users as proxies
        /// </summary>
        public bool CanAddOtherUsers { get; set; }

        /// <summary>
        /// Text conveying information about the View/Add Third Party Access process
        /// </summary>
        public string ProxyFormHeaderText { get; set; }

        /// <summary>
        /// Text conveying information about the Add a Proxy workflow within the View/Add Third Party Access process
        /// </summary>
        public string AddProxyHeaderText { get; set; }

        /// <summary>
        /// Text conveying information about the user's need to reauthorize their granted access
        /// </summary>
        public string ReauthorizationText { get; set; }

        /// <summary>
        /// Proxy workflow groups
        /// </summary>
        public IEnumerable<ProxyWorkflowGroup> WorkflowGroups { get; set; }

        /// <summary>
        /// Codes of relationship types to which proxy access can be granted 
        /// </summary>
        public IEnumerable<string> RelationshipTypeCodes { get; set; }

        /// <summary>
        /// Demographic information potentially collected when adding persons to Colleague via Proxy functionality
        /// </summary>
        public IEnumerable<DemographicField> DemographicFields { get; set; }

        /// <summary>
        /// Flag indicating if the SSN is formatted. (###-##-####)
        /// </summary>
        public bool SSNIsFormatted { get; set; }

        /// <summary>
        /// Proxy and User Permission mapping objects
        /// </summary>
        public IEnumerable<ProxyAndUserPermissionsMap> ProxyAndUserPermissionsMap { get; set; }
    }
}