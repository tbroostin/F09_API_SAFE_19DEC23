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
        /// Resource Version Number
        /// </summary>
        public string ApiVersionNumber { get; set; }

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
        /// List of the extended data for this resource and version 
        /// </summary>
        public IList<EthosExtensibleDataRow> ExtendedDataList { get; set; }

        /// <summary>
        /// List of the filter criteria for this resource and version 
        /// </summary>
        public IList<EthosExtensibleDataFilter> ExtendedDataFilterList { get; set; }

    }
}