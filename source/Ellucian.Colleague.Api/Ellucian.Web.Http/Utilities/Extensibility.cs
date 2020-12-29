// Copyright 2017 - 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ellucian.Dmi.Runtime;
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

            char _VM = Convert.ToChar(DynamicArray.VM);
            char _SM = Convert.ToChar(DynamicArray.SM);

            try
            {
                var objectToReturn = jsonObjectToExtend;
                var patcher = new JsonPatcher();
                var arrayObjects = new Dictionary<string, List<EthosExtensibleDataRow>>();

                var extendedDataListSorted = extendConfig.ExtendedDataList.OrderBy(ex => ex.FullJsonPath);
                foreach (var extendedData in extendedDataListSorted)
                {
                    bool isArrayObject = false;
                    if (extendedData.JsonPath.Contains("[]")) isArrayObject = true;
                    if (isArrayObject)
                    { 
                        var pathKey = extendedData.JsonPath.Split('[')[0] + "[]/";
                        if (arrayObjects.ContainsKey(pathKey))
                        {
                            arrayObjects[pathKey].Add(extendedData);
                        }
                        else
                        {
                            arrayObjects.Add(pathKey, new List<EthosExtensibleDataRow>() { extendedData });
                        }
                    }

                    if (!isArrayObject)
                    {
                        var propSplit =
                        extendedData.FullJsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

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
                                var selectedToken = objectToReturn.SelectToken(sb.ToString().TrimStart('$').TrimEnd(']').TrimEnd('['));
                                if (selectedToken == null && i < count - 1)
                                {
                                    var path = sb.ToString().Replace('.', '/').TrimStart('$').TrimEnd(']').TrimEnd('[');
                                    patcher.Patch(ref objectToReturn,
                                        new PatchDocument(new AddOperation()
                                        {
                                            Path = new JsonPointer(path),
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
                }

                // Process Array properties
                if (arrayObjects != null && arrayObjects.Any())
                {
                    foreach (var arrayObject in arrayObjects)
                    {
                        try
                        {
                            var arrayValues = new JArray();
                            var arrayPath = arrayObject.Key;
                            var ethosObjects = arrayObject.Value;
                            int totalCount = 0;
                            foreach (var eo in ethosObjects)
                            {
                                if (eo.ExtendedDataValue.Split(_VM).Count() > totalCount)
                                    totalCount = eo.ExtendedDataValue.Split(_VM).Count();
                            }

                            for (int idx = 0; idx < totalCount; idx++)
                            {
                                var objectValues = new JObject();
                                foreach (var extendedData in ethosObjects)
                                {
                                    try
                                    { 
                                        var dataValues = extendedData.ExtendedDataValue.Split(_VM);
                                        var dataType = extendedData.JsonPropertyType;
                                        var propertyName = extendedData.JsonTitle;
                                        if (dataValues.Count() > idx)
                                        {
                                            if (extendedData.JsonPath == arrayPath)
                                            {
                                                if (dataValues.Count() > idx && !string.IsNullOrEmpty(dataValues[idx]))
                                                {
                                                    var propertyValue = new JValue(ConvertValueToJValue(dataType, dataValues[idx]));
                                                    if (propertyName.EndsWith("[]"))
                                                    {
                                                        var jArray = new JArray();
                                                        var subValues = dataValues[idx].Split(_SM);
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
                                                var pathSplit =
                                                    extendedData.FullJsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                                                if (!pathSplit.Any()) continue;

                                                var pathCount = pathSplit.Count();
                                                var path = new StringBuilder();

                                                path.Append("/");
                                                var nestedObject = new Dictionary<string, bool>();
                                                for (int i = 0; i < pathCount; i++)
                                                {
                                                    path.Append(pathSplit[i]);
                                                    path.Append("/");
                                                    if (path.ToString().StartsWith(arrayPath) && path.ToString() != arrayPath)
                                                    {
                                                        if (i < pathCount - 1)
                                                        {
                                                            bool isArray = pathSplit[i].Contains(("[]"));
                                                            nestedObject.Add(pathSplit[i].TrimEnd(']').TrimEnd('['), isArray);
                                                        }
                                                        else if (i == pathCount - 1)
                                                        {
                                                            bool isArray = pathSplit[i].Contains(("[]"));
                                                            nestedObject.Add(pathSplit[i].TrimEnd(']').TrimEnd('['), isArray);
                                                        }
                                                    }
                                                }
                                                if (nestedObject != null && nestedObject.Any())
                                                {
                                                    if (dataValues.Count() > idx && !string.IsNullOrEmpty(dataValues[idx]))
                                                    {
                                                        var propertyValue = new JValue(ConvertValueToJValue(dataType, dataValues[idx]));
                                                        var nestedCount = nestedObject.Count();
                                                        var sb = new StringBuilder();

                                                        sb.Append("$");

                                                        for (int i = 0; i < nestedCount; i++)
                                                        {
                                                            sb.Append(".");
                                                            sb.Append(nestedObject.ElementAt(i).Key);
                                                            var fullPath = sb.ToString().Replace('.', '/').TrimStart('$').TrimEnd(']').TrimEnd('[');
                                                            var selectedToken = objectValues.SelectToken(sb.ToString().TrimStart('$').TrimEnd(']').TrimEnd('['));
                                                            if (selectedToken == null && i < nestedCount - 1)
                                                            {
                                                                selectedToken = objectValues.SelectToken("$");
                                                                patcher.Patch(ref selectedToken,
                                                                    new PatchDocument(new AddOperation()
                                                                    {
                                                                        Path = new JsonPointer(fullPath),
                                                                        Value = new JObject()
                                                                    }));
                                                            }
                                                            else if (selectedToken == null && i == nestedCount - 1)
                                                            {
                                                                if (propertyName.EndsWith("[]"))
                                                                {
                                                                    var jArray = new JArray();
                                                                    var subValues = dataValues[idx].Split(_SM);
                                                                    foreach (var subval in subValues)
                                                                    {
                                                                        if (!string.IsNullOrEmpty(subval))
                                                                        {
                                                                            jArray.Add(ConvertValueToJValue(dataType, subval));
                                                                        }
                                                                    }
                                                                    selectedToken = objectValues.SelectToken("$");
                                                                    patcher.Patch(ref selectedToken,
                                                                        new PatchDocument(new AddOperation()
                                                                        {
                                                                            Path = new JsonPointer(fullPath),
                                                                            Value = jArray
                                                                        }));
                                                                }
                                                                else
                                                                {
                                                                    selectedToken = objectValues.SelectToken("$");
                                                                    patcher.Patch(ref selectedToken,
                                                                        new PatchDocument(new AddOperation()
                                                                        {
                                                                            Path = new JsonPointer(fullPath),
                                                                            Value = propertyValue
                                                                        }));
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        if (logger == null) continue;
                                        logger.Error(e, string.Format("Extensiblity failed to apply with this error. {0}", arrayObject.ToString()));
                                        continue;
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
                                        patcher.Patch(ref objectToReturn,
                                            new PatchDocument(new AddOperation()
                                            {
                                                Path = new JsonPointer(path.TrimEnd(']').TrimEnd('[')),
                                                Value = new JObject()
                                            }));
                                    }
                                    else if (selectedToken == null && i == count - 1)
                                    {
                                        patcher.Patch(ref objectToReturn,
                                            new PatchDocument(new AddOperation()
                                            {
                                                Path = new JsonPointer(path.TrimEnd(']').TrimEnd('[')),
                                                Value = arrayValues
                                            }));
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (logger == null) continue;
                            logger.Error(e, string.Format("Extensiblity failed to apply with this error. {0}", arrayObject.ToString()));
                            continue;
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

        private static AddOperation CreateAddOperation(EthosExtensibleDataRow extendedDataRow)
        {
            char _VM = Convert.ToChar(DynamicArray.VM);
            char _SM = Convert.ToChar(DynamicArray.SM);

            var addOp = new AddOperation { Path = new JsonPointer(extendedDataRow.FullJsonPath.TrimEnd(']').TrimEnd('[')) };

            if (extendedDataRow.JsonTitle.EndsWith("[]"))
            {
                var extendedDataArray = extendedDataRow.ExtendedDataValue.Replace(_SM, _VM).Split(_VM);
                var arrayValues = new JArray();
                foreach (var dataValue in extendedDataArray)
                {
                    arrayValues.Add(ConvertValueToJValue(extendedDataRow.JsonPropertyType, dataValue));
                }
                addOp.Value = arrayValues;
            }
            else
            {
                addOp.Value = ConvertValueToJValue(extendedDataRow.JsonPropertyType, extendedDataRow.ExtendedDataValue);
            }

            return addOp;
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
            char _VM = Convert.ToChar(DynamicArray.VM); //253
            char _SM = Convert.ToChar(DynamicArray.SM); //252
            char _TM = Convert.ToChar(DynamicArray.TM); //251
            char _XM = Convert.ToChar(250);

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
              
                JToken jsonSelectObject = null;

                try
                {
                    if (jsonPathForProperty.Contains("[]"))
                    {
                        var delim = _SM;
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

                                foreach (var value in arrayValues)
                                {
                                    if (ethosExtensibleConfig.JsonPropertyType == JsonPropertyTypeExtensions.DateTime)
                                    {
                                        try
                                        {
                                            //have to convert the date from UTC that it is in to the ColleagueTimeZone value
                                            DateTime utcDateTime = value.Value<DateTime>();
                                            //convert from utc to colleaguetimezone
                                            var localDateTime = TimeZoneInfo.ConvertTime(utcDateTime, TimeZoneInfo.Utc, extensibleDataDefinitions.ColleagueTimeZone);
                                            values = string.Concat(values, localDateTime.ToString(CultureInfo.InvariantCulture), _XM);
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
                                            DateTime justDate = value.Value<DateTime>();
                                            //take just the date and tostring with invariant culture
                                            values = string.Concat(values, justDate.ToString("MM/dd/yyyy"), _XM);
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Error(e, string.Concat("Date property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath));
                                            throw new FormatException(string.Concat("Date property was not in the correct format for extended property : ", ethosExtensibleConfig.FullJsonPath), e);
                                        }
                                    }
                                    else
                                    {
                                        values = string.Concat(values, value.Value<string>().Replace(_SM, _TM).Replace(_VM, _SM), _XM);
                                    }
                                }

                                if (values.EndsWith(_XM.ToString()))
                                {
                                    values = values.Remove(values.Length - 1);
                                }
                                values = string.Concat(values, delim);
                            }

                            //if (!string.IsNullOrEmpty(values.TrimEnd(delim)))
                            if (values.EndsWith(delim.ToString()) || string.IsNullOrEmpty(values))
                            {
                                retDictionary.Add(ethosExtensibleConfig.ColleagueColumnName, string.IsNullOrEmpty(values) ? "": values.Remove(values.Length - 1));
                                dataAdded = true;
                            }
                            continue;
                        }
                    }
                    else
                    {
                        jsonSelectObject = extendedObject.SelectToken(jsonPathForProperty, false);
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }

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

                        for (int idx = i+1; idx < pathCount; idx++)
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
