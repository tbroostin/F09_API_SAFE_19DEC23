// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Web.Http.EthosExtend
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosExtensibleDataFilter
    {
        /// <summary>
        /// Collumn name from Colleague
        /// </summary>
        public string ColleagueColumnName { get; set; }

        /// <summary>
        /// The Database Usage Type for the Colleague Column Name
        /// </summary>
        public string DatabaseUsageType { get; set; }

        /// <summary>
        /// File name from Colleague
        /// </summary>
        public string ColleagueFileName { get; set; }

        /// <summary>
        /// Length of the property in Colleague if it has any
        /// </summary>
        public int? ColleaguePropertyLength { get; set; }

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
        /// The actual filter value to search for and respond with
        /// </summary>
        public List<string> FilterValue { get; set; }

        /// <summary>
        /// The filter operator to use when searching ($eq, $ne, $lte, etc.)
        /// </summary>
        public string FilterOper { get; set; }

        /// <summary>
        /// Selection File Name for retrieval of records
        /// </summary>
        public string SelectFileName { get; set; }

        /// <summary>
        /// Selection Subroutine Name for retrieval of records
        /// </summary>
        public string SelectSubroutineName { get; set; }

        /// <summary>
        /// The column name when the criteria is applied to a filter or named query
        /// </summary>
        public string SelectColumnName { get; set; }

        /// <summary>
        /// List of Selection Criteria building elements
        /// </summary>
        public List<EthosApiSelectCriteria> SelectionCriteria { get; set; }

        /// <summary>
        /// Paragraph used to further down selection of records.
        /// </summary>
        public List<string> SelectParagraph { get; set; }

        /// <summary>
        /// Rules to be applied to selected records to further narrow down selection
        /// </summary>
        public List<string> SelectRules { get; set; }

        /// <summary>
        /// Sort columns and operations when sorting is applied.
        /// </summary>
        public List<EthosApiSortCriteria> SortColumns { get; set; }

        /// <summary>
        /// Saving Field Name
        /// </summary>
        public string SavingField { get; set; }

        /// <summary>
        /// Saving Field Option
        /// </summary>
        public string SavingOption { get; set; }

        /// <summary>
        /// Database Usage Type that defines the GUID translation value
        /// </summary>
        public string GuidDatabaseUsageType { get; set; }

        /// <summary>
        /// Column Name that defines the GUID translation value
        /// </summary>
        public string GuidColumnName { get; set; }

        /// <summary>
        /// File Name where the GUID has been defined for translation
        /// </summary>
        public string GuidFileName { get; set; }

        /// <summary>
        /// Translation Column Name for straight translations
        /// </summary>
        public string TransColumnName { get; set; }

        /// <summary>
        /// Translation File Name where the translation is defined
        /// </summary>
        public string TransFileName { get; set; }

        /// <summary>
        /// Translation Valcode Table Name where the translation is defined
        /// </summary>
        public string TransTableName { get; set; }

        /// <summary>
        /// Enumeration table for translations
        /// </summary>
        public List<EthosApiEnumerations> Enumerations { get; set; }

        /// <summary>
        /// The valid filter operators available to use when searching ($eq, $ne, $lte, etc.)
        /// </summary>
        public List<string> ValidFilterOpers { get; set; }

        /// <summary>
        /// Contains the named query name or "criteria" for validation in controller of the query string request
        /// </summary>
        public string QueryName { get; set; }
    }

    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosApiEnumerations
    {
        public string EnumerationValue { get; set; }

        public string ColleagueValue { get; set; }

        public EthosApiEnumerations(string enumeration, string value)
        {
            EnumerationValue = enumeration;
            ColleagueValue = value;
        }
    }
}