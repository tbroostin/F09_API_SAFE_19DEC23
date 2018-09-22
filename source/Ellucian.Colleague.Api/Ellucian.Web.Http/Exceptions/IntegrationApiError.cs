// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Net;
using Newtonsoft.Json;

namespace Ellucian.Web.Http.Exceptions
{
    /// <summary>
    /// An error returned by the Integration API
    /// </summary>
    [Serializable]
    public class IntegrationApiError
    {
        /// <summary>
        /// Error code
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Standard description of error
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Message describing the specific error condition
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// HTTP status code corresponding to this error
        /// </summary>
        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        [JsonConstructor]
        public IntegrationApiError()
        {
        }

        /// <summary>
        /// Convenience constructor to create an error
        /// </summary>
        /// <param name="code">Error code, required</param>
        /// <param name="description">Error description, optional</param>
        /// <param name="message">Error message, optional</param>
        /// <param name="statusCode">HTTP return code, optional</param>
        public IntegrationApiError(string code, string description = null, string message = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (string.IsNullOrEmpty(code))
            {
                return;
            }

            Code = code;
            Description = description;
            Message = message;
            StatusCode = statusCode;
        }
    }
}
