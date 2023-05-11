// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Web.Http.EthosExtend
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosExtensibleDataRow
    {
        /// <summary>
        /// Collumn name from Colleague
        /// </summary>
        public string ColleagueColumnName { get; set; }

        /// <summary>
        /// File name from Colleague
        /// </summary>
        public string ColleagueFileName { get; set; }

        /// <summary>
        /// Position of the Colleague field within the file, primarily used for keys.
        /// </summary>
        public int? ColleaguePropertyPosition { get; set; }

        /// <summary>
        /// Length of the property in Colleague if it has any
        /// </summary>
        public int? ColleaguePropertyLength { get; set; }

        /// <summary>
        /// Indicates whether this field is required 
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Title of the extended property in the Ethos json schema
        /// </summary>
        public string JsonTitle { get; set; }

        /// <summary>
        /// Path of the extended property in the Ethos json schema
        /// </summary>
        public string JsonPath { get; set; }

        /// <summary>
        /// Full json path, that being path + the property name
        /// </summary>
        public string FullJsonPath { get; set; }

        /// <summary>
        /// Json Property type of the extended property in the Ethos json schema
        /// </summary>
        public JsonPropertyTypeExtensions JsonPropertyType { get; set; }

        /// <summary>
        /// The actual data in the colleague field to return to the API call
        /// </summary>
        public string ExtendedDataValue { get; set; }

        /// <summary>
        /// Association Controller
        /// </summary>
        public string AssociationController { get; set; }

        /// <summary>
        /// Database Usage Type
        /// </summary>
        public string UsageType { get; set; }

        /// <summary>
        /// Translation Type
        /// </summary>
        public string TransType { get; set; }
    }

    public enum JsonPropertyTypeExtensions
    {
        String,
        Number,
        Date,
        DateTime,
        Time
    }
}
