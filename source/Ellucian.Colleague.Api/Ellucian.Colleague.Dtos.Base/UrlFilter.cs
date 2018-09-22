// Copyright 2014 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Filter
    /// </summary>
    public class UrlFilter
    {
        /// <summary>
        /// The field on which we're filtering data
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// The operation to perform on the value of the field selected.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// The value contained within the field.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// UrlFilter
        /// </summary>
        public UrlFilter()
        {
            this.Operator = "eq";
        }
    }
}
