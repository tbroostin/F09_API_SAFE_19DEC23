// Copyright 2017 Ellucian Company L.P. and its affiliates.

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
        /// Collumn name from Colleague
        /// </summary>
        public string ColleagueColumnName { get; private set; }

        /// <summary>
        /// File name from Colleague
        /// </summary>
        public string ColleagueFileName { get; private set; }

        /// <summary>
        /// Length of the property in Colleague if it has any
        /// </summary>
        public int? ColleaguePropertyLength { get; private set; }

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