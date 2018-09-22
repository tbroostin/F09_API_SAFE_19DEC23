// Copyright 2017 - 2018 Ellucian Company L.P. and its affiliates.

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
        /// List of the extended data for this resource and version 
        /// </summary>
        public IList<EthosExtensibleDataRow> ExtendedDataList { get; private set; }

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
        /// Adds an item to the Extended Data list long as the object isnt null
        /// </summary>
        /// <param name="extDataRow">non null data row item</param>
        public void AddItemToExtendedData(EthosExtensibleDataRow extDataRow)
        {
            if (extDataRow != null)
            {
                ExtendedDataList.Add(extDataRow);
            }
        }
    }
}