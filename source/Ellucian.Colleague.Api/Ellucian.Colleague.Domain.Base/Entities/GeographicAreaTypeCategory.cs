// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an Geographic Area Type
    /// </summary>
    [Serializable]
    public enum GeographicAreaTypeCategory
    {
        /// <summary>
        /// Governmental
        /// </summary>
        Governmental,
        /// <summary>
        /// Postal
        /// </summary>
        Postal,
        /// <summary>
        /// Fundraising
        /// </summary>
        Fundraising,
        /// <summary>
        /// Recruitment
        /// </summary>
        Recruitment,
    }
}