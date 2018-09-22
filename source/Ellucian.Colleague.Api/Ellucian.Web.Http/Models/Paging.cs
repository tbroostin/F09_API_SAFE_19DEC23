// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;

namespace Ellucian.Web.Http.Models
{
    /// <summary>
    /// Model for paging
    /// </summary>
    [ModelBinder(typeof(PagingBinder))]
    public class Paging
    {
        /// <summary>
        /// Gets & Sets the limit for Paging
        /// </summary>
        public int Limit { get; set; }
        /// <summary>
        /// Gets & Sets the Offset for Paging
        /// </summary>
        public int Offset { get; set; }

        public int? DefaultLimit { get; set; }


        /// <summary>
        /// Constructor for paging class
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        public Paging(int limit, int offset)
        {
            Limit = limit;
            Offset = offset;
            DefaultLimit = limit;
        }
    }
}
