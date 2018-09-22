// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A single severity style mapping - institution configurable styling for severity range
    /// </summary>
    [Serializable]
    public class SeverityStyleMapping
    {
        /// <summary>
        /// SeverityStart is an integer from 0 to 999
        /// </summary>
        public int SeverityStart { get { return _severityStart; } }

        /// <summary>
        /// SeverityEnd is an integer from 0 to 999 and must be greater than SeverityStart
        /// </summary>
        public int SeverityEnd { get { return _severityEnd; } }

        /// <summary>
        /// The style that is used for the specified range
        /// </summary>
        public AlertStyle Style { get { return _style; } }

        private int _severityStart;
        private int _severityEnd;
        private AlertStyle _style;

        public SeverityStyleMapping(int? severityStart, int? severityEnd, AlertStyle style)
        {
            if(!severityStart.HasValue)
            {
                throw new ArgumentNullException("severityStart");
            }
            if (!severityEnd.HasValue)
            {
                throw new ArgumentNullException("severityEnd");
            }
            if (style == null)
            {
                throw new ArgumentNullException("style");
            }
            if(severityStart < 0 || severityStart > 999)
            {
                throw new ArgumentOutOfRangeException("severityStart", "SeverityStart must be an integer from 0 to 999");
            }
            if (severityEnd< 0 || severityEnd > 999)
            {
                throw new ArgumentOutOfRangeException("severityEnd", "SeverityEnd must be an integer from 0 to 999");
            }
            if(severityEnd < severityStart)
            {
                throw new ArgumentException("severityEnd", "SeverityEnd must be greater than SeverityStart");
            }

            _severityStart = severityStart.Value;
            _severityEnd = severityEnd.Value;
            _style = style;
        }
    }
}
