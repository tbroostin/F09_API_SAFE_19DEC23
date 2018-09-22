// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Cycle restriction for a specific location
    /// </summary>
    public class LocationCycleRestriction
    {
        /// <summary>
        /// Location specific to this location cycle restriction (required)
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Session cycle this item is restricted to
        /// </summary>
        public string SessionCycle { get; set; }
        /// <summary>
        /// Yearly cycle that this item is restricted to
        /// </summary>
        public string YearlyCycle { get; set; }
    }
}
