// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// ICalendar representation of an event
    /// </summary>
    [Serializable]
    public class EventsICal
    {
        private readonly string _iCal;
        /// <summary>
        /// Gets the i cal.
        /// </summary>
        /// <value>
        /// The i cal.
        /// </value>
        public string iCal { get { return _iCal; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsICal"/> class.
        /// </summary>
        /// <param name="iCal">The i cal.</param>
        public EventsICal(string iCal)
        {
            _iCal = iCal;
        }
    }
}
