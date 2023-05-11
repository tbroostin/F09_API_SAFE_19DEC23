﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Ellucian.Web.Http.EthosExtend
{
    /// <summary>
    /// Route resource information for Ethos endpoints
    /// </summary>
    [Serializable]
    public class EthosResourceRouteInfo
    {
        /// <summary>
        /// Name of the resource (api)
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Name of the parent resource (used for alternative representations)
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// Version Number for the resource
        /// </summary>
        public string ResourceVersionNumber { get; set; }

        /// <summary>
        /// Version Number for the resource
        /// </summary>
        public string RequestedVersionNumber { get; set; }

        /// <summary>
        /// Version Number status (beta) for the resource
        /// </summary>
        public string ResourceVersionStatus { get; set; }

        /// <summary>
        /// Full representation of the custom media type
        /// </summary>
        public string EthosResourceIdentifier { get; set; }

        /// <summary>
        /// Extended Schema Identifier
        /// </summary>
        public string ExtendedSchemaResourceId { get; set; }

        /// <summary>
        /// Extended Filters from URI or request body (QAPI)
        /// </summary>
        public Dictionary<string, Tuple<List<string>, string>> ExtendedFilterDefinitions { get; set; }

        /// <summary>
        /// Bulk representation version
        /// </summary>
        public string BulkRepresentation { get; set; }

        /// <summary>
        /// System setting for Bypassing Cache for configuration data
        /// </summary>
        public bool BypassCache { get; set; }

        /// <summary>
        /// Check to see if we should be reporting Ethos Extend Errors for stand-alone
        /// spec driven APIs.
        /// </summary>
        public bool ReportEthosExtendedErrors { get; set; }

        /// <summary>
        /// The requested HTTP Method
        /// </summary>
        public HttpMethod RequestMethod { get; set; }

        /// <summary>
        /// True if Query By Post has been requested
        /// </summary>
        public bool isQueryByPost { get; set; }

        /// <summary>
        /// Current User ID for getting records that match current API user only.
        /// </summary>
        public string CurrentUserIdPath { get; set; }

        /// <summary>
        /// If the Request Header "Accept-Restricted-Fields" has been set to "*" we need to retrieve restricted field data
        /// </summary>
        public bool ReturnRestrictedFields { get; set; }

        public EthosResourceRouteInfo()
        {
            ExtendedFilterDefinitions = new Dictionary<string, Tuple<List<string>, string>>();
            ReportEthosExtendedErrors = false;
        }
    }
}
