// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Rest.Client.Exceptions;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client.Core
{
    /// <summary>
    /// Provides the low level service APIs used by the Colleague Api Client. This class cannot be
    /// extended or used outside of the Colleague Api Client libs.
    /// </summary>
    internal sealed class ColleagueServiceClient : Ellucian.Rest.Client.AsyncBaseServiceClient
    {
        public const string LoggingRestrictionsHeaderKey = "X-Logging-Restrictions";

        /// <summary>
        /// Creates a new ColleagueServiceClient.
        /// </summary>
        /// <param name="baseUrl">Base url of client requests.</param>
        /// <param name="logger"><see cref="slf4net.ILogger"/> instance.</param>
        public ColleagueServiceClient(string baseUrl, ILogger logger)
            :base(baseUrl, logger)
        {
        }

        /// <summary>
        /// Creates a new ColleagueServiceClient using the provided HttpClient.
        /// This is intended to be used for unit testing only.
        /// </summary>
        /// <param name="httpClient"><see cref="HttpClient"/> instance to use for client requests.</param>
        /// <param name="logger"><see cref="slf4net.ILogger"/> instance.</param>
        public ColleagueServiceClient(HttpClient httpClient, ILogger logger)
            :base(httpClient, logger)
        {
        }

        /// <summary>
        /// Gets or sets the maximum number of concurrent client connections this service client
        /// can make to the Colleague API.
        /// </summary>
        /// <remarks>
        /// This value must be even because the underlying <see cref="Ellucian.Rest.Client.BaseServiceClient"/> uses two
        /// instances of the HttpClient class, one to service cached requests and one to service non-cacheable requests.
        /// Each instance of HttpClient independently respects the ServicePointManager.DefaultConnectionLimit so that's
        /// why the number is being split.
        /// </remarks>
        public int MaxConnections
        {
            get
            {
                return ServicePointManager.DefaultConnectionLimit * 2;
            }
            set
            {
                if (value < 2 || value % 2 != 0)
                {
                    throw new ArgumentOutOfRangeException("value", "MaxConnections must be an even number 2 or higher.");
                }

                ServicePointManager.DefaultConnectionLimit = value / 2;
            }
        }

        /// <summary>
        /// Executes an HTTP GET request and returns the resulting response.
        /// </summary>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="user">The <see cref="ServiceClientUser"/> user making the request.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        public HttpResponseMessage ExecuteGetRequestWithResponse(string urlPath, Ellucian.Rest.Client.ServiceClientUser user, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return base.ExecuteGetRequestWithResponse(urlPath, urlArguments, headers, useCache, user);
        }

        /// <summary>
        /// Executes an HTTP GET request asynchronously and returns the resulting response.
        /// </summary>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="user">The <see cref="ServiceClientUser"/> user making the request.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        public Task<HttpResponseMessage> ExecuteGetRequestWithResponseAsync(string urlPath, Ellucian.Rest.Client.ServiceClientUser user, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return base.ExecuteGetRequestWithResponseAsync(urlPath, urlArguments, headers, useCache, user);
        }

        /// <summary>
        /// Executes an HTTP POST request and returns the resulting response.
        /// </summary>
        /// <typeparam name="T">Object to be serialized and sent.</typeparam>
        /// <param name="objectToSend">Object to be serialized and sent.</param>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="user">The <see cref="ServiceClientUser"/> user making the request.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        public HttpResponseMessage ExecutePostRequestWithResponse<T>(T objectToSend, string urlPath, Ellucian.Rest.Client.ServiceClientUser user, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return base.ExecutePostRequestWithResponse<T>(objectToSend, urlPath, urlArguments, headers, useCache, user);
        }

        /// <summary>
        /// Executes an HTTP POST request asynchronously and returns the resulting response.
        /// </summary>
        /// <typeparam name="T">Object to be serialized and sent.</typeparam>
        /// <param name="objectToSend">Object to be serialized and sent.</param>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="user">The <see cref="ServiceClientUser"/> user making the request.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        public Task<HttpResponseMessage> ExecutePostRequestWithResponseAsync<T>(T objectToSend, string urlPath, Ellucian.Rest.Client.ServiceClientUser user, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return base.ExecutePostRequestWithResponseAsync<T>(objectToSend, urlPath, urlArguments, headers, useCache, user);
        }

        /// <summary>
        /// Executes an HTTP PUT request and returns the resulting response.
        /// </summary>
        /// <typeparam name="T">Object to be serialized and sent.</typeparam>
        /// <param name="objectToSend">Object to be serialized and sent.</param>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="user">The <see cref="ServiceClientUser"/> user making the request.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        public HttpResponseMessage ExecutePutRequestWithResponse<T>(T objectToSend, string urlPath, Ellucian.Rest.Client.ServiceClientUser user, string urlArguments = null, NameValueCollection headers = null)
        {
            return base.ExecutePutRequestWithResponse<T>(objectToSend, urlPath, urlArguments, headers, user);
        }

        /// <summary>
        /// Executes an HTTP PUT request asynchronously and returns the resulting response.
        /// </summary>
        /// <typeparam name="T">Object to be serialized and sent.</typeparam>
        /// <param name="objectToSend">Object to be serialized and sent.</param>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="user">The <see cref="ServiceClientUser"/> user making the request.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        public Task<HttpResponseMessage> ExecutePutRequestWithResponseAsync<T>(T objectToSend, string urlPath, Ellucian.Rest.Client.ServiceClientUser user, string urlArguments = null, NameValueCollection headers = null)
        {
            return base.ExecutePutRequestWithResponseAsync<T>(objectToSend, urlPath, urlArguments, headers, user);
        }

        /// <summary>
        /// Executes an HTTP DELETE request and returns the resulting response.
        /// </summary>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="user">The <see cref="ServiceClientUser"/> user making the request.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        public HttpResponseMessage ExecuteDeleteRequestWithResponse(string urlPath, Ellucian.Rest.Client.ServiceClientUser user, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return base.ExecuteDeleteRequestWithResponse(urlPath, urlArguments, headers, useCache, user);
        }

        /// <summary>
        /// Executes an HTTP DELETE request asynchronously and returns the resulting response.
        /// </summary>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="user">The <see cref="ServiceClientUser"/> user making the request.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        public Task<HttpResponseMessage> ExecuteDeleteRequestWithResponseAsync(string urlPath, Ellucian.Rest.Client.ServiceClientUser user, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return base.ExecuteDeleteRequestWithResponseAsync(urlPath, urlArguments, headers, useCache, user);
        }

        /// <summary>
        /// Called just before sending the HTTP request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>request</param>
        protected override async Task OnRequestAsync(HttpRequestMessage request)
        {
            if (logger.IsDebugEnabled)
            {
                try
                {
                    StringBuilder debug = new StringBuilder();
                    debug.AppendLine(string.Format("Colleague API request [{0}]: {1} {2}", request.GetHashCode(), request.Method.ToString(), request.RequestUri.ToString()));
                    debug.AppendLine("Headers:");
                    if (!string.IsNullOrEmpty(request.Headers.ToString()))
                    {
                        debug.Append(request.Headers.ToString());
                    }
                    if (request.Content != null)
                    {
                        debug.Append(request.Content.Headers.ToString());
                        debug.AppendLine("Body:");
                        if (CanLogRequestContent(request))
                        {
                            debug.Append(await request.Content.ReadAsStringAsync());
                        }
                        else
                        {
                            debug.Append("## This request has requested to not be logged ##");
                        }
                    }
                    logger.Debug(debug.ToString());
                    debug = null;
                }
                catch (Exception e)
                {
                    logger.Debug(e, "Unable to log Colleague API request details");
                }
            }
        }

        /// <summary>
        /// Called upon receiving the HTTP response.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> response</param>
        protected override async Task OnResponseAsync(HttpResponseMessage response)
        {
            if (logger.IsDebugEnabled)
            {
                try
                {
                    StringBuilder debug = new StringBuilder();
                    debug.AppendLine(string.Format("Colleague API response [{0}]: {1} {2} - {3}, {4}", response.RequestMessage.GetHashCode(), response.RequestMessage.Method.ToString(), response.RequestMessage.RequestUri.ToString(), (int)response.StatusCode, response.ReasonPhrase));
                    debug.AppendLine("Headers:");
                    if (!string.IsNullOrEmpty(response.Headers.ToString()))
                    {
                        debug.Append(response.Headers.ToString());
                    }
                    if (response.Content != null)
                    {
                        debug.Append(response.Content.Headers.ToString());
                        debug.AppendLine("Body:");
                        if (CanLogResponseContent(response))
                        {
                            debug.Append(await response.Content.ReadAsStringAsync());
                        }
                        else
                        {
                            debug.Append("## This response has requested to not be logged ##");
                        }
                    }
                    logger.Debug(debug.ToString());
                    debug = null;
                }
                catch (Exception e)
                {
                    logger.Debug(e, "Unable to log Colleague API response details");
                }
            }
        }

        /// <summary>
        /// Called upon receiving an error response (anything other than a 401 or 404).
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> response</param>
        protected override async Task OnErrorResponseAsync(HttpResponseMessage response)
        {
            logger.Error("Unhandled non-successful HTTP status: " + response.RequestMessage.RequestUri);
            string rawContent = await response.Content.ReadAsStringAsync();
            ColleagueApiErrorDetail details = TryParseColleagueApiErrorDetail(rawContent);

            HttpRequestFailedException exception = new HttpRequestFailedException((details != null && !string.IsNullOrEmpty(details.Message)) ? details.Message : rawContent,
                response.StatusCode);
            if (details != null)
            {
                exception.Data.Add(typeof(ColleagueApiErrorDetail).Name, details);
            }
            throw exception;
        }

        /// <summary>
        /// Called upon receiving an HTTP not found (404) response.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> response</param>
        protected override async Task OnNotFoundResponseAsync(HttpResponseMessage response)
        {
            logger.Error("Resource not found: " + response.RequestMessage.RequestUri);
            string rawContent = await response.Content.ReadAsStringAsync();
            ColleagueApiErrorDetail details = TryParseColleagueApiErrorDetail(rawContent);

            ResourceNotFoundException exception = new ResourceNotFoundException((details != null && !string.IsNullOrEmpty(details.Message)) ? details.Message : rawContent,
                response.StatusCode);
            if (details != null)
            {
                exception.Data.Add(typeof(ColleagueApiErrorDetail).Name, details);
            }
            throw exception;
        }

        /// <summary>
        /// Called upon receiving a unauthorized (401) response.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> response</param>
        protected override async Task OnUnauthorizedResponseAsync(HttpResponseMessage response)
        {
            logger.Error("Unauthorized: " + response.RequestMessage.RequestUri);
            string rawContent = await response.Content.ReadAsStringAsync();
            ColleagueApiErrorDetail details = TryParseColleagueApiErrorDetail(rawContent);

            LoginException exception = new LoginException((details != null && !string.IsNullOrEmpty(details.Message)) ? details.Message : rawContent,
                response.StatusCode);
            if (details != null)
            {
                exception.Data.Add(typeof(ColleagueApiErrorDetail).Name, details);
            }
            throw exception;
        }

        /// <summary>
        /// Attempts to parse the input string into a <see cref="ColleagueApiErrorDetail"/> object.
        /// </summary>
        /// <param name="response">Http response body as a string.</param>
        /// <returns>A <see cref="ColleagueApiErrorDetail"/> object or null if the input string cannot be parsed into a <see cref="ColleagueApiErrorDetail"/> object.</returns>
        private ColleagueApiErrorDetail TryParseColleagueApiErrorDetail(string response)
        {
            ColleagueApiErrorDetail details = null;
            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    details = JsonConvert.DeserializeObject<ColleagueApiErrorDetail>(response);
                    logger.Debug("ColleagueApiErrorDetail: " + details.ToString());
                }
                catch (Exception)
                {
                    details = null;
                }
            }
            return details;
        }

        /// <summary>
        /// Determines if the request content can be logged.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>request</param>
        /// <returns>True if the request content can be logged, false if not.</returns>
        private bool CanLogRequestContent(HttpRequestMessage request)
        {
            if (request != null)
            {
                try
                {
                    IEnumerable<string> values;
                    if (request.Headers.TryGetValues(LoggingRestrictionsHeaderKey, out values))
                    {
                        if (values != null && !string.IsNullOrEmpty(values.FirstOrDefault()))
                        {
                            LoggingRestrictions restrictions = (LoggingRestrictions)Enum.Parse(typeof(LoggingRestrictions), values.FirstOrDefault());
                            if ((restrictions & LoggingRestrictions.DoNotLogRequestContent) == LoggingRestrictions.DoNotLogRequestContent)
                            {
                                return false;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    logger.Debug(string.Format("Unable to parse {0} header.", LoggingRestrictionsHeaderKey));
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if the response content can be logged.
        /// </summary>
        /// <param name="request">The <see cref="HttpResponseMessage"/> response</param>
        /// <returns>True if the response content can be logged, false if not.</returns>
        private bool CanLogResponseContent(HttpResponseMessage response)
        {
            if (response != null)
            {
                try
                {
                    IEnumerable<string> values;
                    if (response.RequestMessage.Headers.TryGetValues(LoggingRestrictionsHeaderKey, out values))
                    {
                        if (values != null && !string.IsNullOrEmpty(values.FirstOrDefault()))
                        {
                            LoggingRestrictions restrictions = (LoggingRestrictions)Enum.Parse(typeof(LoggingRestrictions), values.FirstOrDefault());
                            if ((restrictions & LoggingRestrictions.DoNotLogResponseContent) == LoggingRestrictions.DoNotLogResponseContent)
                            {
                                return false;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    logger.Debug(string.Format("Unable to parse {0} header.", LoggingRestrictionsHeaderKey));
                }
            }
            return true;
        }

        /// <summary>
        /// Disposes of resources used by this <see cref="ColleagueServiceClient"/> instance.
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();
        }
    }
}