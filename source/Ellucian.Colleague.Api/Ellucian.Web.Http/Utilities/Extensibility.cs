// Copyright 2017 - 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ellucian.Web.Http.EthosExtend;
using JsonDiffPatch;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Newtonsoft.Json.Linq;
using slf4net;
using Tavis;

namespace Ellucian.Web.Http.Utilities
{
    public static class Extensibility
    {
        /// <summary>
        /// Apply Extensibility to a single or list of json objects
        /// </summary>
        /// <param name="jsonToExtend">json body to extend</param>
        /// <param name="extendedDataList">settings and data to extend</param>
        /// <param name="logger">logger</param>
        /// <returns></returns>
        public static JContainer ApplyExtensibility(JContainer jsonToExtend, IList<EthosExtensibleData> extendedDataList, ILogger logger)
        {
            if (extendedDataList == null || !extendedDataList.Any())
            {
                return null;
            }

            try
            {
                if (jsonToExtend.Type == JTokenType.Array)
                {
                    var jArray = new JArray();
                    
                    jsonToExtend.ForEach(j =>
                    {
                        var id = (string)j.SelectToken("$.id", false);
                        if(string.IsNullOrEmpty(id))
                        {
                            jArray.Add(j);
                            return;
                        }
                        var extendData = extendedDataList.FirstOrDefault(e => e.ResourceId.Equals(id));
                        if (extendData == null)
                        {
                            jArray.Add(j);
                            return;
                        }
                        var extendedObject = ExtendSingleJsonObject(j, extendData, logger);
                        if(extendedObject == null)
                        {
                            jArray.Add(j);
                            return;
                        }
                        jArray.Add(extendedObject);
                    });
                    return jArray;
                }

                if (jsonToExtend.Type == JTokenType.Object)
                {
                    return (JContainer)ExtendSingleJsonObject(jsonToExtend, extendedDataList.First(), logger);
                }
                
            }
            catch (Exception ex)
            {
                if (logger == null) return null;

                var sb = new StringBuilder();
                sb.Append("Extensiblity failed to apply for the following API and resources.");
                sb.AppendLine();
                extendedDataList.ForEach(e =>
                {
                    sb.Append(" API : ");
                    sb.Append(e.ApiResourceName);
                    sb.Append(" Version : ");
                    sb.Append(e.ApiVersionNumber);
                    sb.Append(" Id : ");
                    sb.Append(e.ResourceId);
                });

                logger.Error(ex, sb.ToString());

                return null;
            }

            return null;

        }

        private static JToken ExtendSingleJsonObject(JToken jsonObjectToExtend, EthosExtensibleData extendConfig, ILogger logger)
        {
            //check extended data sent in, make sure it isn't null, row collection isn't null and it has something in it
            if (extendConfig == null || extendConfig.ExtendedDataList == null || !extendConfig.ExtendedDataList.Any())
            {
                return null; 
            }

            try
            {
                var objectToReturn = jsonObjectToExtend;
                var patcher = new JsonPatcher();

                foreach (var extendedData in extendConfig.ExtendedDataList)
                {
                    var propSplit =
                        extendedData.FullJsonPath.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

                    if (!propSplit.Any()) continue;

                    var count = propSplit.Count();

                    if (count == 1)
                    {
                        patcher.Patch(ref objectToReturn,
                            new PatchDocument(CreateAddOperation(extendedData)));
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        sb.Append("$");

                        for (int i = 0; i < count; i++)
                        {
                            sb.Append(".");
                            sb.Append(propSplit[i]);

                            var selectedToken = objectToReturn.SelectToken(sb.ToString());
                            if (selectedToken == null && i < count - 1)
                            {
                                patcher.Patch(ref objectToReturn,
                                    new PatchDocument(new AddOperation()
                                    {
                                        Path = new JsonPointer(sb.ToString().Replace('.', '/').TrimStart('$')),
                                        Value = new JObject()
                                    }));
                            }
                            else if (selectedToken == null && i == count - 1)
                            {
                                patcher.Patch(ref objectToReturn,
                                    new PatchDocument(CreateAddOperation(extendedData)));
                            }
                        }
                    }
                }

                return objectToReturn;
            }
            catch (Exception e)
            {
                if (logger == null) return null;
                logger.Error(e, "Extensiblity failed to apply with this error.");
                throw;
            }
        }

        private static AddOperation CreateAddOperation(EthosExtensibleDataRow extendeDataRow)
        {
            var addOp =  new AddOperation {Path = new JsonPointer(extendeDataRow.FullJsonPath)};

            switch (extendeDataRow.JsonPropertyType)
            {
                case JsonPropertyTypeExtensions.String:
                    addOp.Value = new JValue(extendeDataRow.ExtendedDataValue);
                    break;
                case JsonPropertyTypeExtensions.Number:
                    bool decimalNumber = extendeDataRow.ExtendedDataValue.Contains(".");
                    int parsedValueInt = 0;
                    decimal parsedValueDecimal = 0.0m;

                    bool parseStatus = decimalNumber ? decimal.TryParse(extendeDataRow.ExtendedDataValue, out parsedValueDecimal) : int.TryParse(extendeDataRow.ExtendedDataValue, out parsedValueInt);

                    if (parseStatus && !decimalNumber)
                    {
                        addOp.Value = new JValue(parsedValueInt);
                    }
                    else if (parseStatus && decimalNumber)
                    {
                        addOp.Value = new JValue(parsedValueDecimal);
                    }
                    else
                    {
                        addOp.Value = new JValue(extendeDataRow.ExtendedDataValue);
                    }
                    break;
                case JsonPropertyTypeExtensions.DateTime:
                    addOp.Value = new JValue(extendeDataRow.ExtendedDataValue);
                    break;
                case JsonPropertyTypeExtensions.Date:
                    addOp.Value = new JValue(extendeDataRow.ExtendedDataValue);
                    break;
                case JsonPropertyTypeExtensions.Time:
                    addOp.Value = new JValue(extendeDataRow.ExtendedDataValue);
                    break;
                default:
                    addOp.Value = new JValue(extendeDataRow.ExtendedDataValue);
                    break;
            }

            return addOp;
        }

        /// <summary>
        /// Extract the extended data out of the json payload if present and return in a dictionary, column name + value
        /// DateTimes will be converted to localtime, the rest of the data is sent through as is
        /// </summary>
        /// <param name="extendedObject">json object that could have extended data in it</param>
        /// <param name="extensibleDataDefinitions">extended data settings for this schema</param>
        /// <param name="logger">logger</param>
        /// <returns>dictionary of the extended data, keyed by column name and value is the data present</returns>
        public static Dictionary<string, string> ExtractExtendedEthosData(JToken extendedObject, EthosExtensibleData extensibleDataDefinitions, ILogger logger)
        {
            var retDictionary = new Dictionary<string, string>();
            bool dataAdded = false;
            
            //add the API version number being used for the colleague subroutine to use
            retDictionary.Add("EDME.VERSION.NUMBER", extensibleDataDefinitions.ApiVersionNumber);

            foreach (var ethosExtensibleConfig in extensibleDataDefinitions.ExtendedDataList)
            {
                if (IsMetadataOperDateTime(ethosExtensibleConfig.ColleagueColumnName))
                {
                    continue;
                }

                //build jsonpath string to search with, don't need to worry about arrays right now so just simple dot notation
                var jsonPathForProperty = string.Concat("$", ethosExtensibleConfig.FullJsonPath.Replace('/', '.'));

                var jsonSelectObject = extendedObject.SelectToken(jsonPathForProperty, false);

                if (jsonSelectObject == null)
                {
                    continue;
                }

                if (ethosExtensibleConfig.JsonPropertyType == JsonPropertyTypeExtensions.DateTime)
                {
                    try
                    {
                        //have to convert the date from UTC that it is in to the ColleagueTimeZone value
                        DateTime utcDateTime = jsonSelectObject.Value<DateTime>();
                        //convert from utc to colleaguetimezone
                        var localDateTime = TimeZoneInfo.ConvertTime(utcDateTime, TimeZoneInfo.Utc, extensibleDataDefinitions.ColleagueTimeZone);
                        retDictionary.Add(ethosExtensibleConfig.ColleagueColumnName, localDateTime.ToString(CultureInfo.InvariantCulture));
                        dataAdded = true;
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, string.Concat("DateTime property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath));
                        throw new FormatException(string.Concat("DateTime property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath), e);
                    }
                }
                else if (ethosExtensibleConfig.JsonPropertyType == JsonPropertyTypeExtensions.Date)
                {
                    try
                    {
                        //get value out as a datetime
                        DateTime justDate = jsonSelectObject.Value<DateTime>();
                        //take just the date and tostring with invariant culture
                        retDictionary.Add(ethosExtensibleConfig.ColleagueColumnName, justDate.ToString("MM/dd/yyyy"));
                        dataAdded = true;
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, string.Concat("Date property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath));
                        throw new FormatException(string.Concat("Date property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath), e);
                    }
                }
                else
                {
                    retDictionary.Add(ethosExtensibleConfig.ColleagueColumnName, jsonSelectObject.Value<string>());
                    dataAdded = true;
                }
            }
            //if nothing was found/added to the dictionary just return an empty list
            return dataAdded ? retDictionary : new Dictionary<string, string>();
        }

        private static bool IsMetadataOperDateTime(string ethosColumnName)
        {
            if (ethosColumnName.Contains('.'))
            {
                var columnName = ethosColumnName.Split('.');
                var last = columnName.Count() - 1;
                var prev = columnName.Count() - 2;

                switch (columnName[last])
                {
                    case "ADDOPR":
                        return true;
                    case "ADDDATE":
                        return true;
                    case ("ADDTIME"):
                        return true;
                    case "CHGOPR":
                        return true;
                    case "CHGDATE":
                        return true;
                    case "CHGTIME":
                        return true;
                }
                switch (columnName[prev] + "." + columnName[last])
                {
                    case "ADD.OPERATOR":
                        return true;
                    case "ADD.DATE":
                        return true;
                    case "CHANGE.OPERATOR":
                        return true;
                    case "CHANGE.DATE":
                        return true;
                }
            }
            return false;
        }
    }
}
