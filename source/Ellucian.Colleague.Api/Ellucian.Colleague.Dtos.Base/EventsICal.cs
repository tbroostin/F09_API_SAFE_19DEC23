// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// I-Calendar event item
    /// </summary>
    public class EventsICal
    {
        /// <summary>
        /// iCal string representation
        /// </summary>
        public string iCal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsICal"/> class.
        /// </summary>
        /// <param name="rawICal">The raw iCal item.</param>
        public EventsICal(string rawICal)
        {
            iCal = rawICal;
        }
    }
}
