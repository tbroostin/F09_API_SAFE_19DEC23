// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace Ellucian.Web.Http.Extensions
{
    /// <summary>
    /// Extensions for HttpResponseMessage
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        private static bool? includeLinkSelfHeaders;
        private static object sync = new object();

        public static bool IncludeLinkSelfHeaders
        {
            get
            {
                if (!includeLinkSelfHeaders.HasValue)
                {
                    lock (sync)
                    {
                        if (!includeLinkSelfHeaders.HasValue)
                        {
                            // Return the value from app settings
                            bool includeLinkSelfHeadersConfig = false;
                            if (bool.TryParse(WebConfigurationManager.AppSettings["IncludeLinkSelfHeaders"], out includeLinkSelfHeadersConfig))
                            {
                                includeLinkSelfHeaders = includeLinkSelfHeadersConfig;
                            }
                        }
                    }
                }
                return includeLinkSelfHeaders.HasValue ? includeLinkSelfHeaders.Value : false;
            }
        }

        /// <summary>
        /// Add Pagination links in the response headers when no total record count provided
        /// </summary>
        /// <param name="response"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        public static void AddPagingHeaders(this System.Net.Http.HttpResponseMessage response, int offset, int limit)
        {
            AddPagingHeaders(response, offset, limit, -1);
        }

        /// <summary>
        /// Add Pagination links in the response headers when total record count provided
        /// </summary>
        /// <param name="response"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="TotalRecCount"></param>
        public static void AddPagingHeaders(this System.Net.Http.HttpResponseMessage response, int offset, int limit, int TotalRecCount)
        {
            // Check to see if config is setup to include link and self headers before adding them
            if (IncludeLinkSelfHeaders)
            {
                //build _link
                var request = response.RequestMessage;
                string baseURL = request.RequestUri.GetLeftPart(UriPartial.Authority);
                string absolutePath = request.RequestUri.AbsolutePath;
                var queryString = HttpUtility.ParseQueryString(request.RequestUri.Query);
                StringBuilder builder = new StringBuilder();

                //self
                string self = request.RequestUri.ToString();
                response.Headers.Add("Self", self);

                //first
                var firstQueryString = queryString;
                firstQueryString.Set("offset", "0");
                firstQueryString.Set("limit", limit.ToString());
                builder.Append(GetURL(baseURL, absolutePath, firstQueryString, "first"));

                // If current page is first page OR provided offset is out of bound,  do not generate "Previous" header 
                if (offset != 0 && offset <= TotalRecCount)
                {
                    //Previous
                    var prevOffset = ((offset - limit) > 0) ? (offset - limit) : 0;
                    var prevQueryString = queryString;
                    prevQueryString.Set("offset", prevOffset.ToString());
                    prevQueryString.Set("limit", limit.ToString());
                    builder.Append(string.Format(",{0}", GetURL(baseURL, absolutePath, prevQueryString, "prev")));
                }

                //If TotalRecCount is not provided no "Next" , "Last" & "X-Total-Count" links are provided
                if (TotalRecCount > 0)
                {
                    if (TotalRecCount - offset > limit) // If Current page is last page do not generate "Next" header
                    {
                        //next
                        var nextOffset = offset + limit;
                        var nextQueryString = queryString;
                        nextQueryString.Set("offset", nextOffset.ToString());
                        nextQueryString.Set("limit", limit.ToString());
                        builder.Append(string.Format(",{0}", GetURL(baseURL, absolutePath, nextQueryString, "next")));
                    }

                    //Last
                    var lastOffset = (TotalRecCount / limit) * limit;
                    if (lastOffset == TotalRecCount) //If last page is NULL then set the prior page as Last page
                    {
                        lastOffset = TotalRecCount - limit;
                    }
                    var lastQueryString = queryString;
                    lastQueryString.Set("offset", lastOffset.ToString());
                    lastQueryString.Set("limit", limit.ToString());
                    builder.Append(string.Format(",{0}", GetURL(baseURL, absolutePath, lastQueryString, "last")));
                }

                response.Headers.Add("Link", builder.ToString());
            }
        }

        /// <summary>
        /// Generate the URL for pagination links
        /// </summary>
        /// <param name="baseURL"></param>
        /// <param name="absolutePath"></param>
        /// <param name="queryString"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        private static string GetURL(string baseURL, string absolutePath, NameValueCollection queryString, string header)
        {
            StringBuilder builder = new StringBuilder();
            string URL = string.Format("<{0}{1}?{2}>; rel=\"{3}\"", baseURL, absolutePath, queryString, header);
            return URL;
        }

        /// <summary>
        /// Creates an HttpResponseMessage with an optional message.
        /// </summary>
        /// <param name="message">Optional message to be returned with the Response e.g. update failed.</param>
        /// <param name="statusCode">HttpStatusCode to be returned. Default is 400 (bad request)</param>
        /// <returns>void</returns>
        public static void SetErrorResponse(this System.Net.Http.HttpResponseMessage response, string message = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message = message.Replace(Environment.NewLine, " ").Replace("\n", " ");
            }

            response.StatusCode = statusCode;
            response.Content = new StringContent(message);
        }

        /// <summary>
        /// Add Total Record Count in the response headers when provided
        /// </summary>
        /// <param name="response"></param>
        /// <param name="TotalRecCount"></param>
        public static void AddTotalRecordCountHeader(this System.Net.Http.HttpResponseMessage response, int TotalRecCount)
        {
            //X-Total-Count
            if (TotalRecCount > -1)
                response.Headers.Add("X-Total-Count", Convert.ToString(TotalRecCount));
        }

        /// <summary>
        /// Add X-Max-Page-Size
        /// </summary>
        /// <param name="response"></param>
        /// <param name="maxPageSize"></param>
        public static void AddMaxPageSizeHeader(this System.Net.Http.HttpResponseMessage response, int maxPageSize)
        {
            //X-Max-Page-Size
            if (maxPageSize > -1)
                response.Headers.Add("X-Max-Page-Size", Convert.ToString(maxPageSize));
        }
    }
}
