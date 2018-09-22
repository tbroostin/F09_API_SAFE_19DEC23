// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Web.Http.Models;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Ellucian.Web.Http.Extensions
{
    /// <summary>
    /// Extensions for HttpRequestMessage
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Retrieves filter parameters from query string
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static QueryStringFilter GetFilterParameters(this System.Net.Http.HttpRequestMessage request, string namedQuery)
        {
            QueryStringFilter queryStringFilter = null;

            var queryString = HttpUtility.ParseQueryString(request.RequestUri.Query);

            if (queryString != null)
            {
                var json = queryString.Get(namedQuery);
                if (!string.IsNullOrEmpty(json))
                {
                    if (!json.Contains(":"))
                    {
                        json = json.Insert(1, string.Concat("\"", namedQuery, "\" : "));
                    }
                    queryStringFilter = new QueryStringFilter(namedQuery, json);
                }
            }
            return queryStringFilter;
        }

        /// <summary>
        /// Retrieves Limit and Offset from Query String, Validates and set the Paging params
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Paging GetPagingParameters(this System.Net.Http.HttpRequestMessage request, int defaultLimit)
        {
            var queryString = HttpUtility.ParseQueryString(request.RequestUri.Query);
            int offset;
            int limit;
            Paging pagingParams = null;

            //Check if offset and limit params exists in URL
            if (queryString["offset"] != null && queryString["limit"] != null)
            {
                //Check for non-numeric values
                bool validOffset = int.TryParse(queryString.Get("offset"), out offset);
                bool validLimit = int.TryParse(queryString.Get("limit"), out limit);

                //Check Limit - if limit less than or equal to zero or provided limit > default limit OR InvalidLimit, 
                //Assign it to DefaultLimit
                if (limit > defaultLimit || limit <= 0 || !validLimit)
                    limit = defaultLimit;

                //Check if offset is negative
                if (offset < 0 || !validOffset)
                    offset = 0;

                pagingParams = new Paging(limit, offset);
                pagingParams.DefaultLimit = defaultLimit;
            }
            // If the limit parameter is omitted, the API will default to its determined page size.
            else if (queryString["offset"] != null && queryString["limit"] == null)
            {
                //Check for non-numeric values
                bool validOffset = int.TryParse(queryString.Get("offset"), out offset);
                
                limit = defaultLimit;

                //Check if offset is negative
                if (offset < 0 || !validOffset)
                    offset = 0;

                pagingParams = new Paging(limit, offset);
                pagingParams.DefaultLimit = defaultLimit;
            }
            // If offset is omitted, the API SHOULD assume 0
            else if (queryString["offset"] == null && queryString["limit"] != null)
            {
                //Check for non-numeric values
               bool validLimit = int.TryParse(queryString.Get("limit"), out limit);

                //Check Limit - if limit less than or equal to zero or provided limit > default limit OR InvalidLimit, 
                //Assign it to DefaultLimit
                if (limit > defaultLimit || limit <= 0 || !validLimit)
                    limit = defaultLimit;

                offset = 0;

                pagingParams = new Paging(limit, offset);
                pagingParams.DefaultLimit = defaultLimit;
            }
            return pagingParams;
        }

        /// <summary>
        /// Retrieves Sort from Query String, Validates and set the Sorting params
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static List<Sorting> GetSortingParameters(this System.Net.Http.HttpRequestMessage request)
        {
            var queryString = HttpUtility.ParseQueryString(request.RequestUri.Query);
            List<Sorting> sortParameters = null;

            try
            {

                if (queryString["sort"] != null)
                {
                    //Retrieve the json values from querystring
                    var parsedQueryString = HttpUtility.ParseQueryString(queryString["sort"]);

                    //Decode the encoded string
                    var decodedValue = HttpUtility.UrlDecode(parsedQueryString.ToString());

                    //Convert the decoded json string to jArray
                    JArray jsonArray = JArray.Parse(decodedValue);

                    if (jsonArray.Count > 0)
                    {
                        sortParameters = new List<Sorting>(); ;
                        //Convert the retrieved query strings to a list of type Sorting
                        foreach (JObject content in jsonArray.Children<JObject>())
                        {
                            foreach (JProperty prop in content.Properties())
                            {
                                SortingOrder sortOrder;
                                switch (prop.Value.ToString().ToLower())
                                {
                                    case "asc":
                                        sortOrder = SortingOrder.Asc;
                                        break;
                                    case "desc":
                                        sortOrder = SortingOrder.Desc;
                                        break;
                                    default:
                                        throw new ArgumentException("Invalid sorting order provided");
                                }
                                sortParameters.Add(new Sorting(prop.Name, sortOrder));
                            }
                        }
                    }
                }
            }
            catch(Exception)
            { throw; }
            return sortParameters;
        }
    }
}