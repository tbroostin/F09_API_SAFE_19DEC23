// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Web.Http.Utilities
{
    /// <summary>
    /// Helper Class for Data Privacy
    /// </summary>
    public static class DataPrivacy
    {
        /// <summary>
        /// Method to perform DataPrivacy on a JObject object (single return aka get/post/put)
        /// </summary>
        /// <param name="jsonContainer">JContainer to work on</param>
        /// <param name="dataPrivacySettingList">List of data privacy settings to use</param>
        /// <param name="logger">logger to log with</param>
        /// <returns>updated JContainer if properties were removed, null if nothing removed</returns>
        public static JContainer ApplyDataPrivacy(JContainer jsonContainer, IEnumerable<string> dataPrivacySettingList, ILogger logger)
        {
            var orginalContentLength = jsonContainer.ToString().Length;

            //try to apply data privacy, catch error and log what settings failed if the is an error
            try
            {
                if (jsonContainer.Type == JTokenType.Array)
                {
                    jsonContainer.ForEach(j =>
                    {
                        dataPrivacySettingList.ForEach(s =>
                        {
                            DataPrivacy.RemovePropertiesForDataPrivacy(s, (JObject)j, logger);
                        });
                    });
                }
                else if (jsonContainer.Type == JTokenType.Object)
                {
                    dataPrivacySettingList.ForEach(s =>
                    {
                        DataPrivacy.RemovePropertiesForDataPrivacy(s, (JObject)jsonContainer, logger);
                    });
                }

                //check length of the strings from before dataprivacy was applied to when it was to determine is anything was removed.
                //The remove function in json.net does not return anything
                var propertiesRemoved = orginalContentLength > jsonContainer.ToString().Length;
                
                //if properties were removed attempt to remove empty/null properties
                if (propertiesRemoved)
                {
                    var emptyPropsRemoved = RemoveOrReplaceEmptyProperties(jsonContainer, logger);
                    if (emptyPropsRemoved != null)
                    {//if method returns an object it removed empty properties
                        return emptyPropsRemoved;
                    }
                    else
                    {//remove method either errored out or did not remove anything
                        return jsonContainer;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    var sb = new StringBuilder();
                    sb.Append("Data Privacy Failed to apply with the following settings from the EDPS form. (");
                    dataPrivacySettingList.ForEach(s =>
                    {
                        sb.Append(s);
                        sb.Append(", ");
                    });
                    sb.Append(")");

                    logger.Error(ex, sb.ToString());
                }

                return null;
            }
        }

        /// <summary>
        /// Removes empty properties from a JContainer or replace objects with null and leave empty properties and arrays
        /// </summary>
        /// <param name="content">JContainer content</param>
        /// <param name="logger">logger to log with</param>
        /// <param name="replaceEmptyObjectsWithNull">false means remove properties, true means replace objects with null and leave empty properties, arrays</param>
        /// <returns>JContainer with empty properties removed or null if none removed or an error occcured</returns>
        public static JContainer RemoveOrReplaceEmptyProperties(JContainer content, ILogger logger, bool replaceEmptyObjectsWithNull = false)
        {
            if (content.Type == JTokenType.Array)
            {
                try
                {
                    return (JArray)RemoveOrReplaceEmptyChildren(content, replaceEmptyObjectsWithNull);
                }
                catch (Exception ex)
                {

                    if (logger != null)
                    {
                        logger.Error(ex, "Failed to clear null values");
                    }
                    return null;
                }
            }
            else if (content.Type == JTokenType.Object)
            {
                try
                {
                    return (JObject)RemoveOrReplaceEmptyChildren(content, replaceEmptyObjectsWithNull);
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.Error(ex, "Failed to clear null values");
                    }
                    return null;
                }    
            }

            return null;
        }

        /// <summary>
        /// Method to remove json properties based on propertyPath setting
        /// </summary>
        /// <param name="propertyPath"></param>
        /// <param name="jsonToModify"></param>
        /// <param name="logger"></param>
        private static void RemovePropertiesForDataPrivacy(string propertyPath, JObject jsonToModify, ILogger logger)
        {
            string invalidDataPrivacyFormat = "Data Privacy setting is not valid. Setting '{0}' is not structured correctly. {1}";
            //empty or null string, immediate return
            if (string.IsNullOrEmpty(propertyPath))
                return;
            bool loggerAvailable = logger != null;

            try
            {
                //split path by . as that is the property separator
                string[] splitPath = propertyPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                //make sure there are elements in the string array, split is done with remove empty option
                if (splitPath.Any())
                {
                    var splitCount = splitPath.Count();

                    //if there is only one entry remove and exit immediately. 
                    if (splitCount == 1)
                    {
                        //TODO:figure out how to implement == checks on top level property
                        jsonToModify.Remove(splitPath[0]);
                        return;
                    }

                    if (splitCount == 2 && splitPath[1].StartsWith("@"))
                    {
                        string arrayTypeSearchFormat = "$.{0}[?(@.{1} == '{2}')]";
                        string arg1 = splitPath[0];
                        
                        if (!splitPath[1].Contains("=="))
                        {
                            if (loggerAvailable)
                            {
                                logger.Error(string.Format(invalidDataPrivacyFormat, ConcatRemainingSettings(splitPath, splitCount), "Value assignment with == is missing."));
                            }
                            return;
                        }

                        var fieldAndValue = splitPath[1].Replace("@", "").Replace("==", ".");

                        var splitFieldAndValue = fieldAndValue.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                        if (splitFieldAndValue.Count() < 2)
                        {
                            if (loggerAvailable)
                            {
                                logger.Error(string.Format(invalidDataPrivacyFormat,
                                    ConcatRemainingSettings(splitPath, splitCount),
                                    "There are not enough values remaining to process correctly."));
                            }
                        }

                        string arg2 = splitFieldAndValue[0];
                        string arg3 = splitFieldAndValue[1];

                        string formatedJsonQuery = string.Format(arrayTypeSearchFormat, arg1, arg2, arg3);

                        //since we can't remove from a jsontoken collection we have to loop through and find all the possible matches one by one and remove one by one
                        bool tokenExists = true;
                        do
                        {
                            var selectedToken = jsonToModify.SelectToken(formatedJsonQuery, false);
                            if (selectedToken != null)
                            {
                                selectedToken.Remove();
                            }
                            else
                            {
                                tokenExists = false;
                            }
                        } while (tokenExists);
                        
                        return;
                    }

                    //this means that the selection is a property of the first one that needs to be removed by a value
                    if (splitCount > 2 && splitPath[1].StartsWith("@"))
                    {
                        string arrayTypeSearchFormat = "$.{0}[?(@.{1}.{2} == '{3}')]";

                        string arg1 = splitPath[0];
                        string arg2 = splitPath[1].Replace("@", "");

                        if (!splitPath[2].Contains("=="))
                        {
                            if (loggerAvailable)
                            {
                                logger.Error(string.Format(invalidDataPrivacyFormat, ConcatRemainingSettings(splitPath, splitCount), "Value assignment with == is missing."));
                            }
                            return;
                        }

                        var fieldAndValue = splitPath[2].Replace("==", ".");

                        var splitFieldAndValue = fieldAndValue.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                        if (splitFieldAndValue.Count() < 2)
                        {
                            if (loggerAvailable)
                            {
                                logger.Error(string.Format(invalidDataPrivacyFormat,
                                    ConcatRemainingSettings(splitPath, splitCount),
                                    "There are not enough values remaining to process correctly."));
                            }
                        }

                        string arg3 = splitFieldAndValue[0];
                        string arg4 = splitFieldAndValue[1];

                        string formatedJsonQuery = string.Format(arrayTypeSearchFormat, arg1, arg2, arg3, arg4);

                        //since we can't remove from a jsontoken collection we have to loop through and find all the possible matches one by one and remove one by one
                        bool tokenExists = true;
                        do
                        {
                            var selectedToken = jsonToModify.SelectToken(formatedJsonQuery, false);
                            if (selectedToken != null)
                            {
                                selectedToken.Remove();
                            }
                            else
                            {
                                tokenExists = false;
                            }
                        } while (tokenExists);
                        
                        return;
                    }

                    //get token that is the first property
                    var foundToken = jsonToModify.SelectToken(string.Concat("$.", splitPath[0]));

                    //nothing found, return
                    if (foundToken == null)
                    {
                        return;
                    }

                    //check if this is an array return.
                    if (foundToken.Type == JTokenType.Array)
                    {
                        //only two in the property list, means the property to remove is a child of the property in the array
                        //so just remove it directly
                        if (splitCount == 2)
                        {
                            foundToken.ForEach(t =>
                            {
                                JProperty removeMe = t.Children<JProperty>().FirstOrDefault(p => p.Name == splitPath[1]);
                                if (removeMe != null)
                                {
                                    removeMe.Remove();
                                }
                            });

                            return;
                        }
                        else if (splitCount >= 3) //more than two in the property list, means you need to pass it down and recursively call back in
                        {
                            var remainingPropertyString = ConcatRemainingSettings(splitPath, splitCount);

                            foundToken.ForEach(t =>
                            {
                                RemovePropertiesForDataPrivacy(remainingPropertyString, (JObject)t, logger);
                            });
                            return;
                        }
                    }
                    else
                    {
                        if (splitCount == 2)
                        {
                            //property isn't an array, recursively call this method, only need to go one more level down though.
                            RemovePropertiesForDataPrivacy(splitPath[1], (JObject)foundToken, logger);
                            return;
                        }
                        else
                        {
                            //property isn't an array, recursively call this method
                            RemovePropertiesForDataPrivacy(ConcatRemainingSettings(splitPath, splitCount), (JObject)foundToken, logger);
                            return;
                        }
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                if (loggerAvailable)
                {
                    logger.Error(ex.ToString());
                }
                return;
            }
        }

        /// <summary>
        /// Remove empty children by recursively examining the entire json object unless replaceEmptyObjectsWithNull is set
        /// </summary>
        /// <param name="token">token to examine</param>
        /// <param name="replaceEmptyObjectsWithNull">false means remove properties, true means replace objects with null and leave empty properties, arrays</param>
        /// <returns></returns>
        private static JToken RemoveOrReplaceEmptyChildren(JToken token, bool replaceEmptyObjectsWithNull = false) 
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    JToken child = prop.Value;
                    if (child.HasValues)
                    {
                        child = RemoveOrReplaceEmptyChildren(child, replaceEmptyObjectsWithNull);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(prop.Name, child);
                    }
                    else if(replaceEmptyObjectsWithNull && child.Type == JTokenType.Object)
                    {
                        copy.Add(prop.Name, null);
                    }
                    else if(replaceEmptyObjectsWithNull)
                    {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = RemoveOrReplaceEmptyChildren(child, replaceEmptyObjectsWithNull);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(child);
                    }
                    else if (replaceEmptyObjectsWithNull)
                    {
                        copy.Add(child);
                    }
                }
                return copy;
            }
            return token;
        }

        private static bool IsEmpty(JToken token)
        {
            return (token.Type == JTokenType.Null) ||
               (token.Type == JTokenType.Array && !token.HasValues) ||
               (token.Type == JTokenType.Object && !token.HasValues);
        }

        /// <summary>
        /// concat the remaining data privacy settings strings
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="remainingCount"></param>
        /// <returns></returns>
        private static string ConcatRemainingSettings(string[] settings, int remainingCount)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 1; i < remainingCount; i++)
            {
                sb.Append(settings[i]);
                sb.Append(".");
            }

            return sb.ToString();
        }
    }
}
