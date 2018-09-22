// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Period type enumeration
    /// </summary>
    [Serializable]
    public enum PeriodType
    {
        /// <summary>
        /// The past
        /// </summary>
        Past,
        /// <summary>
        /// The current
        /// </summary>
        Current,
        /// <summary>
        /// The future
        /// </summary>
        Future
    }
}
