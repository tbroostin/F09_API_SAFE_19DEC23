// Copyright 2017 - 2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Web.Http.Utilities
{
    public static class ErrorMessageHelper
    {
        private class FormattedErrorMessage
        {
            public string code { get; set; }
            public string description { get; set; }
            public string message { get; set; }
        }

        private class FormattedErrors
        {
            public List<FormattedErrorMessage> errors { get; set; }
        }

        /// <summary>
        /// Apply Extensibility to a single or list of json objects
        /// </summary>
        /// <param name="jsonToExtend">json body to extend</param>
        /// <param name="extendedDataList">settings and data to extend</param>
        /// <param name="logger">logger</param>
        /// <returns></returns>
        public static JContainer ConvertToV2ErrorMessage(JContainer jsonToExtend, int status, ILogger logger)
        {
            try
            {
                if (jsonToExtend.Type == JTokenType.Array)
                {
                    var jArray = new JArray();
                    
                    jsonToExtend.ForEach(j =>
                    {
                        var errors = j.SelectToken("$.errors", false);
                        if (errors != null && errors.Type == (JTokenType.Array))
                        {
                            jArray.Add(j);
                            return;
                        }

                        var convertedObject = ConvertSingleJsonObject(j, status, logger);
                        if(convertedObject == null)
                        {
                            jArray.Add(j);
                            return;
                        }
                        jArray.Add(convertedObject);
                    });
                    return jArray;
                }

                if (jsonToExtend.Type == JTokenType.Object)
                {
                    var errors = jsonToExtend.SelectToken("$.errors", false);
                    if (errors != null && errors.Type == (JTokenType.Array))
                    {
                        return jsonToExtend;
                    }
                    return (JContainer)ConvertSingleJsonObject(jsonToExtend, status, logger);
                }
                
            }
            catch (Exception ex)
            {
                if (logger == null) return null;

                var sb = new StringBuilder();
                sb.Append("Convertion of Error Message to v2 standards failed.");
                logger.Error(ex, sb.ToString());

                return null;
            }

            return null;

        }

        private static JToken ConvertSingleJsonObject(JToken jsonObjectToConvert, int status, ILogger logger)
        {
            var messageText = (string)jsonObjectToConvert.SelectToken("$.Message", false);
            if (string.IsNullOrEmpty(messageText)) messageText = (string)jsonObjectToConvert.SelectToken("$.message", false);
            if (string.IsNullOrEmpty(messageText)) messageText = (string)jsonObjectToConvert.SelectToken("$.description", false);
            if (string.IsNullOrEmpty(messageText)) messageText = (string)jsonObjectToConvert.SelectToken("$.Description", false);
            if (string.IsNullOrEmpty(messageText))
            {
                return jsonObjectToConvert;
            }
            // Choose the default error code and description for the status code returned.
            string code = "Global.Internal.Error";
            string description = "Unspecified Error on the system which prevented execution.";
            switch (status)
            {
                case 401:
                    {
                        code = "Authentication.Required";
                        description = "Authentication failed or wasn't provided.";
                        break;
                    }
                case 403:
                    {
                        code = "Access.Denied";
                        description = "Client not authorized to perform this action.";
                        break;
                    }
                case 404:
                    {
                        code = "Key.Not.Found";
                        description = "The provided key was not found in the database. ";
                        break;
                    }
            }
            var convertedMessage = new FormattedErrorMessage()
            {
                code = code,
                description = description,
                message = messageText
            };
            var convertedErrorList = new FormattedErrors() { errors = new List<FormattedErrorMessage>() { convertedMessage } };
            var objectToReturn = JObject.FromObject(convertedErrorList);

            return objectToReturn;
        }
    }
}
