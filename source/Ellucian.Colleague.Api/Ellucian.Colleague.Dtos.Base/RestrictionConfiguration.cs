// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A configuration object that contains a list of Institution defined Severity Style Mapping objects.
    /// </summary>
    public class RestrictionConfiguration
    {
        /// <summary>
        /// List of Severity Style Mapping objects
        /// </summary>
        public List<SeverityStyleMapping> Mapping { get; set; }
    }
}
