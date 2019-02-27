// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Ellucian.Web.Http.Exceptions
{
    /// <summary>
    /// Integration API exception
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class IntegrationApiException : Exception
    {
        /// <summary>
        /// List of error codes and messages
        /// </summary>
        [JsonProperty("errors")]
        public List<IntegrationApiError> Errors { get; set; }

        /// <summary>
        /// Get the HTTP return code to use with the errors contained in this exception
        /// </summary>
        public HttpStatusCode HttpStatusCode
        {
            get { return Errors.Any() ? Errors.Max(x => x.StatusCode) : HttpStatusCode.BadRequest; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public IntegrationApiException()
            : base ("Integration API exception")
        {
            Errors = new List<IntegrationApiError>();
        }

        /// <summary>
        /// Overload Constructor with message
        /// </summary>
        /// <param name="message">Exception message</param>
        public IntegrationApiException(string message)
            : base(message)
        {
            Errors = new List<IntegrationApiError>();
        }

        /// <summary>
        /// Overload Constructor with message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="e">Inner exception</param>
        public IntegrationApiException(string message, Exception e)
            : base(message, e)
        {
            Errors = new List<IntegrationApiError>();
        }

        /// <summary>
        /// Overload Constructor used with serialization
        /// </summary>
        /// <param name="info">Data describing how to serialize or deserialize the exception</param>
        /// <param name="context">Provides context for serialization</param>
        protected IntegrationApiException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Overload Constructor with message and error
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="error">Integration API error</param>
        public IntegrationApiException(string message, IntegrationApiError error)
            : base(message)
        {
            Errors = new List<IntegrationApiError>();
            if (error != null)
            {
                AddError(error);
            }
        }

        /// <summary>
        /// Overload Constructor with message and errors
        /// </summary>
        /// <param name="message">Exception message</param>
        public IntegrationApiException(string message, IEnumerable<IntegrationApiError> errors)
            : base(message)
        {
            Errors = new List<IntegrationApiError>();
            if (errors != null  && errors.Count() > 0)
            {
                AddErrors(errors);
            }
        }

        /// <summary>
        /// Add an error to the list of errors
        /// </summary>
        /// <param name="error">Integration API Error</param>
        public void AddError(IntegrationApiError error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            if (!Errors.Contains<IntegrationApiError>(error))
                Errors.Add(error);
        }

        /// <summary>
        /// Add multiple errors to the list of errors
        /// </summary>
        /// <param name="errors">List of Integration API Error</param>
        public void AddErrors(IEnumerable<IntegrationApiError> errors)
        {
            foreach (var error in errors)
            {
                AddError(error);
            }
        }

        /// <summary>
        /// Override of ToString(), used for logging
        /// </summary>
        /// <returns>Serialized version of the error info</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("IntegrationApiErrors: ");
            if (!string.IsNullOrEmpty(Message))
            {
                builder.Append("[Message: " + Message + "]" + Environment.NewLine);
            }
            for (int i = 0; i < Errors.Count; i++ )
            {
                if (i > 0)
                {
                    builder.Append("," + Environment.NewLine);
                }
                builder.Append("Error[" + i.ToString() + "]:");
                builder.Append(FormatErrorOutput(Errors[i]));
            }

            return builder.ToString();
        }

        /// <summary>
        /// String for formatting log output
        /// </summary>
        private const string _logValue = "[{0}, {1}, {2}, {3}]";
        /// <summary>
        /// Format an error for inclusion in a log
        /// </summary>
        /// <param name="error">Error code</param>
        /// <returns>String value showing all the input arguments</returns>
        private string FormatErrorOutput(IntegrationApiError error)
        {
            return string.Format(_logValue, error.Code, error.Description, error.Message, error.StatusCode.ToString());
        }
    }
}
