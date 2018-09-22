// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Single Severity Style Mapping object.
    /// </summary>
    public class SeverityStyleMapping
    {
        /// <summary>
        /// The starting point for this severity range.
        /// </summary>
        public int SeverityStart { get; set; }

        /// <summary>
        /// The ending point for this severity range.
        /// </summary>
        public int SeverityEnd { get; set; }

        /// <summary>
        /// The style for this severity range.
        /// </summary>
        public AlertStyle Style { get; set; }
    }
}
