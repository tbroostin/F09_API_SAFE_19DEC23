// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;

namespace Ellucian.Web.Http.Models
{
    /// <summary>
    /// Model for QueryStringFilter
    /// </summary>
    [ModelBinder(typeof(QueryStringFilterBinder))]
    public class QueryStringFilter
    {

        /// <summary>
        /// query string parameter associated with this filter 
        /// </summary>
        public string QueryName { get; set; }

        /// <summary>
        /// Json extracted from query string 
        /// </summary>
        public string JsonQuery { get; set; }


        /// <summary>
        ///  Constructor 
        /// </summary>
        /// <param name="criteria"></param>
        public QueryStringFilter(string queryName, string jsonQuery)
        {
            QueryName = queryName;
            JsonQuery = jsonQuery;
        }
    }
}
