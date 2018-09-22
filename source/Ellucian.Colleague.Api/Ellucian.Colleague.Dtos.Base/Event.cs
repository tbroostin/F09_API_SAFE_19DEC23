// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Calendar item
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Nature of the event, institutionally defined 
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Location, institutionally defined (typically campus)
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Building(s) in which event occurs
        /// </summary>
        public List<string> Buildings { get; set; }
        /// <summary>
        /// Room(s) in which event occurs (corresponding to buildings)
        /// </summary>
        public List<string> Rooms { get; set; }
        /// <summary>
        /// Date/Time event starts.
        /// </summary>
        public DateTimeOffset StartTime { get; set; }
        /// <summary>
        /// Date/Time event ends.
        /// </summary>
        public DateTimeOffset EndTime { get; set; }

    }
}
