// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosExtensibleDataRow
    {
        /// <summary>
        /// Association
        /// </summary>
        public string AssociationController { get; set; }
        
        /// <summary>
        /// Transaction Type
        /// </summary>
        public string TransType { get; set; }

        /// <summary>
        /// Database Usage Type
        /// </summary>
        public string DatabaseUsageType { get; set; }

        /// <summary>
        /// Indicates whether this field is required 
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Description of the Data Row
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Data Conversion associated to the Data Row (such as Date or number)
        /// </summary>
        public string Conversion { get; set; }

        /// <summary>
        /// Collumn name from Colleague
        /// </summary>
        public string ColleagueColumnName { get; private set; }

        /// <summary>
        /// Translation Column Name for straight translations
        /// </summary>
        public string TransColumnName { get; set; }

        /// <summary>
        /// Translation Valcode Table Name where the translation is defined
        /// </summary>
        public string TransTableName { get; set; }

        /// <summary>
        /// Translation File Name where the translation is defined
        /// </summary>
        public string TransFileName { get; set; }

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
        public int? ColleaguePropertyPosition { get; set; }

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
        /// The actual data in the colleague field to return to the API call
        /// </summary>
        public string ExtendedDataValue { get; private set; }
        
        /// <summary>
        /// constructor for the row of extended data
        /// </summary>
        /// <param name="colColumnName"></param>
        /// <param name="colFileName"></param>
        /// <param name="jsonTitle"></param>
        /// <param name="jsonPath"></param>
        /// <param name="jsonPropType"></param>
        /// <param name="extendedDataValue"></param>
        /// <param name="length"></param>
        public EthosExtensibleDataRow(string colColumnName, string colFileName, string jsonTitle, string jsonPath, string jsonPropType, string extendedDataValue, int? length = null)
        {
            ColleagueColumnName = colColumnName;
            ColleagueFileName = colFileName;
            JsonTitle = jsonTitle;
            JsonPath = jsonPath;
            JsonPropertyType = jsonPropType;
            ExtendedDataValue = extendedDataValue;
            ColleaguePropertyLength = length;

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

        }
    }
}