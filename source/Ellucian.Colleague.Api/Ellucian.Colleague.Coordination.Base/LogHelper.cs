// Copyright 2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using slf4net;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base
{
    /// <summary>
    /// This static generic class permits logging an object's properties.
    /// </summary>
    /// <typeparam name="TModel">The generic class type</typeparam>
    public static class LogHelper<TModel> where TModel : class
    {
        /// <summary>
        /// Method to serialize any generic object using the logger while only including values for explicitly needed properties.
        /// </summary>
        /// <param name="logger">The logger object to use.</param>
        /// <param name="logLevel">The log level (specified as a string) to use for the log entry. If no value is passed, the "Debug" level is used.</param>
        /// <param name="genericObj">The object whose properties should be logged.</param>
        /// <param name="propertiesToLog">The string array (whitelist) specifying what properties should be logged. Note that the property names are CASE SENSITIVE.</param>
        public static void LogObjectUsingPropertyWhitelist(ILogger logger, string logLevel, TModel genericObj, params string[] propertiesToLog)
        {
            try
            {
                // Determine the type of the generic object
                Type genericObjectType = genericObj.GetType();

                // Get the details about the class properties
                PropertyInfo[] arrayPropertyInfos = genericObjectType.GetProperties();

                // Create an array of property/value strings
                List<string> logParts = new List<string>();

                // Loop through all properties...
                foreach (PropertyInfo property in arrayPropertyInfos)
                {
                    if (property != null)
                    {
                        // If the property is listed in the whitelist...
                        if (propertiesToLog.Contains(property.Name))
                        {
                            // Add the property and its value to the array.
                            try
                            {
                                logParts.Add(property.Name + ": " + property.GetValue(genericObj).ToString());
                            }
                            catch (Exception)
                            {
                                logParts.Add(property.Name + ": null");
                            }
                        }
                        else
                        {
                            // Add only the property's name to the array.
                            logParts.Add(property.Name + ": ***");
                        }
                    }
                }

                // Write to the log file.
                LogByLevel(logger, logLevel, logParts);
            }
            catch (Exception e)
            {
                logger.Debug(e, "Could not scrub and serialize the object.");
            }
        }

        /// <summary>
        /// Log the object parts using the desired log level.
        /// </summary>
        /// <param name="logger">The logger object to use.</param>
        /// <param name="logLevel">The log level (specified as a string) to use for the log entry.</param>
        /// <param name="logParts">The strings to be joined by ", " when writing the log entry.</param>
        private static void LogByLevel(ILogger logger, string logLevel, List<string> logParts)
        {
            string ll = "debug";
            if (logLevel != null)
            {
                ll = logLevel.ToLower();
            }


            if (ll.StartsWith("i"))
            {
                logger.Info(string.Join(", ", logParts));
            }
            else if (ll.StartsWith("w"))
            {
                logger.Warn(string.Join(", ", logParts));
            }
            else if (ll.StartsWith("e"))
            {
                logger.Error(string.Join(", ", logParts));
            }
            else
            {
                logger.Debug(string.Join(", ", logParts));
            }
        }
    }
}
