// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an email address
    /// </summary>
    [Serializable]
    public enum OrganizationEmailType
    {
        /// <summary>
        /// An organization's email address for sales inquiries.
        /// </summary>
        Sales,
        /// <summary>
        /// A organization's email address for support inquiries.
        /// </summary>
        Support,
        /// <summary>
        /// A organization's email address for general inquiries.
        /// </summary>
        General,
        /// <summary>
        /// A organization's email address for billing inquiries.
        /// </summary>
        Billing,
        /// <summary>
        /// A organization's email address for legal inquiries.
        /// </summary>
        Legal,
        /// <summary>
        /// A organization's email address for HR inquiries.
        /// </summary>
        HR,
        /// <summary>
        /// A organization's email address for media inquiries.
        /// </summary>
        Media,
        /// <summary>
        /// Uncategorized email type
        /// </summary>
        Other
    }
}
