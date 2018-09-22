// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Web.Http.Models
{
    /// <summary>
    ///  Model for Sorting
    /// </summary>
    public class Sorting
    {
        /// <summary>
        /// Gets and Sets the field to SortBy
        /// </summary>
        public string SortByField { get; set; }
        /// <summary>
        /// Gets and Sets the Sort Order
        /// </summary>
        public SortingOrder SortOrder { get; set; }
        /// <summary>
        /// Constructor for Sorting Class
        /// </summary>
        /// <param name="sortByField">Sort Field</param>
        /// <param name="sortorder">Sort Order (asc,desc)</param>
        public Sorting(string sortByField, SortingOrder sortorder)
        {
            SortByField = sortByField;
            SortOrder = sortorder;
        }

    }
}
