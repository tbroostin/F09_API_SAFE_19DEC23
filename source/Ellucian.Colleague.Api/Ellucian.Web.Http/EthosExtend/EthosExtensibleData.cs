// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;


namespace Ellucian.Web.Http.EthosExtend
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosExtensibleData
    {
        /// <summary>
        /// Resource Name
        /// </summary>
        public string ApiResourceName { get; set; }

        /// <summary>
        /// The API Type of E, T, A, or S (Extension, Transaction (BPA), Spec Based API, or Subroutine Call)
        /// </summary>
        public string ApiType { get; set; }

        /// <summary>
        /// Resource Version Number
        /// </summary>
        public string ApiVersionNumber { get; set; }

        /// <summary>
        /// Release Status of the Version
        /// </summary>
        public string VersionReleaseStatus { get; set; }

        /// <summary>
        /// Extended Schema Type Identifier
        /// </summary>
        public string ExtendedSchemaType { get; set; }

        /// <summary>
        /// Resource Id for the current Data
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Timezone set in Colleague
        /// </summary>
        public TimeZoneInfo ColleagueTimeZone { get; set; }

        /// <summary>
        /// List of file used by an API
        /// </summary>
        public List<string> ColleagueFileNames { get; set; }

        /// <summary>
        /// List of record keys required for a Business Process API call (ApiType of "T")
        /// </summary>
        public IList<string> ColleagueKeyNames { get; set; }

        /// <summary>
        /// List of the extended data for this resource and version 
        /// </summary>
        public IList<EthosExtensibleDataRow> ExtendedDataList { get; set; }

        /// <summary>
        /// List of the filter criteria for this resource and version 
        /// </summary>
        public IList<EthosExtensibleDataFilter> ExtendedDataFilterList { get; set; }

        /// <summary>
        /// Current User ID path for getting records that match current API user only.
        /// </summary>
        public string CurrentUserIdPath { get; set; }

        /// <summary>
        /// Current User from API user required to pass data into repository
        /// </summary>
        public string CurrentUserId { get; set; }

    }
}