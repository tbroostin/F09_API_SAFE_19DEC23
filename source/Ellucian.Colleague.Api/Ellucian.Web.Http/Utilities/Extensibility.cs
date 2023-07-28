// Copyright 2017 - 2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
                        var id = (string)j.SelectToken("$._id", false);
                        if (string.IsNullOrEmpty(id)) id = (string)j.SelectToken("$.id", false);
                        if (string.IsNullOrEmpty(id)) id = (string)j.SelectToken("$.Id", false);
                        if (string.IsNullOrEmpty(id)) id = (string)j.SelectToken("$.Code", false);
                        if (string.IsNullOrEmpty(id)) id = (string)j.SelectToken("$.code", false);
                        if (string.IsNullOrEmpty(id))
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
                        if (extendedObject == null)
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

        public static JToken ExtendSingleJsonObject(JToken jsonObjectToExtend, EthosExtensibleData extendConfig, ILogger logger)
        {
            //check extended data sent in, make sure it isn't null, row collection isn't null and it has something in it
            if (extendConfig == null || extendConfig.ExtendedDataList == null || !extendConfig.ExtendedDataList.Any())
            {
                return null;
            }

            try
            {
                var objectToReturn = jsonObjectToExtend;
                var patchDocument = new JsonPatchDocument();

                // A UI form may contain multiple key parts and each key part needs to be identified within
                // the "id" object.  If only one record key part is defined, then move the value from the "_id"
                // property to the "id" property and ignore the original value in the "_id" property.
                if (extendConfig.ExtendedDataList.Count(edl => edl.FullJsonPath.StartsWith("/id/")) > 0 || extendConfig.ExtendedDataList.Count(edl => edl.FullJsonPath.Equals("/id", StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    var selectedToken = objectToReturn.SelectToken("$._id");
                    if (selectedToken != null)
                    {
                        var path = "/_id";
                        patchDocument.Remove(path);
                        patchDocument.ApplyTo(objectToReturn);
                    }
                }
                else
                {
                    // Move "_id" property to "id" property.
                    var selectedToken = objectToReturn.SelectToken("$._id");
                    if (selectedToken != null)
                    {
                        patchDocument.Add("/id", new JValue(selectedToken.ToString()));
                        patchDocument.Remove("/_id");
                        patchDocument.ApplyTo(objectToReturn);
                    }
                }

                patchDocument = new JsonPatchDocument();

                // Extract all Array Objects with full path to array values as the key.
                var arrayObjects = new Dictionary<string, List<EthosExtensibleDataRow>>();
                var extendedDataListSorted = extendConfig.ExtendedDataList.OrderBy(ex => ex.JsonPath);
                foreach (var extendedData in extendedDataListSorted)
                {
                    bool isArrayObject = false;
                    if (extendedData.JsonPath.Contains("[]")) isArrayObject = true;
                    if (isArrayObject)
                    {
                        var pathSplit = extendedData.JsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        var pathCount = pathSplit.Count();
                        var path = new StringBuilder();

                        path.Append("/");

                        for (int i = 0; i < pathCount; i++)
                        {
                            path.Append(pathSplit[i]);
                            path.Append("/");
                            var pathKey = path.ToString();
                            if (pathKey.EndsWith("[]/"))
                            {
                                if (arrayObjects.ContainsKey(pathKey))
                                {
                                    arrayObjects[pathKey].Add(extendedData);
                                }
                                else
                                {
                                    arrayObjects.Add(pathKey, new List<EthosExtensibleDataRow>() { extendedData });
                                }
                                i = pathCount;  // Stop processing the array path
                            }
                        }
                    }
                }

                var extendedDataList = extendConfig.ExtendedDataList; //.OrderBy(ex => ex.FullJsonPath);
                foreach (var extendedData in extendedDataList)
                {
                    try
                    {
                        bool isArrayObject = false;
                        if (extendedData.JsonPath.Contains("[]")) isArrayObject = true;

                        if (!isArrayObject)
                        {
                            var propSplit =
                            extendedData.FullJsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                            if (!propSplit.Any()) continue;

                            var count = propSplit.Count();

                            if (count == 1)
                            {
                                var convertedValues = CreateAddOperation(extendedData);
                                patchDocument.Add(convertedValues.Item1, convertedValues.Item2);
                            }
                            else
                            {
                                var sb = new StringBuilder();

                                sb.Append("$");

                                for (int i = 0; i < count; i++)
                                {
                                    sb.Append(".");
                                    sb.Append(propSplit[i]);
                                    var selectedToken = objectToReturn.SelectToken(sb.ToString().TrimStart('$').TrimEnd(']').TrimEnd('['));
                                    if (selectedToken == null && i < count - 1)
                                    {
                                        var path = sb.ToString().Replace('.', '/').TrimStart('$').TrimEnd(']').TrimEnd('[');
                                        patchDocument.Add(path, new JObject());
                                    }
                                    else if (selectedToken == null && i == count - 1)
                                    {
                                        try
                                        {
                                            var convertedValues = CreateAddOperation(extendedData);
                                            patchDocument.Add(convertedValues.Item1, convertedValues.Item2);
                                        }
                                        catch
                                        {
                                            var temp = extendedData;
                                        }
                                    }
                                }
                            }
                        }

                        patchDocument.ApplyTo(objectToReturn);
                        patchDocument = new JsonPatchDocument();
                    }
                    catch (Exception e)
                    {
                        if (logger == null) continue;
                        logger.Error(e, string.Format("Extensiblity failed to apply with this error. {0}", extendedData.ToString()));
                        continue;
                    }

                    // Now process all array objects
                    objectToReturn = ProcessArrayObjects(objectToReturn, arrayObjects, patchDocument, logger);
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

        private static JToken ProcessArrayObjects(JToken objectToReturn, Dictionary<string, List<EthosExtensibleDataRow>> ethosExtendedDataDictionary, JsonPatchDocument patchDocument, ILogger logger)
        {
            char _XM = Convert.ToChar(250);

            // Process Array properties
            if (ethosExtendedDataDictionary != null && ethosExtendedDataDictionary.Any())
            {
                foreach (var ethosExtendedDictItem in ethosExtendedDataDictionary)
                {
                    try
                    {
                        var arrayValues = new JArray();
                        var arrayPath = ethosExtendedDictItem.Key;
                        var ethosObjects = ethosExtendedDictItem.Value;
                        int totalCount = 0;
                        foreach (var eo in ethosObjects)
                        {
                            if (eo.ExtendedDataValue.Split(DmiString._VM).Count() > totalCount)
                                totalCount = eo.ExtendedDataValue.Split(DmiString._VM).Count();
                        }

                        for (int idx = 0; idx < totalCount; idx++)
                        {
                            var objectValues = new JObject();
                            var objectPatchDocument = new JsonPatchDocument();
                            var arrayObjects = new Dictionary<string, List<EthosExtensibleDataRow>>();
                            foreach (var extendedData2 in ethosObjects)
                            {
                                try
                                {
                                    var dataValues = extendedData2.ExtendedDataValue.Split(DmiString._VM);
                                    var dataType = extendedData2.JsonPropertyType;
                                    var propertyName = extendedData2.JsonTitle;
                                    if (dataValues.Count() > idx)
                                    {
                                        if (extendedData2.JsonPath == arrayPath)
                                        {
                                            if (dataValues.Count() > idx && !string.IsNullOrEmpty(dataValues[idx]))
                                            {
                                                var propertyValue = new JValue(ConvertValueToJValue(dataType, dataValues[idx]));
                                                if (propertyName.EndsWith("[]"))
                                                {
                                                    var jArray = new JArray();
                                                    var subValues = dataValues[idx].Split(DmiString._SM);
                                                    foreach (var subval in subValues)
                                                    {
                                                        jArray.Add(ConvertValueToJValue(dataType, subval));
                                                    }
                                                    objectValues.Add(new JProperty(propertyName.TrimEnd(']').TrimEnd('['), jArray));
                                                }
                                                else
                                                {
                                                    objectValues.Add(new JProperty(propertyName.TrimEnd(']').TrimEnd('['), propertyValue));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var nestedObject = new Dictionary<string, bool>();

                                            var pathSplit =
                                                extendedData2.FullJsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                                            if (!pathSplit.Any()) continue;

                                            var pathCount = pathSplit.Count();
                                            var path = new StringBuilder();

                                            path.Append("/");

                                            bool isArray = false;
                                            string pathKey = string.Empty;
                                            for (int i = 0; i < pathCount; i++)
                                            {
                                                path.Append(pathSplit[i]);
                                                path.Append("/");
                                                if (path.ToString().StartsWith(arrayPath) && path.ToString() != arrayPath)
                                                {
                                                    if (!isArray)
                                                    {
                                                        isArray = pathSplit[i].EndsWith(("[]"));
                                                        if (isArray)
                                                        {
                                                            pathKey = string.Concat("/", pathSplit[i], "/");
                                                        }
                                                    }
                                                    if (!isArray)
                                                    {
                                                        if (i < pathCount - 1)
                                                        {
                                                            nestedObject.Add(pathSplit[i].TrimEnd(']').TrimEnd('['), isArray);
                                                        }
                                                        else if (i == pathCount - 1)
                                                        {
                                                            if (nestedObject.ContainsKey(pathSplit[i]))
                                                            {
                                                                nestedObject.Add(string.Concat(pathSplit[i].TrimEnd(']').TrimEnd('['), ".", pathSplit[i].TrimEnd(']').TrimEnd('[')), isArray);
                                                            }
                                                            else
                                                            {
                                                                nestedObject.Add(pathSplit[i].TrimEnd(']').TrimEnd('['), isArray);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (dataValues.Count() > idx && !string.IsNullOrEmpty(dataValues[idx]))
                                                        {
                                                            if (string.IsNullOrEmpty(pathKey))
                                                            {
                                                                pathKey = string.Concat("/", pathSplit[i], "/");
                                                            }
                                                            var partialPath = "";
                                                            if (pathCount > i - 1)
                                                            {
                                                                for (int bp = i; bp < pathCount - 1; bp++)
                                                                {
                                                                    partialPath = string.Concat(partialPath, "/", pathSplit[bp]);
                                                                }
                                                                partialPath = string.Concat(partialPath, "/");
                                                            }
                                                            var partialFullPath = string.Concat(partialPath, propertyName, "/");
                                                            EthosExtensibleDataRow extendedDataObject = new EthosExtensibleDataRow()
                                                            {
                                                                ColleagueColumnName = extendedData2.ColleagueColumnName,
                                                                JsonTitle = propertyName,
                                                                JsonPath = partialPath,
                                                                FullJsonPath = partialFullPath,
                                                                ExtendedDataValue = extendedData2.ExtendedDataValue,
                                                                JsonPropertyType = extendedData2.JsonPropertyType
                                                            };
                                                            var tempDataValues = dataValues[idx];
                                                            if (tempDataValues.Contains(DmiString._SM))
                                                            {
                                                                tempDataValues = tempDataValues.Replace(DmiString._SM, DmiString._VM);
                                                            }
                                                            else if (tempDataValues.Contains(DmiString._TM))
                                                            {
                                                                tempDataValues = tempDataValues.Replace(DmiString._TM, DmiString._VM);
                                                            }
                                                            else if (tempDataValues.Contains(_XM))
                                                            {
                                                                tempDataValues = tempDataValues.Replace(_XM, DmiString._VM);
                                                            }

                                                            if (!string.IsNullOrEmpty(tempDataValues))
                                                            {
                                                                extendedDataObject.ExtendedDataValue = tempDataValues;
                                                                if (arrayObjects.ContainsKey(pathKey))
                                                                {
                                                                    arrayObjects[pathKey].Add(extendedDataObject);
                                                                }
                                                                else
                                                                {
                                                                    arrayObjects.Add(pathKey, new List<EthosExtensibleDataRow>() { extendedDataObject });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (nestedObject != null && nestedObject.Any())
                                            {
                                                if (dataValues.Count() > idx && !string.IsNullOrEmpty(dataValues[idx]))
                                                {
                                                    var nestedCount = nestedObject.Count();
                                                    var sb = new StringBuilder();

                                                    sb.Append("$");

                                                    for (int i = 0; i < nestedCount; i++)
                                                    {
                                                        sb.Append(".");
                                                        if (nestedObject.ElementAt(i).Key.Contains("."))
                                                        {
                                                            sb.Append(nestedObject.ElementAt(i).Key.Split('.')[0]);
                                                        }
                                                        else
                                                        {
                                                            sb.Append(nestedObject.ElementAt(i).Key);
                                                        }
                                                        var fullPath = sb.ToString().Replace('.', '/').TrimStart('$').TrimEnd(']').TrimEnd('[');
                                                        var selectedToken = objectValues.SelectToken(sb.ToString().TrimStart('$').TrimEnd(']').TrimEnd('['));
                                                        if (selectedToken == null && i < nestedCount - 1)
                                                        {
                                                            if (propertyName.EndsWith("[]"))
                                                            {
                                                                var jArray = new JArray();
                                                                var subValues = dataValues[idx].Split(DmiString._SM);
                                                                foreach (var subval in subValues)
                                                                {
                                                                    if (!string.IsNullOrEmpty(subval))
                                                                    {
                                                                        jArray.Add(ConvertValueToJValue(dataType, subval));
                                                                    }
                                                                }
                                                                selectedToken = objectValues.SelectToken("$");
                                                                objectPatchDocument.Add(fullPath, jArray);
                                                            }
                                                            else
                                                            {
                                                                selectedToken = objectValues.SelectToken("$");
                                                                objectPatchDocument.Add(fullPath, new JObject());
                                                            }
                                                        }
                                                        else if (selectedToken == null && i == nestedCount - 1)
                                                        {
                                                            if (propertyName.EndsWith("[]"))
                                                            {
                                                                var jArray = new JArray();
                                                                var subValues = dataValues[idx].Split(DmiString._SM);
                                                                foreach (var subval in subValues)
                                                                {
                                                                    if (!string.IsNullOrEmpty(subval))
                                                                    {
                                                                        jArray.Add(ConvertValueToJValue(dataType, subval));
                                                                    }
                                                                }
                                                                selectedToken = objectValues.SelectToken("$");
                                                                objectPatchDocument.Add(fullPath, jArray);
                                                            }
                                                            else
                                                            {
                                                                var propertyValue = new JValue(ConvertValueToJValue(dataType, dataValues[idx]));
                                                                selectedToken = objectValues.SelectToken("$");
                                                                objectPatchDocument.Add(fullPath, propertyValue);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    objectPatchDocument.ApplyTo(objectValues);
                                }
                                catch (Exception e)
                                {
                                    if (logger == null) continue;
                                    logger.Error(e, string.Format("Extensiblity failed to apply with this error. {0}", ethosExtendedDictItem.ToString()));
                                    continue;
                                }
                            }
                            // Look for array objects within the array
                            if (arrayObjects != null && arrayObjects.Any())
                            {
                                objectPatchDocument = new JsonPatchDocument();
                                // Now process all array objects
                                var tempObjectToReturn = new JObject();
                                var returnToken = ProcessArrayObjects(tempObjectToReturn, arrayObjects, objectPatchDocument, logger);
                                foreach (var arrayObj in arrayObjects)
                                {
                                    var tempExtendedData = ethosExtendedDataDictionary.SelectMany(eed => eed.Value.Where(eedv => eedv.JsonPath.EndsWith(arrayObj.Key)));
                                    var partialPath = tempExtendedData.FirstOrDefault().JsonPath.Substring(arrayPath.Length - 1);
                                    //partialPath = partialPath.Substring(0, partialPath.Length - arrayObj.Key.Length);
                                    var path = string.Concat(partialPath.TrimEnd('/').TrimEnd(']').TrimEnd('['));
                                    var tokenValues = returnToken.Values().FirstOrDefault();
                                    var selectedToken = objectValues.SelectToken("$");

                                    objectPatchDocument = new JsonPatchDocument();
                                    objectPatchDocument.Add(path, tokenValues);
                                    objectPatchDocument.ApplyTo(objectValues);
                                }
                            }
                            if (objectValues != null && objectValues.HasValues)
                                arrayValues.Add(objectValues);
                        }

                        if (arrayValues != null && arrayValues.Any())
                        {
                            var propSplit =
                                arrayPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                            if (!propSplit.Any()) continue;

                            var count = propSplit.Count();
                            var sb = new StringBuilder();

                            sb.Append("$");

                            for (int i = 0; i < count; i++)
                            {
                                sb.Append(".");
                                sb.Append(propSplit[i]);

                                var path = sb.ToString().Replace('.', '/').TrimStart('$');
                                var selectedToken = objectToReturn.SelectToken(sb.ToString().TrimEnd(']').TrimEnd('['));
                                if (selectedToken == null && i < count - 1)
                                {
                                    patchDocument.Add(path.TrimEnd(']').TrimEnd('['), new JObject());
                                }
                                else if (selectedToken == null && i == count - 1)
                                {
                                    patchDocument.Add(path.TrimEnd(']').TrimEnd('['), arrayValues);
                                }
                                else if (selectedToken != null && i == count - 1)
                                {
                                    selectedToken.Parent.Remove();
                                    patchDocument.Add(path.TrimEnd(']').TrimEnd('['), arrayValues);

                                }
                                else if (selectedToken == null && i < count - 1)
                                {
                                    selectedToken.Parent.Remove();
                                    patchDocument.Add(path.TrimEnd(']').TrimEnd('['), new JObject());
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (logger == null) continue;
                        logger.Error(e, string.Format("Extensiblity failed to apply with this error. {0}", ethosExtendedDictItem.ToString()));
                        continue;
                    }
                }
            }

            patchDocument.ApplyTo(objectToReturn);

            return objectToReturn;
        }

        private static Tuple<string, JToken> CreateAddOperation(EthosExtensibleDataRow extendedDataRow)
        {
            var path = extendedDataRow.FullJsonPath.TrimEnd(']').TrimEnd('[');
            JToken value = "";

            if (extendedDataRow.JsonTitle.EndsWith("[]"))
            {
                var extendedDataArray = extendedDataRow.ExtendedDataValue.Replace(DmiString._SM, DmiString._VM).Split(DmiString._VM);
                var arrayValues = new JArray();
                foreach (var dataValue in extendedDataArray)
                {
                    arrayValues.Add(ConvertValueToJValue(extendedDataRow.JsonPropertyType, dataValue));
                }
                value = arrayValues;
            }
            else
            {
                value = ConvertValueToJValue(extendedDataRow.JsonPropertyType, extendedDataRow.ExtendedDataValue);
            }

            return new Tuple<string, JToken>(path, value);
        }

        private static JValue ConvertValueToJValue(JsonPropertyTypeExtensions jsonPropertyType, string extendedDataValue)
        {
            var jValue = new JValue(extendedDataValue);
            switch (jsonPropertyType)
            {
                case JsonPropertyTypeExtensions.String:
                    break;
                case JsonPropertyTypeExtensions.Number:
                    bool decimalNumber = extendedDataValue.Contains(".");
                    int parsedValueInt = 0;
                    decimal parsedValueDecimal = 0.0m;

                    bool parseStatus = decimalNumber ? decimal.TryParse(extendedDataValue, out parsedValueDecimal) : int.TryParse(extendedDataValue, out parsedValueInt);

                    if (parseStatus && !decimalNumber)
                    {
                        jValue = new JValue(parsedValueInt);
                    }
                    else if (parseStatus && decimalNumber)
                    {
                        jValue = new JValue(parsedValueDecimal);
                    }
                    else
                    {
                        jValue = new JValue(extendedDataValue);
                    }
                    break;
                case JsonPropertyTypeExtensions.DateTime:
                    break;
                case JsonPropertyTypeExtensions.Date:
                    break;
                case JsonPropertyTypeExtensions.Time:
                    break;
                default:
                    break;
            }
            return jValue;
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
            char _XM = Convert.ToChar(250);

            var retDictionary = new Dictionary<string, string>();
            bool dataAdded = false;

            //add the API version number being used for the colleague subroutine to use
            retDictionary.Add("EDME.VERSION.NUMBER", extensibleDataDefinitions.ApiVersionNumber);

            var currentUserIdPath = extensibleDataDefinitions.CurrentUserIdPath;
            bool currentUserIdPathFound = false;
            var currentUserId = extensibleDataDefinitions.CurrentUserId;

            foreach (var ethosExtensibleConfig in extensibleDataDefinitions.ExtendedDataList)
            {
                if (IsMetadataOperDateTime(ethosExtensibleConfig.ColleagueColumnName))
                {
                    continue;
                }

                //build jsonpath string to search with, don't need to worry about arrays right now so just simple dot notation
                var jsonPathForProperty = string.Concat("$", ethosExtensibleConfig.FullJsonPath.Replace('/', '.'));

                // Since the "id" property can be either a property or an object containing multiple key values, then
                // we need to modify the path if necessary to point to the property within the "id" object.
                if (!string.IsNullOrEmpty(extensibleDataDefinitions.ApiType) && extensibleDataDefinitions.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase) && extensibleDataDefinitions.ColleagueKeyNames != null && extensibleDataDefinitions.ColleagueKeyNames.Count() > 1)
                {
                    if (extensibleDataDefinitions.ColleagueKeyNames.Contains(ethosExtensibleConfig.ColleagueColumnName))
                    {
                        if (ethosExtensibleConfig.JsonPath == "/" || string.IsNullOrEmpty(ethosExtensibleConfig.JsonPath))
                        {
                            jsonPathForProperty = string.Concat("$.id", ethosExtensibleConfig.FullJsonPath.Replace('/', '.'));
                        }
                    }
                }

                JToken jsonSelectObject = null;

                try
                {
                    if (jsonPathForProperty.Contains("[]"))
                    {
                        bool jsonPropertyFound = false;
                        var delim = DmiString._SM;
                        // if (jsonPathForProperty.Split('[').Count() == 2)
                        // {
                        //    delim = _TM;
                        //}
                        //else if (jsonPathForProperty.Split('[').Count() >= 3)
                        //{
                        //    delim = _XM;
                        //}

                        var rootFromJsonProperty = jsonPathForProperty.Substring(0, jsonPathForProperty.IndexOf("[]"));
                        var selectedToken = extendedObject.SelectToken(rootFromJsonProperty);
                        if (selectedToken != null)
                        {
                            var selectedJarray = (JArray)selectedToken;

                            var originalPathWithoutRoot = ethosExtensibleConfig.FullJsonPath.Substring(0, jsonPathForProperty.IndexOf("[]") + 1);
                            var jsonPath = ethosExtensibleConfig.FullJsonPath.Replace(originalPathWithoutRoot, "");

                            string values = string.Empty;
                            foreach (var token in selectedJarray.Children())
                            {
                                var arrayValues = GetArrayValues(jsonPath, token);
                                if (arrayValues == null || !arrayValues.Any())
                                {
                                    values = string.Concat(values, delim);
                                    continue;
                                }
                                jsonPropertyFound = true;

                                foreach (var value in arrayValues)
                                {
                                    if (ethosExtensibleConfig.JsonPropertyType == JsonPropertyTypeExtensions.DateTime)
                                    {
                                        try
                                        {
                                            if (!string.IsNullOrEmpty(value.Value<string>()))
                                            {
                                                //have to convert the date from UTC that it is in to the ColleagueTimeZone value
                                                DateTime utcDateTime = value.Value<DateTime>();
                                                //convert from utc to colleaguetimezone
                                                var localDateTime = TimeZoneInfo.ConvertTime(utcDateTime, TimeZoneInfo.Utc, extensibleDataDefinitions.ColleagueTimeZone);
                                                values = string.Concat(values, localDateTime.ToString(CultureInfo.InvariantCulture), _XM);
                                            }
                                            else
                                            {
                                                // Allow for removal of a Time property
                                                values = string.Concat(values, string.Empty, _XM);
                                            }
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
                                            if (!string.IsNullOrEmpty(value.Value<string>()))
                                            {
                                                //get value out as a datetime
                                                DateTime justDate = value.Value<DateTime>();
                                                //take just the date and tostring with invariant culture
                                                values = string.Concat(values, justDate.ToString("MM/dd/yyyy"), _XM);
                                            }
                                            else
                                            {
                                                // Allow for removal of a Date property
                                                values = string.Concat(values, string.Empty, _XM);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Error(e, string.Concat("Date property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath));
                                            throw new FormatException(string.Concat("Date property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath), e);
                                        }
                                    }
                                    else if (ethosExtensibleConfig.JsonPropertyType == JsonPropertyTypeExtensions.Number)
                                    {
                                        try
                                        {
                                            if (!string.IsNullOrEmpty(value.Value<string>()))
                                            {
                                                //get value out as a datetime
                                                decimal justNumber = value.Value<decimal>();
                                                //take the number as decimal and store in values
                                                values = string.Concat(values, justNumber, _XM);
                                            }
                                            else
                                            {
                                                // Allow for removal of a Date property
                                                values = string.Concat(values, string.Empty, _XM);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Error(e, string.Concat("Number property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath));
                                            throw new FormatException(string.Concat("Number property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath), e);
                                        }
                                    }
                                    else
                                    {
                                        //values = string.Concat(values, value.Value<string>().Replace(_SM, _TM).Replace(_VM, _SM), _XM);
                                        values = string.Concat(values, value.Value<string>().Replace(DmiString._VM, DmiString._SM), DmiString._TM);
                                    }
                                }

                                // if (values.EndsWith(_XM.ToString()))
                                if (values.EndsWith(DmiString.sTM) || values.EndsWith(_XM.ToString()))
                                {
                                    values = values.Remove(values.Length - 1);
                                }
                                values = string.Concat(values, delim);
                            }
                            if (!jsonPropertyFound)
                            {
                                continue;
                            }

                            //if (!string.IsNullOrEmpty(values.TrimEnd(delim)))
                            if (values.EndsWith(delim.ToString()) || string.IsNullOrEmpty(values))
                            {
                                retDictionary.Add(ethosExtensibleConfig.ColleagueColumnName, string.IsNullOrEmpty(values) ? "" : values.Remove(values.Length - 1));
                                dataAdded = true;
                            }
                            if (!string.IsNullOrEmpty(currentUserIdPath) && ethosExtensibleConfig.FullJsonPath.Equals(currentUserIdPath, StringComparison.OrdinalIgnoreCase) && dataAdded)
                            {
                                if (!values.Contains(currentUserId))
                                {
                                    throw new PermissionsException("User is not authorized to update this content.");
                                }
                                currentUserIdPathFound = true;
                            }
                            continue;
                        }
                    }
                    else
                    {
                        jsonSelectObject = extendedObject.SelectToken(jsonPathForProperty, false);
                        if (jsonSelectObject == null && extensibleDataDefinitions.ColleagueKeyNames != null && extensibleDataDefinitions.ColleagueKeyNames.Count() > 1)
                        {
                            // If we couldn't find the property, then see if it's part of the "id" object
                            var tempJsonPathForProperty = string.Concat("$.id", ethosExtensibleConfig.FullJsonPath.Replace('/', '.'));
                            jsonSelectObject = extendedObject.SelectToken(jsonPathForProperty, false);
                            if (jsonSelectObject != null)
                            {
                                jsonPathForProperty = tempJsonPathForProperty;
                            }
                        }
                    }
                }
                catch (FormatException ex)
                {
                    throw ex;
                }
                catch (Exception)
                {
                    continue;
                }

                if (jsonSelectObject == null)
                {
                    // Check to see if we've set a null object.  In such cases, all items within the object
                    // should be included with an empty string for the value.
                    var splitPath = jsonPathForProperty.Split('.');
                    if (splitPath.Count() > 2)
                    {
                        var testPath = jsonPathForProperty;
                        for (int i = 2; i < splitPath.Count(); i++)
                        {
                            testPath = testPath.Substring(0, testPath.LastIndexOf('.'));
                            try
                            {
                                jsonSelectObject = extendedObject.SelectToken(testPath, false);
                                if (jsonSelectObject != null && !retDictionary.ContainsKey(ethosExtensibleConfig.ColleagueColumnName))
                                {
                                    retDictionary.Add(ethosExtensibleConfig.ColleagueColumnName, "");
                                    dataAdded = true;
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                    continue;
                }

                // Make sure we don't already have this column in the dictionary
                if (retDictionary.ContainsKey(ethosExtensibleConfig.ColleagueColumnName))
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
                if (!string.IsNullOrEmpty(currentUserIdPath) && ethosExtensibleConfig.FullJsonPath.Equals(currentUserIdPath, StringComparison.OrdinalIgnoreCase) && dataAdded)
                {
                    if (!jsonSelectObject.Value<string>().Equals(currentUserId, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new PermissionsException("User is not authorized to update this content.");
                    }
                    currentUserIdPathFound = true;
                }
            }
            // If we have record security setup for the current user only, then we need to
            // abort with permissions error if they haven't included the path in the body
            // of the request.
            if (!string.IsNullOrEmpty(currentUserIdPath) && !currentUserIdPathFound)
            {
                throw new PermissionsException("User is not authorized to update this content.");
            }
            //if nothing was found/added to the dictionary just return an empty list
            //return dataAdded ? retDictionary : new Dictionary<string, string>();
            return retDictionary;
        }

        private static List<JToken> GetArrayValues(string fullPath, JToken extendedObject)
        {
            List<JToken> returnValues = new List<JToken>();

            var pathSplit =
                fullPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (extendedObject.Type == JTokenType.String && !string.IsNullOrEmpty(extendedObject.ToString()))
            {
                returnValues.Add(extendedObject.Value<string>());
                return returnValues;
            }

            if (!pathSplit.Any()) return returnValues;

            var pathCount = pathSplit.Count();
            var sb = new StringBuilder();

            sb.Append("$");

            for (int i = 0; i < pathCount; i++)
            {
                sb.Append(".");
                // sb.Append(pathSplit[i])
                sb.Append(pathSplit[i].TrimEnd(']').TrimEnd('['));

                // var selectedToken = extendedObject.SelectToken(sb.ToString().TrimEnd(']').TrimEnd('['));
                var selectedToken = extendedObject.SelectToken(sb.ToString());

                if (selectedToken != null && i < pathCount - 1)
                {
                    if (selectedToken.Type == JTokenType.Array)
                    {
                        var path = new StringBuilder();

                        for (int idx = i + 1; idx < pathCount; idx++)
                        {
                            path.Append("/");
                            path.Append(pathSplit[idx]);
                        }
                        foreach (JToken selToken in selectedToken)
                        {
                            var resp = GetArrayValues(path.ToString(), selToken);
                            if (resp != null && resp.Any())
                            {
                                returnValues.AddRange(resp);
                            }
                        }
                        i = pathCount;
                    }
                }
                else if (selectedToken != null && i == pathCount - 1)
                {
                    if (selectedToken.Type == JTokenType.Array)
                    {
                        var path = new StringBuilder();

                        for (int idx = i + 1; idx < pathCount; idx++)
                        {
                            path.Append("/");
                            path.Append(pathSplit[idx]);
                        }
                        foreach (JToken selToken in selectedToken)
                        {
                            var resp = GetArrayValues(path.ToString(), selToken);
                            if (resp != null && resp.Any())
                            {
                                returnValues.AddRange(resp);
                            }
                        }
                        i = pathCount;
                    }
                    else
                    {
                        returnValues.Add(selectedToken.Value<string>());
                    }
                }
            }
            return returnValues;
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
