// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

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
        /// The global identifier of the resource in error.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Guid { get; set; }

        /// <summary>
        /// The source applications data reference identifier for the primary data entity used to create the resource
        /// </summary>
        [JsonProperty("sourceId", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

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
        /// <param name="guid">The global identifier of the resource in error.</param>
        /// <param name="id">The source applications data reference identifier for the primary data entity used to create the resource.</param>
        public IntegrationApiError(string code, string description = null, string message = null,
             HttpStatusCode statusCode = HttpStatusCode.BadRequest, string guid = null, string id = null)
        {
            if (string.IsNullOrEmpty(code))
            {
                return;
            }

            Code = code;
            Description = description;
            Message = message;
            StatusCode = statusCode;
            Guid = guid;
            Id = id;
        }

        /// <summary>
        /// Override the Equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            IntegrationApiError other = obj as IntegrationApiError;
            if (other == null)
            {
                return false;
            }
            if (other.Code == Code &&
                other.Description == Description &&
                other.Guid == Guid &&
                other.Id == Id &&
                other.Message == Message &&
                other.StatusCode == StatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
