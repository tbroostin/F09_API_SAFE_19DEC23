// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// This object defines display options for filters shown in the course catalog. 
    /// </summary>
    public class CatalogFilterOption
    {
        /// <summary>
        /// Indicates the type of catalog filter 
        /// </summary>
        public CatalogFilterType Type { get; set; }
        /// <summary>
        /// Indicates whether this item should be hidden or not.
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
