// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Define the Component Tests within a Test Result
    /// </summary>
    public class ComponentTest
    {
        /// <summary>
        /// Component Test Code
        /// </summary>
        public string Test { get; set; }
        /// <summary>
        /// Component Score
        /// </summary>
        public int? Score { get; set; }
        /// <summary>
        /// Component Percentile
        /// </summary>
        public int? Percentile { get; set; }
    }
}
