// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Extensions;

namespace Ellucian.Web.Http
{
    /// <summary>
    /// Handles IHttpActionResult return type for paging
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedHttpActionResult<T> : IHttpActionResult where T : IEnumerable<object>
    {
        private HttpStatusCode statusCode;
        private HttpRequestMessage request;
        private Paging pagingParams;
        private T results;
        private int totalRecCount;

        /// <summary>
        /// Constructor when total record  count is unknown. Returns an OK response
        /// </summary>
        /// <param name="results"></param>
        /// <param name="pagingParams"></param>
        /// <param name="request"></param>
        public PagedHttpActionResult(T results, Paging pagingParams, HttpRequestMessage request)
            : this(results, pagingParams, -1, HttpStatusCode.OK, request)
        {
        }

        /// <summary>
        /// Constructor when total record  count is provided. Returns an OK response
        /// </summary>
        /// <param name="results"></param>
        /// <param name="pagingParams"></param>
        /// <param name="totalRecCount"></param>
        /// <param name="request"></param>
        public PagedHttpActionResult(T results, Paging pagingParams, int totalRecCount, HttpRequestMessage request)
            : this(results, pagingParams, totalRecCount, HttpStatusCode.OK, request)
        {
        }

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="results"></param>
        /// <param name="pagingParams"></param>
        /// <param name="totalRecCount"></param>
        /// <param name="statusCode"></param>
        /// <param name="request"></param>
        public PagedHttpActionResult(T results, Paging pagingParams, int totalRecCount, HttpStatusCode statusCode, HttpRequestMessage request)
        {
            this.results = results;
            this.pagingParams = pagingParams;
            this.totalRecCount = totalRecCount;
            this.statusCode = statusCode;
            this.request = request;
        }

        /// <summary>
        /// Sets the pagination link headers in response
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<System.Net.Http.HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage()
            {
                StatusCode = this.statusCode,
                RequestMessage = this.request,
            };

            //Set Content
            response.Content = new ObjectContent<T>(this.results, new JsonMediaTypeFormatter());

            //Set Headers
            if (pagingParams != null)
            {
                response.AddPagingHeaders(pagingParams.Offset, pagingParams.Limit, totalRecCount);
                if (pagingParams.DefaultLimit.HasValue && pagingParams.DefaultLimit.Value != int.MaxValue)
                {
                    response.AddMaxPageSizeHeader(pagingParams.DefaultLimit.Value);
                }
            }
            response.AddTotalRecordCountHeader(totalRecCount);
            return Task.FromResult(response);
        }
    }
}
