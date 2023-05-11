// Copyright 2017 - 2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
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
        public string ApiResourceName { get; private set; }

        /// <summary>
        /// Resource Version Number
        /// </summary>
        public string ApiVersionNumber { get; private set; }

        /// <summary>
        /// Release Status of the Version
        /// </summary>
        public string VersionReleaseStatus { get; set; }

        /// <summary>
        /// The API Type of E, T, A, or S (Extension, Transaction (BPA), Spec Based API, or Subroutine Call)
        /// </summary>
        public string ApiType { get; set; }

        /// <summary>
        /// Extended Schema Type Identifier
        /// </summary>
        public string ExtendedSchemaType { get; private set; }

        /// <summary>
        /// Resource Id for the current Data
        /// </summary>
        public string ResourceId { get; private set; }

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
        /// List of the inquiry fields
        /// </summary>
        public IList<string> InquiryFields { get; set; }

        /// <summary>
        /// List of the filter criteria for this resource and version 
        /// </summary>
        public IList<EthosExtensibleDataFilter> ExtendedDataFilterList { get; private set; }

        /// <summary>
        /// The date that the API will no longer be supported
        /// </summary>
        public DateTime? DeprecationDate { get; set; }

        /// <summary>
        /// Notice of deprecation for documentation with the resources API
        /// </summary>
        public string DeprecationNotice { get; set; }

        /// <summary>
        /// The date that the API will no longer be available
        /// </summary>
        public DateTime? SunsetDate { get; set; }

        /// <summary>
        /// The HTTP methods supported by this API
        /// </summary>
        public List<string> HttpMethodsSupported { get; set; }

        /// <summary>
        /// The Parent API for alternate views
        /// </summary>
        public string ParentApi { get; set; }

        /// <summary>
        /// The overall description of the API for documentation with schemas API
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Current User ID path for getting records that match current API user only.
        /// </summary>
        public string CurrentUserIdPath { get; set; }

        /// <summary>
        /// Current User from API user required to pass data into repository
        /// </summary>
        public string CurrentUserId { get; set; }

        /// <summary>
        /// Unused at this time.
        /// </summary>
        public string Conversion { get; set; }

        /// <summary>
        /// Set to true if this is a custom resource
        /// </summary>
        public bool IsCustomResource { get; set; }


        /// <summary>
        /// Constructor for Ethos Extended Data
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceVersion"></param>
        /// <param name="extSchemaType"></param>
        /// <param name="resourceId"></param>
        /// <param name="collTimeZone"></param>
        /// <param name="dataList"></param>
        public EthosExtensibleData(string resourceName, string resourceVersion, string extSchemaType, string resourceId, string collTimeZone, IList<EthosExtensibleDataRow> dataList = null)
        {
            ApiResourceName = resourceName;
            ApiVersionNumber = resourceVersion;
            ExtendedSchemaType = extSchemaType;
            ResourceId = resourceId;
            ExtendedDataFilterList = new List<EthosExtensibleDataFilter>();

            if (dataList != null && dataList.Any())
            {
                ExtendedDataList = dataList;
            }
            else
            {
                ExtendedDataList = new List<EthosExtensibleDataRow>();
            }

            if (string.IsNullOrEmpty(collTimeZone))
            {
                ColleagueTimeZone = TimeZoneInfo.Local;
            }
            else
            {
                try
                {
                    ColleagueTimeZone = TimeZoneInfo.FindSystemTimeZoneById(collTimeZone);
                }
                catch 
                {
                    ColleagueTimeZone = TimeZoneInfo.Local;
                }
                
            }
            if (string.IsNullOrEmpty(ExtendedSchemaType))
            {
                if (!string.IsNullOrEmpty(ApiVersionNumber))
                {
                    ExtendedSchemaType = string.Format("application/vnd.hedtech.integration.v{0}+json", ApiVersionNumber);
                }
            }

        }/// <summary>
         /// Constructor for Ethos Extended Data
         /// </summary>
        public EthosExtensibleData()
        {
            ExtendedDataList = new List<EthosExtensibleDataRow>();
            ExtendedDataFilterList = new List<EthosExtensibleDataFilter>();
        }

        /// <summary>
        /// Updates the Extended Data list with a full set, only sets if the list coming in is not null and has something in it
        /// </summary>
        /// <param name="dataList">non null list with at least one item</param>
        public void UpdateExtendedData(IList<EthosExtensibleDataRow> dataList)
        {
            if (dataList != null && dataList.Any())
            {
                ExtendedDataList = dataList;
            }
        }

        /// <summary>
        /// Adds an item to the Extended Data list as long as the object isnt null
        /// </summary>
        /// <param name="extDataRow">non null data row item</param>
        public void AddItemToExtendedData(EthosExtensibleDataRow extDataRow)
        {
            if (extDataRow != null)
            {
                ExtendedDataList.Add(extDataRow);
            }
        }

        /// <summary>
        /// Updates the Extended Data filter list with a full set, only sets if the list coming in is not null and has something in it
        /// </summary>
        /// <param name="filterList">non null list with at least one item</param>
        public void UpdateExtendedDataFilter(IList<EthosExtensibleDataFilter> filterList)
        {
            if (filterList != null && filterList.Any())
            {
                ExtendedDataFilterList = filterList;
            }
        }

        /// <summary>
        /// Adds an item to the Extended Data Filter list as long as the object isnt null
        /// </summary>
        /// <param name="extDataRow">non null data row item</param>
        public void AddItemToExtendedDataFilter(EthosExtensibleDataFilter extDataFilter)
        {
            if (extDataFilter != null)
            {
                ExtendedDataFilterList.Add(extDataFilter);
            }
        }
    }
}