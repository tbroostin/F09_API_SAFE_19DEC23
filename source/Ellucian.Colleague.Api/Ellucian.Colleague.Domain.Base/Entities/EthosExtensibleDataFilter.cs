// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosExtensibleDataFilter
    {
        /// <summary>
        /// Column name from Colleague
        /// </summary>
        public string ColleagueColumnName { get; private set; }

        /// <summary>
        /// The Database Usage Type for the Colleague Column Name
        /// </summary>
        public string DatabaseUsageType { get; set; }

        /// <summary>
        /// File name from Colleague
        /// </summary>
        public string ColleagueFileName { get; private set; }

        /// <summary>
        /// Length of the property in Colleague if it has any
        /// </summary>
        public int? ColleaguePropertyLength { get; private set; }

        /// <summary>
        /// Position of the Colleague field within the file, primarily used for keys.
        /// </summary>
        public int? ColleagueFieldPosition { get; set; }

        /// <summary>
        /// Indicates whether this field is required 
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Description of the filter
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Title of the extended property in the Ethos json schema
        /// </summary>
        public string JsonTitle { get; private set; }

        /// <summary>
        /// Path of the extended property in the Ethos json schema
        /// </summary>
        public string JsonPath { get; private set; }

        /// <summary>
        /// Full json path, that being path + the property name
        /// </summary>
        public string FullJsonPath { get; private set; }

        /// <summary>
        /// Json Property type of the extended property in the Ethos json schema
        /// </summary>
        public string JsonPropertyType { get; private set; }

        /// <summary>
        /// The actual filter value to search for and respond with
        /// </summary>
        public List<string> FilterValue { get; private set; }

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
        /// Named Query flag
        /// </summary>
        public bool NamedQuery { get; set; }

        /// <summary>
        /// Key Query flag
        /// </summary>
        public bool KeyQuery { get; set; }

        /// <summary>
        /// constructor for the row of extended data
        /// </summary>
        /// <param name="colColumnName"></param>
        /// <param name="colFileName"></param>
        /// <param name="jsonTitle"></param>
        /// <param name="jsonPath"></param>
        /// <param name="jsonPropType"></param>
        /// <param name="filterValue"></param>
        /// <param name="length"></param>
        /// <param name="namedQuery"></param>
        public EthosExtensibleDataFilter(string colColumnName, string colFileName, string jsonTitle, string jsonPath, string jsonPropType, List<string> filterValue, 
                int? length = null, bool namedQuery = false, bool keyQuery = false)
        {
            ColleagueColumnName = colColumnName;
            ColleagueFileName = colFileName;
            JsonTitle = jsonTitle;
            JsonPath = jsonPath;
            JsonPropertyType = jsonPropType;
            FilterValue = filterValue;
            ColleaguePropertyLength = length;
            SelectionCriteria = new List<EthosApiSelectCriteria>();
            SortColumns = new List<EthosApiSortCriteria>();
            Enumerations = new List<EthosApiEnumerations>();
            NamedQuery = namedQuery;
            KeyQuery = keyQuery;
            //build the full json patch path which includes the property itself
            
            var sb = new StringBuilder();

            if (!jsonPath.StartsWith("/") && !string.IsNullOrEmpty(jsonPath))
            {
                sb.Append("/");
            }

            //just make sure the path ends with / and if it doesn't add it to the end of it
            if (jsonPath.EndsWith("/"))
            {
                sb.Append(jsonPath);
            }
            else
            {
                sb.Append(jsonPath);
                sb.Append("/");
            }

            //now make sure the property name doesn't start with / and if so trim off
            sb.Append(jsonTitle.StartsWith("/") ? jsonTitle.TrimStart(new char[] {'/'}) : jsonTitle);

            FullJsonPath = sb.ToString();
            FullJsonPath = FullJsonPath.TrimStart(new char[] { '/' }).Replace('/', '.').TrimEnd(']').TrimEnd('[');
        }
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